using UnityEngine;

namespace EntreFogoEAgua
{
    /// <summary>Os dois poderes de Magnus.</summary>
    public enum Elemento
    {
        Fogo,
        Agua
    }

    /// <summary>
    /// Qualquer objeto que reage quando é atingido por um projétil elemental
    /// (guardas, barreira de fogo, barreira de gelo).
    /// </summary>
    public interface IReceptorElemental
    {
        void ReceberElemento(Elemento elemento, float dano, Vector3 ponto);
    }

    public static class ElementoUtil
    {
        public static Elemento Oposto(Elemento e) => e == Elemento.Fogo ? Elemento.Agua : Elemento.Fogo;

        public static string Nome(Elemento e) => e == Elemento.Fogo ? "Fogo" : "Água";

        public static Color Cor(Elemento e) =>
            e == Elemento.Fogo ? new Color(1f, 0.45f, 0.1f) : new Color(0.2f, 0.65f, 1f);
    }

    /// <summary>Nomes das cenas usados nas transições.</summary>
    public static class NomesCena
    {
        public const string Menu = "Menu";
        public const string Fase = "Fase01_BaseASDA";
    }
}
