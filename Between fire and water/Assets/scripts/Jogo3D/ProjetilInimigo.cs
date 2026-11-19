using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Disparo dos guardas. Usa um collider TRIGGER: ao entrar em contato com Magnus causa dano;
    /// ao tocar em qualquer superfície sólida, desaparece.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ProjetilInimigo : MonoBehaviour
    {
        [SerializeField] private float dano = 10f;
        [SerializeField] private float tempoDeVida = 4f;
        [SerializeField] private Renderer visual;
        [SerializeField] private Material materialFogo;
        [SerializeField] private Material materialAgua;

        public void Disparar(Vector3 velocidade, Elemento elemento)
        {
            if (visual != null)
                visual.sharedMaterial = elemento == Elemento.Fogo ? materialFogo : materialAgua;
            GetComponent<Rigidbody>().linearVelocity = velocidade;
            Destroy(gameObject, tempoDeVida);
        }

        private void OnTriggerEnter(Collider outro)
        {
            if (outro.isTrigger)
                return;
            MagnusStatus magnus = outro.GetComponent<MagnusStatus>();
            if (magnus != null)
            {
                magnus.ReceberDano(dano, "Magnus foi derrubado pelos guardas da ASDA.");
                Destroy(gameObject);
                return;
            }
            if (outro.GetComponentInParent<Guarda>() != null || outro.GetComponentInParent<ProjetilElemental>() != null)
                return; // atravessa outros guardas
            Destroy(gameObject);
        }
    }
}
