using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Objects;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Digifinex.Net
{
    internal class DigifinexAuthenticationProvider : AuthenticationProvider<ApiCredentials>
    {
        public string ApiKey => _credentials.Key!.GetString();
        public string SecretKey => _credentials.Secret!.GetString();

        public DigifinexAuthenticationProvider(ApiCredentials credentials) : base(credentials)
        {
        }

        public override void AuthenticateRequest(
            RestApiClient apiClient, 
            Uri uri, 
            HttpMethod method, 
            IDictionary<string, object> uriParameters, 
            IDictionary<string, object> bodyParameters, 
            Dictionary<string, string> headers, 
            bool auth, 
            ArrayParametersSerialization arraySerialization, 
            HttpMethodParameterPosition parameterPosition, 
            RequestBodyFormat requestBodyFormat)
        {
            if (!auth)
            {
                return;
            }

            // Set required headers (not Content-Type - that's handled by the framework)
            headers["ACCESS-KEY"] = ApiKey;
            
            var timestamp = DateTimeConverter.ConvertToSeconds(GetTimestamp(apiClient)).Value;
            headers["ACCESS-TIMESTAMP"] = timestamp.ToString();
            
            // Create signature string from all parameters
            var allParameters = new Dictionary<string, object>();
            
            // Add URI parameters
            foreach (var param in uriParameters)
            {
                allParameters[param.Key] = param.Value;
            }
            
            // Add body parameters
            foreach (var param in bodyParameters)
            {
                allParameters[param.Key] = param.Value;
            }
            
            // Sort all parameters by key (ASCII order)
            var sortedParams = allParameters
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .Select(p => $"{p.Key}={FormatValue(p.Value)}");
            
            var paramString = string.Join("&", sortedParams);
            
            // Generate signature using HMAC SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var signBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(paramString));
                var signature = BitConverter.ToString(signBytes).Replace("-", "").ToLower();
                headers["ACCESS-SIGN"] = signature;
            }
        }

        private string FormatValue(object value)
        {
            // Format the value for signature (no URL encoding)
            return value switch
            {
                null => "",
                decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
                double doubleValue => doubleValue.ToString(CultureInfo.InvariantCulture),
                float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
                bool boolValue => boolValue.ToString().ToLower(),
                _ => value.ToString() ?? ""
            };
        }

        public string CreateWebsocketSignature(long timestamp)
        {
            var message = timestamp.ToString();
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToBase64String(hash);
            }
        }
    }
}