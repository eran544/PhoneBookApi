using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.DTOs.Responses;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using System.Security.Claims;

namespace PhoneBookApi.Controllers;

[ApiController]
[Route("phonebook")]
[Authorize]
public class PhoneBookController : ControllerBase
{
    private readonly ILogger<PhoneBookController> _logger;
    private readonly DataHandler _dataHandler;

    public PhoneBookController(ILogger<PhoneBookController> logger, DataHandler dataHandler)
    {
        _logger = logger;
        _dataHandler = dataHandler;
    }

    [HttpPost("createContact")]
    public async Task<ActionResult<CreateContactResponse>> CreateContact([FromBody] CreateContactRequest request)
    {
        try
        {
            var (userId, role) = JwtHandler.GetUserIdAndRole(User);
            if (userId == null)
            {
                return BadRequest(new CreateContactResponse
                {
                    ErrorMessage = "Invalid userId"
                });
            }

            var roleString = User.FindFirst(ClaimTypes.Role)?.Value;

            bool shouldSaveAsGlobal = request.IsGlobal && role == Role.Admin;

            var contactId = await _dataHandler.CreateContactAsync(request, shouldSaveAsGlobal ? null : userId);
            if (contactId != null)
            {
                return Created(string.Empty, new CreateContactResponse() { ContactId = contactId }); // add more data later if needed
            }

            return StatusCode(500, new CreateContactResponse
            {
                ErrorMessage = "Unexpected failure while saving contact"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in CreateContact");

            return StatusCode(500, new CreateContactResponse
            {
                Exception = ex,
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<GetContactsResponse>> GetContacts([FromQuery] int page = 1)
    {
        GetContactsResponse response = new()
        {
            Page = page,
        };
        try
        {
            var (userId, _) = JwtHandler.GetUserIdAndRole(User);
            if (userId == null)
            {
                return BadRequest(new GetContactsResponse() { ErrorMessage = "userId not found" });
            }
            var contacts = await _dataHandler.GetContactsAsync(userId.Value, page);
            response.Contacts = contacts;
            return Ok(response);
        }
        catch (ArgumentException ae)
        {
            response.ErrorMessage = ae.Message;
            response.Exception = ae;
            return BadRequest(response);
        }
        catch (InvalidOperationException ioe)
        {
            response.ErrorMessage = ioe.Message;
            response.Exception = ioe;
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            response.Exception = ex;
            return StatusCode(500, response);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateContactResponse>> UpdateContact(string id, [FromBody] UpdateContactRequest request)
    {
        var updateContactResponse = new UpdateContactResponse();
        if (!ObjectId.TryParse(id, out var contactId))
        {
            updateContactResponse.ErrorMessage = $"{id} is not a valid id";
            return BadRequest(updateContactResponse);
        }
        var (userId, role) = JwtHandler.GetUserIdAndRole(User);
        if (userId == null)
        {
            return BadRequest(new CreateContactResponse
            {
                ErrorMessage = "Invalid userId"
            });
        }
        updateContactResponse.ContactId = contactId;
        try
        {
            bool updated = await _dataHandler.UpdateContactAsync(request, userId.Value, role, contactId);
            if (updated)
            {
                return Ok(updateContactResponse);
            }
            updateContactResponse.ErrorMessage = "No Contact Found";
            return NotFound(updateContactResponse);
        }
        catch (UnauthorizedAccessException uae)
        {
            _logger.LogWarning(uae, $"Unauthorized update attempt from {userId}");
            updateContactResponse.ErrorMessage = uae.Message;
            return StatusCode(403, updateContactResponse);
        }
        catch (Exception ex)
        {
            updateContactResponse.ErrorMessage = ex.Message;
            updateContactResponse.Exception = ex;
            return StatusCode(500, updateContactResponse);
        }

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeleteContactResponse>> DeleteContact(string id)
    {
        var deleteContactResponse = new DeleteContactResponse();
        if (!ObjectId.TryParse(id, out var contactId))
        {
            deleteContactResponse.ErrorMessage = $"{id} is not a valid id";
            return BadRequest(deleteContactResponse);
        }
        var (userId, role) = JwtHandler.GetUserIdAndRole(User);
        if (userId == null)
        {
            return BadRequest(new CreateContactResponse
            {
                ErrorMessage = "Invalid userId"
            });
        }
        deleteContactResponse.ContactId = contactId;
        try
        {
            bool deleted = await _dataHandler.DeleteContactAsync(userId.Value, role, contactId);
            if (deleted)
            {
                return Ok(deleteContactResponse);
            }
            deleteContactResponse.ErrorMessage = "No Contact Found";
            return NotFound(deleteContactResponse);
        }
        catch (UnauthorizedAccessException uae)
        {
            _logger.LogWarning(uae, $"Unauthorized delete attempt from {userId}");
            deleteContactResponse.ErrorMessage = uae.Message;
            return StatusCode(403, deleteContactResponse);
        }
        catch (Exception ex)
        {
            deleteContactResponse.ErrorMessage = ex.Message;
            deleteContactResponse.Exception = ex;
            return StatusCode(500, deleteContactResponse);
        }
    }


}

