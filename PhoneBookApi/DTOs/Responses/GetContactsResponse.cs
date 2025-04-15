using PhoneBookApi.Models;

namespace PhoneBookApi.DTOs.Responses
{
    public class GetContactsResponse : BaseResponse
    {
        public List<Contact> Contacts { get; set; } = null!;
        public int Page { get; set; }

    }
}