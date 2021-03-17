using UnityEngine;
using System.Runtime.InteropServices;

namespace Voronoi
{
    public class VoronoiGenerator2D : VoronoiGenerator
    {
        static readonly float numthreads2D = 32.0f;
        Vector2[] seeds;
        public VoronoiGenerator2D(int _resolution, int _seedNum, bool _invert = true) : base(_resolution, _seedNum, _invert)
        {
            voronoiCS = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/CS/JFA_VoronoiNoise_2D.compute");
            // Find all kernel
            clearKernel = voronoiCS.FindKernel("Clear");
            initKernel = voronoiCS.FindKernel("InitSeeds");
            solverKernel = voronoiCS.FindKernel("JFA");
            minMaxKernel = voronoiCS.FindKernel("GlobalMinMax");
            normalizeKernel = voronoiCS.FindKernel("Normalize");

            pixels = new RenderTexture(resolution * 3, resolution * 3, 0, RenderTextureFormat.ARGBHalf, 0)
            {
                enableRandomWrite = true,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            pixels.Create();
            voronoiCS.SetTexture(clearKernel, "pixels", pixels);
            voronoiCS.SetTexture(initKernel, "pixels", pixels);
            voronoiCS.SetTexture(solverKernel, "pixels", pixels);

            seeds = SeedGenerator.GenerateCompleteRandom2DSeed(seedNum);
            seedBuffer = new ComputeBuffer(seeds.Length, Marshal.SizeOf(typeof(Vector2)));
            seedBuffer.SetData(seeds);
            voronoiCS.SetBuffer(initKernel, "seeds", seedBuffer);
            voronoiCS.SetBuffer(solverKernel, "seeds", seedBuffer);

            // Record Min/Max distance
            int[] minMax = { int.MaxValue, 0 };
            minMaxBuffer = new ComputeBuffer(minMax.Length, sizeof(int));
            minMaxBuffer.SetData(minMax);
            voronoiCS.SetBuffer(minMaxKernel, "minMax", minMaxBuffer);
            voronoiCS.SetBuffer(normalizeKernel, "minMax", minMaxBuffer);

            voronoiCS.SetInt("resolution", resolution * 3);
            voronoiCS.SetInt("numSeeds", seeds.Length);
            voronoiCS.SetBool("invert", invert);

            dst = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, 0)
            {
                enableRandomWrite = true,
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            dst.Create();
            voronoiCS.SetTexture(minMaxKernel, "pixels", dst);
            voronoiCS.SetTexture(normalizeKernel, "pixels", dst);
        }

        public override RenderTexture Solve()
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            //Clear pixels
            int numGroupsTex = Mathf.CeilToInt(resolution * 3 / numthreads2D);
            voronoiCS.Dispatch(clearKernel, numGroupsTex, numGroupsTex, 1);

            // Initilize seeds into pixels
            int numGroupsInit = Mathf.CeilToInt(seeds.Length / 512f);
            voronoiCS.Dispatch(initKernel, numGroupsInit, 1, 1);

            int step = Mathf.CeilToInt(resolution * 3 / 2.0f);//Mathf.CeilToInt(Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(resolution * 3))));
            int iter = 0;
            while (true)
            {
                voronoiCS.SetInt("step", step);
                voronoiCS.Dispatch(solverKernel, numGroupsTex, numGroupsTex, 1);
                iter++;
                if (step / 2.0f < 1)
                    break;
                else
                    step = Mathf.CeilToInt(step / 2.0f);
            }
            // 1+jfa
            voronoiCS.SetInt("step", 1);
            voronoiCS.Dispatch(solverKernel, numGroupsTex, numGroupsTex, 1);

            // Copy the center of pixels to dst
            Graphics.CopyTexture(pixels, 0, 0, resolution, resolution, resolution, resolution, dst, 0, 0, 0, 0);
            int numGroupNormalize = Mathf.CeilToInt(resolution / numthreads2D);
            voronoiCS.Dispatch(minMaxKernel, numGroupNormalize, numGroupNormalize, 1);
            voronoiCS.Dispatch(normalizeKernel, numGroupNormalize, numGroupNormalize, 1);
            Debug.Log($"Voronoi2D Time:{timer.ElapsedMilliseconds}ms");
            int[] minmax = { 0, 0 };
            minMaxBuffer.GetData(minmax);
            return dst;
        }
    }
}
