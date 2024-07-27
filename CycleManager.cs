using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BaldiPlus_Seasons
{
    public enum Weather
    {
        Clear,
        Cloudy,
        Rain,
        Storm,
        Windy
    }

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
        static bool debug = true;
#endif

        public static CycleManager Instance;
        public static EnvironmentController ec;
        private void Awake()
        {
            DontDestroyOnLoad(this);
            Instance = this;

            RefreshTime();
        }

        public void RefreshTime()
        {
            if (BasePlugin.southern)
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
                seasons = Seasons.Winter;
                time = 1;
            }
#endif
        }

        public Seasons seasons;
        public Weather weather;
        public int time;

        public static Material droplets;
        public static GameObject snowman;

        public static List<SeasonalRoom> targetRooms = new List<SeasonalRoom>();

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
    }

    public class SeasonalRoom
    {
        public RoomAsset roomAsset;
        public List<Texture2D> floorReplacements = new List<Texture2D>();

        public bool affectsLighting = false;
        public bool targetsWallTexture = false;
        public List<Texture2D> wallReplacements = new List<Texture2D>();
    }
}
