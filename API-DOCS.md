# Gerra ToDo List — API Documentation

## Base URL

```
http://localhost:<port>/api
```

## Authentication

Semua endpoint (kecuali yang ditandai **Public**) wajib menyertakan JWT token di header:

```
Authorization: Bearer <token>
```

Token didapat dari `POST /api/auth/login`, berlaku **7 hari**.

### Response Error Auth

| Status | Body |
|--------|------|
| 401 | `{"success": false, "message": "Token Required"}` |
| 401 | `{"success": false, "message": "Token Expired"}` |
| 401 | `{"success": false, "message": "Invalid Token"}` |

---

## Response Envelope

Semua response mengikuti format:

```json
{
  "success": true,
  "message": "string",
  "data": { ... },
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "total": 50,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

- `pagination` hanya ada di endpoint yang mengembalikan list (paginated).
- `data` bisa berupa object tunggal atau array.

---

## Enums

| Enum | Values |
|------|--------|
| `TodoStatus` | `0` = NotStarted, `1` = InProgress, `2` = Completed |
| `SubTaskStatus` | `0` = NotStarted, `1` = InProgress, `2` = Completed |
| `GoalStatus` | `0` = NotStarted, `1` = InProgress, `2` = Completed |
| `HabitStatus` | `0` = NotStarted, `1` = InProgress, `2` = Completed |
| `TypeGoal` | `"personal"`, `"professional"` |
| `EntityType` | `0` = Task, `1` = SubTask, `2` = Goal, `3` = Habit |

---

## 1. Auth

### `POST /api/auth/login` — **Public**

Login dengan email/username + password.

**Request Body:**
```json
{
  "identity": "string (email atau username)",
  "password": "string"
}
```

**Response `200`:**
```json
{
  "token": "eyJhbGciOi...",
  "code": 200,
  "message": "login successfully",
  "user": {
    "id": 1,
    "name": "string",
    "email": "string",
    "username": "string",
    "isActive": true,
    "role": { "id": 1, "name": "string" },
    "createdAt": "datetime",
    "updatedAt": "datetime",
    "deletedAt": null
  },
  "expiredAt": "datetime"
}
```

**Response `404`:**
```json
{
  "code": 404,
  "message": "username or password is wrong"
}
```

---

### `GET /api/auth/me`

Mendapatkan data user yang sedang login.

**Response `200`:** `APIResponse<User>` (sama seperti `user` di login response)

---

## 2. User

### `POST /api/user` — **Public**

Registrasi user baru.

**Request Body:**
```json
{
  "username": "string*",
  "name": "string*",
  "email": "string*",
  "password": "string*",
  "isActive": true,
  "roleId": 1
}
```

**Response `201`:** `User`

---

### `GET /api/user`

List user (paginated).

**Query Params:**

| Param | Default | Keterangan |
|-------|---------|------------|
| `page` | 1 | Halaman |
| `pageSize` | 10 | Item per halaman |
| `search` | — | Cari username/name/email |

**Response `200`:** `APIResponse<IEnumerable<User>>` + pagination

---

### `GET /api/user/{id}`

Detail user by ID.

**Response `200`:** `APIResponse<User>`

---

### `PATCH /api/user/{id}`

Update user.

**Request Body (semua opsional):**
```json
{
  "username": "string?",
  "name": "string?",
  "email": "string?",
  "password": "string?",
  "isActive": true,
  "roleId": 1
}
```

**Response `200`:** `APIResponse<User>`

---

### `DELETE /api/user/softDelete?id={id}`

Soft-delete user (set `DeletedAt`).

**Response `200`:** `APIResponse<User>`  
**Response `400`:** Gagal hapus

---

## 3. Task

### `GET /api/task`

List task (paginated + filter).

**Query Params:**

| Param | Default | Keterangan |
|-------|---------|------------|
| `page` | 1 | Halaman |
| `pageSize` | 10 | Item per halaman |
| `search` | — | Cari judul/deskripsi |
| `status` | — | `0`/`1`/`2` |
| `priority` | — | `"low"` / `"medium"` / `"high"` |
| `goalId` | — | Filter by goal |

**Response `200`:** `APIResponse<IEnumerable<Task>>` + pagination

---

### `GET /api/task/{id}`

Detail task (include SubTasks, Goal, TaskCategory).

**Response `200`:** `APIResponse<Task>`

---

### `POST /api/task`

Buat task baru.

**Request Body:**
```json
{
  "title": "string*",
  "description": "string?",
  "status": 0,
  "priority": "medium",
  "typeHabbit": "string?",
  "goalId": null,
  "taskCategoryId": null,
  "subTasks": [
    { "title": "string*", "status": 0 }
  ]
}
```

**Response `201`:** `Task`

---

### `PATCH /api/task/{id}`

Update task.

**Request Body (semua opsional):**
```json
{
  "title": "string?",
  "description": "string?",
  "status": 1,
  "priority": "string?",
  "typeHabbit": "string?",
  "goalId": null,
  "taskCategoryId": null
}
```

**Response `200`:** `APIResponse<Task>`

---

### `DELETE /api/task/{id}`

Soft-delete task.

**Response `200`:** `APIResponse<Task>`

---

### `DELETE /api/task/{id}/hard`

Hard-delete task (permanen).

**Response `200`:** `APIResponse<Task>`

---

## 4. SubTask

### `POST /api/task/{taskId}/subtask`

Tambah subtask ke task.

**Request Body:**
```json
{
  "title": "string*",
  "status": 0
}
```

**Response `201`:** `SubTask`

---

### `PATCH /api/task/subtask/{subTaskId}`

Update subtask.

**Request Body (semua opsional):**
```json
{
  "title": "string?",
  "status": 1
}
```

**Response `200`:** `APIResponse<SubTask>`

---

### `DELETE /api/task/subtask/{subTaskId}`

Soft-delete subtask.

**Response `200`:** `APIResponse<SubTask>`

---

## 5. Task Category

### `GET /api/task-category`

List semua kategori task user login.

**Response `200`:** `APIResponse<IEnumerable<TaskCategory>>`

---

### `GET /api/task-category/{id}`

Detail kategori.

**Response `200`:** `APIResponse<TaskCategory>`

---

### `POST /api/task-category`

Buat kategori baru.

**Request Body:**
```json
{
  "name": "string*"
}
```

**Response `201`:** `TaskCategory`

---

### `PATCH /api/task-category/{id}`

Update kategori.

**Request Body:**
```json
{
  "name": "string?"
}
```

**Response `200`:** `APIResponse<TaskCategory>`

---

### `DELETE /api/task-category/{id}`

Hard-delete kategori (permanen, tidak ada soft-delete).

**Response `200`:** `APIResponse<TaskCategory>`

---

## 6. Goal

### `GET /api/goal`

List goal (paginated + filter).

**Query Params:**

| Param | Default | Keterangan |
|-------|---------|------------|
| `page` | 1 | Halaman |
| `pageSize` | 10 | Item per halaman |
| `search` | — | Cari judul |
| `status` | — | `0`/`1`/`2` |
| `typeGoal` | — | `"personal"` / `"professional"` |

**Response `200`:** `APIResponse<IEnumerable<Goal>>` + pagination

---

### `GET /api/goal/{id}`

Detail goal (include Tasks yang belum dihapus).

**Response `200`:** `APIResponse<Goal>`

---

### `POST /api/goal`

Buat goal baru.

**Request Body:**
```json
{
  "title": "string*",
  "typeGoal": "personal",
  "status": 0
}
```

**Response `201`:** `Goal`

---

### `PATCH /api/goal/{id}`

Update goal.

**Request Body (semua opsional):**
```json
{
  "title": "string?",
  "typeGoal": "string?",
  "status": 1
}
```

**Response `200`:** `APIResponse<Goal>`

---

### `DELETE /api/goal/{id}`

Soft-delete goal.

**Response `200`:** `APIResponse<Goal>`

---

### `DELETE /api/goal/{id}/hard`

Hard-delete goal (permanen).

**Response `200`:** `APIResponse<Goal>`

---

## 7. Habit

### `GET /api/habit`

List habit (paginated + filter).

**Query Params:**

| Param | Default | Keterangan |
|-------|---------|------------|
| `page` | 1 | Halaman |
| `pageSize` | 10 | Item per halaman |
| `search` | — | Cari judul |
| `status` | — | `0`/`1`/`2` |
| `period` | — | `"daily"` / `"weekly"` / `"monthly"` |

**Response `200`:** `APIResponse<IEnumerable<Habit>>` + pagination

---

### `GET /api/habit/{id}`

Detail habit.

**Response `200`:** `APIResponse<Habit>`

---

### `POST /api/habit`

Buat habit baru.

**Request Body:**
```json
{
  "title": "string*",
  "period": "daily",
  "targetMinutes": 30,
  "status": 0
}
```

**Response `201`:** `Habit`

---

### `PATCH /api/habit/{id}`

Update habit.

**Request Body (semua opsional):**
```json
{
  "title": "string?",
  "period": "string?",
  "targetMinutes": 60,
  "currentMinutes": 15,
  "status": 1
}
```

**Response `200`:** `APIResponse<Habit>`

---

### `DELETE /api/habit/{id}`

Soft-delete habit.

**Response `200`:** `APIResponse<Habit>`

---

### `DELETE /api/habit/{id}/hard`

Hard-delete habit (permanen).

**Response `200`:** `APIResponse<Habit>`

---

## 8. Activity Log

### `GET /api/activity-log`

List log aktivitas (paginated + filter). **Read-only** — log dibuat otomatis oleh sistem.

**Query Params:**

| Param | Default | Keterangan |
|-------|---------|------------|
| `page` | 1 | Halaman |
| `pageSize` | 20 | Item per halaman |
| `action` | — | `"create"` / `"update"` / `"delete"` |
| `entityType` | — | `0`=Task, `1`=SubTask, `2`=Goal, `3`=Habit |

**Response `200`:** `APIResponse<IEnumerable<ActivityLog>>` + pagination

---

## Ringkasan Endpoint

| Controller | Endpoint | Method | Auth |
|------------|----------|--------|------|
| Auth | `/api/auth/login` | POST | Public |
| Auth | `/api/auth/me` | GET | JWT |
| User | `/api/user` | POST | Public |
| User | `/api/user` | GET | JWT |
| User | `/api/user/{id}` | GET | JWT |
| User | `/api/user/{id}` | PATCH | JWT |
| User | `/api/user/softDelete?id={id}` | DELETE | JWT |
| Task | `/api/task` | GET | JWT |
| Task | `/api/task/{id}` | GET | JWT |
| Task | `/api/task` | POST | JWT |
| Task | `/api/task/{id}` | PATCH | JWT |
| Task | `/api/task/{id}` | DELETE | JWT |
| Task | `/api/task/{id}/hard` | DELETE | JWT |
| SubTask | `/api/task/{taskId}/subtask` | POST | JWT |
| SubTask | `/api/task/subtask/{subTaskId}` | PATCH | JWT |
| SubTask | `/api/task/subtask/{subTaskId}` | DELETE | JWT |
| TaskCategory | `/api/task-category` | GET | JWT |
| TaskCategory | `/api/task-category/{id}` | GET | JWT |
| TaskCategory | `/api/task-category` | POST | JWT |
| TaskCategory | `/api/task-category/{id}` | PATCH | JWT |
| TaskCategory | `/api/task-category/{id}` | DELETE | JWT |
| Goal | `/api/goal` | GET | JWT |
| Goal | `/api/goal/{id}` | GET | JWT |
| Goal | `/api/goal` | POST | JWT |
| Goal | `/api/goal/{id}` | PATCH | JWT |
| Goal | `/api/goal/{id}` | DELETE | JWT |
| Goal | `/api/goal/{id}/hard` | DELETE | JWT |
| Habit | `/api/habit` | GET | JWT |
| Habit | `/api/habit/{id}` | GET | JWT |
| Habit | `/api/habit` | POST | JWT |
| Habit | `/api/habit/{id}` | PATCH | JWT |
| Habit | `/api/habit/{id}` | DELETE | JWT |
| Habit | `/api/habit/{id}/hard` | DELETE | JWT |
| ActivityLog | `/api/activity-log` | GET | JWT |

**Total: 34 endpoint** (2 public, 32 protected)
