using UnityEngine;
using UnityEngine.InputSystem;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Câmera em terceira pessoa que segue Magnus. O mouse gira a câmera (horizontal e vertical).
    /// Um SphereCast aproxima a câmera quando há parede entre ela e o personagem.
    /// </summary>
    public sealed class CameraTerceiraPessoa : MonoBehaviour
    {
        [SerializeField] private Transform alvo;
        [SerializeField] private float distancia = 6.5f;
        [SerializeField] private float alturaAlvo = 1.5f;
        [SerializeField] private float sensibilidade = 0.12f;
        [SerializeField] private float inclinacaoMin = 5f;
        [SerializeField] private float inclinacaoMax = 55f;
        [SerializeField] private LayerMask obstaculos = ~0;

        private float yaw;
        private float pitch = 20f;

        public Transform Alvo
        {
            get => alvo;
            set => alvo = value;
        }

        private void Start()
        {
            if (alvo != null)
                yaw = alvo.eulerAngles.y;
        }

        private void LateUpdate()
        {
            if (alvo == null)
                return;

            GerenciadorPartida partida = GerenciadorPartida.Instancia;
            bool jogando = partida == null || partida.Estado == EstadoPartida.Jogando;
            Mouse mouse = Mouse.current;
            if (jogando && mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * sensibilidade;
                pitch = Mathf.Clamp(pitch - delta.y * sensibilidade, inclinacaoMin, inclinacaoMax);
            }
            Keyboard teclado = Keyboard.current;
            if (jogando && teclado != null)
            {
                // Alternativa sem mouse: J/L giram a câmera.
                if (teclado.jKey.isPressed) yaw -= 120f * Time.deltaTime;
                if (teclado.lKey.isPressed) yaw += 120f * Time.deltaTime;
            }

            Quaternion rotacao = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 foco = alvo.position + Vector3.up * alturaAlvo;
            Vector3 desejada = foco - rotacao * Vector3.forward * distancia;

            float dist = distancia;
            if (Physics.SphereCast(foco, 0.3f, desejada - foco, out RaycastHit hit, distancia, obstaculos, QueryTriggerInteraction.Ignore)
                && hit.collider.attachedRigidbody == null)
                dist = Mathf.Max(1f, hit.distance - 0.1f);

            transform.position = foco - rotacao * Vector3.forward * dist;
            transform.rotation = rotacao;
        }
    }
}
