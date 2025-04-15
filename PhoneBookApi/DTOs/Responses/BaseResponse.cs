namespace PhoneBookApi.DTOs.Responses
{
    public class BaseResponse
    {
        public Exception? Exception { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
