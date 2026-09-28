using System;
using System.Numerics;
using Raylib_cs;

namespace SecoesConicas
{
    // =====================================================================
    //  ELIPSE — seção cônica com entrada de dados
    // ---------------------------------------------------------------------
    //  Digite no campo do painel a sua equação (ou os semieixos) e o
    //  programa desenha o cone duplo, o plano de corte correspondente e a
    //  elipse da interseção, com centro, focos e vértices.
    //
    //  Formatos aceitos:     a=5 b=3      5 3
    //                        x²/25 + y²/9 = 1        9x² + 25y² = 225
    //
    //  Entrada inválida mostra o erro no painel e mantém a última elipse
    //  válida na tela.
    //
    //  Linha de comando:
    //      dotnet run
    //      dotnet run -- --claro
    //      dotnet run -- --a=5 --b=3
    //      dotnet run -- --png --saida=elipse.png
    // =====================================================================
    static class Program
    {
        const int LarguraTela = 1280;
        const int AlturaTela = 800;
        const int LarguraPainel = 360;

        static void Main(string[] args)
        {
            bool salvarImagem = false;
            bool temaClaro = false;
            string arquivoPng = "elipse.png";
            float semiA = 5.0f, semiB = 3.0f;

            foreach (string s in args)
            {
                if (s == "--png") salvarImagem = true;
                else if (s == "--claro") temaClaro = true;
                else if (s == "--teste") { Teste(); return; }
                else if (s.StartsWith("--saida=")) arquivoPng = s.Substring(8);
                else if (s.StartsWith("--a=")) float.TryParse(s.Substring(4), out semiA);
                else if (s.StartsWith("--b=")) float.TryParse(s.Substring(4), out semiB);
            }

            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
            Raylib.InitWindow(LarguraTela, AlturaTela, "Elipse - seção cônica");
            Raylib.SetTargetFPS(60);
            Raylib.SetExitKey(KeyboardKey.Null);        // ESC é tratado no código
            Fonte.Carregar();

            // O corte deixa a face interna do cone à vista
            Rlgl.DisableBackfaceCulling();

            RenderTexture2D rt = Raylib.LoadRenderTexture(LarguraTela - LarguraPainel, AlturaTela);

            Tema tema = temaClaro ? Tema.Claro() : Tema.Escuro();

            Analise.Resultado inicial = Analise.Ler($"a={semiA} b={semiB}");
            if (inicial.Ok) { semiA = inicial.A; semiB = inicial.B; }
            else { semiA = 5.0f; semiB = 3.0f; }

            Elipse el = Elipse.DeSemieixos(semiA, semiB);
            string mensagem = $"ok — a = {semiA:0.###}, b = {semiB:0.###}";
            bool erro = false;

            CampoTexto campo = new CampoTexto(
                new Rectangle(14.0f, 96.0f, LarguraPainel - 28.0f, 34.0f),
                $"a={semiA:0.###} b={semiB:0.###}");

            bool girar = true, wireframe = false, elementos = true, inset2D = true;
            float tempo = 0.0f;
            bool sair = false;

            while (!Raylib.WindowShouldClose() && !sair)
            {
                float dt = Raylib.GetFrameTime();
                tempo += dt;

                // ---------------- ENTRADA ----------------
                if (Raylib.IsKeyPressed(KeyboardKey.Tab)) campo.Focado = !campo.Focado;

                bool confirmou = campo.Atualizar(dt);

                if (confirmou)
                {
                    Analise.Resultado r = Analise.Ler(campo.Texto);
                    mensagem = r.Mensagem;
                    erro = !r.Ok;
                    if (r.Ok)
                    {
                        semiA = r.A;
                        semiB = r.B;
                        el = Elipse.DeSemieixos(semiA, semiB);
                    }
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                {
                    if (campo.Focado) campo.Focado = false;
                    else sair = true;
                }

                if (!campo.Focado)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.R)) girar = !girar;
                    if (Raylib.IsKeyPressed(KeyboardKey.W)) wireframe = !wireframe;
                    if (Raylib.IsKeyPressed(KeyboardKey.E)) elementos = !elementos;
                    if (Raylib.IsKeyPressed(KeyboardKey.Two)) inset2D = !inset2D;
                    if (Raylib.IsKeyPressed(KeyboardKey.B))
                        tema = tema.Nome == "escuro" ? Tema.Claro() : Tema.Escuro();
                    if (Raylib.IsKeyPressed(KeyboardKey.F10)) Raylib.TakeScreenshot("elipse.png");

                    // Setas ajustam os semieixos ao vivo
                    float passo = 2.0f * dt * MathF.Max(1.0f, semiA * 0.25f);
                    bool mudou = false;
                    if (Raylib.IsKeyDown(KeyboardKey.Right)) { semiA += passo; mudou = true; }
                    if (Raylib.IsKeyDown(KeyboardKey.Left)) { semiA -= passo; mudou = true; }
                    if (Raylib.IsKeyDown(KeyboardKey.Up)) { semiB += passo; mudou = true; }
                    if (Raylib.IsKeyDown(KeyboardKey.Down)) { semiB -= passo; mudou = true; }

                    if (mudou)
                    {
                        semiA = Math.Clamp(semiA, 0.2f, 60.0f);
                        semiB = Math.Clamp(semiB, 0.2f, 60.0f);

                        float maior = MathF.Max(semiA, semiB);
                        float menor = MathF.Min(semiA, semiB);
                        if (maior / menor > 10.0f) menor = maior / 10.0f;   // limite de desenho

                        el = Elipse.DeSemieixos(maior, menor);
                        campo.Texto = $"a={maior:0.###} b={menor:0.###}";
                        mensagem = MathF.Abs(maior - menor) < 1.0e-4f
                            ? "ok — a = b, caso particular: circunferência"
                            : $"ok — a = {maior:0.###}, b = {menor:0.###}";
                        erro = false;
                    }
                }

