using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>Saída da fase depois do portão: atravessar com todas as pistas = vitória.</summary>
    public sealed class SaidaFase : MonoBehaviour
    {
        private void OnTriggerEnter(Collider outro)
        {
            if (outro.GetComponent<MagnusStatus>() == null)
                return;
            GerenciadorPartida partida = GerenciadorPartida.Instancia;
            if (partida == null)
                return;
            if (partida.TodasPistas)
                partida.Vencer();
            else
                partida.MostrarAviso("Ainda faltam pistas: " + partida.Pistas + "/" + partida.PistasNecessarias + ".");
        }
    }
}
