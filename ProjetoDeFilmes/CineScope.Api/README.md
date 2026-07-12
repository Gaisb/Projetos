# CineScope API — ASP.NET Core

Versão em C#/.NET da API originalmente implementada em `server.py`. Ela reutiliza
o mesmo `filmes.db` e os mesmos arquivos da pasta `public`, portanto o frontend não
precisa ser alterado.

## Executar

```bash
cd /home/schifter/Documentos/Projetos/ProjetoDeFilmes/CineScope.Api
dotnet run
```

A aplicação estará disponível em <http://127.0.0.1:8000>.

## Endpoints

- `GET /api/health`
- `GET /api/filtros`
- `GET /api/filmes?q=&genero=&ordem=nota_desc&pagina=1&por_pagina=12`
