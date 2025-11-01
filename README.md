# FIAP Cloud Games

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-007ACC?style=for-the-badge&logo=dotnet" alt=".NET 9.0"/>
  <img src="https://img.shields.io/badge/Entity_Framework-Core-1BA1E2?style=for-the-badge&logo=microsoft" alt="EF Core"/>
  <img src="https://img.shields.io/badge/SQLite-Database-00A4DC?style=for-the-badge&logo=sqlite" alt="SQLite"/>
  <img src="https://img.shields.io/badge/JWT-Authentication-FF6B35?style=for-the-badge&logo=jsonwebtokens" alt="JWT"/>
  <img src="https://img.shields.io/badge/xUnit-Testing-A020F0?style=for-the-badge" alt="xUnit"/>
  <a href="https://github.com/thefenixdevs/FIAP-Cloud-Games/releases/latest"><img src="https://img.shields.io/github/v/release/thefenixdevs/FIAP-Cloud-Games?style=for-the-badge&label=Release&logo=github" alt="Release badge"/></a>
  <a href="https://github.com/thefenixdevs/FIAP-Cloud-Games/actions/workflows/ci.yml"><img src="https://img.shields.io/github/actions/workflow/status/thefenixdevs/FIAP-Cloud-Games/ci.yml?style=for-the-badge&label=CI&logo=github" alt="CI badge"/></a>
