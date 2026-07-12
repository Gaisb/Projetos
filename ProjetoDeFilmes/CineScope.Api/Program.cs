using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var projectRoot = Path.GetFullPath("..", app.Environment.ContentRootPath);
var databasePath = Path.Combine(projectRoot, "filmes.db");
var publicPath = Path.Combine(projectRoot, "public");
var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();

if (!File.Exists(databasePath))
    throw new FileNotFoundException("Banco de filmes não encontrado.", databasePath);

app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(publicPath)
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicPath)
});

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/filtros", async () =>
{
    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();

    var genres = new List<string>();
    await using (var command = connection.CreateCommand())
    {
        command.CommandText = "SELECT DISTINCT genero FROM filmes WHERE genero IS NOT NULL ORDER BY genero";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) genres.Add(reader.GetString(0));
    }

    await using var statsCommand = connection.CreateCommand();
    statsCommand.CommandText = """
        SELECT COUNT(*), ROUND(AVG(nota_imdb), 1), MIN(ano_lancamento), MAX(ano_lancamento)
        FROM filmes
        """;
    await using var statsReader = await statsCommand.ExecuteReaderAsync();
    await statsReader.ReadAsync();

    return Results.Ok(new
    {
        generos = genres,
        estatisticas = new
        {
            total = statsReader.GetInt32(0),
            media = statsReader.GetDouble(1),
            primeiro_ano = statsReader.GetInt32(2),
            ultimo_ano = statsReader.GetInt32(3)
        }
    });
});

app.MapGet("/api/filmes", async (HttpRequest request) =>
{
    var query = request.Query["q"].ToString().Trim();
    var genre = request.Query["genero"].ToString().Trim();
    var sort = request.Query["ordem"].ToString().Trim();
    if (string.IsNullOrEmpty(sort)) sort = "nota_desc";

    if (!int.TryParse(request.Query["pagina"].FirstOrDefault() ?? "1", out var page) ||
        !int.TryParse(request.Query["por_pagina"].FirstOrDefault() ?? "12", out var perPage))
        return Results.BadRequest(new { erro = "Paginação inválida" });

    page = Math.Max(1, page);
    perPage = Math.Clamp(perPage, 1, 24);

    var validSorts = new HashSet<string>
    {
        "nota_desc", "nota_asc", "ano_desc", "ano_asc", "titulo_asc", "titulo_desc"
    };
    if (!validSorts.Contains(sort))
        return Results.BadRequest(new { erro = "Ordenação inválida" });

    var movies = new List<Movie>();
    await using (var connection = new SqliteConnection(connectionString))
    {
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, titulo, genero, ano_lancamento, duracao_minutos, diretor, nota_imdb
            FROM filmes
            WHERE ($genero = '' OR genero = $genero)
            """;
        command.Parameters.AddWithValue("$genero", genre);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            movies.Add(new Movie(
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.GetInt32(3), reader.GetInt32(4), reader.GetString(5), reader.GetDouble(6)));
        }
    }

    if (!string.IsNullOrWhiteSpace(query))
    {
        var terms = Text.Normalize(query).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        movies = movies.Where(movie =>
        {
            var searchable = Text.Normalize($"{movie.Id} {movie.Title} {movie.Genre} {movie.Year} {movie.DurationMinutes} {movie.Director} {movie.ImdbRating}");
            return terms.All(searchable.Contains);
        }).ToList();
    }

    movies = sort switch
    {
        "nota_asc" => movies.OrderBy(m => m.ImdbRating).ThenBy(m => m.Title).ToList(),
        "nota_desc" => movies.OrderByDescending(m => m.ImdbRating).ThenBy(m => m.Title).ToList(),
        "ano_asc" => movies.OrderBy(m => m.Year).ThenBy(m => m.Title).ToList(),
        "ano_desc" => movies.OrderByDescending(m => m.Year).ThenBy(m => m.Title).ToList(),
        "titulo_desc" => movies.OrderByDescending(m => Text.Normalize(m.Title)).ToList(),
        _ => movies.OrderBy(m => Text.Normalize(m.Title)).ToList()
    };

    var total = movies.Count;
    var pages = Math.Max(1, (int)Math.Ceiling(total / (double)perPage));
    var data = movies.Skip((page - 1) * perPage).Take(perPage);

    return Results.Ok(new
    {
        dados = data,
        paginacao = new { pagina = page, por_pagina = perPage, total, paginas = pages }
    });
});

app.MapFallback(async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(publicPath, "index.html"));
});

app.Run("http://127.0.0.1:8000");

record Movie(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("titulo")] string Title,
    [property: JsonPropertyName("genero")] string Genre,
    [property: JsonPropertyName("ano_lancamento")] int Year,
    [property: JsonPropertyName("duracao_minutos")] int DurationMinutes,
    [property: JsonPropertyName("diretor")] string Director,
    [property: JsonPropertyName("nota_imdb")] double ImdbRating);

static class Text
{
    public static string Normalize(string value)
    {
        var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var result = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                result.Append(character);
        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
