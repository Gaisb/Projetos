#!/usr/bin/env python3
"""Servidor HTTP e API do Catálogo de Filmes, sem dependências externas."""

from __future__ import annotations

import json
import re
import sqlite3
import unicodedata
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import parse_qs, urlparse

ROOT = Path(__file__).resolve().parent
DB = ROOT / "filmes.db"
SQL_SOURCE = ROOT.parent / "Banco_Dados_Filmes-main" / "filmes.sql"


def normalize(value: str) -> str:
    value = unicodedata.normalize("NFKD", value.casefold())
    return "".join(char for char in value if not unicodedata.combining(char))


def init_database() -> None:
    if DB.exists():
        return
    if not SQL_SOURCE.exists():
        raise FileNotFoundError(f"Banco de origem não encontrado: {SQL_SOURCE}")
    connection = sqlite3.connect(DB)
    connection.executescript(SQL_SOURCE.read_text(encoding="utf-8-sig"))
    connection.execute("CREATE INDEX IF NOT EXISTS idx_filmes_genero ON filmes(genero)")
    connection.execute("CREATE INDEX IF NOT EXISTS idx_filmes_ano ON filmes(ano_lancamento)")
    connection.commit()
    connection.close()


class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(ROOT / "public"), **kwargs)

    def log_message(self, fmt, *args):
        print(f"[http] {self.address_string()} - {fmt % args}")

    def json_response(self, payload, status=200):
        body = json.dumps(payload, ensure_ascii=False).encode()
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Cache-Control", "no-store")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        parsed = urlparse(self.path)
        if parsed.path == "/api/filmes":
            return self.list_movies(parse_qs(parsed.query))
        if parsed.path == "/api/filtros":
            return self.filters()
        if parsed.path == "/api/health":
            return self.json_response({"status": "ok"})
        if parsed.path != "/" and not (ROOT / "public" / parsed.path.lstrip("/")).exists():
            self.path = "/"
        return super().do_GET()

    def connect(self):
        connection = sqlite3.connect(DB)
        connection.row_factory = sqlite3.Row
        return connection

    def filters(self):
        with self.connect() as db:
            genres = [
                row[0]
                for row in db.execute(
                    "SELECT DISTINCT genero FROM filmes "
                    "WHERE genero IS NOT NULL ORDER BY genero"
                )
            ]
            stats = dict(
                db.execute(
                    "SELECT COUNT(*) total, ROUND(AVG(nota_imdb), 1) media, "
                    "MIN(ano_lancamento) primeiro_ano, "
                    "MAX(ano_lancamento) ultimo_ano FROM filmes"
                ).fetchone()
            )
        self.json_response({"generos": genres, "estatisticas": stats})

    def list_movies(self, params):
        get = lambda key, default="": params.get(key, [default])[0].strip()
        query, genre = get("q"), get("genero")
        sort = get("ordem", "nota_desc")
        try:
            page = max(1, int(get("pagina", "1")))
            per_page = min(24, max(1, int(get("por_pagina", "12"))))
        except ValueError:
            return self.json_response({"erro": "Paginação inválida"}, 400)

        orderings = {
            "nota_desc": "nota_imdb DESC, titulo ASC",
            "nota_asc": "nota_imdb ASC, titulo ASC",
            "ano_desc": "ano_lancamento DESC, titulo ASC",
            "ano_asc": "ano_lancamento ASC, titulo ASC",
            "titulo_asc": "titulo COLLATE NOCASE ASC",
            "titulo_desc": "titulo COLLATE NOCASE DESC",
        }
        if sort not in orderings:
            return self.json_response({"erro": "Ordenação inválida"}, 400)

        clauses, values = [], []
        if genre:
            clauses.append("genero = ?")
            values.append(genre)
        where = " WHERE " + " AND ".join(clauses) if clauses else ""
        with self.connect() as db:
            rows = [dict(row) for row in db.execute(f"SELECT * FROM filmes{where}", values)]

        # A normalização em Python torna a busca tolerante a acentos e caixa sem extensões SQLite.
        if query:
            terms = normalize(query).split()
            rows = [
                movie
                for movie in rows
                if all(
                    term
                    in normalize(" ".join(str(v or "") for v in movie.values()))
                    for term in terms
                )
            ]
        rows.sort(key=lambda movie: normalize(movie["titulo"]))
        reverse = sort.endswith("desc")
        field = "nota_imdb" if sort.startswith("nota") else "ano_lancamento" if sort.startswith("ano") else "titulo"
        rows.sort(
            key=lambda movie: (
                normalize(str(movie[field]))
                if field == "titulo"
                else movie[field] or 0
            ),
            reverse=reverse,
        )
        total = len(rows)
        start = (page - 1) * per_page
        pages = max(1, (total + per_page - 1) // per_page)
        self.json_response(
            {
                "dados": rows[start : start + per_page],
                "paginacao": {
                    "pagina": page,
                    "por_pagina": per_page,
                    "total": total,
                    "paginas": pages,
                },
            }
        )


if __name__ == "__main__":
    init_database()
    server = ThreadingHTTPServer(("127.0.0.1", 8000), Handler)
    print("Catálogo disponível em http://127.0.0.1:8000")
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nServidor encerrado.")
