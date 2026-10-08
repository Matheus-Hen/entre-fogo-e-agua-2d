using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

// Additive runtime campaign. The user's scene and movement script are not rewritten.
public sealed class EntregaBasica : MonoBehaviour
{
    const string SaveKey = "EntreFogoAgua.Campanha.1";
    const float UiWidth = 1100f;
    const float UiHeight = 700f;
    enum Mode { Menu, Explore, Dialogue, Battle, Victory, Defeat, Pause, Settings, ConfirmNewGame, ConfirmDeleteSave, Transition, Ending }
    [Serializable] class Progress
    {
        public int schema = 1;
        public int phase, xp, hp = 100, energy = 8;
        public bool[] won = new bool[6];
        public bool[] clues = new bool[3];
        public bool finished;
    }
    class Point
    {
        public string title; public Vector2 position; public int kind, index;
        public GameObject view;
    }
    readonly string[] phases = { "Acampamento e templo", "Posto DASA", "Base ASDA" };
    readonly string[] names = { "Ki — treinamento", "Guarda da trilha", "Guarda DASA", "Guarda do arquivo", "Elite ASDA", "Mark" };
    readonly int[] enemyHealth = { 38, 52, 66, 76, 94, 125 };
    readonly int[] enemyPower = { 9, 12, 14, 16, 19, 22 };
    readonly List<Point> points = new List<Point>();
    Progress data = new Progress();
    Mode mode = Mode.Menu;
    Transform map, hero;
    Rigidbody2D body;
    Animator animator;
    Camera cam;
    Sprite square;
    Material unlit;
    GameObject markerPrefab;
    Sprite[] explorationMaps = new Sprite[3];
    AudioSource speaker;
    AudioSource footsteps;
    AudioClip cue;
    AudioClip waterAttackCue, fireAttackCue, walkingCue;
    VideoClip finalCreditsClip;
    VideoPlayer finalCreditsPlayer;
    RenderTexture finalCreditsTexture;
    Texture2D[] backgrounds;
    Texture2D portrait, ki, guards, mark;
    Vector2 motion;
    string dialogue = "", log = "", notice = "";
    int enemy, enemyHP, startHP, startEnergy;
    bool busy, bossCharging;
    Mode settingsReturnMode = Mode.Menu;
    float effectUntil, fadeAlpha;
    Color effectColor;
    GUIStyle titleStyle, textStyle, smallStyle, buttonStyle;
    int Level => 1 + (data.xp >= 100 ? 2 : data.xp >= 40 ? 1 : 0);
    int MaxHP => 100 + (Level - 1) * 25;
    float UiScale => Mathf.Min(Screen.width / UiWidth, Screen.height / UiHeight);
    Vector2 UiOffset => new Vector2((Screen.width - UiWidth * UiScale) * .5f, (Screen.height - UiHeight * UiScale) * .5f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene" || FindAnyObjectByType<EntregaBasica>() != null) return;
        new GameObject("Campanha — entrega básica").AddComponent<EntregaBasica>();
    }

