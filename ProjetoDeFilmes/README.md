# CineScope — catálogo de filmes

Aplicação full stack de busca construída com ASP.NET Core, C#, SQLite, HTML, CSS e JavaScript puros. A API e o frontend são servidos pela mesma aplicação na porta 8000.

## Executar

Requer o SDK do .NET 10:

```bash
cd /home/schifter/Documentos/Projetos/ProjetoDeFilmes
dotnet run
```

Acesse <http://127.0.0.1:8000>.

## Recursos

- busca por título, diretor, gênero, ano e nota, tolerante a maiúsculas e acentos;
- filtro por gênero, seis ordenações e paginação;
- filtros preservados na URL;
- API JSON (`/api/filmes`, `/api/filtros` e `/api/health`);
- interface responsiva e acessível, com estados de carregamento, vazio e erro;
- API ASP.NET Core integrada ao frontend;
- banco SQLite local com 101 filmes.
