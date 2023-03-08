using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UNI.API.Contracts.Models;
using UNI.API.Contracts.RequestsDTO;
using UNI.Core.Library;
using UNI.Core.Library.GenericModels;

namespace UNI.Core.Client
{
    public class UniClient<T> where T : BaseModel
    {
        /// <summary>
        /// Corresponds to the controller base route = server url + /api/v{version:apiVersion}/[controller]
        /// </summary>
        private static readonly List<string> baseEndpoints = new List<string>();
        private readonly IConfigurationSection configurationSectionServerUrls;
        private readonly IConfigurationSection configurationSectionApiVersion;

        public UniClient(IConfigurationSection configurationSection = null)
        {
            // Xamarin needs to pass configuration section
            if (configurationSection != null)
            {
                configurationSectionServerUrls = configurationSection.GetSection("ServerUrls");
                configurationSectionApiVersion = configurationSection.GetSection("ApiVersion");
            }
            else
            {
                configurationSectionServerUrls = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ClientConfiguration").GetSection("ServerUrls");
                configurationSectionApiVersion = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ClientConfiguration").GetSection("ApiVersion");
            }

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            foreach (KeyValuePair<string, string> server in configurationSectionServerUrls.AsEnumerable())
                if (!string.IsNullOrWhiteSpace(server.Value) && !baseEndpoints.Contains(server.Value + $"/api/{configurationSectionApiVersion.Value.ToString()}/"))
                    baseEndpoints.Add(server.Value + $"/api/{configurationSectionApiVersion.Value.ToString()}/");
        }

        #region Identity methods

