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
docker-compose up --build
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

yaml
Copy code

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

To test admin-only features, the following seeded user exists, and their details will be provided seperatly:

```json
{
"username": "Admin",
"email": "admin@admin.com",
"password": "PaswordWillBeProvided1"
}
```
