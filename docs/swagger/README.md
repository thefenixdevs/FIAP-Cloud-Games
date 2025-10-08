# Swagger UI estático

Esta pasta contém uma versão estática da documentação Swagger da **FIAP Cloud Games API**, preparada para ser servida via GitHub Pages.

## 🔄 Como atualizar o arquivo `swagger.json`

1. **Compilar a solução** (garante que o assembly está pronto):
   ```powershell
   dotnet build GameStore.sln
   ```
2. **Instalar o CLI do Swashbuckle** (uma vez só):
   ```powershell
   dotnet tool install Swashbuckle.AspNetCore.Cli --tool-path .dotnet-tools
   ```
3. **Gerar o Swagger diretamente do assembly**:
   ```powershell
   .\.dotnet-tools\swagger tofile --output docs\swagger\swagger.json GameStore.API\bin\Debug\net9.0\GameStore.API.dll v1
   ```

> Dica: se preferir rodar a API e fazer download manual, inicie o projeto e execute:
> ```powershell
> dotnet run --project GameStore.API\GameStore.API.csproj
> curl.exe https://localhost:5001/swagger/v1/swagger.json -o docs/swagger/swagger.json
> ```
> (ajuste a URL caso utilize outra porta.)

## 🌐 Publicando no GitHub Pages

1. Acesse **Settings ▸ Pages** e selecione a branch principal com a pasta `/docs`.
2. Após o deploy, abra `https://<seu-usuario>.github.io/<seu-repo>/swagger/` para visualizar a UI.

## ✅ Checklist pós-atualização

- [ ] Executou `dotnet build` e garantiu que não há erros.
- [ ] Atualizou `docs/swagger/swagger.json`.
- [ ] Validou localmente abrindo `docs/swagger/index.html` no navegador.
- [ ] Commitou os arquivos atualizados (`swagger.json`, `index.html`, eventuais ajustes na API).
