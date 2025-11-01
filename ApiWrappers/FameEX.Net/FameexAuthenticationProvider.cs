using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace FameEX.Net
{
    internal class FameexAuthenticationProvider : AuthenticationProvider<ApiCredentials>
    {
        public string ApiKey => _credentials.Key!.GetString();
        public string SecretKey => _credentials.Secret!.GetString();

        public FameexAuthenticationProvider(ApiCredentials credentials) : base(credentials)
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

            // Set required headers
            headers["X-CH-APIKEY"] = ApiKey;

            var timestamp = DateTimeConverter.ConvertToMilliseconds(GetTimestamp(apiClient)).Value;
            headers["X-CH-TS"] = timestamp.ToString();

            // Build request path with query string
            var requestPath = uri.AbsolutePath;
            if (uriParameters.Any())
            {
                var queryString = string.Join("&",
                    uriParameters.Select(p => $"{p.Key}={FormatValue(p.Value)}"));
                requestPath = $"{requestPath}?{queryString}";
            }

            // Build signature string: timestamp + method + requestPath + body
            var signPayload = timestamp.ToString() + method.Method.ToUpper() + requestPath;

            // Add body for POST requests
            if (method == HttpMethod.Post && bodyParameters.Any())
            {
                var bodyJson = JsonConvert.SerializeObject(bodyParameters,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.None
                    });
                signPayload += bodyJson;
            }

            // Generate signature using HMAC SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var signBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signPayload));
                var signature = BitConverter.ToString(signBytes).Replace("-", "").ToLower();
                headers["X-CH-SIGN"] = signature;
            }
        }

        private string FormatValue(object value)
        {
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
                // FameEX требует hex-строку!
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}