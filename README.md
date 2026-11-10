# AnimeWiki.Api — бекенд із реєстрацією й авторизацією

ASP.NET Core **8** Web API для AnimeWiki. База даних: PostgreSQL 16, EF Core. API аніме, імпорт з AniList, Swagger та **JWT-авторизація користувачів**.

## Що працює

- `GET /api/anime` — аніме, пошук, жанри, пагінація;
- `GET /api/anime/{id}` — деталі з персонажами;
- `POST /api/import/anilist/{id}` — імпорт з AniList (залишено без змін);
- `POST /api/auth/register` — реєстрація;
- `POST /api/auth/login` — вхід за email або username;
- `GET /api/auth/me` — отримання профілю за JWT (вимагає токен).

Пароль **ніколи не зберігається відкритим текстом**: застосовано `PasswordHasher<User>` з ASP.NET Core Identity. Email/username індексуються у нормалізованому вигляді, тому дублікати не пройдуть навіть за іншого регістру. JWT підписується HMAC-SHA256, перевіряються підпис, issuer, audience, термін дії; термін стандартно **120 хвилин**. Налаштовано CORS і просте обмеження частоти запитів на авторизацію.

## Запуск з нуля

1. Встанови **.NET 8 SDK** та **Docker Desktop**. Відкрий папку `AnimeWiki.Api` (де знаходиться `.csproj`).
2. Запусти PostgreSQL:

```bash
docker compose up -d
```

3. Перевір `appsettings.json`: конфігурація під Docker уже налаштована на `127.0.0.1:5433`, `anime_wiki`, `postgres` / `postgres`. Це **локальний демонстраційний пароль**, перед реальним розгортанням заміни його.
4. Запусти:

```bash
dotnet restore
dotnet run --launch-profile http
```

5. Перейди до [Swagger](http://localhost:5080/swagger), виклич `POST /api/auth/register`, потім `GET /api/auth/me` з токеном через кнопку **Authorize**.

> В `appsettings.Development.json` знаходиться **демонстраційний ключ JWT тільки для локальної розробки**. У Production не задавай ключ у коді або Git: встанови змінну середовища `Jwt__Key` з випадковим секретом щонайменше 32 байти. Без ключа бекенд навмисно не запуститься. Також використовуй HTTPS і свої облікові дані БД.

### Додавання користувачів до вже існуючої БД

**Видаляти або пересоздавати БД не потрібно.** Під час запуску `DatabaseInitializer` робить `EnsureCreatedAsync()` та окремо виконує `CREATE TABLE IF NOT EXISTS "Users"` і створення індексів, оскільки `EnsureCreatedAsync()` **не додає нові таблиці** до існуючої бази.

Якщо потрібно додати таблицю вручну через DBeaver/psql, відкрий `Database/add_users.sql` на базі `anime_wiki` і виконай скрипт. Окремо є повна SQL-схема `Database/create_database.sql` для **нової** БД (цей файл містить `CREATE DATABASE`, тому його не треба запускати на вже створеній БД).

**Таблиця `Users`:** `Id`, `Username`, `NormalizedUsername`, `Email`, `NormalizedEmail`, `PasswordHash`, `CreatedAt`. Унікальні індекси: `IX_Users_NormalizedEmail`, `IX_Users_NormalizedUsername`.

Існуючі таблиці `Anime`, `Genres`, `Characters`, `AnimeGenre`, `AnimeCharacter` та їхні дані не змінюються.

## Фронтендеру

Передай файл **[FRONTEND_AUTH_GUIDE.md](FRONTEND_AUTH_GUIDE.md)** — в ньому готовий API-контракт, приклади JSON, коди помилок, CORS, приклади `fetch` для React/Next.js/Vite, перевірка через Swagger. Також додано готовий `Examples/frontend/authApi.js` та тестові запити `Tests/AuthApi.http`.

Коротко:

| Запит | Endpoint | Результат |
|---|---|---|
| `POST` | `/api/auth/register` | `201` і `{ token, tokenType, expiresAt, user }` |
| `POST` | `/api/auth/login` | `200` і `{ token, tokenType, expiresAt, user }` |
| `GET` | `/api/auth/me` | `200` та `{ id, username, email, createdAt }` |

Токен передавай заголовком `Authorization: Bearer <token>`. Інші маршрути API залишено доступними без авторизації, щоб не порушити старі сценарії; для нових приватних контролерів застосовуй `[Authorize]`.

## Конфігурація

- `ConnectionStrings:DefaultConnection` — PostgreSQL (за замовчуванням порт **5433**);
- `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiresMinutes` — налаштування JWT;
- `Jwt:Key` — секрет, для Production через `Jwt__Key`;
- `Cors:AllowedOrigins` — дозволені точні адреси фронтенду.

## Важливо

- Авторизацію додано без нових ролей, refresh-токенів, відновлення пароля чи підтвердження email: це базова функціональна система реєстрації/входу. За потреби можна розширити окремо.
- `EnsureCreated` залишено для сумісності з поточним проєктом, який не містить EF migrations; скрипт додавання `Users` є точковим безпечним доповненням. Для довгострокової підтримки схеми надалі краще перейти до міграцій окремим завданням.
- Реальну збірку/запуск треба перевірити на машині із .NET 8 SDK та доступною PostgreSQL.
