using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using UNI.API.Contracts.Models;
using UNI.API.Contracts.RequestsDTO;
using UNI.API.DAL.v2;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;

namespace UNI.API.Client;

public class UNIBlazorClient<T> where T : BaseModel
{
    #region CTOR

    /// <summary>
    /// Corresponds to the controller base route = server url + /api/v{version:apiVersion}/[controller]
    /// </summary>
    private static readonly List<string> baseEndpoints = new();

    private UNIClientConfiguration? configuration;
    public UNIBlazorUser UNIBlazorUser { get; set; }

    public UNIBlazorClient(UNIBlazorUser user)
    {
        UniClientInitialization();
        UNIBlazorUser = user;
    }

    /// <summary>
    /// Xamarin needs to pass configuration section
    /// </summary>
    /// <param name="configurationSection"></param>
    public UNIBlazorClient(IConfigurationSection configurationSection, UNIBlazorUser user)
    {
        UniClientInitialization(configurationSection);
        UNIBlazorUser = user;
    }

    private void UniClientInitialization(IConfigurationSection? configurationSection = null)
    {
        if (configurationSection != null)
        {
            configuration = configurationSection
               .Get<UNIClientConfiguration>();
        }
        else
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection(nameof(UNIClientConfiguration))
                .Get<UNIClientConfiguration>();
        }

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration), $"Check the section {nameof(UNIClientConfiguration)} in your appsettings.json");

        foreach (string server in configuration.ServerUrls)
            if (!string.IsNullOrWhiteSpace(server) && !baseEndpoints.Contains(server + $"/api/{configuration.ApiVersion}/"))
                baseEndpoints.Add(server + $"/api/{configuration.ApiVersion}/");

        ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
    }

    #endregion

    #region Identity methods

    /// <summary>
    /// Get the JWT token for the user
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<UNIToken?> Authenticate(string username, string password)
    {
        Credentials credentials = new() { Username = username, Password = password };

        RestRequest request = PreparePostRequestWithBody(credentials);

        var response = await ProcessRequest<UNIToken>(request, additionalRoute: "identity/token", isTokenRequest: true, isIdentityRequest: true);

        return response?.Data;
    }

    /// <summary>
    /// Authenticated users/clients (they have token above) can call this to change their password at any time
    /// </summary>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    public async Task<bool> ChangePassword(ChangePasswordRequestDTO requestDTO)
    {
        RestRequest request = PreparePostRequestWithBody(requestDTO);

        var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/changePassword", isIdentityRequest: true);

        return response != null && response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> ResetPassword(Credentials newCredentials)
    {
        RestRequest request = PreparePostRequestWithBody(newCredentials);

        var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/admin/resetPassword", isIdentityRequest: true);

        return response != null && response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<int> CreateCredential(Credentials credentials)
    {
        RestRequest request = PreparePostRequestWithBody(credentials);

        var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/admin/createCredential", isIdentityRequest: true);
        if (response == null || response.Content == null || response.StatusCode != HttpStatusCode.OK)
            return 0;
        else return Convert.ToInt32(response.Content);
    }

    #endregion

    #region Public CRUD methods
    public async Task<T?> CreateItem(T item)
    {
        InitBaseModel(item);

        RestRequest request = PreparePostRequestWithBody(item);

        var response = await ProcessRequest<T>(request);
        if (response != null && response.Data != null)
            InitBaseModel(response.Data);

        return response?.Data;
    }

    public async Task<List<T>?> GetItems()
    {
        List<T> res = new();

        RestRequest request = new() { Method = Method.Get };
        var response = await ProcessRequest<List<T>>(request);

        if (response?.Data == null && response?.Content != null)
            return await DeserializeInterface();

        if (response?.Data != null)
        {
            InitBaseModelList(response.Data);
            res = response.Data;
        }

        return res;
    }

    public async Task<int> DeleteItem(T item)
    {
        InitBaseModel(item);

        RestRequest request = new() { Method = Method.Delete };
        string body = JsonConvert.SerializeObject(item.ID);
        request.AddParameter("application/json", body, ParameterType.RequestBody);

        var response = await ProcessRequest<T>(request);

        return Convert.ToInt32(response?.StatusCode);
    }

    public async Task<int> UpdateItem(T item)
    {
        InitBaseModel(item);

        RestRequest request = PreparePostRequestWithBody(item);
        request.Method = Method.Put;

        var response = await ProcessRequest<T>(request);

        return Convert.ToInt32(response?.StatusCode);
    }

    public async Task<List<T>?> Get(GetDataSetRequestDTO requestDTO)
    {
        ApiResponseModel<T>? apiResponseModel = await GetDataSet(requestDTO);
        return apiResponseModel?.ResponseBaseModels;
    }

    public async Task<ApiResponseModel<T>?> GetDataSet(GetDataSetRequestDTO requestDTO)
    {
        //try
        //{
            RestRequest request = configuration!.ApiVersion switch
            {
                "v1" => GetRequestV1(requestDTO),
                "v2" => PreparePostRequestWithBody(requestDTO),
                _ => throw new NotImplementedException("Missing apiVersion in appsettings"),
            };

            var response = await ProcessRequest<ApiResponseModel<T>>(request, additionalRoute: "/get");

            if (response == null || response.Content == null || response.StatusCode != HttpStatusCode.OK)
                return null;

            ApiResponseModel<T>? apiResponseModel = JsonConvert.DeserializeObject<ApiResponseModel<T>>(response.Content.ToString());

            if (apiResponseModel == null)
                return null;

            if (!requestDTO.SkipInit)
            {
                if (apiResponseModel.BaseModelDependencies != null && apiResponseModel.BaseModelDependencies.Any())
                    Parallel.ForEach(apiResponseModel.ResponseBaseModels, obj =>
                    {
                        AssignDependencies(obj, apiResponseModel.BaseModelDependencies, new List<Type>() { typeof(T) });
                    });
                else
                    Parallel.ForEach(apiResponseModel.ResponseBaseModels, obj =>
                    {
                        InitBaseClientsDependencies(obj, new List<Type>() { typeof(T) });
                    });

                InitBaseModelList(apiResponseModel.ResponseBaseModels);
            }

            return new ApiResponseModel<T>()
            {
                ResponseBaseModels = apiResponseModel.ResponseBaseModels,
                Count = apiResponseModel.Count,
                DataBlocks = apiResponseModel.DataBlocks
            };
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.Message);
        //    return null;
        //}
    }

    public async Task<int> CreateItems(List<T> items)
    {
        InitBaseModelList(items);

        RestRequest request = PreparePostRequestWithBody(items);

        var response = await ProcessRequest<T>(request, additionalRoute: "/List");

        return Convert.ToInt32(response?.StatusCode);
    }

    public async Task<int> UpdateItems(List<T> items)
    {
        InitBaseModelList(items);

        RestRequest request = PreparePostRequestWithBody(items);
        request.Method = Method.Put;

        var response = await ProcessRequest<T>(request, additionalRoute: "/List");

        return Convert.ToInt32(response?.StatusCode);
    }

    public async Task<List<T>?> GetItemsSkipLists()
    {
        List<T> res = new();
        RestRequest request = new() { Method = Method.Get };

        var response = await ProcessRequest<List<T>>(request, additionalRoute: "/skiplist");

        if (response?.Data == null && response?.Content != null)
            return await DeserializeInterface();

        if (response?.Data != null)
        {
            InitBaseModelList(response.Data);
            res = response.Data;
        }

        return res;
    }

    public async Task<T?> CreateRapidItem(T item)
    {
        InitBaseModel(item);

        RestRequest request = PreparePostRequestWithBody(item);

        var response = await ProcessRequest<T>(request, additionalRoute: "/PostRapid");

        if (response?.Data != null)
            InitBaseModel(response.Data);

        return response?.Data;
    }

    public async Task<List<T>?> GetItemsWhere(int id, string idName)
    {
        RestRequest request = new() { Method = Method.Get };
        request.AddParameter("id", id, ParameterType.QueryString);
        request.AddParameter("idName", idName, ParameterType.QueryString);

        var response = await ProcessRequest<List<T>>(request, additionalRoute: "/GetWhere");

        if (response?.Data != null)
            InitBaseModelList(response.Data);

        return response?.Data;
    }

    #endregion

    #region Private methods

    private static RestRequest PreparePostRequestWithBody(object payload)
    {
        RestRequest request = new() { Method = Method.Post };

        string body = JsonConvert.SerializeObject(payload);
        request.AddParameter("application/json", body, ParameterType.RequestBody);

        return request;
    }

    private async Task<RestResponse<K>?> ProcessRequest<K>(RestRequest request, string? additionalRoute = null, bool isTokenRequest = false, bool isIdentityRequest = false)
    {
        // refresh token if expired
        if (configuration!.IsTokenAutoRefreshable)
        {
            if (!isTokenRequest && UNIBlazorUser.Token != null)
            {
                JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(UNIBlazorUser.Token.Value);
                if (jwtSecurityToken.ValidTo <= DateTime.UtcNow)
                    UNIBlazorUser.Token = await Authenticate(UNIBlazorUser.Username!, UNIBlazorUser.Password!);
            }
        }

        int attempt = 0;
        RestResponse<K>? response;
        do
        {
            string typeName = isIdentityRequest ? string.Empty : typeof(T).Name;

            RestClient client = GetRestClient(attempt, typeName, additionalRoute);

            if (!isTokenRequest)
            {
                if (UNIBlazorUser.Token == null)
                {
                    if (configuration!.IsTokenAutoRefreshable)
                        UNIBlazorUser.Token = await Authenticate(UNIBlazorUser.Username!, UNIBlazorUser.Password!);
                    else
                        throw new Exception("Need to authenticate and obtain token.");
                }

                client.AddDefaultHeader("Authorization", $"Bearer {UNIBlazorUser.Token!.Value}");
            }

            response = await client.ExecuteAsync<K>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (attempt > 0)
                {
                    string workingEndpoint = baseEndpoints[attempt];
                    baseEndpoints.RemoveAt(attempt);
                    baseEndpoints.Insert(0, workingEndpoint);
                }
                break;
            }

            attempt++;
        }
        while (attempt < baseEndpoints.Count);

        return response;
    }

    private static RestClient GetRestClient(int attempt, string typeName, string? additionalRoute = null)
    {
        string baseEndpoint = baseEndpoints[attempt] + typeName;
        if (!string.IsNullOrWhiteSpace(additionalRoute))
            baseEndpoint += additionalRoute;

        RestClientOptions options = new()
        {
            //BaseHost = baseEndpoint,
            BaseUrl = new Uri(baseEndpoint),
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };

        return new RestClient(options);
    }

    private RestRequest GetRequestV1(GetDataSetRequestDTO requestDTO)
    {
        RestRequest request = new() { Method = Method.Get };

        if (requestDTO.Id != null)
            request.AddParameter("id", requestDTO.Id, ParameterType.QueryString);
        if (requestDTO.IdName != null)
            request.AddParameter("idName", requestDTO.IdName, ParameterType.QueryString);
        if (requestDTO.RequestedEntriesNumber != null)
            request.AddParameter("requestedEntriesNumber", requestDTO.RequestedEntriesNumber, ParameterType.QueryString);
        if (requestDTO.BlockToReturn != null)
            request.AddParameter("blockToReturn", requestDTO.BlockToReturn, ParameterType.QueryString);
        if (requestDTO.FilterText != null)
            request.AddParameter("filterText", requestDTO.FilterText, ParameterType.QueryString);

        request.AddParameter("filterDateFormat", requestDTO.FilterDateFormat, ParameterType.QueryString);

        // if you need/want to skip the null check, make default values or make properties not nullable!
        if (requestDTO.FilterExpressions != null && requestDTO.FilterExpressions.Any())
            foreach (var filterexpression in requestDTO.FilterExpressions)
                request.AddParameter("filterExpression", $"{filterexpression.PropertyName},{filterexpression.PropertyValue},{filterexpression.ComparisonType}", ParameterType.QueryString);

        return request;
    }

    private static async Task<List<T>?> DeserializeInterface()
    {
        if (!typeof(T).IsInterface)
            return null;

        BaseModel baseModel = new();//if you not use any class it will not load the assembly

        List<T> values = new();

        var library = Assembly.GetExecutingAssembly().GetReferencedAssemblies().First(a => a.Name == "UNI.Core.Library");
        Assembly assLib = Assembly.Load(library);

        var types = assLib.GetTypes().Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface);

        // reflection on generic method with run-time determined types
        foreach (Type type in types)
        {
            Type myClassType = typeof(UNIBlazorClient<>);
            Type[] typeArgs = { type };
            Type constructed = myClassType.MakeGenericType(typeArgs);

            // Create instance of generic type
            var myClassInstance = Activator.CreateInstance(constructed);

            // Find GetAll() method and invoke
            MethodInfo? getAllMethod = constructed.GetMethod("GetItems");

            if (getAllMethod == null || myClassInstance == null)
                continue;

            var resultTask = getAllMethod.Invoke(myClassInstance, new object[] { 0 });

            if (resultTask == null || resultTask is not Task result)
                continue;

            await result.ConfigureAwait(false);
            PropertyInfo? resultProperty = result.GetType().GetProperty("Result");

            if (resultProperty == null)
                continue;

            var vals = resultProperty.GetValue(result);

            if (vals == null || vals is not IList listValues)
                continue;

            foreach (var item in listValues)
                values.Add((T)item);
        }

        return values;
    }

    private void InitBaseModel(BaseModel obj)
    {
        if (obj == null)
            return;

        obj.Loaded();

        foreach (var pro in obj.GetType().GetProperties())
        {
            if (pro.PropertyType == null)
                continue;

            if (pro.PropertyType.IsGenericType)
            {
                if (pro.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                    if (!string.IsNullOrEmpty(pro.PropertyType.FullName) && !pro.PropertyType.FullName.Contains("UniDataSet"))
                        if (pro.GetValue(obj) is IList list)
                            InitBaseModelList(list.Cast<BaseModel>().ToList());
            }
            else if (pro.PropertyType.IsSubclassOf(typeof(BaseModel)))
            {
                if (pro.GetValue(obj) is BaseModel model)
                    InitBaseModel(model);
            }
        }
    }

    private void InitBaseModelList(List<BaseModel> objs)
    {
        foreach (BaseModel obj in objs)
            InitBaseModel(obj);
    }

    private void InitBaseModelList(List<T> objs)
    {
        foreach (T obj in objs)
            InitBaseModel(obj);
    }

    private void InitBaseClientsDependencies(BaseModel obj, List<Type> fatherPath)
    {
        if (obj == null)
            return;

        foreach (var pro in obj.GetType().GetProperties().ToList())
        {
            if (pro.PropertyType.IsGenericType)
            {
                if (!pro.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                    continue;

                //skip all generics without a valueinfo
                if (pro.GetCustomAttribute(typeof(ValueInfo)) is not ValueInfo valueInfo || valueInfo == null)
                    continue;

                //if the property is unidataset
                if (pro.PropertyType.FullName != null && pro.PropertyType.FullName.Contains("UniDataSet"))
                    AssignDependenciesGenericUniDataSet(pro, obj);
                else if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName) || !fatherPath.Contains(pro.PropertyType.GenericTypeArguments[0])) //if the link is not many to many
                    if (pro.GetValue(obj) is IList instanceList)
                        foreach (var item in instanceList)
                            if (item is BaseModel bm)
                                InitBaseClientsDependencies(bm, DALHelper.GetMyPath(fatherPath, pro.PropertyType.GenericTypeArguments[0]));
            }
            else if (pro.PropertyType.IsSubclassOf(typeof(BaseModel)))
            {
                if (pro.PropertyType == typeof(UniImage))
                    continue;

                if (fatherPath.Contains(pro.PropertyType))
                    continue;

                if (pro.GetValue(obj) is BaseModel value)
                    InitBaseClientsDependencies(value, DALHelper.GetMyPath(fatherPath, pro.PropertyType));
            }
        }
    }

    private void AssignDependencies(BaseModel obj, Dictionary<string, IList> allObjectsLists, List<Type> fatherPath)
    {
        if (obj == null)
            return;

        var objectProperties = obj.GetType().GetProperties().ToList();

        foreach (var pro in objectProperties)
        {
            if (pro.PropertyType.IsGenericType)
            {
                if (!pro.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                    continue;

                //skip all generics without a valueinfo
                var valueInfo = pro.GetCustomAttribute(typeof(ValueInfo));
                if (valueInfo == null)
                    continue;

                //if the property is unidataset
                if (pro.PropertyType.FullName != null && pro.PropertyType.FullName.Contains("UniDataSet"))
                    AssignDependenciesGenericUniDataSet(pro, obj);
                else
                    AssignDependenciesGeneric(pro, fatherPath, allObjectsLists, objectProperties, obj, (ValueInfo)valueInfo);
            }
            else if (pro.PropertyType.IsSubclassOf(typeof(BaseModel)))
            {
                if (pro.PropertyType == typeof(UniImage))
                    continue;

                AssignDependenciesBaseModel(pro, fatherPath, allObjectsLists, objectProperties, obj);
            }
        }
    }

    private static void AssignDependenciesGenericUniDataSet(PropertyInfo pro, BaseModel obj)
    {
        //TODO fixare creazione UNIBlazorClient in xamarin e altri framework
        try
        {
            //create unidataset instance
            var datasetType = typeof(UNIDataSet<>);
            var constructedDatasetType = datasetType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
            var instanceDataset = Activator.CreateInstance(constructedDatasetType);

            //create baseclient instance
            var baseclientType = typeof(UNIBlazorClient<>);
            var constructedBaseClientType = baseclientType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
            var instanceBaseClient = Activator.CreateInstance(constructedBaseClientType);

            if (instanceDataset == null)
                return;

            foreach (var prop in instanceDataset.GetType().GetProperties().ToList())
                if (prop.PropertyType.FullName != null && prop.PropertyType.FullName.Contains("BaseClient"))
                    prop.SetValue(instanceDataset, instanceBaseClient);

            pro.SetValue(obj, instanceDataset);
        }
        catch(Exception ex)
        {

        }

    }

    private void AssignDependenciesGeneric(PropertyInfo pro, List<Type> fatherPath, Dictionary<string, IList> allObjectsLists, List<PropertyInfo> objectProperties, BaseModel obj, ValueInfo valueInfo)
    {
        var propertyType = pro.PropertyType.GenericTypeArguments[0];

        if (fatherPath.Contains(propertyType))
            return;

        List<Type> mypath = DALHelper.GetMyPath(fatherPath, propertyType);

        Type listType = typeof(List<>);
        Type constructedListType = listType.MakeGenericType(propertyType);
        object? reflectedInstanceList = Activator.CreateInstance(constructedListType);

        if (reflectedInstanceList == null || reflectedInstanceList is not IList instanceList)
            return;

        string keyname = $"mtm_{valueInfo.ManyToManySQLName}_{propertyType.Name}";
        //if it's not MtM
        if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
            keyname = constructedListType.GenericTypeArguments[0].Name;

        if (!allObjectsLists.TryGetValue(keyname, out IList? typeList))
            return;

        List<BaseModel> typeListBaseModel = DALHelper.GetTypeListBaseModel(typeList, propertyType);

        PropertyInfo? idProperty = objectProperties.Find(i => i.Name == "ID");
        PropertyInfo? idChildProperty = propertyType.GetProperties().ToList().FirstOrDefault(i => i.Name == "IdMtm");
        //if it's not MtM
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

        if (idChildProperty == null)
            return;

        int? id = (int?)idProperty?.GetValue(obj);

        foreach (var item in typeListBaseModel)
        {
            int? idChild = (int?)idChildProperty.GetValue(item);
            if (idChild == id && id != null && idChild != null)
                instanceList.Add(item);
        }

        pro.SetValue(obj, instanceList);

        foreach (var item in instanceList)
            if (item is BaseModel bm)
                AssignDependencies(bm, allObjectsLists, mypath);
    }

    private void AssignDependenciesBaseModel(PropertyInfo pro, List<Type> fatherPath, Dictionary<string, IList> allObjectsLists, List<PropertyInfo> objectProperties, BaseModel obj)
    {
        var propertyType = pro.PropertyType.GenericTypeArguments[0];

        if (fatherPath.Contains(propertyType))
            return;

        List<Type> mypath = DALHelper.GetMyPath(fatherPath, propertyType);

        Type listType = typeof(List<>);
        Type constructedListType = listType.MakeGenericType(propertyType);

        if (!allObjectsLists.TryGetValue(constructedListType.GenericTypeArguments[0].Name, out IList? typeList))
            return;

        List<BaseModel> typeListBaseModel = DALHelper.GetTypeListBaseModel(typeList, propertyType);

        PropertyInfo? idItemProperty = objectProperties.Find(i => i.Name == $"Id{pro.Name}");
        if (idItemProperty == null)
            return;

        int? idItem = (int?)idItemProperty.GetValue(obj);
        if (idItem == null)
            return;

        BaseModel? value = typeListBaseModel.Find(i => i.ID == idItem);
        if (value == null)
            return;

        pro.SetValue(obj, value);

        AssignDependencies(value, allObjectsLists, mypath);
    }

    #endregion
}