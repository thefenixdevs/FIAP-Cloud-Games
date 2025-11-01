
---

# FIAP Cloud Games

Plataforma de gestão de jogos digitais construída em .NET com arquitetura orientada a domínio (Clean Architecture + DDD)

## Índice

* Sobre o Projeto
* Visão Geral & Arquitetura
* Tecnologias, Frameworks e Bibliotecas
* Estrutura de Pastas
* Módulos de Negócio e Fluxos Desenvolvidos
* Estratégia de Persistência de Dados
* Estratégia de Testes
* Como Executar o Projeto
* Configuração
* Endpoints da API
* Publicação e Releases
* Contribuindo
* Licença
* Equipe

---

## Sobre o Projeto

O FIAP Cloud Games é uma plataforma de gestão de catálogo de jogos digitais, desenvolvida como parte do Tech Challenge da instituição FIAP. O sistema demonstra a aplicação de boas práticas de engenharia de software, arquitetura moderna (Clean Architecture) e design orientado a domínio (DDD).
As funcionalidades principais englobam autenticação/autorização, CRUD de jogos, perfis de usuário diferenciados (comum e admin), e rastreabilidade de requisições.

### Objetivos principais

* Aplicar Clean Architecture e DDD na tecnologia .NET
* Implementar autenticação e autorização robustas com JWT
* Utilizar padrões de design como Repository, Unit of Work e Dependency Injection
* Garantir qualidade via testes automatizados em múltiplas camadas
* Persistência com Entity Framework Core usando abordagem Code-First
* Logging estruturado e rastreabilidade com CorrelationId

---

## Visão Geral & Arquitetura

A aplicação está organizada em camadas bem definidas, seguindo os princípios da Clean Architecture, com dependências unidirecionais do mundo externo para o domínio. As camadas são:

* **Presentation / API** (ex. `GameStore.API`) — ponto de entrada HTTP/REST, controllers, middlewares, configuração.
* **Application / Use Cases** (ex. `GameStore.Application`) — serviços de aplicação, DTOs, casos de uso, lógica orquestradora.
* **Domain / Core** (ex. `GameStore.Domain`) — entidades de domínio, value objects, interfaces de repositório, regras de negócio puras (sem dependência de frameworks).
* **Infrastructure / External** (ex. `GameStore.Infrastructure`) — implementações técnicas, persistência (EF Core), repositórios, seeders, migrations.
* **Tests** (ex. `GameStore.Tests`) — testes unitários e de integração distribuídos nas camadas acima.

Essa separação favorece a testabilidade, manutenibilidade e evolução da aplicação.

---

## Tecnologias, Frameworks e Bibliotecas

### Runtime / Framework principal

* .NET 9.0 SDK – runtime e framework principal. ([GitHub][1])
* C# 12 – linguagem utilizada. ([GitHub][1])
* ASP.NET Core 9.0 – para a API web. ([GitHub][1])

### Banco de Dados / ORM

* SQLite – banco relacional leve para desenvolvimento e demonstração. ([GitHub][1])
* Entity Framework Core 9.0 – ORM para mapeamento objeto-relacional e migrations. ([GitHub][1])

### Autenticação e Segurança

* JWT (JSON Web Tokens) – autenticação stateless. ([GitHub][1])
* BCrypt.Net-Next – para hashing seguro de senhas. ([GitHub][1])
* Microsoft.AspNetCore.Authentication.JwtBearer – middleware de JWT no ASP.NET Core. ([GitHub][1])

### Logging e Observabilidade

* Serilog – logging estruturado. ([GitHub][1])
* Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File – para integração e armazenamento de logs. ([GitHub][1])

### Testes

* xUnit – framework de testes unitários. ([GitHub][1])
* Moq – biblioteca para mocking / stubbing. ([GitHub][1])
* FluentAssertions – para assert mais legíveis. ([GitHub][1])
* EF Core InMemory – provider em memória para testes de persistência. ([GitHub][1])

### Documentação

* Swagger / OpenAPI – para documentação interativa da API. ([GitHub][1])
* Swashbuckle.AspNetCore – geração automática da documentação. ([GitHub][1])

