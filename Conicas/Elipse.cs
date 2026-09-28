using System;
using System.Numerics;

namespace SecoesConicas
{
    // =====================================================================
    //  A ELIPSE COMO SEÇÃO CÔNICA
    // ---------------------------------------------------------------------
    //  Cone duplo, vértice na origem, eixo = eixo Y, semi-ângulo de 45°:
    //
    //        x² + z² = y²
    //
    //  Escrito como união das geratrizes (retas pelo vértice):
    //
    //        P(t, θ) = t · (cos θ , 1 , sen θ)
    //
    //  Plano de corte inclinado em torno do eixo X:
    //
    //        y + a·z = d          com  0 <= a < 1  e  d > 0
    //
    //  Substituindo P(t, θ) no plano:
    //
    //        t(θ) = d / (1 + a·sen θ)
    //
    //  Como |a| < 1 o denominador nunca zera: a curva é FECHADA e fica toda
    //  em uma folha do cone — é a elipse. (a = 0 dá a circunferência.)
    //
    //  Substituindo y = d - a·z em x² + z² = y² e completando o quadrado:
    //
    //        centro   C = ( 0 , d/(1-a²) , -a·d/(1-a²) )
    //        semieixo maior  A = d·√(1+a²) / (1-a²)      (direção inclinada)
    //        semieixo menor  B = d / √(1-a²)             (direção X)
    //
    //  INVERSO (é o que o programa usa para desenhar a equação digitada):
    //  dados A e B, com r = B²/A²,
    //
    //        a = √( (1-r) / (1+r) )        d = B·√(1-a²)
    // =====================================================================
    sealed class Elipse
    {
        public const float K = 1.0f;          // tan(45°) = 1

        public float Inclinacao;              // 'a' do plano  y + a·z = d
        public float Distancia;               // 'd' do plano

        // Extensão desenhada do cone e tamanho do retângulo do plano
        public float AlturaSup, AlturaInf;
        public float PlanoU, PlanoV, PlanoOffsetV;

        // ---------------- Construção ----------------

        public static Elipse DeSemieixos(float semieixo1, float semieixo2)
        {
            float A = MathF.Max(semieixo1, semieixo2);
            float B = MathF.Min(semieixo1, semieixo2);

            float r = (B * B) / (A * A);                       // 0 < r <= 1
            float a = MathF.Sqrt((1.0f - r) / (1.0f + r));
            a = Math.Clamp(a, 0.0f, 0.995f);

            Elipse e = new Elipse
            {
                Inclinacao = a,
                Distancia = B * MathF.Sqrt(1.0f - a * a)
            };
            e.Enquadrar();
            return e;
        }

        // Ajusta o tamanho do cone e do plano ao tamanho da elipse
        public void Enquadrar()
        {
            // ponto mais distante da elipse: sen θ = -1  ->  t = d/(1-a)
            float tMax = Distancia / MathF.Max(1.0e-3f, 1.0f - Inclinacao);

            AlturaSup = tMax * 1.02f;
            AlturaInf = MathF.Max(2.0f, AlturaSup * 0.85f);

            var (A, B) = Semieixos();
            PlanoU = B * 1.55f;
            PlanoV = A * 1.30f;
            PlanoOffsetV = Vector3.Dot(Centro - PlanoCentro, PlanoDirV);
        }

        // ---------------- Cone ----------------

        public float Coef(float theta) => 1.0f + Inclinacao * K * MathF.Sin(theta);

        public static Vector3 PontoCone(float theta, float t) =>
            new Vector3(t * K * MathF.Cos(theta), t, t * K * MathF.Sin(theta));

        public static Vector3 NormalCone(Vector3 p)
        {
            Vector3 n = new Vector3(p.X, -p.Y, p.Z);
            float len = n.Length();
            return len < 1.0e-6f ? Vector3.UnitY : n / len;
        }

        // A folha superior é aparada no plano: a borda da malha É a elipse
        public float LimiteSup(float theta) =>
            MathF.Min(AlturaSup, Distancia / MathF.Max(1.0e-3f, Coef(theta)));

        public float LimiteInf(float theta) => -AlturaInf;

        public bool TryPontoCurva(float theta, out Vector3 p)
        {
            float t = Distancia / MathF.Max(1.0e-3f, Coef(theta));
            p = PontoCone(theta, t);
            return t <= AlturaSup + 1.0e-3f;
        }

        // ---------------- Plano ----------------

        public Vector3 PlanoCentro => new Vector3(0.0f, Distancia, 0.0f);
        public Vector3 PlanoDirU => Vector3.UnitX;
        public Vector3 PlanoDirV => Vector3.Normalize(new Vector3(0.0f, Inclinacao, -1.0f));
        public Vector3 PlanoNormal => Vector3.Normalize(new Vector3(0.0f, 1.0f, Inclinacao));

        public Vector3 PlanoPonto(float u, float v) =>
            PlanoCentro + PlanoDirU * u + PlanoDirV * (v + PlanoOffsetV);

        public Vector3[] PlanoCantos() => new[]
        {
            PlanoPonto(-PlanoU, -PlanoV),
            PlanoPonto( PlanoU, -PlanoV),
            PlanoPonto( PlanoU,  PlanoV),
            PlanoPonto(-PlanoU,  PlanoV),
        };

        public float AnguloPlanoGraus => MathF.Atan(Inclinacao) * 180.0f / MathF.PI;

        // ---------------- Elementos da elipse ----------------

        public Vector3 Centro
        {
            get
            {
                float den = 1.0f - Inclinacao * Inclinacao;
                return new Vector3(0.0f, Distancia / den, -Inclinacao * Distancia / den);
            }
        }

        public (float A, float B) Semieixos()
        {
            float a = Inclinacao;
            float den = 1.0f - a * a;
            float maior = Distancia * MathF.Sqrt(1.0f + a * a) / den;
            float menor = Distancia / MathF.Sqrt(den);
            return (maior, menor);
        }

        public float SemieixoMaior => Semieixos().A;
        public float SemieixoMenor => Semieixos().B;

        public float DistanciaFocal                       // c
        {
            get
            {
                var (A, B) = Semieixos();
                return MathF.Sqrt(MathF.Max(0.0f, A * A - B * B));
            }
        }

        // e = c/A = |a|·√2 / √(1+a²)
        public float Excentricidade
        {
            get
            {
                float a = MathF.Abs(Inclinacao);
                return a * MathF.Sqrt(2.0f) / MathF.Sqrt(1.0f + a * a);
            }
        }

        public bool EhCircunferencia => Inclinacao < 1.0e-3f;

        public Vector3[] Focos()
        {
            float c = DistanciaFocal;
            if (c < 1.0e-4f) return Array.Empty<Vector3>();
            Vector3 v = PlanoDirV;
            return new[] { Centro - v * c, Centro + v * c };
        }

        public Vector3[] Vertices()
        {
            var (A, B) = Semieixos();
            Vector3 v = PlanoDirV, u = PlanoDirU;
            return new[] { Centro - v * A, Centro + v * A, Centro - u * B, Centro + u * B };
        }

        public string EquacaoReduzida()
        {
            var (A, B) = Semieixos();
            return $"x²/{A * A:0.###} + y²/{B * B:0.###} = 1";
        }
    }
}
