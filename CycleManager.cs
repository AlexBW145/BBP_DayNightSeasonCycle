using MTM101BaldAPI.Reflection;
using ShadowGroveGames.RealWeatherAndTimeEvents.Scripts;
using ShadowGroveGames.RealWeatherAndTimeEvents.Scripts.Control;
using ShadowGroveGames.RealWeatherAndTimeEvents.Scripts.OpenWeatherApi.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BaldiPlus_Seasons;

/*public enum Weather
{
    Clear,
    Cloudy,
    Rain,
    Storm,
    Windy
}*/

public enum Seasons
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class CycleManager : MonoBehaviour
{
#if DEBUG
    const bool debug = true;
#endif

    public static CycleManager Instance { get; private set; }
    internal System.Random visualRNG = new System.Random();
    private void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
        gameObject.SetActive(false);
        var api = gameObject.AddComponent<RealWeatherAndTimeEventsScript>();
        api.ReflectionSetVariable("_apiKey", UnityCipher.RijndaelEncryption.Decrypt("s0MkHUJTQNSnLP10iBlMgHIHvOT4M/ERfXfxvwn+/QR23o5N+fpYxoAYQmQPtlLmoQd7530+XK3bR7WFl5rxyIKMO0TZ5jehRJlNxKGuuXqXndcgvxR0X4zbRqtTDwVaqbpnVXH/FiCOxoKNazPNGlkTmdwYd3YaJiVV90tq8KQ=", "TSC_BALDIMOD"));
        api.ReflectionSetVariable("_events", new RealWeatherAndTimeEventsEvents());
        api.ReflectionSetVariable("_cityName", TimeSeasonCyclerPlugin.Location.Value);
        gameObject.SetActive(true);

        RefreshTime();
    }

    internal void RefreshTime()
    {
        if (TimeSeasonCyclerPlugin.southern)
        {
            switch (DateTime.Today.Month)
            {
                case 1 or 2 or 12:
                    seasons = Seasons.Summer;
                    break;
                case 3 or 4 or 5:
                    seasons = Seasons.Autumn;
                    break;
                case 6 or 7 or 8:
                    seasons = Seasons.Winter;
                    break;
                case 9 or 10 or 11:
                    seasons = Seasons.Spring;
                    break;
            }
        }
        else
        {
            switch (DateTime.Today.Month)
            {
                case 1 or 2 or 12:
                    seasons = Seasons.Winter;
                    break;
                case 3 or 4 or 5:
                    seasons = Seasons.Spring;
                    break;
                case 6 or 7 or 8:
                    seasons = Seasons.Summer;
                    break;
                case 9 or 10 or 11:
                    seasons = Seasons.Autumn;
                    break;
            }
        }

        time = DateTime.Now.Hour;

#if DEBUG
        if (debug)
        {
            //seasons = Seasons.Winter;
            //time = 1;
        }
#endif
    }

    public Seasons seasons { get; private set; }
    public Weather.WeatherMainType weather =>
#if DEBUG
        Weather.WeatherMainType.Snow;
#else
        RealWeatherAndTimeEventsScript.Instance?.WeatherInformation != null ? RealWeatherAndTimeEventsScript.Instance.WeatherInformation.Weather.Main : Weather.WeatherMainType.Unknown;
