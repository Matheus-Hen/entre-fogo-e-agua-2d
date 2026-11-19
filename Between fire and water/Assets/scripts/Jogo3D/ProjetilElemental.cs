using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Bola de fogo ou de água lançada por Magnus. É um Rigidbody com gravidade (faz um arco).
    /// Ao colidir, avisa o alvo (IReceptorElemental). A bola de fogo também gera uma
    /// pequena explosão que empurra objetos físicos próximos (caixas).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ProjetilElemental : MonoBehaviour
    {
        [SerializeField] private Elemento elemento = Elemento.Fogo;
        [SerializeField] private float dano = 25f;
        [SerializeField] private float tempoDeVida = 3f;
        [SerializeField] private float forcaExplosao = 6f;
        [SerializeField] private float raioExplosao = 2.5f;

        private Rigidbody corpo;
        private bool atingiu;

        public Elemento Elemento => elemento;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody>();
        }

        public void Disparar(Vector3 velocidade, Collider[] ignorar)
        {
            Collider meu = GetComponent<Collider>();
            if (ignorar != null && meu != null)
                foreach (Collider c in ignorar)
                    if (c != null)
                        Physics.IgnoreCollision(meu, c);
            corpo.linearVelocity = velocidade;
            Destroy(gameObject, tempoDeVida);
        }

        private void OnCollisionEnter(Collision colisao)
        {
            if (atingiu)
                return;
            atingiu = true;

            Vector3 ponto = colisao.contactCount > 0 ? colisao.GetContact(0).point : transform.position;
            IReceptorElemental alvo = colisao.collider.GetComponentInParent<IReceptorElemental>();
            alvo?.ReceberElemento(elemento, dano, ponto);

            if (elemento == Elemento.Fogo)
            {
                // Resposta física: empurra caixas (Rigidbody) próximas.
                foreach (Collider c in Physics.OverlapSphere(ponto, raioExplosao))
                {
                    Rigidbody rb = c.attachedRigidbody;
                    if (rb != null && !rb.isKinematic && rb != corpo)
                        rb.AddExplosionForce(forcaExplosao, ponto, raioExplosao, 0.3f, ForceMode.VelocityChange);
                }
            }
            Destroy(gameObject);
        }
    }
}
