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
- **Banco**: (MySQL/SQL) – ajuste a *connection string* em `appsettings.*.json`.
- **Mensageria**: RabbitMQ (para log centralizado e real time configurations).

---

## ⚙️ Configuração (Ambiente)

Defina as variáveis/`appsettings` conforme seu ambiente. Exemplos de chaves:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "<<value>>"
  },
  "MessageBrokerConfiguration": {
    "BrokerName": "<<value>>",
    "Host": "<<value>>",
    "Port": "<<value>>",
    "UserName": "<<value>>",
    "Password": "<<value>>",
    "ErrorQueue": "<<value>>",
    "ErrorCriticalQueue": "<<value>>"
  },
  "ApiGetawayConfiguration": {
    "BaseUrl": "<<value>>"
  },
  "ServiceConfiguration": {
    "ServiceName": "<<value>>",
    "ClientId": "<<value>>",
    "ClientSecret": "<<value>>"
  }
}

```

---

## ▶️ Execução Local

1. **Restaure e compile**:
   ```bash
   dotnet restore
   dotnet build
   ```
2. **Configure o ambiente**:
   - Crie `appsettings.Development.json` com suas credenciais.

3. **Execute a API**:
   ```bash
   dotnet run --project ./Orcamentaria.AuthService.API
   ```
4. **Testes** (se presentes):
   ```bash
   dotnet test
   ```

---

## 🧩 Integração com outros serviços

- **API Gateway**: valide o **audience** e a **assinatura**.
- **Service-to-Service**: use o **ServiceSigningKey** e roles específicas de serviço (ex.: `CONFIG:READ`, `CONFIG:INSERT`).
- **ConfigBag**: carregue as envs em real time.

---

## ✨ Autor

**Marcelo Fernando**  
Desenvolvedor Fullstack | Arquitetura de Microserviços
