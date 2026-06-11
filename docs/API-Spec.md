# API Specification

# DevVault

Version: 1.0

---

# Purpose

This document defines the REST API contract for DevVault.

The API enables:

* Authentication
* User Management
* Project Management
* Secret Management
* Audit Logging
* Dashboard Reporting

All endpoints return JSON responses.

---

# API Standards

## Base URL

```text
/api/v1
```

---

## Authentication

Protected endpoints require:

```http
Authorization: Bearer <jwt-token>
```

---

## Standard Success Response

```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": {}
}
```

---

## Standard Error Response

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": []
}
```

---

# Authentication Module

## Login

### Endpoint

```http
POST /api/v1/auth/login
```

### Request

```json
{
  "email": "admin@devvault.com",
  "password": "Password123!"
}
```

### Response

```json
{
  "success": true,
  "data": {
    "accessToken": "jwt-token",
    "expiresAt": "2026-01-01T00:00:00Z"
  }
}
```

### Authorization

Public

---

## Logout

### Endpoint

```http
POST /api/v1/auth/logout
```

### Authorization

Authenticated User

---

## Current User

### Endpoint

```http
GET /api/v1/auth/me
```

### Response

```json
{
  "id": "uuid",
  "email": "admin@devvault.com",
  "roles": [
    "Administrator"
  ]
}
```

---

# User Management Module

## Get Users

### Endpoint

```http
GET /api/v1/users
```

### Authorization

Administrator

---

## Get User By Id

### Endpoint

```http
GET /api/v1/users/{id}
```

---

## Create User

### Endpoint

```http
POST /api/v1/users
```

### Request

```json
{
  "email": "developer@devvault.com",
  "firstName": "John",
  "lastName": "Doe",
  "roleId": "uuid"
}
```

### Response

```json
{
  "success": true,
  "message": "User created successfully."
}
```

---

## Update User

### Endpoint

```http
PUT /api/v1/users/{id}
```

### Request

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "isActive": true
}
```

---

## Deactivate User

### Endpoint

```http
PATCH /api/v1/users/{id}/deactivate
```

---

## Assign Role

### Endpoint

```http
POST /api/v1/users/{id}/roles
```

### Request

```json
{
  "roleId": "uuid"
}
```

---

# Role Module

## Get Roles

### Endpoint

```http
GET /api/v1/roles
```

### Authorization

Administrator

---

## Get Role By Id

### Endpoint

```http
GET /api/v1/roles/{id}
```

---

# Project Management Module

## Get Projects

### Endpoint

```http
GET /api/v1/projects
```

### Authorization

Authenticated User

---

## Get Project By Id

### Endpoint

```http
GET /api/v1/projects/{id}
```

---

## Create Project

### Endpoint

```http
POST /api/v1/projects
```

### Authorization

Administrator

### Request

```json
{
  "name": "Ecommerce API",
  "description": "Main backend application"
}
```

---

## Update Project

### Endpoint

```http
PUT /api/v1/projects/{id}
```

---

## Archive Project

### Endpoint

```http
PATCH /api/v1/projects/{id}/archive
```

---

## Assign User To Project

### Endpoint

```http
POST /api/v1/projects/{id}/members
```

### Request

```json
{
  "userId": "uuid"
}
```

---

## Remove User From Project

### Endpoint

```http
DELETE /api/v1/projects/{id}/members/{userId}
```

---

## Get Project Members

### Endpoint

```http
GET /api/v1/projects/{id}/members
```

---

# Secret Management Module

## Get Secrets

### Endpoint

```http
GET /api/v1/projects/{projectId}/secrets
```

### Authorization

Project Member

---

## Get Secret By Id

### Endpoint

```http
GET /api/v1/secrets/{id}
```

### Response

```json
{
  "id": "uuid",
  "name": "DATABASE_CONNECTION",
  "value": "decrypted-secret",
  "projectId": "uuid"
}
```

---

## Create Secret

### Endpoint

```http
POST /api/v1/projects/{projectId}/secrets
```

### Request

```json
{
  "name": "DATABASE_CONNECTION",
  "value": "Server=..."
}
```

---

## Update Secret

### Endpoint

```http
PUT /api/v1/secrets/{id}
```

### Request

```json
{
  "value": "Updated Connection String"
}
```

---

## Delete Secret

### Endpoint

```http
DELETE /api/v1/secrets/{id}
```

### Authorization

Administrator

---

## Search Secrets

### Endpoint

```http
GET /api/v1/projects/{projectId}/secrets/search?name=DATABASE
```

---

# Audit Log Module

## Get Audit Logs

### Endpoint

```http
GET /api/v1/audit-logs
```

### Authorization

Administrator, Auditor

---

## Get Audit Log By Id

### Endpoint

```http
GET /api/v1/audit-logs/{id}
```

---

## Filter Audit Logs

### Endpoint

```http
GET /api/v1/audit-logs/filter
```

### Query Parameters

```http
?action=CreateSecret
&userId=uuid
&from=2026-01-01
&to=2026-01-31
```

---

# Dashboard Module

## Get Dashboard Summary

### Endpoint

```http
GET /api/v1/dashboard/summary
```

### Response

```json
{
  "totalUsers": 10,
  "totalProjects": 5,
  "totalSecrets": 120,
  "recentActivities": 15
}
```

---

## Get Recent Activities

### Endpoint

```http
GET /api/v1/dashboard/recent-activities
```

---

# Health Check Module

## Health Check

### Endpoint

```http
GET /api/v1/health
```

### Response

```json
{
  "status": "Healthy"
}
```

### Authorization

Public

---

# Authorization Matrix

| Endpoint        | Admin | Developer | Auditor |
| --------------- | ----- | --------- | ------- |
| Login           | ✅     | ✅         | ✅       |
| Users           | ✅     | ❌         | ❌       |
| Roles           | ✅     | ❌         | ❌       |
| Projects        | ✅     | ✅         | ❌       |
| Project Members | ✅     | ❌         | ❌       |
| Secrets         | ✅     | ✅         | ❌       |
| Delete Secret   | ✅     | ❌         | ❌       |
| Audit Logs      | ✅     | ❌         | ✅       |
| Dashboard       | ✅     | ❌         | ❌       |

---

# HTTP Status Codes

| Code | Meaning               |
| ---- | --------------------- |
| 200  | Success               |
| 201  | Resource Created      |
| 204  | No Content            |
| 400  | Bad Request           |
| 401  | Unauthorized          |
| 403  | Forbidden             |
| 404  | Not Found             |
| 409  | Conflict              |
| 500  | Internal Server Error |

---

# API Versioning Strategy

Current Version:

```text
v1
```

Base Route:

```http
/api/v1
```

Future Versions:

```http
/api/v2
/api/v3
```

Versioning ensures backward compatibility when introducing new features.

---

# MVP API Coverage

The MVP API includes:

* Authentication
* User Management
* Role Management
* Project Management
* Project Membership
* Secret Management
* Audit Logging
* Dashboard Reporting
* Health Checks

These endpoints provide complete support for the DevVault MVP and align with the requirements, use cases, ERD, and architecture documents.