        /// <summary>
        /// Get the JWT token for the user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UNIToken> Authenticate(string username, string password)
        {
            dynamic credentials = new { Username = username, Password = password };

            RestRequest request = UniClient<T>.PreparePostRequestWithBody(credentials);

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

        public async Task<List<User>> GetUsers()
        {
            var res = new List<User>();

            var request = new RestRequest() { Method = Method.Get };

            var response = await ProcessRequest<List<User>>(request, additionalRoute: "identity/admin/users", isIdentityRequest: true);

            if (response?.Data != null)
                res = response.Data;

            return res;
        }

        public async Task<bool> CreateUser(Credentials credentials)
        {
            RestRequest request = PreparePostRequestWithBody(credentials);

            var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/admin/createUser", isIdentityRequest: true);

            return response != null && response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeleteUser(string username)
        {
            var request = new RestRequest() { Method = Method.Delete };

            request.AddQueryParameter("username", username);

            var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/admin/deleteUser", isIdentityRequest: true);

            return response != null && response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Get all roles or just the roles assigned to the user if userId is passed
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<Role>> GetRoles(int? userId)
        {
            var res = new List<Role>();

            var request = new RestRequest() { Method = Method.Get };

            if (userId.HasValue)
                request.AddQueryParameter("userId", (int)userId);

            var response = await ProcessRequest<List<Role>>(request, additionalRoute: "identity/admin/getRoles", isIdentityRequest: true);

            if (response?.Data != null)
                res = response.Data;

            return res;
        }

        public async Task<bool> AssignRoleToUser(ChangeUserRolesDTO requestDTO)
        {
            RestRequest request = PreparePostRequestWithBody(requestDTO);

            var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/admin/assignRole", isIdentityRequest: true);

            return response != null && response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> RemoveRoleFromUser(ChangeUserRolesDTO requestDTO)
        {
            RestRequest request = PreparePostRequestWithBody(requestDTO);

            var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/admin/removeRole", isIdentityRequest: true);

            return response != null && response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> ResetPassword(Credentials newCredentials)
        {
            RestRequest request = PreparePostRequestWithBody(newCredentials);

            var response = await ProcessRequest<RestResponse>(request, additionalRoute: "identity/admin/resetPassword", isIdentityRequest: true);

            return response != null && response.StatusCode == HttpStatusCode.OK;
        }

        #endregion


        #region Public Generic CRUD methods
        public async Task<T> CreateItem(T item)
        {
            InitBaseModel(item);

            var request = new RestRequest() { Method = Method.Post };
            string body = JsonConvert.SerializeObject(item);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await ProcessRequest<T>(request);
            if (response != null && response.Data != null)
                InitBaseModel(response.Data);

            return response?.Data;
        }

        public async Task<List<T>> GetItems()
        {
            var res = new List<T>();

            var request = new RestRequest() { Method = Method.Get };
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

            var request = new RestRequest() { Method = Method.Delete };
            string body = JsonConvert.SerializeObject(item.ID);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await ProcessRequest<T>(request);

            return Convert.ToInt32(response?.StatusCode);
        }

        public async Task<int> UpdateItem(T item)
        {
            InitBaseModel(item);

            var request = new RestRequest() { Method = Method.Put };
            string body = JsonConvert.SerializeObject(item);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await ProcessRequest<T>(request);

            return Convert.ToInt32(response?.StatusCode);
        }

        public async Task<List<T>> Get(GetDataSetRequestDTO requestDTO)
        {
            RestRequest request;

            if (configurationSectionApiVersion.Value.ToString().Equals("v1"))
                request = GetRequestV1(requestDTO);
            else request = GetRequestV2(requestDTO);

            try {

                var response = await ProcessRequest<ApiResponseModel<T>>(request);

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    ApiResponseModel<T> apiResponseModel = JsonConvert.DeserializeObject<ApiResponseModel<T>>(response.Content.ToString());
                    if (apiResponseModel != null)
                    {
                        if (apiResponseModel.BaseModelDependencies != null && apiResponseModel.BaseModelDependencies.Any())
                            foreach (var obj in apiResponseModel.ResponseBaseModels)
                                AssignDependencies(obj, apiResponseModel.BaseModelDependencies, new List<Type>() { typeof(T) });
                        else
                            foreach (var obj in apiResponseModel.ResponseBaseModels)
                                InitBaseClientsDependencies(obj, new List<Type>() { typeof(T) });

                        InitBaseModelList(apiResponseModel.ResponseBaseModels);

                        return apiResponseModel.ResponseBaseModels;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public RestRequest GetRequestV1(GetDataSetRequestDTO requestDTO)
        {
            
                RestRequest request = new RestRequest() { Method = Method.Get };

                if (requestDTO.Id != null)
                    request.AddParameter("id", requestDTO.Id, ParameterType.QueryString);
                if (requestDTO.IdName != null)
                    request.AddParameter("idName", requestDTO.IdName, ParameterType.QueryString);
                if (requestDTO.RequestedEntriesNumber != null)
                    request.AddParameter("requestedEntriesNumber", requestDTO.RequestedEntriesNumber, ParameterType.QueryString);
                if (requestDTO.BlockToReturn != null)
                    request.AddParameter("blockToReturn", requestDTO.BlockToReturn, ParameterType.QueryString);

                request.AddParameter("filterDateFormat", requestDTO.FilterDateFormat, ParameterType.QueryString);

                foreach (var filterexpression in requestDTO.FilterExpressions ?? new List<FilterExpression>())
                    request.AddParameter("filterExpression", $"{filterexpression.PropertyName},{filterexpression.PropertyValue},{filterexpression.ComparisonType}", ParameterType.QueryString);

            return request;

        }
        public RestRequest GetRequestV2(GetDataSetRequestDTO requestDTO)
        {
            RestRequest request = new RestRequest() { Method = Method.Get };
            request.AddJsonBody(requestDTO);
            return request;
        }

        public async Task<ApiResponseModel<T>> GetDataSet(GetDataSetRequestDTO requestDTO)
        {
            try
            {
                RestRequest request;
                var apiResponse = new ApiResponseModel<T>();
                if (configurationSectionApiVersion.Value.ToString().Equals("v1"))
                    request = GetDataSetRequestV1(requestDTO);
                else request = GetRequestV2(requestDTO);

                var response = await ProcessRequest<ApiResponseModel<T>>(request);
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    ApiResponseModel<T> apiResponseModel = JsonConvert.DeserializeObject<ApiResponseModel<T>>(response.Content.ToString());

                    if (apiResponseModel != null)
                    {
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

                        apiResponse.ResponseBaseModels = apiResponseModel.ResponseBaseModels;
                        apiResponse.Count = apiResponseModel.Count;
                        apiResponse.DataBlocks = apiResponseModel.DataBlocks;
                    }
                }
                else
                    return null;

                return apiResponse;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public  RestRequest GetDataSetRequestV1(GetDataSetRequestDTO requestDTO)
        {
            var request = new RestRequest() { Method = Method.Get };

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

            if (requestDTO.FilterExpressions != null)
                foreach (var filterexpression in requestDTO.FilterExpressions)
                    request.AddParameter("filterExpression", $"{filterexpression.PropertyName},{filterexpression.PropertyValue},{filterexpression.ComparisonType}", ParameterType.QueryString);
            return request;
        }

       

        public async Task<int> CreateItems(List<T> items)
        {
            InitBaseModelList(items);

            var request = new RestRequest() { Method = Method.Post };
            string body = JsonConvert.SerializeObject(items);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await ProcessRequest<T>(request, additionalRoute: "/List");

            return Convert.ToInt32(response?.StatusCode);
        }

        public async Task<int> UpdateItems(List<T> items)
        {
            InitBaseModelList(items);

            var request = new RestRequest() { Method = Method.Put };
            string body = JsonConvert.SerializeObject(items);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await ProcessRequest<T>(request, additionalRoute: "/List");

            return Convert.ToInt32(response?.StatusCode);
        }

        public async Task<List<T>> GetItemsSkipLists()
        {
            var res = new List<T>();
            var request = new RestRequest() { Method = Method.Get };

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

        public async Task<T> CreateRapidItem(T item)
        {
            InitBaseModel(item);

            var request = new RestRequest() { Method = Method.Post };
            string body = JsonConvert.SerializeObject(item);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            var response = await ProcessRequest<T>(request, additionalRoute: "/PostRapid");

            if (response?.Data != null)
                InitBaseModel(response.Data);

            return response?.Data;
        }

        public async Task<List<T>> GetItemsWhere(int id, string idName)
        {
            var request = new RestRequest() { Method = Method.Get };
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
            var request = new RestRequest() { Method = Method.Post };

            string body = JsonConvert.SerializeObject(payload);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            return request;
        }
        private async Task<RestResponse<K>> ProcessRequest<K>(RestRequest request, string additionalRoute = null, bool isTokenRequest = false, bool isIdentityRequest = false)
        {
            // refresh token if expired
            if (!isTokenRequest && UNIUser.Token != null)
            {
                JwtSecurityToken jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(UNIUser.Token.Value);
                if (jwtSecurityToken.ValidTo <= DateTime.UtcNow)
                    UNIUser.Token = await Authenticate(UNIUser.Username, UNIUser.Password);
            }

            int attempt = 0;
            RestResponse<K> response;
            do
            {
                string typeName = isIdentityRequest ? string.Empty : typeof(T).Name;

                RestClient client = GetRestClient(attempt, typeName, additionalRoute);

                if (!isTokenRequest)
                {
                    if (UNIUser.Token == null)
                        UNIUser.Token = await Authenticate(UNIUser.Username, UNIUser.Password);

                    client.AddDefaultHeader("Authorization", $"Bearer {UNIUser.Token.Value}");
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

        private static RestClient GetRestClient(int attempt, string typeName, string additionalRoute = null)
        {
            string baseEndpoint = baseEndpoints[attempt] + typeName;
            if (!string.IsNullOrWhiteSpace(additionalRoute))
                baseEndpoint += additionalRoute;

            var options = new RestClientOptions()
            {
                //BaseHost = baseEndpoint,
                BaseUrl = new Uri(baseEndpoint),
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            return new RestClient(options);
        }

        private static async Task<List<T>> DeserializeInterface()
        {
            var baseModel = new BaseModel();//if you not use any class it will not load the assembly
            if (typeof(T).IsInterface)
            {
                var values = new List<T>();

                var library = Assembly.GetExecutingAssembly().GetReferencedAssemblies().First(a => a.Name == "UNI.Core.Library");
                Assembly assLib = Assembly.Load(library);

                var types = assLib.GetTypes().Where(p => typeof(T).IsAssignableFrom(p) && !p.IsInterface);

                // reflection on generic method with run-time determined types
                foreach (Type K in types)
                {
                    Type myClassType = typeof(UniClient<>);
                    Type[] typeArgs = { K };
                    Type constructed = myClassType.MakeGenericType(typeArgs);

                    // Create instance of generic type
                    var myClassInstance = Activator.CreateInstance(constructed);

                    // Find GetAll() method and invoke
                    MethodInfo getAllMethod = constructed.GetMethod("GetItems");
                    if (getAllMethod != null && myClassInstance != null)
                    {
                        var result = (Task)getAllMethod.Invoke(myClassInstance, new object[] { 0 });

                        if (result != null)
                        {
                            await result.ConfigureAwait(false);
                            PropertyInfo resultProperty = result.GetType().GetProperty("Result");
                            if (resultProperty != null)
                            {
                                var vals = (IList)resultProperty.GetValue(result);

                                if (vals != null)
                                    foreach (var item in vals)
                                        values.Add((T)item);
                            }
                        }
                    }
                }
                return values;
            }
            return null;
        }

        private void InitBaseModel(BaseModel obj)
        {
            obj?.Loaded();
            if (obj != null)
            {
                PropertyInfo[] objectProperties = obj.GetType().GetProperties();
                foreach (var pro in objectProperties)
                {
                    if (pro.PropertyType != null && pro.PropertyType.IsGenericType)
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
        }

        private void InitBaseModelList(List<BaseModel> objs)
        {
            foreach (BaseModel obj in objs)
                InitBaseModel(obj);
        }

        private void InitBaseModelList(List<T> objs)
        {
            foreach (T obj in objs ?? new List<T>())
                InitBaseModel(obj);
        }

        private void AssignDependencies(BaseModel obj, Dictionary<string, IList> allObjectsLists, List<Type> fatherPath)
        {
            if (obj != null)
            {
                var objectProperties = obj.GetType().GetProperties().ToList();

                foreach (var pro in objectProperties)
                {
                    if (pro.PropertyType.IsGenericType)
                    {
                        if (pro.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                        {
                            //skip all generics without a valueinfo
                            if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                            {
                                //if the property is unidataset
                                if (pro.PropertyType.FullName != null && pro.PropertyType.FullName.Contains("UniDataSet"))
                                {
                                    //create unidataset instance
                                    var datasetType = typeof(UniDataSet<>);
                                    var constructedDatasetType = datasetType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
                                    var instanceDataset = Activator.CreateInstance(constructedDatasetType);

                                    //create baseclient instance
                                    var baseclientType = typeof(UniClient<>);
                                    var constructedBaseClientType = baseclientType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
                                    var instanceBaseClient = Activator.CreateInstance(constructedBaseClientType, new object[] { configurationSectionServerUrls });

                                    if (instanceDataset != null)
                                    {
                                        foreach (var prop in instanceDataset.GetType().GetProperties().ToList() ?? new List<PropertyInfo>())
                                        {
                                            if (prop.PropertyType.FullName != null && prop.PropertyType.FullName.Contains("BaseClient"))
                                                prop.SetValue(instanceDataset, instanceBaseClient);
                                        }
                                        pro.SetValue(obj, instanceDataset);
                                    }
                                }
                                //if the link is not many to many
                                else if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                                {

                                    if (!fatherPath.Contains(pro.PropertyType.GenericTypeArguments[0]))
                                    {
                                        var mypath = new List<Type>();
                                        foreach (var type in fatherPath)
                                            mypath.Add(type);
                                        mypath.Add(pro.PropertyType.GenericTypeArguments[0]);

                                        var listType = typeof(List<>);
                                        var constructedListType = listType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
                                        IList instanceList = (IList)Activator.CreateInstance(constructedListType);

                                        if (instanceList != null)
                                        {
                                            if (allObjectsLists.TryGetValue(constructedListType.GenericTypeArguments[0].Name, out IList typeList))
                                            {
                                                List<BaseModel> typeListBaseModel = new List<BaseModel>();
                                                foreach (var entry in typeList)
                                                {
                                                    if (entry is JObject jobject)
                                                        typeListBaseModel.Add(jobject.ToObject(pro.PropertyType.GenericTypeArguments[0]) as BaseModel);
                                                }
                                                PropertyInfo idProperty = objectProperties.Find(i => i.Name == $"ID");
                                                PropertyInfo idChildProperty = null;
                                                List<Type> extendedTypes = UtilityMethods.FindAllParentsTypes(obj.GetType());
                                                extendedTypes.Add(obj.GetType());
                                                foreach (var type in extendedTypes)
                                                {
                                                    idChildProperty = pro.PropertyType.GenericTypeArguments[0].GetProperties().ToList().Find(i => i.Name == $"Id{type.Name}");
                                                    if (idChildProperty != null)
                                                        break;
                                                }

                                                int? id = (int?)idProperty?.GetValue(obj);

                                                if (idChildProperty != null)
                                                {
                                                    foreach (var item in typeListBaseModel)
                                                    {
                                                        int? idChild = (int?)idChildProperty.GetValue(item);
                                                        if (idChild == id && id != null && idChild != null)
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

                                        if (instanceList != null)
                                        {
                                            string keyname = $"mtm_{valueInfo.ManyToManySQLName}_{pro.PropertyType.GenericTypeArguments[0].Name}";
                                            if (allObjectsLists.TryGetValue(keyname, out IList typeList))
                                            {
                                                List<BaseModel> typeListBaseModel = new List<BaseModel>();
                                                foreach (var entry in typeList)
                                                {
                                                    if (entry is JObject jobject)
                                                        typeListBaseModel.Add(jobject.ToObject(pro.PropertyType.GenericTypeArguments[0]) as BaseModel);
                                                }
                                                PropertyInfo idProperty = objectProperties.Find(i => i.Name == $"ID");
                                                PropertyInfo idChildProperty = pro.PropertyType.GenericTypeArguments[0].GetProperties().ToList().Find(i => i.Name == $"IdMtm");
                                                int? id = (int?)idProperty?.GetValue(obj);

                                                if (idChildProperty != null)
                                                {
                                                    foreach (var item in typeListBaseModel)
                                                    {
                                                        int? idChild = (int?)idChildProperty.GetValue(item);
                                                        if (idChild == id && id != null && idChild != null)
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
                    }
                    else if (pro.PropertyType.IsSubclassOf(typeof(BaseModel)))
                    {
                        if (pro.PropertyType == typeof(UniImage))
                            continue;

                        if (!fatherPath.Contains(pro.PropertyType))
                        {
                            var mypath = new List<Type>();
                            foreach (var type in fatherPath)
                                mypath.Add(type);

                            mypath.Add(pro.PropertyType);

                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(pro.PropertyType);
                            var instance = Activator.CreateInstance(constructedListType);

                            if (allObjectsLists.TryGetValue(constructedListType.GenericTypeArguments[0].Name, out IList typeList))
                            {

                                List<BaseModel> typeListBaseModel = new List<BaseModel>();
                                foreach (var entry in typeList)
                                {
                                    if (entry is JObject jobject)
                                        typeListBaseModel.Add(jobject.ToObject(pro.PropertyType) as BaseModel);
                                }

                                PropertyInfo idItemProperty = objectProperties.Find(i => i.Name == $"Id{pro.Name}");
                                if (idItemProperty != null)
                                {
                                    int? idItem = (int?)idItemProperty.GetValue(obj);
                                    if (idItem != null)
                                    {
                                        BaseModel value = typeListBaseModel.Find(i => i.ID == idItem);
                                        if (value != null)
                                        {
                                            pro.SetValue(obj, value);

                                            AssignDependencies(value, allObjectsLists, mypath);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitBaseClientsDependencies(BaseModel obj, List<Type> fatherPath)
        {
            if (obj != null)
            {
                List<PropertyInfo> objectProperties = obj.GetType().GetProperties().ToList();
                foreach (var pro in objectProperties)
                {
                    if (pro.PropertyType.IsGenericType)
                    {
                        if (pro.PropertyType.GenericTypeArguments[0].IsSubclassOf(typeof(BaseModel)))
                        {
                            //skip all generics without a valueinfo
                            if (pro.GetCustomAttribute(typeof(ValueInfo)) is ValueInfo valueInfo)
                            {
                                //if the property is unidataset
                                if (pro.PropertyType.FullName != null && pro.PropertyType.FullName.Contains("UniDataSet"))
                                {
                                    //create unidataset instance
                                    var datasetType = typeof(UniDataSet<>);
                                    var constructedDatasetType = datasetType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
                                    var instanceDataset = Activator.CreateInstance(constructedDatasetType);

                                    //create baseclient instance
                                    var baseclientType = typeof(UniClient<>);
                                    var constructedBaseClientType = baseclientType.MakeGenericType(pro.PropertyType.GenericTypeArguments[0]);
                                    var instanceBaseClient = Activator.CreateInstance(constructedBaseClientType, new object[] { configurationSectionServerUrls });

                                    if (instanceDataset != null)
                                        foreach (var prop in instanceDataset.GetType().GetProperties().ToList() ?? new List<PropertyInfo>())
                                            if (prop.PropertyType.FullName != null && prop.PropertyType.FullName.Contains("BaseClient"))
                                                prop.SetValue(instanceDataset, instanceBaseClient);

                                    pro.SetValue(obj, instanceDataset);

                                }
                                //if the link is not many to many
                                else if (string.IsNullOrWhiteSpace(valueInfo.ManyToManySQLName))
                                {
                                    var mypath = new List<Type>();
                                    foreach (var type in fatherPath)
                                        mypath.Add(type);
                                    mypath.Add(pro.PropertyType.GenericTypeArguments[0]);

                                    if (pro.GetValue(obj) is IList instanceList)
                                        foreach (var item in instanceList)
                                            InitBaseClientsDependencies(item as BaseModel, mypath);
                                }
                                else
                                {
                                    if (!fatherPath.Contains(pro.PropertyType.GenericTypeArguments[0]))
                                    {
                                        var mypath = new List<Type>();
                                        foreach (var type in fatherPath)
                                            mypath.Add(type);
                                        mypath.Add(pro.PropertyType.GenericTypeArguments[0]);

                                        IList instanceList = pro.GetValue(obj) as IList;

                                        if (instanceList != null)
                                            foreach (var item in instanceList)
                                                InitBaseClientsDependencies(item as BaseModel, mypath);
                                    }
                                }
                            }
                        }
                    }
                    else if (pro.PropertyType.IsSubclassOf(typeof(BaseModel)))
                    {
                        if (pro.PropertyType == typeof(UniImage))
                            continue;

                        if (!fatherPath.Contains(pro.PropertyType))
                        {
                            List<Type> mypath = new List<Type>();
                            foreach (var type in fatherPath)
                                mypath.Add(type);
                            mypath.Add(pro.PropertyType);

                            if (pro.GetValue(obj) is BaseModel value)
                                InitBaseClientsDependencies(value, mypath);
                        }
                    }
                }
            }
        }
        #endregion
    }
}