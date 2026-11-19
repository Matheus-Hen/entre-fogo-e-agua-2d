using System;
using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Vida e energia de Magnus. A energia é gasta pelos poderes e se recupera com o tempo.
    /// Vida zerada = derrota. Cair no rio custa vida e volta ao último checkpoint.
    /// </summary>
    public sealed class MagnusStatus : MonoBehaviour
    {
        [SerializeField] private float vidaMaxima = 100f;
        [SerializeField] private float energiaMaxima = 100f;
        [SerializeField] private float recargaEnergiaPorSegundo = 14f;
        [SerializeField] private float invulneravelAposDano = 0.7f;
        [SerializeField] private float danoQueda = 30f;
        [SerializeField] private AudioClip somDano;

        private float tempoInvulneravel;
        private Vector3 checkpoint;
        private MagnusMovimento movimento;

        public float Vida { get; private set; }
        public float Energia { get; private set; }
        public float VidaMaxima => vidaMaxima;
        public float EnergiaMaxima => energiaMaxima;

        public event Action StatusMudou;

        private void Awake()
        {
            Vida = vidaMaxima;
            Energia = energiaMaxima;
            checkpoint = transform.position;
            movimento = GetComponent<MagnusMovimento>();
        }

        private void Update()
        {
            if (tempoInvulneravel > 0f)
                tempoInvulneravel -= Time.deltaTime;

            if (Energia < energiaMaxima)
            {
                Energia = Mathf.Min(energiaMaxima, Energia + recargaEnergiaPorSegundo * Time.deltaTime);
                StatusMudou?.Invoke();
            }
        }

        /// <summary>Gasta energia se houver o suficiente. Retorna false quando falta energia.</summary>
        public bool GastarEnergia(float custo)
        {
            if (Energia < custo)
                return false;
            Energia -= custo;
            StatusMudou?.Invoke();
            return true;
        }

        /// <summary>Aplica dano (com breve invulnerabilidade). Vida 0 => derrota.</summary>
        public bool ReceberDano(float dano, string motivo)
        {
            if (tempoInvulneravel > 0f || Vida <= 0f)
                return false;
            GerenciadorPartida partida = GerenciadorPartida.Instancia;
            if (partida != null && partida.Estado != EstadoPartida.Jogando)
                return false;

            Vida = Mathf.Max(0f, Vida - dano);
            tempoInvulneravel = invulneravelAposDano;
            GerenciadorAudio.Tocar(somDano);
            StatusMudou?.Invoke();

            if (Vida <= 0f && partida != null)
                partida.Perder(motivo);
            return true;
        }

        public void DefinirCheckpoint(Vector3 posicao)
        {
            checkpoint = posicao;
        }

        /// <summary>Chamado pela ZonaQueda (rio): perde vida e volta ao checkpoint.</summary>
        public void Cair()
        {
            tempoInvulneravel = 0f;
            bool sobreviveu = ReceberDano(danoQueda, "Magnus foi arrastado pela correnteza do rio.") && Vida > 0f;
            if (sobreviveu && movimento != null)
            {
                movimento.Teleportar(checkpoint);
                GerenciadorPartida.Instancia?.MostrarAviso("Magnus caiu no rio e voltou ao último ponto seguro (-" + danoQueda + " de vida).");
            }
        }
    }
}
