using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Pista (documento da ASDA). Trigger: quando Magnus encosta, soma pontos, avança o objetivo
    /// e some. O id único impede que a mesma pista seja contada duas vezes.
    /// A animação de flutuar vem do Animator (clip Pista_Flutuar).
    /// </summary>
    public sealed class Pista : MonoBehaviour
    {
        [SerializeField] private string idPista = "1";
        [SerializeField] private int pontos = 100;
        [SerializeField, TextArea] private string texto = "Pista encontrada.";
        [SerializeField] private AudioClip somColeta;

        private bool coletada;

        public string Id => idPista;

        /// <summary>Usado quando a pista é criada durante a partida (drop do guarda).</summary>
        public void Configurar(string id, string textoPista)
        {
            idPista = id;
            texto = textoPista;
        }

        private void OnTriggerEnter(Collider outro)
        {
            if (coletada || outro.GetComponent<MagnusStatus>() == null)
                return;

            GerenciadorPartida partida = GerenciadorPartida.Instancia;
            if (partida == null || !partida.ColetarPista(idPista, pontos))
                return;

            coletada = true;
            GerenciadorAudio.Tocar(somColeta);
            partida.MostrarAviso(texto);
            Destroy(gameObject);
        }
    }
}