### Ferramentas de Desenvolvimento

* Git – controle de versão. ([GitHub][1])
* Visual Studio / VS Code – IDEs suportadas. ([GitHub][1])

### Padrões de Projeto e Outras Bibliotecas

* Repository, Unit of Work, Dependency Injection – padrões aplicados no projeto. ([GitHub][1])
* Futuramente/Opção: AutoMapper (mapeamento objeto-objeto), FluentValidation (validações fluentes), MediatR (CQRS/mediador) – conforme README atual do projeto. ([GitHub][1])

---

## Estrutura de Pastas

```
FIAP-Cloud-Games/
│
├── GameStore.sln                  # Solução principal
├── global.json                    # Definição da versão do SDK
├── LICENSE.txt                    # Licença do projeto
├── README.md                      # (este arquivo)
│
├── GameStore.API/                 # Camada de Apresentação (API)
│   ├── Controllers/               # Endpoints REST (AuthController, GamesController) :contentReference[oaicite:24]{index=24}  
│   ├── Authorization/             # Políticas de autorização customizadas :contentReference[oaicite:25]{index=25}  
│   ├── Middleware/                # Middlewares customizados (CorrelationId etc) :contentReference[oaicite:26]{index=26}  
│   ├── Database/                  # Arquivo SQLite (gamestore.db) :contentReference[oaicite:27]{index=27}  
│   ├── Program.cs                 # Bootstrapping da aplicação :contentReference[oaicite:28]{index=28}  
│   └── appsettings*.json          # Configuração da aplicação :contentReference[oaicite:29]{index=29}  
│
├── GameStore.Application/         # Camada de Aplicação (Use Cases) :contentReference[oaicite:30]{index=30}  
│   ├── Services/                  # Serviços (AuthService, GameService, JwtService) :contentReference[oaicite:31]{index=31}  
│   ├── DTOs/                      # DTOs (AuthDTOs, GameDTOs) :contentReference[oaicite:32]{index=32}  
│   └── DependencyInjection.cs     # Registro de serviços desta camada :contentReference[oaicite:33]{index=33}  
│
├── GameStore.Domain/              # Camada de Domínio (Core) :contentReference[oaicite:34]{index=34}  
│   ├── Entities/                  # Entidades (User, Game, BaseEntity) :contentReference[oaicite:35]{index=35}  
│   ├── Enums/                     # Enumerações (AccountStatus, ProfileType) :contentReference[oaicite:36]{index=36}  
│   └── Repositories/              # Contratos de repositórios (IGameRepository, IUserRepository) :contentReference[oaicite:37]{index=37}  
│
├── GameStore.Infrastructure/      # Camada de Infraestrutura (Persistência) :contentReference[oaicite:38]{index=38}  
│   ├── Data/                      # DbContext, Configurations (Fluent API) :contentReference[oaicite:39]{index=39}  
│   ├── Migrations/                # Migrations do EF Core :contentReference[oaicite:40]{index=40}  
│   ├── Repositories/              # Implementações de repositórios (GameRepository, UserRepository) :contentReference[oaicite:41]{index=41}  
│   └── DependencyInjection.cs     # Registro de infra-estrutura no DI :contentReference[oaicite:42]{index=42}  
│
└── GameStore.Tests/               # Camada de Testes Automatizados :contentReference[oaicite:43]{index=43}  
    ├── API/                       # Testes de controllers / middleware  
    ├── Application/               # Testes de serviços  
    ├── Infrastructure/             # Testes de repositórios / UoW  
    └── Usings.cs                   # Usings compartilhados para testes  
```

---

## Módulos de Negócio e Fluxos Desenvolvidos

### 1. Autenticação e Autorização

**Funcionalidades**

* Registro de novos usuários com validação de e-mail único. ([GitHub][1])
* Login com geração de token JWT. ([GitHub][1])
* Hash de senhas utilizando BCrypt. ([GitHub][1])
* Perfis de usuário: `CommonUser` e `Admin`. ([GitHub][1])
* Status da conta: `Pending`, `Confirmed`, `Banned`. ([GitHub][1])

