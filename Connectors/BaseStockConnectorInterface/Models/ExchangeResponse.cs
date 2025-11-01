using Newtonsoft.Json;

namespace BaseStockConnectorInterface.Models
{
    /// <summary>
    /// Wrapper for exchange HTTP responses to store original response in case of error.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExchangeResponse<T>
    {
        [JsonProperty]
        public bool IsSuccess { get; private set; }

        [JsonProperty]
        public T? Model { get; private set; }

        [JsonProperty]
        public object? OriginalResponse { get; private set; }

        public ExchangeResponse()
        {
            
        }

        private ExchangeResponse(bool isSuccess, T? model, object? originalResponse)
        {
            IsSuccess = isSuccess;
            Model = model;
            OriginalResponse = originalResponse;
        }

        private ExchangeResponse(bool isSuccess, object? originalResponse)
        {
            IsSuccess = isSuccess;
            OriginalResponse = originalResponse;
        }

        public static ExchangeResponse<T> Success(T? model, object? originalResponse)
        {
            return new ExchangeResponse<T>(isSuccess: true, model, originalResponse);
        }

        public static ExchangeResponse<T> Failed(object? originalResponse)
        {
            return new ExchangeResponse<T>(isSuccess: false, originalResponse);
        }
    }
}
