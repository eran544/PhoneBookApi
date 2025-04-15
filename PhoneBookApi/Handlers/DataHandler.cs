using MongoDB.Bson;
using MongoDB.Driver;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.Models;
using System.Linq;

namespace PhoneBookApi.Handlers
{
    public class DataHandler(IMongoDatabase database, ILogger<DataHandler> logger)
    {
        private readonly IMongoDatabase _database = database;
        private readonly IMongoCollection<User> usersCollection = database.GetCollection<User>("Users");
        private readonly IMongoCollection<Contact> contactsCollection = database.GetCollection<Contact>("Contacts");
        private readonly ILogger<DataHandler> _logger = logger;

        public async Task<User?> RegisterUser(RegisterRequest request)
        {
            try
            {
                User? user = null;
                if ((await usersCollection.FindAsync(u => u.Username == request.Username || u.Email == request.Email)).Any())
                {
                    return null;
                }
                user = new User()
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow
                };
                await usersCollection.InsertOneAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user");
                return null;
            }
        }

        public bool TryGetUser(LoginRequest request, out User? user)
        {
            var username = request.Username;
            user = usersCollection
            .AsQueryable()
            .FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                bool PasswordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!PasswordMatch)
                {
                    user = null;
                    _logger.LogInformation("user {username} logged with incorrect password", username);
                }
            }

            return user != null;
        }

        public async Task<ObjectId?> CreateContactAsync(CreateContactRequest request, ObjectId? mongoUserId)
        {
            try
            {
                Contact contact = new Contact()
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Address = request.Address,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = mongoUserId
                };
                await contactsCollection.InsertOneAsync(contact);
                return contact.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when creating contact");
                return null;
            }

        }

        public async Task<List<Contact>> GetContactsAsync(ObjectId mongoUserId, int page)
        {
            if (page < 1)
            {
                throw new ArgumentException("Page number must be greater than or equal to 1.", nameof(page));
            }

            try
            {
                const int pageSize = 10; // Number of contacts per page
                int skip = (page - 1) * pageSize;

                var filter = Builders<Contact>.Filter.Or(
                    Builders<Contact>.Filter.Eq(c => c.UserId, mongoUserId),
                    Builders<Contact>.Filter.Eq(c => c.UserId, null)
                );

                var sort = Builders<Contact>.Sort.Ascending(c => c.FirstName);

                // Count total contacts matching the filter
                var totalContacts = await contactsCollection.CountDocumentsAsync(filter);

                if (skip >= totalContacts)
                {
                    throw new InvalidOperationException($"Page {page} exceeds the total number of available contacts.");
                }

                var contacts = await contactsCollection
                    .Find(filter)
                    .Sort(sort)
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();

                return contacts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving contacts");
                throw; // Re-throw the exception to propagate it
            }
        }

        private async Task<Contact?> GetAccessibleContactAsync(ObjectId contactId, ObjectId userId, Role role)
        {
            var filter = Builders<Contact>.Filter.Eq(c => c.Id, contactId) &
                         (Builders<Contact>.Filter.Eq(c => c.UserId, userId) |
                          Builders<Contact>.Filter.Eq(c => c.UserId, null));

            var contact = await contactsCollection.Find(filter).FirstOrDefaultAsync();

            if (contact?.UserId == null && role != Role.Admin)
            {
                throw new UnauthorizedAccessException($"User {userId} is not allowed to access global contact {contactId}");
            }

            return contact;
        }

        public async Task<bool> DeleteContactAsync(ObjectId mongoUserId, Role role, ObjectId contactId)
        {
            var contactToDelete = await GetAccessibleContactAsync(contactId, mongoUserId, role);
            if (contactToDelete == null)
            {
                return false;
            }
            var deleteResult = await contactsCollection.DeleteOneAsync(c => c.Id == contactToDelete.Id);
            return deleteResult.DeletedCount == 1;
        }

        public async Task<bool> UpdateContactAsync(UpdateContactRequest request, ObjectId mongoUserId, Role role, ObjectId contactId)
        {
            var contactToUpdate = await GetAccessibleContactAsync(contactId, mongoUserId, role);
            if (contactToUpdate == null)
            {
                return false;
            }
            var updatedContact = new Contact
            {
                Id = contactToUpdate.Id, // Preserve the immutable Id
                UserId = contactToUpdate.UserId, // Preserve the immutable UserId
                FirstName = request.FirstName ?? contactToUpdate.FirstName,
                LastName = request.LastName ?? contactToUpdate.LastName,
                PhoneNumber = request.PhoneNumber ?? contactToUpdate.PhoneNumber,
                Address = request.Address ?? contactToUpdate.Address,
                Email = request.Email ?? contactToUpdate.Email,
                CreatedAt = contactToUpdate.CreatedAt, // Preserve the original CreatedAt
                UpdatedAt = DateTime.UtcNow // Update the UpdatedAt field
            };

            // Perform the update
            var replaceResult = await contactsCollection.ReplaceOneAsync(c => c.Id == contactToUpdate.Id, updatedContact);
            return replaceResult.ModifiedCount == 1;
        }
    }
}
