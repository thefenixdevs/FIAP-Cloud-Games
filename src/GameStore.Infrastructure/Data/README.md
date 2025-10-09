# Estrutura de Inicialização do Banco de Dados

## 📁 Organização de Pastas

```
GameStore.Infrastructure/
├── Data/
│   ├── Initialization/           # Inicialização do banco (migrations + seeding)
│   │   └── DatabaseInitializationExtensions.cs
│   ├── Seeders/                  # Seeders de dados
│   │   ├── Abstractions/         # Contratos para seeders
│   │   │   ├── IDataSeeder.cs
│   │   │   └── IOrderedDataSeeder.cs
│   │   ├── Users/                # Seeders agrupados por contexto
│   │   │   └── UserSeeder.cs
│   │   └── DataSeederOrchestrator.cs  # Orquestrador de seeders
│   └── ...
├── DependencyInjection.cs        # Registro de serviços da Infrastructure
└── ...
```

## 🎯 Responsabilidades

### 1. **DatabaseInitializationExtensions**
- **Responsabilidade**: Inicializar o banco de dados na startup
- **Ações**:
  - Aplica migrations pendentes
  - Executa seeders através do orquestrador
  - Provê logging centralizado do processo
- **Uso**: `await app.Services.InitializeDatabaseAsync();`

### 2. **DataSeederOrchestrator**
- **Responsabilidade**: Orquestrar a execução de múltiplos seeders
- **Ações**:
  - Ordena seeders por prioridade (via `IOrderedDataSeeder`)
  - Executa seeders sequencialmente
  - Gerencia erros e logging de cada seeder
- **Padrão**: Orchestrator Pattern

### 3. **IDataSeeder / IOrderedDataSeeder**
- **Responsabilidade**: Contratos para implementação de seeders
- **IDataSeeder**: Interface base para qualquer seeder
- **IOrderedDataSeeder**: Permite definir ordem de execução (propriedade `Order`)

### 4. **UserSeeder** (exemplo)
- **Responsabilidade**: Popular dados iniciais de usuários
- **Contexto**: Agrupado na pasta `Users/`
- **Ordem**: 0 (primeiro a executar)
- **Ações**: Cria usuário administrador padrão

### 5. **DependencyInjection**
- **Responsabilidade**: Registro centralizado de serviços da camada Infrastructure
- **Registra**:
  - DbContext
  - Repositories
  - Seeders e orquestrador

## ✅ Boas Práticas Aplicadas

1. **Separação de Responsabilidades (SRP)**
   - Inicialização separada de seeding
   - Orquestrador separado dos seeders concretos
   - Cada seeder com responsabilidade única

2. **Nomes Descritivos**
   - `DatabaseInitializationExtensions` (foco: inicialização)
   - `DataSeederOrchestrator` (foco: orquestração)
   - Não mais nomes genéricos como "DatabaseSeeder"

3. **Padrões de Design**
   - **Strategy Pattern**: IDataSeeder permite múltiplas implementações
   - **Orchestrator Pattern**: DataSeederOrchestrator coordena execução
   - **Extension Methods**: Simplifica chamada no Program.cs

4. **Organização por Contexto**
   - Seeders agrupados por domínio (Users/, Games/, etc.)
   - Abstrações em pasta separada
   - Facilita crescimento do projeto

5. **Dependency Injection**
   - Registro centralizado em DependencyInjection.cs
   - Program.cs limpo e focado no pipeline
   - Fácil substituição e teste

6. **Logging Estruturado**
   - Logs em cada etapa do processo
   - Facilita debugging e monitoramento
   - Informações contextuais (nome do seeder, etc.)

## 🚀 Fluxo de Execução

```
1. Program.cs
   ↓
2. app.Services.InitializeDatabaseAsync()
   ↓
3. DatabaseInitializationExtensions
   ├── MigrateAsync() → Aplica migrations
   └── DataSeederOrchestrator.SeedAsync()
       ├── OrderSeeders() → Ordena por prioridade
       └── Para cada seeder:
           └── seeder.SeedAsync() → Executa seeder específico
```

## 📝 Exemplo de Adição de Novo Seeder

```csharp
// 1. Criar seeder em Seeders/Games/GameSeeder.cs
public class GameSeeder : IOrderedDataSeeder
{
  public int Order => 1; // Executa após UserSeeder (0)
  
  public async Task SeedAsync(CancellationToken cancellationToken = default)
  {
    // Lógica de seeding de games
  }
}

// 2. Registrar em DependencyInjection.cs
services.AddScoped<IDataSeeder, GameSeeder>();

// Pronto! O orquestrador cuidará do resto.
```

## 🎓 Benefícios da Estrutura

- ✅ **Escalável**: Fácil adicionar novos seeders
- ✅ **Testável**: Componentes isolados e injetáveis
- ✅ **Manutenível**: Responsabilidades claras
- ✅ **Legível**: Nomes descritivos e organização lógica
- ✅ **Profissional**: Segue padrões da indústria
