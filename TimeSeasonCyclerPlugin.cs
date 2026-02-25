using BepInEx;
using HarmonyLib;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;
using System.IO;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace BaldiPlus_Seasons;

[BepInPlugin("alexbw145.baldiplus.seasons", "Day & Season Cycle", "2.0.0.0")]
[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", "10.0.0.0")]
public class TimeSeasonCyclerPlugin : BaseUnityPlugin
{
    public static TimeSeasonCyclerPlugin Instance { get; private set; }
    public static bool southern { get; internal set; } = false;
    public static bool eastern { get; internal set; } = false;
    public static readonly AssetManager assetMan = new AssetManager();

    internal static ConfigEntry<string> Location;

    private void Awake()
    {
        Harmony harmony = new Harmony("alexbw145.baldiplus.seasons");
        Location = Config.Bind("OpenWeather API", "Location", "New York, USA", "Used for weather purposes (besides the time and season), View https://openweathermap.org/weathermap for locations.");
        new GameObject("Cycle Manager", typeof(CycleManager));
        Instance = this;
        harmony.PatchAllConditionals();

        //CycleManager.MIDIsongs.Add(AssetLoader.MidiFromFile(Path.Combine(AssetLoader.GetModPath(this), "MidiDB", "school_winter.mid"), "school_winter"));
        LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad, LoadingEventOrder.Pre);
        LoadingEvents.RegisterOnAssetsLoaded(Info, () =>
        {
            // Adding deez...
            foreach (var playground in Resources.FindObjectsOfTypeAll<RoomAsset>().Where(x => x.roomFunctionContainer != null && x.roomFunctionContainer.gameObject.GetComponent<SunlightRoomFunction>() != null))
                CycleManager.Instance.AddNewRoomTarget(playground, assetMan.Get<Texture2D[]>("Grass").ToList(), true);
        }, LoadingEventOrder.Post);

        CustomOptionsCore.OnMenuInitialize += AddOptions;
        ModdedSaveSystem.AddSaveLoadAction(this, (isSave, path) =>
        {
            if (isSave)
                File.WriteAllText(Path.Combine(path, "managerOptions.txt"), southern.ToString() +"\n"+ eastern.ToString());
            else if (File.Exists(Path.Combine(path, "managerOptions.txt")))
            {
                southern = bool.Parse(File.ReadAllLines(Path.Combine(path, "managerOptions.txt"))[0]);
                eastern = bool.Parse(File.ReadAllLines(Path.Combine(path, "managerOptions.txt"))[1]);
            }
        });
    }

    private void PreLoad()
    {
        assetMan.Add<Texture2D[]>("Grass", [
            AssetLoader.TextureFromMod(this, "Texture2D", "Grass_Spring.png"),
            Resources.FindObjectsOfTypeAll<Texture2D>().Last(g => g.name == "Grass"),
            AssetLoader.TextureFromMod(this, "Texture2D", "Grass_Autumn.png"),
            AssetLoader.TextureFromMod(this, "Texture2D", "Grass_Winter.png")]);
        var summerTree = Resources.FindObjectsOfTypeAll<Material>().Last(g => g.name == "TreeCG"); // Avoid reusing the resources function
        var autumnTree = Instantiate(summerTree);
        autumnTree.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "TreeCGAutumn.png"));
        autumnTree.name = "AutumnTree";
        var winterTree = Instantiate(summerTree);
        winterTree.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "TreeSnowed.png"));
        winterTree.name = "WinterTree";
        var springTree = Instantiate(summerTree);
        springTree.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "TreeCGSpring.png"));
        springTree.name = "SpringTree";
        var summerSunsetTree = Resources.FindObjectsOfTypeAll<Sprite>().Last(g => g.name == "Tree_Sunset");
        var autumnSunsetTree = AssetLoader.SpriteFromMod(this, summerSunsetTree.pivot / 128f, summerSunsetTree.pixelsPerUnit, "Texture2D", "Tree_SunsetAutumn.png");
        autumnSunsetTree.name = "AutumnSunsetTree";
        var winterSunsetTree = AssetLoader.SpriteFromMod(this, summerSunsetTree.pivot / 128f, summerSunsetTree.pixelsPerUnit, "Texture2D", "Tree_SunsetSnowed.png");
        winterSunsetTree.name = "WinterSunsetTree";
        var springSunsetTree = AssetLoader.SpriteFromMod(this, summerSunsetTree.pivot / 128f, summerSunsetTree.pixelsPerUnit, "Texture2D", "Tree_SunsetSpring.png");
        springSunsetTree.name = "SpringSunsetTree";
        var summerSunsetBarrier = Resources.FindObjectsOfTypeAll<Material>().Last(g => g.name == "TreeLine_Sunset");
        var autumnSunsetBarrier = Instantiate(summerSunsetBarrier);
        autumnSunsetBarrier.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "TreeLine_SunsetAutumn.png"));
        autumnSunsetBarrier.name = "TreeLine_SunsetAutumn";
        var winterSunsetBarrier = Instantiate(summerSunsetBarrier);
        winterSunsetBarrier.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "TreeLine_SunsetSnowed.png"));
        winterSunsetBarrier.name = "TreeLine_SunsetWinter";
        var springSunsetBarrier = Instantiate(summerSunsetBarrier);
        springSunsetBarrier.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "TreeLine_SunsetSpring.png"));
        springSunsetBarrier.name = "TreeLine_SunsetSpring";
        var bullyTreeSummer = Resources.FindObjectsOfTypeAll<Sprite>().Last(x => x.name == "BullyTree_Sprite"); // Blend in?? What are you doing Bully??
        var bullyTreeAutumn = AssetLoader.SpriteFromMod(this, bullyTreeSummer.pivot / 128f, bullyTreeSummer.pixelsPerUnit, "Texture2D", "BullyTree_Autumn.png");
        bullyTreeAutumn.name = "BullyTree_AutumnSprite";
        var bullyTreeWinter = AssetLoader.SpriteFromMod(this, bullyTreeSummer.pivot / 128f, bullyTreeSummer.pixelsPerUnit, "Texture2D", "BullyTree_Snowed.png");
        bullyTreeWinter.name = "BullyTree_WinterSprite";
        var bullyTreeSpring = AssetLoader.SpriteFromMod(this, bullyTreeSummer.pivot / 128f, bullyTreeSummer.pixelsPerUnit, "Texture2D", "BullyTree_Spring.png");
        bullyTreeSpring.name = "BullyTree_SpringSprite";

        assetMan.Add<Material[]>("Tree", [
            springTree,
            summerTree,
            autumnTree,
            winterTree,
        ]);
        assetMan.Add<Sprite[]>("SunsetTree", [
            springSunsetTree,
            summerSunsetTree,
            autumnSunsetTree,
            winterSunsetTree,
            ]);
        assetMan.Add<Material[]>("CampingBarrier", [
            springSunsetBarrier,
            summerSunsetBarrier,
            autumnSunsetBarrier,
            winterSunsetBarrier,
        ]);
        assetMan.Add<Sprite[]>("BullyCampTree", [
            bullyTreeSpring,
            bullyTreeSummer,
            bullyTreeAutumn,
            bullyTreeWinter
            ]);
        assetMan.Add<Cubemap>("NightSky", AssetLoader.CubemapFromMod(this, "Texture2D", "Cubemap_Night.png"));

        var thing = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "DustTest"));
        thing.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "Droplet.png"));
        assetMan.Add("Droplet", thing);

        var thing2 = Material.Instantiate(summerTree);
        thing2.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "HugeSnowman.png"));
        var smallsnowman = Material.Instantiate(summerTree);
        smallsnowman.SetMainTexture(AssetLoader.TextureFromMod(this, "Texture2D", "SmallSnowman.png"));

        // Spawns in all seasons which was supposed to spawn in winter only, useless.
        /*Resources.FindObjectsOfTypeAll<RoomAsset>().ToList().Find(x => x.name.Contains("Playground")).basicSwaps.Add(
            new BasicObjectSwapData()
            {
                prefabToSwap = Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TreeCG").transform,
                potentialReplacements = [new WeightedTransform()
                {
                    selection = CycleManager.snowman.transform,
                    weight = 175
                }],
                chance = 1f
            });*/

        //CycleManager.Instance.AddNewRoomTarget(RoomAssetMetaStorage.Instance.Get("Room_FieldTrip").value, CycleManager.Grass, true);
        var cyclereplacer = Resources.FindObjectsOfTypeAll<GameObject>().Last(x => x.name == "TreeCG").AddComponent<SeasonCyclerRender>();
        cyclereplacer.render = cyclereplacer.gameObject.GetComponentInChildren<MeshRenderer>();
        CycleManager.Instance.AddSelfCycleRender(cyclereplacer, new object[]
        {
            springTree,
            summerTree,
            autumnTree,
            new List<WeightedMaterial>(){ new()
            {
                selection = winterTree,
                weight = 100
            }, new()
            {
                selection = thing2,
                weight = 55
            }, new()
            {
                selection = smallsnowman,
                weight = 85
            }
            },
        });
        cyclereplacer = Resources.FindObjectsOfTypeAll<GameObject>().Last(x => x.name == "BananaTree").AddComponent<SeasonCyclerRender>();
        cyclereplacer.render = cyclereplacer.gameObject.GetComponentInChildren<MeshRenderer>();
        CycleManager.Instance.AddSelfCycleRender(cyclereplacer, new object[]
        {
            springTree,
            summerTree,
            autumnTree,
            winterTree,
        });
        cyclereplacer = Resources.FindObjectsOfTypeAll<GameObject>().Last(x => x.name == "AppleTree").AddComponent<SeasonCyclerRender>();
        cyclereplacer.render = cyclereplacer.gameObject.GetComponentInChildren<MeshRenderer>();
        CycleManager.Instance.AddSelfCycleRender(cyclereplacer, new object[]
        {
            springTree,
            summerTree,
            autumnTree,
            winterTree,
        });
        cyclereplacer = Resources.FindObjectsOfTypeAll<GameObject>().Last(x => x.name == "PineTree").AddComponent<SeasonCyclerRender>();
        cyclereplacer.render = cyclereplacer.GetComponent<RendererContainer>().renderers[0];
        CycleManager.Instance.AddSelfCycleRender(cyclereplacer, new object[]
        {
            springSunsetTree,
            summerSunsetTree,
            autumnSunsetTree,
            winterSunsetTree,
        });
        var barrier = Resources.FindObjectsOfTypeAll<GameObject>().Last(x => x.name == "CampBarrier");
        foreach (var side in barrier.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (!side.GetMaterial().name.ToString().StartsWith(summerSunsetBarrier.name.ToString())) continue;
            cyclereplacer = side.gameObject.AddComponent<SeasonCyclerRender>();
            cyclereplacer.render = side;
            CycleManager.Instance.AddSelfCycleRender(cyclereplacer, new object[]
            {
                springSunsetBarrier,
                summerSunsetBarrier,
                autumnSunsetBarrier,
                winterSunsetBarrier
            });
        }
        var campingHub = Resources.FindObjectsOfTypeAll<GameObject>().Last(x => x.name == "CampHubRoomFunction");
        cyclereplacer = campingHub.transform.Find("ObjectBase").Find("PineTree_Bully").gameObject.AddComponent<SeasonCyclerRender>();
        cyclereplacer.render = cyclereplacer.GetComponent<RendererContainer>().renderers[0];
        CycleManager.Instance.AddSelfCycleRender(cyclereplacer, new object[]
        {
            bullyTreeSpring,
            bullyTreeSummer,
            bullyTreeAutumn,
            bullyTreeWinter
        });

        // New Weather Feature?!
        assetMan.AddRange([AssetLoader.AudioClipFromMod(this, "AudioClip", "rain.wav")], ["RainAmbience"]);
        var weather = Resources.FindObjectsOfTypeAll<SunlightRoomFunction>().Last().gameObject.AddComponent<OutsideWeatherFunction>();
        weather.gravityFactor = 1f;
        weather.lifeTime = 3f;
        weather.rotationFactor = 0f;
        AudioSource source = weather.gameObject.AddComponent<AudioSource>();
        source.volume = 0f;
        source.playOnAwake = true;
        source.loop = true;
        source.clip = assetMan.Get<AudioClip>("RainAmbience");
        source.priority = 128;
        source.pitch = 1f;
        source.panStereo = 0f;
        source.spatialBlend = 0f;
        source.reverbZoneMix = 1f;
        source.dopplerLevel = 0f;
        source.spread = 0f;
        source.rolloffMode = AudioRolloffMode.Custom;
        source.maxDistance = 100f;
        weather.source = source;
        weather.gameObject.GetComponent<RoomFunctionContainer>().AddFunction(weather);

        GeneratorManagement.Register(this, GenerationModType.Base, (name, num, scene) =>
        {
            var meta = scene.GetMeta();
            if (meta == null) return;
            foreach (var level in scene.GetCustomLevelObjects())
            {
                if (level.IsModifiedByMod(Info)) continue;
                level.SetCustomModValue(Info, "AffectedByTimeCycle", (meta.tags.Contains("main") || meta.tags.Contains("endless") || meta.tags.Contains("pitstop")) && level.type != LevelType.Laboratory);
                level.MarkAsModifiedByMod(Info);
            }
        });
        GeneratorManagement.Register(this, GenerationModType.Override, (name, num, scene) =>
        {
            foreach (var level in scene.GetCustomLevelObjects())
            {
                if (level.IsModifiedByMod(Info) || (bool)level.GetCustomModValue(Info, "AffectedByTimeCycle") == false) continue;
                if (level.type == LevelType.Schoolhouse)
                    level.standardDarkLevel = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                if (name == "F1")
                    level.lightMode = LightMode.Cumulative;
                else if (name == "F2" || name == "F4")
                    level.lightMode = LightMode.Greatest;
                level.MarkAsModifiedByMod(Info);
            }
        });
        assetMan.Add("PlaygroundAmb", Resources.FindObjectsOfTypeAll<AudioClip>().Last(g => g.name == "PlaygroundAmbience"));
        //assetMan.Add("CricketsAmb", Resources.FindObjectsOfTypeAll<AudioClip>().Last(g => g.name == "Crickets"));
        assetMan.Add("Twilight", Resources.FindObjectsOfTypeAll<Cubemap>().Last(s => s.name.Contains("_Twilight")));
        assetMan.Add("DayStandard", Resources.FindObjectsOfTypeAll<Cubemap>().Last(s => s.name.Contains("_DayStandard")));
    }

    private void AddOptions(OptionsMenu __instance, CustomOptionsHandler handler) => handler.AddCategory<TimeSeasonCycleOptions>("Time & Season Cycle");
}

internal class TimeSeasonCycleOptions : CustomOptionsCategory
{
    public override void Build()
    {
        MenuToggle sh = CreateToggle("Southern", "Southern Hemisphere Mode", TimeSeasonCyclerPlugin.southern, new Vector2(75f, 0f), 199f);
        AddTooltip(sh, "Sets the current real season to the other side,\nusually for players who lives in the southern hemisphere.\n\nDefaults to \"false\"!");
        sh.GetComponentInChildren<StandardMenuButton>().OnPress.AddListener(() => TimeSeasonCyclerPlugin.southern = sh.Value);
        MenuToggle wh = CreateToggle("Eastern", "Eastern Hemisphere Mode", TimeSeasonCyclerPlugin.eastern, new Vector2(75f, -60f), 199f);
        AddTooltip(wh, "Sets the current real time to the other side,\nusually for players who lives in the eastern hemisphere.\n\nDefaults to \"false\"!");
        wh.GetComponentInChildren<StandardMenuButton>().OnPress.AddListener(() => TimeSeasonCyclerPlugin.eastern = wh.Value);
    }
}