**Regras de negócio**

* Novos usuários iniciam com `Pending` e não podem acessar funcionalidades protegidas até confirmação. ([GitHub][1])
* Apenas usuários com status `Confirmed` podem executar operações protegidas. ([GitHub][1])
* Senhas são hasheadas antes de persistir no banco. ([GitHub][1])

**Fluxo típico**

1. Usuário envia requisição `POST /api/auth/register` com email, username e password.
2. Validação de email/username único.
3. Password é hasheada e entidade `User` criada com perfil `CommonUser` e status `Pending`.
4. Usuário faz login via `POST /api/auth/login`, recebe token JWT se credenciais válidas.
5. Em requisições subsequentes a API valida token, verifica claims e aplica políticas de autorização (por exemplo: somente `ConfirmedAdmin` pode criar/editar/excluir jogos).

---

### 2. Gestão de Jogos

**Funcionalidades**

* Listagem de todos os jogos (`GET /api/games`) para usuários confirmados. ([GitHub][1])
* Obtenção de jogo por ID (`GET /api/games/{id}`). ([GitHub][1])
* Criação de novo jogo (`POST /api/games`) — somente para usuários com perfil `Admin` e status `Confirmed`. ([GitHub][1])
* Atualização de jogo (`PUT /api/games/{id}`) — somente Admins. ([GitHub][1])
* Exclusão de jogo (`DELETE /api/games/{id}`) — somente Admins. ([GitHub][1])

**Regras de negócio**

* Título do jogo é obrigatório. ([GitHub][1])
* Preço não pode ser negativo. ([GitHub][1])
* Data de lançamento é opcional. ([GitHub][1])
* Somente usuários confirmados e com perfil adequado podem operar os endpoints conforme permissão. ([GitHub][1])

**Fluxo típico**

1. Usuário autenticado com token válido envia `GET /api/games` para listar.
2. Usuário Admin envia `POST /api/games` com payload do jogo (título, descrição, preço, gênero, data de lançamento).
3. Aplicação passa para `GameService.CreateAsync`, que valida o DTO, aplica regras de negócio, usa `IGameRepository.AddAsync`, e `IUnitOfWork.SaveChangesAsync` para persistir.
4. Se operação bem-sucedida, retorna 201 com recurso criado; caso contrário, erro apropriado.
5. Requisições subsequentes podem atualizar (`PUT`) ou deletar (`DELETE`) seguindo lógica semelhante.

---

### 3. Rastreabilidade e Logging

**Funcionalidades**

* Interceptação de todas as requisições HTTP por um middleware `CorrelationIdMiddleware`, que garante que cada requisição tenha um `X‐Correlation‐Id` no header. ([GitHub][1])
* Esse `CorrelationId` é propagado no contexto da aplicação e incluído em todos os logs para rastreabilidade. ([GitHub][1])
* Logs estruturados com contexto (timestamp, nível, correlation id, mensagem) via Serilog, com output para console e arquivo (rolling daily). ([GitHub][1])

**Fluxo típico**

1. Cliente envia requisição para o endpoint da API com ou sem header `X-Correlation-Id`.
2. O middleware verifica: se header existe, reutiliza; senão, gera novo GUID e adiciona no contexto.
3. No início e fim da pipeline de requisição, logs são emitidos com o correlation id.
4. Caso haja erro/unhandled exception, o middleware de exceção intercepta, emite log com correlation id e retorna resposta de erro consistente para o cliente.
5. Dessa forma, quando suportes ou debugging forem necessários, basta buscar nos logs pelo correlation id para rastrear toda a cadeia de execução daquela requisição.

---

## Estratégia de Persistência de Dados

### Abordagem: Code-First com Entity Framework Core

