using MongoDB.Bson;

namespace PhoneBookApi.DTOs.Responses
{
    public class DeleteContactResponse : BaseResponse
    {
        public ObjectId? ContactId { get; set; }
    }
}
