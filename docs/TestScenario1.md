
---

## 🧪 Roteiro de Teste – Registro e Login de Usuário

### 🔹 Funcionalidade: Registro e Autenticação de Usuário

### 🔹 Objetivo: Garantir que o usuário consiga se registrar e logar apenas em condições válidas.

### 🔹 Requisitos:

* Registro exige email válido e senha com padrão mínimo de segurança.
* Login só é permitido para usuários existentes, não bloqueados, confirmados e não banidos.

---

## 🔁 Cenários de Teste

### 🔸 [CT001] Registro de usuário – Sucesso

* **Pré-condição**: Nenhuma.
* **Dados de entrada**:

  * Email: `usuario@email.com`
  * Senha: `Senha@123`
* **Passos**:

  1. Acessar a tela de registro.
  2. Preencher os campos obrigatórios com dados válidos.
  3. Submeter o formulário.
* **Resultado esperado**:

  * Retorno de sucesso (HTTP 201 ou equivalente).
  * Usuário criado com status “pendente de confirmação” (se aplicável).
  * E-mail de confirmação enviado (se houver verificação).

---

### 🔸 [CT002] Registro com email inválido – Erro

* **Dados de entrada**:

  * Email: `usuarioemail.com`
  * Senha: `Senha@123`
* **Resultado esperado**:

  * Retorno de erro (HTTP 400).
  * Mensagem: "Email inválido."

---

### 🔸 [CT003] Registro com senha fraca – Erro

* **Dados de entrada**:

  * Email: `teste@email.com`
  * Senha: `123456`
* **Resultado esperado**:

  * Retorno de erro (HTTP 400).
  * Mensagem: "Senha não atende aos requisitos de segurança."

> Requisitos mínimos sugeridos para senha:
>
> * Mínimo 8 caracteres
> * Pelo menos 1 letra maiúscula
> * Pelo menos 1 número
> * Pelo menos 1 caractere especial

---

### 🔸 [CT004] Registro com email já cadastrado – Erro

* **Pré-condição**: Email já existente no sistema.
* **Dados de entrada**:

  * Email: `usuario@email.com`
  * Senha: `Senha@123`
* **Resultado esperado**:

  * Retorno de erro (HTTP 409 ou 400).
  * Mensagem: "Email já está em uso."

---

### 🔸 [CT005] Login de usuário – Sucesso

* **Pré-condição**:

  * Conta existe, está confirmada, não está bloqueada nem banida.
* **Dados de entrada**:

  * Email: `usuario@email.com`
  * Senha: `Senha@123`
* **Resultado esperado**:

  * Retorno de sucesso (HTTP 200).
  * Token de autenticação retornado (JWT ou equivalente).

---

### 🔸 [CT006] Login com usuário inexistente – Erro

* **Dados de entrada**:

  * Email: `naoexiste@email.com`
  * Senha: `Senha@123`
* **Resultado esperado**:

  * Retorno de erro (HTTP 401 ou 404).
  * Mensagem: "Usuário ou senha inválidos."

---

### 🔸 [CT007] Login com senha incorreta – Erro

* **Pré-condição**: Conta existente.
* **Dados de entrada**:

  * Email: `usuario@email.com`
  * Senha: `senhaerrada`
* **Resultado esperado**:

  * Retorno de erro (HTTP 401).
  * Mensagem: "Usuário ou senha inválidos."

---

### 🔸 [CT008] Login com conta bloqueada – Erro

* **Pré-condição**: Conta está com status **bloqueado**.
* **Resultado esperado**:

  * Retorno de erro (HTTP 403).
  * Mensagem: "Conta bloqueada. Entre em contato com o suporte."

---

### 🔸 [CT009] Login com conta pendente de confirmação – Erro

* **Pré-condição**: Conta não confirmou o email.
* **Resultado esperado**:

  * Retorno de erro (HTTP 403).
  * Mensagem: "Conta não confirmada. Verifique seu email."

---

### 🔸 [CT010] Login com conta banida – Erro

* **Pré-condição**: Conta está com status **banido**.
* **Resultado esperado**:

  * Retorno de erro (HTTP 403).
  * Mensagem: "Conta banida. Acesso negado."

