namespace PaySkyDemo.Types
{
    public class ApiResponseWithMessage
    {
        public required string Message { get; set; }
        public  int StatusCode { get; set; } = 200;
    }
}
