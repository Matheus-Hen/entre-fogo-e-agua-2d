using UnityEngine;
using UnityEngine.InputSystem;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Controle de Magnus: WASD/setas movem em relação à câmera, Shift corre, Espaço pula.
    /// Usa CharacterController; ao encostar em um Rigidbody (caixa), empurra o objeto.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class MagnusMovimento : MonoBehaviour
    {
        [SerializeField] private float velocidadeAndar = 4.5f;
        [SerializeField] private float velocidadeCorrer = 7.5f;
        [SerializeField] private float alturaPulo = 1.3f;
        [SerializeField] private float gravidade = 20f;
        [SerializeField] private float velocidadeGiro = 12f;
        [SerializeField] private float forcaEmpurrar = 3f;
        [SerializeField] private Transform referenciaCamera;
        [SerializeField] private Animator animator;
        [SerializeField] private AudioClip somPulo;

        private static readonly int ParamVelocidade = Animator.StringToHash("Velocidade");

        private CharacterController controlador;
        private MagnusStatus status;
        private float velocidadeVertical;

        public Transform ReferenciaCamera
        {
            get => referenciaCamera;
            set => referenciaCamera = value;
        }

        private void Awake()
        {
            controlador = GetComponent<CharacterController>();
            status = GetComponent<MagnusStatus>();
            if (referenciaCamera == null && Camera.main != null)
                referenciaCamera = Camera.main.transform;
        }

        private void Update()
        {
            GerenciadorPartida partida = GerenciadorPartida.Instancia;
            if (partida != null && partida.Estado != EstadoPartida.Jogando)
                return;

            Vector2 entrada = LerEntrada(out bool correndo, out bool pulou);

            // Direção relativa à câmera (apenas no plano horizontal).
            Vector3 frente = Vector3.forward;
            Vector3 direita = Vector3.right;
            if (referenciaCamera != null)
            {
                frente = Vector3.ProjectOnPlane(referenciaCamera.forward, Vector3.up).normalized;
                direita = Vector3.ProjectOnPlane(referenciaCamera.right, Vector3.up).normalized;
            }
            Vector3 direcao = frente * entrada.y + direita * entrada.x;
            if (direcao.sqrMagnitude > 1f)
                direcao.Normalize();

            if (direcao.sqrMagnitude > 0.01f)
                VirarPara(direcao, velocidadeGiro * Time.deltaTime);

            // Gravidade e pulo.
            if (controlador.isGrounded && velocidadeVertical < 0f)
                velocidadeVertical = -2f;
            if (pulou && controlador.isGrounded)
            {
                velocidadeVertical = Mathf.Sqrt(2f * gravidade * alturaPulo);
                GerenciadorAudio.Tocar(somPulo, 0.6f);
            }
            velocidadeVertical -= gravidade * Time.deltaTime;

            float velocidade = correndo ? velocidadeCorrer : velocidadeAndar;
            Vector3 movimento = direcao * velocidade + Vector3.up * velocidadeVertical;
            controlador.Move(movimento * Time.deltaTime);

            if (animator != null)
                animator.SetFloat(ParamVelocidade, direcao.magnitude * (correndo ? 1f : 0.6f), 0.1f, Time.deltaTime);
        }

        private static Vector2 LerEntrada(out bool correndo, out bool pulou)
        {
            Vector2 entrada = Vector2.zero;
            correndo = false;
            pulou = false;
            Keyboard teclado = Keyboard.current;
            if (teclado == null)
                return entrada;

            if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) entrada.y += 1f;
            if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) entrada.y -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) entrada.x += 1f;
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) entrada.x -= 1f;
            correndo = teclado.leftShiftKey.isPressed || teclado.rightShiftKey.isPressed;
            pulou = teclado.spaceKey.wasPressedThisFrame;
            return entrada;
        }

        /// <summary>Gira Magnus para uma direção horizontal (usado também ao lançar poderes).</summary>
        public void VirarPara(Vector3 direcao, float suavizacao = 1f)
        {
            direcao.y = 0f;
            if (direcao.sqrMagnitude < 0.0001f)
                return;
            Quaternion alvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, alvo, Mathf.Clamp01(suavizacao));
        }

        /// <summary>Move Magnus instantaneamente (respawn em checkpoint).</summary>
        public void Teleportar(Vector3 posicao)
        {
            controlador.enabled = false;
            transform.position = posicao;
            controlador.enabled = true;
            velocidadeVertical = 0f;
        }

        // Chamado pelo CharacterController ao colidir durante Move().
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Encostar nas chamas machuca.
            BarreiraFogo barreira = hit.collider.GetComponentInParent<BarreiraFogo>();
            if (barreira != null && barreira.Acesa && status != null)
                status.ReceberDano(barreira.DanoContato, "As chamas da barreira queimaram Magnus.");

            // Encostar em um guarda também causa dano.
            if (status != null && hit.collider.GetComponentInParent<Guarda>() != null)
                status.ReceberDano(10f, "Magnus foi capturado por um guarda da ASDA.");

            // Empurrar caixas: aplica velocidade horizontal ao Rigidbody tocado.
            Rigidbody corpo = hit.collider.attachedRigidbody;
            if (corpo == null || corpo.isKinematic)
                return;
            if (hit.moveDirection.y < -0.3f)
                return; // pisando em cima, não empurra

            Vector3 empurrao = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
            corpo.linearVelocity = new Vector3(empurrao.x * forcaEmpurrar, corpo.linearVelocity.y, empurrao.z * forcaEmpurrar);
        }
    }
}
