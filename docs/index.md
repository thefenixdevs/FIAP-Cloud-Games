---
layout: default
title: FIAP Cloud Games Demo
---

# FIAP Cloud Games · Demo v0.2.0

Bem-vindo à página de demonstração estática do **FIAP Cloud Games**! Aqui você encontra um resumo rápido da API, links úteis e instruções para executar uma demo local.

> ℹ️ Para publicar esta página no GitHub Pages, acesse **Settings ▸ Pages** e selecione a branch `master` (ou `main`) apontando para a pasta `/docs`.

## 📥 Baixe a Release
- Repositório: [thefenixdevs/FIAP-Cloud-Games](https://github.com/thefenixdevs/FIAP-Cloud-Games)
- Última versão: **v0.2.0**
- Artefato: arquivo ZIP gerado automaticamente pela pipeline de release contendo a API pronta para `dotnet run` em produção.

## 🚀 Executando a Demo Localmente
1. Garanta que o **.NET SDK 9.0** esteja instalado.
2. Baixe a release `FIAP-Cloud-Games-v0.2.0.zip`.
3. Extraia o conteúdo e execute:
   ```powershell
   dotnet GameStore.API.dll
   ```
4. A API estará disponível em `https://localhost:5001` (ou `http://localhost:5000`).
5. Acesse a documentação interativa via Swagger em `https://localhost:5001/swagger`.

## 🔑 Credenciais Seedadas
| Perfil | Usuário | Senha | Observações |
|--------|---------|-------|-------------|
| Admin confirmado | `admin@fiap.com` | `Admin@123` | Ideal para testar endpoints protegidos |
| Usuário comum pendente | `user@fiap.com` | `User@123` | Precisa ser confirmado via banco |

## 📡 Endpoints Principais
- `POST /api/auth/register` – Cria um novo usuário.
- `POST /api/auth/login` – Gera token JWT.
- `GET /api/games` – Lista jogos (requer usuário confirmado).
- `POST /api/games` – Cria jogo (requer admin confirmado).

## 🧪 Cobertura de Testes
Os testes automatizados rodam automaticamente em cada push/pull request e durante as releases. Para rodar manualmente:
```powershell
dotnet test --configuration Release
```

## 🌐 Próximos Passos para uma Demo Online Real
- Provisionar um App Service (Azure, Render, Railway etc.) para hospedar a API.
- Gerar uma chave JWT segura via variáveis de ambiente.
- Atualizar esta página com a URL pública fixa da API hospedada.

Ficou com dúvidas? Abra uma issue no repositório ou contribua com um pull request! ✨
