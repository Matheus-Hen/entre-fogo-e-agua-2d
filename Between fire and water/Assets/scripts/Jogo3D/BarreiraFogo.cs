using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Barreira de fogo: bloqueia a passagem e queima ao toque.
    /// Só é apagada com ÁGUA (precisa de alguns acertos). Fogo não tem efeito.
    /// </summary>
    public sealed class BarreiraFogo : MonoBehaviour, IReceptorElemental
    {
        [SerializeField] private string idBarreira = "B1";
        [SerializeField] private int acertosParaApagar = 2;
        [SerializeField] private int pontos = 25;
        [SerializeField] private float danoContato = 15f;
        [SerializeField] private ParticleSystem chamas;
        [SerializeField] private Collider bloqueio;
        [SerializeField] private Light luz;
        [SerializeField] private AudioClip somApagar;

        private int acertos;

        public bool Acesa { get; private set; } = true;
        public float DanoContato => danoContato;

        public void ReceberElemento(Elemento elemento, float dano, Vector3 ponto)
        {
            if (!Acesa)
                return;
            if (elemento == Elemento.Fogo)
            {
                GerenciadorPartida.Instancia?.MostrarAviso("O fogo só alimenta as chamas. Use ÁGUA (Q troca o elemento).");
                return;
            }

            acertos++;
            if (chamas != null)
            {
                ParticleSystem.EmissionModule emissao = chamas.emission;
                emissao.rateOverTimeMultiplier *= 0.5f;
            }
            if (acertos >= acertosParaApagar)
                Apagar();
        }

        private void Apagar()
        {
            Acesa = false;
            if (chamas != null)
                chamas.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (bloqueio != null)
                bloqueio.enabled = false;
            if (luz != null)
                luz.enabled = false;
            GerenciadorAudio.Tocar(somApagar);
            GerenciadorPartida.Instancia?.RegistrarPontos("barreira:" + idBarreira, pontos);
            GerenciadorPartida.Instancia?.MostrarAviso("Barreira de fogo apagada!");
        }
    }
}
