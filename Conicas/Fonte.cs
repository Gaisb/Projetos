using System;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace SecoesConicas
{
    // Carrega uma fonte TrueType do sistema (com acentos) e centraliza o
    // desenho de texto. Se nenhuma fonte for encontrada, cai na fonte
    // interna da Raylib - que so tem ASCII.
    static class Fonte
    {
        static Font regular;
        static Font negrito;
        static bool ttf;

        static readonly string[] CaminhosRegular =
        {
            @"C:\Windows\Fonts\segoeui.ttf",
            @"C:\Windows\Fonts\arial.ttf",
            @"C:\Windows\Fonts\calibri.ttf",
        };

        static readonly string[] CaminhosNegrito =
        {
            @"C:\Windows\Fonts\segoeuib.ttf",
            @"C:\Windows\Fonts\arialbd.ttf",
            @"C:\Windows\Fonts\calibrib.ttf",
        };

        public static void Carregar()
        {
            // Latin-1 (cobre todos os acentos do portugues) + alguns simbolos
            // matematicos usados nos rotulos.
            int[] extras =
            {
                0x03B8, // theta
                0x221A, // raiz quadrada
                0x2225, // paralelo
                0x2190, 0x2191, 0x2192, 0x2193, // setas
                0x25B8, // marcador
                0x2260, // diferente
                0x2013, 0x2014, // travessoes
                0x00B1, // mais-menos
                0x21B5, // enter
            };

            int[] cp = new int[224 + extras.Length];
            for (int i = 0; i < 224; i++) cp[i] = 32 + i;
            Array.Copy(extras, 0, cp, 224, extras.Length);

            regular = Tentar(CaminhosRegular, cp);
            negrito = Tentar(CaminhosNegrito, cp);
            if (negrito.Texture.Id == 0) negrito = regular;
        }

        static Font Tentar(string[] caminhos, int[] cp)
        {
            foreach (string p in caminhos)
            {
                if (!File.Exists(p)) continue;
                Font f = Raylib.LoadFontEx(p, 56, cp, cp.Length);
                if (f.Texture.Id != 0)
                {
                    Raylib.SetTextureFilter(f.Texture, TextureFilter.Bilinear);
                    ttf = true;
                    return f;
                }
            }
            return Raylib.GetFontDefault();
        }

        public static void Descarregar()
        {
            if (!ttf) return;
            if (negrito.Texture.Id != regular.Texture.Id) Raylib.UnloadFont(negrito);
            Raylib.UnloadFont(regular);
        }

        static float Espacamento(float tam) => ttf ? 0.0f : tam / 10.0f;

        public static void Desenhar(string texto, float x, float y, float tam, Color cor, bool bold = false)
        {
            Font f = bold ? negrito : regular;
            Raylib.DrawTextEx(f, texto, new Vector2(x, y), tam, Espacamento(tam), cor);
        }

        public static float Largura(string texto, float tam, bool bold = false)
        {
            Font f = bold ? negrito : regular;
            return Raylib.MeasureTextEx(f, texto, tam, Espacamento(tam)).X;
        }

        public static void DesenharCentralizado(string texto, float centroX, float y, float tam, Color cor, bool bold = false)
        {
            Desenhar(texto, centroX - Largura(texto, tam, bold) / 2.0f, y, tam, cor, bold);
        }

        // Escreve quebrando em varias linhas dentro de 'larguraMax'.
        // Devolve o Y final (depois da ultima linha).
        public static float DesenharQuebrado(string texto, float x, float y, float larguraMax,
                                             float tam, Color cor, bool bold = false)
        {
            string[] palavras = texto.Split(' ');
            string linha = "";

            foreach (string p in palavras)
            {
                string tentativa = linha.Length == 0 ? p : linha + " " + p;
                if (Largura(tentativa, tam, bold) > larguraMax && linha.Length > 0)
                {
                    Desenhar(linha, x, y, tam, cor, bold);
                    y += tam * 1.25f;
                    linha = p;
                }
                else linha = tentativa;
            }

            if (linha.Length > 0)
            {
                Desenhar(linha, x, y, tam, cor, bold);
                y += tam * 1.25f;
            }
            return y;
        }
    }
}
