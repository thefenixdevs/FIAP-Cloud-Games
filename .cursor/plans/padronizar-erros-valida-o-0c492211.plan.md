<!-- 0c492211-4320-4ccf-bed5-71c824919236 ebb2e2f9-2bab-4bf4-8ebb-d9e63d6f79c9 -->
# Implementação de ValidationErrors no Domain (Opção 1 - DDD)

## Objetivo

Criar uma **classe genérica** (sem sufixo Domain/Application) para acumular violações de validação, posicionada no **Domain** onde as regras de negócio residem. Isso segue DDD ao manter a estrutura de validação no núcleo do domínio, e a camada Application apenas a utiliza quando necessário converter para exceção.

## Arquitetura Proposta

### 1. Criar Classe Genérica de Validação no Domain

- **Arquivo**: `src/GameStore.Domain/Common/ValidationErrors.cs` (ou `ValidationResult.cs`)
- Nome genérico que não acopla a nenhuma camada específica
- Propriedade `Errors` do tipo `IReadOnlyDictionary<string, string[]>` (mesma estrutura)
- Classe imutável seguindo padrão de acumulação
- Métodos:
  - `Empty` / `Valid` - propriedade estática para resultado válido
  - `AddError(string field, string errorKey)` - adiciona uma violação
  - `AddErrors(string field, IEnumerable<string> errorKeys)` - adiciona múltiplas violações
  - `Merge(ValidationErrors other)` - combina dois resultados
  - `IsValid` - indica se há violações
  - Construtor que aceita `Dictionary<string, string[]>` diretamente

### 2. Mover/Adaptar ApplicationValidationException

- **Arquivo**: `src/GameStore.Application/Common/Exceptions/ApplicationValidationException.cs`
- Adicionar construtor que aceita `ValidationErrors` (a classe genérica do Domain):
  ```csharp
  public ApplicationValidationException(ValidationErrors validationErrors, string message = null)
  ```

- Usa `validationErrors.Errors` para obter o dicionário
- A exceção continua na Application (camada apropriada para exceções)
- A estrutura de dados vem do Domain (núcleo do negócio)

### 3. Modificar Value Objects

- **Arquivos**:
  - `src/GameStore.Domain/ValueObjects/Email.cs`
  - `src/GameStore.Domain/ValueObjects/Password.cs`
  - `src/GameStore.Domain/ValueObjects/Username.cs`
- Criar métodos `TryCreate` que retornam `(ValueObject?, ValidationErrors)`
- Usar a classe genérica `ValidationErrors` do Domain
- Acumular violações usando chaves de localização já existentes

### 4. Modificar Entidades

- **Arquivos**:
  - `src/GameStore.Domain/Entities/User.cs`
  - `src/GameStore.Domain/Entities/Game.cs`
- Modificar métodos factory para retornar `(Entity?, ValidationErrors)` ou acumular em `ValidationErrors` passado por referência
- Chamar `TryCreate` dos Value Objects e acumular violações
- Retornar `ValidationErrors` com todas as violações acumuladas

### 5. Atualizar Handlers

- **Arquivos**: Handlers em `src/GameStore.Application/Features/`
- Usar `ValidationErrors` do Domain
- Se houver violações, criar `ApplicationValidationException` passando o `ValidationErrors`
- Converter para `ApplicationResult.ValidationFailure` quando necessário

### 6. Integração com FluentValidation

- FluentValidation continua na Application (validações de entrada)
- Pode converter erros do FluentValidation para `ValidationErrors` quando necessário
- Ambos usam o mesmo formato `Dictionary<string, string[]>`

## Princípios DDD Aplicados

- **Ubiquidade da Linguagem**: Classe genérica que representa o conceito de "violações de validação" sem amarras de camada
- **Núcleo no Domain**: A estrutura de validação vive no Domain onde as regras residem
- **Separação de Responsabilidades**: Domain define estrutura, Application usa para exceções/respostas HTTP
- **Reutilização**: Mesma classe usada em todo o sistema sem duplicação
- **Independência de Camada**: A classe não sabe sobre Application, mas Application sabe sobre Domain (dependência correta)

## Estrutura Final

```
Domain/
  Common/
    ValidationErrors.cs  ← Classe genérica, estrutura de dados

Application/
  Common/
    Exceptions/
      ApplicationValidationException.cs  ← Aceita ValidationErrors
```

## Exemplo de Uso

```csharp
// No Domain
var errors = ValidationErrors.Empty;
var (email, emailErrors) = Email.TryCreate(emailValue);
errors = errors.Merge(emailErrors);

var (password, passwordErrors) = Password.TryCreate(passwordValue, hasher);
errors = errors.Merge(passwordErrors);

// Na Application
if (!errors.IsValid)
{
    throw new ApplicationValidationException(errors);
}
```

### To-dos

- [ ] Criar DomainValidationResult no domínio para acumular violações organizadas por campo
- [ ] Modificar Value Objects (Email, Password, Username) para acumular violações ao invés de lançar exceções imediatamente
- [ ] Modificar entidade User para acumular violações em métodos factory (Register, Update)
- [ ] Modificar entidade Game para acumular violações se necessário
- [ ] Atualizar handlers para verificar DomainValidationResult e converter para ApplicationResult.ValidationFailure
- [ ] Atualizar ApplicationValidationException para usar mensagem traduzível (ValidationFailed)