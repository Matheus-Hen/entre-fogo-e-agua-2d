using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Guarda da ASDA com AFINIDADE ELEMENTAL (regra criada pela equipe):
    /// - o guarda ABSORVE o próprio elemento (recupera vida);
    /// - só recebe dano do elemento oposto.
    /// Patrulha entre dois pontos; ao ver Magnus, vira-se e dispara projéteis.
    /// Derrotado: +pontos (uma única vez) e, opcionalmente, deixa cair uma pista.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Guarda : MonoBehaviour, IReceptorElemental
    {
        [Header("Identidade")]
        [SerializeField] private string idGuarda = "G1";
        [SerializeField] private Elemento afinidade = Elemento.Fogo;
        [SerializeField] private float vidaMaxima = 60f;
        [SerializeField] private int pontos = 50;

        [Header("Patrulha e ataque")]
        [SerializeField] private Vector3 deslocamentoPatrulha = new Vector3(6f, 0f, 0f);
        [SerializeField] private float velocidade = 2f;
        [SerializeField] private float alcanceVisao = 14f;
        [SerializeField] private float intervaloTiro = 2.6f;
        [SerializeField] private float velocidadeTiro = 11f;
        [SerializeField] private ProjetilInimigo projetil;
        [SerializeField] private Transform pontoTiro;

        [Header("Visual e som")]
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer aura;
        [SerializeField] private Material materialAuraFogo;
        [SerializeField] private Material materialAuraAgua;
        [SerializeField] private Transform barraVida;
        [SerializeField] private AudioClip somDerrota;
        [SerializeField] private AudioClip somAbsorver;

        [Header("Recompensa")]
        [SerializeField] private Pista pistaAoMorrer;
        [SerializeField] private string idPistaAoMorrer = "";

        private static readonly int ParamAtingido = Animator.StringToHash("Atingido");

        private Rigidbody corpo;
        private Transform jogador;
        private Vector3 inicio;
        private bool indo = true;
        private float proximoTiro;
        private bool derrotado;

        public float Vida { get; private set; }
        public Elemento Afinidade => afinidade;
        public bool Derrotado => derrotado;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody>();
            corpo.isKinematic = true; // move por MovePosition, mas ainda colide com projéteis
            Vida = vidaMaxima;
            inicio = transform.position;
        }

        private void Start()
        {
            MagnusStatus magnus = FindAnyObjectByType<MagnusStatus>();
            if (magnus != null)
                jogador = magnus.transform;
            if (aura != null)
                aura.sharedMaterial = afinidade == Elemento.Fogo ? materialAuraFogo : materialAuraAgua;
            proximoTiro = Time.time + Random.Range(0.5f, intervaloTiro);
            AtualizarBarra();
        }

        private void FixedUpdate()
        {
            if (derrotado)
                return;

            if (VeJogador(out Vector3 paraJogador))
            {
                // Encara Magnus e atira de tempos em tempos.
                corpo.MoveRotation(Quaternion.Slerp(corpo.rotation, Quaternion.LookRotation(paraJogador), 0.15f));
                if (Time.time >= proximoTiro)
                {
                    proximoTiro = Time.time + intervaloTiro;
                    Atirar(paraJogador);
                }
                return;
            }

            // Patrulha de ida e volta.
            Vector3 destino = indo ? inicio + deslocamentoPatrulha : inicio;
            Vector3 passo = Vector3.MoveTowards(corpo.position, destino, velocidade * Time.fixedDeltaTime);
            Vector3 dir = destino - corpo.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                corpo.MoveRotation(Quaternion.Slerp(corpo.rotation, Quaternion.LookRotation(dir), 0.1f));
            corpo.MovePosition(passo);
            if ((passo - destino).sqrMagnitude < 0.01f)
                indo = !indo;
        }

        private void LateUpdate()
        {
            // A barra de vida sempre encara a câmera.
            if (barraVida != null && Camera.main != null)
                barraVida.parent.rotation = Camera.main.transform.rotation;
        }

        private bool VeJogador(out Vector3 direcao)
        {
            direcao = Vector3.zero;
            if (jogador == null)
                return false;
            Vector3 olhos = transform.position + Vector3.up * 1.6f;
            Vector3 alvo = jogador.position + Vector3.up * 1.0f;
            Vector3 delta = alvo - olhos;
            if (delta.magnitude > alcanceVisao)
                return false;
            // Linha de visão: paredes e barreiras escondem Magnus.
            if (Physics.Raycast(olhos, delta.normalized, out RaycastHit hit, delta.magnitude, ~0, QueryTriggerInteraction.Ignore)
                && hit.transform != jogador && !hit.transform.IsChildOf(jogador))
                return false;
            direcao = new Vector3(delta.x, 0f, delta.z).normalized;
            return true;
        }

        private void Atirar(Vector3 direcaoHorizontal)
        {
            if (projetil == null || jogador == null)
                return;
            Vector3 origem = pontoTiro != null ? pontoTiro.position : transform.position + Vector3.up * 1.4f;
            Vector3 alvo = jogador.position + Vector3.up * 1.0f;
            ProjetilInimigo p = Instantiate(projetil, origem, Quaternion.LookRotation(direcaoHorizontal));
            p.Disparar((alvo - origem).normalized * velocidadeTiro, afinidade);
        }

        public void ReceberElemento(Elemento elemento, float dano, Vector3 ponto)
        {
            if (derrotado)
                return;

            if (elemento == afinidade)
            {
                // Mesmo elemento: o guarda absorve e se cura.
                Vida = Mathf.Min(vidaMaxima, Vida + dano * 0.5f);
                GerenciadorAudio.Tocar(somAbsorver, 0.7f);
                GerenciadorPartida.Instancia?.MostrarAviso(
                    "O guarda de " + ElementoUtil.Nome(afinidade) + " ABSORVEU o ataque! Use " +
                    ElementoUtil.Nome(ElementoUtil.Oposto(afinidade)) + ".");
            }
            else
            {
                Vida -= dano;
                if (animator != null)
                    animator.SetTrigger(ParamAtingido);
            }
            AtualizarBarra();
            if (Vida <= 0f)
                Derrotar();
        }

        private void Derrotar()
        {
            derrotado = true;
            GerenciadorPartida.Instancia?.RegistrarPontos("guarda:" + idGuarda, pontos);
            GerenciadorAudio.Tocar(somDerrota);
            if (pistaAoMorrer != null)
            {
                Pista pista = Instantiate(pistaAoMorrer, transform.position + Vector3.up * 1.0f, Quaternion.identity);
                pista.Configurar(idPistaAoMorrer, "Pista 3: ordem assinada por Mark — \"Eliminem Magnus antes da lua cheia.\"");
            }
            Destroy(gameObject);
        }

        private void AtualizarBarra()
        {
            if (barraVida != null)
                barraVida.localScale = new Vector3(Mathf.Clamp01(Vida / vidaMaxima), 1f, 1f);
        }
    }
}
