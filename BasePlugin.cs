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

namespace BaldiPlus_Seasons
{
    [BepInPlugin("alexbw145.baldiplus.seasons", "Day & Season Cycle", "1.0.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    [BepInProcess("BALDI.exe")]
    public class BasePlugin : BaseUnityPlugin
    {
        public static BasePlugin plugin;
        private void Awake()
        {
            Harmony harmony = new Harmony("alexbw145.baldiplus.seasons");
            new GameObject("Cycle Manager", typeof(CycleManager));
            plugin = this;

            //CycleManager.MIDIsongs.Add(AssetLoader.MidiFromFile(Path.Combine(AssetLoader.GetModPath(this), "MidiDB", "school_winter.mid"), "school_winter"));
            LoadingEvents.RegisterOnAssetsLoaded(PreLoad, false);
            GeneratorManagement.Register(this, GenerationModType.Override, (name, num, ld) =>
            {
                switch (name)
                {
                    case "F1" or "F2" or "F3":
                        ld.standardDarkLevel = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                        if (name == "F1")
                            ld.lightMode = LightMode.Cumulative;
                        else if (name == "F2")
                            ld.lightMode = LightMode.Greatest;
                        break;
                }
            });

            harmony.PatchAll();
        }

        private void PreLoad()
        {
            CycleManager.Grass.AddRange([
                AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "Grass_Spring.png"),
                Resources.FindObjectsOfTypeAll<Texture2D>().ToList().Find(g => g.name == "Grass"),
                AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "Grass_Autumn.png"),
                AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "Grass_Winter.png")]);
            var autumnTree = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"));
            autumnTree.SetTexture("_MainTex", AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "TreeCGAutumn.png"));
            var winterTree = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"));
            winterTree.SetTexture("_MainTex", AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "TreeSnowed.png"));

            CycleManager.Tree.AddRange([
                Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"),
                Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"),
                autumnTree,
                winterTree
            ]);

#if DEBUG
            //CycleManager.nightCubemap = CycleManager.ThirdParty_EndlessFloors_CubemapFromTexture2D(AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "DarkSky_OneImage.png"));
#endif

            var thing = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "DustTest"));
            thing.SetTexture("_BaseMap", AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "Droplet.png"));
            CycleManager.droplets = thing;

            var thing2 = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "TreeCG"));
            thing2.SetTexture("_MainTex", AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D", "Snowman.png"));
            CycleManager.snowman = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(x => x.name == "TreeCG"));
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

            // Just separating as "asset" because of the target texture...
            RoomAsset asset = Resources.FindObjectsOfTypeAll<RoomAsset>().ToList().Find(x => x.name.Contains("Playground"));
            CycleManager.Instance.AddNewRoomTarget(asset, CycleManager.Grass, true);
            asset = Resources.FindObjectsOfTypeAll<RoomAsset>().ToList().Find(x => x.name.Contains("FieldTrip"));
            CycleManager.Instance.AddNewRoomTarget(asset, CycleManager.Grass, true);
        }
    }
}
