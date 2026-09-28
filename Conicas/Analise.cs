using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SecoesConicas
{
    // =====================================================================
    //  LEITURA DA ENTRADA DO USUÁRIO
    // ---------------------------------------------------------------------
    //  Formatos aceitos (todos equivalentes a a=5, b=3):
    //
    //      a=5 b=3
    //      5 3
    //      x²/25 + y²/9 = 1
    //      9x² + 25y² = 225
    //
    //  Qualquer outra coisa devolve Ok=false com uma mensagem explicando o
    //  problema — a tela mostra o erro e mantém a última elipse válida.
    // =====================================================================
    static class Analise
    {
        public readonly struct Resultado
        {
            public readonly bool Ok;
            public readonly float A;          // semieixo maior
            public readonly float B;          // semieixo menor
            public readonly string Mensagem;

            Resultado(bool ok, float a, float b, string msg) { Ok = ok; A = a; B = b; Mensagem = msg; }

            public static Resultado Erro(string msg) => new Resultado(false, 0.0f, 0.0f, msg);
            public static Resultado Valido(float a, float b, string msg) => new Resultado(true, a, b, msg);
        }

        // Limites de desenho
        const float Min = 0.05f;
        const float Max = 200.0f;
        const float RazaoMax = 10.0f;

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        static readonly Regex RxAtrib = new Regex(@"(?:^|[^a-z0-9])([ab])\s*=\s*(\d+(?:\.\d+)?)",
                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static readonly Regex RxDoisNumeros = new Regex(@"^\s*(\d+(?:\.\d+)?)\s*[;\s]\s*(\d+(?:\.\d+)?)\s*$",
                                                        RegexOptions.Compiled);

        static readonly Regex RxTermo = new Regex(@"([+-])?\s*(\d+(?:\.\d+)?)?\s*\*?\s*([xy])\s*\^\s*2\s*(?:/\s*(\d+(?:\.\d+)?))?",
                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Resultado Ler(string entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada))
                return Resultado.Erro("digite algo: a=5 b=3   ou   x²/25 + y²/9 = 1");

            string s = Normalizar(entrada);

            // ---- forma  a=5 b=3 ----
            MatchCollection atrib = RxAtrib.Matches(s);
            if (atrib.Count > 0)
            {
                float? va = null, vb = null;
                foreach (Match m in atrib)
                {
                    float v = float.Parse(m.Groups[2].Value, Inv);
                    if (m.Groups[1].Value.ToLowerInvariant() == "a") va = v; else vb = v;
                }
                if (va == null || vb == null)
                    return Resultado.Erro("informe os dois semieixos: a=5 b=3");

                return Validar(va.Value, vb.Value);
            }

            // ---- forma  "5 3" ----
            Match dn = RxDoisNumeros.Match(s);
            if (dn.Success)
                return Validar(float.Parse(dn.Groups[1].Value, Inv),
                               float.Parse(dn.Groups[2].Value, Inv));

            // ---- forma equação ----
            int ig = s.IndexOf('=');
            if (ig < 0)
                return Resultado.Erro("falta o \"=\" da equação (ex.: x²/25 + y²/9 = 1)");

            string esq = s.Substring(0, ig);
            string dir = s.Substring(ig + 1).Trim();

            if (Regex.IsMatch(dir, "[a-z]"))
                return Resultado.Erro("do lado direito do \"=\" deixe só um número");

            if (dir.Length == 0) dir = "1";
            if (!float.TryParse(dir, NumberStyles.Float, Inv, out float rhs))
                return Resultado.Erro($"não entendi o lado direito: \"{dir}\"");

            if (MathF.Abs(rhs) < 1.0e-9f)
                return Resultado.Erro("lado direito igual a zero: isso não é uma elipse (é um ponto)");

            float cx = 0.0f, cy = 0.0f;
            int nx = 0, ny = 0;
            string resto = esq;

            foreach (Match m in RxTermo.Matches(esq))
            {
                float sinal = m.Groups[1].Value == "-" ? -1.0f : 1.0f;
                float num = m.Groups[2].Success && m.Groups[2].Value.Length > 0
                          ? float.Parse(m.Groups[2].Value, Inv) : 1.0f;
                float den = m.Groups[4].Success ? float.Parse(m.Groups[4].Value, Inv) : 1.0f;

                if (MathF.Abs(den) < 1.0e-9f)
                    return Resultado.Erro("divisão por zero na equação");

                float coef = sinal * num / den;

                if (m.Groups[3].Value.ToLowerInvariant() == "x") { cx += coef; nx++; }
                else { cy += coef; ny++; }

                resto = resto.Replace(m.Value, " ");
            }

            if (nx == 0 && ny == 0)
                return Resultado.Erro("não achei os termos x² e y² (ex.: x²/25 + y²/9 = 1)");
            if (nx == 0 || ny == 0)
                return Resultado.Erro("falta o termo em " + (nx == 0 ? "x²" : "y²") +
                                      ": com um só termo quadrático a curva é uma parábola");

            if (Regex.IsMatch(resto, "[xy]"))
                return Resultado.Erro("só aceito a forma reduzida, sem termos lineares em x ou y");
            if (Regex.IsMatch(resto, "[a-z]"))
                return Resultado.Erro("troque a² e b² pelos valores numéricos (ex.: x²/25 + y²/9 = 1)");

            // Normaliza para  cx·x² + cy·y² = rhs  com rhs > 0
            if (rhs < 0.0f) { cx = -cx; cy = -cy; rhs = -rhs; }

            if (cx < 0.0f && cy < 0.0f)
                return Resultado.Erro("os dois coeficientes são negativos: não existe curva real");

            if (cx < 0.0f || cy < 0.0f)
                return Resultado.Erro("sinais opostos em x² e y²: essa equação é de uma HIPÉRBOLE");

            if (MathF.Abs(cx) < 1.0e-9f || MathF.Abs(cy) < 1.0e-9f)
                return Resultado.Erro("coeficiente zero em x² ou y²");

            float a2 = rhs / cx;
            float b2 = rhs / cy;

            if (a2 <= 0.0f || b2 <= 0.0f)
                return Resultado.Erro("os denominadores a² e b² precisam ser positivos");

            return Validar(MathF.Sqrt(a2), MathF.Sqrt(b2));
        }

        static Resultado Validar(float v1, float v2)
        {
            if (float.IsNaN(v1) || float.IsNaN(v2) || float.IsInfinity(v1) || float.IsInfinity(v2))
                return Resultado.Erro("valor inválido");

            if (v1 <= 0.0f || v2 <= 0.0f)
                return Resultado.Erro("os semieixos precisam ser maiores que zero");

            float A = MathF.Max(v1, v2);
            float B = MathF.Min(v1, v2);

            if (B < Min || A > Max)
                return Resultado.Erro($"valores fora da faixa desenhável ({Min:0.##} a {Max:0.##})");

            if (A / B > RazaoMax)
                return Resultado.Erro($"elipse muito alongada para o desenho: a/b = {A / B:0.#} (máximo {RazaoMax:0.#})");

            string extra = MathF.Abs(A - B) < 1.0e-4f
                ? "ok — a = b, caso particular: circunferência"
                : $"ok — a = {A:0.###}, b = {B:0.###}";

            return Resultado.Valido(A, B, extra);
        }

        // Deixa a expressão num formato único: minúsculas, '^2' em vez de '²',
        // ponto decimal, sem simbolos de multiplicacao exoticos.
        static string Normalizar(string s)
        {
            s = s.Trim().ToLowerInvariant();
            s = s.Replace('−', '-').Replace('–', '-');
            s = s.Replace('·', '*').Replace('×', '*');
            s = s.Replace("²", "^2").Replace("^²", "^2");

            // vírgula entre dígitos = separador decimal; qualquer outra = espaço
            char[] c = s.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != ',') continue;
                bool digAntes = i > 0 && char.IsDigit(c[i - 1]);
                bool digDepois = i + 1 < c.Length && char.IsDigit(c[i + 1]);
                c[i] = (digAntes && digDepois) ? '.' : ' ';
            }
            return new string(c);
        }
    }
}
