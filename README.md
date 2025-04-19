# 📞 PhoneBook API

A secure, role-based contact management REST API built with **.NET 8** and **MongoDB**, designed for managing private and global contacts with search and pagination support.

---

## ✨ Features

- 🔐 **JWT Authentication** with role-based access (User / Admin)
- 📇 **Contact Management**:
  - Add, edit, delete contacts
  - Global contacts (Admins only)
  - Private contacts (per user)
- 🔎 **Search Contacts** by first name, last name, email, or phone number
- 📄 **Pagination** support (10 contacts per page)
- 🧪 **Extensive test suite** with xUnit + FluentAssertions
- 🐳 **Docker-ready** deployment setup using `.env`

---

## 🚀 Getting Started (Docker)

### 📁 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/)
- (Optional) [MongoDB Compass](https://www.mongodb.com/products/compass) for inspecting the database

---

### 🛠 Environment Setup

1. Copy the example environment file:

```
cp .env.example .env
```

2. Edit `.env` to add your JWT secret and/or change database name and/or admin credentials.

---

### 🐳 Run the Project with Docker

```
docker compose --env-file .env up
```

- The API will be available at: http://localhost:5001
- 4. (Optional) Mongo Express (GUI for DB) at:  http://localhost:8081
---

## 🔐 Authentication

All endpoints require a valid JWT token.

1. **Register**:  
`POST /auth/register`  
Returns a JWT on successful registration.

2. **Login**:  
`POST /auth/login`  
Returns a JWT if credentials are valid.

3. **Use the token** in your requests:
Authorization: Bearer <your_token>

---

## 📚 API Endpoints Overview

### 🧾 Auth

| Method | Route            | Description         |
|--------|------------------|---------------------|
| POST   | `/auth/register` | Register a new user |
| POST   | `/auth/login`    | Login and get token |

---

### 📇 Contacts

| Method | Route                          | Description                              |
|--------|--------------------------------|------------------------------------------|
| GET    | `/phonebook?page=1`           | Get all visible contacts (paginated)     |
| GET    | `/phonebook/search?query=x`   | Search by name/email/phone               |
| POST   | `/phonebook/createContact`    | Create new contact                       |
| PUT    | `/phonebook/{id}`             | Update a contact (if user has access)    |
| DELETE | `/phonebook/{id}`             | Delete a contact (if user has access)    |

---

### 📝 Example Requests

#### Register a New User
```http
POST /auth/register
Content-Type: application/json

{
  "username": "john_doe",  // string, required
  "email": "john.doe@example.com",  // string, required
  "password": "securepassword123"  // string, required
}
```

#### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",  // string, required
  "password": "securepassword123"  // string, required
}
```

#### Create a New Contact
```http
POST /phonebook/createContact
Authorization: Bearer <your_token>
Content-Type: application/json

{
  "firstName": "Jane",  // string, required, only letters
  "lastName": "Doe",  // string, optional, only letters
  "phoneNumber": "1234567890",  // string, required, only digits
  "address": "123 Main St",  // string, optional
  "email": "jane.doe@example.com",  // string, optional, valid email format
  "isGlobal": false  // boolean, optional, default: false, ignored if token not aligned with admin user
}
```

#### Search Contacts
```http
GET /phonebook/search
Authorization: Bearer <your_token>
Content-Type: application/json
```

**Query Parameters**:
- `query` (string, required): The search term to filter contacts by first name, last name, email, or phone number.
- `searchField` (string, optional): The specific field to search in. Possible values are `FirstName`, `LastName`, `PhoneNumber`, `Email`, or leave empty for all fields (default: all).
- `page` (int, optional): The page number for pagination (default: 1).

**Example Request**:
```http
GET /phonebook/search?query=Jane&searchField=FirstName&page=1
Authorization: Bearer <your_token>
```


#### Update a Contact
```http
PUT /phonebook/{id}
Authorization: Bearer <your_token>
Content-Type: application/json

{
  "firstName": "Jane",  // string, optional, only letters
  "lastName": "Smith",  // string, optional, only letters
  "phoneNumber": "9876543210",  // string, optional, only digits
  "address": "456 Elm St",  // string, optional
  "email": "jane.smith@example.com"  // string, optional, valid email format
}
```

#### Delete a Contact
```http
DELETE /phonebook/{id}
Authorization: Bearer <your_token>
```

---

## 🔐 Authentication

All endpoints require a valid JWT token, except for `/auth/register` and `/auth/login`. Include the token in the `Authorization` header as follows:

```
Authorization: Bearer <your_token>
```

## 🧪 Running Tests

```
dotnet test
```

- Uses a separate `PhoneBookDbTest` database
- Includes:
- Auth tests
- Search and pagination
- Controller access rules
- JWT validation tests

---

## 🛠 Environment Variables

Configured in `appsettings.Development.json`, `appsettings.Test.json`, or via Docker:

```json
"Jwt": {
"SecretKey": "<base64_32byte_key>",
"Issuer": "PhoneBookApi",
"Audience": "PhoneBookClient"
},
"MongoDbSettings": {
"ConnectionString": "mongodb://mongo:27017",
"DatabaseName": "PhoneBookDb"
}
```

---

## 👤 Default Admin User

When Starting application, admin user will be created, using the ADMIN_USERNAME, ADMIN_PASSWORD, ADMIN_EMAIL variables supplied in the .env file, or via the appsetting.Development.json file.
Use those credentials to authenticate in login method, and perform admin methofs.
