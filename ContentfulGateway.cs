using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using ContentfulNetAPI.Types;
using Microsoft.CSharp.RuntimeBinder;
using RestSharp;

namespace ContentfulNetAPI
{
    public static class ContentfulGatewayFactory
    {
        //force allow only 1 gateway per apiKey - implement global rate limiting
        private static readonly Dictionary<string,ContentfulGateway> _gatewayByApiKey = new Dictionary<string, ContentfulGateway>();

        public static async Task<ContentfulGateway> GetGateway(string apiKey, string spaceId)
        {
            if (_gatewayByApiKey.ContainsKey(apiKey) == false)
            {
                var gateway = new ContentfulGateway(apiKey, spaceId);
                await gateway.Connect();
                _gatewayByApiKey[apiKey] = gateway;
                return gateway;
            }
            else
            {
                return _gatewayByApiKey[apiKey];
            }
        }
    }

    public class ContentfulGateway
    {
        private readonly string _apiKey;
        private readonly string _spaceId;
        private bool _connected = false;
        private RestClient _client;

        internal ContentfulGateway(string apiKey, string spaceId)
        {
            _apiKey = apiKey;
            _spaceId = spaceId;
        }

        public async Task<object> Connect()
        {
            if (_connected == false)
            {
                _client = new RestClient("https://cdn.contentful.com");
                var request = BuildRequest($"spaces/{_spaceId}");
                var response = await _client.ExecuteTaskAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _connected = true;
                }
                else
                {
                    throw new Exception("Connect failed, check apikey and space id: " + response.ErrorMessage);
                }
                var jsonResponseObject = Json.Decode(response.Content);
                return jsonResponseObject;
            }

            return null;
        }

        private RestRequest BuildRequest(string urlBase)
        {
            var request = new RestRequest(urlBase);
            request.AddParameter("access_token", _apiKey);
            return request;
        }

        public async Task<List<ContentType>> GetAllContentTypes()
        {
            var request = BuildRequest($"spaces/{_spaceId}/content_types");
            var response = await _client.ExecuteTaskAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic jsonObject = Json.Decode(response.Content);
                dynamic[] items = jsonObject.items;
                var contentTypes = new List<ContentType>();
                foreach (var item in items)
                {
                    var contentType = new ContentType()
                    {
                        id = item.sys.id,
                        name = item.name,
                        description = item.description,
                        displayField = item.displayField,
                        fields = new List<ContentTypeField>()
                    };
                    dynamic[] fieldsObject = item.fields;
                    foreach (var fieldObject in fieldsObject)
                    {
                        var field = new ContentTypeField()
                        {
                            id = fieldObject.id,
                            name = fieldObject.name,
                            type = fieldObject.type,
                            localized = fieldObject.localized,
                            required = fieldObject.required,
                            disabled = fieldObject.disabled,
                            linkType = fieldObject.linkType
                        };
                        contentType.fields.Add(field);
                    }

                    contentTypes.Add(contentType);
                }


                return contentTypes;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public Task<List<Dictionary<string, object>>> GetObjectsFieldValuesByContentType(ContentTypeAlias alias)
        {
            return GetObjectsFieldValuesByContentType(alias.id);
        }

        public async Task<List<Dictionary<string, object>>> GetObjectsFieldValuesByContentType(string contentTypeId)
        {
            var request = BuildRequest($"spaces/{_spaceId}/entries");
            request.AddParameter("content_type", contentTypeId);
            var response = await _client.ExecuteTaskAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseItems = new List<Dictionary<string, object>>();
                var jsonResponse = Json.Decode(response.Content);
                
                foreach (dynamic jsonItem in jsonResponse.items)
                {
                    DynamicJsonObject itemFields = jsonItem.fields;
                    var fieldNames = itemFields.GetDynamicMemberNames();
                    var itemFieldValues = new Dictionary<string, object>();
                    foreach (string fieldName in fieldNames)
                    {
                        var propertyValue = itemFields.GetPropertyValue(fieldName);
                        if (propertyValue.GetType() != typeof (DynamicJsonObject))
                        {
                            itemFieldValues[fieldName] = propertyValue;
                        }
                        else
                        {
                            //property is a reference of another object, instead stuff the id of the object
                            dynamic dynamicPropValue = propertyValue as DynamicJsonObject;
                            itemFieldValues[fieldName] = dynamicPropValue?.sys.id;
                        }
                        
                    }
                    itemFieldValues["id"] = jsonItem.sys.Id;
                    responseItems.Add(itemFieldValues);
                }

                return responseItems;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }

        public Task<List<T>> GetObjectsByContentType<T>(ContentTypeAlias alias) where T : class, new()
        {
            return GetObjectsByContentType<T>(alias.id);
        }

        public async Task<List<T>> GetObjectsByContentType<T>(string contentTypeId) where T : class, new()
        {
            var itemsDicts = await GetObjectsFieldValuesByContentType(contentTypeId);
            return itemsDicts.Select(itemsDict => itemsDict.ToObject<T>()).ToList();
        }

        public async Task<string> GetObjectsJsonByContentType(string contentTypeId)
        {
            var request = BuildRequest($"spaces/{_spaceId}/entries");
            request.AddParameter("content_type", contentTypeId);
            var response = await _client.ExecuteTaskAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content;
            }
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }
    }
}
