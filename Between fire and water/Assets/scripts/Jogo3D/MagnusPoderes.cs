using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Poderes elementais de Magnus.
    /// Q / Tab / roda do mouse: troca entre Fogo e Água.  Clique esquerdo ou F: lança o elemento ativo.
    /// Cada lançamento gasta energia e cria um projétil físico (Rigidbody) a partir do prefab.
    /// </summary>
    public sealed class MagnusPoderes : MonoBehaviour
    {
        [SerializeField] private ProjetilElemental projetilFogo;
        [SerializeField] private ProjetilElemental projetilAgua;
        [SerializeField] private Transform pontoLancamento;
        [SerializeField] private Transform referenciaCamera;
        [SerializeField] private Animator animator;
        [SerializeField] private float custoEnergia = 12f;
        [SerializeField] private float intervalo = 0.35f;
        [SerializeField] private float velocidadeProjetil = 18f;
        [SerializeField] private AudioClip somFogo;
        [SerializeField] private AudioClip somAgua;

        private static readonly int ParamLancar = Animator.StringToHash("Lancar");

        private MagnusStatus status;
        private MagnusMovimento movimento;
        private Collider[] meusColisores;
        private float proximoLancamento;

        public Elemento ElementoAtual { get; private set; } = Elemento.Fogo;
        public event Action ElementoMudou;

        private void Awake()
        {
            status = GetComponent<MagnusStatus>();
            movimento = GetComponent<MagnusMovimento>();
            meusColisores = GetComponentsInChildren<Collider>();
            if (referenciaCamera == null && Camera.main != null)
                referenciaCamera = Camera.main.transform;
        }

        private void Update()
        {
            GerenciadorPartida partida = GerenciadorPartida.Instancia;
            if (partida != null && partida.Estado != EstadoPartida.Jogando)
                return;

            Keyboard teclado = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool trocar = teclado != null && (teclado.qKey.wasPressedThisFrame || teclado.tabKey.wasPressedThisFrame);
            if (mouse != null && Mathf.Abs(mouse.scroll.ReadValue().y) > 0.1f)
                trocar = true;
            if (teclado != null && teclado.digit1Key.wasPressedThisFrame) DefinirElemento(Elemento.Fogo);
            if (teclado != null && teclado.digit2Key.wasPressedThisFrame) DefinirElemento(Elemento.Agua);
            if (trocar)
                DefinirElemento(ElementoUtil.Oposto(ElementoAtual));

            bool lancar = (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                          (teclado != null && teclado.fKey.wasPressedThisFrame);
            if (lancar)
                Lancar();
        }

        public void DefinirElemento(Elemento elemento)
        {
            if (ElementoAtual == elemento)
                return;
            ElementoAtual = elemento;
            ElementoMudou?.Invoke();
        }

        /// <summary>Tenta lançar o elemento atual na direção da câmera. Retorna o projétil criado (ou null).</summary>
        public ProjetilElemental Lancar()
        {
            if (Time.time < proximoLancamento)
                return null;
            if (status != null && !status.GastarEnergia(custoEnergia))
            {
                GerenciadorPartida.Instancia?.MostrarAviso("Sem energia! Espere ela recarregar.");
                return null;
            }
            proximoLancamento = Time.time + intervalo;

            Vector3 direcao = transform.forward;
            if (referenciaCamera != null)
                direcao = Vector3.ProjectOnPlane(referenciaCamera.forward, Vector3.up).normalized;
            if (movimento != null)
                movimento.VirarPara(direcao);

            ProjetilElemental prefab = ElementoAtual == Elemento.Fogo ? projetilFogo : projetilAgua;
            Vector3 origem = pontoLancamento != null ? pontoLancamento.position : transform.position + Vector3.up * 1.2f;
            ProjetilElemental projetil = Instantiate(prefab, origem + direcao * 0.6f, Quaternion.LookRotation(direcao));
            projetil.Disparar(direcao * velocidadeProjetil + Vector3.up * 1.5f, meusColisores);

            GerenciadorAudio.Tocar(ElementoAtual == Elemento.Fogo ? somFogo : somAgua, 0.8f);
            if (animator != null)
                animator.SetTrigger(ParamLancar);
            return projetil;
        }
    }
}
