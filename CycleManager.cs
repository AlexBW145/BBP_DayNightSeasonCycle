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
            time = DateTime.Now.Hour;

#if DEBUG
            if (debug)
            {
                seasons = Seasons.Summer;
                time = 21;
            }
#endif
        }
#if DEBUG
        // THIRD PARTY FUNCTIONS THAT WERE PROBABLY USELESS TO USE ANYWAYS

        // Taken from Endless Floors
        // I really wished that Missing Texture Dude added this to the API...
        static Texture2D ThirdParty_EndlessFloors_FlipX(Texture2D texture)
        {
            Texture2D flipped = new Texture2D(texture.width, texture.height);

            int width = texture.width;
            int height = texture.height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    flipped.SetPixel(width - i - 1, j, texture.GetPixel(i, j));
                }
            }
            flipped.Apply();

            return flipped;
        }

        static Texture2D ThirdParty_EndlessFloors_FlipY(Texture2D texture)
        {
            Texture2D flipped = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

            int width = texture.width;
            int height = texture.height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    flipped.SetPixel(i, height - j - 1, texture.GetPixel(i, j));
                }
            }
            flipped.Apply();

            return flipped;
        }

        public static Cubemap ThirdParty_EndlessFloors_CubemapFromTexture2D(Texture2D texture)
        {
            texture = ThirdParty_EndlessFloors_FlipX(texture);
            texture = ThirdParty_EndlessFloors_FlipY(texture);

            int cubemapWidth = texture.width / 6;
            Cubemap cubemap = new Cubemap(cubemapWidth, TextureFormat.ARGB32, false);
            cubemap.SetPixels(texture.GetPixels(0 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.NegativeZ);
            cubemap.SetPixels(texture.GetPixels(1 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.PositiveZ);
            cubemap.SetPixels(texture.GetPixels(2 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.NegativeY);
            cubemap.SetPixels(texture.GetPixels(3 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.PositiveY);
            cubemap.SetPixels(texture.GetPixels(4 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.NegativeX);
            cubemap.SetPixels(texture.GetPixels(5 * cubemapWidth, 0, cubemapWidth, cubemapWidth), CubemapFace.PositiveX);
            cubemap.Apply();
            return cubemap;
        }
#endif

        public Seasons seasons;
        public Weather weather;
        public int time;

        public static List<Texture2D> Grass = new List<Texture2D>();
        public static List<Material> Tree = new List<Material>();
#if DEBUG
        public static Cubemap nightCubemap;
#endif
        public static Material droplets;
        public static GameObject snowman;
    }
}
