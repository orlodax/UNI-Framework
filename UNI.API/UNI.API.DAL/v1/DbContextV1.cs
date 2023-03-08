using MySqlConnector;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;
using UNI.Core.Library.GenericModels.Mapping;

namespace UNI.API.DAL.v1
{
    public class DbContextV1
    {
        #region Properties
        public string ConnectionString { get; set; }
        public string ApiKey { get; set; }
        #endregion

        #region CTOR

        public DbContextV1(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public DbContextV1(string connectionString, string apikey)
        {
            ConnectionString = connectionString;
            ApiKey = apikey;
        }
        #endregion

        #region Querys
        public DataTable ExecuteReadQuery(string query)
        {
            DataTable DbImportato = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    DbDataReader reader;
                    reader = cmd.ExecuteReader();

                    DbImportato.Load(reader);
                    conn.Close();
                }
                catch (MySqlException e)
                {
                    OnMySqlError(new MySqlErrorEventArgs(e));
                    conn.Close();
                }
            }
            //if the SQL connection succeeds, this method returns values from remote database storing them into a DataTable type variable called "DbImportato"
            return DbImportato;
        }

        async Task<int> SetData(string query, Dictionary<string, byte[]> queryParameters = null)
        {

            int lastId = 0;
            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (queryParameters != null)
                    {
                        foreach (string key in queryParameters.Keys)
                        {
                            if (queryParameters.TryGetValue(key, out byte[] value))
                            {
                                if (value != null)
                                {
                                    MySqlParameter paramImage = new MySqlParameter($"@{key}", MySqlDbType.LongBlob, value.Length);
                                    paramImage.Value = value;
                                    cmd.Parameters.Add(paramImage);
                                }

                            }

                        }

                    }
                    try
                    {
                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                        lastId = Convert.ToInt32(cmd.LastInsertedId);
                        conn.Close();
                    }
                    catch (MySqlException e)
                    {
                        OnMySqlError(new MySqlErrorEventArgs(e));
                        conn.Close();
                        lastId = 0;
                    }
                    return lastId;
                }

            }
        }
        #endregion

        #region Generic CRUD

        /// <summary>
        /// Generic method to perform UPDATE query on any table
        /// </summary>
        /// <param name="obj" one of our models to be updated></param>
        /// <returns></returns>
        public async Task<bool> UpdateObject(BaseModel obj)
        {
            try
            {
                Type type = obj.GetType();
                List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(type);
                List<PropertyInfo> enumeratedProperties = new List<PropertyInfo>();
                extendedTypes.Insert(0, type);
                extendedTypes.Reverse();

                Dictionary<string, byte[]> queryParameters = new Dictionary<string, byte[]>();

                foreach (Type t in extendedTypes)
                {
                    if (t.GetCustomAttribute(typeof(ClassInfo)) is ClassInfo classInfo && classInfo != null)
                    {
                        string tablename = classInfo.SQLName;

                        string query = "UPDATE " + tablename + " SET ";

                        var properties = t.GetProperties().ToList();


                        //get the id for every update query
                        int objectId = obj.ID;
                        PropertyInfo idProperty = properties.Find(p => p.Name == $"Id{t.Name}");
                        if (idProperty != null)
                        {
                            objectId = (int)idProperty.GetValue(obj);
                        }

                        foreach (PropertyInfo pro in properties)
                        {
                            if (!enumeratedProperties.Any(p => p.Name == pro.Name)) 
                            {
                                if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                                {
                                    enumeratedProperties.Add(pro);
                                    var proType = pro.PropertyType;
                                    if (proType.IsGenericType)
                                    {
                                        if (!UtilityMethods.FindAllParentsTypes(pro.PropertyType.GenericTypeArguments[0]).Contains(typeof(BaseModel)))
                                            continue;
                                        else if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                                        {
                                            // do list select of actual elements in list
                                            MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.SelectObjects));
                                            MethodInfo generic = method.MakeGenericMethod(pro.PropertyType.GenericTypeArguments[0]);
                                            object[] parameters = { obj.ID, valueInfo.ManyToManySQLName, "Mtm" };

                                            var result = (IList)generic.Invoke(this, parameters);
                                            var actualList = result.Cast<BaseModel>().ToList();

                                            var property = (IList)pro.GetValue(obj);
                                            if (property != null)
                                            {
                                                var modifiedList = property.Cast<BaseModel>().ToList();

                                                //delete all no more needed links
                                                foreach (var item in actualList)
                                                    if (modifiedList.Find(i => i.ID == item?.ID) == null)
                                                        DeleteLinkFromObjects(valueInfo.LinkTableSQLName, item, obj, valueInfo.ColumnReference1Name, valueInfo.ColumnReference2Name);

                                                //create all needed links
                                                foreach (var item in modifiedList)
                                                    if (actualList.Find(i => i.ID == item?.ID) == null)
                                                    {
                                                        if (item.ID == 0)
                                                        {
                                                            MethodInfo methodInsert = typeof(DbContextV1).GetMethod(nameof(DbContextV1.InsertViewObject));
                                                            MethodInfo genericInser = method.MakeGenericMethod(item.GetType());
                                                            object[] parametersInsert = { item };
                                                            int id = (int)generic.Invoke(this, parametersInsert);

                                                            if (id != 0)
                                                                item.ID = id;
                                                        }

                                                        if (item.ID != 0)
                                                            InsertLinkObjects(valueInfo.LinkTableSQLName, item.GetType(), item.ID, obj.GetType(), obj.ID, valueInfo.ColumnReference1Name, valueInfo.ColumnReference2Name);
                                                    }
                                            }


                                            continue;
                                        }
                                        else continue;


                                    }
                                    else if (valueInfo.IsReadOnly)
                                        continue;
                                    else if (pro.DeclaringType != t)
                                        continue;
                                    else if (pro.PropertyType.IsInterface)
                                        continue;
                                    else if (pro.Name == "IdRef" || pro.Name == "TableRef")
                                        continue;
                                    else
                                        if (UtilityMethods.FindAllParentsTypes(pro.PropertyType).Contains(typeof(BaseModel)))
                                        continue;

                                    var value = pro.GetValue(obj);

                                    if (pro.PropertyType.Equals(typeof(string)))
                                    {
                                        if (value == null)
                                            value = string.Empty;
                                        else
                                            value = MySqlHelper.EscapeString(value as string);

                                        query += valueInfo.SQLName + $" = '{value}', ";
                                    }
                                    else if (pro.PropertyType.Equals(typeof(byte[])))
                                    {
                                        queryParameters.Add(pro.Name, value as byte[]);
                                        query += valueInfo.SQLName + $" = @{pro.Name}, ";
                                    }
                                    else if (pro.PropertyType.Equals(typeof(DateTime)))
                                    {
                                        if (value != null)
                                        {
                                            if (!value.Equals(new DateTime()))

                                            {

                                                DateTime date = (DateTime)value;
                                                date = date.ToLocalTime();
                                                value = date.ToString("yyyy-MM-dd HH:mm:ss");
                                                query += valueInfo.SQLName + $" = '{value}', ";
                                            }
                                            else
                                                query += valueInfo.SQLName + $" = NULL, ";

                                        }
                                        else
                                            query += valueInfo.SQLName + $" = NULL, ";
                                    }
                                    else
                                        query += valueInfo.SQLName + $" = {value.ToString().Replace(',', '.')}, ";
                                }

                            }

                        }

                        query = query.Remove(query.Length - 2, 2);

                        query += $" WHERE id = {objectId};";

                        await SetData(query, queryParameters);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Generic method to perform INSERT query on any table
        /// </summary>
        // /// <param name="obj" one of our models to be inserted></param>
        public async Task<int> InsertRapidObject(BaseModel obj)
        {
            try
            {
                Type type = obj.GetType();
                double maxNumber = 0;
                string tableName = string.Empty;
                string columnName = string.Empty;
                Type lowerType = null;
                List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(type);
                extendedTypes.Insert(0, type);
                extendedTypes.Reverse();

                Dictionary<string, int> foreignKeys = new Dictionary<string, int>();

                foreach (Type t in extendedTypes)
                {
                    if (t.GetCustomAttribute(typeof(ClassInfo)) is ClassInfo classInfo)
                    {
                        string tablename = classInfo.SQLName;
                        var lowerClassInfo = lowerType?.GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

                        foreach (var prop in type.GetProperties())
                        {
                            List<Type> types = UtilityMethods.FindAllParentsTypes(prop.PropertyType);

                            if (prop.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                            {
                                if (valueInfo.IsMainNumeration)
                                {
                                    if (maxNumber == 0)
                                    {
                                        string query = string.Format("SELECT MAX({0}) FROM {1};", valueInfo.SQLName, tablename);
                                        DataTable table = ExecuteReadQuery(query) ?? new DataTable();
                                        tableName = tablename;
                                        columnName = valueInfo.SQLName;
                                        if (table.Rows.Count > 0)
                                        {
                                            if (table.Rows[0][0].GetType() != typeof(DBNull))
                                            {
                                                maxNumber = Convert.ToDouble(table.Rows[0][0]);
                                                continue;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                string query1 = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tableName, columnName, maxNumber + 1);
                return await SetData(query1);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// Generic method to perform INSERT query on any table
        /// </summary>
        // /// <param name="obj" one of our models to be inserted></param>
        public async Task<int> InsertViewObject(BaseModel obj)
        {
            try
            {
                Type type = obj.GetType();


                //set the classtype string 
                if (type.GetCustomAttribute(typeof(ClassInfo)) is ClassInfo info)
                {
                    if (!string.IsNullOrWhiteSpace(info.ClassType))
                    {
                        var classTypeProp = type.GetProperties().ToList().Find(i => i.Name == "ClassType");
                        if (classTypeProp != null)
                        {
                            classTypeProp.SetValue(obj, info.ClassType);
                        }
                    }
                }

                Type lowerType = null;
                List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(type);
                int itemId = 0;
                extendedTypes.Insert(0, type);
                extendedTypes.Reverse();

                Dictionary<string, int> foreignKeys = new Dictionary<string, int>();

                Dictionary<string, byte[]> queryParameters = new Dictionary<string, byte[]>();


                string previousTableName = string.Empty;

                foreach (Type t in extendedTypes)
                {
                    if (t.GetCustomAttribute(typeof(ClassInfo)) is ClassInfo classInfo)
                    {
                        string tablename = classInfo.SQLName;
                        var lowerClassInfo = lowerType?.GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

                        if (tablename != previousTableName)
                        {
                            previousTableName = tablename;
                            string query = "INSERT INTO " + tablename + " (";

                            //List of propertyinfo that contains only the ones that can be written on the db in the iterated type. This list is used in the second foreach
                            var writeablePropertyInfos = new List<PropertyInfo>();
                            //List of propertyinfo that requires a many to many in a link table
                            var manyToManyPropertyInfos = new List<PropertyInfo>();

                            foreach (var prop in type.GetProperties())
                            {
                                List<Type> types = UtilityMethods.FindAllParentsTypes(prop.PropertyType);

                                if (prop.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                                {
                                    var proType = prop.PropertyType;

                                    if (prop.PropertyType.IsInterface)
                                        continue;

                                    if (prop.DeclaringType != t)
                                        continue;

                                    if (valueInfo.IsReadOnly)
                                        continue;

                                    if (proType.IsGenericType)
                                    {
                                        if (!UtilityMethods.FindAllParentsTypes(prop.PropertyType.GenericTypeArguments[0]).Contains(typeof(BaseModel)))
                                            continue;
                                        else if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                                        {
                                            manyToManyPropertyInfos.Add(prop);
                                        }
                                    }
                                    else if (UtilityMethods.FindAllParentsTypes(prop.PropertyType).Contains(typeof(BaseModel)))
                                    {
                                        continue;
                                    }

                                    //if it is a class derived from basemodel it will not be added in the query
                                    if (!prop.PropertyType.IsClass)
                                    {
                                        query += valueInfo.SQLName + ", ";
                                        writeablePropertyInfos.Add(prop);
                                    }
                                    else
                                    {
                                        if (!types.Contains(typeof(BaseModel)) && prop.PropertyType != typeof(BaseModel) && !prop.PropertyType.IsGenericType)
                                        {
                                            query += valueInfo.SQLName + ", ";
                                            writeablePropertyInfos.Add(prop);
                                        }
                                    }
                                }
                            }

                            query = query.Remove(query.Length - 2, 2);

                            query += ") VALUES (";

                            foreach (PropertyInfo pro in writeablePropertyInfos)
                            {
                                ValueInfo valueinfo = pro.GetCustomAttribute(typeof(ValueInfo)) as ValueInfo;

                                var value = pro.GetValue(obj);
                                if (valueinfo.IsMainNumeration)
                                {
                                    string queryMainNumeratino = string.Format("SELECT MAX({0}) FROM {1};", valueinfo.SQLName, tablename);
                                    DataTable table = ExecuteReadQuery(queryMainNumeratino) ?? new DataTable();

                                    if (table.Rows.Count > 0)
                                    {
                                        if (table.Rows[0][0].GetType() != typeof(DBNull))
                                        {
                                            bool isNumeric = int.TryParse(table.Rows[0][0].ToString(), out int n);
                                            if (isNumeric)
                                            {
                                                double val = Convert.ToDouble(table.Rows[0][0]);
                                                value = val + 1;
                                            }

                                        }

                                    }
                                }
                                if (pro.Name.Equals("IdRef"))
                                {
                                    if (foreignKeys.TryGetValue(lowerType?.Name, out int id))
                                        query += $"{id}, ";
                                }
                                else if (pro.Name.Equals("TableRef"))
                                {
                                    if (foreignKeys.TryGetValue(lowerType?.Name, out int id))
                                        query += $"'{lowerClassInfo.SQLName}', ";
                                }
                                else if (pro.Name.StartsWith("Id"))
                                {
                                    if (foreignKeys.TryGetValue(pro.Name.Substring(2), out int id))
                                        query += $"{id}, ";
                                    else
                                    {
                                        query += $"{value}, ";
                                    }
                                }
                                else if (pro.PropertyType.Equals(typeof(byte[])))
                                {
                                    queryParameters.Add(pro.Name, value as byte[]);
                                    query += $"@{pro.Name}, ";
                                }
                                else if (pro.PropertyType.Equals(typeof(string)))
                                {
                                    if (value == null)
                                        value = string.Empty;
                                    else
                                        value = MySqlHelper.EscapeString(value as string);

                                    query += $"'{value}', ";
                                }
                                else if (pro.PropertyType.Equals(typeof(DateTime)))
                                {
                                    if (value != null)
                                    {
                                        if (!value.Equals(new DateTime()))
                                        {
                                            DateTime date = (DateTime)value;
                                            date = date.ToLocalTime();
                                            value = date.ToString("yyyy-MM-dd HH:mm:ss");
                                            query += $"'{value}', ";
                                        }
                                        else query += $"NULL, ";

                                    }
                                    else query += $"NULL, ";
                                }
                                else
                                {
                                    if (value != null)
                                        query += $"{value.ToString().Replace(',', '.')}, ";
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
                                    if (prop.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                                    {
                                        var dependencyAttribute = prop.PropertyType.GenericTypeArguments[0].GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

                                        var propertyList = (IList)prop.GetValue(obj);
                                        if (propertyList != null)
                                        {
                                            var modifiedList = propertyList.Cast<BaseModel>().ToList();

                                            //create all needed links
                                            foreach (var item in modifiedList)
                                            {
                                                if (item.ID == 0)
                                                {
                                                    MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.InsertViewObject));
                                                    object[] parameters = { item };
                                                    var task = (Task)method.Invoke(this, parameters);
                                                    await task;
                                                    var resultProperty = typeof(Task<>).MakeGenericType(typeof(int)).GetProperty("Result");
                                                    int id = (int)resultProperty.GetValue(task);

                                                    if (id != 0)
                                                        item.ID = id;
                                                }

                                                if (item.ID != 0)
                                                    InsertLinkObjects(valueInfo.LinkTableSQLName, item.GetType(), item.ID, obj.GetType(), lastId, valueInfo.ColumnReference1Name, valueInfo.ColumnReference2Name);
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                itemId = 0;
                                return itemId;
                            }
                            lowerType = t;
                        }
                        else
                        {
                            obj.ID = itemId;
                            await UpdateObject(obj);
                        }
                    }


                }
                return itemId;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// Generic method to perform SELECT * or SELECT WHERE ID = ... on any table
        /// </summary>
        /// <typeparam name="T" the type of the object-s we want></typeparam>
        /// <param name="id" facultative parameter></param>
        /// <returns></returns>
        public List<T> SelectObjects<T>(int? idToMatch = null, string tableAttritbute = null, string idName = null)
        {
            try
            {
                if (typeof(T).IsInterface)
                {
                    List<T> values = new List<T>();

                    var library = Assembly.GetExecutingAssembly().GetReferencedAssemblies().First(a => a.Name == "Uni.Common.Library");
                    Assembly assLib = Assembly.Load(library);

                    var types = assLib.GetTypes().Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface);

                    // reflection on generic method with run-time determined types
                    MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.SelectObjects));
                    foreach (var K in types)
                    {
                        MethodInfo generic = method.MakeGenericMethod(K);
                        object[] parameters = { null, null, null };
                        values.AddRange((IEnumerable<T>)generic.Invoke(this, parameters));
                    }

                    return values;
                }

                var classInfo = typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

                if (classInfo != null)
                {
                    if (tableAttritbute == null)
                        tableAttritbute = classInfo.SQLName;

                    string query = "SELECT * FROM " + tableAttritbute;

                    if (idToMatch.HasValue)
                    {
                        query += $" WHERE id{idName} = {idToMatch}";
                        //if (!String.IsNullOrWhiteSpace(classInfo.ClassType))
                        //{
                        //    query += $" AND classtype = '{classInfo.ClassType}'";
                        //}
                    }
                    //else
                    //{
                    //    if (!String.IsNullOrWhiteSpace(classInfo.ClassType))
                    //    {
                    //        query += $" WHERE classtype = '{classInfo.ClassType}'";
                    //    }
                    //}

                    var results = new ListHelperV1<T>(ConnectionString).GetData(query);

                    FillDependenciesExperimental(results);
                    return results;

                }
                return null;
                //foreach (var obj in results)
                //FillDependencies(obj);
            }
            catch (Exception e)
            {
                return null;
            }


        }


        /// <summary>
        /// Generic method to perform SELECT * or SELECT WHERE ID = ... on any table
        /// </summary>
        /// <typeparam name="T" the type of the object-s we want></typeparam>
        /// <param name="id" facultative parameter></param>
        /// <returns></returns>
        public ApiResponseModel<T> Get<T>(int? idToMatch = null, string? tableAttritbute = null, string? idName = null, int? requestedEntriesNumber = null, int? blockToReturn = null, string? filterText = null, List<FilterExpression>? filterExpressions = null, bool backendDependencyResolve = false, string filterDateFormat = "%Y-%m-%d")
        {
            try
            {
                ApiResponseModel<T> response = Activator.CreateInstance(typeof(ApiResponseModel<T>)) as ApiResponseModel<T>;

                if (typeof(T).IsInterface)
                {

                }
                else
                {
                    var classInfo = typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                    if (classInfo != null)
                    {
                        if (tableAttritbute == null)
                            tableAttritbute = classInfo.SQLName;

                        string query = "SELECT * FROM " + tableAttritbute;

                        bool firstWhereCondition = true;

                        if (idToMatch.HasValue)
                        {
                            query += $" WHERE id{idName} = {idToMatch}";
                            firstWhereCondition = false;
                            if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                            {
                                query += $" AND classtype = '{classInfo.ClassType}'";
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(classInfo.ClassType))
                            {
                                query += $" WHERE classtype = '{classInfo.ClassType}'";
                                firstWhereCondition = false;
                            }
                        }

                        //add where in

                        //prefilter the items with a query
                        List<PropertyInfo> properties = typeof(T).GetProperties().ToList() ?? new List<PropertyInfo>();
                        List<FilterExpression> notQueryFilterExpressions = new List<FilterExpression>();
                        if (filterExpressions != null)
                        {
                            if (filterExpressions.Count > 0)
                            {
                                if (!query.Contains("WHERE")) query += " WHERE ";
                                foreach (var filterExpression in filterExpressions)
                                {
                                    var property = properties.Find(p => p.Name == filterExpression.PropertyName);

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
                                                    query += valueInfo.SQLName + $" LIKE '%{MySqlHelper.EscapeString(filterExpression.PropertyValue.ToString())}%'";
                                                else if (property.PropertyType == typeof(DateTime))
                                                {

                                                    query += $" DATE_FORMAT({valueInfo.SQLName},'{filterDateFormat}') = '{filterExpression.PropertyValue}' ";
                                                }
                                                else
                                                    query += valueInfo.SQLName + $" = {filterExpression.PropertyValue.ToString()}";
                                            }
                                            else
                                            {
                                                notQueryFilterExpressions.Add(filterExpression);
                                            }
                                        }

                                    }
                                }
                            }

                        }

                        //execute the query
                        var results = new ListHelperV1<T>(ConnectionString).GetData(query);

                        //resolve dependencies if a filter text is specified
                        if (!string.IsNullOrWhiteSpace(filterText))
                        {
                            var resultCopyWithDependencies = new ListHelperV1<T>(ConnectionString).GetData(query);
                            FillDependenciesExperimental(resultCopyWithDependencies);
                            results = FilterListCommand<T>(filterText, resultCopyWithDependencies.Cast<BaseModel>().ToList(), results.Cast<BaseModel>().ToList());
                        }

                        //if a requested value is set count the databolocks
                        response.Count = results.Count;
                        if (requestedEntriesNumber.HasValue)
                            response.DataBlocks = (response.Count + (requestedEntriesNumber ?? 50) - 1) / requestedEntriesNumber ?? 50;

                        if (requestedEntriesNumber.HasValue && results.Count() > requestedEntriesNumber)
                        {
                            int startIndex = 0;
                            if (blockToReturn > 1)
                                startIndex = (blockToReturn - 1) * requestedEntriesNumber ?? 50;
                            //int endIndex = startIndex + requestedEntriesNumber??1;
                            if (startIndex + requestedEntriesNumber > results.Count)
                            {
                                requestedEntriesNumber = results.Count - requestedEntriesNumber * (blockToReturn - 1);
                            }
                            results = results.GetRange(startIndex, requestedEntriesNumber ?? 50);
                        }

                        //Resolve sql complex fields
                        ResolveSqlFields(results);

                        //postfilter to consider also the resolved fields with specifi sql queries(TO SOSTITUTE WITH LINQ)
                        List<T> postFilteredItems = new List<T>();
                        if (notQueryFilterExpressions != null)
                        {
                            if (notQueryFilterExpressions.Count > 0)
                            {
                                foreach (var filterExpression in filterExpressions)
                                {
                                    var property = properties.Find(p => p.Name == filterExpression.PropertyName);
                                    foreach (var item in results)
                                    {
                                        object value = TryConvert(property.GetValue(item), property.PropertyType);
                                        object reference = TryConvert(filterExpression.PropertyValue, property.PropertyType);
                                        if (CompareObjects(value, reference, property.PropertyType))
                                            postFilteredItems.Add(item);
                                    }
                                }

                                results = postFilteredItems;
                            }

                        }

                        response.ResponseBaseModels = results;

                        if (results.Count == 0)
                        {

                        }

                        if (!backendDependencyResolve)
                        {
                            if (results.Count > 2000)
                            {
                                response.BaseModelDependencies = GetDependeciesLists(results);
                            }
                            else
                            {
                                FillDependenciesExperimental(response.ResponseBaseModels);
                                response.BaseModelDependencies = null;
                            }
                        }
                        else
                        {
                            FillDependenciesExperimental(response.ResponseBaseModels);
                            response.BaseModelDependencies = GetDependeciesLists(results);

                        }
                    }
                }

                return response;
            }
            catch (Exception e)
            {
                return null;
            }


        }

        private void ResolveSqlFields<T>(List<T> items)
        {
            List<PropertyInfo> properties = typeof(T).GetProperties().ToList() ?? new List<PropertyInfo>();

            foreach (T item in items ?? new List<T>())
            {
                foreach (var property in properties ?? new List<PropertyInfo>())
                {
                    List<object> parameters = new List<object>();

                    if (property.GetCustomAttribute(typeof(SqlFieldInfo)) is SqlFieldInfo sqlFieldInfo)
                    {
                        string[] fields = sqlFieldInfo.PropertyWhereValues.Split(',');
                        if (fields != null)
                        {
                            if (fields.Count() > 0)
                            {
                                foreach (string field in fields)
                                {
                                    var prop = properties.Find(p => p.Name == field);
                                    if (prop != null)
                                    {
                                        if (prop.PropertyType == typeof(DateTime))
                                        {
                                            DateTime dateTime = Convert.ToDateTime(prop.GetValue(item) ?? new DateTime());
                                            parameters.Add(dateTime.ToString("yyyyMMdd"));
                                        }
                                        else
                                            parameters.Add(prop.GetValue(item));
                                    }

                                }
                            }
                        }

                        string query = string.Format(sqlFieldInfo.Query, parameters.ToArray());
                        DataTable table = ExecuteReadQuery(query) ?? new DataTable();
                        if (table.Rows.Count > 0)
                        {
                            object value = table.Rows[0][0];
                            try
                            {
                                if (value.GetType() != typeof(DBNull))
                                {
                                    property.SetValue(item, TryConvert(value, property.PropertyType));
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }

                    }

                }
            }
        }

        private Dictionary<string, IList> GetDependeciesLists<T>(List<T> results)
        {
            //get the type of the objects
            Type type = typeof(T);

            //get the properties of the objects
            var objectProperties = type.GetProperties().ToList();
            //reset the stati list that contains the enumerated types
            enumeratedTypes = new List<Type>();

            //prepare the method info to execute the method selectobjctsnofill (select without dependency resolution)
            MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.SelectObjectsNoFill));
            baseModelTypes = new Dictionary<string, Type>();
            //init the dictionary that contains all the list of objects
            Dictionary<string, IList> listOfObjects = new Dictionary<string, IList>();
            Dictionary<Type, List<int>> listOfIndexes = new Dictionary<Type, List<int>>();


            //enumerate all needed types
            GetAllSubTypes(typeof(T));



            //preapre list
            foreach (var baseModelType in baseModelTypes)
            {
                if (baseModelType.Key.StartsWith("mtm")) //mtm case
                {

                    string[] keyParts = baseModelType.Key.Split('_');
                    MethodInfo generic = method.MakeGenericMethod(baseModelType.Value);
                    object[] parameters = { null, keyParts[1], null }; // the method requires a string parameter instead of a classinfo
                    IList value = (IList)generic.Invoke(this, parameters); // it returns a list
                                                                           //List<BaseModel> valueBaseModel = value.Cast<BaseModel>().ToList();

                    if (!listOfObjects.ContainsKey(baseModelType.Key))
                        listOfObjects.Add(baseModelType.Key, value);
                }
                else
                {
                    if (!UtilityMethods.FindAllParentsTypes(baseModelType.Value).Contains(typeof(BaseModel)))
                    {
                        MethodInfo generic = method.MakeGenericMethod(baseModelType.Value);
                        var tableAttribute = baseModelType.Value.GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                        object[] parameters = { null, null, null }; // the method requires a string parameter instead of a classinfo
                        IList value = (IList)generic.Invoke(this, parameters); // it returns a list
                                                                               //List<BaseModel> valueBaseModel = value.Cast<BaseModel>().ToList();

                        if (!listOfObjects.ContainsKey(baseModelType.Key))
                            listOfObjects.Add(baseModelType.Key, value);
                    }

                }

            }

            foreach (string key in listOfObjects.Keys)
            {

            }

            return listOfObjects;
        }

        public List<T> SelectObjectsNoFill<T>(int? idToMatch = null, string tableAttritbute = null, string idName = null)
        {
            if (typeof(T).IsInterface)
            {
                List<T> values = new List<T>();

                var library = Assembly.GetExecutingAssembly().GetReferencedAssemblies().First(a => a.Name == "Uni.Common.Library");
                Assembly assLib = Assembly.Load(library);

                var types = assLib.GetTypes().Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface);

                // reflection on generic method with run-time determined types
                MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.SelectObjects));
                foreach (var K in types)
                {
                    MethodInfo generic = method.MakeGenericMethod(K);
                    object[] parameters = { null, null, null };
                    values.AddRange((IEnumerable<T>)generic.Invoke(this, parameters));
                }

                return values;
            }

            if (tableAttritbute == null)
                tableAttritbute = (typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo).SQLName;

            string query = "SELECT * FROM " + tableAttritbute;

            //if (idToMatch.HasValue)
            //    query += $" WHERE id{idName} = {idToMatch}";

            if (idToMatch.HasValue)
            {
                query += $" WHERE id{idName} = {idToMatch}";
                if (!string.IsNullOrWhiteSpace((typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo).ClassType))
                {
                    query += $" AND classtype = '{(typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo).ClassType}'";
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace((typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo).ClassType))
                {
                    query += $" WHERE classtype = '{(typeof(T).GetCustomAttribute(typeof(ClassInfo)) as ClassInfo).ClassType}'";
                }
            }

            var results = new ListHelperV1<T>(ConnectionString).GetData(query);
            //foreach (var obj in results)
            //    FillDependencies(obj);

            //Resolve sql complex fields
            //ResolveSqlFields(results);

            return results;
        }

        #region experimental
        private void PrepareLists<T>(List<T> objs)
        {
            List<int> idsObjs = new List<int>();
            foreach (T item in objs)
            {
                int id = (item as BaseModel).ID;
                if (!idsObjs.Contains(id))
                    idsObjs.Add(id);
            }
        }


        private void FillDependenciesExperimental<T>(List<T> objs)
        {
            //get the type of the objects
            Type type = typeof(T);

            //get the properties of the objects
            var objectProperties = type.GetProperties().ToList();
            //reset the stati list that contains the enumerated types
            enumeratedTypes = new List<Type>();

            //prepare the method info to execute the method selectobjctsnofill (select without dependency resolution)
            MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.SelectObjectsNoFill));
            baseModelTypes = new Dictionary<string, Type>();
            //init the dictionary that contains all the list of objects
            Dictionary<string, IList> listOfObjects = new Dictionary<string, IList>();

            //enumerate all needed types
            GetAllSubTypes(typeof(T));



            //preapre list
            foreach (var baseModelType in baseModelTypes)
            {
                if (baseModelType.Key.StartsWith("mtm")) //mtm case
                {

                    //var dependencyAttribute = pro.PropertyType.GenericTypeArguments[0].GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                    //// do list select
                    //var tableAttribute = pro.PropertyType.GenericTypeArguments[0].GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                    //MethodInfo generic = method.MakeGenericMethod(pro.PropertyType.GenericTypeArguments[0]);
                    //var idRef = (obj as BaseModel).ID;
                    //var columnRef = obj.GetType().Name;
                    //object[] parameters = { idRef, valueInfo.ManyToManySQLName, columnRef };
                    //pro.SetValue(obj, generic.Invoke(this, parameters));

                    string[] keyParts = baseModelType.Key.Split('_');
                    MethodInfo generic = method.MakeGenericMethod(baseModelType.Value);
                    object[] parameters = { null, keyParts[1], null }; // the method requires a string parameter instead of a classinfo
                    IList value = (IList)generic.Invoke(this, parameters); // it returns a list
                                                                           //List<BaseModel> valueBaseModel = value.Cast<BaseModel>().ToList();

                    if (!listOfObjects.ContainsKey(baseModelType.Key))
                        listOfObjects.Add(baseModelType.Key, value);
                }
                else
                {
                    if (UtilityMethods.FindAllParentsTypes(baseModelType.Value).Contains(typeof(BaseModel)))
                    {
                        MethodInfo generic = method.MakeGenericMethod(baseModelType.Value);
                        var tableAttribute = baseModelType.Value.GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                        object[] parameters = { null, null, null }; // the method requires a string parameter instead of a classinfo
                        IList value = (IList)generic.Invoke(this, parameters); // it returns a list
                                                                               //List<BaseModel> valueBaseModel = value.Cast<BaseModel>().ToList();

                        if (!listOfObjects.ContainsKey(baseModelType.Key))
                            listOfObjects.Add(baseModelType.Key, value);
                    }
                }

            }

            enumeratedTypes = new List<Type>();
            //assing items
            Parallel.ForEach(objs, obj =>
            {
                AssignDependencies(obj as BaseModel, listOfObjects, new List<Type>() { type });
            });
            //foreach (var obj in objs)
            //    AssignDependencies(obj as BaseModel, listOfObjects, new List<Type>() { type });

        }
        private static List<Type> enumeratedTypes = new List<Type>();
        private static Dictionary<string, Type> baseModelTypes = new Dictionary<string, Type>();
        private static Dictionary<string, Type> dataModelTypes = new Dictionary<string, Type>();
        private static Dictionary<Type, List<int>> listOfIndexes = new Dictionary<Type, List<int>>();

        private void GetAllSubTypes(Type type)
        {
            foreach (var pro in type.GetProperties().ToList() ?? new List<PropertyInfo>())
            {
                //list case
                if (pro.PropertyType.IsGenericType && !pro.PropertyType.FullName.Contains("UniDataSet"))
                {
                    if (UtilityMethods.FindAllParentsTypes(pro.PropertyType.GenericTypeArguments[0]).Contains(typeof(BaseModel)))
                    {
                        //skip all generics without a valueinfo
                        if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                        {
                            //if the link is not many to many
                            if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                            {
                                if (!enumeratedTypes.Contains(pro.PropertyType.GenericTypeArguments[0]))
                                {
                                    enumeratedTypes.Add(pro.PropertyType.GenericTypeArguments[0]);
                                    if (!baseModelTypes.ContainsKey(pro.PropertyType.GenericTypeArguments[0].Name))
                                        baseModelTypes.Add(pro.PropertyType.GenericTypeArguments[0].Name, pro.PropertyType.GenericTypeArguments[0]);
                                    GetAllSubTypes(pro.PropertyType.GenericTypeArguments[0]);
                                }

                            }
                            else //if it is mtm
                            {
                                string keyname = $"mtm_{valueInfo.ManyToManySQLName}_{pro.PropertyType.GenericTypeArguments[0].Name}";

                                if (!baseModelTypes.ContainsKey(keyname))
                                {
                                    //enumeratedTypes.Add(pro.PropertyType.GenericTypeArguments[0]);
                                    if (!baseModelTypes.ContainsKey(keyname))
                                        baseModelTypes.Add(keyname, pro.PropertyType.GenericTypeArguments[0]);
                                    GetAllSubTypes(pro.PropertyType.GenericTypeArguments[0]);
                                }
                            }
                        }
                    }
                }
                else if (UtilityMethods.FindAllParentsTypes(pro.PropertyType).Contains(typeof(BaseModel)))
                {
                    if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                    {
                        if (!enumeratedTypes.Contains(pro.PropertyType))
                        {
                            enumeratedTypes.Add(pro.PropertyType);
                            if (!baseModelTypes.ContainsKey(pro.PropertyType.Name))
                                baseModelTypes.Add(pro.PropertyType.Name, pro.PropertyType);
                            GetAllSubTypes(pro.PropertyType);
                        }
                    }
                }
            }
        }
        private void GetAllSubTypesData(Type type)
        {
            foreach (var pro in type.GetProperties().ToList() ?? new List<PropertyInfo>())
            {
                //list case
                if (pro.PropertyType.IsGenericType)
                {
                    if (UtilityMethods.FindAllParentsTypes(pro.PropertyType.GenericTypeArguments[0]).Contains(typeof(BaseModel)))
                    {
                        //skip all generics without a valueinfo
                        if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                        {
                            //if the link is not many to many
                            if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                            {

                                if (!enumeratedTypes.Contains(pro.PropertyType.GenericTypeArguments[0]))
                                {
                                    enumeratedTypes.Add(pro.PropertyType.GenericTypeArguments[0]);
                                    if (!baseModelTypes.ContainsKey(pro.PropertyType.GenericTypeArguments[0].Name))
                                        baseModelTypes.Add(pro.PropertyType.GenericTypeArguments[0].Name, pro.PropertyType.GenericTypeArguments[0]);
                                    GetAllSubTypes(pro.PropertyType.GenericTypeArguments[0]);
                                }

                            }
                            else //if it is mtm
                            {
                                //if (!enumeratedTypes.Contains(pro.PropertyType.GenericTypeArguments[0]))
                                //{
                                //    enumeratedTypes.Add(pro.PropertyType.GenericTypeArguments[0]);
                                string keyname = $"mtm_{valueInfo.ManyToManySQLName}_{pro.PropertyType.GenericTypeArguments[0].Name}";
                                if (!baseModelTypes.ContainsKey(keyname))
                                    baseModelTypes.Add(keyname, pro.PropertyType.GenericTypeArguments[0]);
                                GetAllSubTypes(pro.PropertyType.GenericTypeArguments[0]);
                                //}
                            }
                        }
                    }
                }
                else if (UtilityMethods.FindAllParentsTypes(pro.PropertyType).Contains(typeof(BaseModel)))
                {
                    if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                    {
                        if (!enumeratedTypes.Contains(pro.PropertyType))
                        {
                            enumeratedTypes.Add(pro.PropertyType);
                            if (!baseModelTypes.ContainsKey(pro.PropertyType.Name))
                                baseModelTypes.Add(pro.PropertyType.Name, pro.PropertyType);
                            GetAllSubTypes(pro.PropertyType);
                        }
                    }
                }
            }
        }


        private void AssignDependencies(BaseModel obj, Dictionary<string, IList> allObjectsLists, List<Type> fatherPath)
        {
            if (obj != null)
            {
                enumeratedTypes.Add(obj.GetType());

                List<PropertyInfo> objectProperties = obj.GetType().GetProperties().ToList();
                foreach (var pro in objectProperties)
                {
                    if (pro.PropertyType.FullName.Contains("UniDataSet"))
                    {
                    }
                    else if (pro.PropertyType.IsGenericType)
                    {

                        if (UtilityMethods.FindAllParentsTypes(pro.PropertyType.GenericTypeArguments[0]).Contains(typeof(BaseModel)))
                        {
                            //skip all generics without a valueinfo
                            if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                            {
                                //if the link is not many to many
                                if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                                {

                                    if (!fatherPath.Contains(pro.PropertyType.GenericTypeArguments[0]))
                                    {
                                        List<Type> mypath = new List<Type>();
                                        foreach (var type in fatherPath)
                                            mypath.Add(type);
                                        mypath.Add(pro.PropertyType.GenericTypeArguments[0]);

                                        var listType = typeof(List<>);
                                        var constructedListType = listType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
                                        IList instanceList = (IList)Activator.CreateInstance(constructedListType);
                                        IList typeList;

                                        if (allObjectsLists.TryGetValue(constructedListType.GenericTypeArguments[0].Name, out typeList))
                                        {
                                            List<BaseModel> typeListBaseModel = typeList.Cast<BaseModel>().ToList();
                                            PropertyInfo idProperty = objectProperties.Find(i => i.Name == $"ID");
                                            List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(obj.GetType()) ?? new List<Type>();
                                            extendedTypes.Add(obj.GetType());
                                            PropertyInfo idChildProperty = null;
                                            foreach (var type in extendedTypes)
                                            {
                                                idChildProperty = pro.PropertyType.GenericTypeArguments[0].GetProperties().ToList().Find(i => i.Name == $"Id{type.Name}");
                                                if (idChildProperty != null)
                                                    break;
                                            }
                                            int id = (int)idProperty.GetValue(obj);

                                            if (idChildProperty != null)
                                            {
                                                foreach (var item in typeListBaseModel)
                                                {
                                                    int idChild = (int)idChildProperty.GetValue(item);
                                                    if (idChild == id)
                                                        instanceList.Add(item);
                                                }

                                                pro.SetValue(obj, instanceList);

                                                foreach (var item in instanceList)
                                                    AssignDependencies(item as BaseModel, allObjectsLists, mypath);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!fatherPath.Contains(pro.PropertyType.GenericTypeArguments[0]))
                                    {
                                        List<Type> mypath = new List<Type>();
                                        foreach (var type in fatherPath)
                                            mypath.Add(type);
                                        mypath.Add(pro.PropertyType.GenericTypeArguments[0]);


                                        var listType = typeof(List<>);
                                        var constructedListType = listType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
                                        IList instanceList = (IList)Activator.CreateInstance(constructedListType);
                                        IList typeList;

                                        string keyname = $"mtm_{valueInfo.ManyToManySQLName}_{pro.PropertyType.GenericTypeArguments[0].Name}";
                                        if (allObjectsLists.TryGetValue(keyname, out typeList))
                                        {
                                            List<BaseModel> typeListBaseModel = typeList.Cast<BaseModel>().ToList();
                                            PropertyInfo idProperty = objectProperties.Find(i => i.Name == $"ID");
                                            PropertyInfo idChildProperty = pro.PropertyType.GenericTypeArguments[0].GetProperties().ToList().Find(i => i.Name == $"IdMtm");
                                            int id = (int)idProperty.GetValue(obj);

                                            if (idChildProperty != null)
                                            {
                                                foreach (var item in typeListBaseModel)
                                                {
                                                    int idChild = (int)idChildProperty.GetValue(item);
                                                    if (idChild == id)
                                                        instanceList.Add(item);
                                                }

                                                pro.SetValue(obj, instanceList);

                                                foreach (var item in instanceList)
                                                    AssignDependencies(item as BaseModel, allObjectsLists, mypath);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (UtilityMethods.FindAllParentsTypes(pro.PropertyType).Contains(typeof(BaseModel)))
                    {
                        if (pro.PropertyType == typeof(UniImage))
                        {

                        }
                        else if (!fatherPath.Contains(pro.PropertyType))
                        {
                            List<Type> mypath = new List<Type>();
                            foreach (var type in fatherPath)
                                mypath.Add(type);
                            mypath.Add(pro.PropertyType);


                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(pro.PropertyType);
                            var instance = Activator.CreateInstance(constructedListType);
                            IList typeList;

                            if (allObjectsLists.TryGetValue(constructedListType.GenericTypeArguments[0].Name, out typeList))
                            {
                                List<BaseModel> typeListBaseModel = typeList.Cast<BaseModel>().ToList();

                                PropertyInfo idItemProperty = objectProperties.Find(i => i.Name == $"Id{pro.Name}");
                                if (idItemProperty != null)
                                {
                                    int idItem = (int)idItemProperty.GetValue(obj);
                                    var value = typeListBaseModel.Find(i => i.ID == idItem);
                                    pro.SetValue(obj, value);

                                    AssignDependencies(value, allObjectsLists, mypath);
                                }

                            }
                        }
                    }
                }
            }
            else
            {
            }
        }


        #endregion


        private void FillDependencies<T>(T obj)
        {
            Type type = typeof(T);

            var objectProperties = type.GetProperties();

            MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.SelectObjects));

            foreach (var pro in objectProperties)
            {
                var proType = pro.PropertyType;
                var proName = pro.Name;

                if (proName == "Referee")
                {
                    var multipleRelationObject = obj as IMultipleRelation;

                    Type matchingType = null;

                    var libraries = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(a => a.Name.EndsWith(".Library"));
                    var allTypes = new List<Type>();

                    foreach (var lib in libraries)
                        allTypes.AddRange(Assembly.Load(lib).GetTypes());

                    foreach (var classType in allTypes)
                        if (classType.GetCustomAttribute(typeof(ClassInfo)) is ClassInfo classInfo)
                            if (classInfo.SQLName == multipleRelationObject.TableRef)
                                matchingType = classType;

                    if (matchingType != null)
                    {
                        MethodInfo generic = method.MakeGenericMethod(matchingType);
                        object[] parameters = { multipleRelationObject.IdRef, multipleRelationObject.TableRef, null };
                        IList value = (IList)generic.Invoke(this, parameters); // it returns a list
                        if (value.Count > 0)
                            pro.SetValue(obj, value[0]);
                    }
                    continue;
                }

                if (proType.IsGenericType)
                {
                    if (!UtilityMethods.FindAllParentsTypes(pro.PropertyType.GenericTypeArguments[0]).Contains(typeof(BaseModel)))
                        continue;
                    else
                    {
                        //skip all generics without a valueinfo
                        if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                        {
                            //if the link is many to many
                            if (!string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                            {
                                var dependencyAttribute = pro.PropertyType.GenericTypeArguments[0].GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                                // do list select
                                var tableAttribute = pro.PropertyType.GenericTypeArguments[0].GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                                MethodInfo generic = method.MakeGenericMethod(pro.PropertyType.GenericTypeArguments[0]);
                                var idRef = (obj as BaseModel).ID;
                                var columnRef = "Mtm";
                                object[] parameters = { idRef, valueInfo.ManyToManySQLName, columnRef };
                                pro.SetValue(obj, generic.Invoke(this, parameters));
                                continue;
                            }
                            else
                            {
                                // do list select
                                var tableAttribute = pro.PropertyType.GenericTypeArguments[0].GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                                MethodInfo generic = method.MakeGenericMethod(pro.PropertyType.GenericTypeArguments[0]);
                                var idRef = (obj as BaseModel).ID;
                                var columnRef = obj.GetType().Name;
                                object[] parameters = { idRef, tableAttribute.SQLName, columnRef };
                                pro.SetValue(obj, generic.Invoke(this, parameters));
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    if (UtilityMethods.FindAllParentsTypes(pro.PropertyType).Contains(typeof(BaseModel)))
                    {
                        // do single select
                        MethodInfo generic = method.MakeGenericMethod(proType);

                        var referencingProperty = objectProperties.FirstOrDefault(p => p.Name == $"Id{proName}");
                        if (referencingProperty != null)
                        {
                            var tableAttribute = proType.GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;
                            object[] parameters = { referencingProperty.GetValue(obj), tableAttribute.SQLName, null }; // the method requires a string parameter instead of a classinfo
                            IList value = (IList)generic.Invoke(this, parameters); // it returns a list
                            if (value.Count > 0)
                                pro.SetValue(obj, value[0]);
                            continue;
                        }
                        else
                            throw new ArgumentException($"This class has no related Id{proName} member to populate encapsulated {proName} member.", "Error");
                    }
                    else 
                        continue;
                }
            }
        }

        /// <summary>
        /// Generic method to perform DELETE WHERE ID = ... on any table
        /// </summary>
        /// <param name="obj" the object to be deleted></param>
        public async void DeleteObject(BaseModel obj)
        {

            Type type = obj.GetType();

            var tableAttritbute = type.GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

            await SetData("DELETE FROM " + tableAttritbute.SQLName + $" WHERE id = {obj.ID}");
        }
        public async void DeleteObject(Type type, int id)
        {
            var tableAttritbute = type.GetCustomAttribute(typeof(ClassInfo)) as ClassInfo;

            await SetData("DELETE FROM " + tableAttritbute?.SQLName + $" WHERE id = {id}");
        }


        public async void InsertLinkObjects(string tablename, Type type1, int idObj1, Type type2, int idObj2, string columnReference1Name, string columnReference2Name)
        {
            if (string.IsNullOrWhiteSpace(columnReference1Name))
                columnReference1Name = $"id{type1.Name}";

            if (string.IsNullOrWhiteSpace(columnReference2Name))
                columnReference2Name = $"id{type2.Name}";

            string query = "INSERT INTO " + tablename + $" ({columnReference1Name}, {columnReference2Name}) VALUES ({idObj1} ,{idObj2})";
            await SetData(query);
        }
        public async void DeleteLinkFromObjects(string tablename, BaseModel obj1, BaseModel obj2, string columnReference1Name, string columnReference2Name)
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
        #endregion

        #region Error events

        internal static event EventHandler<MySqlErrorEventArgs> MySqlError;
        internal static void OnMySqlError(MySqlErrorEventArgs e)
        {
            MySqlError?.Invoke(null, e);
        }

        internal class MySqlErrorEventArgs : EventArgs
        {
            public MySqlErrorEventArgs(Exception _exception)
            {
                Exception = _exception;
            }

            public Exception Exception { get; }
        }
        #endregion



        public List<T> FilterListCommand<T>(string searchText, List<BaseModel> itemsWithDependencies, List<BaseModel> itemsToFilter)
        {
            MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.FilterListParameter));
            MethodInfo generic = method.MakeGenericMethod(typeof(T));


            string[] searchRequests = new string[] { searchText };
            if (searchText.Contains(","))
            {
                searchRequests = searchText.Split(',');
            }

            List<T> items = new List<T>();
            foreach (string text in searchRequests)
            {
                object[] parameters = { text, itemsWithDependencies, itemsToFilter };
                items = generic.Invoke(this, parameters) as List<T>;
                itemsToFilter = items.Cast<BaseModel>().ToList();
            }
            return items;

        }

        public List<T> FilterListParameter<T>(string searchText, List<BaseModel> itemsWithDependencies, List<BaseModel> itemsToFilter)
        {


            List<BaseModel> filteredItemsSource = new List<BaseModel>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var item in itemsToFilter)
                {
                    foreach (var property in typeof(T).GetProperties() ?? new PropertyInfo[] { })
                    {
                        if (property.Name == "Created" || property.Name == "LastModify")
                        {

                        }
                        else if (property.PropertyType == typeof(DateTime))
                        {
                            DateTime value = (DateTime)property.GetValue(item);
                            DateTimeOffset dateTimeOffset = new DateTimeOffset(value.ToLocalTime());
                            if (dateTimeOffset.ToString().ToLower().Trim().Contains(searchText))
                                filteredItemsSource.Add(item);
                        }
                        else if (!UtilityMethods.FindAllParentsTypes(property.PropertyType).Contains(typeof(BaseModel)) && !property.PropertyType.IsGenericType)
                        {
                            if (item != null)
                            {
                                var val = property.GetValue(item);
                                if (val != null)
                                {
                                    string value = property.GetValue(item).ToString().ToLower().Trim() ?? string.Empty;
                                    if (value.Contains(searchText.ToLower().Trim()))
                                    {
                                        if (!filteredItemsSource.Contains(item))
                                            filteredItemsSource.Add(item);
                                    }
                                }
                            }

                        }
                        else if (property.PropertyType.IsGenericType && !property.PropertyType.FullName.Contains("UniDataSet"))

                        {
                            MethodInfo method = typeof(DbContextV1).GetMethod(nameof(DbContextV1.FilterListParameter));
                            MethodInfo generic = method.MakeGenericMethod(property.PropertyType.GenericTypeArguments[0]);
                            BaseModel itemWithDependencies = itemsWithDependencies.Find(i => i.ID == item.ID);

                            List<BaseModel> dependencyToFilter = (property.GetValue(itemWithDependencies) as IList)?.Cast<BaseModel>().ToList();
                            if(dependencyToFilter != null)
                            {
                                object[] parameters = { searchText, dependencyToFilter as List<BaseModel>, dependencyToFilter as List<BaseModel> };
                                IList items = generic.Invoke(this, parameters) as IList;
                                if (items?.Count > 0)
                                    if (!filteredItemsSource.Contains(item))
                                        filteredItemsSource.Add(item);
                            }
                          
                        }
                        else if (UtilityMethods.FindAllParentsTypes(property.PropertyType).Contains(typeof(BaseModel)))
                        {
                            BaseModel itemWithDependencies = itemsWithDependencies.Find(i => i.ID == item.ID);
                            if (SearchValueInObject(searchText, property.GetValue(itemWithDependencies)))
                            {
                                if (itemWithDependencies != null)
                                {
                                    if (!filteredItemsSource.Contains(item))
                                    {
                                        filteredItemsSource.Add(item);
                                        break;
                                    }

                                }
                            }
                        }

                    }
                }

            }

            return filteredItemsSource.Cast<T>().ToList();
        }


        public bool SearchValueInObject(string searchText, object item)
        {
            if (item != null)
            {
                Type t = item.GetType();
                foreach (var property in t.GetProperties() ?? new PropertyInfo[] { })
                {
                    if (property.Name != "Created" && property.Name != "LastModify" && property.PropertyType != typeof(DateTime))
                    {
                        var val = property.GetValue(item);
                        if (val != null)
                        {
                            string value = val.ToString().ToLower().Trim() ?? string.Empty;
                            if (value.Contains(searchText.ToLower().Trim()))
                            {
                                return true;
                            }
                        }
                    }


                }
            }

            return false;
        }

        public static object TryConvert(object source, Type dest)
        {
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
            catch (Exception e)
            {

            }
            return null;
        }

        public static bool CompareObjects(object obj1, object obj2, Type dest)
        {
            try
            {
                if (dest == typeof(double))
                {
                    double val1 = Convert.ToDouble(obj1);
                    double val2 = Convert.ToDouble(obj2);
                    if (val1 == val2) return true;
                    else return false;
                }
                else if (dest == typeof(bool))
                {
                    bool val1 = Convert.ToBoolean(obj1);
                    bool val2 = Convert.ToBoolean(obj2);
                    if (val1 == val2) return true;
                    else return false;
                }
                else if (dest == typeof(byte))
                {
                    byte val1 = Convert.ToByte(obj1);
                    byte val2 = Convert.ToByte(obj2);
                    if (val1 == val2) return true;
                    else return false;
                }
                else if (dest == typeof(int))
                {
                    int val1 = Convert.ToInt32(obj1);
                    int val2 = Convert.ToInt32(obj2);
                    if (val1 == val2) return true;
                    else return false;
                }
                else if (dest == typeof(decimal))
                {
                    decimal val1 = Convert.ToDecimal(obj1);
                    decimal val2 = Convert.ToDecimal(obj2);
                    if (val1 == val2) return true;
                    else return false;
                }
                else if (dest == typeof(string))
                {
                    string val1 = Convert.ToString(obj1).Trim();
                    string val2 = Convert.ToString(obj2).Trim();
                    if (val1 == val2) return true;
                    else return false;
                }
                else if (dest == typeof(DateTime))
                {
                    DateTime val1 = Convert.ToDateTime(obj1);
                    DateTime val2 = Convert.ToDateTime(obj2);
                    if (val1 == val2) return true;
                    else return false;
                }
            }
            catch (Exception e)
            {

            }
            return false;
        }
    }


}
