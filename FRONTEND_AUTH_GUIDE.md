# Підключення авторизації для фронтенду AnimeWiki

**Бекенд:** ASP.NET Core 8, `http://localhost:5080` (профіль `http` у `Properties/launchSettings.json`).  
**Базовий API:** `http://localhost:5080/api/auth`  
**Формат запитів:** `Content-Type: application/json`. Відповіді мають camelCase.

## API-контракт

| Метод | URL | Потрібний токен | Що робить |
|---|---|---|---|
| POST | `/api/auth/register` | Ні | Створити користувача й одразу видати JWT |
| POST | `/api/auth/login` | Ні | Увійти за email **або** ім'ям й видати JWT |
| GET | `/api/auth/me` | Так | Отримати профіль користувача за токеном |

### 1. Реєстрація

`POST /api/auth/register`

```json
{
  "username": "animefan",
  "email": "fan@example.com",
  "password": "StrongPass123!"
}
```

Успіх: **201 Created**. `username` — 3–50 символів (літери, цифри, `_.-`); email коректний; пароль — 8–128 символів. Email та username повинні бути унікальними незалежно від регістру.

### 2. Вхід

`POST /api/auth/login`

```json
{
  "emailOrUsername": "fan@example.com",
  "password": "StrongPass123!"
}
```

Успіх: **200 OK**. У полі `emailOrUsername` можна вказати також `animefan`.

### Відповідь реєстрації та входу

```json
{
  "token": "<JWT_TOKEN>",
  "tokenType": "Bearer",
  "expiresAt": "2026-11-10T20:00:00Z",
  "user": {
    "id": 1,
    "username": "animefan",
    "email": "fan@example.com",
    "createdAt": "2026-11-10T18:00:00Z"
  }
}
```

`token` і дати наведено **лише як приклад форми**; сервер повертає реальні значення.

### 3. Профіль

`GET /api/auth/me` з заголовком:

```http
Authorization: Bearer <JWT_TOKEN>
```

Успіх: **200 OK** — об'єкт `user` без зовнішньої обгортки:

```json
{
  "id": 1,
  "username": "animefan",
  "email": "fan@example.com",
  "createdAt": "2026-11-10T18:00:00Z"
}
```

Без токена, з недійсним або простроченим токеном: **401 Unauthorized**.

## Готовий приклад для React / Next.js / Vite (fetch)

Можеш просто взяти готовий файл `Examples/frontend/authApi.js` із цього архіву (без залежностей). Або створи у фронтенді `authApi.js` за таким простим прикладом:

```javascript
const API_URL = "http://localhost:5080/api/auth";

export async function register(username, email, password) {
  const response = await fetch(`${API_URL}/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, email, password })
  });
  return readResponse(response);
}

export async function login(emailOrUsername, password) {
  const response = await fetch(`${API_URL}/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ emailOrUsername, password })
  });
  return readResponse(response);
}

export async function getMe(token) {
  const response = await fetch(`${API_URL}/me`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return readResponse(response);
}

async function readResponse(response) {
  const text = await response.text();
  const data = text ? JSON.parse(text) : {};
  if (!response.ok) {
    throw new Error(data.message || data.title || "Помилка запиту");
  }
  return data;
}
```

Приклад використання після входу:

```javascript
const result = await login("fan@example.com", "StrongPass123!");
const token = result.token;
const me = await getMe(token);
console.log(me.username);
```

Токен передавай у `Authorization: Bearer ...` для захищених API-запитів. Для безпечнішого продакшен-рішення бажано використовувати HTTPS, зберігати токен у пам'яті клієнта й продумати повторний вхід після його завершення. `localStorage`/`sessionStorage` несуть ризик викрадення токена при XSS. У цьому MVP немає refresh-токенів або серверного logout: на фронтенді вихід означає видалення локально збереженого токена, а вже виданий токен діє до `expiresAt`.

## Статуси помилок

- `400` — невалідні поля (ASP.NET Core повертає стандартний `ValidationProblemDetails`, з `errors`).
- `401` — неправильний логін/пароль або недійсний токен.
- `409` — email чи username вже використовується.
- `429` — перевищено ліміт запитів до реєстрації/входу (10 запитів на хвилину з одного IP).

## CORS

За замовчуванням у `appsettings.json` дозволені `localhost:3000`, `127.0.0.1:3000`, `localhost:5173`, `127.0.0.1:5173`, `localhost:5174`, `localhost:4200`. Для іншого домену/порту додай адресу до `Cors:AllowedOrigins`. Origin має збігатися **точно**, включно з протоколом.

## Swagger

Після `dotnet run` відкрий `http://localhost:5080/swagger`. Натисни `POST /api/auth/register` або `login`, скопіюй `token`, натисни `Authorize`, встав **лише токен**, потім виклич `GET /api/auth/me`.