#endif
    public int time { get; private set; }

    public static readonly HashSet<SeasonalRoom> targetRooms = new HashSet<SeasonalRoom>();
    /// <summary>
    /// Makes the <see cref="RoomAsset"/> have seasonal variants
    /// </summary>
    /// <param name="rmAsset"></param>
    /// <param name="floorReplaces"></param>
    /// <param name="light"></param>
    /// <param name="targetWalls"></param>
    /// <param name="wallReplaces"></param>
    /// <returns></returns>
    public SeasonalRoom AddNewRoomTarget(RoomAsset rmAsset, List<Texture2D> floorReplaces, bool light = false, bool targetWalls = false, List<Texture2D> wallReplaces = null)
    {
        try
        {
            SeasonalRoom room = new SeasonalRoom();
            room.roomAsset = rmAsset;
            //room.targetFloorTexture = targetFloorTexture;
            room.affectsLighting = light;
            room.floorReplacements = floorReplaces;
            if (targetWalls) {
                room.targetsWallTexture = targetWalls;
                room.wallReplacements = wallReplaces;
            }

            targetRooms.Add(room);
            return room;
        }
        catch (Exception e)
        {
            Debug.LogError("  ____________  _________   _____ _________   _____ ____  _   __   ________  __________    ______\r\n /_  __/  _/  |/  / ____/  / ___// ____/   | / ___// __ \\/ | / /  / ____/\\ \\/ / ____/ /   / ____/\r\n  / /  / // /|_/ / __/     \\__ \\/ __/ / /| | \\__ \\/ / / /  |/ /  / /      \\  / /   / /   / __/   \r\n / / _/ // /  / / /___    ___/ / /___/ ___ |___/ / /_/ / /|  /  / /___    / / /___/ /___/ /___   \r\n/_/ /___/_/  /_/_____/   /____/_____/_/  |_/____/\\____/_/ |_/   \\____/   /_/\\____/_____/_____/   \r\n                                                                                                 \n\nDid you get an error? Oops! Here's what you've made!");
            if (rmAsset == null)
                Debug.LogError("rmAsset is null, you can grab vanilla ones by Resources.FindObjectsOfTypeAll<RoomAsset>().ToList().Find(x => x.name == \"name\") or you can use your own!");
            /*else if (targetFloorTexture == null)
                Debug.LogError("Did you not even do targetFloorTexture correctly?? Lame!");*/
            else if (floorReplaces == null)
                Debug.LogError("floorReplaces uses a List<Texture2D>, not a Texture2D[]! Use the .ToList() for that!");

            Debug.LogError("\n" + e);
        }

        return null;
    }
    /// <summary>
    /// Adds in season cycler variants to the renderer itself.
    /// </summary>
    /// <param name="render">The <see cref="Renderer"/> object that has the <see cref="SeasonCyclerRender"/> component</param>
    /// <param name="stuff">Array of objects that will replace the visual appearance of the <see cref="Renderer"/> object</param>
    /// <returns>This <see cref="Renderer"/> object</returns>
    public SeasonCyclerRender AddSelfCycleRender(SeasonCyclerRender render, object[] stuff)
    {
        for (int i = 0; i < 4; i++)
        {
            if (stuff[i] is Sprite)
                render.seasonReplacers[i] = new SeasonCycleReplacer() { sprite = stuff[i] as Sprite };
            else if (stuff[i] is List<WeightedSprite>)
                render.seasonReplacers[i] = new SeasonCycleReplacer() { weightedSprites = stuff[i] as List<WeightedSprite> };
            else if (stuff[i] is Material)
                render.seasonReplacers[i] = new SeasonCycleReplacer() { material = stuff[i] as Material };
            else if (stuff[i] is List<WeightedMaterial>)
                render.seasonReplacers[i] = new SeasonCycleReplacer() { weightedMaterials = stuff[i] as List<WeightedMaterial> };
            else if (stuff[i] is Material[])
                render.seasonReplacers[i] = new SeasonCycleReplacer() { materialarray = stuff[i] as Material[] };
        }
        return render;
    }
}
[Serializable]
public class SeasonalRoom
{
    public RoomAsset roomAsset;
    public List<Texture2D> floorReplacements = new List<Texture2D>(), wallReplacements = new List<Texture2D>();

    public bool affectsLighting = false, targetsWallTexture = false;
}

internal class SeasonCycleRoomVars : MonoBehaviour
{
    public RoomAsset roomAsset;
}

[Serializable]
public class SeasonCycleReplacer
{
    public Sprite sprite;
    public List<WeightedSprite> weightedSprites;
    public Material material;
    public List<WeightedMaterial> weightedMaterials;
    public Material[] materialarray;
}

public class SeasonCyclerRender : MonoBehaviour
{
    // This is stupid
    // Before the serialization patcher was added in, this was a alternative solution.
    //internal static Dictionary<string, Dictionary<Seasons, object>> SeasonReplacers = new Dictionary<string, Dictionary<Seasons, object>>();
    [SerializeField] internal SeasonCycleReplacer[] seasonReplacers = new SeasonCycleReplacer[4];
    public Renderer render;


    internal void SetRender()
    {
        var replacer = seasonReplacers[(int)CycleManager.Instance.seasons];
        if ((replacer.weightedMaterials?.Count ?? 0) != 0 || (replacer.weightedSprites?.Count ?? 0) != 0)
        {
            var rng = CycleManager.Instance.visualRNG;
            rng.Next(1, 99);
            if ((replacer.weightedMaterials?.Count ?? 0) != 0)
                replacer.material = WeightedMaterial.ControlledRandomSelection(replacer.weightedMaterials.ToArray(), rng);
            else if ((replacer.weightedSprites?.Count ?? 0) != 0)
                replacer.sprite = WeightedSprite.ControlledRandomSelection(replacer.weightedSprites.ToArray(), rng);
        }
        if (render is SpriteRenderer && replacer.sprite != null)
        {
            var spriteRender = render as SpriteRenderer;
            spriteRender.sprite = replacer.sprite;
        }
        else if (render is MeshRenderer && replacer.material != null)
        {
            var meshRender = render as MeshRenderer;
            meshRender.SetMaterial(replacer.material);
        }
        else if (render is MeshRenderer && replacer.materialarray != null)
        {
            var meshRender = render as MeshRenderer;
            meshRender.SetMaterialArray(replacer.materialarray);
        }
    }

    void Start()
    {
        if (CycleManager.Instance != null && render != null)
            SetRender();
    }
}

public static class CycleExtensions
{
    public static bool IsSeasonalRoom(this RoomController me)
    {
        var component = me.GetComponent<SeasonCycleRoomVars>();
        if (component != null && CycleManager.targetRooms.Select(x => x.roomAsset).Contains(component.roomAsset))
            return true;
        return false;
    }

