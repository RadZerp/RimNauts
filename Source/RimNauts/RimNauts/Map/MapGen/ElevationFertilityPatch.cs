﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimNauts
{
    [HarmonyPatch(typeof(RimWorld.GenStep_ElevationFertility), "Generate")]
    static class ElevationFertilityPatch
    {
        static void Postfix(Map map, GenStepParams parms)
        {
            // check if it's our biome. If not, skip the patch
            if (map.Biome.defName != "RockMoonBiome")
            {
                return;
            }

            // Map generation is based mostly on these two grids. We're making custom grids.
            MapGenFloatGrid fertility = MapGenerator.Fertility;
            MapGenFloatGrid elevation = MapGenerator.Elevation;

            // the size of terrain features. Smaller numbers = bigger terrain features. Vanilla = 0.021
            float mountainSize = Rand.Range(0.025f, 0.035f);

            // Overall shape. Smaller numbers = smoother. Vanilla = 2.0
            float mountainSmoothness = Rand.Range(0.0f, 1.0f);

            // the overal shape of the mountains and map features
            ModuleBase elevationGrid = new Perlin(mountainSize, mountainSmoothness, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);

            // Make the mountains bigger
            double elevationScaling = 1.25f;
            elevationGrid = new Multiply(elevationGrid, new Const(elevationScaling));

            // By setting fertility = elevation, we ensure that the shape of the terrain will follow the shape of the mountains
            foreach (IntVec3 tile in map.AllCells)
            {
                fertility[tile] = elevationGrid.GetValue(tile);
            }

            // This changes the relative amount of light and dark mountains
            float offset = 0.4f;

            // Skews the grid towards the center to create a "sea"
            IntVec3 center = map.Center;
            int size = map.Size.x / 2;
            float seaAmount = 2.0f;

            foreach (IntVec3 tile in map.AllCells)
            {
                float distance = (float)Math.Sqrt(Math.Pow(tile.x - center.x, 2) + Math.Pow(tile.z - center.z, 2));
                //float difference = seaAmount * distance / size - 0.5f;
                float difference = Math.Min(1, difference = seaAmount * distance / size - 0.5f);

                fertility[tile] += difference;

                // use Abs so that there's mountains on both ends of the grid.
                elevation[tile] = Mathf.Abs(fertility[tile] - offset);

            }

        }
    }
}
