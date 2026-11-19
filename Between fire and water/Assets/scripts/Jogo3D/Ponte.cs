using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>Ponte que sobe do rio até a altura do chão quando ativada.</summary>
    public sealed class Ponte : MonoBehaviour
    {
        [SerializeField] private float alturaFinal = 0f;
        [SerializeField] private float velocidade = 3f;

        private bool subindo;

        public void Erguer()
        {
            subindo = true;
        }

        private void FixedUpdate()
        {
            if (!subindo)
                return;
            Vector3 p = transform.position;
            p.y = Mathf.MoveTowards(p.y, alturaFinal, velocidade * Time.fixedDeltaTime);
            transform.position = p;
            if (Mathf.Approximately(p.y, alturaFinal))
                subindo = false;
        }
    }
}
