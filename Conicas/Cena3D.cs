using System;
using System.Numerics;
using Raylib_cs;

namespace SecoesConicas
{
    // =====================================================================
    //  RENDERIZAÇÃO 3D: cone duplo + plano de corte + a elipse
    // ---------------------------------------------------------------------
    //  A superfície é uma malha de quadriláteros em (θ, t):
    //
    //      P(t, θ) = t · (cos θ , 1 , sen θ)
    //
    //  Para cada θ o parâmetro t vai de 0 (vértice) até o limite daquela
    //  geratriz. Na folha superior o limite é o próprio ponto de interseção
    //  com o plano, então a BORDA DA MALHA É A ELIPSE.
    // =====================================================================
    static class Cena3D
    {
        // Luz quase horizontal: produz a faixa clara vertical no cone
        static readonly Vector3 Luz = Vector3.Normalize(new Vector3(-0.58f, 0.16f, 0.80f));

        public static void Desenhar(Elipse el, Camera3D cam, Tema tema,
                                    int larguraFbo, int alturaFbo,
                                    int nTheta, int nS,
                                    bool wireframe, float espessuraCurva,
                                    bool mostrarElementos)
        {
            Raylib.BeginMode3D(cam);

            DesenharFolha(el, +1, cam, tema, wireframe, nTheta, nS);
            DesenharFolha(el, -1, cam, tema, wireframe, nTheta, nS);
            DesenharTampaInferior(el, tema, nTheta);
            DesenharPlano(el, tema);

            Raylib.EndMode3D();

            // Contorno do plano e a curva em 2D, por cima: linhas finas e nítidas
            DesenharContornoPlano(el, cam, tema, larguraFbo, alturaFbo);
            DesenharCurva(el, cam, tema, larguraFbo, alturaFbo, espessuraCurva);

            if (mostrarElementos)
                DesenharElementos(el, cam, tema, larguraFbo, alturaFbo);
        }

        // ---------------- Superfície ----------------

        static void DesenharFolha(Elipse el, int sinal, Camera3D cam, Tema tema,
                                  bool wireframe, int nTheta, int nS)
        {
            float passo = MathF.Tau / nTheta;

            for (int i = 0; i < nTheta; i++)
            {
                float th0 = i * passo;
                float th1 = (i + 1) * passo;

                float l0 = sinal > 0 ? el.LimiteSup(th0) : el.LimiteInf(th0);
                float l1 = sinal > 0 ? el.LimiteSup(th1) : el.LimiteInf(th1);

                for (int j = 0; j < nS; j++)
                {
                    float s0 = (float)j / nS;
                    float s1 = (float)(j + 1) / nS;

                    Vector3 p00 = Elipse.PontoCone(th0, l0 * s0);
                    Vector3 p01 = Elipse.PontoCone(th0, l0 * s1);
                    Vector3 p11 = Elipse.PontoCone(th1, l1 * s1);
                    Vector3 p10 = Elipse.PontoCone(th1, l1 * s0);

                    if (wireframe)
                    {
                        Raylib.DrawLine3D(p01, p11, tema.Aresta);
                        Raylib.DrawLine3D(p00, p01, tema.Aresta);
                    }
                    else
                    {
                        Vector3 centro = (p00 + p01 + p11 + p10) * 0.25f;
                        Color cor = CorSuperficie(centro, cam, tema);
                        Raylib.DrawTriangle3D(p00, p01, p11, cor);
                        Raylib.DrawTriangle3D(p00, p11, p10, cor);
                    }
                }
            }
        }

        static Color CorSuperficie(Vector3 p, Camera3D cam, Tema tema)
        {
            Vector3 n = Elipse.NormalCone(p);
            Vector3 vista = Vector3.Normalize(cam.Position - p);

            // Face interna: aparece pela abertura onde o plano cortou o cone
            if (Vector3.Dot(n, vista) < 0.0f)
            {
                float diI = 0.5f + 0.5f * Vector3.Dot(-n, Luz);
                return Misturar(tema.ConeInterno, tema.Tampa, 0.55f * Math.Clamp(diI, 0.0f, 1.0f));
            }

            float dif = 0.5f + 0.5f * Vector3.Dot(n, Luz);          // meio-Lambert
            float k = 0.07f + 0.93f * MathF.Pow(Math.Clamp(dif, 0.0f, 1.0f), 2.1f);
            return Misturar(tema.ConeEscuro, tema.ConeClaro, k);
        }