                // ---------------- CENA 3D ----------------
                float alturaCena = el.AlturaSup + el.AlturaInf;
                float azimute = girar ? 24.0f + 22.0f * MathF.Sin(tempo * 0.28f) : 24.0f;
                Camera3D cam = MontarCamera(azimute, 18.0f, alturaCena * 3.3f,
                                            new Vector3(0.0f, (el.AlturaSup - el.AlturaInf) * 0.5f, 0.0f), 34.0f);

                Raylib.BeginTextureMode(rt);
                Raylib.ClearBackground(tema.Fundo);
                Cena3D.Desenhar(el, cam, tema, rt.Texture.Width, rt.Texture.Height,
                                120, 16, wireframe, 3.4f, elementos);
                Raylib.EndTextureMode();

                // ---------------- TELA ----------------
                Raylib.BeginDrawing();
                Raylib.ClearBackground(tema.Fundo);

                Raylib.DrawTextureRec(rt.Texture,
                    new Rectangle(0.0f, 0.0f, rt.Texture.Width, -rt.Texture.Height),
                    new Vector2(LarguraPainel, 0.0f), Color.White);

                if (inset2D) DesenharVista2D(el, tema);
                DesenharPainel(el, tema, campo, mensagem, erro, girar, wireframe, elementos, inset2D);

                Raylib.EndDrawing();

                if (salvarImagem && tempo > 0.25f)
                {
                    Raylib.TakeScreenshot(arquivoPng);
                    break;
                }
            }

