using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EntreFogoEAgua
{
    public enum EstadoPartida
    {
        Jogando,
        Pausado,
        Vitoria,
        Derrota
    }

    /// <summary>
    /// Regras da partida: pontuação, pistas coletadas, tempo, pausa, vitória, derrota e reinício.
    /// Todo o estado vive nesta cena; recarregar a cena (Reiniciar) zera tudo sem duplicar objetos.
    /// </summary>
    public sealed class GerenciadorPartida : MonoBehaviour
    {
        public static GerenciadorPartida Instancia { get; private set; }

        [SerializeField] private int pistasNecessarias = 3;
        [SerializeField] private AudioClip somVitoria;
        [SerializeField] private AudioClip somDerrota;

        // Cada evento que dá pontos tem um id único (ex.: "pista:1", "guarda:G2").
        // O HashSet garante que o mesmo evento não seja contado duas vezes.
        private readonly HashSet<string> eventosPontuados = new HashSet<string>();

        public int Pontuacao { get; private set; }
        public int Pistas { get; private set; }
        public int PistasNecessarias => pistasNecessarias;
        public bool TodasPistas => Pistas >= pistasNecessarias;
        public float TempoDecorrido { get; private set; }
        public EstadoPartida Estado { get; private set; } = EstadoPartida.Jogando;
        public string MotivoResultado { get; private set; } = string.Empty;

        public event Action PontuacaoMudou;
        public event Action PistasMudaram;
        public event Action EstadoMudou;
        public event Action<string> Aviso;

        private void Awake()
        {
            Instancia = this;
            Time.timeScale = 1f;
        }

        private void Start()
        {
            TravarCursor(true);
        }

        private void OnDestroy()
        {
            if (Instancia == this)
                Instancia = null;
        }

        private void Update()
        {
            if (Estado == EstadoPartida.Jogando)
                TempoDecorrido += Time.deltaTime;

            Keyboard teclado = Keyboard.current;
            if (teclado != null && teclado.escapeKey.wasPressedThisFrame)
                AlternarPausa();
        }

        /// <summary>
        /// Soma pontos para um evento. Retorna false (e não soma) se a partida acabou
        /// ou se esse mesmo evento já tinha sido pontuado.
        /// </summary>
        public bool RegistrarPontos(string idEvento, int pontos)
        {
            if (Estado != EstadoPartida.Jogando && Estado != EstadoPartida.Pausado)
                return false;
            if (!eventosPontuados.Add(idEvento))
                return false;

            Pontuacao += pontos;
            PontuacaoMudou?.Invoke();
            return true;
        }

        /// <summary>Coleta uma pista: +pontos e avança o objetivo. Ao juntar todas, o portão abre.</summary>
        public bool ColetarPista(string idPista, int pontos)
        {
            if (!RegistrarPontos("pista:" + idPista, pontos))
                return false;

            Pistas++;
            PistasMudaram?.Invoke();
            if (TodasPistas)
                MostrarAviso("Todas as pistas encontradas! O portão da base ASDA está aberto.");
            return true;
        }

        public void MostrarAviso(string texto)
        {
            Aviso?.Invoke(texto);
        }

        public void Vencer()
        {
            if (Estado != EstadoPartida.Jogando)
                return;
            int bonusTempo = Mathf.Max(0, 300 - Mathf.FloorToInt(TempoDecorrido)); // bônus por terminar rápido
            RegistrarPontos("bonus:tempo", bonusTempo);
            Encerrar(EstadoPartida.Vitoria, "Magnus escapou da base com as provas contra a ASDA!", somVitoria);
        }

        public void Perder(string motivo)
        {
            if (Estado != EstadoPartida.Jogando)
                return;
            Encerrar(EstadoPartida.Derrota, motivo, somDerrota);
        }

        private void Encerrar(EstadoPartida resultado, string motivo, AudioClip som)
        {
            Estado = resultado;
            MotivoResultado = motivo;
            Time.timeScale = 0f;
            TravarCursor(false);
            GerenciadorAudio.AbaixarMusica();
            GerenciadorAudio.Tocar(som);
            Debug.Log("[Jogo] Fim de partida: " + resultado + " | " + motivo + " | pontos=" + Pontuacao);
            EstadoMudou?.Invoke();
        }

        public void AlternarPausa()
        {
            if (Estado == EstadoPartida.Jogando)
            {
                Estado = EstadoPartida.Pausado;
                Time.timeScale = 0f;
                TravarCursor(false);
            }
            else if (Estado == EstadoPartida.Pausado)
            {
                Estado = EstadoPartida.Jogando;
                Time.timeScale = 1f;
                TravarCursor(true);
            }
            else
            {
                return;
            }
            EstadoMudou?.Invoke();
        }

        /// <summary>Nova tentativa: recarrega a cena, restaurando posição, pontos, inimigos e barreiras.</summary>
        public void Reiniciar()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void IrParaMenu()
        {
            Time.timeScale = 1f;
            TravarCursor(false);
            SceneManager.LoadScene(NomesCena.Menu);
        }

        private static void TravarCursor(bool travar)
        {
            Cursor.lockState = travar ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !travar;
        }
    }
}
