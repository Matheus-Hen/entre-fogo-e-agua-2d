using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace EntreFogoEAgua.Ferramentas
{
    /// <summary>
    /// Ferramenta de Editor que (re)gera o conteúdo do jogo 3D a partir dos assets do projeto:
    /// configurações de importação, materiais, clips e Animator Controllers, prefabs,
    /// as cenas Menu e Fase01_BaseASDA e a lista de cenas do Build.
    /// Menu: Entre Fogo e Água > Gerar jogo 3D.
    /// Depois de gerado, tudo é asset comum da Unity e pode ser editado à mão.
    /// </summary>
    public static class ConstrutorJogo3D
    {
        private const string PastaMateriais = "Assets/Materials/Jogo3D";
        private const string PastaAnimacoes = "Assets/Animations";
        private const string PastaPrefabs = "Assets/Prefabs";
        private const string PastaCenas = "Assets/Scenes";
        private const string ModeloMagnus = "Assets/Models/Magnus.fbx";
        private const string ModeloSoldado = "Assets/Models/SoldadoASDA.fbx";
        private const string PastaProps = "Assets/Models/Props/";
        private const string PastaAudio = "Assets/Audio/";

        // Os FBX do Blender (frente em -Y) já chegam olhando para +Z na Unity; o +X do Blender vira -X.
        private static readonly Vector3 RotacaoModelo = Vector3.zero;

        private static readonly Dictionary<string, Material> Mats = new Dictionary<string, Material>();
        private static Font fonte;

        [MenuItem("Entre Fogo e Água/Gerar jogo 3D (cenas, prefabs e animações)")]
        public static void GerarTudo()
        {
            SalvarCenasAbertas();
            CriarPastas();
            ConfigurarImportacao();
            CriarMateriais();
            var controladores = CriarAnimacoes();
            var prefabs = CriarPrefabs(controladores);
            CriarCenaFase(prefabs, controladores);
            CriarCenaMenu(controladores);
            ConfigurarBuild();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(PastaCenas + "/Menu.unity");
            Debug.Log("[Jogo] Jogo 3D gerado com sucesso.");
        }

        // ------------------------------------------------------------------ utilidades gerais
        private static void SalvarCenasAbertas()
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var cena = EditorSceneManager.GetSceneAt(i);
                if (cena.isDirty && !string.IsNullOrEmpty(cena.path))
                    EditorSceneManager.SaveScene(cena);
            }
        }

        private static void CriarPastas()
        {
            foreach (var p in new[] { PastaMateriais, PastaAnimacoes, PastaPrefabs, PastaCenas })
                Directory.CreateDirectory(p);
            AssetDatabase.Refresh();
        }

        private static void Definir(Object alvo, string propriedade, object valor)
        {
            var so = new SerializedObject(alvo);
            var p = so.FindProperty(propriedade);
            if (p == null)
            {
                Debug.LogError("[Jogo] Propriedade não encontrada: " + alvo.GetType().Name + "." + propriedade);
                return;
            }
            switch (valor)
            {
                case Object o: p.objectReferenceValue = o; break;
                case string s: p.stringValue = s; break;
                case int i when p.propertyType == SerializedPropertyType.Enum: p.enumValueIndex = i; break;
                case int i: p.intValue = i; break;
                case float f: p.floatValue = f; break;
                case bool b: p.boolValue = b; break;
                case Vector3 v: p.vector3Value = v; break;
                case System.Enum e: p.enumValueIndex = System.Convert.ToInt32(e); break;
                case null: p.objectReferenceValue = null; break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Vazio(string nome, Transform pai, Vector3 posicaoLocal)
        {
            var go = new GameObject(nome);
            go.transform.SetParent(pai, false);
            go.transform.localPosition = posicaoLocal;
            return go;
        }

        private static GameObject Caixa(string nome, Transform pai, Vector3 centro, Vector3 tamanho, string material, bool colisor = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nome;
            go.transform.SetParent(pai, false);
            go.transform.localPosition = centro;
            go.transform.localScale = tamanho;
            go.GetComponent<Renderer>().sharedMaterial = Mats[material];
            if (!colisor)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject Primitiva(PrimitiveType tipo, string nome, Transform pai, Vector3 pos, Vector3 escala, string material, bool colisor = false)
        {
            var go = GameObject.CreatePrimitive(tipo);
            go.name = nome;
            go.transform.SetParent(pai, false);
            go.transform.localPosition = pos;
            go.transform.localScale = escala;
            go.GetComponent<Renderer>().sharedMaterial = Mats[material];
            if (!colisor)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>
        /// Instancia um FBX. Com envolver=true cria um pivô vazio "Modelo" e coloca a malha como filha ("Malha"),
        /// para que os clips possam girar/mover o pivô sem apagar a rotação de conversão de eixos do FBX.
        /// </summary>
        private static GameObject Modelo(string caminho, Transform pai, Vector3 pos, float escala = 1f, bool envolver = true)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            Transform alvoPai = pai;
            Vector3 posMalha = pos;
            GameObject pivo = null;
            if (envolver)
            {
                pivo = Vazio("Modelo", pai, pos);
                pivo.transform.localEulerAngles = RotacaoModelo;
                alvoPai = pivo.transform;
                posMalha = Vector3.zero;
                go.name = "Malha";
            }
            else
            {
                go.name = "Modelo";
            }
            go.transform.SetParent(alvoPai, false);
            // Preserva o deslocamento e a rotação de conversão de eixos que o FBX traz na raiz
            // (malhas sem rig vêm com -90° em X).
            go.transform.localPosition = posMalha + asset.transform.localPosition * escala;
            go.transform.localRotation = (envolver ? Quaternion.identity : Quaternion.Euler(RotacaoModelo)) * asset.transform.localRotation;
            go.transform.localScale = asset.transform.localScale * escala;
            return envolver ? pivo : go;
        }

        private static AudioClip Som(string nome) => AssetDatabase.LoadAssetAtPath<AudioClip>(PastaAudio + nome + ".wav");

        // ------------------------------------------------------------------ importação
        private static void ConfigurarImportacao()
        {
            var mi = (ModelImporter)AssetImporter.GetAtPath(ModeloMagnus);
            if (mi.animationType != ModelImporterAnimationType.Generic)
            {
                mi.animationType = ModelImporterAnimationType.Generic;
                mi.SaveAndReimport();
            }
            var clips = mi.defaultClipAnimations;
            foreach (var c in clips)
            {
                if (c.takeName.Contains("Idle")) { c.name = "Magnus_Idle"; c.loopTime = true; }
                else if (c.takeName.Contains("Walk")) { c.name = "Magnus_Walk"; c.loopTime = true; }
                else if (c.takeName.Contains("Cast")) { c.name = "Magnus_Cast"; c.loopTime = false; }
            }
            mi.clipAnimations = clips;
            mi.SaveAndReimport();

            foreach (var caminho in new[] { ModeloSoldado }.Concat(Directory.GetFiles(PastaProps, "*.fbx")))
            {
                var imp = (ModelImporter)AssetImporter.GetAtPath(caminho.Replace('\\', '/'));
                if (imp == null) continue;
                imp.animationType = ModelImporterAnimationType.None;
                imp.importAnimation = false;
                imp.SaveAndReimport();
            }

            var musica = (AudioImporter)AssetImporter.GetAtPath(PastaAudio + "Musica_Base.wav");
            var cfg = musica.defaultSampleSettings;
            cfg.loadType = AudioClipLoadType.Streaming;
            musica.defaultSampleSettings = cfg;
            musica.SaveAndReimport();

            LogarAltura(ModeloMagnus);
            LogarAltura(ModeloSoldado);
        }

        private static void LogarAltura(string caminho)
        {
            var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(caminho));
            var b = new Bounds(go.transform.position, Vector3.zero);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
            Debug.Log("[Jogo] Altura de " + caminho + " = " + b.size.y.ToString("0.00") + " m, centro z=" + b.center.z.ToString("0.00"));
            Object.DestroyImmediate(go);
        }

        // ------------------------------------------------------------------ materiais
        private static Material Mat(string nome, Color cor, float brilho = 0.15f, Color? emissao = null, bool transparente = false, float metal = 0f)
        {
            string caminho = PastaMateriais + "/" + nome + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (m == null)
            {
                m = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(m, caminho);
            }
            m.shader = Shader.Find("Standard");
            m.color = cor;
            m.SetFloat("_Glossiness", brilho);
            m.SetFloat("_Metallic", metal);
            if (emissao.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emissao.Value);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                m.DisableKeyword("_EMISSION");
            }
            if (transparente)
            {
                m.SetFloat("_Mode", 3f);
                m.SetInt("_SrcBlend", (int)BlendMode.One);
                m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.DisableKeyword("_ALPHABLEND_ON");
                m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                m.renderQueue = (int)RenderQueue.Transparent;
            }
            EditorUtility.SetDirty(m);
            Mats[nome] = m;
            return m;
        }

        private static void CriarMateriais()
        {
            Mat("Grama", new Color(0.33f, 0.52f, 0.25f), 0.05f);
            Mat("Terra", new Color(0.50f, 0.38f, 0.24f), 0.05f);
            Mat("PisoBase", new Color(0.47f, 0.49f, 0.52f), 0.2f);
            Mat("MuroBase", new Color(0.22f, 0.24f, 0.30f), 0.25f);
            Mat("MuroPedra", new Color(0.45f, 0.42f, 0.38f), 0.1f);
            Mat("LeitoRio", new Color(0.12f, 0.16f, 0.20f), 0.1f);
            Mat("AguaRio", new Color(0.15f, 0.45f, 0.75f, 0.65f), 0.9f, null, true);
            Mat("MadeiraPonte", new Color(0.50f, 0.33f, 0.17f), 0.1f);
            Mat("PlacaInativa", new Color(0.55f, 0.45f, 0.15f), 0.5f, new Color(0.25f, 0.18f, 0.02f), false, 0.6f);
            Mat("PlacaAtiva", new Color(1f, 0.85f, 0.25f), 0.6f, new Color(1.2f, 0.9f, 0.2f), false, 0.6f);
            Mat("Gelo", new Color(0.65f, 0.90f, 1f, 0.75f), 0.95f, new Color(0.05f, 0.15f, 0.25f), true);
            Mat("AuraFogo", new Color(1f, 0.35f, 0.05f), 0.3f, new Color(2.5f, 0.7f, 0.1f));
            Mat("AuraAgua", new Color(0.1f, 0.55f, 1f), 0.3f, new Color(0.2f, 1.0f, 2.5f));
            Mat("ProjetilFogo", new Color(1f, 0.5f, 0.1f), 0.5f, new Color(4f, 1.5f, 0.2f));
            Mat("ProjetilAgua", new Color(0.3f, 0.7f, 1f), 0.9f, new Color(0.4f, 1.6f, 4f));
            Mat("BarraFundo", new Color(0.1f, 0.1f, 0.1f), 0f, new Color(0.05f, 0.05f, 0.05f));
            Mat("BarraVida", new Color(0.9f, 0.15f, 0.15f), 0f, new Color(1.2f, 0.1f, 0.1f));
            Mat("PedestalMenu", new Color(0.20f, 0.22f, 0.28f), 0.4f);

            // Material de partículas (shader embutido de partículas, aditivo e com textura padrão).
            var particula = AssetDatabase.LoadAssetAtPath<Material>(PastaMateriais + "/ParticulaFogo.mat");
            if (particula == null)
            {
                particula = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                AssetDatabase.CreateAsset(particula, PastaMateriais + "/ParticulaFogo.mat");
            }
            particula.shader = Shader.Find("Legacy Shaders/Particles/Additive");
            particula.mainTexture = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
            particula.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
            Mats["ParticulaFogo"] = particula;

            var rastro = AssetDatabase.LoadAssetAtPath<Material>(PastaMateriais + "/Rastro.mat");
            if (rastro == null)
            {
                rastro = new Material(Shader.Find("Sprites/Default"));
                AssetDatabase.CreateAsset(rastro, PastaMateriais + "/Rastro.mat");
            }
            Mats["Rastro"] = rastro;
            AssetDatabase.SaveAssets();
        }

        // ------------------------------------------------------------------ animações
        private static AnimationClip SalvarClip(AnimationClip clip, bool loop)
        {
            var cfg = AnimationUtility.GetAnimationClipSettings(clip);
            cfg.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, cfg);
            string caminho = PastaAnimacoes + "/" + clip.name + ".anim";
            AssetDatabase.DeleteAsset(caminho);
            AssetDatabase.CreateAsset(clip, caminho);
            return clip;
        }

        private static AnimationCurve Curva(params float[] tempoValor)
        {
            var keys = new List<Keyframe>();
            for (int i = 0; i < tempoValor.Length; i += 2)
                keys.Add(new Keyframe(tempoValor[i], tempoValor[i + 1]));
            var curva = new AnimationCurve(keys.ToArray());
            for (int i = 0; i < curva.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curva, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curva, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            return curva;
        }

        private static AnimatorController NovoController(string nome)
        {
            string caminho = PastaAnimacoes + "/" + nome + ".controller";
            AssetDatabase.DeleteAsset(caminho);
            return AnimatorController.CreateAnimatorControllerAtPath(caminho);
        }

        private static Dictionary<string, AnimatorController> CriarAnimacoes()
        {
            var resultado = new Dictionary<string, AnimatorController>();

            // --- Magnus: clips do FBX feitos no Blender (Idle, Walk, Cast)
            var clipsMagnus = AssetDatabase.LoadAllAssetsAtPath(ModeloMagnus).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToDictionary(c => c.name);
            var cm = NovoController("Magnus");
            cm.AddParameter("Velocidade", AnimatorControllerParameterType.Float);
            cm.AddParameter("Lancar", AnimatorControllerParameterType.Trigger);
            var sm = cm.layers[0].stateMachine;
            var idle = sm.AddState("Parado");
            idle.motion = clipsMagnus["Magnus_Idle"];
            var andar = sm.AddState("Andando");
            andar.motion = clipsMagnus["Magnus_Walk"];
            andar.speed = 1.7f;
            andar.speedParameterActive = true;
            andar.speedParameter = "Velocidade";
            var lancar = sm.AddState("Lancando");
            lancar.motion = clipsMagnus["Magnus_Cast"];
            sm.defaultState = idle;
            var t = idle.AddTransition(andar);
            t.hasExitTime = false; t.duration = 0.1f;
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Velocidade");
            t = andar.AddTransition(idle);
            t.hasExitTime = false; t.duration = 0.15f;
            t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Velocidade");
            t = sm.AddAnyStateTransition(lancar);
            t.hasExitTime = false; t.duration = 0.05f; t.canTransitionToSelf = false;
            t.AddCondition(AnimatorConditionMode.If, 0f, "Lancar");
            t = lancar.AddTransition(idle);
            t.hasExitTime = true; t.exitTime = 0.9f; t.duration = 0.1f;
            resultado["Magnus"] = cm;

            // --- Guarda: patrulha (balanço) e atingido (recuo), animando o filho "Modelo"
            var patrulha = new AnimationClip { name = "Guarda_Patrulha" };
            patrulha.SetCurve("Modelo", typeof(Transform), "m_LocalPosition.y", Curva(0f, 0f, 0.35f, 0.07f, 0.7f, 0f));
            patrulha.SetCurve("Modelo", typeof(Transform), "localEulerAnglesRaw.z", Curva(0f, -2f, 0.35f, 2f, 0.7f, -2f));
            SalvarClip(patrulha, true);
            var atingido = new AnimationClip { name = "Guarda_Atingido" };
            atingido.SetCurve("Modelo", typeof(Transform), "localEulerAnglesRaw.x", Curva(0f, 0f, 0.08f, -18f, 0.35f, 0f));
            atingido.SetCurve("Modelo", typeof(Transform), "m_LocalPosition.y", Curva(0f, 0f, 0.35f, 0f));
            atingido.SetCurve("Modelo", typeof(Transform), "localEulerAnglesRaw.z", Curva(0f, 0f, 0.35f, 0f));
            SalvarClip(atingido, false);
            var cg = NovoController("Guarda");
            cg.AddParameter("Atingido", AnimatorControllerParameterType.Trigger);
            sm = cg.layers[0].stateMachine;
            var sp = sm.AddState("Patrulha"); sp.motion = patrulha;
            var sa = sm.AddState("Atingido"); sa.motion = atingido;
            sm.defaultState = sp;
            t = sm.AddAnyStateTransition(sa); t.hasExitTime = false; t.duration = 0.02f;
            t.AddCondition(AnimatorConditionMode.If, 0f, "Atingido");
            t = sa.AddTransition(sp); t.hasExitTime = true; t.exitTime = 1f; t.duration = 0.05f;
            resultado["Guarda"] = cg;

            // --- Pista: flutua e gira
            var flutuar = new AnimationClip { name = "Pista_Flutuar" };
            flutuar.SetCurve("Modelo", typeof(Transform), "m_LocalPosition.y", Curva(0f, 1.0f, 1f, 1.25f, 2f, 1.0f));
            var giro = AnimationCurve.Linear(0f, 0f, 2f, 360f);
            flutuar.SetCurve("Modelo", typeof(Transform), "localEulerAnglesRaw.y", giro);
            SalvarClip(flutuar, true);
            var cp = NovoController("Pista");
            cp.layers[0].stateMachine.AddState("Flutuar").motion = flutuar;
            resultado["Pista"] = cp;

            // --- Portão: Fechado -> Abrindo (parâmetro bool Aberto)
            var fechado = new AnimationClip { name = "Portao_Fechado" };
            // Cada folha se estende para -X local a partir da dobradiça; 90° aponta a folha para +Z (abre para fora).
            fechado.SetCurve("FolhaEsquerda", typeof(Transform), "localEulerAnglesRaw.y", Curva(0f, 180f, 0.1f, 180f));
            fechado.SetCurve("FolhaDireita", typeof(Transform), "localEulerAnglesRaw.y", Curva(0f, 0f, 0.1f, 0f));
            SalvarClip(fechado, false);
            var abrindo = new AnimationClip { name = "Portao_Abrindo" };
            abrindo.SetCurve("FolhaEsquerda", typeof(Transform), "localEulerAnglesRaw.y", Curva(0f, 180f, 1.6f, 85f));
            abrindo.SetCurve("FolhaDireita", typeof(Transform), "localEulerAnglesRaw.y", Curva(0f, 0f, 1.6f, 95f));
            SalvarClip(abrindo, false);
            var cpo = NovoController("Portao");
            cpo.AddParameter("Aberto", AnimatorControllerParameterType.Bool);
            sm = cpo.layers[0].stateMachine;
            var sf = sm.AddState("Fechado"); sf.motion = fechado;
            var sab = sm.AddState("Abrindo"); sab.motion = abrindo;
            sm.defaultState = sf;
            t = sf.AddTransition(sab); t.hasExitTime = false; t.duration = 0f;
            t.AddCondition(AnimatorConditionMode.If, 0f, "Aberto");
            resultado["Portao"] = cpo;

            AssetDatabase.SaveAssets();
            return resultado;
        }

        // ------------------------------------------------------------------ prefabs
        private class Prefabs
        {
            public GameObject Magnus, ProjetilFogo, ProjetilAgua, ProjetilInimigo, Caixa, Pista, Guarda, BarreiraFogo, BarreiraGelo;
        }

        private static GameObject SalvarPrefab(GameObject raiz, string nome)
        {
            string caminho = PastaPrefabs + "/" + nome + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
            Object.DestroyImmediate(raiz);
            return prefab;
        }

        private static Prefabs CriarPrefabs(Dictionary<string, AnimatorController> ctrl)
        {
            var p = new Prefabs();

            // Projéteis de Magnus
            p.ProjetilFogo = CriarProjetil("ProjetilFogo", Elemento.Fogo, "ProjetilFogo", new Color(1f, 0.5f, 0.1f));
            p.ProjetilAgua = CriarProjetil("ProjetilAgua", Elemento.Agua, "ProjetilAgua", new Color(0.3f, 0.7f, 1f));

            // Projétil dos guardas (trigger)
            var pi = Primitiva(PrimitiveType.Sphere, "ProjetilInimigo", null, Vector3.zero, Vector3.one * 0.35f, "AuraFogo", true);
            pi.GetComponent<SphereCollider>().isTrigger = true;
            var rbi = pi.AddComponent<Rigidbody>();
            rbi.useGravity = false;
            rbi.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var scrPi = pi.AddComponent<ProjetilInimigo>();
            Definir(scrPi, "visual", pi.GetComponent<Renderer>());
            Definir(scrPi, "materialFogo", Mats["AuraFogo"]);
            Definir(scrPi, "materialAgua", Mats["AuraAgua"]);
            p.ProjetilInimigo = SalvarPrefab(pi, "ProjetilInimigo");

            // Caixa empurrável
            var cx = new GameObject("Caixa");
            Modelo(PastaProps + "Caixa.fbx", cx.transform, Vector3.zero, 1.2f);
            var bc = cx.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 0.6f, 0f);
            bc.size = Vector3.one * 1.2f;
            var rbc = cx.AddComponent<Rigidbody>();
            rbc.mass = 15f;
            rbc.linearDamping = 2f;
            rbc.angularDamping = 2f;
            rbc.constraints = RigidbodyConstraints.FreezeRotation;
            rbc.interpolation = RigidbodyInterpolation.Interpolate;
            cx.AddComponent<CaixaEmpurravel>();
            p.Caixa = SalvarPrefab(cx, "Caixa");

            // Pista
            var pista = new GameObject("Pista");
            Modelo(PastaProps + "Pista.fbx", pista.transform, Vector3.up, 1.4f);
            var sc = pista.AddComponent<SphereCollider>();
            sc.isTrigger = true; sc.radius = 1.0f; sc.center = Vector3.up;
            var anP = pista.AddComponent<Animator>();
            anP.runtimeAnimatorController = ctrl["Pista"];
            var luzP = Vazio("Brilho", pista.transform, Vector3.up * 1.2f).AddComponent<Light>();
            luzP.type = LightType.Point; luzP.color = new Color(1f, 0.85f, 0.4f); luzP.range = 4f; luzP.intensity = 2f;
            var scrP = pista.AddComponent<Pista>();
            Definir(scrP, "somColeta", Som("SFX_Coleta"));
            p.Pista = SalvarPrefab(pista, "Pista");

            // Guarda da ASDA
            var g = new GameObject("Guarda");
            Modelo(ModeloSoldado, g.transform, Vector3.zero, 0.92f);
            var aura = Primitiva(PrimitiveType.Cylinder, "Aura", g.transform, new Vector3(0f, 0.03f, 0f), new Vector3(1.8f, 0.02f, 1.8f), "AuraFogo");
            var cap = g.AddComponent<CapsuleCollider>();
            cap.center = new Vector3(0f, 0.95f, 0f); cap.height = 1.9f; cap.radius = 0.45f;
            var rbg = g.AddComponent<Rigidbody>();
            rbg.isKinematic = true;
            rbg.interpolation = RigidbodyInterpolation.Interpolate;
            var anG = g.AddComponent<Animator>();
            anG.runtimeAnimatorController = ctrl["Guarda"];
            var tiro = Vazio("PontoTiro", g.transform, new Vector3(0f, 1.4f, 0.7f));
            var barraPivo = Vazio("BarraVidaPivo", g.transform, new Vector3(0f, 2.25f, 0f));
            Primitiva(PrimitiveType.Cube, "Fundo", barraPivo.transform, Vector3.zero, new Vector3(1.04f, 0.14f, 0.02f), "BarraFundo");
            var preench = Vazio("Preenchimento", barraPivo.transform, new Vector3(-0.5f, 0f, -0.02f));
            Primitiva(PrimitiveType.Cube, "Barra", preench.transform, new Vector3(0.5f, 0f, 0f), new Vector3(1f, 0.1f, 0.02f), "BarraVida");
            var scrG = g.AddComponent<Guarda>();
            Definir(scrG, "projetil", p.ProjetilInimigo.GetComponent<ProjetilInimigo>());
            Definir(scrG, "pontoTiro", tiro.transform);
            Definir(scrG, "animator", anG);
            Definir(scrG, "aura", aura.GetComponent<Renderer>());
            Definir(scrG, "materialAuraFogo", Mats["AuraFogo"]);
            Definir(scrG, "materialAuraAgua", Mats["AuraAgua"]);
            Definir(scrG, "barraVida", preench.transform);
            Definir(scrG, "somDerrota", Som("SFX_InimigoDerrotado"));
            Definir(scrG, "somAbsorver", Som("SFX_Clique"));
            p.Guarda = SalvarPrefab(g, "Guarda");

            // Barreira de fogo (4 m de largura)
            var bf = new GameObject("BarreiraFogo");
            var braseiro = Modelo(PastaProps + "Braseiro.fbx", bf.transform, Vector3.zero);
            braseiro.transform.localScale = new Vector3(1.55f, 1f, 1f);
            var bloqueio = bf.AddComponent<BoxCollider>();
            bloqueio.center = new Vector3(0f, 1.5f, 0f); bloqueio.size = new Vector3(4.2f, 3f, 1f);
            var chamas = CriarChamas(bf.transform, 3.8f);
            var luzF = Vazio("Luz", bf.transform, new Vector3(0f, 1.5f, 0f)).AddComponent<Light>();
            luzF.type = LightType.Point; luzF.color = new Color(1f, 0.55f, 0.15f); luzF.range = 8f; luzF.intensity = 3f;
            var scrBf = bf.AddComponent<BarreiraFogo>();
            Definir(scrBf, "chamas", chamas);
            Definir(scrBf, "bloqueio", bloqueio);
            Definir(scrBf, "luz", luzF);
            Definir(scrBf, "somApagar", Som("SFX_Apagar"));
            p.BarreiraFogo = SalvarPrefab(bf, "BarreiraFogo");

            // Barreira de gelo (6 m)
            var bg = new GameObject("BarreiraGelo");
            var bloco = Vazio("Bloco", bg.transform, Vector3.zero);
            bloco.transform.localScale = new Vector3(6f, 3.2f, 0.9f);
            Primitiva(PrimitiveType.Cube, "Gelo", bloco.transform, new Vector3(0f, 0.5f, 0f), Vector3.one, "Gelo", true);
            var scrBg = bg.AddComponent<BarreiraGelo>();
            Definir(scrBg, "bloco", bloco.transform);
            Definir(scrBg, "somDerreter", Som("SFX_Apagar"));
            p.BarreiraGelo = SalvarPrefab(bg, "BarreiraGelo");

            // Magnus (jogador)
            var m = new GameObject("Magnus");
            m.tag = "Player";
            var modelo = Modelo(ModeloMagnus, m.transform, Vector3.zero, 1f, false);
            var anM = modelo.GetComponent<Animator>();
            if (anM == null) anM = modelo.AddComponent<Animator>();
            anM.runtimeAnimatorController = ctrl["Magnus"];
            anM.applyRootMotion = false;
            var cc = m.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 0.86f, 0f); cc.height = 1.7f; cc.radius = 0.32f; cc.stepOffset = 0.35f;
            var ponto = Vazio("PontoLancamento", m.transform, new Vector3(-0.3f, 1.25f, 0.4f));
            m.AddComponent<MagnusStatus>();
            var mov = m.AddComponent<MagnusMovimento>();
            var pod = m.AddComponent<MagnusPoderes>();
            Definir(m.GetComponent<MagnusStatus>(), "somDano", Som("SFX_Dano"));
            Definir(mov, "animator", anM);
            Definir(mov, "somPulo", Som("SFX_Pulo"));
            Definir(pod, "animator", anM);
            Definir(pod, "pontoLancamento", ponto.transform);
            Definir(pod, "projetilFogo", p.ProjetilFogo.GetComponent<ProjetilElemental>());
            Definir(pod, "projetilAgua", p.ProjetilAgua.GetComponent<ProjetilElemental>());
            Definir(pod, "somFogo", Som("SFX_Fogo"));
            Definir(pod, "somAgua", Som("SFX_Agua"));
            p.Magnus = SalvarPrefab(m, "Magnus");

            return p;
        }

        private static GameObject CriarProjetil(string nome, Elemento elemento, string material, Color cor)
        {
            var go = Primitiva(PrimitiveType.Sphere, nome, null, Vector3.zero, Vector3.one * 0.45f, material, true);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var trilha = go.AddComponent<TrailRenderer>();
            trilha.sharedMaterial = Mats["Rastro"];
            trilha.time = 0.25f;
            trilha.widthMultiplier = 0.35f;
            trilha.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(cor, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            trilha.colorGradient = grad;
            var luz = Vazio("Luz", go.transform, Vector3.zero).AddComponent<Light>();
            luz.type = LightType.Point; luz.color = cor; luz.range = 4f; luz.intensity = 2f;
            var scr = go.AddComponent<ProjetilElemental>();
            Definir(scr, "elemento", elemento);
            return SalvarPrefab(go, nome);
        }

        private static ParticleSystem CriarChamas(Transform pai, float largura)
        {
            var go = Vazio("Chamas", pai, new Vector3(0f, 0.45f, 0f));
            go.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.3f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.65f, 0.15f), new Color(1f, 0.25f, 0.05f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 400;
            main.gravityModifier = -0.15f;
            var em = ps.emission;
            em.rateOverTime = 70f;
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = new Vector3(largura, 0.6f, 0.1f);
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f), new GradientColorKey(new Color(1f, 0.3f, 0.05f), 0.6f), new GradientColorKey(new Color(0.3f, 0.1f, 0.05f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = Mats["ParticulaFogo"];
            return ps;
        }

        // ------------------------------------------------------------------ cena da fase
        private static GameObject Instanciar(GameObject prefab, Transform pai, Vector3 pos, float rotY = 0f)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(pai, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            return go;
        }

        private static void Iluminacao(Color ambienteCeu, Color ambienteChao, bool neblina)
        {
            var sol = new GameObject("Sol | luz direcional").AddComponent<Light>();
            sol.type = LightType.Directional;
            sol.color = new Color(1f, 0.93f, 0.82f);
            sol.intensity = 1.15f;
            sol.shadows = LightShadows.Soft;
            sol.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.sun = sol;
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambienteCeu;
            RenderSettings.ambientEquatorColor = Color.Lerp(ambienteCeu, ambienteChao, 0.5f);
            RenderSettings.ambientGroundColor = ambienteChao;
            RenderSettings.fog = neblina;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.62f, 0.70f, 0.78f);
            RenderSettings.fogDensity = 0.012f;
        }

        private static void CriarCenaFase(Prefabs p, Dictionary<string, AnimatorController> ctrl)
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Iluminacao(new Color(0.55f, 0.62f, 0.75f), new Color(0.25f, 0.22f, 0.18f), true);

            // Sistemas
            var sistemas = new GameObject("Sistemas");
            var partida = sistemas.AddComponent<GerenciadorPartida>();
            Definir(partida, "somVitoria", Som("SFX_Vitoria"));
            Definir(partida, "somDerrota", Som("SFX_Derrota"));
            var audio = sistemas.AddComponent<GerenciadorAudio>();
            Definir(audio, "musica", Som("Musica_Base"));

            // ---------------- Cenário
            var cenario = new GameObject("Cenario").transform;
            // Área A: acampamento (z -10..30) | rio (z 30..38) | Área B: base ASDA (z 38..95)
            Caixa("Chao | acampamento", cenario, new Vector3(0f, -2.5f, 10f), new Vector3(35f, 5f, 40f), "Grama");
            Caixa("Trilha | acampamento", cenario, new Vector3(0f, 0.01f, 10f), new Vector3(4f, 0.02f, 40f), "Terra", false);
            Caixa("Chao | base ASDA", cenario, new Vector3(0f, -2.5f, 66.5f), new Vector3(35f, 5f, 57f), "PisoBase");
            Caixa("Leito do rio", cenario, new Vector3(0f, -5.5f, 34f), new Vector3(35f, 1f, 8f), "LeitoRio");
            var agua = Caixa("Agua do rio", cenario, new Vector3(0f, -1.8f, 34f), new Vector3(35f, 0.1f, 8f), "AguaRio", false);
            var zonaQueda = Vazio("ZonaQueda | rio", cenario, new Vector3(0f, -3f, 34f));
            var zq = zonaQueda.AddComponent<BoxCollider>(); zq.isTrigger = true; zq.size = new Vector3(35f, 4f, 8f);
            zonaQueda.AddComponent<ZonaQueda>();

            // muros do perímetro
            Caixa("Muro | oeste", cenario, new Vector3(-17.8f, 2f, 42.5f), new Vector3(1f, 4f, 106f), "MuroPedra");
            Caixa("Muro | leste", cenario, new Vector3(17.8f, 2f, 42.5f), new Vector3(1f, 4f, 106f), "MuroPedra");
            Caixa("Muro | sul", cenario, new Vector3(0f, 2f, -10.5f), new Vector3(36.6f, 4f, 1f), "MuroPedra");
            Caixa("Muro | norte", cenario, new Vector3(0f, 2f, 95.5f), new Vector3(36.6f, 4f, 1f), "MuroBase");

            // Ponte (sobe do rio quando a placa é ativada)
            var ponte = Caixa("Ponte", cenario, new Vector3(0f, -3.6f, 34f), new Vector3(4.2f, 0.5f, 9.4f), "MadeiraPonte");
            Definir(ponte.AddComponent<Ponte>(), "alturaFinal", -0.25f);

            // Placa de pressão
            var placa = Vazio("PlacaPressao", cenario, new Vector3(-10f, 0f, 22f));
            Caixa("Moldura", placa.transform, new Vector3(0f, 0.04f, 0f), new Vector3(2.6f, 0.08f, 2.6f), "MuroBase", false);
            var indicador = Caixa("Placa", placa.transform, new Vector3(0f, 0.09f, 0f), new Vector3(2.0f, 0.06f, 2.0f), "PlacaInativa", false);
            var tp = placa.AddComponent<BoxCollider>(); tp.isTrigger = true; tp.center = new Vector3(0f, 0.8f, 0f); tp.size = new Vector3(2f, 1.6f, 2f);
            var scrPlaca = placa.AddComponent<PlacaPressao>();
            Definir(scrPlaca, "ponte", ponte.GetComponent<Ponte>());
            Definir(scrPlaca, "indicador", indicador.GetComponent<Renderer>());
            Definir(scrPlaca, "materialAtivo", Mats["PlacaAtiva"]);
            Definir(scrPlaca, "somAtivar", Som("SFX_Portao"));

            // Nicho da pista 1 (barreira de fogo)
            Caixa("Nicho1 | parede sul", cenario, new Vector3(13.65f, 1.5f, 9.7f), new Vector3(7.3f, 3f, 0.6f), "MuroPedra");
            Caixa("Nicho1 | parede norte", cenario, new Vector3(13.65f, 1.5f, 14.3f), new Vector3(7.3f, 3f, 0.6f), "MuroPedra");
            Caixa("Nicho1 | teto", cenario, new Vector3(13.65f, 3.15f, 12f), new Vector3(7.3f, 0.3f, 5.2f), "MadeiraPonte");

            // Decoração do acampamento
            var deco = new GameObject("Decoracao").transform;
            foreach (var pos in new[] { new Vector3(-12f, 0f, -4f), new Vector3(-12f, 0f, 4f), new Vector3(12f, 0f, -3f), new Vector3(-12f, 0f, 12f) })
            {
                var barraca = Vazio("Barraca", deco, pos);
                Modelo(PastaProps + "Barraca.fbx", barraca.transform, Vector3.zero);
                var cb = barraca.AddComponent<BoxCollider>(); cb.center = new Vector3(0f, 1f, 0f); cb.size = new Vector3(2.6f, 2f, 2.6f);
            }
            foreach (var pos in new[] { new Vector3(-13f, 0f, 83f), new Vector3(13f, 0f, 83f) })
            {
                var torre = Vazio("Torre de vigia", deco, pos);
                Modelo(PastaProps + "Torre.fbx", torre.transform, Vector3.zero);
                var cb = torre.AddComponent<BoxCollider>(); cb.center = new Vector3(0f, 3f, 0f); cb.size = new Vector3(2.4f, 6f, 2.4f);
            }
            var fogueira = Vazio("Fogueira", deco, new Vector3(-5f, 0f, -3f));
            Caixa("Pedras", fogueira.transform, new Vector3(0f, 0.15f, 0f), new Vector3(1.2f, 0.3f, 1.2f), "MuroPedra");
            CriarChamas(fogueira.transform, 0.8f);
            var luzFog = Vazio("Luz", fogueira.transform, Vector3.up).AddComponent<Light>();
            luzFog.type = LightType.Point; luzFog.color = new Color(1f, 0.6f, 0.2f); luzFog.range = 7f; luzFog.intensity = 2f;

            // Área B: nicho de gelo, muro intermediário e portão
            Caixa("NichoGelo | parede sul", cenario, new Vector3(-14.15f, 1.5f, 53.7f), new Vector3(6.3f, 3f, 0.6f), "MuroBase");
            Caixa("NichoGelo | parede norte", cenario, new Vector3(-14.15f, 1.5f, 60.3f), new Vector3(6.3f, 3f, 0.6f), "MuroBase");
            Caixa("NichoGelo | teto", cenario, new Vector3(-14.15f, 3.15f, 57f), new Vector3(6.3f, 0.3f, 7.2f), "MuroBase");
            Caixa("Muro interno | oeste", cenario, new Vector3(-9.65f, 1.75f, 66f), new Vector3(15.3f, 3.5f, 0.8f), "MuroBase");
            Caixa("Muro interno | leste", cenario, new Vector3(9.65f, 1.75f, 66f), new Vector3(15.3f, 3.5f, 0.8f), "MuroBase");
            Caixa("Muralha do portao | oeste", cenario, new Vector3(-9.75f, 2.25f, 88f), new Vector3(15.1f, 4.5f, 1f), "MuroBase");
            Caixa("Muralha do portao | leste", cenario, new Vector3(9.75f, 2.25f, 88f), new Vector3(15.1f, 4.5f, 1f), "MuroBase");
            Caixa("Verga do portao", cenario, new Vector3(0f, 4.0f, 88f), new Vector3(4.6f, 1f, 1.2f), "MuroBase");

            var portao = Vazio("Portao", cenario, new Vector3(0f, 0f, 88f));
            var folhaE = Vazio("FolhaEsquerda", portao.transform, new Vector3(-2.2f, 0f, 0f));
            folhaE.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            Modelo(PastaProps + "Portao.fbx", folhaE.transform, Vector3.zero);
            var folhaD = Vazio("FolhaDireita", portao.transform, new Vector3(2.2f, 0f, 0f));
            Modelo(PastaProps + "Portao.fbx", folhaD.transform, Vector3.zero);
            var bloqPortao = portao.AddComponent<BoxCollider>();
            bloqPortao.center = new Vector3(0f, 1.7f, 0f); bloqPortao.size = new Vector3(4.4f, 3.4f, 0.6f);
            var anPortao = portao.AddComponent<Animator>();
            anPortao.runtimeAnimatorController = ctrl["Portao"];
            var scrPortao = portao.AddComponent<PortaoBase>();
            Definir(scrPortao, "animator", anPortao);
            Definir(scrPortao, "bloqueio", bloqPortao);
            Definir(scrPortao, "somAbrir", Som("SFX_Portao"));

            var saida = Vazio("SaidaFase", cenario, new Vector3(0f, 1.5f, 92f));
            var sc = saida.AddComponent<BoxCollider>(); sc.isTrigger = true; sc.size = new Vector3(4.4f, 3f, 3f);
            saida.AddComponent<SaidaFase>();

            var cp = Vazio("Checkpoint | margem da base", cenario, new Vector3(0f, 0.2f, 40.5f));
            var cpc = cp.AddComponent<BoxCollider>(); cpc.isTrigger = true; cpc.center = Vector3.up; cpc.size = new Vector3(6f, 2f, 2f);
            Definir(cp.AddComponent<Checkpoint>(), "mensagem", "Ponto seguro: margem da base ASDA. Cuidado com a afinidade dos guardas!");

            // ---------------- Prefabs na fase
            var objetos = new GameObject("Gameplay").transform;
            Instanciar(p.Caixa, objetos, new Vector3(-10f, 0f, 13f));
            Instanciar(p.Caixa, objetos, new Vector3(5f, 0f, 19f));
            Instanciar(p.Caixa, objetos, new Vector3(3.5f, 0f, 45f));
            Instanciar(p.Caixa, objetos, new Vector3(-3.5f, 0f, 71f));

            var bf1 = Instanciar(p.BarreiraFogo, objetos, new Vector3(10.3f, 0f, 12f), 90f);
            Definir(bf1.GetComponent<BarreiraFogo>(), "idBarreira", "Acampamento");
            var bf2 = Instanciar(p.BarreiraFogo, objetos, new Vector3(0f, 0f, 66f));
            Definir(bf2.GetComponent<BarreiraFogo>(), "idBarreira", "MuroInterno");
            Definir(bf2.GetComponent<BarreiraFogo>(), "acertosParaApagar", 3);

            var gelo = Instanciar(p.BarreiraGelo, objetos, new Vector3(-11f, 0f, 57f), 90f);
            Definir(gelo.GetComponent<BarreiraGelo>(), "idBarreira", "NichoGelo");

            var p1 = Instanciar(p.Pista, objetos, new Vector3(14.5f, 0f, 12f));
            Definir(p1.GetComponent<Pista>(), "idPista", "1");
            Definir(p1.GetComponent<Pista>(), "texto", "Pista 1/3: relatório da DASA — o \"acampamento de proteção\" é fachada da ASDA.");
            var p2 = Instanciar(p.Pista, objetos, new Vector3(-14.5f, 0f, 57f));
            Definir(p2.GetComponent<Pista>(), "idPista", "2");
            Definir(p2.GetComponent<Pista>(), "texto", "Pista 2/3: mapa da base com a profecia do fogo e da água anotada.");

            CriarGuarda(p, objetos, "G1", Elemento.Fogo, new Vector3(-7f, 0f, 47f), new Vector3(8f, 0f, 0f), 60f, null);
            CriarGuarda(p, objetos, "G2", Elemento.Agua, new Vector3(7f, 0f, 52f), new Vector3(-6f, 0f, 0f), 60f, null);
            CriarGuarda(p, objetos, "G3", Elemento.Fogo, new Vector3(-6f, 0f, 72f), new Vector3(0f, 0f, 8f), 60f, null);
            CriarGuarda(p, objetos, "G4-Elite", Elemento.Agua, new Vector3(6f, 0f, 78f), new Vector3(-5f, 0f, 0f), 120f, p.Pista);

            // ---------------- Jogador e câmera
            var magnus = Instanciar(p.Magnus, null, new Vector3(0f, 0.05f, -3f));
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var camera = cam.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 300f;
            cam.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0f, 3.7f, -9f);
            cam.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            Definir(cam.AddComponent<CameraTerceiraPessoa>(), "alvo", magnus.transform);
            Definir(magnus.GetComponent<MagnusMovimento>(), "referenciaCamera", cam.transform);
            Definir(magnus.GetComponent<MagnusPoderes>(), "referenciaCamera", cam.transform);

            // ---------------- HUD
            CriarHud(magnus, scrPlaca);
            CriarEventSystem();

            EditorSceneManager.SaveScene(cena, PastaCenas + "/" + NomesCena.Fase + ".unity");
        }

        private static void CriarGuarda(Prefabs p, Transform pai, string id, Elemento elemento, Vector3 pos, Vector3 patrulha, float vida, GameObject drop)
        {
            var g = Instanciar(p.Guarda, pai, pos, 180f);
            g.name = "Guarda " + id + " (" + ElementoUtil.Nome(elemento) + ")";
            var scr = g.GetComponent<Guarda>();
            Definir(scr, "idGuarda", id);
            Definir(scr, "afinidade", elemento);
            Definir(scr, "deslocamentoPatrulha", patrulha);
            Definir(scr, "vidaMaxima", vida);
            if (vida > 60f)
            {
                Definir(scr, "pontos", 100);
                g.transform.localScale = Vector3.one * 1.15f;
            }
            // A aura já aparece correta na cena (o script também ajusta ao iniciar).
            g.transform.Find("Aura").GetComponent<Renderer>().sharedMaterial = Mats[elemento == Elemento.Fogo ? "AuraFogo" : "AuraAgua"];
            if (drop != null)
            {
                Definir(scr, "pistaAoMorrer", drop.GetComponent<Pista>());
                Definir(scr, "idPistaAoMorrer", "3");
            }
        }

        // ------------------------------------------------------------------ UI
        private static Font Fonte()
        {
            if (fonte == null)
                fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return fonte;
        }

        private static RectTransform Retangulo(string nome, Transform pai, Vector2 ancoraMin, Vector2 ancoraMax, Vector2 pivo, Vector2 pos, Vector2 tamanho)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(pai, false);
            rt.anchorMin = ancoraMin;
            rt.anchorMax = ancoraMax;
            rt.pivot = pivo;
            rt.anchoredPosition = pos;
            rt.sizeDelta = tamanho;
            return rt;
        }

        private static Text Texto(string nome, Transform pai, Vector2 ancora, Vector2 pivo, Vector2 pos, Vector2 tamanho, string conteudo, int tamanhoFonte,
            TextAnchor alinhamento, Color cor, FontStyle estilo = FontStyle.Normal)
        {
            var rt = Retangulo(nome, pai, ancora, ancora, pivo, pos, tamanho);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Fonte();
            t.text = conteudo;
            t.fontSize = tamanhoFonte;
            t.alignment = alinhamento;
            t.color = cor;
            t.fontStyle = estilo;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var sombra = rt.gameObject.AddComponent<Outline>();
            sombra.effectColor = new Color(0f, 0f, 0f, 0.85f);
            sombra.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        private static Image Imagem(string nome, Transform pai, Vector2 ancoraMin, Vector2 ancoraMax, Vector2 pivo, Vector2 pos, Vector2 tamanho, Color cor)
        {
            var rt = Retangulo(nome, pai, ancoraMin, ancoraMax, pivo, pos, tamanho);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = cor;
            img.raycastTarget = false;
            return img;
        }

        private static Button Botao(Transform pai, string rotulo, Vector2 pos, UnityAction acao, float largura = 360f)
        {
            var img = Imagem("Botao " + rotulo, pai, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(largura, 72f),
                new Color(0.10f, 0.20f, 0.36f, 0.95f));
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<Button>();
            var cores = b.colors;
            cores.highlightedColor = new Color(1f, 0.75f, 0.45f);
            cores.pressedColor = new Color(0.6f, 0.85f, 1f);
            b.colors = cores;
            var t = Texto("Rotulo", img.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(largura, 72f), rotulo, 32,
                TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            t.raycastTarget = false;
            UnityEventTools.AddPersistentListener(b.onClick, acao);
            return b;
        }

        private static Canvas NovoCanvas(string nome)
        {
            var go = new GameObject(nome, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var cs = go.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920f, 1080f);
            cs.matchWidthOrHeight = 0.5f;
            return c;
        }

        private static void CriarEventSystem()
        {
            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            es.AddComponent<InputSystemUIInputModule>();
        }

        private static RectTransform Barra(Transform pai, string nome, Vector2 pos, Color cor, string rotulo)
        {
            var fundo = Imagem(nome, pai, Vector2.zero, Vector2.zero, Vector2.zero, pos, new Vector2(380f, 28f), new Color(0f, 0f, 0f, 0.6f));
            var preench = Imagem("Preenchimento", fundo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero, cor);
            preench.rectTransform.offsetMin = new Vector2(3f, 3f);
            preench.rectTransform.offsetMax = new Vector2(-3f, -3f);
            Texto("Rotulo", fundo.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(360f, 28f), rotulo, 20,
                TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            return preench.rectTransform;
        }

        private static void CriarHud(GameObject magnus, PlacaPressao placa)
        {
            var canvas = NovoCanvas("HUD");
            var raiz = canvas.transform;
            var hud = canvas.gameObject.AddComponent<HudPartida>();
            var branco = Color.white;
            var tl = new Vector2(0f, 1f);

            Imagem("Painel placar", raiz, tl, tl, tl, new Vector2(20f, -20f), new Vector2(330f, 150f), new Color(0f, 0f, 0f, 0.45f));
            var tPontos = Texto("Pontuacao", raiz, tl, tl, new Vector2(40f, -28f), new Vector2(320f, 44f), "Pontos: 0", 36, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.3f), FontStyle.Bold);
            var tPistas = Texto("Pistas", raiz, tl, tl, new Vector2(40f, -76f), new Vector2(320f, 36f), "Pistas: 0/3", 28, TextAnchor.UpperLeft, branco);
            var tTempo = Texto("Tempo", raiz, tl, tl, new Vector2(40f, -114f), new Vector2(320f, 36f), "Tempo 00:00", 26, TextAnchor.UpperLeft, branco);

            var topo = new Vector2(0.5f, 1f);
            Imagem("Fundo objetivo", raiz, topo, topo, topo, new Vector2(0f, -20f), new Vector2(980f, 52f), new Color(0f, 0f, 0f, 0.45f));
            var tObj = Texto("Objetivo", raiz, topo, topo, new Vector2(0f, -26f), new Vector2(960f, 44f), "Objetivo", 24, TextAnchor.UpperCenter, branco);

            var tAviso = Texto("Aviso", raiz, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(1300f, 90f), "", 30,
                TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.6f), FontStyle.Bold);

            var tr = new Vector2(1f, 1f);
            Texto("Controles", raiz, tr, tr, new Vector2(-20f, -20f), new Vector2(430f, 200f),
                "WASD: mover   Shift: correr\nEspaço: pular   Mouse: câmera\nClique / F: lançar elemento\nQ / Tab / roda: trocar elemento\nEsc: pausa", 20,
                TextAnchor.UpperRight, new Color(1f, 1f, 1f, 0.9f));

            Texto("Mira", raiz, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(60f, 60f), "+", 40, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.8f));

            var bl = Vector2.zero;
            Imagem("Painel status", raiz, bl, bl, bl, new Vector2(20f, 20f), new Vector2(420f, 150f), new Color(0f, 0f, 0f, 0.45f));
            var bVida = Barra(raiz, "Barra vida", new Vector2(40f, 110f), new Color(0.85f, 0.2f, 0.2f), "VIDA");
            var bEnergia = Barra(raiz, "Barra energia", new Vector2(40f, 72f), new Color(0.3f, 0.6f, 1f), "ENERGIA");
            var icone = Imagem("Icone elemento", raiz, bl, bl, bl, new Vector2(40f, 32f), new Vector2(28f, 28f), new Color(1f, 0.45f, 0.1f));
            var tElem = Texto("Elemento", raiz, bl, bl, new Vector2(78f, 28f), new Vector2(340f, 36f), "Elemento: FOGO", 26, TextAnchor.MiddleLeft, new Color(1f, 0.45f, 0.1f), FontStyle.Bold);

            // Painel de pausa
            var pausa = Imagem("Painel pausa", raiz, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.7f));
            pausa.raycastTarget = true;
            Texto("Titulo", pausa.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(800f, 100f), "PAUSADO", 72, TextAnchor.MiddleCenter, branco, FontStyle.Bold);
            Botao(pausa.transform, "Continuar", new Vector2(0f, 50f), hud.BotaoContinuar);
            Botao(pausa.transform, "Reiniciar fase", new Vector2(0f, -40f), hud.BotaoReiniciar);
            Botao(pausa.transform, "Menu principal", new Vector2(0f, -130f), hud.BotaoMenu);

            // Painel de resultado (vitória/derrota)
            var res = Imagem("Painel resultado", raiz, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.08f, 0.85f));
            res.raycastTarget = true;
            var tTit = Texto("Titulo", res.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 280f), new Vector2(1000f, 120f), "VITÓRIA!", 96, TextAnchor.MiddleCenter, branco, FontStyle.Bold);
            var tDet = Texto("Detalhe", res.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1100f, 300f), "", 34, TextAnchor.MiddleCenter, branco);
            Botao(res.transform, "Jogar novamente", new Vector2(0f, -170f), hud.BotaoReiniciar, 420f);
            Botao(res.transform, "Menu principal", new Vector2(0f, -260f), hud.BotaoMenu, 420f);

            Definir(hud, "textoPontuacao", tPontos);
            Definir(hud, "textoPistas", tPistas);
            Definir(hud, "textoTempo", tTempo);
            Definir(hud, "textoElemento", tElem);
            Definir(hud, "textoObjetivo", tObj);
            Definir(hud, "textoAviso", tAviso);
            Definir(hud, "barraVida", bVida);
            Definir(hud, "barraEnergia", bEnergia);
            Definir(hud, "iconeElemento", icone);
            Definir(hud, "painelPausa", pausa.gameObject);
            Definir(hud, "painelResultado", res.gameObject);
            Definir(hud, "textoTituloResultado", tTit);
            Definir(hud, "textoDetalheResultado", tDet);
            Definir(hud, "magnus", magnus.GetComponent<MagnusStatus>());
            Definir(hud, "poderes", magnus.GetComponent<MagnusPoderes>());
            Definir(hud, "placa", placa);
            Definir(hud, "somClique", Som("SFX_Clique"));
            pausa.gameObject.SetActive(false);
            res.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ cena de menu
        private static void CriarCenaMenu(Dictionary<string, AnimatorController> ctrl)
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Iluminacao(new Color(0.45f, 0.5f, 0.65f), new Color(0.15f, 0.12f, 0.1f), false);

            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var camera = cam.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.07f, 0.13f);
            camera.fieldOfView = 40f;
            cam.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0f, 1.3f, -5.2f);
            cam.transform.rotation = Quaternion.Euler(4f, 0f, 0f);

            var audio = new GameObject("Audio").AddComponent<GerenciadorAudio>();
            Definir(audio, "musica", Som("Musica_Base"));

            // Vitrine 3D: Magnus girando sobre um pedestal, com chamas e um guarda ao fundo.
            var vitrine = Vazio("Vitrine", null, new Vector3(1.25f, 0f, 0f));
            vitrine.transform.rotation = Quaternion.Euler(0f, 160f, 0f);
            var modelo = Modelo(ModeloMagnus, vitrine.transform, Vector3.zero, 1f, false);
            var an = modelo.GetComponent<Animator>();
            if (an == null) an = modelo.AddComponent<Animator>();
            an.runtimeAnimatorController = ctrl["Magnus"];
            Primitiva(PrimitiveType.Cylinder, "Pedestal", null, new Vector3(1.25f, -0.1f, 0f), new Vector3(1.6f, 0.1f, 1.6f), "PedestalMenu");
            Primitiva(PrimitiveType.Cube, "Chao", null, new Vector3(1.5f, -0.25f, 4f), new Vector3(14f, 0.1f, 14f), "MuroBase");
            var guarda = Vazio("Guarda ao fundo", null, new Vector3(2.9f, -0.2f, 2.8f));
            guarda.transform.rotation = Quaternion.Euler(0f, 200f, 0f);
            Modelo(ModeloSoldado, guarda.transform, Vector3.zero, 0.92f);
            var chamas = Vazio("Chamas", null, new Vector3(0.6f, 0f, 2.5f));
            CriarChamas(chamas.transform, 1.5f);
            var luzF = Vazio("Luz fogo", null, new Vector3(0.2f, 1f, 1.5f)).AddComponent<Light>();
            luzF.type = LightType.Point; luzF.color = new Color(1f, 0.5f, 0.15f); luzF.range = 5f; luzF.intensity = 2.5f;
            var luzA = Vazio("Luz agua", null, new Vector3(2.4f, 1.2f, -0.8f)).AddComponent<Light>();
            luzA.type = LightType.Point; luzA.color = new Color(0.2f, 0.6f, 1f); luzA.range = 5f; luzA.intensity = 2.5f;

            var canvas = NovoCanvas("Canvas menu");
            var raiz = canvas.transform;
            var menu = canvas.gameObject.AddComponent<MenuPrincipal>();
            var esq = new Vector2(0f, 0.5f);
            Texto("Titulo", raiz, esq, esq, new Vector2(110f, 300f), new Vector2(1400f, 130f), "ENTRE FOGO E ÁGUA", 92, TextAnchor.MiddleLeft, new Color(1f, 0.75f, 0.35f), FontStyle.Bold);
            Texto("Subtitulo", raiz, esq, esq, new Vector2(115f, 210f), new Vector2(900f, 60f), "Infiltração na Base ASDA", 42, TextAnchor.MiddleLeft, new Color(0.55f, 0.8f, 1f));

            var colunaBotoes = Retangulo("Botoes", raiz, esq, esq, new Vector2(0f, 0.5f), new Vector2(110f, -20f), new Vector2(420f, 400f));
            Botao(colunaBotoes, "Jogar", new Vector2(0f, 110f), menu.BotaoJogar, 400f);
            Botao(colunaBotoes, "Como jogar", new Vector2(0f, 20f), menu.BotaoComoJogar, 400f);
            Botao(colunaBotoes, "Sair", new Vector2(0f, -70f), menu.BotaoSair, 400f);

            Texto("Creditos", raiz, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, 40f), new Vector2(1400f, 60f),
                "Projeto de Jogos Digitais II — Larissa Campos Cardoso • Izak Pinheiro • Matheus Henrique", 22, TextAnchor.LowerLeft, new Color(1f, 1f, 1f, 0.7f));

            var painel = Imagem("Painel como jogar", raiz, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120f, 0f), new Vector2(1150f, 820f),
                new Color(0.03f, 0.05f, 0.1f, 0.95f));
            painel.raycastTarget = true;
            Texto("Titulo", painel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1000f, 70f), "COMO JOGAR", 52, TextAnchor.MiddleCenter, new Color(1f, 0.75f, 0.35f), FontStyle.Bold);
            Texto("Instrucoes", painel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1050f, 600f),
                "Magnus descobriu que a ASDA quer eliminá-lo. Encontre as 3 PISTAS e fuja pelo portão da base.\n\n" +
                "WASD / setas: mover   •   Shift: correr   •   Espaço: pular   •   Mouse: girar a câmera\n" +
                "Clique esquerdo ou F: lançar o elemento   •   Q / Tab / roda do mouse: trocar Fogo ↔ Água   •   Esc: pausa\n\n" +
                "REGRA DOS ELEMENTOS\n" +
                "• Guardas de FOGO (aura laranja) absorvem fogo e só sofrem dano de ÁGUA — e vice-versa.\n" +
                "• Barreiras de fogo só apagam com ÁGUA. O gelo só derrete com FOGO (água reforça o gelo).\n" +
                "• Cada lançamento gasta energia, que se recupera com o tempo.\n\n" +
                "DESAFIOS: empurre uma caixa até a placa dourada para erguer a ponte; cair no rio custa vida.\n" +
                "Pontos: pistas +100, guardas +50 (elite +100), barreiras e placa +25, bônus de tempo na vitória.\n" +
                "Derrota: a vida de Magnus chega a zero.",
                26, TextAnchor.UpperLeft, Color.white);
            Botao(painel.transform, "Voltar", new Vector2(0f, -340f), menu.BotaoComoJogar, 300f);

            Definir(menu, "painelComoJogar", painel.gameObject);
            Definir(menu, "vitrine", vitrine.transform);
            Definir(menu, "somClique", Som("SFX_Clique"));
            CriarEventSystem();

            EditorSceneManager.SaveScene(cena, PastaCenas + "/" + NomesCena.Menu + ".unity");
        }

        private static void ConfigurarBuild()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(PastaCenas + "/" + NomesCena.Menu + ".unity", true),
                new EditorBuildSettingsScene(PastaCenas + "/" + NomesCena.Fase + ".unity", true)
            };
        }
    }
}
