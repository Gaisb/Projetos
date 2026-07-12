# CineScope — catálogo de filmes

Aplicação full stack de busca construída com Python, SQLite, HTML, CSS e JavaScript puros. O banco `filmes.db` é criado automaticamente com os 101 registros de `../Projeto_08_Banco_Dados_Filmes-main/filmes.sql` na primeira execução.

## Executar

Requer apenas Python 3:

```bash
cd /home/schifter/Documentos/ProjetoDeFilmes
python3 server.py
```

Acesse <http://127.0.0.1:8000>.

## Recursos

- busca por título, diretor, gênero, ano e nota, tolerante a maiúsculas e acentos;
- filtro por gênero, seis ordenações e paginação;
- filtros preservados na URL;
- API JSON (`/api/filmes`, `/api/filtros` e `/api/health`);
- interface responsiva e acessível, com estados de carregamento, vazio e erro;
- zero dependências externas em tempo de execução (a fonte web possui fallback local).
