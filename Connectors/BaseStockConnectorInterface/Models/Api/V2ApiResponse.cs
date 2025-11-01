namespace BaseStockConnectorInterface.Models.Api
{
    public class V2ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
