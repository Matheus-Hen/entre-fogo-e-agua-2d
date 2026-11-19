using UnityEngine;
using UnityEngine.SceneManagement;

namespace EntreFogoEAgua
{
    /// <summary>
    /// Menu inicial (cena Menu). O botão Jogar carrega a cena da fase.
    /// </summary>
    public sealed class MenuPrincipal : MonoBehaviour
    {
        [SerializeField] private GameObject painelComoJogar;
        [SerializeField] private Transform vitrine;
        [SerializeField] private float giroVitrine = 20f;
        [SerializeField] private AudioClip somClique;

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (painelComoJogar != null)
                painelComoJogar.SetActive(false);
        }

        private void Update()
        {
            if (vitrine != null)
                vitrine.Rotate(0f, giroVitrine * Time.deltaTime, 0f);
        }

        public void BotaoJogar()
        {
            GerenciadorAudio.Tocar(somClique);
            SceneManager.LoadScene(NomesCena.Fase);
        }

        public void BotaoComoJogar()
        {
            GerenciadorAudio.Tocar(somClique);
            if (painelComoJogar != null)
                painelComoJogar.SetActive(!painelComoJogar.activeSelf);
        }

        public void BotaoSair()
        {
            GerenciadorAudio.Tocar(somClique);
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
