using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public enum SeedType { CompleteRandom, InCellRandom }

    public static class SeedGenerator
    {
        public static Vector2[] GenerateCompleteRandom2DSeed(int numSeed)
        {
            System.Random random = new System.Random();
            Vector2[] seeds = new Vector2[numSeed];
            for (int i = 0; i < numSeed; i++)
            {
                seeds[i] = Vector2.zero;
                seeds[i].x += (float)random.NextDouble();
                seeds[i].y += (float)random.NextDouble();
            }

            Vector2[] duplicatedSeed = new Vector2[numSeed * 9];
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    int index = (x + 3 * y) * numSeed;
                    for (int i = 0; i < numSeed; i++)
                    {
                        duplicatedSeed[index + i] = (seeds[i] + new Vector2(x, y)) / 3.0f;
                    }
                }
            }
            return duplicatedSeed;
        }

        public static Vector3[] GenerateCompleteRandom3DSeed(int numSeed)
        {
            System.Random random = new System.Random();
            Vector3[] seeds = new Vector3[numSeed];
            for (int i = 0; i < numSeed; i++)
            {
                seeds[i] = Vector3.zero;
                seeds[i].x += (float)random.NextDouble();
                seeds[i].y += (float)random.NextDouble();
                seeds[i].z += (float)random.NextDouble();
            }

            Vector3[] duplicatedSeed = new Vector3[numSeed * 27];
            for (int z = 0; z < 3; z++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        int index = (x + 3 * y + 9 * z) * numSeed;
                        for (int i = 0; i < numSeed; i++)
                        {
                            duplicatedSeed[index + i] = (seeds[i] + new Vector3(x, y, z)) / 3.0f;
                        }
                    }
                }
            }
            return duplicatedSeed;
        }

        public static Vector2[] GenerateInCellRandom2DSeed(int numCellsPerAxis)
        {
            System.Random random = new System.Random();
            int nPlusOne = numCellsPerAxis + 1;
            int extendedNumCellsPerAxis = numCellsPerAxis + 2;
            Vector2[] seeds = new Vector2[extendedNumCellsPerAxis * extendedNumCellsPerAxis];

            float cellSize = 1f / extendedNumCellsPerAxis;

            for (int x = 1; x <= numCellsPerAxis; x++)
            {
                for (int y = 1; y <= numCellsPerAxis; y++)
                {
                    // left-bottom corner of each cell
                    Vector2 seed = new Vector2(x, y);
                    Vector2 randomOffset = Vector2.zero;
                    randomOffset.x += (float)random.NextDouble();
                    randomOffset.y += (float)random.NextDouble();

                    seeds[x + extendedNumCellsPerAxis * y] = (seed + randomOffset) * cellSize;

                    if (x == 1)
                    {
                        seeds[nPlusOne + extendedNumCellsPerAxis * y] = (new Vector2(nPlusOne, y) + randomOffset) * cellSize;
                        if (y == 1)
                        {
                            seeds[nPlusOne * (extendedNumCellsPerAxis + 1)] = (new Vector2(nPlusOne, nPlusOne) + randomOffset) * cellSize;
                        }
                        else if (y == numCellsPerAxis)
                        {
                            seeds[nPlusOne] = (new Vector2(nPlusOne, 0) + randomOffset) * cellSize;
                        }
                    }
                    else if (x == numCellsPerAxis)
                    {
                        seeds[extendedNumCellsPerAxis * y] = (new Vector2(0, y) + randomOffset) * cellSize;
                        if (y == 1)
                        {
                            seeds[extendedNumCellsPerAxis * nPlusOne] = (new Vector2(0, nPlusOne) + randomOffset) * cellSize;
                        }
                        else if (y == numCellsPerAxis)
                        {
                            seeds[0] = randomOffset * cellSize;
                        }
                    }

                    if (y == 1)
                    {
                        seeds[x + extendedNumCellsPerAxis * nPlusOne] = (new Vector2(x, nPlusOne) + randomOffset) * cellSize;
                    }
                    else if (y == numCellsPerAxis)
                    {
                        seeds[x] = (new Vector2(x, 0) + randomOffset) * cellSize;
                    }
                }
            }

            return seeds;
        }

        public static Vector3[] GenerateInCellRandom3DSeed(int numCellsPerAxis)
        {
            System.Random random = new System.Random();
            int nPlusOne = numCellsPerAxis + 1;
            int extendedNumCellsPerAxis = numCellsPerAxis + 2;

            Vector3[] seeds = new Vector3[extendedNumCellsPerAxis * extendedNumCellsPerAxis * extendedNumCellsPerAxis];

            float cellSize = 1f / extendedNumCellsPerAxis;

            for (int x = 1; x <= numCellsPerAxis; x++)
            {
                for (int y = 1; y <= numCellsPerAxis; y++)
                {
                    for (int z = 1; z <= numCellsPerAxis; z++)
                    {
                        Vector3 corner = new Vector3(x, y, z);
                        Vector3 randomOffset = Vector3.zero;
                        randomOffset.x += (float)random.NextDouble();
                        randomOffset.y += (float)random.NextDouble();
                        randomOffset.z += (float)random.NextDouble();

                        seeds[x + extendedNumCellsPerAxis * (y + z * extendedNumCellsPerAxis)] = (corner + randomOffset) * cellSize;
                        // along x axis
                        if (x == 1)
                        {
                            seeds[nPlusOne + extendedNumCellsPerAxis * (y + z * extendedNumCellsPerAxis)] = (new Vector3(nPlusOne, y, z) + randomOffset) * cellSize;
                            // outside
                            if (y == 1)
                            {
                                seeds[nPlusOne + extendedNumCellsPerAxis * (nPlusOne + z * extendedNumCellsPerAxis)] = (new Vector3(nPlusOne, nPlusOne, z) + randomOffset) * cellSize;
                                if (z == 1)
                                {
                                    seeds[nPlusOne + extendedNumCellsPerAxis * (nPlusOne + nPlusOne * extendedNumCellsPerAxis)] = (new Vector3(nPlusOne, nPlusOne, nPlusOne) + randomOffset) * cellSize;
                                }
                                else if (z == numCellsPerAxis)
                                {
                                    seeds[nPlusOne + extendedNumCellsPerAxis * nPlusOne] = (new Vector3(nPlusOne, nPlusOne, 0) + randomOffset) * cellSize;
                                }
                            }
                            else if (y == numCellsPerAxis)
                            {
                                seeds[nPlusOne + extendedNumCellsPerAxis * z * extendedNumCellsPerAxis] = (new Vector3(nPlusOne, 0, z) + randomOffset) * cellSize;
                                if (z == 1)
                                {
                                    seeds[nPlusOne + extendedNumCellsPerAxis * nPlusOne * extendedNumCellsPerAxis] = (new Vector3(nPlusOne, 0, nPlusOne) + randomOffset) * cellSize;
                                }
                                else if (z == numCellsPerAxis)
                                {
                                    seeds[nPlusOne] = (new Vector3(nPlusOne, 0, 0) + randomOffset) * cellSize;
                                }
                            }
                        }
                        else if (x == numCellsPerAxis)
                        {
                            seeds[extendedNumCellsPerAxis * (y + z * extendedNumCellsPerAxis)] = (new Vector3(0, y, z) + randomOffset) * cellSize;
                            // outside
                            if (y == 1)
                            {
                                seeds[extendedNumCellsPerAxis * (nPlusOne + z * extendedNumCellsPerAxis)] = (new Vector3(0, nPlusOne, z) + randomOffset) * cellSize;
                                if (z == 1)
                                {
                                    seeds[extendedNumCellsPerAxis * (nPlusOne + nPlusOne * extendedNumCellsPerAxis)] = (new Vector3(0, nPlusOne, nPlusOne) + randomOffset) * cellSize;
                                }
                                else if (z == numCellsPerAxis)
                                {
                                    seeds[extendedNumCellsPerAxis * nPlusOne] = (new Vector3(0, nPlusOne, 0) + randomOffset) * cellSize;
                                }
                            }
                            else if (y == numCellsPerAxis)
                            {
                                seeds[extendedNumCellsPerAxis * z * extendedNumCellsPerAxis] = (new Vector3(0, 0, z) + randomOffset) * cellSize;
                                if (z == 1)
                                {
                                    seeds[extendedNumCellsPerAxis * nPlusOne * extendedNumCellsPerAxis] = (new Vector3(0, 0, nPlusOne) + randomOffset) * cellSize;
                                }
                                else if (z == numCellsPerAxis)
                                {
                                    seeds[0] = randomOffset * cellSize;
                                }
                            }
                        }
                        // along y axis
                        if (y == 1)
                        {
                            seeds[x + extendedNumCellsPerAxis * (nPlusOne + z * extendedNumCellsPerAxis)] = (new Vector3(x, nPlusOne, z) + randomOffset) * cellSize;
                            if (z == 1)
                            {
                                seeds[x + extendedNumCellsPerAxis * (nPlusOne + nPlusOne * extendedNumCellsPerAxis)] = (new Vector3(x, nPlusOne, nPlusOne) + randomOffset) * cellSize;
                            }
                            else if (z == numCellsPerAxis)
                            {
                                seeds[x + extendedNumCellsPerAxis * nPlusOne] = (new Vector3(x, nPlusOne, 0) + randomOffset) * cellSize;
                            }
                        }
                        else if (y == numCellsPerAxis)
                        {
                            seeds[x + extendedNumCellsPerAxis * z * extendedNumCellsPerAxis] = (new Vector3(x, 0, z) + randomOffset) * cellSize;
                            if (z == 1)
                            {
                                seeds[x + extendedNumCellsPerAxis * nPlusOne * extendedNumCellsPerAxis] = (new Vector3(x, 0, nPlusOne) + randomOffset) * cellSize;
                            }
                            else if (z == numCellsPerAxis)
                            {
                                seeds[x] = (new Vector3(x, 0, 0) + randomOffset) * cellSize;
                            }
                        }
                        // along z axis
                        if (z == 1)
                        {
                            seeds[x + extendedNumCellsPerAxis * (y + nPlusOne * extendedNumCellsPerAxis)] = (new Vector3(x, y, nPlusOne) + randomOffset) * cellSize;
                            if (x == 1)
                            {
                                seeds[nPlusOne + extendedNumCellsPerAxis * (y + nPlusOne * extendedNumCellsPerAxis)] = (new Vector3(nPlusOne, y, nPlusOne) + randomOffset) * cellSize;
                            }
                            else if (x == numCellsPerAxis)
                            {
                                seeds[extendedNumCellsPerAxis * (y + nPlusOne * extendedNumCellsPerAxis)] = (new Vector3(0, y, nPlusOne) + randomOffset) * cellSize;
                            }
                        }
                        else if (z == numCellsPerAxis)
                        {
                            seeds[x + extendedNumCellsPerAxis * y] = (new Vector3(x, y, 0) + randomOffset) * cellSize;
                            if (x == 1)
                            {
                                seeds[nPlusOne + extendedNumCellsPerAxis * y] = (new Vector3(nPlusOne, y, 0) + randomOffset) * cellSize;
                            }
                            else if (x == numCellsPerAxis)
                            {
                                seeds[extendedNumCellsPerAxis * y] = (new Vector3(0, y, 0) + randomOffset) * cellSize;
                            }
                        }
                    }
                }
            }

            return seeds;
        }
    }
}