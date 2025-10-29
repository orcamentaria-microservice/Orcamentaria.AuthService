# 🔐 Orcamentaria.AuthService

Serviço de **autenticação e emissão de tokens** do ecossistema **Orcamentaria**. Provê fluxo de login, renovação/refresh, emissão de **tokens de usuário** e **tokens de serviço** (service-to-service), além de utilitários para **perfis/roles e permissões**.

> **Padrão de camadas**: API, Application, Domain e Infrastructure – alinhado ao restante do ecossistema e às libs compartilhadas (`Orcamentaria.Lib.*`).

---

## 🧱 Estrutura do Repositório

```
Orcamentaria.AuthService/
 ├── Orcamentaria.AuthService.API/
 ├── Orcamentaria.AuthService.Application/
 ├── Orcamentaria.AuthService.Domain/
 ├── Orcamentaria.AuthService.Infrastructure/
 └── Orcamentaria.AuthService.sln
```

- **API**: Endpoints HTTP (controllers, middlewares, auth handlers).
- **Application**: Casos de uso/serviços de aplicação (login, emissão e refresh de tokens, validação de credenciais, etc.).
- **Domain**: Entidades (User, Service, Role/Permission, Bootstrap, RefreshToken), agregados, eventos e validações.
- **Infrastructure**: Repositórios, mapeamentos, acesso a dados e integrações externas (ex.: Service Registry, ConfigBag), providers de chaves.

---

## ✨ Principais Funcionalidades

- **Login de usuário** e emissão de **JWT** assinado com a *chave privada de usuário*.
- **Emissão de token de serviço** (m2m) com **chave privada distinta** da de usuário.
- **Refresh token**: rotação segura e revogação.
- **Perfis/Roles & Permissões**: acoplados ao token de usuário quando aplicável.
- **Bootstrap/Service Credentials**: fluxo para serviços obterem token inicial e registrar-se (quando habilitado).
- **Middlewares** de autenticação padronizados via `Orcamentaria.Lib.*`.

> **Observação**: alguns módulos podem estar em desenvolvimento; veja a seção **Roadmap**.

---

## 🛠️ Stack & Requisitos

- **.NET**: 8.0+
- **C#**: 12
- **Bibliotecas**: `Orcamentaria.Lib.Domain`, `Orcamentaria.Lib.Application`, `Orcamentaria.Lib.Infrastructure`
- **Banco**: (defina aqui – MySQL/SQL Server/Oracle) – ajuste a *connection string* em `appsettings.*.json`.
- **Mensageria (opcional)**: RabbitMQ (para eventos e/ou bootstrap), se aplicável no ambiente.

---

## ⚙️ Configuração (Ambiente)

Defina as variáveis/`appsettings` conforme seu ambiente. Exemplos de chaves:

```jsonc
{
  "ConnectionStrings": {
    "Default": "<SUA CONNECTION STRING>"
  },
  "Jwt": {
    "Issuer": "Orcamentaria.AuthService",
    "Audience": "Orcamentaria",
    "AccessTokenMinutes": 30,
    "RefreshTokenDays": 7,
    "UserSigningKey": "file://keys/auth-user-key-1.pem",
    "ServiceSigningKey": "file://keys/auth-service-key-1.pem"
  },
  "Bootstrap": {
    "Enabled": true,
    "ExpiresMinutes": 15
  },
  "ServiceRegistry": {
    "BaseUrl": "https://registry.local",
    "Enabled": false
  }
}
```

> **Dica**: armazene **chaves privadas** fora do repositório (KMS/Secrets Manager/Key Vault). Nunca *commite* chaves.

---

## ▶️ Execução Local

1. **Restaure e compile**:
   ```bash
   dotnet restore
   dotnet build
   ```
2. **Configure o ambiente**:
   - Crie `appsettings.Development.json` com suas credenciais.
   - Disponibilize as chaves de assinatura localmente (ou aponte para um provedor).
3. **Execute a API**:
   ```bash
   dotnet run --project ./Orcamentaria.AuthService.API
   ```
4. **Testes** (se presentes):
   ```bash
   dotnet test
   ```

---

## 🔑 Fluxos de Autenticação (Exemplos)

**POST** `/api/auth/login`
```json
{
  "username": "alice",
  "password": "p@ssw0rd"
}
```

**200 OK**
```json
{
  "accessToken": "<jwt>",
  "expiresIn": 1800,
  "refreshToken": "<refresh>",
  "tokenType": "Bearer",
  "roles": ["USER:READ", "USER:UPDATE"],
  "permissions": ["USER:UPDATE:ALTERPERMISSION"]
}
```

**POST** `/api/auth/refresh`
```json
{
  "refreshToken": "<refresh>"
}
```

**POST** `/api/service/token`
```json
{
  "clientId": "service.person",
  "clientSecret": "<secret>"
}
```

**200 OK**
```json
{
  "accessToken": "<jwt>",
  "expiresIn": 1800,
  "tokenType": "Bearer"
}
```

---

## 🧩 Integração com outros serviços

- **API Gateway**: valide o **audience** e a **assinatura**.
- **Service-to-Service**: use o **ServiceSigningKey** e roles específicas de serviço (ex.: `CONFIG:READ`, `CONFIG:WRITE`).
- **ConfigBag**: opcionalmente, carregue *envs* no startup com token **bootstrap** (tempo curto + revogável).

---

## 🧪 Boas Práticas de Segurança

- **Rotacione chaves** e use *kid* no header do JWT.
- **Separe** permissões de **usuário** e **serviço** (chaves diferentes, audiences distintos).
- **Curta validade** para access tokens; **refresh** com rotação e *denylist*.
- **Least privilege**: roles e permissões mínimas por caso de uso.
- **Logs** sem dados sensíveis (PII/segredos).

---

## 🗺️ Roadmap

- [ ] Documentar endpoints reais (Swashbuckle/Swagger).
- [ ] Incluir exemplos de *policies* (ex.: `RequireRole("ADMIN")`).
- [ ] Expor */health* e */readiness*.
- [ ] CI/CD (build, testes, *docker publish*).
- [ ] Chaves via KMS/KeyVault e rotação automatizada.

---

## 🐳 Docker

```yaml
version: '3.8'
services:
  authservice:
    image: orcamentaria/authservice:latest
    build: ./
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=${DB_CONN}
      - Jwt__Issuer=Orcamentaria.AuthService
      - Jwt__Audience=Orcamentaria
      - Jwt__AccessTokenMinutes=30
      - Jwt__RefreshTokenDays=7
      - Jwt__UserSigningKey=/run/secrets/user_key
      - Jwt__ServiceSigningKey=/run/secrets/service_key
    secrets:
      - user_key
      - service_key
secrets:
  user_key:
    file: ./keys/auth-user-key-1.pem
  service_key:
    file: ./keys/auth-service-key-1.pem
```

---

## 🤝 Contribuição

1. Crie uma *branch* a partir de `master`.
2. Faça commits pequenos e descritivos.
3. Abra um PR com **descrição do contexto, motivação e evidências** (logs/prints/requests).

---

## 📄 Licença

Defina a licença do projeto (ex.: MIT). Adicione `LICENSE` na raiz.

---

## 👤 Autor

**Marcelo Fernando** – Arquitetura & Backend (Auth/Segurança)
