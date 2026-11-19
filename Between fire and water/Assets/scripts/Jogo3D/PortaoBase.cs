using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Portão da base: abre (Animator, parâmetro "Aberto") quando todas as pistas forem coletadas.
    /// </summary>
    public sealed class PortaoBase : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Collider bloqueio;
        [SerializeField] private AudioClip somAbrir;

        private static readonly int ParamAberto = Animator.StringToHash("Aberto");
        private GerenciadorPartida partida;
        private bool aberto;

        public bool Aberto => aberto;

        private void Start()
        {
            partida = GerenciadorPartida.Instancia;
            if (partida != null)
                partida.PistasMudaram += Verificar;
        }

        private void OnDestroy()
        {
            if (partida != null)
                partida.PistasMudaram -= Verificar;
        }

        private void Verificar()
        {
            if (aberto || partida == null || !partida.TodasPistas)
                return;
            aberto = true;
            if (animator != null)
                animator.SetBool(ParamAberto, true);
            if (bloqueio != null)
                bloqueio.enabled = false;
            GerenciadorAudio.Tocar(somAbrir);
        }
    }
}
