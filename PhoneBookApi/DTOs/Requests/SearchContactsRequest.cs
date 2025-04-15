namespace PhoneBookApi.DTOs.Requests
{
    public class SearchContactsRequest
    {
        public required string Query { get; set; }
        public string? SearchField { get; set; } = null; // Optional (default = all)
        public int Page { get; set; } = 1;
    }
}
