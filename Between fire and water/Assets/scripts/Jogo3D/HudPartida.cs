using UnityEngine;
using UnityEngine.UI;

namespace EntreFogoEAgua
{
    /// <summary>
    /// HUD em Canvas: pontuação, pistas, tempo, vida, energia, elemento ativo, objetivo e avisos.
    /// Também controla os painéis de pausa, vitória e derrota e os botões de cada um.
    /// Os valores vêm dos eventos do GerenciadorPartida e do MagnusStatus (sempre os valores reais).
    /// </summary>
    public sealed class HudPartida : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private Text textoPontuacao;
        [SerializeField] private Text textoPistas;
        [SerializeField] private Text textoTempo;
        [SerializeField] private Text textoElemento;
        [SerializeField] private Text textoObjetivo;
        [SerializeField] private Text textoAviso;

        [Header("Barras (RectTransform do preenchimento)")]
        [SerializeField] private RectTransform barraVida;
        [SerializeField] private RectTransform barraEnergia;
        [SerializeField] private Image iconeElemento;

        [Header("Painéis")]
        [SerializeField] private GameObject painelPausa;
        [SerializeField] private GameObject painelResultado;
        [SerializeField] private Text textoTituloResultado;
        [SerializeField] private Text textoDetalheResultado;

        [Header("Referências")]
        [SerializeField] private MagnusStatus magnus;
        [SerializeField] private MagnusPoderes poderes;
        [SerializeField] private PlacaPressao placa;
        [SerializeField] private AudioClip somClique;

        private GerenciadorPartida partida;
        private float tempoAviso;

        private void Start()
        {
            partida = GerenciadorPartida.Instancia;
            partida.PontuacaoMudou += AtualizarTudo;
            partida.PistasMudaram += AtualizarTudo;
            partida.EstadoMudou += AtualizarPaineis;
            partida.Aviso += MostrarAviso;
            if (magnus != null) magnus.StatusMudou += AtualizarBarras;
            if (poderes != null) poderes.ElementoMudou += AtualizarElemento;

            textoAviso.text = string.Empty;
            AtualizarTudo();
            AtualizarBarras();
            AtualizarElemento();
            AtualizarPaineis();
            MostrarAviso("Encontre as 3 pistas da ASDA e fuja pelo portão. Q troca o elemento, clique lança.");
        }

        private void OnDestroy()
        {
            if (partida != null)
            {
                partida.PontuacaoMudou -= AtualizarTudo;
                partida.PistasMudaram -= AtualizarTudo;
                partida.EstadoMudou -= AtualizarPaineis;
                partida.Aviso -= MostrarAviso;
            }
            if (magnus != null) magnus.StatusMudou -= AtualizarBarras;
            if (poderes != null) poderes.ElementoMudou -= AtualizarElemento;
        }

        private void Update()
        {
            int segundos = Mathf.FloorToInt(partida.TempoDecorrido);
            textoTempo.text = string.Format("Tempo {0:00}:{1:00}", segundos / 60, segundos % 60);

            if (tempoAviso > 0f)
            {
                tempoAviso -= Time.unscaledDeltaTime;
                if (tempoAviso <= 0f)
                    textoAviso.text = string.Empty;
            }
            AtualizarObjetivo();
        }

        private void AtualizarTudo()
        {
            textoPontuacao.text = "Pontos: " + partida.Pontuacao;
            textoPistas.text = "Pistas: " + partida.Pistas + "/" + partida.PistasNecessarias;
        }

        private void AtualizarBarras()
        {
            if (magnus == null)
                return;
            DefinirBarra(barraVida, magnus.Vida / magnus.VidaMaxima);
            DefinirBarra(barraEnergia, magnus.Energia / magnus.EnergiaMaxima);
        }

        private static void DefinirBarra(RectTransform barra, float fracao)
        {
            if (barra != null)
                barra.anchorMax = new Vector2(Mathf.Clamp01(fracao), 1f);
        }

        private void AtualizarElemento()
        {
            if (poderes == null)
                return;
            textoElemento.text = "Elemento: " + ElementoUtil.Nome(poderes.ElementoAtual).ToUpperInvariant();
            textoElemento.color = ElementoUtil.Cor(poderes.ElementoAtual);
            if (iconeElemento != null)
                iconeElemento.color = ElementoUtil.Cor(poderes.ElementoAtual);
        }

        private void AtualizarObjetivo()
        {
            string objetivo;
            if (partida.TodasPistas)
                objetivo = "Objetivo: atravesse o portão da base ASDA (ao fundo).";
            else if (placa != null && !placa.Ativada)
                objetivo = "Objetivo: empurre uma caixa até a placa dourada para erguer a ponte. Pistas " + partida.Pistas + "/" + partida.PistasNecessarias;
            else
                objetivo = "Objetivo: encontre as pistas da ASDA (" + partida.Pistas + "/" + partida.PistasNecessarias + ").";
            textoObjetivo.text = objetivo;
        }

        private void MostrarAviso(string texto)
        {
            textoAviso.text = texto;
            tempoAviso = 4f;
        }

        private void AtualizarPaineis()
        {
            painelPausa.SetActive(partida.Estado == EstadoPartida.Pausado);
            bool fim = partida.Estado == EstadoPartida.Vitoria || partida.Estado == EstadoPartida.Derrota;
            painelResultado.SetActive(fim);
            if (!fim)
                return;

            bool venceu = partida.Estado == EstadoPartida.Vitoria;
            textoTituloResultado.text = venceu ? "VITÓRIA!" : "DERROTA";
            textoTituloResultado.color = venceu ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.3f, 0.25f);
            int s = Mathf.FloorToInt(partida.TempoDecorrido);
            textoDetalheResultado.text = partida.MotivoResultado + "\n\nPontuação final: " + partida.Pontuacao +
                                         "\nPistas: " + partida.Pistas + "/" + partida.PistasNecessarias +
                                         string.Format("\nTempo: {0:00}:{1:00}", s / 60, s % 60);
        }

        // ---- Botões (ligados no Inspector pelo onClick)
        public void BotaoContinuar()
        {
            GerenciadorAudio.Tocar(somClique);
            partida.AlternarPausa();
        }

        public void BotaoReiniciar()
        {
            GerenciadorAudio.Tocar(somClique);
            partida.Reiniciar();
        }

        public void BotaoMenu()
        {
            GerenciadorAudio.Tocar(somClique);
            partida.IrParaMenu();
        }
    }
}
