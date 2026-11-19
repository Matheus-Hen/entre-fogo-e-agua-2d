using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>Água do rio: quem cai perde vida e volta ao último checkpoint.</summary>
    public sealed class ZonaQueda : MonoBehaviour
    {
        private void OnTriggerEnter(Collider outro)
        {
            MagnusStatus magnus = outro.GetComponent<MagnusStatus>();
            if (magnus != null)
                magnus.Cair();
        }
    }
}
