using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Placa de pressão: quando uma CAIXA (não o jogador) fica sobre ela, ergue a ponte.
    /// </summary>
    public sealed class PlacaPressao : MonoBehaviour
    {
        [SerializeField] private Ponte ponte;
        [SerializeField] private Renderer indicador;
        [SerializeField] private Material materialAtivo;
        [SerializeField] private AudioClip somAtivar;

        private bool ativada;

        public bool Ativada => ativada;

        private void OnTriggerEnter(Collider outro)
        {
            if (ativada || outro.GetComponentInParent<CaixaEmpurravel>() == null)
                return;

            ativada = true;
            if (indicador != null && materialAtivo != null)
                indicador.sharedMaterial = materialAtivo;
            GerenciadorAudio.Tocar(somAtivar);
            GerenciadorPartida.Instancia?.RegistrarPontos("placa:ponte", 25);
            GerenciadorPartida.Instancia?.MostrarAviso("A caixa ativou o mecanismo: a ponte subiu!");
            if (ponte != null)
                ponte.Erguer();
        }
    }
}
