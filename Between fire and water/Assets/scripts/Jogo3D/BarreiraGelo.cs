using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Barreira de gelo: derretida pelo FOGO (encolhe a cada acerto).
    /// ÁGUA congela e REFORÇA o gelo (precisa de mais acertos de fogo).
    /// </summary>
    public sealed class BarreiraGelo : MonoBehaviour, IReceptorElemental
    {
        [SerializeField] private string idBarreira = "Gelo1";
        [SerializeField] private int acertosParaDerreter = 3;
        [SerializeField] private int maximoAcertos = 6;
        [SerializeField] private int pontos = 25;
        [SerializeField] private Transform bloco;
        [SerializeField] private AudioClip somDerreter;

        private int acertosNecessarios;
        private Vector3 escalaInicial;
        private bool derretida;

        public int AcertosRestantes => acertosNecessarios;

        private void Awake()
        {
            acertosNecessarios = acertosParaDerreter;
            if (bloco == null)
                bloco = transform;
            escalaInicial = bloco.localScale;
        }

        public void ReceberElemento(Elemento elemento, float dano, Vector3 ponto)
        {
            if (derretida)
                return;

            if (elemento == Elemento.Agua)
            {
                acertosNecessarios = Mathf.Min(maximoAcertos, acertosNecessarios + 1);
                GerenciadorPartida.Instancia?.MostrarAviso("A água congelou e o gelo ficou mais grosso! Use FOGO.");
            }
            else
            {
                acertosNecessarios--;
                GerenciadorAudio.Tocar(somDerreter, 0.6f);
            }

            float fracao = Mathf.Clamp01((float)acertosNecessarios / acertosParaDerreter);
            bloco.localScale = new Vector3(escalaInicial.x, escalaInicial.y * Mathf.Max(0.25f, fracao), escalaInicial.z);

            if (acertosNecessarios <= 0)
            {
                derretida = true;
                GerenciadorPartida.Instancia?.RegistrarPontos("barreira:" + idBarreira, pontos);
                GerenciadorPartida.Instancia?.MostrarAviso("O gelo derreteu!");
                gameObject.SetActive(false);
            }
        }
    }
}
