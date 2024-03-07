using HarmonyLib;
using MonoMod.Cil;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace BaldiPlus_Seasons
{
    [HarmonyPatch(typeof(GameInitializer), "Initialize")]
    class ChangeSkybox
    {
        static void Prefix()
        {
            CycleManager.ec = null;
            var tree = Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(g => g.name == "TreeCG");
            var treeApple = Resources.FindObjectsOfTypeAll<GameObject>().ToList().Find(g => g.name == "AppleTree");
            switch (CycleManager.Instance.seasons)
            {
                case Seasons.Spring:
                    tree.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[0];
                    tree.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[0];
                    break;
                case Seasons.Summer:
                    tree.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[1];
                    treeApple.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[1];
                    break;
                case Seasons.Autumn:
                    tree.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[2];
                    treeApple.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[2];
                    break;
                case Seasons.Winter:
                    tree.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[3];
                    treeApple.GetComponentInChildren<MeshRenderer>().material = CycleManager.Tree[3];
                    break;
            }

            CycleManager.Instance.RefreshTime();

            if (Singleton<CoreGameManager>.Instance != null)
            {
                SceneObject ___sceneObject = Singleton<CoreGameManager>.Instance.sceneObject;
                switch (CycleManager.Instance.time)
                {
                    case >= 0 and <= 5:
                        ___sceneObject.skybox = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().Find(s => s.name.Contains("_Twilight"));
                        ___sceneObject.skyboxColor = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                        break;
                    case >= 6 and <= 11:
                        ___sceneObject.skybox = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().Find(s => s.name.Contains("_Twilight"));
                        ___sceneObject.skyboxColor = Color.white;
                        break;
                    case >= 12 and <= 17:
                        ___sceneObject.skybox = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().Find(s => s.name.Contains("_DayStandard"));
                        ___sceneObject.skyboxColor = Color.white;
                        break;
                    case >= 18 and <= 23:
                        ___sceneObject.skybox = Resources.FindObjectsOfTypeAll<Cubemap>().ToList().Find(s => s.name.Contains("_Twilight"));
                        ___sceneObject.skyboxColor = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LevelGenerator), "StartGenerate")]
    class SetVars
    {
        static void Prefix(LevelGenerator __instance)
        {
            CycleManager.ec = __instance.Ec;
        }
    }

    [HarmonyPatch(typeof(EnvironmentController), "GenerateLight")]
    class AdjustLightProperties
    {
        static bool Prefix(EnvironmentController __instance, Cell tile, Color color, int strength, ref LightController[,] ___lightMap, ref IntVector2 ____lightPos) 
        {
            Color outsideColo = color;
            if (tile.room.gameObject.name.ToLower().Contains("playground"))
                switch (CycleManager.Instance.time)
                {
                    case (>= 0 and <= 5) or (>= 18 and <= 23):
                        outsideColo = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                        break;
                    case (>= 6 and <= 11) or (>= 12 and <= 17):
                        outsideColo = Color.white;
                        break;
                }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            tile.hasLight = true;
            tile.lightOn = true;
            tile.lightStrength = strength;
            tile.lightColor = outsideColo;
            for (int i = -tile.lightStrength; i <= tile.lightStrength; i++)
            {
                for (int j = -tile.lightStrength; j <= tile.lightStrength; j++)
                {
                    ____lightPos.x = i + tile.position.x;
                    ____lightPos.z = j + tile.position.z;
                    if (Mathf.Abs(tile.position.x - ____lightPos.x) + Mathf.Abs(tile.position.z - ____lightPos.z) < tile.lightStrength && __instance.ContainsCoordinates(____lightPos) && !__instance.cells[____lightPos.x, ____lightPos.z].Null)
                    {
                        int num = __instance.NavigableDistance(tile, __instance.cells[____lightPos.x, ____lightPos.z], PathType.Const) - 1;
                        if (num > -1 && num <= tile.lightStrength)
                        {
                            ___lightMap[____lightPos.x, ____lightPos.z].AddSource(tile, num);
                            Singleton<CoreGameManager>.Instance.UpdateLighting(___lightMap[____lightPos.x, ____lightPos.z].Color, ____lightPos);
                        }
                    }
                }
            }

            if (!tile.permanentLight)
            {
                __instance.lights.Add(tile);
            }

            stopwatch.Stop();
            return false;
        }
    }

    /*[HarmonyPatch(typeof(LevelGenerator), "Generate", MethodType.Enumerator)]
    class AdjustLightProperties
    {
        static FieldInfo f_someField = AccessTools.Field(typeof(EnvironmentController), nameof(EnvironmentController.GenerateLight));

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            foreach (var instruction in instructions)
            {
                if (instruction.StoresField(f_someField))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, () =>
                    {

                    });
                    found = true;
                }
                yield return instruction;
            }
            if (found is false)
                Debug.LogError("Failed to change LevelGenerator's Generate()");
        }
    }*/

    /*[HarmonyPatch(typeof(PlaygroundSpecialRoom))]
    class PlaygroundPatches // (0.3 script, useless)
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPrefix]
        static void ChangeTextures(PlaygroundSpecialRoom __instance, ref RoomController ___room)
        {
            switch (CycleManager.Instance.seasons)
            {
                case Seasons.Spring:
                    ___room.floorTex = CycleManager.Grass[0];
                    break;
                case Seasons.Summer:
                    ___room.floorTex = CycleManager.Grass[1];
                    break;
                case Seasons.Autumn:
                    ___room.floorTex = CycleManager.Grass[2];
                    break;
                case Seasons.Winter:
                    ___room.floorTex = CycleManager.Grass[3];
                    break;
            }
            //ParticleSystem snow = __instance.GetComponentsInChildren<MeshRenderer>().ToList().Find(m => m.name == "Quad (4)").gameObject.AddComponent<ParticleSystem>(); // this is kinda hard to make...
        }
        [HarmonyPatch("AfterUpdatingTiles")]
        [HarmonyPostfix]
        static void ChangeLight(PlaygroundSpecialRoom __instance)
        {
            Color color = new Color();
            SceneObject ___sceneObject = Singleton<CoreGameManager>.Instance.sceneObject;
            switch (CycleManager.Instance.time)
            {
                case >= 0 and <= 5:
                    color = new Color(0.3f, 0.3f, 0.3f);
                    break;
                case >= 6 and <= 11:
                    color = Color.white;
                    break;
                case >= 12 and <= 17:
                    color = Color.gray;
                    break;
                case >= 18 and <= 23:
                    color = Color.gray;
                    break;
            }
            for (int i = 0; i < __instance.Room.TileCount; i++)
            {
                __instance.Room.TileAtIndex(i).lightColor = color;
            }
        }
    }*/

    [HarmonyPatch(typeof(RoomController), "GenerateTextureAtlas")]
    class ChangeTextureAndLighting
    {
        static void Prefix(RoomController __instance)
        {
            if (__instance.florTex == CycleManager.Grass[1])
            {
                switch (CycleManager.Instance.seasons)
                {
                    case Seasons.Spring:
                        __instance.florTex = CycleManager.Grass[0];
                        break;
                    case Seasons.Summer:
                        __instance.florTex = CycleManager.Grass[1];
                        break;
                    case Seasons.Autumn:
                        __instance.florTex = CycleManager.Grass[2];
                        break;
                    case Seasons.Winter:
                        __instance.florTex = CycleManager.Grass[3];
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RoomController), "GenerateTextureAtlas")]
    class ChangeAmbience // Stupid thing to do with the ambience class (while it's applied to two of the special rooms.)
    {
        static void Postfix(RoomController __instance)
        {
            if (__instance.gameObject.name.ToLower().Contains("playground"))
            {
                AudioSource source = __instance.gameObject.GetComponentInChildren<AudioSource>();
                if (source.clip == Resources.FindObjectsOfTypeAll<AudioClip>().ToList().Find(g => g.name == "PlaygroundAmbience") || source.clip == Resources.FindObjectsOfTypeAll<AudioClip>().ToList().Find(g => g.name == "Crickets"))
                {
                    switch (CycleManager.Instance.time)
                    {
                        case (>= 0 and <= 5) or (>= 18 and <= 23):
                            source.clip = Resources.FindObjectsOfTypeAll<AudioClip>().ToList().Find(g => g.name == "Crickets");
                            break;
                        case (>= 6 and <= 11) or (>= 12 and <= 17):
                            source.clip = Resources.FindObjectsOfTypeAll<AudioClip>().ToList().Find(g => g.name == "PlaygroundAmbience");
                            break;
                    }
                    source.Play();
                }
            }
        }
    }

    // Reserved, useless.
    /*[HarmonyPatch(typeof(RoomController), "GenerateTextureAtlas")]
    class MakeItRain
    {
        static void Postfix(RoomController __instance)
        {
            if (__instance.gameObject.name.ToLower().Contains("playground"))
            {
                ParticleSystem particle = __instance.GetComponentsInChildren<MeshRenderer>().ToList().Find(r => r.name == "Quad (4)").gameObject.AddComponent<ParticleSystem>();
                var main = particle.main;

                main.duration = 1f;
                main.startLifetime = 2.2f;
                main.startSpeed = 5f;
                main.gravityModifier = 1f;
                main.simulationSpeed = 2f;

                var shape = particle.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.position = new Vector3(0f, 0f, 10f);
                shape.scale = new Vector3(10f,10f,1f);

                var velocity = particle.velocityOverLifetime;
                velocity.enabled = true;
                velocity.z = -5f;
                velocity.speedModifier = 5f;

                var render = __instance.GetComponentsInChildren<MeshRenderer>().ToList().Find(r => r.name == "Quad (4)").gameObject.GetComponent<ParticleSystemRenderer>();
                render.renderMode = ParticleSystemRenderMode.Billboard;
                render.material = CycleManager.droplets;
                render.sortMode = ParticleSystemSortMode.None;
                render.maxParticleSize = 0.01f;
                render.alignment = ParticleSystemRenderSpace.Facing;

                particle.Play();
            }
        }
    }*/

    [HarmonyPatch(typeof(FogEvent), "Begin")]
    class FogColorChange
    {
        static void Prefix(ref Color ___fogColor)
        {
            switch (CycleManager.Instance.seasons)
            {
                case Seasons.Spring:
                    ___fogColor = Color.gray;
                    break;
                case Seasons.Summer:
                    ___fogColor = new Color(1f, 0.9883563f, 0.7607843f);
                    break;
                case Seasons.Autumn:
                    ___fogColor = new Color(1f, 0.759434f, 0.759434f);
                    break;
                case Seasons.Winter:
                    ___fogColor = Color.white;
                    break;
            }
        }
    }
}
