using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>Ao passar por aqui, Magnus passa a reaparecer neste ponto.</summary>
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private string mensagem = "";

        private bool usado;

        private void OnTriggerEnter(Collider outro)
        {
            MagnusStatus magnus = outro.GetComponent<MagnusStatus>();
            if (magnus == null)
                return;
            magnus.DefinirCheckpoint(transform.position);
            if (!usado && !string.IsNullOrEmpty(mensagem))
                GerenciadorPartida.Instancia?.MostrarAviso(mensagem);
            usado = true;
        }
    }
}
