using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Library;
using UNI.Core.Library.AttributesMetadata;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.GenericModels.Mapping;

namespace UNI.API.DAL.v2;

public class DbContextV2<T> where T : BaseModel
{
    private readonly string connectionString;
    private readonly ILogger logger;
    private readonly ListHelperV2<T> listHelper;

    public DbContextV2(string connectionString)
    {
        this.connectionString = connectionString;

        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                                                    .SetMinimumLevel(LogLevel.Trace)
                                                    .AddConsole());
        logger = loggerFactory.CreateLogger<DbContextV2<T>>();
        listHelper = new ListHelperV2<T>(connectionString);
    }

    #region Queries

    public async Task<List<T>> GetData(string query)
    { 
        return await listHelper.GetData(query);
    }

    public DataTable ExecuteReadQuery(string query)
    {
        DataTable dataset = new();

        using MySqlConnection conn = new(connectionString);
        using MySqlCommand cmd = new(query, conn);

        try
        {
            conn.Open();
            DbDataReader reader;
            reader = cmd.ExecuteReader();

            dataset.Load(reader);
            conn.Close();
        }
        catch (MySqlException)
        {
            conn.Close();
        }

        return dataset;
    }

    /// <summary>
    /// Can perform writing queries with safe sql parameters
    /// </summary>
    /// <param name="query"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public async Task SetDataParametrized(string query, MySqlParameter[] queryParameters)
    {
        using MySqlConnection conn = new(connectionString);
        using MySqlCommand cmd = new(query, conn);

        cmd.Parameters.AddRange(queryParameters);

        conn.Open();

        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (MySqlException e)
        {
            logger.Log(LogLevel.Error, "{controllerName}: Error message: '{message}'. Inner exception: '{innerException}'", nameof(DbContextV2<T>), e.Message, e.InnerException?.Message);
        }

        conn.Close();
    }

    /// <summary>
    /// Can perform all writing queries; queryParameters is for byte blobs only though
    /// </summary>
    /// <param name="query"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public async Task<int> SetData(string query, Dictionary<string, byte[]>? queryParameters = null)
    {
        int lastId;

        using MySqlConnection conn = new(connectionString);
        using MySqlCommand cmd = new(query, conn);

        if (queryParameters != null)
            foreach (string key in queryParameters.Keys)
                if (queryParameters.TryGetValue(key, out byte[]? value))
                    if (value != null)
                        cmd.Parameters.Add(new MySqlParameter($"@{key}", MySqlDbType.LongBlob, value.Length) { Value = value });

        conn.Open();

        try
        {
            await cmd.ExecuteNonQueryAsync();
            lastId = Convert.ToInt32(cmd.LastInsertedId);
        }
        catch (MySqlException e)
        {
            logger.Log(LogLevel.Error, "{controllerName}: Error message: '{message}'. Inner exception: '{innerException}'", nameof(DbContextV2<T>), e.Message, e.InnerException?.Message);
            lastId = 0;
        }

        conn.Close();

        return lastId;
    }
    #endregion

    #region PUBLIC Generic CRUD

    /// <summary>
    /// Generic method to perform UPDATE query on any table
    /// </summary>
    /// <param name="obj" one of our models to be updated></param>
    /// <returns></returns>
    public async Task<bool> UpdateObject(T obj)
    {
        try
        {
            Type type = typeof(T);
            List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(type);
            List<PropertyInfo> enumeratedProperties = new();
            List<PropertyInfo> externalProperties = new();
            extendedTypes.Insert(0, type);
            extendedTypes.Reverse();

            List<string> queryArgs = new();
            Dictionary<string, byte[]> queryParameters = new();

            foreach (Type t in extendedTypes)
            {
                bool propertiesToWrite = false;
                var classInfo = (ClassInfo?)t.GetCustomAttribute(typeof(ClassInfo));
                if (classInfo == null)
                    continue;

                string tablename = classInfo.SQLName;

                string query = "UPDATE " + tablename + " SET ";

                var properties = type.GetProperties().ToList();

                //get the id for every update query
                int objectId = obj.ID;
                PropertyInfo? idProperty = properties.Find(p => p.Name == $"Id{t.Name}");
                if (idProperty != null)
                    objectId = Convert.ToInt32(idProperty.GetValue(obj));

                foreach (PropertyInfo pro in properties)
                {
                    if (enumeratedProperties.Any(p => p.Name == pro.Name))
                        continue;

                    var valueInfo = (ValueInfo?)pro.GetCustomAttribute(typeof(ValueInfo));
                    if (valueInfo == null)
                        continue;

                    if (valueInfo.IsReadOnly
                        || pro.DeclaringType != t
                        || pro.Name == "IdRef" || pro.Name == "TableRef"
                        || pro.PropertyType.IsSubclassOf(typeof(BaseModel)))
                        continue;

                    enumeratedProperties.Add(pro);

                    // if the property points to another table, set it aside
                    if (!string.IsNullOrEmpty(valueInfo.WriteTable))
                    {
                        if (!externalProperties.Contains(pro))
                            externalProperties.Add(pro);
                        continue;
                    }

                    propertiesToWrite = true;

                    if (pro.PropertyType.IsGenericType)
                    {
                        if (!pro.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                            continue;

                        if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                            continue;

                        var idsToMatch = new List<int>
                        {
                            obj.ID
                        };
                        var list = await SelectObjects(type: pro.PropertyType.GenericTypeArguments[0], tableAttritbute: valueInfo.ManyToManySQLName, idMatchAttribute: "Mtm", idsToMatch: idsToMatch);
                        var actualList =  list.Cast<BaseModel>().ToList();
        
                        var property = (IList?)pro.GetValue(obj);
                        if (property == null || actualList == null)
                            continue;

                        var modifiedList = property.Cast<BaseModel>().ToList();

                        //delete all no more needed links
                        foreach (var item in actualList)
                            if (!modifiedList.Any(i => i.ID == item?.ID))
                                DeleteLinkFromObjects(valueInfo.LinkTableSQLName, item, obj, valueInfo.ColumnReference1Name, valueInfo.ColumnReference2Name);

                        //create all needed links
                        foreach (var item in modifiedList)
                        {
                            if (!actualList.Any(i => i.ID == item?.ID))
                            {
                                if (item.ID == 0)
                                {
                                    MethodInfo methodInsert = GetType().GetMethod(nameof(InsertObject));
                                    MethodInfo reflectedInsertObject = methodInsert.MakeGenericMethod(item.GetType());

                                    int id = 0;
                                    if (reflectedInsertObject != null)
                                        id = (int)reflectedInsertObject.Invoke(this, new object[] { item })!;

                                    if (id != 0)
                                        item.ID = id;
                                }

                                if (item.ID != 0)
                                    InsertLinkObjects(valueInfo.LinkTableSQLName, item.GetType(), item.ID, obj.GetType(), obj.ID, valueInfo.ColumnReference1Name, valueInfo.ColumnReference2Name);
                            }
                        }
                        continue;
                    }

                    var value = pro.GetValue(obj);
                    string queryValue = ParsePropertyValue(pro.GetValue(obj), pro, queryParameters);
                    query += $"{valueInfo.SQLName} = {queryValue}";
                }

                // remove last comma and space and finish query with WHERE clause
                query = query.Remove(query.Length - 2, 2);
                query += $" WHERE id = {objectId};";

                if (propertiesToWrite)
                    await SetData(query, queryParameters);

                // now update the other tables if needed
                foreach (PropertyInfo property in externalProperties)
                {
                    var valueInfoExternalProperty = (ValueInfo?)property.GetCustomAttribute(typeof(ValueInfo));
                    if (valueInfoExternalProperty == null)
                        continue;

                    if (property.PropertyType.IsGenericType)
                        continue;

                    string additionalTableQuery = "UPDATE " + valueInfoExternalProperty.WriteTable + " SET ";

                    Dictionary<string, byte[]> additionalQueryParameters = new();
                    string additionalValue = ParsePropertyValue(property.GetValue(obj), property, additionalQueryParameters);
                    additionalTableQuery += $"{valueInfoExternalProperty.SQLName} = {additionalValue}";

                    // remove last comma and space and finish query with WHERE clause
                    additionalTableQuery = additionalTableQuery.Remove(additionalTableQuery.Length - 2, 2);
                    additionalTableQuery += $" WHERE id = {objectId};";

                    await SetData(additionalTableQuery, additionalQueryParameters);
                }
            }
            return true;
        }
        catch 
        {
            return false;
        }
    }

    /// <summary>
    /// Generic method to perform INSERT query on any table
    /// </summary>
    // /// <param name="obj" one of our models to be inserted></param>
    public async Task<int> InsertRapidObject()
    {
        try
        {
            Type type = typeof(T);
            double maxNumber = 0;
            string tableName = string.Empty;
            string columnName = string.Empty;
            List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(type);
            extendedTypes.Insert(0, type);
            extendedTypes.Reverse();

            Dictionary<string, int> foreignKeys = new();

            foreach (Type t in extendedTypes)
            {
                var classInfo = (ClassInfo?)t.GetCustomAttribute(typeof(ClassInfo));
                if (classInfo == null)
                    continue;

                string tablename = classInfo.SQLName;

                foreach (var prop in type.GetProperties())
                {
                    List<Type> types = UtilityMethods.FindAllParentsTypes(prop.PropertyType);

                    var valueInfo = (ValueInfo?)t.GetCustomAttribute(typeof(ValueInfo));
                    if (valueInfo == null)
                        continue;

                    if (!valueInfo.IsMainNumeration)
                        continue;

                    if (maxNumber != 0)
                        continue;

                    string getMaxquery = string.Format("SELECT MAX({0}) FROM {1};", valueInfo.SQLName, tablename);
                    DataTable? table = ExecuteReadQuery(getMaxquery);
                    tableName = tablename;
                    columnName = valueInfo.SQLName;
                    if (table == null || table.Rows.Count <= 0)
                        continue;

                    if (table.Rows[0][0].GetType() != typeof(DBNull))
                    {
                        maxNumber = Convert.ToDouble(table.Rows[0][0]);
                        continue;
                    }
                }
            }

            string insertQuery = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tableName, columnName, maxNumber + 1);
            return await SetData(insertQuery);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Generic method to perform INSERT query on any table
    /// </summary>
    // /// <param name="obj" one of our models to be inserted></param>
    public async Task<int> InsertObject(T obj)
    {
        int res = 0;

        obj.InitAttributes();

        if (obj.Metadata.ClassAttributes.BaseModelType == EnBaseModelTypes.ViewOnlyBaseModel)
            res = await InsertViewOnlyObject(obj);

        try
        {
            //set the classtype string 
            if (typeof(T).GetCustomAttribute(typeof(ClassInfo)) is ClassInfo info)
            {
                if (!string.IsNullOrWhiteSpace(info.ClassType))
                {
                    var classTypeProp = typeof(T).GetProperties().ToList().Find(i => i.Name == "ClassType");
                    classTypeProp?.SetValue(obj, info.ClassType);
                }
            }

            Type? lowerType = null;
            List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(typeof(T));
            int itemId = 0;
            extendedTypes.Insert(0, typeof(T));
            extendedTypes.Reverse();

            Dictionary<string, int> foreignKeys = new();

            Dictionary<string, byte[]> queryParameters = new();

            string previousTableName = string.Empty;

            foreach (Type t in extendedTypes)
            {
                var classInfo = (ClassInfo?)t.GetCustomAttribute(typeof(ClassInfo));
                if (classInfo == null)
                    continue;

                string tablename = classInfo.SQLName;

                if (tablename != previousTableName)
                {
                    previousTableName = tablename;
                    string query = "INSERT INTO " + tablename + " (";

                    //List of propertyinfo that contains only the ones that can be written on the db in the iterated type. This list is used in the second foreach
                    var writeablePropertyInfos = new List<PropertyInfo>();
                    //List of propertyinfo that requires a many to many in a link table
                    var manyToManyPropertyInfos = new List<PropertyInfo>();

                    foreach (var prop in typeof(T).GetProperties())
                    {
                        List<Type> types = UtilityMethods.FindAllParentsTypes(prop.PropertyType);

                        var valueInfo = (ValueInfo?)prop.GetCustomAttribute(typeof(ValueInfo));
                        if (valueInfo == null)
                            continue;

                        if (valueInfo.IsReadOnly
                            || prop.PropertyType.IsInterface
                            || prop.DeclaringType != t)
                            continue;

                        if (prop.PropertyType.IsGenericType)
                        {
                            if (!prop.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                                continue;
                            else if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                                manyToManyPropertyInfos.Add(prop);
                        }
                        else if (prop.PropertyType.IsSubclassOf(typeof(BaseModel)))
                            continue;

                        //if it is a class derived from basemodel it will not be added in the query
                        if (!prop.PropertyType.IsClass)
                        {
                            query += valueInfo.SQLName + ", ";
                            writeablePropertyInfos.Add(prop);
                        }
                        else
                        {
                            if (!types.Contains(typeof(BaseModel))
                                && prop.PropertyType != typeof(BaseModel)
                                && !prop.PropertyType.IsGenericType)
                            {
                                query += valueInfo.SQLName + ", ";
                                writeablePropertyInfos.Add(prop);
                            }
                        }
                    }

                    query = query.Remove(query.Length - 2, 2);

                    query += ") VALUES (";

                    foreach (PropertyInfo pro in writeablePropertyInfos)
                    {
                        var value = pro.GetValue(obj);

                        if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueinfo && valueinfo.IsMainNumeration)
                        {
                            string queryMainNumeratino = string.Format("SELECT MAX({0}) FROM {1};", valueinfo.SQLName, tablename);
                            DataTable table = ExecuteReadQuery(queryMainNumeratino);

                            if (table.Rows.Count > 0)
                                if (table.Rows[0][0].GetType() != typeof(DBNull))
                                    if (int.TryParse(table.Rows[0][0].ToString(), out int n))
                                        value = Convert.ToDouble(table.Rows[0][0]) + 1;
                        }
                        if (pro.Name.Equals("IdRef"))
                        {
                            if (lowerType != null && foreignKeys.TryGetValue(lowerType.Name, out int id))
                                query += $"{id}, ";
                        }
                        else if (pro.Name.Equals("TableRef"))
                        {
                            if (lowerType != null && foreignKeys.TryGetValue(lowerType.Name, out int id) && lowerType?.GetCustomAttribute(typeof(ClassInfo)) is ClassInfo lowerClassInfo)
                                query += $"'{lowerClassInfo.SQLName}', ";
                        }
                        else if (pro.Name.StartsWith("Id"))
                        {
                            if (foreignKeys.TryGetValue(pro.Name[2..], out int id))
                                query += $"{id}, ";
                            else
                                query += $"{value}, ";
                        }
                        else
                        {
                            query += ParsePropertyValue(pro.GetValue(obj), pro, queryParameters);
                        }
                    }
                    query = query.Remove(query.Length - 2, 2);

                    query += ");";

                    var lastId = await SetData(query, queryParameters);
                    itemId = lastId;
                    if (lastId > 0)
                    {
                        foreignKeys.Add(t.Name, lastId);

                        //sets all many to many links because we need the id of the created object
                        foreach (var prop in manyToManyPropertyInfos)
                        {
                            var valueInfo = (ValueInfo?)t.GetCustomAttribute(typeof(ValueInfo));
                            if (valueInfo == null)
                                continue;

                            var dependencyAttribute = prop.PropertyType.GenericTypeArguments[0].GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

                            var propertyList = (IList?)prop.GetValue(obj);
                            if (propertyList != null)
                            {
                                var modifiedList = propertyList.Cast<BaseModel>().ToList();

                                //create all needed links
                                foreach (var item in modifiedList)
                                {
                                    if (item.ID == 0)
                                    {
                                        MethodInfo method = GetType().GetMethod(nameof(FilterListParameter));
                                        MethodInfo reflectedInsertViewObject = method.MakeGenericMethod(item.GetType());

                                        int id = 0;
                                        if (reflectedInsertViewObject != null)
                                            id = (int)reflectedInsertViewObject.Invoke(this, new object[] { item })!;

                                        if (id != 0)
                                            item.ID = id;
                                    }

                                    if (item.ID != 0)
                                        InsertLinkObjects(valueInfo.LinkTableSQLName, item.GetType(), item.ID, obj.GetType(), lastId, valueInfo.ColumnReference1Name, valueInfo.ColumnReference2Name);
                                }
                            }
                        }
                    }
                    else
                    {
                        return 0;
                    }
                    lowerType = t;
                }
                else
                {
                    obj.ID = itemId;
                    await UpdateObject(obj);
                }
            }
            res = itemId;
        }
        catch
        {

        }

        return res;
    }


    /// <summary>
    /// Generic method to perform SELECT * or SELECT WHERE ID = ... on any table
    /// </summary>
    /// <typeparam name="T" the type of the object-s we want></typeparam>
    /// <param name="id" facultative parameter></param>
    /// <returns></returns>
    public async Task<List<T>> ReadObjects (int? idToMatch = null, string? tableAttritbute = null, string? idName = null)
    {
        List<T> values = new();
        try
        {
            if (typeof(T).IsInterface)
            {
                var library = Assembly.GetExecutingAssembly().GetReferencedAssemblies().First(a => a.Name == "Uni.Common.Library");
                Assembly assLib = Assembly.Load(library);

                var types = assLib.GetTypes().Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface);

                // reflection on generic method with run-time determined types
                foreach (var K in types)
                {
                    MethodInfo method = GetType().GetMethod(nameof(SelectObjects));
                    MethodInfo reflectedSelectObjects = method.MakeGenericMethod(K);

                    if (reflectedSelectObjects == null)
                        continue;

                    object?[] parameters = { null, null, null };
                    var valuesToAdd = (IEnumerable<T>?)reflectedSelectObjects.Invoke(this, parameters);
                    if (valuesToAdd == null)
                        continue;

                    values.AddRange(valuesToAdd);
                }

                return values;
            }

            if (typeof(T).GetCustomAttribute(typeof(ClassInfo)) is ClassInfo classInfo)
            {
                tableAttritbute ??= classInfo.SQLName;

                string query = "SELECT * FROM " + tableAttritbute;

                if (idToMatch.HasValue)
                    query += $" WHERE id{idName} = {idToMatch}";

                var results = await listHelper.GetData(query);

                await FillDependencies(results, false);
                return results;
            }
        }
        catch 
        {

        }

        return values;

    }

    /// <summary>
    /// Generic method to perform SELECT * or SELECT WHERE ID = ... on any table
    /// </summary>
    /// <typeparam name="T" the type of the object-s we want></typeparam>
    /// <param name="id" facultative parameter></param>
    /// <returns></returns>
    public async Task<ApiResponseModel<T>?> Get(GetDataSetRequestDTO requestDTO)
    {
        try
        {
            ApiResponseModel<T> response = new();

            if (typeof(T).IsInterface || typeof(T).GetCustomAttribute(typeof(ClassInfo)) is not ClassInfo classInfo)
                return response;

            requestDTO.TableAttribute ??= classInfo.SQLName;

            string query = "SELECT * FROM " + requestDTO.TableAttribute;

            bool firstWhereCondition = true;

            if (requestDTO.Id.HasValue)
            {
                query += $" WHERE id{requestDTO.IdName} = {requestDTO.Id}";
                firstWhereCondition = false;
                if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                    query += $" AND classtype = '{classInfo.ClassType}'";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                {
                    query += $" WHERE classtype = '{classInfo.ClassType}'";
                    firstWhereCondition = false;
                }
            }

            //prefilter the items with a query
            var properties = typeof(T).GetProperties();
            List<FilterExpression> notQueryFilterExpressions = new();

            if (requestDTO.FilterExpressions.Any())
            {
                if (!query.Contains("WHERE")) query += " WHERE ";

                //TODO Convertire filterexpression riferite a dipendenze
                requestDTO.FilterExpressions = await ConvertFilterExpressionDependencies(requestDTO.FilterExpressions);

                foreach (var filterExpression in requestDTO.FilterExpressions)
                {
                    if (filterExpression.FilterExpressions.Any())
                        query += " ( ";
                    query += FilterExpressionToSql(filterExpression, properties.ToList(), firstWhereCondition, requestDTO.FilterDateFormat);
                    if (filterExpression.FilterExpressions.Any())
                    {
                        if (query[^4..] == "AND ")
                        {
                            if (query.Length > 3)
                                query = query.Remove(query.Length - 4, 4);
                        }
                        else if (query[^3..] == "OR ")
                        {
                            if (query.Length > 2)
                                query = query.Remove(query.Length - 3, 3);
                        }
                        query += $" ) ";
                    }
                }
            }

            if (query[^4..] == "AND ")
            {
                if (query.Length > 3)
                    query = query.Remove(query.Length - 4, 4);
            }
            else if (query[^3..] == "OR ")
            {
                if (query.Length > 2)
                    query = query.Remove(query.Length - 3, 3);
            }

            if (query[^6..] == "WHERE ")
                query = query.Remove(query.Length - 6, 6);

            //execute the query
            var results = await listHelper.GetData(query);

            bool dependenciesAlreadyResolved = false;

            //resolve dependencies if a filter text is specified
            if (!string.IsNullOrWhiteSpace(requestDTO.FilterText))
            {
                var resultCopyWithDependencies = new List<T>(results);
                await FillDependencies(resultCopyWithDependencies, requestDTO.LargeTablesLogic);

                string[] searchRequests = new string[] { requestDTO.FilterText };
                if (requestDTO.FilterText.Contains(','))
                    searchRequests = requestDTO.FilterText.Split(',');

                List<T>? items = new();
                foreach (string text in searchRequests)
                    results = FilterListParameter<T>(text, resultCopyWithDependencies.Cast<BaseModel>().ToList(), results.Cast<BaseModel>().ToList());

                List<T> itemsFilteredWithDependencies = new();
                foreach (T item in results)
                    itemsFilteredWithDependencies.Add(resultCopyWithDependencies.Find(i => i.ID == item.ID));

                results = itemsFilteredWithDependencies;

                dependenciesAlreadyResolved = true;
            }

            int pagingMagicNumber = 50;

            //if a requested value is set count the databolocks
            response.Count = results.Count;
            if (requestDTO.RequestedEntriesNumber.HasValue)
                response.DataBlocks = (response.Count + (requestDTO.RequestedEntriesNumber ?? pagingMagicNumber) - 1) / requestDTO.RequestedEntriesNumber ?? pagingMagicNumber;

            if (requestDTO.RequestedEntriesNumber.HasValue && results.Count > requestDTO.RequestedEntriesNumber)
            {
                int startIndex = 0;
                if (requestDTO.BlockToReturn > 1)
                    startIndex = (requestDTO.BlockToReturn - 1) * requestDTO.RequestedEntriesNumber ?? pagingMagicNumber;

                if (startIndex + requestDTO.RequestedEntriesNumber > results.Count)
                    requestDTO.RequestedEntriesNumber = results.Count - requestDTO.RequestedEntriesNumber * (requestDTO.BlockToReturn - 1);

                results = results.GetRange(startIndex, requestDTO.RequestedEntriesNumber ?? pagingMagicNumber);
            }

            //Resolve sql complex fields
            ResolveSqlFields(results);

            //postfilter to consider also the resolved fields with specifi sql queries(TO SOSTITUTE WITH LINQ)
            List<T> postFilteredItems = new();
            if (notQueryFilterExpressions != null && notQueryFilterExpressions.Any() && requestDTO.FilterExpressions != null)
            {
                foreach (var filterExpression in requestDTO.FilterExpressions)
                {
                    var property = properties.First(p => p.Name == filterExpression.PropertyName);
                    if (property == null)
                        continue;

                    foreach (var item in results.Where(i => i != null))
                    {
                        object? value = TryConvert(property.GetValue(item), property.PropertyType);
                        object? reference = TryConvert(filterExpression.PropertyValue, property.PropertyType);
                        if (CompareObjects(value, reference, property.PropertyType))
                            postFilteredItems.Add(item);
                    }
                }

                results = postFilteredItems;
            }

            response.ResponseBaseModels = results;

            if (dependenciesAlreadyResolved) 
                return response;

            if (!requestDTO.BackendDependencyResolve)
            {
                //TODO Gestire invio diretto dipendenze
                //if (results.Count > 1)
                //    response.BaseModelDependencies = GetDependenciesLists(results, true);
                //else
                await FillDependencies(response.ResponseBaseModels, requestDTO.LargeTablesLogic);
            }
            else
            {
                await FillDependencies(response.ResponseBaseModels, requestDTO.LargeTablesLogic);
                response.BaseModelDependencies = await GetDependenciesLists(results, requestDTO.LargeTablesLogic);
            }

            return response;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generic method to perform DELETE WHERE ID = ... on any table
    /// </summary>
    /// <param name="obj" the object to be deleted></param>
    public async Task DeleteObject(int id)
    {
        var tableAttritbute = typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

        await SetData("DELETE FROM " + tableAttritbute?.SQLName + $" WHERE id = {id}");
    }

    #endregion

    #region Private methods

    public async Task<List<FilterExpression>> ConvertFilterExpressionDependencies(List<FilterExpression> filterExpressions)
    {
        try
        {
            var baseModelTypes = GetSubTypes(typeof(T));
            var typeProperties = typeof(T).GetProperties();
            List<PropertyInfo> properties = new();

            foreach (var filterExpression in filterExpressions)
            {
                List<FilterExpression> convertedFilterExpressions = new();
                if (filterExpression.PropertyName.Contains("."))
                {
                    var propertyPath = filterExpression.PropertyName.Split('.');

                    Type iterationType = typeof(T);
                    foreach (var subproperty in propertyPath)
                    {
                        PropertyInfo property = iterationType.GetProperties().FirstOrDefault(i => i.Name == subproperty);
                        if (property != null)
                        {
                            properties.Add(property);
                            if (property.PropertyType.IsGenericType)
                                iterationType = property.PropertyType.GenericTypeArguments[0];
                            else iterationType = property.PropertyType;
                        }
                    }

                    var reversePath = propertyPath.Reverse().ToArray();
                    string filterExpressionProperty = propertyPath[0];

                    List<int> nextExectionIdToFilter = new();
                    int count = 0;
                    foreach (string property in reversePath)
                    {
                        if (count > 0)
                        {
                            Type propertyType;
                            var propertyInfo = properties.FirstOrDefault(i => i.Name == property);
                            if (propertyInfo.PropertyType.IsGenericType)
                                propertyType = propertyInfo.PropertyType.GenericTypeArguments[0];
                            else propertyType = propertyInfo.PropertyType;
                            var valueInfoProperty = (ValueInfo?)propertyInfo.GetCustomAttribute(typeof(ValueInfo));
                            if (valueInfoProperty == null)
                                continue;

                            Type childPropertyType;
                            var childPropertyInfo = properties.FirstOrDefault(i => i.Name == reversePath[count - 1]);
                            if (childPropertyInfo.PropertyType.IsGenericType)
                                childPropertyType = childPropertyInfo.PropertyType.GenericTypeArguments[0];
                            else childPropertyType = childPropertyInfo.PropertyType;
                            var valueInfoChildProperty = (ValueInfo?)childPropertyInfo.GetCustomAttribute(typeof(ValueInfo));
                            if (valueInfoChildProperty == null)
                                continue;

                            if (count == 1)
                            {
                                var classInfo = (ClassInfo?)propertyType.GetCustomAttribute(typeof(ClassInfo));
                                if (classInfo == null)
                                    continue;

                                var value = await SelectObjects(type: propertyType, tableAttritbute: classInfo.SQLName, valueMatchAttribute: reversePath[0], valueToMatch: filterExpression.PropertyValue);
                                nextExectionIdToFilter = new();
                                foreach (var item in value)
                                    if (item is BaseModel bm)
                                        if (!nextExectionIdToFilter.Contains(bm.ID))
                                            nextExectionIdToFilter.Add(bm.ID);
                            }

                            else
                            {
                                //caso mtm
                                if (!string.IsNullOrWhiteSpace(valueInfoChildProperty.ManyToManySQLName))
                                {
                                    var value =await SelectObjects(type: propertyType, tableAttritbute: valueInfoChildProperty.ManyToManySQLName, idMatchAttribute: "", idsToMatch: nextExectionIdToFilter);

                                    nextExectionIdToFilter = new();
                                    foreach (var item in value)
                                        if (item is BaseModel bm)
                                            if (!nextExectionIdToFilter.Contains(bm.IdMtm))
                                                nextExectionIdToFilter.Add(bm.IdMtm);
                                }
                                else //normal basemodel or otm
                                {
                                    Type fatherPropertyType;

                                    if (count + 1 < reversePath.Count())
                                    {
                                        var fatherPropertyInfo = properties.FirstOrDefault(i => i.Name == reversePath[count + 1]);
                                        if (fatherPropertyInfo.PropertyType.IsGenericType)
                                            fatherPropertyType = fatherPropertyInfo.PropertyType.GenericTypeArguments[0];
                                        else fatherPropertyType = fatherPropertyInfo.PropertyType;
                                    }
                                    else
                                        fatherPropertyType = typeof(T);

                                    var classInfoProperty = (ClassInfo?)propertyType.GetCustomAttribute(typeof(ClassInfo));
                                    if (classInfoProperty == null)
                                        continue;

                                    string idMatchAttribute = childPropertyInfo.Name;
                                    if (childPropertyInfo.PropertyType.IsGenericType && String.IsNullOrWhiteSpace(valueInfoChildProperty.ManyToManySQLName))
                                        idMatchAttribute = string.Empty;

                                    var value =await SelectObjects(type: propertyType, tableAttritbute: classInfoProperty.SQLName, idMatchAttribute: idMatchAttribute, idsToMatch: nextExectionIdToFilter);

                                    nextExectionIdToFilter = new();

                                    if (propertyInfo.PropertyType.IsGenericType)
                                    {
                                        var dependencyProperty = propertyType.GetProperties().FirstOrDefault(i => i.Name == $"Id{fatherPropertyType.Name}");
                                        foreach (var item in value)
                                        {
                                            int id = (int?)dependencyProperty.GetValue(item) ?? 0;
                                            if (!nextExectionIdToFilter.Contains(id) && id != 0)
                                                nextExectionIdToFilter.Add(id);
                                        }
                                        filterExpressionProperty = string.Empty;
                                    }
                                    else
                                    {
                                        foreach (var item in value)
                                            if (item is BaseModel bm)
                                                if (!nextExectionIdToFilter.Contains(bm.ID))
                                                    nextExectionIdToFilter.Add(bm.ID);

                                        filterExpressionProperty = propertyPath[0];
                                    }
                                }
                            }
                        }
                        count++;
                    }
                    foreach (var id in nextExectionIdToFilter)
                    {
                        FilterExpression newFilterExpression = new()
                        {
                            PropertyValue = id.ToString(),
                            PropertyName = $"Id{filterExpressionProperty}"
                        };
                        filterExpression.FilterExpressions.Add(newFilterExpression);
                        filterExpression.ComparisonType = "OR";
                    }
                }
            }

            return filterExpressions;
        }
        catch
        {
            return filterExpressions;
        }

    }

    public async Task<IList> SelectObjects(Type type, string tableAttritbute, string? idMatchAttribute = null, List<int>? idsToMatch = null, string? valueMatchAttribute = null, string? valueToMatch = null)
    {
        IList res = new List<BaseModel>();

        var dictionaryIds = new Dictionary<string, List<int>>();
        if (idMatchAttribute != null && idsToMatch != null)
            dictionaryIds.Add(idMatchAttribute, idsToMatch);

        var dictionaryValues = new Dictionary<string, string>();
        if (valueMatchAttribute != null && valueToMatch != null)
            dictionaryValues.Add(valueMatchAttribute, valueToMatch);

        string? query = DALHelper.GetSelectObjectsNoFillQuery(type, tableAttritbute, dictionaryIds, dictionaryValues);

        if (query != null)
        {
            var genericListHelper = listHelper.GetGenericInstance(connectionString, type);
            var getDataMethod = genericListHelper.GetType().GetMethod(nameof(ListHelperV2<T>.GetData));
            return (IList)await DALHelper.InvokeAsyncList(genericListHelper, getDataMethod, new[] { query });
        }

        return res;
    }


    /// <summary>
    /// Called by main insert method if class attribute indicates a view-only based BaseModel
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<int> InsertViewOnlyObject(T obj)
    {
        int res = 0;

        obj.InitAttributes();

        IEnumerable<IGrouping<string, KeyValuePair<string, DataAttributes>>> groupsOfWriteTablesAndProperties =
            obj.Metadata.DataAttributes
                .Where(d => !string.IsNullOrWhiteSpace(d.Value.WriteTable))
                .GroupBy(d => d.Value.WriteTable);

        foreach (IGrouping<string, KeyValuePair<string, DataAttributes>> destinationTable in groupsOfWriteTablesAndProperties)
        {
            string query = "INSERT INTO " + destinationTable.Key + " (";

            foreach (var propertyName in destinationTable)
                query += $"{propertyName.Key.ToLower()}, ";

            // the fields that are in linked tables will have to refer the main table using a column named "id{nameOfType}" with the object id itself
            if (destinationTable.Key != obj.Metadata.ClassAttributes.MasterTable)
                query += $"id{typeof(T).Name}, ";

            query = query.Remove(query.Length - 2, 2);
            query += ") VALUES (";

            Dictionary<string, byte[]> queryParameters = new();
            foreach (var propertyName in destinationTable)
            {
                PropertyInfo property = typeof(T).GetProperties().First(p => p.Name == propertyName.Key);
                query += ParsePropertyValue(property.GetValue(obj), property, queryParameters);
            }

            // the fields that are in linked tables will have to refer the main table using a column named "id{nameOfType}" with the object id itself
            if (destinationTable.Key != obj.Metadata.ClassAttributes.MasterTable)
                query += $"{obj.ID}, ";

            query = query.Remove(query.Length - 2, 2);
            query += ");";

            res = await SetData(query, queryParameters);
        }

        return res;
    }

    private static string ParsePropertyValue(object? value, PropertyInfo pro, Dictionary<string, byte[]> queryParameters)
    {
        switch (value)
        {
            case string stringValue:
                if (stringValue == null)
                    stringValue = string.Empty;
                else
                    stringValue = MySqlHelper.EscapeString(stringValue);

                return $"'{stringValue}', ";

            case byte[] blobValue:
                queryParameters.Add(pro.Name, blobValue);
                return $"@{pro.Name}, ";

            case null:
                return $"'', ";


            case DateTime dateValue:
                if (!dateValue.Equals(new DateTime()))
                {
                    dateValue = dateValue.ToLocalTime();
                    var stringDateValue = dateValue.ToString("yyyy-MM-dd HH:mm:ss");
                    return $"'{stringDateValue}', ";
                }
                else
                    return $"NULL, ";

            default:
                return $"{Convert.ToString(value)?.Replace(',', '.')}, ";
        }
    }

    private async void InsertLinkObjects(string tablename, Type type1, int idObj1, Type type2, int idObj2, string columnReference1Name, string columnReference2Name)
    {
        if (string.IsNullOrWhiteSpace(columnReference1Name))
            columnReference1Name = $"id{type1.Name}";

        if (string.IsNullOrWhiteSpace(columnReference2Name))
            columnReference2Name = $"id{type2.Name}";

        string query = "INSERT INTO " + tablename + $" ({columnReference1Name}, {columnReference2Name}) VALUES ({idObj1} ,{idObj2})";
        await SetData(query);
    }

    private async void DeleteLinkFromObjects(string tablename, BaseModel obj1, BaseModel obj2, string columnReference1Name, string columnReference2Name)
    {

        Type type1 = obj1.GetType();
        Type type2 = obj2.GetType();

        if (string.IsNullOrWhiteSpace(columnReference1Name))
            columnReference1Name = $"id{type1.Name}";

        if (string.IsNullOrWhiteSpace(columnReference2Name))
            columnReference2Name = $"id{type2.Name}";

        string query = "DELETE FROM " + tablename + $" WHERE {columnReference1Name} = {obj1.ID} AND {columnReference2Name} = {obj2.ID}";
        await SetData(query);
    }

    private async Task<Dictionary<string, IList>> GetDependenciesLists(List<T> results, bool largeTables)
    {
        if (largeTables)
            return await GetObjectsListsLargeTables(results);
        else 
            return await GetObjectsLists(results);
    }

    private async Task FillDependencies(List<T> objs, bool largeTables)
    {
        ////get the type of the objects
        Type type = typeof(T);

        Dictionary<string, IList> listOfObjects = await GetDependenciesLists(objs, largeTables);

        //assign items
        foreach (var obj in objs)
            if (obj is BaseModel bm)
                AssignDependencies(bm, listOfObjects, new List<Type>() { type });
    }

    //TODO rinominare GetAllSubTypes perchè non li restituisce tutti
    private Dictionary<string, Type> GetAllSubTypes(Type type, Dictionary<string, Type>? baseModelTypes = null, List<string>? enumeratedTypes = null)
    {
        baseModelTypes ??= new Dictionary<string, Type>();
        enumeratedTypes ??= new List<string>();

        foreach (PropertyInfo pro in type.GetProperties())
        {
            if (pro.GetCustomAttribute(typeof(ValueInfo)) is not ValueInfo valueInfo)
                continue;

            Type propertyType = pro.PropertyType;
            string keyname = propertyType.Name;

            if (propertyType.IsGenericType && propertyType.FullName != null && !propertyType.FullName.Contains("UniDataSet"))
            {
                propertyType = propertyType.GenericTypeArguments[0];
                keyname = propertyType.Name;

                if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                    keyname = $"mtm_{valueInfo.ManyToManySQLName}_{keyname}";
            }

            if (!propertyType.IsSubclassOf(typeof(BaseModel)))
                continue;

            if (enumeratedTypes.Contains(keyname))
                continue;

            enumeratedTypes.Add(keyname);

            if (!baseModelTypes.ContainsKey(keyname))
            {
                baseModelTypes.Add(keyname, propertyType);
            }

            GetAllSubTypes(propertyType, baseModelTypes, enumeratedTypes);
        }
        return baseModelTypes;
    }

    private Dictionary<string, Type> GetSubTypes(Type type, Dictionary<string, Type>? baseModelTypes = null, List<Type>? enumeratedTypes = null)
    {
        baseModelTypes ??= new Dictionary<string, Type>();
        enumeratedTypes ??= new List<Type>();

        foreach (PropertyInfo pro in type.GetProperties())
        {
            if (pro.GetCustomAttribute(typeof(ValueInfo)) is not ValueInfo valueInfo)
                continue;

            Type propertyType = pro.PropertyType;
            string keyname = propertyType.Name;

            if (propertyType.IsGenericType && propertyType.FullName != null)
            {
                propertyType = propertyType.GenericTypeArguments[0];
                keyname = propertyType.Name;

                if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                    keyname = $"mtm_{valueInfo.ManyToManySQLName}_{keyname}";
            }

            if (!propertyType.IsSubclassOf(typeof(BaseModel)))
                continue;

            if (enumeratedTypes.Contains(propertyType))
                continue;

            if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                enumeratedTypes.Add(propertyType);

            if (!baseModelTypes.ContainsKey(keyname))
            {
                baseModelTypes.Add(keyname, propertyType);
            }

            GetSubTypes(propertyType, baseModelTypes, enumeratedTypes);
        }
        return baseModelTypes;
    }


    private void AssignDependencies(BaseModel obj, Dictionary<string, IList> allObjectsLists, List<Type> fatherPath, List<Type>? enumeratedTypes = null)
    {
        if (obj == null)
            return;

        enumeratedTypes ??= new List<Type>();
        enumeratedTypes.Add(obj.GetType());

        foreach (var pro in obj.GetType().GetProperties())
        {
            if (pro.PropertyType.FullName != null && pro.PropertyType.FullName.Contains("UniDataSet"))
                continue;

            Type propertyType = pro.PropertyType;
            if (pro.PropertyType.IsGenericType)
                propertyType = pro.PropertyType.GenericTypeArguments[0];

            if (!propertyType.IsSubclassOf(typeof(BaseModel)))
                continue;

            if (pro.PropertyType == typeof(UniImage))
                continue;

            if (fatherPath.Contains(propertyType))
                continue;

            List<Type> mypath = DALHelper.GetMyPath(fatherPath, propertyType);

            Type listType = typeof(List<>);
            Type constructedListType = listType.MakeGenericType(propertyType);
            object? instanceList = Activator.CreateInstance(constructedListType);
            string keyname = constructedListType.GenericTypeArguments[0].Name;

            if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo && pro.PropertyType.IsGenericType)
            {
                //deve entrare qui solamente nel caso di una lista
                PropertyInfo? idChildProperty = null;

                if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                    keyname = $"mtm_{valueInfo.ManyToManySQLName}_{propertyType.Name}";

                if (!allObjectsLists.TryGetValue(keyname, out IList? typeList))
                    continue;

                List<BaseModel> typeListBaseModel = typeList.Cast<BaseModel>().ToList();

                if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                {
                    List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(obj.GetType());
                    extendedTypes.Add(obj.GetType());
                    foreach (var type in extendedTypes)
                    {
                        idChildProperty = propertyType.GetProperties().ToList().Find(i => i.Name == $"Id{type.Name}");
                        if (idChildProperty != null)
                            break;
                    }
                }
                else
                    idChildProperty = propertyType.GetProperties().ToList().Find(i => i.Name == "IdMtm");

                if (idChildProperty == null)
                    continue;

                foreach (var item in typeListBaseModel)
                {
                    PropertyInfo? idProperty = obj.GetType().GetProperties().ToList().Find(i => i.Name == "ID");
                    int? id = (int?)idProperty?.GetValue(obj);
                    int? idChild = (int?)idChildProperty.GetValue(item);
                    if (idChild == id && id != null)
                        if (instanceList != null)
                            ((IList)instanceList).Add(item);
                }

                pro.SetValue(obj, instanceList);

                if (instanceList != null)
                    foreach (var item in (IList)instanceList)
                        if (item is BaseModel bm)
                            AssignDependencies(bm, allObjectsLists, mypath, enumeratedTypes);
            }
            else
            {
                if (!allObjectsLists.TryGetValue(keyname, out IList? typeList))
                    continue;

                List<BaseModel> typeListBaseModel = typeList.Cast<BaseModel>().ToList();

                PropertyInfo? idItemProperty = obj.GetType().GetProperties().ToList().Find(i => i.Name == $"Id{pro.Name}");
                if (idItemProperty != null)
                {
                    int? idItem = (int?)idItemProperty.GetValue(obj);
                    var value = typeListBaseModel.Find(i => i.ID == idItem);
                    pro.SetValue(obj, value);

                    if (value != null)
                        AssignDependencies(value, allObjectsLists, mypath, enumeratedTypes);
                }
            }
        }
    }

    private async Task<Dictionary<string, IList>> GetObjectsListsLargeTables(IList objs, Dictionary<string, IList>? listOfObjects = null, Dictionary<string, List<int>>? baseModelsIndexes = null, Dictionary<string, Type>? baseModelsTypes = null, List<Type>? enumeratedTypes = null)
    {
        Dictionary<string, List<int>> iterationBaseModelsIndexes = new();
        Dictionary<string, List<int>> toReadBaseModelsIndexes = new();

        baseModelsIndexes ??= new Dictionary<string, List<int>>();
        baseModelsTypes ??= new Dictionary<string, Type>();

        listOfObjects ??= new Dictionary<string, IList>();
        Dictionary<string, IList> iterationListOfObjects = new();

        enumeratedTypes ??= new List<Type>();
        Type type = objs.GetType().GenericTypeArguments[0];

        foreach (PropertyInfo pro in type.GetProperties())
        {
            if (pro.GetCustomAttribute(typeof(ValueInfo)) is not ValueInfo valueInfo)
                continue;

            Type propertyType = pro.PropertyType;
            string keyname = propertyType.Name;
            string propertyName = $"Id{pro.Name}";

            if (propertyType.IsGenericType && propertyType.FullName != null && !propertyType.FullName.Contains("UniDataSet"))
            {
                propertyType = propertyType.GenericTypeArguments[0];
                keyname = $"otm_{propertyType.Name}_{typeof(T).Name}";
                propertyName = "ID";

                if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                    keyname = $"mtm_{valueInfo.ManyToManySQLName}_{propertyType.Name}";
            }

            if (!propertyType.IsSubclassOf(typeof(BaseModel)))
                continue;

            if (enumeratedTypes.Contains(propertyType) && !keyname.StartsWith("mtm"))
                continue;

            enumeratedTypes.Add(propertyType);

            if (!iterationBaseModelsIndexes.TryGetValue(keyname, out List<int> indexList))
            {
                indexList = new List<int>();
                iterationBaseModelsIndexes.Add(keyname, indexList);
            }

            foreach (var obj in objs)
            {
                var idProperty = type.GetProperties().FirstOrDefault(i => i.Name == propertyName);
                if (idProperty != null)
                {
                    int id = (int)idProperty.GetValue(obj);
                    if (!indexList.Contains(id) && id != 0)
                        indexList.Add(id);
                    if (!baseModelsTypes.ContainsKey(keyname))
                        baseModelsTypes.Add(keyname, propertyType);
                }
            }
        }

        //unificazione liste di indici iterazione corrente e precedente iterazione con calcolo indici da leggere
        foreach (KeyValuePair<string, List<int>> listOfIndex in iterationBaseModelsIndexes)
        {
            if (baseModelsIndexes.TryGetValue(listOfIndex.Key, out List<int> previoustIndexList))
            {
                List<int> toReadIds = new();
                foreach (int currentId in listOfIndex.Value)
                {
                    if (!previoustIndexList.Contains(currentId))
                    {
                        previoustIndexList.Add(currentId);
                        toReadIds.Add(currentId);
                    }
                }
                if (toReadIds.Count > 0)
                    toReadBaseModelsIndexes.Add(listOfIndex.Key, toReadIds);
            }
            else
            {
                baseModelsIndexes.Add(listOfIndex.Key, listOfIndex.Value);
                toReadBaseModelsIndexes.Add(listOfIndex.Key, listOfIndex.Value);
            }
        }

        foreach (var baseModelType in baseModelsTypes)
        {
            string selectWhereParameter = string.Empty;

            if (!baseModelType.Key.StartsWith("mtm") && !baseModelType.Value.IsSubclassOf(typeof(BaseModel)))
                continue;

            string? keyPart = null;
            if (baseModelType.Key.StartsWith("mtm")) //mtm case
            {
                keyPart = baseModelType.Key.Split('_')[1];
                selectWhereParameter = "Mtm";
            }
            if (baseModelType.Key.StartsWith("otm")) //mtm case
            {
                selectWhereParameter = baseModelType.Key.Split('_')[2];
            }
          
            if (toReadBaseModelsIndexes.TryGetValue(baseModelType.Key, out List<int> indexList))
            {
                if (indexList != null)
                {
                    Dictionary<string, List<int>> selectDict = new()
                    {
                        { selectWhereParameter, indexList }
                    };

                    IList value = new List<BaseModel>();
                    string? query = DALHelper.GetSelectObjectsNoFillQuery(baseModelType.Value, keyPart, selectDict, null);
                    if (query != null)
                    {
                        var genericListHelper = listHelper.GetGenericInstance(connectionString, type);
                        var getDataMethod = genericListHelper.GetType().GetMethod(nameof(ListHelperV2<T>.GetData));
                        value = (IList)await DALHelper.InvokeAsyncList(genericListHelper, getDataMethod, new[] { query });
                    }

                    if (!iterationListOfObjects.ContainsKey(baseModelType.Key) && value != null)
                        iterationListOfObjects.Add(baseModelType.Key, value);
                    if (!listOfObjects.ContainsKey(baseModelType.Key) && value != null)
                        listOfObjects.Add(baseModelType.Key, value);
                }
            }
        }

        foreach (KeyValuePair<string, IList> list in iterationListOfObjects)
           await GetObjectsListsLargeTables(list.Value, listOfObjects, baseModelsIndexes, baseModelsTypes, enumeratedTypes);

        return listOfObjects;
    }

    private async Task<Dictionary<string, IList>> GetObjectsLists(IList objs, Dictionary<string, IList>? listOfObjects = null, Dictionary<string, List<int>>? baseModelsIndexes = null, Dictionary<string, Type>? baseModelsTypes = null, List<Type>? enumeratedTypes = null)
    {
        //get the type of the objects
        Type type = typeof(T);

        //get the properties of the objects
        var objectProperties = type.GetProperties().ToList();

        //init the dictionary that contains all the list of objects
        listOfObjects = new();
        Dictionary<Type, List<int>> listOfIndexes = new();

        //enumerate all needed types
        Dictionary<string, Type> baseModelTypes = GetAllSubTypes(typeof(T));

        //prepare list
        foreach (var baseModelType in baseModelTypes)
        {
            if (!baseModelType.Key.StartsWith("mtm") && !baseModelType.Value.IsSubclassOf(typeof(BaseModel)))
                continue;

            string? keyPart = null;
            if (baseModelType.Key.StartsWith("mtm")) //mtm case
                keyPart = baseModelType.Key.Split('_')[1];

            string? query = DALHelper.GetSelectObjectsNoFillQuery(baseModelType.Value, keyPart, null, null);
            if (query == null)
                continue;

            var genericListHelper = listHelper.GetGenericInstance(connectionString, baseModelType.Value);
            var getDataMethod = genericListHelper.GetType().GetMethod(nameof(ListHelperV2<T>.GetData));

            var value = (IList)(await DALHelper.InvokeAsyncList(genericListHelper, getDataMethod, new[] { query }));

            if (!listOfObjects.ContainsKey(baseModelType.Key) && value != null)
                listOfObjects.Add(baseModelType.Key, value);
        }

        return listOfObjects;
    }

#pragma warning disable CS0693 // Il parametro di tipo ha lo stesso nome del parametro del tipo outer
    private List<T> FilterListParameter<T>(string searchText, List<BaseModel> itemsWithDependencies, List<BaseModel> itemsToFilter)
#pragma warning restore CS0693 // Il parametro di tipo ha lo stesso nome del parametro del tipo outer
    {
        List<BaseModel> filteredItemsSource = new();

        if (string.IsNullOrWhiteSpace(searchText))
            return filteredItemsSource.Cast<T>().ToList();

        foreach (var item in itemsToFilter)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                if (property.Name == "Created" || property.Name == "LastModify")
                    continue;

                if (item == null)
                    continue;

                if (property.PropertyType == typeof(DateTime))
                {
                    DateTime value = Convert.ToDateTime((DateTime?)property.GetValue(item));
                    DateTimeOffset dateTimeOffset = new(value.ToLocalTime());
                    if (dateTimeOffset.ToString().ToLower().Trim().Contains(searchText))
                        filteredItemsSource.Add(item);
                }
                else if (!property.PropertyType.IsSubclassOf(typeof(BaseModel)) && !property.PropertyType.IsGenericType)
                {
                    var val = property.GetValue(item);
                    if (val == null)
                        continue;

                    string? value = Convert.ToString(property.GetValue(item))?.ToLower().Trim();
                    if (value != null && value.Contains(searchText.ToLower().Trim()) && !filteredItemsSource.Contains(item))
                        filteredItemsSource.Add(item);
                }
                else if (property.PropertyType.IsGenericType && property.PropertyType.FullName != null && !property.PropertyType.FullName.Contains("UniDataSet"))
                {
                    BaseModel? itemWithDependencies = itemsWithDependencies.Find(i => i.ID == item.ID);
                    if (itemWithDependencies == null)
                        continue;

                    List<BaseModel>? dependencyToFilter = (property.GetValue(itemWithDependencies) as IList)?.Cast<BaseModel>().ToList();
                    if (dependencyToFilter == null || dependencyToFilter.Count == 0)
                        continue;
                    object[] parameters = { searchText, dependencyToFilter, dependencyToFilter };
                   
                    MethodInfo method = GetType().GetMethod(nameof(FilterListParameter), BindingFlags.NonPublic | BindingFlags.Instance);
                    MethodInfo generic = method.MakeGenericMethod(property.PropertyType.GenericTypeArguments[0]);

                    IList? items = generic?.Invoke(this, parameters) as IList;
                    if (items?.Count > 0 && !filteredItemsSource.Contains(item))
                        filteredItemsSource.Add(item);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(BaseModel)))
                {
                    BaseModel? itemWithDependencies = itemsWithDependencies.Find(i => i.ID == item.ID);
                    if (!SearchValueInObject(searchText, property.GetValue(itemWithDependencies)))
                        continue;

                    if (!filteredItemsSource.Contains(item))
                    {
                        filteredItemsSource.Add(item);
                        break;
                    }
                }
            }
        }

        return filteredItemsSource.Cast<T>().ToList();
    }

    private void ResolveSqlFields(List<T> items)
    {
        var properties = typeof(T).GetProperties();

        foreach (T item in items)
        {
            foreach (var property in properties)
            {
                List<object> parameters = new();

                var sqlFieldInfo = (SqlFieldInfo?)property.GetCustomAttribute(typeof(SqlFieldInfo));
                if (sqlFieldInfo == null)
                    continue;

                string[] fields = sqlFieldInfo.PropertyWhereValues.Split(',');
                if (fields != null && fields.Length > 0)
                {
                    foreach (string field in fields)
                    {
                        var prop = properties.First(p => p.Name == field);
                        if (prop == null)
                            continue;

                        if (prop.PropertyType == typeof(DateTime))
                        {
                            DateTime dateTime = Convert.ToDateTime(prop.GetValue(item));
                            parameters.Add(dateTime.ToString("yyyyMMdd"));
                        }
                        else
                        {
                            var parameter = prop.GetValue(item);
                            if (parameter != null)
                                parameters.Add(parameter);
                        }
                    }
                }

                string query = string.Format(sqlFieldInfo.Query, parameters.ToArray());
                DataTable table = ExecuteReadQuery(query);
                if (table.Rows.Count == 0)
                    continue;

                object value = table.Rows[0][0];
                try
                {
                    if (value.GetType() != typeof(DBNull))
                        property.SetValue(item, TryConvert(value, property.PropertyType));
                }
                catch
                {

                }
            }
        }
    }

    private static bool SearchValueInObject(string searchText, object? item)
    {
        if (item == null)
            return false;

        foreach (var property in item.GetType().GetProperties())
        {
            if (property.Name == "Created" || property.Name == "LastModify" || property.PropertyType == typeof(DateTime))
                continue;

            string? val = Convert.ToString(property.GetValue(item));
            if (val != null && val.ToLower().Trim().Contains(searchText.ToLower().Trim()))
                return true;
        }

        return false;
    }

    private static object? TryConvert(object? source, Type dest)
    {
        if (source == null)
            return null;

        try
        {
            if (dest == typeof(double))
                return Convert.ToDouble(source);

            else if (dest == typeof(bool))
                return Convert.ToBoolean(source);

            else if (dest == typeof(int))
                return Convert.ToInt32(source);

            else if (dest == typeof(byte))
                return Convert.ToByte(source);

            else if (dest == typeof(decimal))
                return Convert.ToDecimal(source);

            else if (dest == typeof(string))
                return Convert.ToString(source);

            else if (dest == typeof(DateTime))
                return Convert.ToDateTime(source);
        }
        catch
        {

        }
        return null;
    }

    private static bool CompareObjects(object? obj1, object? obj2, Type dest)
    {
        object? val1 = null;
        object? val2 = null;

        try
        {
            if (dest == typeof(double))
            {
                val1 = Convert.ToDouble(obj1);
                val2 = Convert.ToDouble(obj2);
            }
            else if (dest == typeof(bool))
            {
                val1 = Convert.ToBoolean(obj1);
                val2 = Convert.ToBoolean(obj2);
            }
            else if (dest == typeof(byte))
            {
                val1 = Convert.ToByte(obj1);
                val2 = Convert.ToByte(obj2);
            }
            else if (dest == typeof(int))
            {
                val1 = Convert.ToInt32(obj1);
                val2 = Convert.ToInt32(obj2);
            }
            else if (dest == typeof(decimal))
            {
                val1 = Convert.ToDecimal(obj1);
                val2 = Convert.ToDecimal(obj2);
            }
            else if (dest == typeof(string))
            {
                val1 = Convert.ToString(obj1)?.Trim();
                val2 = Convert.ToString(obj2)?.Trim();
            }
            else if (dest == typeof(DateTime))
            {
                val1 = Convert.ToDateTime(obj1);
                val2 = Convert.ToDateTime(obj2);
            }
        }
        catch
        {
            return false;
        }

        return val1 == val2;
    }

    private string FilterExpressionToSql(FilterExpression filterExpression, List<PropertyInfo> properties, bool firstWhereCondition, string filterDateFormat)
    {
        var property = properties.Find(p => p.Name == filterExpression.PropertyName);
        string query = string.Empty;

        if (filterExpression.FilterExpressions.Any())
            query += " ( ";

        
        if (!string.IsNullOrWhiteSpace(filterExpression.PropertyName) && filterExpression.PropertyValue != null && property != null)
        {
            if (property.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
            {
                if (!string.IsNullOrWhiteSpace(valueInfo.SQLName))
                {
                    if (!firstWhereCondition)
                    {
                        query += $" {filterExpression.ComparisonType} ";
                    }
                    firstWhereCondition = false;

                    if (property.PropertyType == typeof(string))
                        query += valueInfo.SQLName + $" {filterExpression.OperatorSign} '{MySqlHelper.EscapeString(filterExpression.PropertyValue.ToString())}' ";
                    else if (property.PropertyType == typeof(DateTime))
                    {

                        query += $" DATE_FORMAT({valueInfo.SQLName},'{filterDateFormat}') {filterExpression.OperatorSign} '{filterExpression.PropertyValue}' ";
                    }
                    else
                    {
                        if (filterExpression.ComparisonType == "LIKE")
                            query += " " + valueInfo?.SQLName + $" {filterExpression.OperatorSign} %{filterExpression.PropertyValue}% ";
                        else query += " " + valueInfo?.SQLName + $" {filterExpression.OperatorSign} {filterExpression.PropertyValue} ";
                    }
                }
            }
        }

        int i = 0;
        foreach (var subFilterExpression in filterExpression.FilterExpressions)
        {
            i++;
            query += FilterExpressionToSql(subFilterExpression, properties, firstWhereCondition, filterDateFormat);
            if (i < filterExpression.FilterExpressions.Count)
                query += $" {filterExpression.ComparisonType} ";
        }

        if (filterExpression.FilterExpressions.Any())
        {
            if (query[^4..] == "AND ")
            {
                if (query.Length > 3)
                    query = query.Remove(query.Length - 4, 4);
            }
            else if (query[^3..] == "OR ")
            {
                if (query.Length > 2)
                    query = query.Remove(query.Length - 3, 3);
            }
            query += $" )";
        }
        return query;
    }


    #endregion
}
