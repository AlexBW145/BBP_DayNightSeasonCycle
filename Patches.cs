using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using ShadowGroveGames.RealWeatherAndTimeEvents.Scripts.OpenWeatherApi.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace BaldiPlus_Seasons;

[HarmonyPatch(typeof(GameInitializer), "Initialize")]
class ChangeSkybox
{
    static void Prefix() => CycleManager.Instance.RefreshTime();
    static void Postfix()
    {
        if (CoreGameManager.Instance != null && BaseGameManager.Instance.levelObject is CustomLevelGenerationParameters)
        {
            if ((bool?)((CustomLevelGenerationParameters)BaseGameManager.Instance.levelObject).GetCustomModValue(TimeSeasonCyclerPlugin.Instance.Info, "AffectedByTimeCycle") == true)
            if (TimeSeasonCyclerPlugin.eastern)
            {
                switch (CycleManager.Instance.time)
                {
                    case >= 12 and <= 17:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("NightSky"));
                        Shader.SetGlobalColor("_SkyboxColor", new Color(0.1254902f, 0.09803922f, 0.09803922f));
                        break;
                    case >= 18 and <= 23:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("Twilight"));
                        Shader.SetGlobalColor("_SkyboxColor", Color.white);
                        break;
                    case >= 0 and <= 5:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("DayStandard"));
                        Shader.SetGlobalColor("_SkyboxColor", Color.white);
                        break;
                    case >= 6 and <= 11:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("Twilight"));
                        Shader.SetGlobalColor("_SkyboxColor", new Color(0.4509804f, 0.372549f, 0.6156863f));
                        break;
                }
            }
            else
            {
                switch (CycleManager.Instance.time)
                {
                    case >= 0 and <= 5:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("NightSky"));
                        Shader.SetGlobalColor("_SkyboxColor", new Color(0.1254902f, 0.09803922f, 0.09803922f));
                        break;
                    case >= 6 and <= 11:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("Twilight"));
                        Shader.SetGlobalColor("_SkyboxColor", Color.white);
                        break;
                    case >= 12 and <= 17:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("DayStandard"));
                        Shader.SetGlobalColor("_SkyboxColor", Color.white);
                        break;
                    case >= 18 and <= 23:
                        Shader.SetGlobalTexture("_Skybox", TimeSeasonCyclerPlugin.assetMan.Get<Cubemap>("Twilight"));
                        Shader.SetGlobalColor("_SkyboxColor", new Color(0.4509804f, 0.372549f, 0.6156863f));
                        break;
                }
            }
        }
    }
}

[HarmonyPatch]
class SetVars
{
    [HarmonyPatch(typeof(LevelBuilder), nameof(LevelBuilder.StartGenerate)), HarmonyPostfix]
    static void Postfix(LevelBuilder __instance)
    {
        if (CycleManager.Instance.weather == Weather.WeatherMainType.Fog)
            __instance.Ec.AddFog(OutsideWeatherFunction.fogweather);
    }

    [HarmonyPatch(typeof(LevelBuilder), nameof(LevelBuilder.LoadRoom), [typeof(RoomAsset), typeof(IntVector2), typeof(IntVector2), typeof(Direction), typeof(bool), typeof(Texture2D), typeof(Texture2D), typeof(Texture2D)]), HarmonyTranspiler]
    static IEnumerable<CodeInstruction> StoreInAStupidWay(IEnumerable<CodeInstruction> instructions) => new CodeMatcher(instructions)
        .Start()
        .MatchForward(true,
        new(OpCodes.Ldloc_0),
        new(OpCodes.Ldarg_0),
        new(OpCodes.Ldfld, AccessTools.Field(typeof(LevelBuilder), "ec")),
        new(OpCodes.Stfld, AccessTools.Field(typeof(RoomController), nameof(RoomController.ec)))).ThrowIfInvalid("UH OH.").Advance(1)
        .InsertAndAdvance(new(OpCodes.Ldloc_0), new(OpCodes.Ldarg_1),
        Transpilers.EmitDelegate<Action<RoomController, RoomAsset>>((__result, asset) =>
        {
            if (CycleManager.targetRooms.Select(x => x.roomAsset).Contains(asset))
            {
                var vars = __result.gameObject.AddComponent<SeasonCycleRoomVars>();
                vars.roomAsset = asset;
            }
        }))
        .InstructionEnumeration();
}

// Old patch that crashes the generator, useless.
/*[HarmonyPatch(typeof(EnvironmentController), "GenerateLight")]
class AdjustLightProperties
{
    static bool Prefix(EnvironmentController __instance, Cell tile, Color color, int strength, ref LightController[,] ___lightMap, ref IntVector2 ____lightPos) 
    {
        Color outsideColo = color;
        SeasonalRoom room = CycleManager.targetRooms.Find(x => x.roomAsset.name.ToLower().Contains(tile.room.gameObject.name.ToLower()));
        if (room != null & room.affectsLighting)
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
}*/

// Tried to use during 0.3.8 ol' sakes, useless.
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

// Not part of the reworked, useless.
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

