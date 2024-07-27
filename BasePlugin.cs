using BepInEx;
using HarmonyLib;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;
using UnityEngine.Profiling.Memory.Experimental;
using System;
using System.Security.Cryptography;
using System.IO;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.SaveSystem;
using UnityEngine.Events;

namespace BaldiPlus_Seasons
{
    [BepInPlugin("alexbw145.baldiplus.seasons", "Day & Season Cycle", "1.1.2.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", MTM101BaldiDevAPI.VersionNumber)]
    [BepInProcess("BALDI.exe")]
    public class BasePlugin : BaseUnityPlugin
    {
        public static BasePlugin plugin { get; private set; }
        public static bool southern = false;
        public static bool eastern = false;
        public static AssetManager assetMan = new AssetManager();

        private void Awake()
        {
            Harmony harmony = new Harmony("alexbw145.baldiplus.seasons");
            new GameObject("Cycle Manager", typeof(CycleManager));
            plugin = this;
            harmony.PatchAllConditionals();

            //CycleManager.MIDIsongs.Add(AssetLoader.MidiFromFile(Path.Combine(AssetLoader.GetModPath(this), "MidiDB", "school_winter.mid"), "school_winter"));
            LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad, false);

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
                Resources.FindObjectsOfTypeAll<Texture2D>().ToList().Find(g => g.name == "Grass"),
                AssetLoader.TextureFromMod(this, "Texture2D", "Grass_Autumn.png"),
                AssetLoader.TextureFromMod(this, "Texture2D", "Grass_Winter.png")]);
            var autumnTree = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"));
            autumnTree.SetTexture("_MainTex", AssetLoader.TextureFromMod(this, "Texture2D", "TreeCGAutumn.png"));
            var winterTree = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"));
            winterTree.SetTexture("_MainTex", AssetLoader.TextureFromMod(this, "Texture2D", "TreeSnowed.png"));
            var springTree = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"));
            springTree.SetTexture("_MainTex", AssetLoader.TextureFromMod(this, "Texture2D", "TreeCGSpring.png"));

            assetMan.Add<Material[]>("Tree", [
                springTree,
                Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"),
                autumnTree,
                winterTree]);
            assetMan.Add<Cubemap>("NightSky", AssetLoader.CubemapFromMod(this, "Texture2D", "Cubemap_Night.png"));

            var thing = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "DustTest"));
            thing.SetTexture("_BaseMap", AssetLoader.TextureFromMod(this, "Texture2D", "Droplet.png"));
            CycleManager.droplets = thing;

            var thing2 = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "TreeCG"));
            thing2.SetTexture("_MainTex", AssetLoader.TextureFromMod(this, "Texture2D", "Snowman.png"));
            CycleManager.snowman = Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TreeCG"));
            CycleManager.snowman.name = "Snowman";
            CycleManager.snowman.GetComponentInChildren<MeshRenderer>().material = thing2;
            CycleManager.snowman.GetComponentInChildren<MeshRenderer>().transform.localScale = new Vector3(10, 10, 1);
            CycleManager.snowman.GetComponentInChildren<MeshRenderer>().transform.position = new Vector3(0, 5, 0);
            MonoBehaviour.DontDestroyOnLoad(CycleManager.snowman);
            CycleManager.snowman.SetActive(false);

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

            // Adding deez...
            foreach (var playground in RoomAssetMetaStorage.Instance.FindAll(x => x.value.name.ToLower().Contains("Playground".ToLower())))
                CycleManager.Instance.AddNewRoomTarget(playground.value, assetMan.Get<Texture2D[]>("Grass").ToList(), true);
            //CycleManager.Instance.AddNewRoomTarget(RoomAssetMetaStorage.Instance.Get("Room_FieldTrip").value, CycleManager.Grass, true);

            GeneratorManagement.Register(this, GenerationModType.Override, (name, num, ld) =>
            {
                switch (name)
                {
                    default:
                        ld.standardDarkLevel = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                        if (name == "F1")
                            ld.lightMode = LightMode.Cumulative;
                        else if (name == "F2")
                            ld.lightMode = LightMode.Greatest;
                        break;
                    case "FOX" or "B1":
                        break;
                }
            });
        }

        private void AddOptions(OptionsMenu __instance)
        {
            GameObject ob = CustomOptionsCore.CreateNewCategory(__instance, "Time & Season Cycle");
            MenuToggle sh = CustomOptionsCore.CreateToggleButton(__instance, new Vector2(75f, 0f), "Southern Hemisphere Mode", southern, "Sets the current real season to the other side,\nusually for players who lives in the southern hemisphere.\n\nDefaults to \"false\"!");
            // Yes, he did not focus on preventing a two step way...
            sh.GetComponentInChildren<StandardMenuButton>().OnPress.AddListener(() =>
            {
                southern = sh.Value;
            });
            MenuToggle wh = CustomOptionsCore.CreateToggleButton(__instance, new Vector2(75f, -60f), "Eastern Hemisphere Mode", eastern, "Sets the current real time to the other side,\nusually for players who lives in the eastern hemisphere.\n\nDefaults to \"false\"!");
            wh.GetComponentInChildren<StandardMenuButton>().OnPress.AddListener(() =>
            {
                eastern = wh.Value;
            });
            sh.transform.SetParent(ob.transform, false);
            wh.transform.SetParent(ob.transform, false);
        }
    }
}