---


## 🎮 Roteiro de Teste – Listagem de Jogos

### 🔹 Funcionalidade: Listar Jogos

### 🔹 Objetivo: Garantir que **usuários autenticados e com permissão** possam visualizar jogos (individualmente ou em lista), e bloquear o acesso para usuários não autorizados.

### 🔹 Requisitos:

* O usuário **precisa estar autenticado**.
* O usuário **precisa ter permissão** para visualizar a(s) lista(s) de jogos.
* A listagem pode ser:

  * De **todos os jogos** (`GET /jogos`)
  * De **um jogo específico** (`GET /jogos/{id}`)

---

## 🔁 Cenários de Teste

---

### 🔸 [CT011] Listar todos os jogos – Sucesso

* **Pré-condições**:

  * Usuário autenticado.
  * Usuário tem permissão de visualização (`role: viewer`, `admin`, etc.).
* **Requisição**:

  * `GET /jogos`
  * Cabeçalho: `Authorization: Bearer <token_válido>`
* **Resultado esperado**:

  * Retorno: HTTP 200
  * Corpo da resposta: Lista de jogos (array JSON), com dados como título, descrição, gênero, etc.

---

### 🔸 [CT012] Listar jogo por ID – Sucesso

* **Pré-condições**:

  * Usuário autenticado e autorizado.
  * Jogo com ID `123` existe.
* **Requisição**:

  * `GET /jogos/123`
  * Cabeçalho: `Authorization: Bearer <token_válido>`
* **Resultado esperado**:

  * Retorno: HTTP 200
  * Corpo da resposta: Objeto com os dados do jogo.

---

### 🔸 [CT013] Listar todos os jogos – Erro: usuário não autenticado

* **Requisição**:

  * `GET /jogos`
  * Sem cabeçalho de autenticação.
* **Resultado esperado**:

  * Retorno: HTTP 401 (Unauthorized)
  * Mensagem: "Token de autenticação ausente ou inválido."

---

### 🔸 [CT014] Listar jogo por ID – Erro: usuário não autenticado

* **Requisição**:

  * `GET /jogos/123`
  * Sem token JWT.
* **Resultado esperado**:

  * Retorno: HTTP 401
  * Mensagem: "Não autorizado."

---

### 🔸 [CT015] Listar todos os jogos – Erro: usuário autenticado sem permissão

* **Pré-condições**:

  * Usuário autenticado com token válido.
  * Role/permissão do usuário **não permite visualização** (ex: `role: basic`).
* **Requisição**:

  * `GET /jogos`
  * Cabeçalho: `Authorization: Bearer <token_válido>`
* **Resultado esperado**:

  * Retorno: HTTP 403 (Forbidden)
  * Mensagem: "Permissão insuficiente para acessar este recurso."

---

### 🔸 [CT016] Listar jogo por ID – Erro: usuário autenticado sem permissão

* **Pré-condições**:

  * Usuário autenticado.
  * Role não autorizada.
* **Requisição**:

  * `GET /jogos/123`
* **Resultado esperado**:

  * Retorno: HTTP 403
  * Mensagem: "Você não tem permissão para visualizar este jogo."

---

### 🔸 [CT017] Listar jogo por ID – Erro: jogo inexistente

* **Pré-condições**:

  * Usuário autenticado com permissão.
* **Requisição**:

  * `GET /jogos/9999` (ID inexistente)
* **Resultado esperado**:

  * Retorno: HTTP 404 (Not Found)
  * Mensagem: "Jogo não encontrado."

---

## ✅ Considerações Técnicas

* **Autenticação**:

  * JWT ou outro mecanismo seguro (Bearer token, OAuth, etc).
* **Autorização**:

  * Baseada em roles ou claims no token.
  * Backend deve validar antes de retornar dados sensíveis.

---
## 🧪 Roteiro de Testes – Funcionalidades Administrativas (Admin)

### 🧑‍💼 Perfil: Usuário com `role: admin`

### 🔐 Requisitos:

