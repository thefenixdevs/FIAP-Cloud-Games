# FIAP Cloud Games – Notas de Versão

## v0.2.0 · 2025-10-08

### 🚀 Destaques
- Primeira release automatizada via GitHub Actions, com build, testes e artefato ZIP da API.
- Versionamento centralizado com `Directory.Build.props`, garantindo consistência entre os projetos .NET.
- Pipeline de CI com execução de build e testes para cada push e pull request.
- Página de demonstração estática (`docs/`) para apresentação do projeto no GitHub Pages.
- Documentação do processo de release e publicação atualizada no `README`.

### ✅ Itens concluídos
- Estrutura de pastas e Clean Architecture estabilizadas.
- Middleware de CorrelationId e políticas de autorização customizadas ativos.
- Banco de dados SQLite com seed automático para usuário admin.
- Testes unitários e de integração contemplando serviços e repositórios principais.

### 🔍 Observações
- O artefato publicado contém apenas a Web API. A base de dados SQLite é generation-on-demand.
- Atualize `appsettings.json` com as chaves de JWT apropriadas antes de subir a API em produção.

### 📌 Próximos passos sugeridos
- Adicionar testes end-to-end para os endpoints expostos.
- Automatizar cobertura de código e publicação do relatório nas releases.
- Preparar contêiner Docker e workflow de publicação para um registry público.
- Disponibilizar ambiente hospedado (ex.: Azure App Service) para alimentar a página de demonstração.