[HarmonyPatch(typeof(RoomController), nameof(RoomController.GenerateTextureAtlas), [typeof(int)])]
class ChangeTextureAndLighting
{
    static void Prefix(RoomController __instance)
    {
        if (__instance.IsSeasonalRoom())
        {
            SeasonalRoom seasonAsset = __instance.GetSeasonalRoom();
            switch (CycleManager.Instance.seasons)
            {
                case Seasons.Spring:
                    __instance.florTex = seasonAsset.floorReplacements[0];
                    break;
                case Seasons.Summer:
                    __instance.florTex = seasonAsset.floorReplacements[1];
                    break;
                case Seasons.Autumn:
                    __instance.florTex = seasonAsset.floorReplacements[2];
                    break;
                case Seasons.Winter:
                    __instance.florTex = seasonAsset.floorReplacements[3];
                    break;
            }
            if (seasonAsset.targetsWallTexture)
            {
                switch (CycleManager.Instance.seasons)
                {
                    case Seasons.Spring:
                        __instance.wallTex = seasonAsset.wallReplacements[0];
                        break;
                    case Seasons.Summer:
                        __instance.wallTex = seasonAsset.wallReplacements[1];
                        break;
                    case Seasons.Autumn:
                        __instance.wallTex = seasonAsset.wallReplacements[2];
                        break;
                    case Seasons.Winter:
                        __instance.wallTex = seasonAsset.wallReplacements[3];
                        break;
                }
            }
            if (seasonAsset.affectsLighting)
            {
                foreach (var light in __instance.cells)
                {
                    if (TimeSeasonCyclerPlugin.eastern)
                    {
                        switch (CycleManager.Instance.time)
                        {
                            case >= 12 and <= 17:
                                light.lightColor = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                                light.SetLight(false);
                                break;
                            case >= 6 and <= 11:
                                light.lightColor = new Color(0.4509804f, 0.372549f, 0.6156863f);
                                light.SetLight(true);
                                break;
                            case (>= 0 and <= 5) or (>= 18 and <= 23):
                                light.lightColor = Color.white;
                                light.SetLight(true);
                                break;
                        }
                    }
                    else
                    {
                        switch (CycleManager.Instance.time)
                        {
                            case >= 0 and <= 5:
                                light.lightColor = new Color(0.1254902f, 0.09803922f, 0.09803922f);
                                light.SetLight(false);
                                break;
                            case >= 18 and <= 23:
                                light.lightColor = new Color(0.4509804f, 0.372549f, 0.6156863f);
                                light.SetLight(true);
                                break;
                            case (>= 6 and <= 11) or (>= 12 and <= 17):
                                light.lightColor = Color.white;
                                light.SetLight(true);
                                break;
                        }
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(RoomFunction), nameof(RoomFunction.Initialize))]
class ChangeAmbience // Nvm, I'm changing it completely to all.
{
    static void Postfix(RoomFunction __instance)
    {
        if (__instance is AmbienceRoomFunction)
        {
            AmbienceRoomFunction functionObject = __instance as AmbienceRoomFunction;
            AudioSource source = functionObject.ReflectionGetVariable("source") as AudioSource;
            if (source == null) // Someone mentioned this, and it bugged me out about premade floors.
                return;
            var playground = TimeSeasonCyclerPlugin.assetMan.Get<AudioClip>("PlaygroundAmb");
            //var crickets = BasePlugin.assetMan.Get<AudioClip>("CricketsAmb");
            if (source.clip == playground)
            {
                if (TimeSeasonCyclerPlugin.eastern)
                {
                    switch (CycleManager.Instance.time)
                    {
                        case (>= 6 and <= 11) or (>= 12 and <= 17):
                            source.clip = null;
                            break;
                        case (>= 0 and <= 5) or (>= 18 and <= 23):
                            source.clip = playground;
                            break;
                    }
                }
                else
                {
                    switch (CycleManager.Instance.time)
                    {
                        case (>= 0 and <= 5) or (>= 18 and <= 23):
                            source.clip = null;
                            break;
                        case (>= 6 and <= 11) or (>= 12 and <= 17):
                            source.clip = playground;
                            break;
                    }
                }
                source.Play();
            }
        }
    }
}

// Borrowing BBT's room function, useless.
/*[HarmonyPatch(typeof(SunlightRoomFunction), "Initialize")]
class MakeItRain
{
    static void Postfix(SunlightRoomFunction __instance, RoomController room)
    {
        if (true)
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
            //shape.position = new Vector3(0f, 0f, 10f);
            //shape.scale = new Vector3(10f, 10f, 1f);
            shape.randomDirectionAmount = 0.5f;

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

            AmbienceRoomFunction ambience = __instance.gameObject.AddComponent<AmbienceRoomFunction>();
            AudioSource source = __instance.gameObject.AddComponent<AudioSource>();
            source.volume = 0f;
            source.playOnAwake = true;
            source.loop = true;
            source.clip = BasePlugin.assetMan.Get<AudioClip>("RainAmbience");
            source.priority = 128;
            source.pitch = 1f;
            source.panStereo = 0f;
            source.spatialBlend = 0f;
            source.reverbZoneMix = 1f;
            source.dopplerLevel = 0f;
            source.spread = 0f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.maxDistance = 100f;
            ambience.ReflectionSetVariable("source", source);
            ambience.Initialize(__instance.Room);
            __instance.Room.functions.AddFunction(ambience);
            source.Play();
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
                ___fogColor = Color.white;
                break;
            case Seasons.Autumn:
                ___fogColor = new Color(1f, 0.759434f, 0.759434f);
                break;
            case Seasons.Winter:
                ___fogColor = new Color(0.6966447f, 0.7788854f, 0.9528302f);
                break;
        }
    }
}