* Admin **deve ser criado pela rotina de seeder**.
* Admin **pode realizar CRUD completo** de jogos e usuários.
* Todas as ações **requerem autenticação válida** e verificação do **perfil de admin**.
* Ao remover um usuário, ele **não deve conseguir mais logar**.
* Usuário recém-criado **deve conseguir logar normalmente** (a depender do fluxo de ativação).

---

## 📦 [SEED] Validação da criação de usuário admin pela rotina de seeder

### 🔸 [CT018] Seeder cria usuário admin com perfil correto – Sucesso

* **Pré-condição**: Executar comando de seeder (ex: `npm run seed`, `php artisan db:seed`, etc.).
* **Validação**:

  * Acessar o banco e verificar a existência do usuário:

    * Email: `admin@email.com` (ou padrão do seeder)
    * Role: `admin`
* **Resultado esperado**:

  * Usuário criado com sucesso no banco.
  * Campo `role` definido como `admin`.

---

## 🎮 CRUD de Jogos – Perfil Admin

### 🔸 [CT019] Criar jogo – Sucesso (admin)

* **Requisição**:

  * `POST /jogos`
  * Token de admin no cabeçalho
  * Corpo com dados válidos: título, descrição, gênero etc.
* **Resultado esperado**:

  * HTTP 201
  * Jogo criado com ID e dados retornados.

---

### 🔸 [CT020] Editar jogo – Sucesso (admin)

* **Requisição**:

  * `PUT /jogos/{id}`
  * Token admin
  * Novo corpo de dados
* **Resultado esperado**:

  * HTTP 200
  * Dados atualizados refletidos na resposta.

---

### 🔸 [CT021] Deletar jogo – Sucesso (admin)

* **Requisição**:

  * `DELETE /jogos/{id}`
* **Resultado esperado**:

  * HTTP 204
  * Jogo removido do sistema.

---

### 🔸 [CT022] Acessar endpoints de jogos sem autenticação – Erro

* **Requisição**:

  * Sem token
* **Resultado esperado**:

  * HTTP 401
  * "Não autenticado."

---

### 🔸 [CT023] Acessar endpoints de jogos com perfil não-admin – Erro

* **Requisição**:

  * Token de usuário comum
* **Resultado esperado**:

  * HTTP 403
  * "Permissão negada."

---

## 👥 CRUD de Usuários – Perfil Admin

### 🔸 [CT024] Criar novo usuário – Sucesso

* **Requisição**:

  * `POST /usuarios`
  * Token admin
  * Dados válidos (email, senha, role)
* **Resultado esperado**:

  * HTTP 201
  * Usuário criado e retornado no corpo.

---

### 🔸 [CT025] Editar usuário – Sucesso

* **Requisição**:

  * `PUT /usuarios/{id}`
  * Token admin
* **Resultado esperado**:

  * HTTP 200
  * Usuário atualizado.

---

### 🔸 [CT026] Deletar usuário – Sucesso

* **Requisição**:

  * `DELETE /usuarios/{id}`
* **Resultado esperado**:

  * HTTP 204

---

### 🔸 [CT027] Verificar que usuário deletado não consegue logar – Sucesso

* **Pré-condição**: Usuário foi deletado.
* **Requisição**:

  * `POST /login` com email e senha do usuário deletado.
* **Resultado esperado**:

  * HTTP 401
  * "Usuário ou senha inválidos" ou "Conta não encontrada."

---

### 🔸 [CT028] Verificar que usuário recém-criado consegue logar – Sucesso

* **Pré-condição**: Usuário foi criado com senha válida.
* **Requisição**:

  * `POST /login` com as credenciais criadas.
* **Resultado esperado**:

  * HTTP 200
  * Token JWT retornado.

---

### 🔸 [CT029] Tentar realizar CRUD de usuários sem ser admin – Erro

* **Requisição**:

  * Token de usuário comum
  * Operações: POST, PUT, DELETE em `/usuarios`
* **Resultado esperado**:

  * HTTP 403
  * "Permissão negada."

---

## ✅ Considerações Técnicas

* O sistema deve usar **middleware de autenticação e autorização**:

  * Ex: `auth:api`, `role:admin`
* Operações sensíveis devem ser logadas (opcional, mas recomendado).
* As mensagens de erro não devem expor detalhes do sistema (ex: stacktrace, SQL, etc.).

---