        // Fecha a base da folha inferior (e desenha sua aresta)
        static void DesenharTampaInferior(Elipse el, Tema tema, int nTheta)
        {
            float passo = MathF.Tau / nTheta;
            Vector3 eixo = new Vector3(0.0f, -el.AlturaInf, 0.0f);

            for (int i = 0; i < nTheta; i++)
            {
                float th0 = i * passo;
                float th1 = (i + 1) * passo;

                Vector3 a = Elipse.PontoCone(th0, el.LimiteInf(th0));
                Vector3 b = Elipse.PontoCone(th1, el.LimiteInf(th1));

                Raylib.DrawTriangle3D(eixo, a, b, tema.Tampa);
                Raylib.DrawTriangle3D(eixo, b, a, tema.Tampa);
                Raylib.DrawLine3D(a, b, tema.Aresta);
            }
        }

        static void DesenharPlano(Elipse el, Tema tema)
        {
            Vector3[] k = el.PlanoCantos();
            Raylib.DrawTriangle3D(k[0], k[1], k[2], tema.PlanoPreenche);
            Raylib.DrawTriangle3D(k[0], k[2], k[3], tema.PlanoPreenche);
        }

        // ---------------- Camadas 2D projetadas ----------------

        static Vector2 Projetar(Vector3 p, Camera3D cam, int w, int h) =>
            Raylib.GetWorldToScreenEx(p, cam, w, h);

        static void DesenharContornoPlano(Elipse el, Camera3D cam, Tema tema, int w, int h)
        {
            Vector3[] k = el.PlanoCantos();
            for (int i = 0; i < 4; i++)
            {
                Vector2 a = Projetar(k[i], cam, w, h);
                Vector2 b = Projetar(k[(i + 1) % 4], cam, w, h);
                Raylib.DrawLineEx(a, b, 1.8f, tema.PlanoBorda);
            }
        }

        static void DesenharCurva(Elipse el, Camera3D cam, Tema tema, int w, int h, float espessura)
        {
            const int amostras = 1200;
            bool temAnt = false;
            Vector2 sAnt = default;

            for (int i = 0; i <= amostras; i++)
            {
                float th = MathF.Tau * i / amostras;

                if (el.TryPontoCurva(th, out Vector3 p))
                {
                    Vector2 s = Projetar(p, cam, w, h);
                    if (temAnt) Raylib.DrawLineEx(sAnt, s, espessura, tema.Curva);
                    sAnt = s;
                    temAnt = true;
                }
                else temAnt = false;
            }
        }

        // Centro, focos e vértices sobre a curva
        static void DesenharElementos(Elipse el, Camera3D cam, Tema tema, int w, int h)
        {
            foreach (Vector3 v in el.Vertices())
                Marcar(Projetar(v, cam, w, h), 4.0f, tema.Vertice, tema, "");

            Vector3[] focos = el.Focos();
            for (int i = 0; i < focos.Length; i++)
                Marcar(Projetar(focos[i], cam, w, h), 5.5f, tema.Foco, tema, i == 0 ? "F1" : "F2");

            Marcar(Projetar(el.Centro, cam, w, h), 4.0f, tema.Texto, tema, "C");
        }

        static void Marcar(Vector2 p, float raio, Color cor, Tema tema, string rotulo)
        {
            Raylib.DrawCircleV(p, raio, cor);
            Raylib.DrawCircleLinesV(p, raio, tema.Fundo);
            if (!string.IsNullOrEmpty(rotulo))
                Fonte.Desenhar(rotulo, p.X + 8.0f, p.Y - 9.0f, 17.0f, cor, true);
        }

        // ---------------- Utilidades ----------------

        static Color Misturar(Color a, Color b, float k)
        {
            k = Math.Clamp(k, 0.0f, 1.0f);
            return new Color(
                (int)(a.R + (b.R - a.R) * k),
                (int)(a.G + (b.G - a.G) * k),
                (int)(a.B + (b.B - a.B) * k),
                (int)(a.A + (b.A - a.A) * k));
        }
    }
}
