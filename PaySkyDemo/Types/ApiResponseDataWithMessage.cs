namespace PaySkyDemo.Types
{
    public class ApiResponseDataWithMessage
    {
        public required string Message { get; set; }
        public required object Data { get; set; }
        public  int StatusCode { get; set; } = 200;
    }
}