            Raylib.UnloadRenderTexture(rt);
            Fonte.Descarregar();
            Raylib.CloseWindow();
        }

        // Autoteste do leitor de equações:  dotnet run -- --teste
        static void Teste()
        {
            string[] casos =
            {
                "a=5 b=3",
                "5 3",
                "x²/25 + y²/9 = 1",
                "x^2/25 + y^2/9 = 1",
                "9x² + 25y² = 225",
                "x²/16 + y²/16 = 1",
                "x²/25 - y²/9 = 1",
                "x²/25 = 1",
                "x²/25 + y²/9 = 0",
                "x²/a² + y²/b² = 1",
                "x² + 4y + 9 = 1",
                "a=0 b=3",
                "x²/100 + y²/0.5 = 1",
                "banana",
                "",
            };

            foreach (string caso in casos)
            {
                Analise.Resultado r = Analise.Ler(caso);
                string tag = r.Ok ? "OK   " : "ERRO ";
                Console.WriteLine($"{tag} \"{caso}\"  ->  {r.Mensagem}");
            }
        }

        static Camera3D MontarCamera(float azimuteGraus, float elevacaoGraus,
                                     float distancia, Vector3 alvo, float fov)
        {
            float az = azimuteGraus * MathF.PI / 180.0f;
            float el = elevacaoGraus * MathF.PI / 180.0f;

            Camera3D cam = new Camera3D();
            cam.Position = alvo + new Vector3(
                distancia * MathF.Cos(el) * MathF.Sin(az),
                distancia * MathF.Sin(el),
                distancia * MathF.Cos(el) * MathF.Cos(az));
            cam.Target = alvo;
            cam.Up = new Vector3(0.0f, 1.0f, 0.0f);
            cam.FovY = fov;
            cam.Projection = CameraProjection.Perspective;
            return cam;
        }

        // =============== PAINEL LATERAL ===============
        static void DesenharPainel(Elipse el, Tema tema, CampoTexto campo,
                                   string mensagem, bool erro,
                                   bool girar, bool wireframe, bool elementos, bool inset2D)
        {
            Raylib.DrawRectangle(0, 0, LarguraPainel, AlturaTela, tema.PainelFundo);
            Raylib.DrawLine(LarguraPainel, 0, LarguraPainel, AlturaTela, tema.PainelBorda);

            float larg = LarguraPainel - 28.0f;
            var (A, B) = el.Semieixos();
            float c = el.DistanciaFocal;

            float y = 14.0f;
            Fonte.Desenhar("ELIPSE", 16, y, 30, tema.Curva, true);
            Fonte.Desenhar("seção cônica", 122, y + 12, 15, tema.TextoFraco);
            y += 40;

            Fonte.Desenhar("Sua equação ou semieixos:", 16, y, 16, tema.Texto);
            y += 22;

            // campo de entrada (posição fixa, definida no Main)
            campo.Desenhar(tema, 18.0f, "clique aqui e digite");
            y = campo.Area.Y + campo.Area.Height + 8.0f;

            Fonte.Desenhar("Enter aplica   TAB entra/sai do campo", 16, y, 13, tema.TextoFraco);
            y += 20;

            y = Fonte.DesenharQuebrado(mensagem, 16, y, larg, 15,
                                       erro ? tema.Erro : tema.Sucesso, erro);
            y += 4;

            Fonte.Desenhar("exemplos:  a=5 b=3   |   5 3", 16, y, 13, tema.TextoFraco); y += 17;
            Fonte.Desenhar("x²/25 + y²/9 = 1   |   9x² + 25y² = 225", 16, y, 13, tema.TextoFraco); y += 24;

            Raylib.DrawLine(16, (int)y, LarguraPainel - 16, (int)y, tema.PainelBorda); y += 12;

            Fonte.Desenhar("Equação reduzida:", 16, y, 16, tema.Texto); y += 21;
            Fonte.Desenhar(el.EquacaoReduzida(), 28, y, 19, tema.Destaque, true); y += 30;

            Fonte.Desenhar($"a (semieixo maior) = {A:0.###}", 16, y, 17, tema.Vertice); y += 22;
            Fonte.Desenhar($"b (semieixo menor) = {B:0.###}", 16, y, 17, tema.Vertice); y += 22;
            Fonte.Desenhar($"c = √(a² - b²) = {c:0.###}", 16, y, 17, tema.Foco); y += 22;
            Fonte.Desenhar($"e = c/a = {el.Excentricidade:0.###}", 16, y, 17, tema.Foco); y += 22;
            Fonte.Desenhar($"2a = {2.0f * A:0.###}   (d(P,F1) + d(P,F2))", 16, y, 14, tema.TextoFraco); y += 24;

            Fonte.Desenhar($"focos    F1(-{c:0.##}, 0)   F2({c:0.##}, 0)", 16, y, 15, tema.Texto); y += 20;
            Fonte.Desenhar($"vértices (±{A:0.##}, 0)   (0, ±{B:0.##})", 16, y, 15, tema.Texto); y += 26;

            Raylib.DrawLine(16, (int)y, LarguraPainel - 16, (int)y, tema.PainelBorda); y += 12;

            Fonte.Desenhar("CORTE NO CONE (calculado)", 16, y, 16, tema.Destaque, true); y += 23;
            Fonte.Desenhar("cone:   x² + z² = y²", 24, y, 16, tema.Texto); y += 21;
            Fonte.Desenhar($"plano:  y + {el.Inclinacao:0.###}·z = {el.Distancia:0.###}", 24, y, 16, tema.Texto); y += 21;
            Fonte.Desenhar($"inclinação do plano: {el.AnguloPlanoGraus:0.#}°", 24, y, 14, tema.TextoFraco); y += 19;
            Fonte.Desenhar("a = √((1-r)/(1+r)),  r = b²/a²", 24, y, 13, tema.TextoFraco); y += 26;

            Raylib.DrawLine(16, (int)y, LarguraPainel - 16, (int)y, tema.PainelBorda); y += 12;

            Fonte.Desenhar("CONTROLES", 16, y, 16, tema.Destaque, true); y += 23;
            Fonte.Desenhar("← →  ajusta a     ↑ ↓  ajusta b", 20, y, 15, tema.Texto); y += 20;
            Fonte.Desenhar($"R girar: {(girar ? "ON" : "OFF")}    W malha: {(wireframe ? "ON" : "OFF")}", 20, y, 15, tema.Texto); y += 20;
            Fonte.Desenhar($"E focos: {(elementos ? "ON" : "OFF")}    2 vista 2D: {(inset2D ? "ON" : "OFF")}", 20, y, 15, tema.Texto); y += 20;
            Fonte.Desenhar("B fundo    F10 print    ESC sair", 20, y, 15, tema.Texto);
        }

        // =============== VISTA 2D DA ELIPSE (no plano do corte) ===============
        static void DesenharVista2D(Elipse el, Tema tema)
        {
            var (A, B) = el.Semieixos();
            float c = el.DistanciaFocal;

            float larg = 320.0f, alt = 240.0f;
            float x0 = LarguraPainel + 18.0f;
            float y0 = AlturaTela - alt - 18.0f;

            Rectangle area = new Rectangle(x0, y0, larg, alt);
            Raylib.DrawRectangleRec(area, tema.PainelFundo);
            Raylib.DrawRectangleLinesEx(area, 1.0f, tema.PainelBorda);

            Vector2 o = new Vector2(x0 + larg / 2.0f, y0 + alt / 2.0f + 8.0f);
            float escala = MathF.Min((larg * 0.40f) / A, (alt * 0.36f) / B);

            Vector2 Mapa(float x, float y) => new Vector2(o.X + x * escala, o.Y - y * escala);

            Fonte.Desenhar("vista no plano do corte", x0 + 10.0f, y0 + 6.0f, 14.0f, tema.TextoFraco);

            // eixos
            Raylib.DrawLineEx(Mapa(-A * 1.25f, 0), Mapa(A * 1.25f, 0), 1.0f, tema.PainelBorda);
            Raylib.DrawLineEx(Mapa(0, -B * 1.35f), Mapa(0, B * 1.35f), 1.0f, tema.PainelBorda);

            // a elipse:  x = a·cos t ,  y = b·sen t
            const int passos = 240;
            Vector2 ant = Mapa(A, 0.0f);
            for (int i = 1; i <= passos; i++)
            {
                float t = MathF.Tau * i / passos;
                Vector2 atual = Mapa(A * MathF.Cos(t), B * MathF.Sin(t));
                Raylib.DrawLineEx(ant, atual, 2.5f, tema.Curva);
                ant = atual;
            }

            // semieixos
            Raylib.DrawLineEx(Mapa(0, 0), Mapa(A, 0), 1.6f, tema.Vertice);
            Raylib.DrawLineEx(Mapa(0, 0), Mapa(0, B), 1.6f, tema.Vertice);
            Fonte.Desenhar("a", Mapa(A * 0.5f, 0).X, Mapa(A * 0.5f, 0).Y + 4.0f, 15.0f, tema.Vertice, true);
            Fonte.Desenhar("b", Mapa(0, B * 0.5f).X + 5.0f, Mapa(0, B * 0.5f).Y - 8.0f, 15.0f, tema.Vertice, true);

            // focos
            if (c > 1.0e-4f)
            {
                foreach (float sx in new[] { -1.0f, 1.0f })
                {
                    Vector2 f = Mapa(sx * c, 0.0f);
                    Raylib.DrawCircleV(f, 4.0f, tema.Foco);
                    Fonte.Desenhar(sx < 0 ? "F1" : "F2", f.X - 6.0f, f.Y + 6.0f, 14.0f, tema.Foco, true);
                }
            }

            Raylib.DrawCircleV(o, 3.0f, tema.Texto);
        }
    }
}
