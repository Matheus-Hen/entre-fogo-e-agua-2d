using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Toca a música de fundo da cena (em loop) e os efeitos sonoros 2D.
    /// Existe um por cena; os outros scripts chamam GerenciadorAudio.Tocar(clip).
    /// </summary>
    public sealed class GerenciadorAudio : MonoBehaviour
    {
        public static GerenciadorAudio Instancia { get; private set; }

        [SerializeField] private AudioClip musica;
        [SerializeField, Range(0f, 1f)] private float volumeMusica = 0.35f;
        [SerializeField, Range(0f, 1f)] private float volumeEfeitos = 0.8f;

        private AudioSource fonteMusica;
        private AudioSource fonteEfeitos;

        public AudioSource FonteMusica => fonteMusica;

        private void Awake()
        {
            Instancia = this;
            fonteMusica = gameObject.AddComponent<AudioSource>();
            fonteMusica.clip = musica;
            fonteMusica.loop = true;
            fonteMusica.volume = volumeMusica;
            fonteMusica.playOnAwake = false;
            fonteMusica.spatialBlend = 0f;

            fonteEfeitos = gameObject.AddComponent<AudioSource>();
            fonteEfeitos.playOnAwake = false;
            fonteEfeitos.spatialBlend = 0f;
            fonteEfeitos.volume = volumeEfeitos;
        }

        private void Start()
        {
            if (musica != null)
                fonteMusica.Play();
        }

        private void OnDestroy()
        {
            if (Instancia == this)
                Instancia = null;
        }

        /// <summary>Toca um efeito sonoro. Pode ser chamado várias vezes seguidas (PlayOneShot sobrepõe).</summary>
        public static void Tocar(AudioClip clip, float volume = 1f)
        {
            if (clip == null || Instancia == null)
                return;
            Instancia.fonteEfeitos.PlayOneShot(clip, volume);
        }

        /// <summary>Abaixa a música quando a partida termina.</summary>
        public static void AbaixarMusica()
        {
            if (Instancia != null)
                Instancia.fonteMusica.volume = Instancia.volumeMusica * 0.3f;
        }
    }
}
