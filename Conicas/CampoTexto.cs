using System;
using System.Numerics;
using Raylib_cs;

namespace SecoesConicas
{
    // Campo de texto simples (a Raylib não traz widgets): recebe os
    // caracteres digitados, trata Backspace com repetição, cursor piscando,
    // foco por clique e Enter para confirmar.
    sealed class CampoTexto
    {
        public string Texto = "";
        public bool Focado = true;
        public int MaxCaracteres = 44;
        public Rectangle Area;

        float piscar;
        float tempoBackspace;

        public CampoTexto(Rectangle area, string inicial = "")
        {
            Area = area;
            Texto = inicial;
        }

        // Devolve true quando o usuário aperta Enter
        public bool Atualizar(float dt)
        {
            piscar += dt;

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                Focado = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), Area);

            if (!Focado) return false;

            int ch = Raylib.GetCharPressed();
            while (ch > 0)
            {
                if (ch >= 32 && ch < 256 && Texto.Length < MaxCaracteres)
                    Texto += (char)ch;
                ch = Raylib.GetCharPressed();
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Backspace))
            {
                Apagar();
                tempoBackspace = 0.0f;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Backspace))
            {
                tempoBackspace += dt;
                if (tempoBackspace > 0.35f)
                {
                    tempoBackspace -= 0.05f;
                    Apagar();
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Delete) && Raylib.IsKeyDown(KeyboardKey.LeftShift))
                Texto = "";

            return Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter);
        }

        void Apagar()
        {
            if (Texto.Length > 0) Texto = Texto.Substring(0, Texto.Length - 1);
        }

        public void Desenhar(Tema tema, float tamFonte, string dica)
        {
            Raylib.DrawRectangleRec(Area, tema.CampoFundo);
            Raylib.DrawRectangleLinesEx(Area, Focado ? 2.0f : 1.0f,
                                        Focado ? tema.Destaque : tema.CampoBorda);

            float x = Area.X + 8.0f;
            float y = Area.Y + (Area.Height - tamFonte) / 2.0f;

            if (Texto.Length == 0 && !Focado)
            {
                Fonte.Desenhar(dica, x, y, tamFonte, tema.TextoFraco);
                return;
            }

            Fonte.Desenhar(Texto, x, y, tamFonte, tema.Texto);

            if (Focado && (piscar % 1.0f) < 0.5f)
            {
                float cx = x + Fonte.Largura(Texto, tamFonte) + 1.0f;
                Raylib.DrawLineEx(new Vector2(cx, Area.Y + 5.0f),
                                  new Vector2(cx, Area.Y + Area.Height - 5.0f),
                                  1.6f, tema.Texto);
            }
        }
    }
}
