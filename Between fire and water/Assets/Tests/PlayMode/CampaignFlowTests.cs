using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class CampaignFlowTests
{
    const string SaveKey = "EntreFogoAgua.Campanha.1";
    const string VolumeKey = SaveKey + ".volume";
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    Type campaignType;
    Component campaign;
    bool hadSave, hadVolume;
    string oldSave;
    float oldVolume;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        hadSave = PlayerPrefs.HasKey(SaveKey);
        oldSave = hadSave ? PlayerPrefs.GetString(SaveKey) : null;
        hadVolume = PlayerPrefs.HasKey(VolumeKey);
        oldVolume = hadVolume ? PlayerPrefs.GetFloat(VolumeKey) : 0f;
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(VolumeKey);
        PlayerPrefs.Save();

        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        yield return null;
        yield return null;
        campaignType = Type.GetType("EntregaBasica, Assembly-CSharp");
        Assert.That(campaignType, Is.Not.Null, "Runtime campaign script must be compiled.");
        campaign = Resources.FindObjectsOfTypeAll(campaignType)
            .OfType<Component>()
            .FirstOrDefault(component => component.gameObject.scene.IsValid());
        Assert.That(campaign, Is.Not.Null, "Campaign bootstrap should run in SampleScene.");
        Invoke("StartNewGame");
        SetField(campaign, "mode", Enum.Parse(campaignType.GetNestedType("Mode", BindingFlags.NonPublic), "Explore"));
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (hadSave) PlayerPrefs.SetString(SaveKey, oldSave); else PlayerPrefs.DeleteKey(SaveKey);
        if (hadVolume) PlayerPrefs.SetFloat(VolumeKey, oldVolume); else PlayerPrefs.DeleteKey(VolumeKey);
        PlayerPrefs.Save();
        yield return null;
    }

    [UnityTest]
    public IEnumerator SceneLoadsAllExploreMapsAndCollisionLimits()
    {
        var maps = (Sprite[])GetField(campaign, "explorationMaps");
        Assert.That(maps.Length, Is.EqualTo(3));
        CollectionAssert.AreEqual(new[] { "acampamento-exploracao", "asda-exploracao", "base-asda" }, maps.Select(sprite => sprite.name).ToArray());
        Assert.That(UnityEngine.Object.FindObjectsByType<BoxCollider2D>().Count(collider => collider.name.StartsWith("Limite ")), Is.EqualTo(4));
        Assert.That(GetField(campaign, "body"), Is.TypeOf<Rigidbody2D>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator BattleAwardsXpAndPersistsProgress()
    {
        SetHeroPosition(new Vector2(-5, 0));
        Invoke("Interact");
        AssertMode("Battle");
        yield return RunAction(0);
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(91));
        yield return RunAction(1);
        AssertMode("Victory");
        Assert.That(ReadProgressInt("xp"), Is.EqualTo(20));
        Assert.That(((bool[])GetField(GetField(campaign, "data"), "won"))[0], Is.True);
        StringAssert.Contains("\"xp\":20", PlayerPrefs.GetString(SaveKey));
    }

    [UnityTest]
    public IEnumerator DefeatCanRetryWithoutLosingCheckpoint()
    {
        SetProgressField("hp", 1);
        Invoke("Save");
        SetHeroPosition(new Vector2(-5, 0));
        Invoke("Interact");
        yield return RunAction(0);
        AssertMode("Defeat");
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(0));
        Invoke("RetryBattle");
        AssertMode("Battle");
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(1));
        Assert.That((int)GetField(campaign, "enemyHP"), Is.EqualTo(38));
    }

    [UnityTest]
    public IEnumerator DefenseReducesDamageAndRestoresEnergy()
    {
        SetProgressField("energy", 3);
        Invoke("Save");
        SetHeroPosition(new Vector2(-5, 0));
        Invoke("Interact");
        AssertMode("Battle");
        yield return RunAction(3);
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(96));
        Assert.That(ReadProgressInt("energy"), Is.EqualTo(6));
        AssertMode("Battle");
    }

    [UnityTest]
    public IEnumerator WaterAttackConsumesEnergyAndWeakensTheCounterattack()
    {
        SetProgressField("energy", 2);
        Invoke("Save");
        SetHeroPosition(new Vector2(-5, 0));
        Invoke("Interact");
        yield return RunAction(2);
        Assert.That((int)GetField(campaign, "enemyHP"), Is.EqualTo(22));
        Assert.That(ReadProgressInt("energy"), Is.EqualTo(0));
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(95));
        AssertMode("Battle");
    }

    [UnityTest]
    public IEnumerator MenuConfirmsSaveReplacementAndDeletion()
    {
        SetProgressField("xp", 20);
        SetProgressField("won", new[] { true, false, false, false, false, false });
        Invoke("Save");
        Invoke("RequestNewGame");
        AssertMode("ConfirmNewGame");
        Assert.That(ReadProgressInt("xp"), Is.EqualTo(20));
        Assert.That(PlayerPrefs.HasKey(SaveKey), Is.True);

        SetField(campaign, "mode", Enum.Parse(campaignType.GetNestedType("Mode", BindingFlags.NonPublic), "Menu"));
        Invoke("RequestDeleteSave");
        AssertMode("ConfirmDeleteSave");
        Assert.That(PlayerPrefs.HasKey(SaveKey), Is.True);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SettingsClampAndPersistVolume()
    {
        Invoke("SetVolume", 1.5f);
        Assert.That(((AudioSource)GetField(campaign, "speaker")).volume, Is.EqualTo(1f));
        Assert.That(((AudioSource)GetField(campaign, "footsteps")).volume, Is.EqualTo(1f));
        Assert.That(PlayerPrefs.GetFloat(VolumeKey), Is.EqualTo(1f));
        Invoke("SetVolume", .45f);
        Assert.That(((AudioSource)GetField(campaign, "speaker")).volume, Is.EqualTo(.45f).Within(.001f));
        Assert.That(PlayerPrefs.GetFloat(VolumeKey), Is.EqualTo(.45f).Within(.001f));
        Assert.That(((AudioSource)GetField(campaign, "footsteps")).volume, Is.EqualTo(.45f).Within(.001f));
        yield return null;
    }

    [UnityTest]
    public IEnumerator AttackAndWalkingAudioResourcesAreLoaded()
    {
        Assert.That(GetField(campaign, "waterAttackCue"), Is.TypeOf<AudioClip>());
        Assert.That(GetField(campaign, "fireAttackCue"), Is.TypeOf<AudioClip>());
        Assert.That(GetField(campaign, "walkingCue"), Is.TypeOf<AudioClip>());
        Assert.That(GetField(campaign, "finalCreditsClip"), Is.Not.Null);
        Assert.That(((AudioSource)GetField(campaign, "footsteps")).loop, Is.True);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MarkWarnsBeforeHeavyStrike()
    {
        SetProgressField("phase", 2);
        SetProgressField("xp", 80);
        SetProgressField("hp", 125);
        SetProgressField("won", new[] { true, true, true, true, false, false });
        SetProgressField("clues", new[] { true, true, true });
        Invoke("BuildMap");
        SetHeroPosition(new Vector2(6, 0));
        Invoke("Interact");
        AssertMode("Battle");

        yield return RunAction(0);
        Assert.That((bool)GetField(campaign, "bossCharging"), Is.True);
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(103));
        StringAssert.Contains("ataque forte", (string)GetField(campaign, "log"));

        yield return RunAction(0);
        Assert.That((bool)GetField(campaign, "bossCharging"), Is.False);
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(59));
        StringAssert.Contains("golpe anunciado", (string)GetField(campaign, "log"));
    }

    [UnityTest]
    public IEnumerator LegacySaveMigratesAndCorruptSaveCanBeDeleted()
    {
        PlayerPrefs.SetString(SaveKey,
            "{\"phase\":0,\"xp\":0,\"hp\":100,\"energy\":8,\"won\":[false,false,false,false,false,false],\"clues\":[false,false,false],\"finished\":false}");
        PlayerPrefs.Save();
        Invoke("Continue");
        AssertMode("Explore");
        Assert.That((int)GetField(GetField(campaign, "data"), "schema"), Is.EqualTo(1));
        StringAssert.Contains("\"schema\":1", PlayerPrefs.GetString(SaveKey));

        PlayerPrefs.SetString(SaveKey, "{isso não é json");
        PlayerPrefs.Save();
        SetField(campaign, "mode", Enum.Parse(campaignType.GetNestedType("Mode", BindingFlags.NonPublic), "Menu"));
        Invoke("Continue");
        AssertMode("Menu");
        Assert.That(((string)GetField(campaign, "notice")).Length, Is.GreaterThan(0));
        Invoke("DeleteSave");
        Assert.That(PlayerPrefs.HasKey(SaveKey), Is.False);
        yield return null;
    }

    [UnityTest]
    public IEnumerator PhaseGateFadesAutosavesAndContinueRestoresProgress()
    {
        SetProgressField("xp", 40);
        SetProgressField("hp", 90);
        SetProgressField("energy", 3);
        SetProgressField("won", new[] { true, true, false, false, false, false });
        SetProgressField("clues", new[] { true, false, false });
        Invoke("BuildMap");
        SetHeroPosition(new Vector2(9, 3.5f));
        Invoke("Interact");
        AssertMode("Transition");
        yield return new WaitForSecondsRealtime(1.2f);
        Assert.That(ReadProgressInt("phase"), Is.EqualTo(1));
        Assert.That(ReadProgressInt("hp"), Is.EqualTo(125));
        Assert.That(ReadProgressInt("energy"), Is.EqualTo(8));
        StringAssert.Contains("\"phase\":1", PlayerPrefs.GetString(SaveKey));

        SetProgressField("phase", 0);
        Invoke("Continue");
        Assert.That(ReadProgressInt("phase"), Is.EqualTo(1));
        AssertMode("Explore");

        SetProgressField("won", new[] { true, true, true, true, false, false });
        SetProgressField("xp", 80);
        SetProgressField("clues", new[] { true, true, true });
        Invoke("BuildMap");
        SetHeroPosition(new Vector2(9, 3.5f));
        Invoke("Interact");
        AssertMode("Transition");
        yield return new WaitForSecondsRealtime(1.2f);
        Assert.That(ReadProgressInt("phase"), Is.EqualTo(2));

        SetProgressField("won", new[] { true, true, true, true, true, true });
        SetProgressField("xp", 120);
        Invoke("BuildMap");
        SetHeroPosition(new Vector2(9, 3.5f));
        Invoke("Interact");
        AssertMode("Ending");
        Assert.That((bool)GetField(GetField(campaign, "data"), "finished"), Is.True);
    }

    IEnumerator RunAction(int action)
    {
        var method = campaignType.GetMethod("Act", Fields);
        var routine = (IEnumerator)method.Invoke(campaign, new object[] { action });
        ((MonoBehaviour)campaign).StartCoroutine(routine);
        yield return new WaitForSecondsRealtime(.8f);
    }

    void SetHeroPosition(Vector2 position)
    {
        ((Rigidbody2D)GetField(campaign, "body")).position = position;
    }

    int ReadProgressInt(string field)
    {
        return (int)GetField(GetField(campaign, "data"), field);
    }

    void SetProgressField(string field, object value)
    {
        SetField(GetField(campaign, "data"), field, value);
    }

    void AssertMode(string expected)
    {
        Assert.That(GetField(campaign, "mode").ToString(), Is.EqualTo(expected));
    }

    void Invoke(string name)
    {
        campaignType.GetMethod(name, Fields).Invoke(campaign, null);
    }

    void Invoke(string name, params object[] args)
    {
        campaignType.GetMethod(name, Fields).Invoke(campaign, args);
    }

    static object GetField(object target, string name)
    {
        return target.GetType().GetField(name, Fields).GetValue(target);
    }

    static void SetField(object target, string name, object value)
    {
        target.GetType().GetField(name, Fields).SetValue(target, value);
    }
}