    void Start()
    {
        if (Application.isMobilePlatform) Screen.orientation = ScreenOrientation.LandscapeLeft;
        var source = FindAnyObjectByType<PlayerMoviment>();
        if (source == null) { Debug.LogError("Campanha: Magnus com PlayerMoviment não foi encontrado."); enabled = false; return; }
        hero = source.transform;
        body = hero.GetComponent<Rigidbody2D>();
        animator = hero.GetComponent<Animator>();
        var sprite = hero.GetComponent<SpriteRenderer>();
        if (body == null || sprite == null || sprite.sprite == null)
        {
            Debug.LogError("Campanha: Magnus precisa de Rigidbody2D e SpriteRenderer com sprite.");
            enabled = false;
            return;
        }
        source.enabled = false;
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        hero.localScale = Vector3.one * (1.3f / Mathf.Max(.1f, sprite.sprite.bounds.size.y));
        foreach (var collider in hero.GetComponents<Collider2D>()) collider.enabled = false;
        var feet = hero.gameObject.AddComponent<CircleCollider2D>();
        feet.radius = .28f / hero.localScale.x;
        feet.offset = Vector2.zero;
        sprite.sortingOrder = 10;
        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
        if (shader != null) { unlit = new Material(shader); sprite.sharedMaterial = unlit; }
        if (animator != null && animator.layerCount > 0) animator.SetLayerWeight(0, 1);
        cam = Camera.main;
        if (cam == null) { cam = new GameObject("Campaign Camera").AddComponent<Camera>(); cam.tag = "MainCamera"; }
        cam.orthographic = true;
        cam.transform.SetPositionAndRotation(new Vector3(0, 0, -10), Quaternion.identity);
        cam.backgroundColor = new Color(.035f, .05f, .07f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, Color.white); tex.Apply();
        square = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        markerPrefab = Resources.Load<GameObject>("EntregaBasica/Marco");
        backgrounds = new[] { Load("camp"), Load("post"), Load("base") };
        explorationMaps = new[] {
            Resources.Load<Sprite>("EntregaBasica/acampamento-exploracao"),
            Resources.Load<Sprite>("EntregaBasica/asda-exploracao"),
            Resources.Load<Sprite>("EntregaBasica/base-asda")
        };
        portrait = Load("magnus"); ki = Load("ki"); guards = Load("guards"); mark = Load("mark");
        speaker = gameObject.AddComponent<AudioSource>();
        speaker.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(SaveKey + ".volume", .2f));
        speaker.playOnAwake = false;
        speaker.spatialBlend = 0;
        fireAttackCue = Resources.Load<AudioClip>("EntregaBasica/fireball");
        waterAttackCue = Resources.Load<AudioClip>("EntregaBasica/waterball");
        walkingCue = Resources.Load<AudioClip>("EntregaBasica/walking");
        footsteps = gameObject.AddComponent<AudioSource>();
        footsteps.clip = walkingCue;
        footsteps.loop = true;
        footsteps.playOnAwake = false;
        footsteps.spatialBlend = 0;
        footsteps.volume = speaker.volume;
        finalCreditsClip = Resources.Load<VideoClip>("EntregaBasica/creditoFInal");
        if (finalCreditsClip != null)
        {
            finalCreditsTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
            finalCreditsTexture.Create();
            finalCreditsPlayer = gameObject.AddComponent<VideoPlayer>();
            finalCreditsPlayer.playOnAwake = false;
            finalCreditsPlayer.source = VideoSource.VideoClip;
            finalCreditsPlayer.clip = finalCreditsClip;
            finalCreditsPlayer.renderMode = VideoRenderMode.RenderTexture;
            finalCreditsPlayer.targetTexture = finalCreditsTexture;
            finalCreditsPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        }
        cue = AudioClip.Create("Sinal sintetizado", 4410, 1, 44100, false);
        var samples = new float[4410];
        for (int i = 0; i < samples.Length; i++) samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 440 / 44100) * .18f * (1f - i / 4410f);
        cue.SetData(samples, 0);
        hero.gameObject.SetActive(false);
    }
    Texture2D Load(string name)
    {
        var sprite = Resources.Load<Sprite>("EntregaBasica/" + name);
        return sprite != null ? sprite.texture : Resources.Load<Texture2D>("EntregaBasica/" + name);
    }
    void Sound(float pitch = 1) { if (speaker == null || cue == null) return; speaker.pitch = pitch; speaker.PlayOneShot(cue); }

    void UpdateFootsteps()
    {
        if (footsteps == null || walkingCue == null) return;
        bool shouldPlay = mode == Mode.Explore && motion.sqrMagnitude > .01f;
        if (shouldPlay && !footsteps.isPlaying) footsteps.Play();
        else if (!shouldPlay && footsteps.isPlaying) footsteps.Stop();
    }

    void Update()
    {
        if (hero == null) return;
        cam.orthographicSize = Mathf.Max(6.7f, 11.3f / Mathf.Max(.3f, cam.aspect));
        motion = Vector2.zero;
        var keys = Keyboard.current;
        if (keys != null && keys.escapeKey.wasPressedThisFrame)
        {
            if (mode == Mode.Explore) mode = Mode.Pause;
            else if (mode == Mode.Pause) mode = Mode.Explore;
        }
        if (mode == Mode.Explore && keys != null)
        {
            motion = new Vector2((keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1 : 0) - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1 : 0),
                (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1 : 0) - (keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1 : 0)).normalized;
            if (keys.eKey.wasPressedThisFrame) Interact();
        }
        if (mode == Mode.Explore) motion = ReadTouchMovement(motion);
        if (animator != null)
        {
            animator.SetFloat("Horizontal", motion.x); animator.SetFloat("Vertical", motion.y); animator.SetFloat("Speed", motion.sqrMagnitude);
        }
        UpdateFootsteps();
        if (mode != Mode.Explore) body.linearVelocity = Vector2.zero;
    }
    void FixedUpdate() { if (body != null) body.linearVelocity = mode == Mode.Explore ? motion * 4 : Vector2.zero; }

    Vector2 ReadTouchMovement(Vector2 keyboardMovement)
    {
        var touch = Touchscreen.current != null ? Touchscreen.current.primaryTouch : null;
        if (touch == null || !touch.press.isPressed) return keyboardMovement;
        Vector2 screen = touch.position.ReadValue();
        Vector2 offset = UiOffset;
        Vector2 ui = new Vector2((screen.x - offset.x) / Mathf.Max(.01f, UiScale), UiHeight - (screen.y - offset.y) / Mathf.Max(.01f, UiScale));
        Vector2 center = new Vector2(105, 575);
        Vector2 delta = ui - center;
        if (ui.x < 205 && ui.y > 475) return Vector2.ClampMagnitude(delta / 58f, 1f);
        return keyboardMovement;
    }

    GameObject Block(string name, Vector2 pos, Vector2 size, Color color, bool solid = false)
    {
        var go = markerPrefab != null ? Instantiate(markerPrefab, map) : new GameObject(name);
        go.name = name; go.transform.SetParent(map); go.transform.position = pos; go.transform.localScale = size;
        var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = square; renderer.color = color;
        if (unlit != null) renderer.sharedMaterial = unlit;
        if (solid) go.AddComponent<BoxCollider2D>();
        return go;
    }
    void CollisionBoundary(string name, Vector2 pos, Vector2 size)
    {
        var boundary = new GameObject(name);
        boundary.transform.SetParent(map);
        boundary.transform.position = pos;
        boundary.transform.localScale = size;
        boundary.AddComponent<BoxCollider2D>();
    }
    void AddPoint(string name, float x, float y, int kind, int index)
    {
        var p = new Point { title = name, position = new Vector2(x, y), kind = kind, index = index };
        p.view = Block(name, p.position, kind == 2 ? new Vector2(.6f, 1.5f) : Vector2.one * .7f,
            kind == 0 ? new Color(.9f,.3f,.2f) : kind == 1 ? new Color(.2f,.7f,1) : new Color(.3f,.85f,.5f));
        points.Add(p);
    }
    void BuildMap()
    {
        if (map != null) { map.gameObject.SetActive(false); Destroy(map.gameObject); }
        points.Clear(); map = new GameObject(phases[data.phase]).transform; map.SetParent(transform);
        Color floor = data.phase == 0 ? new Color(.16f,.24f,.17f) : data.phase == 1 ? new Color(.16f,.21f,.28f) : new Color(.25f,.15f,.16f);
        var ground = Block("Chão", Vector2.zero, new Vector2(20, 10), floor); ground.GetComponent<SpriteRenderer>().sortingOrder = -10;
        var mapSprite = explorationMaps[data.phase];
        if (mapSprite != null)
        {
            var groundRenderer = ground.GetComponent<SpriteRenderer>();
            groundRenderer.sprite = mapSprite;
            groundRenderer.color = Color.white;
            ground.transform.localScale = new Vector2(mapSprite.bounds.size.x / mapSprite.bounds.size.y * 12f / mapSprite.bounds.size.x, 12f / mapSprite.bounds.size.y);
        }
        CollisionBoundary("Limite norte", new Vector2(0,5), new Vector2(20,.4f));
        CollisionBoundary("Limite sul", new Vector2(0,-5), new Vector2(20,.4f));
        CollisionBoundary("Limite oeste", new Vector2(-10,0), new Vector2(.4f,10));
        CollisionBoundary("Limite leste", new Vector2(10,0), new Vector2(.4f,10));
        int n = data.phase * 2;
        if (!data.won[n]) AddPoint(names[n], -5, 0, 0, n);
        if (!data.won[n+1]) AddPoint(names[n+1], 6, 0, 0, n+1);
        if (data.phase == 0 && !data.clues[0]) AddPoint("Pista: templo", 0, 3.9f, 1, 0);
        if (data.phase == 1)
        {
            if (!data.clues[1]) AddPoint("Pista: identificação", 0, 3.9f, 1, 1);
            if (!data.clues[2]) AddPoint("Pista: localização", 7, -3.5f, 1, 2);
        }
        if (data.phase == 2) AddPoint("Recuperação", 1, 0, 3, 0);
        AddPoint(data.phase == 2 ? "Desfecho" : "Próxima área", 9, 3.5f, 2, 0);
        hero.gameObject.SetActive(true); body.position = new Vector2(-8,-2); body.linearVelocity = Vector2.zero;
        mode = Mode.Explore;
    }
    string Objective()
    {
        int n = data.phase * 2;
        if (!data.won[n]) return "Vença " + names[n] + ".";
        if (data.phase == 0 && !data.clues[0]) return "Investigue a pista no templo.";
        if (data.phase == 1 && !data.clues[1]) return "Examine a identificação no arquivo.";
        if (!data.won[n+1]) return "Vença " + names[n+1] + ".";
        if (data.phase == 1 && !data.clues[2]) return "Examine a localização da base.";
        return data.phase == 2 ? "Siga ao desfecho." : "Siga à próxima área.";
    }
    Point Nearest()
    {
        Point best = null; float distance = 1.65f;
        foreach (var p in points)
        {
            if (p.view == null || !p.view.activeSelf) continue;
            float d = Vector2.Distance(body.position, p.position);
            if (d < distance) { best = p; distance = d; }
        }
        return best;
    }
    void Talk(string text) { dialogue = text; mode = Mode.Dialogue; Sound(1.4f); }
    void Interact()
    {
        var p = Nearest(); if (p == null) return;
        int n = data.phase * 2;
        if (p.kind == 0)
        {
            if (p.index == n+1 && (!data.won[n] || (data.phase == 0 && !data.clues[0]) || (data.phase == 1 && !data.clues[1]))) { Talk(Objective()); return; }
            enemy = p.index; enemyHP = enemyHealth[enemy]; startHP = data.hp; startEnergy = data.energy;
            busy = false; bossCharging = false;
            log = enemy == 0 ? "Ki: domine fogo, água e defesa. É hora de treinar." : enemy == 5 ? "Mark observa Magnus. Ele prepara golpes fortes e anuncia quando vai atacar." : "Escolha sua ação. O inimigo responde após o seu turno.";
            mode = Mode.Battle; return;
        }
        if (p.kind == 1)
        {
            if (!data.won[n] || (p.index == 2 && !data.won[n+1])) { Talk(Objective()); return; }
            data.clues[p.index] = true; p.view.SetActive(false); Save();
            Talk(p.index == 0 ? "Magnus encontra ordens no templo: a ASDA pretende eliminá-lo. Lizzie ajuda na fuga. A trilha está liberada."
                : p.index == 1 ? "A identificação confirma: DASA é uma fachada da ASDA. Os aliados indicam o terminal de segurança."
                : "O terminal revela a localização da base ASDA. Com as duas pistas, o grupo pode enfrentar a organização."); return;
        }
        if (p.kind == 3) { data.hp = MaxHP; data.energy = 8; Save(); Talk("Os aliados ajudam Magnus a recuperar vida e energia antes de enfrentar Mark."); return; }
        if (!data.won[n] || !data.won[n+1] || (data.phase == 0 && !data.clues[0]) || (data.phase == 1 && (!data.clues[1] || !data.clues[2]))) { Talk(Objective()); return; }
        if (data.phase == 2) { data.finished = true; Save(); mode = Mode.Ending; StartFinalCredits(); Sound(1.8f); }
        else StartCoroutine(TransitionToNextPhase());
    }
    void Save()
    {
        data.schema = 1;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    bool TryReadSave(out Progress saved)
    {
        saved = null;
        if (!PlayerPrefs.HasKey(SaveKey)) return false;
        try
        {
            saved = JsonUtility.FromJson<Progress>(PlayerPrefs.GetString(SaveKey));
            if (saved == null || saved.schema < 0 || saved.schema > 1 || saved.phase < 0 || saved.phase > 2 || saved.xp < 0 || saved.xp > 120 ||
                saved.won == null || saved.won.Length != 6 || saved.clues == null || saved.clues.Length != 3 || saved.energy < 0 || saved.energy > 8)
            { saved = null; return false; }
            int victories = 0;
            foreach (bool won in saved.won) if (won) victories++;
            bool[] fights = saved.won;
            bool[] clues = saved.clues;
            bool invalidOrder = (clues[0] && !fights[0]) || (fights[1] && !clues[0]) || (clues[1] && !fights[2]) ||
                (fights[3] && !clues[1]) || (clues[2] && !fights[3]) || (fights[5] && !fights[4]) ||
                (saved.phase >= 1 && (!fights[0] || !fights[1] || !clues[0])) ||
                (saved.phase >= 2 && (!fights[2] || !fights[3] || !clues[1] || !clues[2])) ||
                (saved.finished && (saved.phase != 2 || !fights[4] || !fights[5]));
            if (invalidOrder || saved.xp != victories * 20) { saved = null; return false; }
            saved.schema = 1; // preserves saves created before schema tracking
            return true;
        }
        catch { saved = null; return false; }
    }
    void StartNewGame()
    {
        data = new Progress();
        notice = "";
        Save();
        BuildMap();
        Talk("Magnus chega ao acampamento. Procure Ki para treinar e investigue o templo. Aproxime-se dos marcos e pressione E.");
    }
    void RequestNewGame()
    {
        if (PlayerPrefs.HasKey(SaveKey)) mode = Mode.ConfirmNewGame;
        else StartNewGame();
    }
    void RequestDeleteSave()
    {
        if (PlayerPrefs.HasKey(SaveKey)) mode = Mode.ConfirmDeleteSave;
    }
    void SetVolume(float volume)
    {
        speaker.volume = Mathf.Clamp01(volume);
        if (footsteps != null) footsteps.volume = speaker.volume;
        PlayerPrefs.SetFloat(SaveKey + ".volume", speaker.volume);
        PlayerPrefs.Save();
    }
    void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        data = new Progress();
        notice = "Progresso apagado.";
        mode = Mode.Menu;
    }
    void RetryBattle()
    {
        data.hp = startHP;
        data.energy = startEnergy;
        enemyHP = enemyHealth[enemy];
        busy = false;
        bossCharging = false;
        log = "Nova tentativa. Sua progressão anterior foi preservada.";
        mode = Mode.Battle;
    }
    IEnumerator TransitionToNextPhase()
    {
        mode = Mode.Transition;
        motion = Vector2.zero;
        while (fadeAlpha < 1f) { fadeAlpha = Mathf.MoveTowards(fadeAlpha, 1f, Time.unscaledDeltaTime * 2.5f); yield return null; }
        data.phase++;
        data.hp = MaxHP;
        data.energy = 8;
        Save();
        BuildMap();
        Talk(data.phase == 1 ? "Posto DASA: investigue os arquivos e descubra onde fica a base." : "Base ASDA: vença a elite, prepare-se e enfrente Mark.");
        while (fadeAlpha > 0f) { fadeAlpha = Mathf.MoveTowards(fadeAlpha, 0f, Time.unscaledDeltaTime * 2.5f); yield return null; }
    }
    void Continue()
    {
        if (!TryReadSave(out var saved)) { notice = "O save não existe ou está inválido. Apague-o ou comece um novo jogo."; return; }
        notice = "";
        data = saved;
        data.hp = Mathf.Clamp(data.hp, 1, MaxHP);
        data.energy = Mathf.Clamp(data.energy, 0, 8);
        BuildMap();
        if (data.finished) { mode = Mode.Ending; StartFinalCredits(); }
    }
    void StartFinalCredits()
    {
        if (finalCreditsPlayer == null || finalCreditsClip == null) return;
        StartCoroutine(PrepareAndPlayFinalCredits());
    }
    IEnumerator PrepareAndPlayFinalCredits()
    {
        finalCreditsPlayer.Stop();
        finalCreditsPlayer.Prepare();
        while (!finalCreditsPlayer.isPrepared) yield return null;
        if (finalCreditsPlayer.audioTrackCount > 0)
        {
            finalCreditsPlayer.EnableAudioTrack(0, true);
            finalCreditsPlayer.SetTargetAudioSource(0, speaker);
        }
        speaker.pitch = 1f;
        finalCreditsPlayer.Play();
    }
    IEnumerator Act(int action)
    {
        if (busy || mode != Mode.Battle) yield break;
        int cost = action == 1 ? 3 : action == 2 ? 2 : 0;
        if (data.energy < cost) yield break;
        busy = true; data.energy -= cost;
        int damage = action == 3 ? 0 : action == 1 ? 24 + Level * 4 : action == 2 ? 13 + Level * 3 : 11 + Level * 3;
        if (action == 3) data.energy = Mathf.Min(8,data.energy + 3);
        enemyHP = Mathf.Max(0,enemyHP - damage);
        log = action == 3 ? "Magnus se defende e recupera 3 de energia." : "Magnus usa " + new[] { "ataque", "fogo", "água" }[action] + ": " + damage + " de dano.";
        effectColor = action == 1 ? new Color(1,.35f,.1f,.4f) : action == 2 ? new Color(.1f,.65f,1,.4f) : new Color(1,1,1,.2f);
        effectUntil = Time.unscaledTime + .35f;
        AudioClip actionCue = action == 1 ? fireAttackCue : action == 2 ? waterAttackCue : null;
        if (actionCue != null) { speaker.pitch = 1f; speaker.PlayOneShot(actionCue); }
        else Sound(action == 1 ? .7f : 1.4f);
        yield return new WaitForSecondsRealtime(.65f);
        if (enemyHP == 0)
        {
            if (!data.won[enemy])
            {
                int oldLevel = Level; data.won[enemy] = true; data.xp += 20;
                if (Level > oldLevel) data.hp = MaxHP;
                data.hp = Mathf.Min(MaxHP, data.hp + 20); data.energy = Mathf.Min(8,data.energy + 3);
            }
            foreach (var p in points) if (p.kind == 0 && p.index == enemy) p.view.SetActive(false);
            Save(); mode = Mode.Victory; busy = false; Sound(1.8f); yield break;
        }
        bool heavyStrike = enemy == 5 && bossCharging;
        int baseHit = enemyPower[enemy] * (heavyStrike ? 2 : 1);
        int hit = Mathf.Max(1,Mathf.RoundToInt(baseHit * (action == 3 ? .4f : action == 2 ? .6f : 1)));
        data.hp = Mathf.Max(0,data.hp - hit);
        log += heavyStrike ? "\nMark desfere o golpe anunciado e causa " + hit + " de dano." : "\n" + names[enemy] + " causa " + hit + " de dano.";
        if (enemy == 5) { bossCharging = !bossCharging; if (bossCharging) log += "\nMark começa a preparar um ataque forte."; }
        Sound(.6f);
        if (data.hp == 0) mode = Mode.Defeat;
        busy = false;
    }
    void MainMenu()
    {
        if (finalCreditsPlayer != null) finalCreditsPlayer.Stop();
        mode = Mode.Menu; hero.gameObject.SetActive(false); if (map != null) map.gameObject.SetActive(false);
    }

    void Styles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, wordWrap = true };
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 19, wordWrap = true };
    }
    bool Button(string label, float x, float y, float w = 260, float h = 46) => GUI.Button(new Rect(x,y,w,h), label, buttonStyle);
    void Label(string label, float x, float y, float w, float h, bool title = false) => GUI.Label(new Rect(x,y,w,h), label,title ? titleStyle : textStyle);
    void Panel(float x, float y, float w, float h) { var old = GUI.color; GUI.color = new Color(.04f,.065f,.09f,.97f); GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture); GUI.color = old; }
    void Art(Texture texture, Rect rect) { if (texture != null) GUI.DrawTexture(rect,texture,ScaleMode.ScaleToFit); }
    void OnGUI()
    {
        if (hero == null || backgrounds == null) return;
        Styles(); var matrix = GUI.matrix; GUI.matrix = Matrix4x4.TRS(UiOffset,Quaternion.identity,Vector3.one * UiScale);
        if (mode == Mode.Menu)
        {
            Art(backgrounds[0],new Rect(0,0,1100,700)); Panel(70,70,620,550);
            Label("ENTRE FOGO E ÁGUA",100,100,560,60,true);
            Label("Uma campanha de Magnus\nTrês fases • seis encontros • fogo e água",100,175,550,95);
            bool hasSave = PlayerPrefs.HasKey(SaveKey);
            if (Button("Novo jogo",100,290)) RequestNewGame();
            GUI.enabled = TryReadSave(out _); if (Button("Continuar",100,348)) Continue(); GUI.enabled = true;
            if (Button("Configurações",100,406)) { settingsReturnMode = Mode.Menu; mode = Mode.Settings; }
            GUI.enabled = hasSave; if (Button("Apagar progresso",100,464)) RequestDeleteSave(); GUI.enabled = true;
            Label("WASD / setas: mover • E: interagir\nEsc: pausa • mouse ou toque: menus",100,520,550,65);
            Label(notice,100,585,550,35);
        }
        else if (mode == Mode.ConfirmNewGame || mode == Mode.ConfirmDeleteSave)
        {
            Art(backgrounds[0],new Rect(0,0,1100,700)); Panel(180,180,740,340);
            bool deleting = mode == Mode.ConfirmDeleteSave;
            Label(deleting ? "Apagar progresso?" : "Começar um novo jogo?",220,215,660,60,true);
            Label(deleting ? "O save atual será removido permanentemente." : "O progresso salvo será substituído. Deseja continuar?",220,295,660,90);
            if (Button(deleting ? "Apagar save" : "Substituir e começar",250,405,290)) { if (deleting) DeleteSave(); else StartNewGame(); }
            if (Button("Voltar",560,405,270)) mode = Mode.Menu;
        }
        else if (mode == Mode.Settings)
        {
            Art(backgrounds[0],new Rect(0,0,1100,700)); Panel(180,160,740,390);
            Label("Configurações",220,190,660,60,true);
            Label("Volume dos efeitos",240,295,300,40);
            float volume = GUI.HorizontalSlider(new Rect(520,305,300,30),speaker.volume,0,1);
            if (!Mathf.Approximately(volume,speaker.volume)) SetVolume(volume);
            Label(Mathf.RoundToInt(speaker.volume * 100) + "%",830,290,70,40);
            if (Button("Voltar",420,420,260)) mode = settingsReturnMode;
        }
        else
        {
            Panel(0,0,1100,104);
            Label(phases[data.phase] + "  |  Nível " + Level + "  |  XP " + data.xp,20,10,850,38,true);
            Label("Vida " + data.hp + "/" + MaxHP + "  •  Energia " + data.energy + "/8    |    " + Objective(),20,55,1050,45);
            if (mode == Mode.Explore || mode == Mode.Dialogue || mode == Mode.Pause)
            {
                foreach (var p in points)
                {
                    if (!p.view.activeSelf) continue;
                    Vector3 screen = cam.WorldToViewportPoint(p.position);
                    GUI.Label(new Rect(screen.x*1100-100,(1-screen.y)*700-42,200,42),p.title,smallStyle);
                }
                Panel(0,642,1100,58);
                var near = Nearest(); Label(near == null ? "WASD / setas: mover     E: interagir     Esc: pausa" : "E — " + near.title,20,650,820,44);
                if (mode == Mode.Explore && Button("Pausa",930,648,150,42)) mode = Mode.Pause;
                if (mode == Mode.Explore && Application.isMobilePlatform)
                {
                    Panel(20,500,170,160); Label("TOQUE E\nARRASTE\npara mover",35,525,140,90);
                    if (Button("Interagir",900,552,170,70)) Interact();
                }
            }
            if (mode == Mode.Dialogue)
            {
                Panel(150,205,800,320); Label(dialogue,180,240,740,180);
                if (Button("Continuar",600,450)) mode = Mode.Explore;
            }
            if (mode == Mode.Battle || mode == Mode.Victory || mode == Mode.Defeat)
            {
                Art(backgrounds[data.phase],new Rect(0,106,1100,536));
                Art(portrait,new Rect(90,150,280,290)); Art(enemy == 0 ? ki : enemy == 5 ? mark : guards,new Rect(720,150,280,290));
                Panel(365,135,370,95); Label(names[enemy]+"\nVida: "+enemyHP+"/"+enemyHealth[enemy],380,142,340,85);
                if (Time.unscaledTime < effectUntil) { var c=GUI.color; GUI.color=effectColor; GUI.DrawTexture(new Rect(720,150,280,290),Texture2D.whiteTexture); GUI.color=c; }
                Panel(30,440,1040,230); Label(log,50,450,1000,85);
                if (mode == Mode.Battle)
                {
                    GUI.enabled = !busy;
                    if (Button("Ataque • grátis",50,555,235)) StartCoroutine(Act(0));
                    GUI.enabled = !busy && data.energy >= 3;
                    if (Button("Fogo • 3 energia",300,555,235)) StartCoroutine(Act(1));
                    GUI.enabled = !busy && data.energy >= 2;
                    if (Button("Água • 2 energia",550,555,235)) StartCoroutine(Act(2));
                    GUI.enabled = !busy;
                    if (Button("Defesa • +3 energia",800,555,235)) StartCoroutine(Act(3));
                    GUI.enabled = true;
                    GUI.Label(new Rect(50,615,1000,45),busy ? "Resolvendo o turno…" : enemy == 5 ? "Água reduz dano em 40%; defesa em 60%. Mark anuncia golpes fortes." : "Água reduz o próximo dano em 40%. Defesa reduz em 60%. Vitória recupera vida e energia.",smallStyle);
                }
                if (mode == Mode.Victory && Button("Vitória! +20 XP — voltar",370,570,360)) mode = Mode.Explore;
                if (mode == Mode.Defeat && Button("Derrota — tentar novamente",370,570,360)) RetryBattle();
            }
            if (mode == Mode.Pause)
            {
                Panel(300,175,500,400); Label("Pausa",330,200,430,50,true);
                if (Button("Retomar",330,270,440)) mode=Mode.Explore;
                if (Button("Salvar progresso",330,328,210)) { Save(); notice="Progresso salvo."; }
                if (Button("Salvar e voltar ao menu",555,328,215)) { Save(); MainMenu(); }
                if (Button("Configurações",330,386,440)) { settingsReturnMode=Mode.Pause; mode=Mode.Settings; }
                Label(notice,330,442,440,35);
            }
            if (mode == Mode.Ending)
            {
                if (finalCreditsPlayer != null && finalCreditsPlayer.isPlaying && finalCreditsTexture != null)
                {
                    GUI.DrawTexture(new Rect(0,0,UiWidth,UiHeight),finalCreditsTexture,ScaleMode.ScaleToFit);
                    Panel(820,610,250,60);
                    if (Button("Pular créditos",830,618,230,44)) MainMenu();
                }
                else
                {
                    Panel(90,145,920,475); Label("A queda da ASDA",125,175,840,60,true);
                    Label("Brian se sacrifica e abre caminho para Magnus e seus amigos derrubarem a ASDA. A profecia se referia a Brian.\n\nOs sobreviventes seguem juntos, agora procurados. A história termina; um novo caminho começa.\n\nFim da campanha — obrigado por jogar!",125,250,840,220);
                    GUI.Label(new Rect(125,478,840,50),"Equipe: Larissa Campos Cardoso, Izak Pinheiro e Matheus Henrique. Créditos finais.",smallStyle);
                    if (Button("Menu inicial",420,550)) MainMenu();
                }
            }
        }
        if (fadeAlpha > 0f)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(0,0,0,fadeAlpha);
            GUI.DrawTexture(new Rect(0,0,UiWidth,UiHeight),Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
        GUI.matrix=matrix;
    }
    void OnDestroy()
    {
        if (finalCreditsTexture != null) { finalCreditsTexture.Release(); Destroy(finalCreditsTexture); }
        if (cue != null) Destroy(cue);
        if (unlit != null) Destroy(unlit);
        if (square != null) { Destroy(square.texture); Destroy(square); }
    }
}
