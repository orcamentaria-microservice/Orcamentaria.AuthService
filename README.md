# Orcamentaria • Auth Service (.NET 8)

Serviço de **Autenticação e Autorização** para o ecossistema Orcamentaria. Emite e valida **JWT** distintos para **usuários** e **serviços**, gerencia **permissões** e **credenciais** de serviços, e implementa o fluxo de **bootstrap** para registro inicial de microserviços.

---

## 🧱 Stack & Arquitetura
- **.NET 8** (ASP.NET Core Web API)
- Camadas: API, Application, Domain, Infrastructure
- Banco: **MySQL** (via `DefaultConnection`)
- Mensageria: **RabbitMQ** (erro / erro crítico)
- Chaves **RSA** (par público/privado) para **user** e **service**
- Versionamento de API: `api/v1/...`

Estrutura (resumo):
```
Orcamentaria.AuthService/
 ├── Orcamentaria.AuthService.API/
 ├── Orcamentaria.AuthService.Application/
 ├── Orcamentaria.AuthService.Domain/
 ├── Orcamentaria.AuthService.Infrastructure/
 └── Orcamentaria.AuthService.sln
```

---

## ⚙️ Configuração (appsettings.json)

- **ConnectionStrings.DefaultConnection**: conexão MySQL (ex.: `Server=localhost;Port=3306;User Id=<user>;Password=<pass>;Initial Catalog=auth-db`)
- **MessageBrokerConfiguration** (RabbitMQ):
  - `Host`, `Port`, `UserName`, `Password`
  - `ErrorQueue`, `ErrorCriticalQueue`

---

## 🔐 Chaves & Tokens

Na pasta `Orcamentaria.AuthService.API/Keys/` existem **4** arquivos:
- `private_key_user.pem` / `public_key_user.pem` — assinam/validam **JWT de usuário**
- `private_key_service.pem` / `public_key_service.pem` — assinam/validam **JWT de serviço**

---

## ▶️ Executando localmente

```bash
# na solução .NET
dotnet restore
dotnet build
dotnet run --project Orcamentaria.AuthService.API
```

A API sobe (por padrão) com versão **v1** sob `/api/v1/...`.

---

## 🧭 Endpoints (v1)

### AuthenticationController
- **POST** `/api/v1/Authentication/Bootstrap/Authenticate/{bootstrapSecret}`
- **POST** `/api/v1/Authentication/Service/Authenticate/{clientId}/{clientSecret}`
- **POST** `/api/v1/Authentication/User/Authenticate/{email}/{password}`
- **POST** `/api/v1/Authentication/User/RefreshToken`

### BootstrapController
- **GET** `/api/v1/Bootstrap/CreateBootstrapSecret/{serviceId}`
- **GET** `/api/v1/Bootstrap/RevokeBootstrapSecret/{serviceId}`

### PermissionController
- **GET** `/api/v1/Permission/GetById/{id}`
- **GET** `/api/v1/Permission/GetByResource/{resource}`
- **GET** `/api/v1/Permission/GetByType/{type}`
- **PUT** `/api/v1/Permission/{id}`

### ServiceController
- **GET** `/api/v1/Service/GetById/{id}`
- **PUT** `/api/v1/Service/UpdateCredentials/{id}`
- **PUT** `/api/v1/Service/{id}`

### UserController
- **GET** `/api/v1/User/GetByCompanyId`
- **GET** `/api/v1/User/GetByEmail/{email}`
- **PUT** `/api/v1/User/AddPermission/{id}`
- **PUT** `/api/v1/User/RemovePermission/{id}`
- **PUT** `/api/v1/User/UpdatePassword/{id}`
- **PUT** `/api/v1/User/{id}`
Observações:
- Rotas seguem o padrão `[Route("api/v1/[controller]")]` na camada de API.
- Algumas operações exigem **autorização** via `[Authorize(Roles = "...")]`.

### Fluxos principais
- **Login de serviço**: `POST /api/v1/Authentication/Service/Authenticate/{clientId}/{clientSecret}` → retorna JWT **de serviço**.
- **Bootstrap**: `POST /api/v1/Authentication/Bootstrap/Authenticate/{bootstrapSecret}` e _GETs_ para criar/revogar segredo em `/api/v1/Bootstrap/...`.
- **Login de usuário**: `POST /api/v1/Authentication/User/Authenticate/{email}/{password}` + `POST /api/v1/Authentication/User/RefreshToken`.
- **Gestão de permissões** (User/PermissionController): adicionar/remover/atualizar.

---

## 🔧 Papéis, Permissões e Políticas

- **Separa** permissões de **usuário** e de **serviço**.
- Controladores usam `[Authorize(Roles = "MASTER,USER:UPDATE:ALTERPERMISSION", ...)]` quando aplicável.

---

## 📦 Dependências técnicas (principais)

- ASP.NET Core, JWT Bearer, DI (Keyed Services), etc.
- Drivers MySQL
- RabbitMQ client

> As implementações incluem múltiplos provedores de token (`ITokenService<T>`): `userToken`, `serviceToken`, `clientIdToken`, `clientSecretToken`, `bootstrapSecretToken`, `bootstrapToken` e um `ITokenProvider` geral.

---

## 🧪 Exemplos (curl)

```bash
# 1) Autenticação de Serviço
curl -X POST "http://localhost:PORT/api/v1/Authentication/Service/Authenticate/{clientId}/{clientSecret}"

# 2) Autenticação de Usuário
curl -X POST "http://localhost:PORT/api/v1/Authentication/User/Authenticate/{email}/{password}"

# 3) Refresh Token de Usuário
curl -X POST "http://localhost:PORT/api/v1/Authentication/User/RefreshToken"
```

Para rotas com `[Authorize]`, envie `Authorization: Bearer <token>` (tipo correto: **user** vs **service**).

---

## ✨ Autor

**Marcelo Fernando**  
Desenvolvedor Fullstack | Arquitetura de Microserviços