* As entidades de domínio são modeladas em C# (ex: `User`, `Game`) e o banco de dados é gerado automaticamente pelas migrations. ([GitHub][1])
* Banco de dados utilizado: SQLite, via um arquivo local (`gamestore.db`). A escolha se dá por simplicidade de setup e portabilidade em ambiente de desenvolvimento. ([GitHub][1])
* O sistema de configurações de entidade (Fluent API) está em `GameStore.Infrastructure/Data/Configurations`, permitindo definir chaves primárias, índices únicos (ex: email, username), relacionamentos, restrições, conversão de enums, etc. ([GitHub][1])
* Migrations e seeders:

  * Migrations permitem evolução do schema com comandos como `dotnet ef migrations add NomeDaMigracao` e `dotnet ef database update`. ([GitHub][1])
  * Seeders: a aplicação possui `IDataSeeder`, `DataSeederOrchestrator` e implementações específicas (ex: `UserSeeder`) para popular dados iniciais (como usuário Admin padrão). ([GitHub][1])
* Padrão Unit of Work (`IUnitOfWork`) e Repository estão implementados para garantir transações explícitas, atomicidade e clareza nas operações de persistência. ([GitHub][1])

---

## Estratégia de Testes

### Filosofia de Testes

O projeto adota uma pirâmide de testes clássica: muitos testes unitários, menos testes de integração, e poucos (quando houver) testes end-to-end. ([GitHub][1])

### Frameworks / Ferramentas

* xUnit – framework de testes para .NET. ([GitHub][1])
* Moq – mocking de dependências. ([GitHub][1])
* FluentAssertions – para assert mais claros. ([GitHub][1])
* EF Core InMemory – provider para testes de repositório/persistência. ([GitHub][1])

### Categorias de Testes

* **Testes Unitários**: localizados em `GameStore.Tests/Application/Services/`. Focam em serviços de aplicação (ex: `AuthService`, `GameService`, `JwtService`) e nas regras de negócio isoladas. Exemplos de cenários: registro com email duplicado, login com senha incorreta, geração de token válida. ([GitHub][1])
* **Testes de Integração**: localizados em `GameStore.Tests/Infrastructure/Repositories/`. Focam em operações de persistência reais (com o provider InMemory) e verificam comportamentos como persistência correta, consulta por ID, integridade de restrições (ex: índice único de email). ([GitHub][1])
* **Testes de Middleware / Autorização**: localizados em `GameStore.Tests/API/Middleware/` e `GameStore.Tests/API/Authorization/`. Focam em middlewares (ex: `CorrelationIdMiddleware`) e handlers de autorização customizados (`ConfirmedCommonUserHandler`, `ConfirmedAdminHandler`) para validar headers, claims e perfis de usuário. ([GitHub][1])

### Cobertura de Testes

* A meta de cobertura é 70% ou mais nas camadas críticas (Application e Domain). ([GitHub][1])
* Boas práticas adotadas: padrão AAA (Arrange-Act-Assert), testes independentes, nomes descritivos, fixtures reutilizáveis, limpeza automática de contextos. ([GitHub][1])

### Executando os Testes

```bash
# Executar todos os testes
dotnet test GameStore.sln

# Com nível de detalhamento
dotnet test GameStore.sln --logger "console;verbosity=detailed"

# Gerar relatório de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

([GitHub][1])

---

## Como Executar o Projeto

### Pré-requisitos

* .NET 9.0 SDK instalado (executar `dotnet --version`, deve retornar algo como 9.0.x) ([GitHub][1])
* Git instalado
* Editor de código de sua preferência (Visual Studio 2022, VS Code, Rider)

### Passo a passo

1. Clone o repositório:

   ```bash
   git clone https://github.com/thefenixdevs/FIAP-Cloud-Games.git
   cd FIAP-Cloud-Games
   ```

   ([GitHub][1])
2. Restaure as dependências:

   ```bash
   dotnet restore GameStore.sln
   ```

   ([GitHub][1])
3. Aplique as migrations para criar o banco de dados SQLite:

   ```bash
   dotnet ef database update --project GameStore.Infrastructure --startup-project GameStore.API
   ```

   Nota: O banco será criado em `GameStore.API/Database/gamestore.db`. ([GitHub][1])
4. Execute a API:

   ```bash
   dotnet run --project GameStore.API/GameStore.API.csproj
   ```

   Saída esperada semelhante a:

   ```
   info: Microsoft.Hosting.Lifetime[14]  
         Now listening on: https://localhost:7001  
   ```

   ([GitHub][1])
5. Acesse o Swagger UI em `https://localhost:7001/swagger` para ver a documentação interativa e testar os endpoints. ([GitHub][1])

