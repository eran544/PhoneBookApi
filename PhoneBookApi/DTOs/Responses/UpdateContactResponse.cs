using MongoDB.Bson;

namespace PhoneBookApi.DTOs.Responses
{
    public class UpdateContactResponse : BaseResponse
    {
        public ObjectId? ContactId { get; set; }
    }
}
