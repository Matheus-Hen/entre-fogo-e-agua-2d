using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Cena01Director : MonoBehaviour
{
    private enum StoryBeat { KnockAtGate, ReportToEmperor, ReachOracle, OracleRevelation, ReturnToGate, Complete }

    [SerializeField] private Transform player;
    [SerializeField] private Transform goldenGate;
    [SerializeField] private Transform leftGateLeaf;
    [SerializeField] private Transform rightGateLeaf;
    [SerializeField] private Transform emperor;
    [SerializeField] private Transform oracle;
    [SerializeField] private Transform storyCamera;
    [SerializeField] private Cena01PlayerController playerController;
    [SerializeField] private float interactionDistance = 4f;
    [SerializeField] private Vector3 openingCameraPosition = new Vector3(-25f, 13f, -17f);
    [SerializeField] private Vector3 openingCameraTarget = new Vector3(-19f, 1f, -7f);
    [SerializeField] private float openingDuration = 4.5f;

    private StoryBeat beat = StoryBeat.KnockAtGate;
    private string dialogue = string.Empty;
    private bool showOpeningCard = true;
    private float openingCardTime = 4.5f;
    private bool gateOpened;
    private Quaternion leftGateClosedRotation;
    private Quaternion rightGateClosedRotation;
    private static GUIStyle titleStyle;
    private static GUIStyle bodyStyle;
    private static GUIStyle introStyle;
    private static GUIStyle subtitleStyle;

    private void Update()
    {
        if (leftGateLeaf != null && rightGateLeaf != null)
        {
            Quaternion leftTarget = gateOpened ? leftGateClosedRotation * Quaternion.Euler(0f, -82f, 0f) : leftGateClosedRotation;
            Quaternion rightTarget = gateOpened ? rightGateClosedRotation * Quaternion.Euler(0f, 82f, 0f) : rightGateClosedRotation;
            leftGateLeaf.localRotation = Quaternion.Slerp(leftGateLeaf.localRotation, leftTarget, 3f * Time.deltaTime);
            rightGateLeaf.localRotation = Quaternion.Slerp(rightGateLeaf.localRotation, rightTarget, 3f * Time.deltaTime);
        }

        if (showOpeningCard)
        {
            openingCardTime -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(openingCardTime / openingDuration);
            if (storyCamera != null && player != null)
            {
                Vector3 cameraEnd = player.position + new Vector3(0f, 3.3f, -6.3f);
                Vector3 targetEnd = player.position + Vector3.up * 1.3f + Vector3.forward * 3f;
                Vector3 cameraPosition = Vector3.Lerp(openingCameraPosition, cameraEnd, progress);
                Vector3 cameraTarget = Vector3.Lerp(openingCameraTarget, targetEnd, progress);
                storyCamera.position = cameraPosition;
                storyCamera.rotation = Quaternion.LookRotation(cameraTarget - cameraPosition);
            }

            if (openingCardTime <= 0f || (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame))
            {
                showOpeningCard = false;
                AttachGameplayCamera();
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.eKey.wasPressedThisFrame)
            return;

        switch (beat)
        {
            case StoryBeat.KnockAtGate:
                if (Near(goldenGate))
                {
                    dialogue = "Soldado: Majestade, a Nação do Fogo está produzindo armas poderosas. (Ele bate três vezes.)";
                    gateOpened = true;
                    beat = StoryBeat.ReportToEmperor;
                }
                break;
            case StoryBeat.ReportToEmperor:
                if (Near(emperor))
                {
                    dialogue = "Imperador: Chame o Oráculo!";
                    beat = StoryBeat.ReachOracle;
                }
                break;
            case StoryBeat.ReachOracle:
                if (Near(oracle))
                {
                    dialogue = "Oráculo: [Inserir aqui o texto da profecia — ainda não consta no roteiro.]";
                    beat = StoryBeat.OracleRevelation;
                }
                break;
            case StoryBeat.OracleRevelation:
                dialogue = "O soldado retorna aos portões e entrega o papiro ao Imperador.";
                beat = StoryBeat.ReturnToGate;
                if (player != null)
                {
                    player.SetPositionAndRotation(new Vector3(0f, 0f, -3.5f), Quaternion.identity);
                }
                break;
            case StoryBeat.ReturnToGate:
                if (Near(goldenGate))
                {
                    dialogue = "Imperador: Criem uma organização secreta. Encontrem e matem a criança.";
                    beat = StoryBeat.Complete;
                }
                break;
            case StoryBeat.Complete:
                dialogue = "Diz a lenda que a organização só ressurgirá quando o herói nascer...";
                beat = StoryBeat.KnockAtGate;
                break;
        }
    }

    private bool Near(Transform target)
    {
        return player != null && target != null &&
               Vector3.Distance(player.position, target.position) <= interactionDistance;
    }

    private void Start()
    {
        if (leftGateLeaf != null) leftGateClosedRotation = leftGateLeaf.localRotation;
        if (rightGateLeaf != null) rightGateClosedRotation = rightGateLeaf.localRotation;
        if (playerController == null && player != null)
            playerController = player.GetComponent<Cena01PlayerController>();
        if (playerController != null)
            playerController.enabled = false;
        if (storyCamera != null)
        {
            storyCamera.SetParent(null, true);
            storyCamera.position = openingCameraPosition;
            storyCamera.rotation = Quaternion.LookRotation(openingCameraTarget - openingCameraPosition);
        }
    }

    private void AttachGameplayCamera()
    {
        if (storyCamera != null && player != null)
        {
            storyCamera.SetParent(player, false);
            storyCamera.localPosition = new Vector3(0f, 3.3f, -6.3f);
            storyCamera.localRotation = Quaternion.Euler(18f, 0f, 0f);
        }
        if (playerController != null)
            playerController.enabled = true;
    }

    private string Objective
    {
        get
        {
            switch (beat)
            {
                case StoryBeat.KnockAtGate: return "Aproxime-se do portão dourado e bata três vezes.";
                case StoryBeat.ReportToEmperor: return "Entre no salão e fale com o Imperador.";
                case StoryBeat.ReachOracle: return "Corra pela floresta até a entrada da gruta.";
                case StoryBeat.OracleRevelation: return "Ouça a profecia e prepare-se para voltar.";
                case StoryBeat.ReturnToGate: return "Volte aos portões para a cena final.";
                default: return "Prólogo concluído. Pressione E para rever o encerramento.";
            }
        }
    }

    private bool CanInteract()
    {
        switch (beat)
        {
            case StoryBeat.KnockAtGate: return Near(goldenGate);
            case StoryBeat.ReportToEmperor: return Near(emperor);
            case StoryBeat.ReachOracle: return Near(oracle);
            case StoryBeat.ReturnToGate: return Near(goldenGate);
            default: return false;
        }
    }

    private void OnGUI()
    {
        EnsureStyles();
        float width = Mathf.Min(Screen.width - 40f, 760f);
        Rect box = new Rect((Screen.width - width) * 0.5f, Screen.height - 190f, width, 150f);
        GUI.color = new Color(0.06f, 0.04f, 0.03f, 0.92f);
        GUI.Box(box, GUIContent.none);
        GUI.color = Color.white;

        GUI.Label(new Rect(box.x + 20f, box.y + 12f, box.width - 40f, 28f), "CENA 1 — PRÓLOGO IMPERIAL | CHINA, 1000 D.C.", titleStyle);
        GUI.Label(new Rect(box.x + 20f, box.y + 44f, box.width - 40f, 42f), Objective, bodyStyle);
        if (!string.IsNullOrEmpty(dialogue))
            GUI.Label(new Rect(box.x + 20f, box.y + 86f, box.width - 40f, 38f), dialogue, bodyStyle);
        else if (CanInteract())
            GUI.Label(new Rect(box.x + 20f, box.y + 86f, box.width - 40f, 30f), "Pressione E para interagir", bodyStyle);

        GUI.Label(new Rect(20f, 20f, 440f, 28f), "WASD / Setas: mover     Shift: correr     E: interagir", bodyStyle);

        if (showOpeningCard)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.58f);
            GUI.Box(new Rect(0f, 0f, Screen.width, 125f), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(40f, 18f, Screen.width - 80f, 55f), "CHINA IMPERIAL • 1000 D.C.", introStyle);
            GUI.Label(new Rect(40f, 76f, Screen.width - 80f, 32f), "Uma mensagem urgente atravessa o palácio e a floresta.", subtitleStyle);
        }
    }

    private static void EnsureStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, alignment = TextAnchor.UpperLeft };
        introStyle = new GUIStyle(GUI.skin.label) { fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
    }
}
