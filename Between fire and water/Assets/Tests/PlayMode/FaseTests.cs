using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EntreFogoEAgua.Testes
{
    /// <summary>Testes de Play Mode do fluxo e das regras principais da fase.</summary>
    public class FaseTests
    {
        private GerenciadorPartida partida;
        private MagnusStatus magnus;

        [UnitySetUp]
        public IEnumerator CarregarFase()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(NomesCena.Fase);
            yield return null;
            yield return null;
            partida = GerenciadorPartida.Instancia;
            magnus = Object.FindAnyObjectByType<MagnusStatus>();
        }

        [UnityTearDown]
        public IEnumerator Limpar()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        private static T Campo<T>(object alvo, string nome) =>
            (T)alvo.GetType().GetField(nome, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(alvo);

        private Guarda GuardaPorId(string id) =>
            Object.FindObjectsByType<Guarda>(FindObjectsSortMode.None).First(g => Campo<string>(g, "idGuarda") == id);

        [Test]
        public void FaseTemElementosObrigatorios()
        {
            Assert.IsNotNull(partida);
            Assert.IsNotNull(magnus);
            Assert.IsNotNull(magnus.GetComponent<CharacterController>());
            Assert.GreaterOrEqual(Object.FindObjectsByType<Guarda>(FindObjectsSortMode.None).Length, 2);
            var caixas = Object.FindObjectsByType<CaixaEmpurravel>(FindObjectsSortMode.None);
            Assert.GreaterOrEqual(caixas.Length, 2);
            Assert.IsTrue(caixas.All(c => c.GetComponent<Rigidbody>().useGravity && !c.GetComponent<Rigidbody>().isKinematic));
            Assert.AreEqual(2, Object.FindObjectsByType<Pista>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(2, Object.FindObjectsByType<BarreiraFogo>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(EstadoPartida.Jogando, partida.Estado);
        }

        [Test]
        public void MesmoEventoNaoPontuaDuasVezes()
        {
            Assert.IsTrue(partida.RegistrarPontos("teste", 10));
            Assert.IsFalse(partida.RegistrarPontos("teste", 10));
            Assert.AreEqual(10, partida.Pontuacao);
        }

        [UnityTest]
        public IEnumerator EncostarNaPistaColetaUmaVez()
        {
            var pista = Object.FindObjectsByType<Pista>(FindObjectsSortMode.None).First(p => p.Id == "1");
            magnus.GetComponent<MagnusMovimento>().Teleportar(pista.transform.position + Vector3.up * 0.1f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(1, partida.Pistas);
            Assert.AreEqual(100, partida.Pontuacao);
            Assert.IsFalse(partida.ColetarPista("1", 100), "a mesma pista não pode contar de novo");
        }

        [Test]
        public void GuardaAbsorveProprioElementoESofreDoOposto()
        {
            var fogo = GuardaPorId("G1");
            float cheia = fogo.Vida;
            fogo.ReceberElemento(Elemento.Fogo, 25f, fogo.transform.position);
            Assert.AreEqual(cheia, fogo.Vida, 0.01f, "fogo cura (limitado à vida máxima)");
            fogo.ReceberElemento(Elemento.Agua, 25f, fogo.transform.position);
            Assert.AreEqual(cheia - 25f, fogo.Vida, 0.01f);
            fogo.ReceberElemento(Elemento.Fogo, 20f, fogo.transform.position);
            Assert.AreEqual(cheia - 15f, fogo.Vida, 0.01f, "fogo recupera metade do dano");
        }

        [UnityTest]
        public IEnumerator ProjetilDeFogoFisicoDerrubaGuardaDeAgua()
        {
            var guarda = GuardaPorId("G2");
            float vida = guarda.Vida;
            var poderes = magnus.GetComponent<MagnusPoderes>();
            var prefab = Campo<ProjetilElemental>(poderes, "projetilFogo");
            Vector3 origem = guarda.transform.position + new Vector3(0f, 1.2f, -1.5f);
            var proj = Object.Instantiate(prefab, origem, Quaternion.identity);
            proj.Disparar(Vector3.forward * 15f, null);
            yield return new WaitForSeconds(0.6f);
            Assert.Less(guarda.Vida, vida, "a bola de fogo (Rigidbody) deve colidir e ferir o guarda de água");
        }

        [Test]
        public void BarreiraDeFogoSoApagaComAgua()
        {
            var barreira = Object.FindObjectsByType<BarreiraFogo>(FindObjectsSortMode.None)
                .First(b => Campo<string>(b, "idBarreira") == "Acampamento");
            barreira.ReceberElemento(Elemento.Fogo, 25f, Vector3.zero);
            Assert.IsTrue(barreira.Acesa);
            barreira.ReceberElemento(Elemento.Agua, 25f, Vector3.zero);
            barreira.ReceberElemento(Elemento.Agua, 25f, Vector3.zero);
            Assert.IsFalse(barreira.Acesa);
            Assert.IsFalse(barreira.GetComponent<Collider>().enabled);
            Assert.AreEqual(25, partida.Pontuacao);
        }

        [Test]
        public void GeloDerreteComFogoEAguaReforca()
        {
            var gelo = Object.FindAnyObjectByType<BarreiraGelo>();
            gelo.ReceberElemento(Elemento.Agua, 25f, Vector3.zero);
            Assert.AreEqual(4, gelo.AcertosRestantes);
            for (int i = 0; i < 4; i++)
                gelo.ReceberElemento(Elemento.Fogo, 25f, Vector3.zero);
            Assert.IsFalse(gelo.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator CaixaNaPlacaErgueAPonte()
        {
            var placa = Object.FindAnyObjectByType<PlacaPressao>();
            var ponte = Object.FindAnyObjectByType<Ponte>();
            var caixa = Object.FindObjectsByType<CaixaEmpurravel>(FindObjectsSortMode.None)
                .OrderBy(c => Vector3.Distance(c.transform.position, placa.transform.position)).First();
            float alturaAntes = ponte.transform.position.y;
            caixa.GetComponent<Rigidbody>().position = placa.transform.position + Vector3.up * 0.3f;
            yield return new WaitForSeconds(2.5f);
            Assert.IsTrue(placa.Ativada);
            Assert.Greater(ponte.transform.position.y, alturaAntes + 2f);
        }

        [UnityTest]
        public IEnumerator CairNoRioCustaVidaEVoltaAoCheckpoint()
        {
            Vector3 inicio = magnus.transform.position;
            magnus.GetComponent<MagnusMovimento>().Teleportar(new Vector3(6f, 0.5f, 34f));
            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(magnus.VidaMaxima - 30f, magnus.Vida, 0.01f);
            Assert.Less(Vector3.Distance(magnus.transform.position, inicio), 1.5f);
        }

        [UnityTest]
        public IEnumerator DerrotaENovaTentativaRestauramEstado()
        {
            partida.RegistrarPontos("algo", 40);
            magnus.ReceberDano(1000f, "teste");
            Assert.AreEqual(EstadoPartida.Derrota, partida.Estado);
            Assert.AreEqual(0f, Time.timeScale);
            partida.Reiniciar();
            yield return null;
            yield return null;
            var novaPartida = GerenciadorPartida.Instancia;
            var novoMagnus = Object.FindAnyObjectByType<MagnusStatus>();
            Assert.AreEqual(EstadoPartida.Jogando, novaPartida.Estado);
            Assert.AreEqual(0, novaPartida.Pontuacao);
            Assert.AreEqual(novoMagnus.VidaMaxima, novoMagnus.Vida);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(1, Object.FindObjectsByType<MagnusStatus>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(4, Object.FindObjectsByType<Guarda>(FindObjectsSortMode.None).Length);
        }

        [UnityTest]
        public IEnumerator TresPistasAbremPortaoESaidaDaVitoria()
        {
            var saida = Object.FindAnyObjectByType<SaidaFase>();
            magnus.GetComponent<MagnusMovimento>().Teleportar(saida.transform.position + Vector3.back * 6f + Vector3.down);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(EstadoPartida.Jogando, partida.Estado, "sem pistas não há vitória");

            partida.ColetarPista("1", 100);
            partida.ColetarPista("2", 100);
            partida.ColetarPista("3", 100);
            yield return null;
            Assert.IsTrue(Object.FindAnyObjectByType<PortaoBase>().Aberto);

            magnus.GetComponent<MagnusMovimento>().Teleportar(saida.transform.position + Vector3.down * 1.4f);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.AreEqual(EstadoPartida.Vitoria, partida.Estado);
            Assert.GreaterOrEqual(partida.Pontuacao, 300);
            partida.Reiniciar();
            yield return null;
            yield return null;
            Assert.AreEqual(0, GerenciadorPartida.Instancia.Pistas);
        }
    }

    public class MenuTests
    {
        [UnityTest]
        public IEnumerator BotaoJogarCarregaAFase()
        {
            SceneManager.LoadScene(NomesCena.Menu);
            yield return null;
            Object.FindAnyObjectByType<MenuPrincipal>().BotaoJogar();
            yield return null;
            yield return null;
            Assert.AreEqual(NomesCena.Fase, SceneManager.GetActiveScene().name);
        }
    }
}
