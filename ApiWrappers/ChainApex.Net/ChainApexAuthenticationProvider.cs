using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using System.Security.Cryptography;
using System.Text;

namespace ChainApex.Net
{
    internal class ChainApexAuthenticationProvider : AuthenticationProvider<ApiCredentials>
    {
        public string ApiKey => _credentials.Key!.GetString();
        public string SecretKey => _credentials.Secret!.GetString();

        public ChainApexAuthenticationProvider(ApiCredentials credentials) : base(credentials)
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
            RequestBodyFormat bodyFormat)
        {
            if (!auth)
                return;

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var requestPath = uri.AbsolutePath;
            var requestMethod = method.Method.ToUpper();

            // Build signature string: timestamp + method + path + body
            var signatureString = timestamp + requestMethod + requestPath;
            
            // Add body if present
            if (bodyParameters != null && bodyParameters.Any())
            {
                var body = string.Join("", bodyParameters.Select(kv => $"{kv.Key}={kv.Value}"));
                signatureString += body;
            }

            // Generate HMAC-SHA256 signature
            var signature = SignHMACSHA256(signatureString, SignOutputType.Hex);

            headers["X-CH-APIKEY"] = ApiKey;
            headers["X-CH-TS"] = timestamp;
            headers["X-CH-SIGN"] = signature;
        }

        public string GenerateSignature(string data)
        {
            return SignHMACSHA256(data, SignOutputType.Hex);
        }
    }
}