    public static SeasonalRoom GetSeasonalRoom(this RoomController me)
    {
        var component = me.GetComponent<SeasonCycleRoomVars>();
        if (component != null && CycleManager.targetRooms.Select(x => x.roomAsset).Contains(component.roomAsset))
            return CycleManager.targetRooms.First(x => x.roomAsset == component.roomAsset);
        return null;
    }
}

public class OutsideWeatherFunction : RoomFunction // Taken from BBT, well, because, oh... I showed PixelGuy this source code for the snowy playground.
{
    public Transform[] planeBounderies { get; private set; }

    [SerializeField]
    internal float gravityFactor = 0.25f, initialFallingSpeed = 3f, emissionFactor = 60f, rotationFactor = 0.1f, lifeTime = 6.5f, yOffset = 65f, sSpeed = 5f;

    [SerializeField]
    internal Vector2 minMaxSpeedX = new(-1.5f, 1.5f), minMaxSpeedZ = new(-1.5f, 1.5f);

    [SerializeField]
    internal AudioSource source;

    internal static Fog fogweather = new Fog()
    {
        color = (Color.white * 0.8f) + new Color(0f, 0f, 0f, 1f),
        startDist = 100f,
        maxDist = 1000f,
        strength = 1f,
        priority = 0,
    };

    public override void OnPlayerEnter(PlayerManager player)
    {
        base.OnPlayerEnter(player);
        source.volume = 1f;
    }

    public override void OnPlayerExit(PlayerManager player)
    {
        base.OnPlayerExit(player);
        source.volume = 0f;
    }

    public override void OnGenerationFinished()
    {
        bool cloudy = false;
        if (CycleManager.Instance.weather == Weather.WeatherMainType.Clouds)
            cloudy = true;
        Material dropling = null;
        switch (CycleManager.Instance.weather)
        {
            default:
                source.Stop();
                return;
            case Weather.WeatherMainType.Rain:
                cloudy = true;
                gravityFactor = 15f;
                lifeTime = 2.2f;
                rotationFactor = 0f;
                sSpeed = 0f;
                emissionFactor = 450f;
                dropling = TimeSeasonCyclerPlugin.assetMan.Get<Material>("Droplet");
                break;
            case Weather.WeatherMainType.Thunderstorm or Weather.WeatherMainType.Tornado:
                cloudy = true;
                gravityFactor = 15f;
                lifeTime = 2.2f;
                rotationFactor = 0f;
                sSpeed = 25f;
                emissionFactor = 850f;
                dropling = TimeSeasonCyclerPlugin.assetMan.Get<Material>("Droplet");
                break;
            case Weather.WeatherMainType.Snow:
                cloudy = true;
                break;
        }
        var particle = new GameObject(room.name + "_particles").AddComponent<ParticleSystem>();
        particle.transform.SetParent(transform);
        particle.transform.localPosition = Vector3.up * 1.25f;
        var renderer = particle.GetComponent<ParticleSystemRenderer>();
        renderer.material = dropling;
        renderer.SetActiveVertexStreams([ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Normal, ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV, ParticleSystemVertexStream.UV2, ParticleSystemVertexStream.Center]);

        var main = particle.main;

        main.startLifetime = lifeTime;
        main.startSpeed = sSpeed;
        main.gravityModifier = gravityFactor;
        main.maxParticles = 10000;

        var shape = particle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = room.ec.RealRoomMid(room) + Vector3.up * yOffset;
        shape.scale = room.ec.RealRoomSize(room);
        shape.randomDirectionAmount = 0.5f;

        if (rotationFactor != 0f)
        {
            var rotation = particle.rotationOverLifetime;
            rotation.enabled = true;
            rotation.x = rotationFactor;
        }

        var velocity = particle.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new(minMaxSpeedX.x, minMaxSpeedX.y);
        velocity.y = new(-Mathf.Abs(initialFallingSpeed), -Mathf.Abs(initialFallingSpeed));
        velocity.z = new(minMaxSpeedZ.x, minMaxSpeedZ.y);

        var emission = particle.emission;
        emission.enabled = true;
        emission.rateOverTimeMultiplier = emissionFactor;

        var collision = particle.collision;
        collision.enabled = true;
        collision.enableDynamicColliders = false;
        collision.bounceMultiplier = 0.25f;

        planeBounderies = new Transform[4];

        planeBounderies[0] = SetPlaneBoundarie(room.ec.RealRoomMin(room), Direction.South);
        planeBounderies[1] = SetPlaneBoundarie(room.ec.RealRoomMin(room), Direction.East);
        planeBounderies[2] = SetPlaneBoundarie(room.ec.RealRoomMax(room), Direction.North);
        planeBounderies[3] = SetPlaneBoundarie(room.ec.RealRoomMax(room), Direction.West);

        Transform SetPlaneBoundarie(Vector3 pos, Direction dir) // Min1 and Min2 are two corners
        {
            var plane = new GameObject($"PlaneOf_{dir}_rotation");
            plane.transform.SetParent(particle.transform);
            plane.transform.position = pos;
            plane.transform.rotation = Quaternion.Euler(0f, dir.ToRotation().eulerAngles.y, 90f);
            collision.AddPlane(plane.transform);
            return plane.transform;
        }
    }
}