---

## Configuração

O arquivo de configuração principal está em `GameStore.API/appsettings.json`. Exemplo de configuração:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Database\\gamestore.db"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123456",
    "Issuer": "GameStore",
    "Audience": "GameStoreApiUsers",
    "ExpirationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

([GitHub][1])

### Configurações importantes

| Configuração                          | Descrição                                    | Valor padrão                        |
| ------------------------------------- | -------------------------------------------- | ----------------------------------- |
| `ConnectionStrings:DefaultConnection` | String de conexão do SQLite                  | `Data Source=Database\gamestore.db` |
| `Jwt:SecretKey`                       | Chave secreta para assinatura dos tokens JWT | Exemplo acima                       |
| `Jwt:Issuer`                          | Emissor do token                             | `GameStore`                         |
| `Jwt:Audience`                        | Audiência/tipo de usuário do token           | `GameStoreApiUsers`                 |
| `Jwt:ExpirationInMinutes`             | Tempo de expiração do token (em minutos)     | `60`                                |

> ⚠️ Em ambiente de produção, **alterar** `Jwt:SecretKey` para uma chave forte (mínimo 32 caracteres), e utilizar variáveis de ambiente ou serviços de cofre (ex: Azure Key Vault) para gerenciamento seguro de segredos. Também configurar HTTPS com certificado válido. ([GitHub][1])

---

## Endpoints da API

### Autenticação

| Método | Endpoint             | Descrição              | Autorização   |
| ------ | -------------------- | ---------------------- | ------------- |
| POST   | `/api/auth/register` | Registrar novo usuário | Não requerida |
| POST   | `/api/auth/login`    | Autenticar e obter JWT | Não requerida |

### Jogos

| Método        | Endpoint          | Descrição                   | Autorização                 |
| ------------- | ----------------- | --------------------------- | --------------------------- |
| GET           | `/api/games`      | Listar todos os jogos       | Usuário confirmado (Common) |
| GET           | `/api/games/{id}` | Obter jogo por ID           | Usuário confirmado (Common) |
| POST          | `/api/games`      | Criar novo jogo             | Usuário Admin confirmado    |
| PUT           | `/api/games/{id}` | Atualizar um jogo existente | Usuário Admin confirmado    |
| DELETE        | `/api/games/{id}` | Excluir um jogo             | Usuário Admin confirmado    |
| ([GitHub][1]) |                   |                             |                             |

---

## Publicação e Releases

* O versionamento segue o semântico e está centralizado no arquivo `Directory.Build.props` (ex: versão `0.2.0`). ([GitHub][1])
* O arquivo de notas de release está em `RELEASE_NOTES.md`. ([GitHub][1])
* É configurada uma pipeline CI (ex: `ci.yml`) para builds automáticos, execução de testes e cobertura de código. ([GitHub][1])
* Para gerar uma nova release:

  1. Garantir branch principal (`master` ou `main`) atualizada.
  2. Criar tag semântica: `git tag v0.2.0 && git push origin v0.2.0`. ([GitHub][1])
  3. Workflow publica artefato ZIP e usa notas de release como base. ([GitHub][1])

---

## Contribuindo

Contribuições são muito bem-vindas! Se você quer melhorar ou estender o projeto, siga os passos abaixo:

1. Faça fork do repositório.
2. Crie uma branch para sua feature: `git checkout -b feature/NovaFuncionalidade`.
3. Realize seus commits: `git commit -m "Add NovaFuncionalidade"`.
4. Faça push da branch: `git push origin feature/NovaFuncionalidade`.
5. Abra um Pull Request no repositório original.
6. Certifique-se que todos os testes passaram e que as alterações seguem os padrões de código e arquitetura do projeto.

---

## Licença

Este projeto está licenciado sob a licença MIT (ver arquivo `LICENSE.txt`).
([GitHub][1])

---

## Equipe

Desenvolvido pela equipe **thefenixdevs** para o Tech Challenge da FIAP.

---

