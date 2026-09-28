using Raylib_cs;

namespace SecoesConicas
{
    // Paleta de cores. Dois temas: escuro (padrao) e claro.
    sealed class Tema
    {
        public string Nome = "";
        public Color Fundo;
        public Color ConeClaro;     // parte iluminada da superficie
        public Color ConeEscuro;    // parte na sombra
        public Color ConeInterno;   // face interna (visivel no corte)
        public Color Tampa;         // tampa/borda do tronco de cone
        public Color Aresta;        // circunferencias de borda
        public Color PlanoPreenche; // preenchimento translucido do plano
        public Color PlanoBorda;    // contorno do plano
        public Color Curva;         // a elipse
        public Color Texto;
        public Color TextoFraco;
        public Color Destaque;
        public Color PainelFundo;
        public Color PainelBorda;
        public Color CampoFundo;
        public Color CampoBorda;
        public Color Erro;
        public Color Sucesso;
        public Color Foco;          // marcador dos focos
        public Color Vertice;       // marcador dos vertices

        public static Tema Escuro() => new Tema
        {
            Nome = "escuro",
            Fundo = new Color(18, 18, 28, 255),
            ConeClaro = new Color(186, 200, 218, 255),
            ConeEscuro = new Color(44, 58, 78, 255),
            ConeInterno = new Color(26, 34, 46, 255),
            Tampa = new Color(58, 74, 96, 255),
            Aresta = new Color(150, 168, 190, 255),
            PlanoPreenche = new Color(150, 195, 240, 40),
            PlanoBorda = new Color(210, 228, 248, 225),
            Curva = new Color(255, 70, 70, 255),
            Texto = new Color(236, 240, 248, 255),
            TextoFraco = new Color(130, 140, 160, 255),
            Destaque = new Color(90, 200, 255, 255),
            PainelFundo = new Color(10, 10, 18, 235),
            PainelBorda = new Color(60, 60, 80, 255),
            CampoFundo = new Color(26, 28, 40, 255),
            CampoBorda = new Color(70, 74, 96, 255),
            Erro = new Color(255, 105, 97, 255),
            Sucesso = new Color(120, 255, 140, 255),
            Foco = new Color(255, 190, 60, 255),
            Vertice = new Color(120, 255, 140, 255),
        };

        public static Tema Claro() => new Tema
        {
            Nome = "claro",
            Fundo = new Color(240, 245, 250, 255),
            ConeClaro = new Color(196, 208, 222, 255),
            ConeEscuro = new Color(58, 76, 100, 255),
            ConeInterno = new Color(40, 54, 72, 255),
            Tampa = new Color(120, 138, 162, 255),
            Aresta = new Color(60, 74, 94, 255),
            PlanoPreenche = new Color(255, 255, 255, 170),
            PlanoBorda = new Color(20, 24, 32, 235),
            Curva = new Color(214, 32, 40, 255),
            Texto = new Color(18, 22, 30, 255),
            TextoFraco = new Color(110, 120, 135, 255),
            Destaque = new Color(20, 90, 160, 255),
            PainelFundo = new Color(226, 233, 241, 240),
            PainelBorda = new Color(180, 192, 206, 255),
            CampoFundo = new Color(255, 255, 255, 255),
            CampoBorda = new Color(170, 182, 198, 255),
            Erro = new Color(196, 30, 30, 255),
            Sucesso = new Color(22, 128, 56, 255),
            Foco = new Color(196, 120, 10, 255),
            Vertice = new Color(22, 128, 56, 255),
        };
    }
}