</p>

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Origem e Evolução](#origem-e-evolução)
- [Visão Geral](#visão-geral)
- [Princípios Arquiteturais](#princípios-arquiteturais)
- [Padrão CQRS e Organização por Features](#-padrão-cqrs-e-organização-por-features)
- [ApplicationResult Pattern](#-applicationresult-pattern)
- [Sistema de Localização](#-sistema-de-localização)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Tecnologias e Ferramentas](#tecnologias-e-ferramentas)
- [Módulos de Negócio](#módulos-de-negócio)
- [Camada CrossCutting](#-camada-crosscutting)
- [Estratégia de Banco de Dados](#estratégia-de-banco-de-dados)
- [Estratégia de Testes](#estratégia-de-testes)
- [Como Executar o Projeto](#como-executar-o-projeto)
- [Configuração](#configuração)
- [Endpoints da API](#endpoints-da-api)
- [Publicação e Releases](#publicação-e-releases)
- [Contribuindo](#contribuindo)

---

## 🎮 Sobre o Projeto

**FIAP Cloud Games** é uma plataforma de gestão de jogos digitais desenvolvida como projeto acadêmico para o Tech Challenge da FIAP. O sistema demonstra a aplicação prática de conceitos avançados de arquitetura de software, utilizando .NET 9 com Clean Architecture, Domain-Driven Design (DDD) e padrões modernos de desenvolvimento.

A aplicação oferece um ecossistema completo para gerenciamento de catálogos de jogos digitais, incluindo autenticação segura, autorização baseada em perfis, auditoria de requisições e persistência de dados robusta.

### 🎯 Objetivos do Projeto

- Demonstrar a aplicação de **Clean Architecture** e **DDD** em ambiente .NET
- Implementar padrão **CQRS** com Mediator para separação de comandos e consultas
- Organizar código por **Features** para melhor manutenibilidade
- Aplicar padrões de design como **Repository**, **Unit of Work**, **Result Pattern** e **Dependency Injection**
- Implementar autenticação e autorização robustas com **JWT**
- Garantir qualidade através de **testes automatizados** em múltiplas camadas
- Utilizar **Entity Framework Core** com abordagem Code-First
- Implementar **logging estruturado** e **rastreabilidade de requisições**
- Implementar **localização multi-idioma** para suporte internacional

---

## 🔄 Origem e Evolução

Este projeto é uma **evolução** da prova de conceito (PoC) desenvolvida no repositório [TechChallengeGameStore](https://github.com/thefenixdevs/TechChallengeGameStore), especificamente do caminho **ProofsOfConcepts/v12**.

A versão atual representa um refinamento arquitetural significativo, incorporando:

- ✅ Migração completa para **.NET 9**
- ✅ Refatoração para **Clean Architecture** pura
- ✅ Implementação de **padrão CQRS** com Mediator
- ✅ Organização por **Features** (Auth, Games, Users)
- ✅ **FluentValidation** para validações automáticas
- ✅ **Mapster** para mapeamento de objetos
- ✅ **ApplicationResult Pattern** para padronização de respostas
- ✅ **Sistema de localização** multi-idioma (pt-BR, en-US)
- ✅ **Camada CrossCutting** para concerns transversais
- ✅ **BaseController** com helpers para padronização
- ✅ **ExceptionHandlingMiddleware** para tratamento centralizado
- ✅ Implementação de **políticas de autorização customizadas**
- ✅ **Middleware de CorrelationId** para rastreabilidade
- ✅ **Seeding automatizado** de dados iniciais
- ✅ **Cobertura de testes** expandida (unitários e de integração)
- ✅ **Logging estruturado** com Serilog
- ✅ **Módulo de gestão de usuários** para administradores

---

## 🔍 Visão Geral

O **FIAP Cloud Games** é estruturado em camadas bem definidas, seguindo os princípios da Clean Architecture e padrão CQRS:

```
┌─────────────────────────────────────────────┐
│         GameStore.API (Presentation)        │
│   Controllers, Middleware, Configuration    │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│      GameStore.Application (Use Cases)      │
│   Commands/Queries, Handlers, Validators    │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│       GameStore.Domain (Core/Entities)      │
│  Entities, Value Objects, Interfaces        │
└────────────────▲────────────────────────────┘
                 │
┌────────────────┴────────────────────────────┐
│   GameStore.Infrastructure (External)       │
│  Database, Repositories, External Services  │
└─────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│   GameStore.CrossCutting (Cross-Cutting)    │
│  DI, Localization, Logging, Swagger Config  │
└─────────────────────────────────────────────┘
```

### 📦 Componentes Principais

| Camada | Responsabilidade | Dependências |
|--------|------------------|--------------|
| **API** | Apresentação, Controllers, Middlewares, Models | Application, Infrastructure, CrossCutting |
| **Application** | Casos de uso, Commands/Queries, Handlers, Validações | Domain |
| **Domain** | Entidades, Regras de negócio, Contratos | Nenhuma (núcleo) |
| **Infrastructure** | Persistência, Repositórios, Seeders, Implementações de serviços | Domain |
| **CrossCutting** | Dependency Injection, Localização, Logging, Swagger | Todas as camadas |
| **Tests** | Testes unitários e de integração | Todas as camadas |

---

## 🏗️ Princípios Arquiteturais

O projeto foi construído seguindo princípios sólidos de engenharia de software:

### 1. **Clean Architecture**
- **Separação de responsabilidades** em camadas distintas
- **Dependências unidirecionais** (sempre apontando para o domínio)
- **Independência de frameworks** na camada de domínio
- **Testabilidade** em todos os níveis

### 2. **Domain-Driven Design (DDD)**
- **Entidades ricas** com comportamento e validações
- **Value Objects** para conceitos de domínio
- **Agregados** para manter consistência de dados
- **Repositórios** como abstração de persistência

### 3. **SOLID Principles**
- **S**ingle Responsibility: Cada classe tem uma única responsabilidade
- **O**pen/Closed: Extensível sem modificação
- **L**iskov Substitution: Interfaces bem definidas
- **I**nterface Segregation: Contratos específicos por necessidade
- **D**ependency Inversion: Dependência de abstrações, não de implementações

### 4. **CQRS (Command Query Responsibility Segregation)**
- **Separação de Commands e Queries** para operações de escrita e leitura
- **Mediator Pattern** para desacoplamento entre Controllers e Handlers
- **Organização por Features** para agrupar casos de uso relacionados
- **Pipeline Behaviors** para validação automática e cross-cutting concerns

### 5. **Result Pattern**
- **ApplicationResult** para padronização de respostas
- Tratamento consistente de sucesso e falhas
- Erros organizados por campo para facilitar tratamento no frontend
- Compatibilidade com diferentes tipos de retorno

### 6. **Separation of Concerns**
- Lógica de negócio isolada da infraestrutura
- Validações no domínio e através de FluentValidation
- Requests/Responses para transferência de dados entre camadas
- Mapeamento explícito de responsabilidades

### 7. **Dependency Injection**
- Injeção de dependência nativa do .NET
- Registro modular por camada (`AddApplication()`, `AddInfrastructure()`)
- Módulos especializados em CrossCutting para organização
- Gerenciamento automático de ciclo de vida

---

## 🎯 Padrão CQRS e Organização por Features

O projeto implementa o padrão **CQRS (Command Query Responsibility Segregation)** utilizando **Mediator**, organizando o código por **Features** que representam contextos de domínio.

### **Estrutura de uma Feature**

Cada feature é organizada da seguinte forma:

```
Features/{Feature}/
├── UseCases/
│   └── {UseCase}/
│       ├── {Command/Query}.cs          # Comando ou Query
│       ├── {Command/Query}Handler.cs   # Handler que processa
│       ├── {Command/Query}Validator.cs # Validações com FluentValidation
│       ├── {Request}.cs                # DTO de entrada (API → Handler)
│       └── {Response}.cs               # DTO de saída (Handler → API)
├── Mappings/                            # Mapeamentos Mapster (opcional)
└── Shared/                              # Modelos compartilhados
```

### **Como Funciona**

1. **Controller** recebe HTTP Request e cria um **Command/Query**
2. **Controller** envia para **Mediator** via `_mediator.Send(command)`
3. **Mediator** localiza automaticamente o **Handler** correspondente
4. **ValidationBehavior** executa validações antes do Handler (pipeline)
5. **Handler** processa a lógica de negócio e retorna **ApplicationResult**
6. **Controller** converte **ApplicationResult** em resposta HTTP apropriada

### **Benefícios**

- ✅ **Desacoplamento**: Controllers não conhecem Handlers diretamente
- ✅ **Organização**: Cada caso de uso em seu próprio namespace
- ✅ **Testabilidade**: Fácil testar Handlers isoladamente
- ✅ **Escalabilidade**: Fácil adicionar novos casos de uso sem modificar existentes
- ✅ **Validação Automática**: Pipeline behavior valida todos os Commands/Queries

### **Exemplo Prático**

```csharp
// Controller
[HttpPost("register")]
public async Task<ActionResult<Guid?>> Register([FromBody] RegisterUserRequest request)
{
    var command = new RegisterUserCommand(request.Name, request.Email, request.Username, request.Password);
    var result = await _mediator.Send(command);
    return ToActionResult(result);
}

// Command
public sealed record RegisterUserCommand(
    string Name, string Email, string Username, string Password) 
    : IRequest<ApplicationResult<Guid?>>;

// Handler
public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApplicationResult<Guid?>>
{
    // Lógica de negócio aqui
}
```

---

## 📊 ApplicationResult Pattern

O projeto utiliza o **Result Pattern** através de `ApplicationResult<T>` para padronizar respostas e tratamento de erros.

### **Características**

- **Tipos de Resultado:**
  - `ApplicationResult<T>` - Para operações que retornam dados
  - `ApplicationResult` - Para operações sem retorno de dados

- **Propriedades:**
  - `IsSuccess` / `IsFailure` - Indica sucesso ou falha
  - `Message` - Mensagem descritiva
  - `Data` - Dados retornados (quando aplicável)
  - `Errors` - Lista de erros (compatibilidade retroativa)
  - `FieldErrors` - Erros organizados por campo (novo formato)

### **Métodos Estáticos**

```csharp
// Sucesso
ApplicationResult<T>.Success(data, message)

// Falha
ApplicationResult<T>.Failure(message)

// Falha de validação (por campo)
ApplicationResult<T>.ValidationFailure(fieldErrors, message)
ApplicationResult<T>.ValidationFailure(fieldName, error, message)
```

### **Benefícios**

- ✅ **Consistência**: Todas as respostas seguem o mesmo padrão
- ✅ **Tradução**: Mensagens são traduzidas na camada de apresentação
- ✅ **Erros por Campo**: Facilita tratamento no frontend
- ✅ **Type Safety**: Tipagem forte para dados de retorno

---

## 🌍 Sistema de Localização

O projeto implementa um sistema de **localização multi-idioma** suportando **pt-BR** e **en-US**.

### **Como Funciona**

1. **Recursos** armazenados em arquivos `.resx` em `CrossCutting/Resources/`
2. **Tradução** acontece na camada de **apresentação** (BaseController)
3. **Chaves** são definidas nos Handlers e validators
4. **Contexto** da requisição determina o idioma (Accept-Language header)

### **Estrutura**

```
CrossCutting/Resources/
├── SharedResource.pt-BR.resx  # Traduções em português
└── SharedResource.en-US.resx  # Traduções em inglês
```

### **Uso em Controllers**

```csharp
public class AuthController : BaseController
{
    // Tradução automática via BaseController
    return BadRequest(new {
        message = TranslatedMessage(result.Message),
        errors = FormatErrors(result)
    });
}
```

---

## 📁 Estrutura de Pastas

```
FIAP-Cloud-Games/
│
├── 📄 GameStore.sln                    # Solução principal
├── 📄 GameStore.slnx                   # Arquivo de solução alternativo
├── 📄 global.json                      # Definição de versão do SDK .NET
├── 📄 LICENSE.txt                      # Licença do projeto
├── 📄 README.md                        # Este arquivo
│
├── 🎯 GameStore.API/                   # Camada de Apresentação
│   ├── Controllers/                    # Endpoints REST
│   │   ├── BaseController.cs           # Controller base com helpers
│   │   ├── AuthController.cs           # Autenticação e registro
│   │   ├── GamesController.cs          # CRUD de jogos
│   │   └── UsersController.cs          # CRUD de usuários (Admin)
│   ├── Models/                         # Modelos de resposta da API
│   │   └── Responses/
│   │       ├── ErrorResponse.cs
│   │       ├── SuccessResponse.cs
│   │       └── ValidationErrorResponse.cs
│   ├── Authorization/                  # Políticas de autorização customizadas
│   │   ├── ConfirmedAdminHandler.cs
│   │   ├── ConfirmedAdminRequirement.cs
│   │   ├── ConfirmedCommonUserHandler.cs
│   │   └── ConfirmedCommonUserRequirement.cs
│   ├── Middleware/                     # Middlewares customizados
│   │   ├── CorrelationIdMiddleware.cs  # Rastreamento de requisições
│   │   └── ExceptionHandlingMiddleware.cs  # Tratamento de exceções
│   ├── Database/                       # Banco de dados SQLite
│   │   └── gamestore.db
│   ├── logs/                           # Logs da aplicação (Serilog)
│   ├── Properties/
│   │   └── launchSettings.json         # Configurações de execução
│   ├── Program.cs                      # Bootstrapping da aplicação
│   ├── appsettings.json                # Configurações gerais
│   └── appsettings.Development.json    # Configurações de desenvolvimento
│
├── 💼 GameStore.Application/           # Camada de Aplicação (CQRS)
│   ├── Features/                       # Features organizadas por domínio
│   │   ├── Auth/                       # Módulo de autenticação
│   │   │   └── UseCases/
│   │   │       ├── Login/
│   │   │       ├── RegisterUser/
│   │   │       ├── SendAccountConfirmation/
│   │   │       └── ValidationAccount/
│   │   ├── Games/                      # Módulo de jogos
│   │   │   ├── Mappings/
│   │   │   ├── Shared/
│   │   │   └── UseCases/
│   │   │       ├── CreateGame/
│   │   │       ├── DeleteGame/
│   │   │       ├── GetAllGames/
│   │   │       ├── GetGameById/
│   │   │       └── UpdateGame/
│   │   └── Users/                      # Módulo de usuários
│   │       ├── Mappings/
│   │       ├── Shared/
│   │       └── UseCases/
│   │           ├── CreateUser/
│   │           ├── DeleteUser/
│   │           ├── GetAllUsers/
│   │           ├── GetUserById/
│   │           └── UpdateUser/
│   ├── Common/                         # Componentes comuns
│   │   ├── Exceptions/
│   │   ├── Mappings/
│   │   └── Results/
│   │       └── ApplicationResult.cs    # Result Pattern
│   ├── Behaviors/                      # Pipeline behaviors
│   │   └── ValidationBehavior.cs       # Validação automática
│   └── Services/                       # Interfaces de serviços
│       ├── IEmailService.cs
│       ├── IEncriptService.cs
│       └── IJwtService.cs
│
├── 🔷 GameStore.Domain/                # Camada de Domínio (Core)
│   ├── Entities/                       # Entidades de domínio
│   │   ├── BaseEntity.cs               # Entidade base (Id, timestamps)
│   │   ├── User.cs                     # Entidade de usuário
│   │   └── Game.cs                     # Entidade de jogo
│   ├── Enums/                          # Enumerações do domínio
│   │   ├── AccountStatus.cs            # Status da conta (Pending, Confirmed, Banned)
│   │   └── ProfileType.cs              # Tipo de perfil (CommonUser, Admin)
│   └── Repositories/                   # Contratos de repositórios
│       ├── IGameRepository.cs
│       ├── IUserRepository.cs
│       └── Abstractions/
│           └── IUnitOfWork.cs
│
├── 🗄️ GameStore.Infrastructure/        # Camada de Infraestrutura
│   ├── Data/                           # Contexto e configurações EF Core
│   │   ├── GameStoreContext.cs         # DbContext principal
│   │   ├── Configurations/             # Fluent API configurations
│   │   ├── Seeders/                    # Seeders de dados iniciais
│   │   │   ├── Abstractions/
│   │   │   │   └── IDataSeeder.cs
│   │   │   ├── Users/
│   │   │   │   └── UserSeeder.cs       # Seeder de usuário admin
│   │   │   └── DataSeederOrchestrator.cs
│   │   └── Initialization/             # Inicialização do banco
│   ├── Migrations/                     # Migrations do EF Core
│   ├── Repositories/                   # Implementações de repositórios
│   │   ├── Games/
│   │   │   └── GameRepository.cs
│   │   ├── Users/
│   │   │   └── UserRepository.cs
│   │   └── Abstractions/
│   │       └── UnitOfWork.cs
│   └── Services/                       # Implementações de serviços
│       ├── EmailService.cs
│       ├── EncriptService.cs
│       └── JwtService.cs
│
├── 🔧 GameStore.CrossCutting/          # Camada de Concerns Transversais
│   ├── DependencyInjection/           # Módulos de DI
│   │   ├── Application/
│   │   │   └── ApplicationModule.cs    # Registro de Application
│   │   ├── Infrastructure/
│   │   │   └── InfrastructureModule.cs # Registro de Infrastructure
│   │   ├── AuthModule.cs               # Configuração JWT
│   │   ├── LoggingModule.cs            # Configuração Serilog
│   │   └── SwaggerModule.cs             # Configuração Swagger
│   ├── Localization/                   # Sistema de localização
│   │   ├── ITranslationService.cs
│   │   ├── LocalizationDependencyInjection.cs
│   │   ├── LocalizationSettings.cs
│   │   └── TranslationService.cs
│   ├── Resources/                      # Arquivos de recursos
│   │   ├── SharedResource.en-US.resx
│   │   └── SharedResource.pt-BR.resx
│   └── SharedResource.cs               # Classe para localização
│
└── 🧪 GameStore.Tests/                 # Camada de Testes
    ├── API/                            # Testes de controllers e middleware
    │   ├── Authorization/
    │   └── Middleware/
    ├── Application/                    # Testes de handlers
    │   └── Features/
    │       ├── Auth/
    │       ├── Games/
    │       └── Users/
    ├── Infrastructure/                 # Testes de repositórios
    │   └── Repositories/
    ├── TestUtils/                      # Utilitários de teste
    └── Usings.cs                       # Usings globais para testes
```

### 🎯 Objetivo de Cada Camada

#### **GameStore.API** (Apresentação)
- **O que faz:** Ponto de entrada da aplicação, expõe endpoints REST
- **Como funciona:** Recebe requisições HTTP, valida JWT, aplica autorização e delega para Application
- **O que pode conter:**
  - Controllers (endpoints)
  - Middlewares (CorrelationId, Exception Handling)
  - Políticas de autorização customizadas
  - Configuração de Swagger/OpenAPI
  - Bootstrapping e configuração de DI

#### **GameStore.Application** (Casos de Uso - CQRS)
- **O que faz:** Orquestra a lógica de negócio através de Commands e Queries
- **Como funciona:** Implementa padrão CQRS com Mediator, organizando casos de uso por Features
- **O que pode conter:**
  - **Features/** organizadas por domínio (Auth, Games, Users)
  - **Commands/Queries** para operações de escrita e leitura
  - **Handlers** que processam Commands/Queries
  - **Validators** usando FluentValidation
  - **Requests/Responses** para transferência de dados
  - **ApplicationResult** para padronização de respostas
  - **Behaviors** para pipeline (validação automática)
  - **Mappings** usando Mapster
  - Interfaces de serviços (implementadas em Infrastructure)

#### **GameStore.Domain** (Núcleo)
- **O que faz:** Define o modelo de domínio e regras de negócio puras
- **Como funciona:** Entidades ricas com comportamento, sem dependências externas
- **O que pode conter:**
  - Entidades (User, Game)
  - Value Objects
  - Enums (AccountStatus, ProfileType)
  - Interfaces de repositórios (contratos)
  - Exceções de domínio
  - **NÃO** contém dependências de frameworks ou bibliotecas externas

#### **GameStore.Infrastructure** (Persistência e Serviços Externos)
- **O que faz:** Implementa detalhes técnicos de infraestrutura
- **Como funciona:** Implementa repositórios, gerencia banco de dados, seeders e serviços
- **O que pode conter:**
  - DbContext (Entity Framework Core)
  - Implementações de repositórios
  - Implementações de serviços (EmailService, JwtService, EncriptService)
  - Configurações Fluent API
  - Migrations
  - Seeders de dados
  - Integrações com serviços externos

#### **GameStore.CrossCutting** (Concerns Transversais)
- **O que faz:** Centraliza configurações e serviços transversais à aplicação
- **Como funciona:** Módulos especializados para diferentes aspectos (DI, Localization, Logging, Swagger)
- **O que pode conter:**
  - **DependencyInjection/** módulos para registro de serviços por camada
  - **Localization/** sistema de tradução multi-idioma
  - **Resources/** arquivos .resx com traduções (pt-BR, en-US)
  - Configurações compartilhadas entre camadas

#### **GameStore.Tests** (Testes Automatizados)
- **O que faz:** Garante qualidade e funcionamento correto do sistema
- **Como funciona:** Testes unitários e de integração usando xUnit, NSubstitute, Moq e EF InMemory
- **O que pode conter:**
  - Testes de Command/Query Handlers (Application/Features)
  - Testes de Validators (FluentValidation)
  - Testes de repositórios (Infrastructure)
  - Testes de controllers (API)
  - Testes de middleware
  - Fixtures e utilitários de teste (TestUtils)

---

## 🛠️ Tecnologias e Ferramentas

### **Framework e Runtime**
- **.NET 9.0** - Framework principal
- **C# 12** - Linguagem de programação
- **ASP.NET Core 9.0** - Web API framework

### **Banco de Dados e ORM**
- **SQLite** - Banco de dados relacional leve
- **Entity Framework Core 9.0** - ORM (Object-Relational Mapping)
- **EF Core Design** - Ferramentas de design-time para migrations
- **EF Core Tools** - CLI para gerenciamento de banco de dados

### **Autenticação e Segurança**
- **JWT (JSON Web Tokens)** - Autenticação stateless
- **BCrypt.Net-Next** - Hashing seguro de senhas
- **Microsoft.AspNetCore.Authentication.JwtBearer** - Middleware JWT

### **Logging e Observabilidade**
- **Serilog** - Logging estruturado
- **Serilog.AspNetCore** - Integração com ASP.NET Core
- **Serilog.Sinks.Console** - Output para console
- **Serilog.Sinks.File** - Output para arquivo (rolling logs)

### **Testes**
- **xUnit** - Framework de testes unitários
- **NSubstitute** - Biblioteca principal de mocking (substituição de dependências)
- **Moq** - Biblioteca alternativa de mocking (usada em alguns testes específicos)
- **EF Core InMemory** - Provider em memória para testes de integração
- **coverlet.collector** - Coleta de cobertura de código nos testes

### **Documentação**
- **Swagger/OpenAPI** - Documentação interativa da API
- **Swashbuckle.AspNetCore** - Geração automática de documentação

### **Ferramentas de Desenvolvimento**
- **Visual Studio 2022** / **VS Code** - IDEs
- **Git** - Controle de versão
- **PowerShell** - Scripts e automação

### **Padrões e Bibliotecas**
- **Mediator.Abstractions** - Implementação de CQRS e mediação
- **FluentValidation** - Validações fluentes e expressivas
- **FluentValidation.DependencyInjectionExtensions** - Extensões para DI
- **Mapster** - Mapeamento objeto-objeto de alta performance
- **Mapster.DependencyInjection** - Extensões para DI do Mapster

---

## 💼 Módulos de Negócio

### 1. **Módulo de Autenticação e Autorização**

#### Funcionalidades
- ✅ **Registro de usuários** com validação de e-mail único
- ✅ **Login** com geração de JWT
- ✅ **Envio de código de confirmação** por email
- ✅ **Validação de conta** via código de confirmação
- ✅ **Hashing de senhas** com BCrypt (10 rounds)
- ✅ **Gestão de perfis**: CommonUser e Admin
- ✅ **Gestão de status de conta**: Pending, Confirmed, Banned

#### Regras de Negócio
- Novos usuários começam com status `Pending`
- Senhas são sempre hasheadas antes de persistir
- E-mail e username devem ser únicos
- JWT expira em 60 minutos (configurável)
- Códigos de confirmação têm tempo de expiração

#### Entidades Envolvidas
- `User` (Id, Name, Email, Username, PasswordHash, ProfileType, AccountStatus)

#### Commands e Queries
- `RegisterUserCommand` - Registrar novo usuário
- `LoginCommand` - Autenticar usuário e gerar JWT
- `SendAccountConfirmationCommand` - Enviar código de confirmação
- `ValidationAccountCommand` - Validar conta com código

#### Serviços Utilizados
- `IJwtService` - Geração e validação de tokens JWT
- `IEmailService` - Envio de emails
- `IEncriptService` - Criptografia de códigos de confirmação

---

### 2. **Módulo de Gestão de Jogos**

#### Funcionalidades
- ✅ **Listagem de jogos** (todos os usuários confirmados)
- ✅ **Consulta por ID** (todos os usuários confirmados)
- ✅ **Criação de jogos** (somente Admins confirmados)
- ✅ **Atualização de jogos** (somente Admins confirmados)
- ✅ **Exclusão de jogos** (somente Admins confirmados)

#### Regras de Negócio
- Apenas usuários com `AccountStatus.Confirmed` podem acessar jogos
- Apenas usuários com `ProfileType.Admin` podem criar/editar/deletar
- Título do jogo é obrigatório
- Preço não pode ser negativo
- Data de lançamento é opcional

#### Entidades Envolvidas
- `Game` (Id, Title, Description, Price, Genre, ReleaseDate)

#### Commands e Queries
- `GetAllGamesQuery` - Listar todos os jogos
- `GetGameByIdQuery` - Obter jogo por ID
- `CreateGameCommand` - Criar novo jogo
- `UpdateGameCommand` - Atualizar jogo existente
- `DeleteGameCommand` - Excluir jogo

---

### 3. **Módulo de Gestão de Usuários**

#### Funcionalidades
- ✅ **Listagem de usuários** (somente Admins confirmados)
- ✅ **Consulta por ID** (somente Admins confirmados)
- ✅ **Criação de usuários** (somente Admins confirmados)
- ✅ **Atualização de usuários** (somente Admins confirmados)
- ✅ **Exclusão de usuários** (somente Admins confirmados)

#### Regras de Negócio
- Apenas usuários com `ProfileType.Admin` e `AccountStatus.Confirmed` podem gerenciar usuários
- Validações de e-mail e username únicos
- Senhas são sempre hasheadas antes de persistir
- Não é possível excluir o próprio usuário logado

#### Entidades Envolvidas
- `User` (Id, Name, Email, Username, PasswordHash, ProfileType, AccountStatus)

#### Commands e Queries
- `GetAllUsersQuery` - Listar todos os usuários
- `GetUserByIdQuery` - Obter usuário por ID
- `CreateUserCommand` - Criar novo usuário
- `UpdateUserCommand` - Atualizar usuário existente
- `DeleteUserCommand` - Excluir usuário

---

### 4. **Módulo de Rastreabilidade**

#### Funcionalidades
- ✅ **CorrelationId** em todas as requisições
- ✅ **Logging estruturado** com contexto de requisição
- ✅ **Logs persistidos em arquivo** (rolling daily)
- ✅ **Tratamento centralizado de exceções** via ExceptionHandlingMiddleware

#### Como Funciona
1. Middleware `CorrelationIdMiddleware` intercepta requisição
2. Gera ou extrai `X-Correlation-Id` do header
3. Injeta no contexto HTTP
4. Logger inclui CorrelationId em todos os logs
5. `ExceptionHandlingMiddleware` captura exceções e retorna respostas padronizadas
6. Response retorna o mesmo CorrelationId

---

## 🔧 Camada CrossCutting

A camada **GameStore.CrossCutting** centraliza **concerns transversais** que são utilizados por múltiplas camadas da aplicação. Esta organização facilita a manutenção e evolução desses aspectos compartilhados.

### **Módulos Principais**

#### **1. DependencyInjection**
Módulos especializados para registro de serviços por camada:

- **ApplicationModule** - Registra Mediator, FluentValidation, Mapster
- **InfrastructureModule** - Registra DbContext, Repositórios, Seeders, Serviços
- **AuthModule** - Configura autenticação e autorização JWT
- **LoggingModule** - Configura Serilog com sinks (Console, File)
- **SwaggerModule** - Configura Swagger/OpenAPI com segurança JWT

#### **2. Localization**
Sistema de localização multi-idioma:

- **ITranslationService** / **TranslationService** - Serviço de tradução
- **LocalizationSettings** - Configurações de idioma padrão
- **Resources/** - Arquivos `.resx` com traduções (pt-BR, en-US)
- **SharedResource** - Classe marcadora para localização

#### **3. Benefícios**
- ✅ **Organização**: Concerns transversais em um único lugar
- ✅ **Reutilização**: Configurações compartilhadas entre camadas
- ✅ **Manutenção**: Fácil localizar e atualizar configurações
- ✅ **Testabilidade**: Fácil mockar serviços transversais em testes

---

## 🗄️ Estratégia de Banco de Dados

### **Abordagem: Code-First com Entity Framework Core**

O projeto utiliza a abordagem **Code-First**, onde as entidades de domínio são definidas em C# e o banco de dados é gerado automaticamente a partir delas.

### **Banco de Dados: SQLite**

**Por que SQLite?**
- ✅ Zero configuração para desenvolvimento local
- ✅ Arquivo único (`gamestore.db`)
- ✅ Portável entre ambientes
- ✅ Suficiente para aplicação de porte médio
- ✅ Facilita testes e demonstrações

**Para produção:** Substituir por PostgreSQL, SQL Server ou MySQL com mínima alteração de código.

### **Configuração de Entidades**

As configurações são aplicadas via **Fluent API** em classes dedicadas:

```
GameStore.Infrastructure/Data/Configurations/
├── UserConfiguration.cs      # Configuração da entidade User
└── GameConfiguration.cs      # Configuração da entidade Game
```

**Características:**
- Primary keys configuradas
- Índices únicos (Email, Username)
- Relacionamentos definidos
- Restrições de campo (Required, MaxLength)
- Conversões de enums

### **Migrations**

Gerenciamento de evolução do schema:

```powershell
# Criar nova migration
dotnet ef migrations add NomeDaMigracao --project GameStore.Infrastructure --startup-project GameStore.API

# Aplicar migrations ao banco
dotnet ef database update --project GameStore.Infrastructure --startup-project GameStore.API

# Reverter migration
dotnet ef migrations remove --project GameStore.Infrastructure --startup-project GameStore.API
```

### **Seeders (Dados Iniciais)**

O projeto utiliza um sistema de **seeders orquestrados** para popular dados iniciais:

#### **Arquitetura de Seeders**

```
IDataSeeder (Interface)
    ↓
DataSeederOrchestrator (Orquestrador)
    ↓
UserSeeder (Implementação)
```

#### **UserSeeder**
Cria usuário administrador padrão:
- **Email:** `admin@gamestore.com`
- **Username:** `admin`
- **Senha:** `Admin@123` (hasheada)
- **Perfil:** Admin
- **Status:** Confirmed

#### **Como Funciona**
1. `Program.cs` registra seeders no DI
2. Durante inicialização, `DataSeederOrchestrator` é executado
3. Migrations são aplicadas automaticamente
4. Seeders são executados em ordem
5. Dados são criados somente se não existirem (idempotente)

### **Unit of Work Pattern**

Coordenação de transações entre repositórios:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

**Benefícios:**
- ✅ Controle explícito de transações
- ✅ Atomicidade de operações
- ✅ Isolamento de lógica de persistência

---

## 🧪 Estratégia de Testes

### **Filosofia de Testes**

O projeto adota uma estratégia de **pirâmide de testes**, priorizando:

```
           /\
          /  \  E2E (Poucos)
         /    \
        /------\  Integration (Médio)
       /        \
      /----------\  Unit (Muitos)
     /______________\
```

### **Frameworks e Bibliotecas**

- **xUnit** - Framework de testes (convenção .NET)
- **NSubstitute** - Biblioteca principal de mocking (Substitute.For<T>)
- **Moq** - Biblioteca alternativa de mocking (usada em alguns testes específicos)
- **EF Core InMemory** - Banco em memória para testes de repositório
- **coverlet.collector** - Coleta de métricas de cobertura de código

### **Categorias de Testes**

#### **1. Testes Unitários (Unit Tests)**

**Objetivo:** Testar componentes isolados sem dependências externas

**Localização:** `GameStore.Tests/Application/Features/`

**Escopo:**
- Command/Query Handlers (RegisterUserCommandHandler, LoginCommandHandler, etc.)
- Validators com FluentValidation
- Lógica de negócio isolada
- Validações de entrada
- Tratamento de erros e ApplicationResult

**Exemplo de Cenários:**
- ✅ `RegisterUserCommandHandler` com e-mail duplicado deve retornar ValidationFailure
- ✅ `LoginCommandHandler` com senha incorreta deve retornar Failure
- ✅ `CreateGameCommandHandler` sem permissão Admin deve falhar
- ✅ Validators devem rejeitar dados inválidos antes do Handler

**Técnicas:**
- **Mocking** de repositórios e serviços com NSubstitute (Substitute.For<T>)
- **Arrange-Act-Assert** pattern
- **Testes parametrizados** (Theory/InlineData)
- **Fixtures** para dados de teste
- **Verificação de ApplicationResult** (IsSuccess, Message, FieldErrors)
- **Asserções xUnit** padrão (Assert.True, Assert.Equal, Assert.NotNull)

---

#### **2. Testes de Integração (Integration Tests)**

**Objetivo:** Testar interação entre componentes reais (com banco em memória)

**Localização:** `GameStore.Tests/Infrastructure/Repositories/`

**Escopo:**
- Repositórios + EF Core
- Operações de persistência
- Queries complexas
- Validações de restrições de banco

**Exemplo de Cenários:**
- ✅ `UserRepository.AddAsync` deve persistir usuário corretamente
- ✅ `GameRepository.GetByIdAsync` deve retornar jogo existente
- ✅ `UnitOfWork.SaveChangesAsync` deve aplicar transações
- ✅ Índice único de e-mail deve prevenir duplicação

**Técnicas:**
- **InMemory Database Provider** (EF Core)
- **DbContext** isolado por teste
- **Transações de teste** (rollback automático)

---

#### **3. Testes de Middleware**

**Objetivo:** Validar comportamento de middlewares customizados

**Localização:** `GameStore.Tests/API/Middleware/`

**Escopo:**
- `CorrelationIdMiddleware`
- Propagação de headers
- Logging contextual

**Exemplo de Cenários:**
- ✅ Middleware deve gerar CorrelationId se ausente
- ✅ Middleware deve preservar CorrelationId do request
- ✅ Response deve incluir header `X-Correlation-Id`

---

#### **4. Testes de Autorização**

**Objetivo:** Validar políticas de autorização customizadas

**Localização:** `GameStore.Tests/API/Authorization/`

**Escopo:**
- `ConfirmedCommonUserHandler`
- `ConfirmedAdminHandler`
- Validação de claims JWT

**Exemplo de Cenários:**
- ✅ Handler deve autorizar usuário confirmado
- ✅ Handler deve negar usuário pendente
- ✅ Handler deve validar perfil Admin

---

### **Cobertura de Testes (Overview)**

| Camada | Tipo | Foco | Quantidade Aproximada |
|--------|------|------|----------------------|
| Application | Unitário | Command/Query Handlers, Validators | ~50 testes |
| Infrastructure | Integração | Repositórios, UoW | ~20 testes |
| API | Middleware | CorrelationId, Exception Handling | ~8 testes |
| API | Autorização | Handlers customizados | ~10 testes |

**Meta de Cobertura:** 70%+ de cobertura de código nas camadas críticas (Application e Domain)

---

### **Boas Práticas Adotadas**

1. ✅ **AAA Pattern** (Arrange-Act-Assert)
2. ✅ **Testes independentes** (sem ordem de execução)
3. ✅ **Nomes descritivos** (MethodName_Scenario_ExpectedBehavior)
4. ✅ **One assertion per test** (quando possível)
5. ✅ **Fixtures reutilizáveis** para dados de teste
6. ✅ **Cleanup automático** (Dispose de contextos)

---

### **Executando os Testes**

```powershell
# Executar todos os testes
dotnet test GameStore.sln

# Executar com detalhamento
dotnet test GameStore.sln --logger "console;verbosity=detailed"

# Executar testes de uma categoria específica
dotnet test --filter "FullyQualifiedName~Application"

# Gerar relatório de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 🚀 Como Executar o Projeto

### **Pré-requisitos**

Certifique-se de ter instalado:

1. **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
   ```powershell
   dotnet --version  # Deve retornar 9.0.x
   ```

2. **Git** - [Download](https://git-scm.com/downloads)

3. **Editor de código** (Visual Studio 2022, VS Code ou JetBrains Rider)

---

### **Passo 1: Clonar o Repositório**

```powershell
# Clone o repositório
git clone https://github.com/thefenixdevs/FIAP-Cloud-Games.git

# Navegue até o diretório
cd FIAP-Cloud-Games
```

---

### **Passo 2: Restaurar Dependências**

```powershell
# Restaurar pacotes NuGet
dotnet restore GameStore.sln
```

---

### **Passo 3: Aplicar Migrations (Criar Banco de Dados)**

```powershell
# Executar migrations para criar o banco SQLite
dotnet ef database update --project GameStore.Infrastructure --startup-project GameStore.API
```

**Nota:** O banco será criado em `GameStore.API/Database/gamestore.db`

---

### **Passo 4: Executar a Aplicação**

```powershell
# Executar a API
dotnet run --project GameStore.API/GameStore.API.csproj
```

**Saída esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

---

### **Passo 5: Acessar o Swagger UI**

Abra seu navegador e acesse:

```
https://localhost:7001/swagger
```

Você verá a documentação interativa da API com todos os endpoints disponíveis.

---

### **Passo 6: Testar a API**

#### **1. Registrar um novo usuário**

```http
POST https://localhost:7001/api/auth/register
Content-Type: application/json

{
  "email": "usuario@example.com",
  "username": "usuario",
  "password": "Senha@123"
}
```

#### **2. Login (obter JWT)**

```http
POST https://localhost:7001/api/auth/login
Content-Type: application/json

{
  "email": "admin@gamestore.com",
  "password": "Admin@123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-08T15:30:00Z"
}
```

#### **3. Listar jogos (com autenticação)**

```http
GET https://localhost:7001/api/games
Authorization: Bearer SEU_TOKEN_AQUI
```

---

### **Executar Testes**

```powershell
# Executar todos os testes
dotnet test GameStore.sln

# Executar com relatório detalhado
dotnet test GameStore.sln --logger "console;verbosity=detailed"
```

---

## ⚙️ Configuração

### **appsettings.json**

Localização: `GameStore.API/appsettings.json`

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

### **Configurações Importantes**

| Configuração | Descrição | Valor Padrão |
|--------------|-----------|--------------|
| `ConnectionStrings:DefaultConnection` | String de conexão SQLite | `Data Source=Database\\gamestore.db` |
| `Jwt:SecretKey` | Chave secreta para assinar JWT | (Alterar em produção!) |
| `Jwt:Issuer` | Emissor do token | `GameStore` |
| `Jwt:Audience` | Audiência do token | `GameStoreApiUsers` |
| `Jwt:ExpirationInMinutes` | Tempo de expiração do token | `60` minutos |

**⚠️ IMPORTANTE:** Em produção:
- Altere `Jwt:SecretKey` para uma chave forte (min. 32 caracteres)
- Use **variáveis de ambiente** ou **Azure Key Vault** para secrets
- Configure HTTPS com certificado válido

---

## 🔌 Endpoints da API

### **Autenticação**

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| POST | `/api/auth/register` | Registrar novo usuário | Não requerida |
| POST | `/api/auth/login` | Autenticar e obter JWT | Não requerida |
| POST | `/api/auth/sendConfirmation` | Enviar código de confirmação por email | Não requerida |
| GET | `/api/auth/validationAccount` | Validar conta com código de confirmação | Não requerida |

### **Jogos**

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/games` | Listar todos os jogos | ConfirmedCommonUser |
| GET | `/api/games/{id}` | Obter jogo por ID | ConfirmedCommonUser |
| POST | `/api/games` | Criar novo jogo | ConfirmedAdmin |
| PUT | `/api/games/{id}` | Atualizar jogo | ConfirmedAdmin |
| DELETE | `/api/games/{id}` | Excluir jogo | ConfirmedAdmin |

### **Usuários**

| Método | Endpoint | Descrição | Autorização |
|--------|----------|-----------|-------------|
| GET | `/api/users` | Listar todos os usuários | ConfirmedAdmin |
| GET | `/api/users/{id}` | Obter usuário por ID | ConfirmedAdmin |
| POST | `/api/users` | Criar novo usuário | ConfirmedAdmin |
| PUT | `/api/users/{id}` | Atualizar usuário | ConfirmedAdmin |
| DELETE | `/api/users/{id}` | Excluir usuário | ConfirmedAdmin |

---

## 🚀 Publicação e Releases

### 📌 Versionamento e release 0.2.0
- Versionamento semântico centralizado em [`Directory.Build.props`](./Directory.Build.props) (`0.2.0`).
- Notas oficiais em [`RELEASE_NOTES.md`](./RELEASE_NOTES.md).
- Para gerar nova release:
  1. Garanta que a branch `master`/`main` esteja atualizada.
  2. Crie a tag semântica (`git tag v0.2.0 && git push origin v0.2.0`).
  3. A workflow [`release.yml`](.github/workflows/release.yml) cria a release, publica o artefato ZIP e reutiliza o conteúdo das notas.

### 🔄 Integração Contínua
- Workflow [`ci.yml`](.github/workflows/ci.yml) roda `dotnet restore ➜ build ➜ test` em todo push/pull request.
- Cobertura coletada via `XPlat Code Coverage` nos artefatos da execução.

### 🌐 Página de demonstração
- Conteúdo estático pronto em [`docs/`](./docs/) para uso com **GitHub Pages**.
- Para ativar: *Settings ▸ Pages ▸ Branch: `master` (ou `main`) /docs*.
- Página inclui instruções de execução, credenciais seedadas e links de download.
- Documentação Swagger pronta em [`docs/swagger/index.html`](./docs/swagger/index.html); após publicar via GitHub Pages, acesse `/swagger/` para navegar na UI interativa.

### ✅ Checklist pré-release sugerido
- [ ] Atualizar `RELEASE_NOTES.md` com mudanças recentes.
- [ ] Garantir que a pipeline de CI esteja verde.
- [ ] Revisar configurações sensíveis (`appsettings*.json`) antes da publicação.
- [ ] Se necessário, anexar scripts de migração ou dumps de banco na release.

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Para contribuir:

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto está sob a licença especificada no arquivo `LICENSE.txt`.

---

## 👥 Equipe

Projeto desenvolvido pela equipe **thefenixdevs** como parte do Tech Challenge FIAP.

---

## 📞 Contato

- **Repositório Original (PoC):** [TechChallengeGameStore](https://github.com/thefenixdevs/TechChallengeGameStore)
- **Repositório Atual:** [FIAP-Cloud-Games](https://github.com/thefenixdevs/FIAP-Cloud-Games)

---

<p align="center">
  Desenvolvido com ❤️ pela equipe <strong>thefenixdevs</strong>
</p>