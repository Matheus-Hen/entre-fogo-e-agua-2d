using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Caixa empurrável (Rigidbody com gravidade). Se cair no rio, reaparece no ponto inicial,
    /// evitando que o quebra-cabeça fique impossível.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CaixaEmpurravel : MonoBehaviour
    {
        [SerializeField] private float alturaMinima = -4f;

        private Rigidbody corpo;
        private Vector3 posicaoInicial;
        private Quaternion rotacaoInicial;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody>();
            posicaoInicial = transform.position;
            rotacaoInicial = transform.rotation;
        }

        private void FixedUpdate()
        {
            if (corpo.position.y < alturaMinima)
            {
                corpo.linearVelocity = Vector3.zero;
                corpo.angularVelocity = Vector3.zero;
                corpo.position = posicaoInicial + Vector3.up * 0.5f;
                corpo.rotation = rotacaoInicial;
                GerenciadorPartida.Instancia?.MostrarAviso("A caixa foi levada pelo rio e voltou ao acampamento.");
            }
        }
    }
}
