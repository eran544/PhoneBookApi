using MongoDB.Bson;

namespace PhoneBookApi.DTOs.Responses
{
    public class CreateContactResponse : BaseResponse
    {
        public ObjectId? ContactId { get; set; }
    }
}
