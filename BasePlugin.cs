﻿using BepInEx;
using HarmonyLib;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;
using UnityEngine.Profiling.Memory.Experimental;
using System;
using System.Security.Cryptography;
using System.IO;
using MTM101BaldAPI.AssetTools;

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

            //CycleManager.MIDIsongs.Add(AssetLoader.MidiFromFile(Path.Combine(AssetLoader.GetModPath(this), "MidiDB/school_winter.mid"), "school_winter"));

            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(NameManager), "Awake")]
    class PostPatch
    {
        public static bool did { get; private set; } = false;
        static void Prefix()
        {
            if (did) return;
            CycleManager.Grass.AddRange([
                AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D/Grass_Spring.png"),
                Resources.FindObjectsOfTypeAll<Texture2D>().ToList().Find(g => g.name == "Grass"),
                AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D/Grass_Autumn.png"),
                AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D/Grass_Winter.png")]);
            var autumnTree = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"));
            autumnTree.SetTexture("_MainTex", AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D/TreeCGAutumn.png"));
            var winterTree = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"));
            winterTree.SetTexture("_MainTex", AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D/TreeSnowed.png"));

            CycleManager.Tree.AddRange([
                Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"),
                Resources.FindObjectsOfTypeAll<Material>().ToList().Find(g => g.name == "TreeCG"),
                autumnTree,
                winterTree
            ]);

            CycleManager.nightCubemap = CycleManager.ThirdParty_EndlessFloors_CubemapFromTexture2D(AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D/DarkSky_OneImage.png"));

            foreach (LevelObject level in Resources.FindObjectsOfTypeAll<LevelObject>().ToList())
            {
                level.standardDarkLevel = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                if (level.name == "Main1")
                    level.lightMode = LightMode.Cumulative;
                else if (level.name == "Main2")
                    level.lightMode = LightMode.Greatest;
            }

            var thing = Material.Instantiate(Resources.FindObjectsOfTypeAll<Material>().ToList().Find(x => x.name == "DustTest"));
            thing.SetTexture("_BaseMap", AssetLoader.TextureFromMod(BasePlugin.plugin, "Texture2D/Droplet.png"));
            CycleManager.droplets = thing;

            did = true;
        }
    }
}
