using System.Runtime.InteropServices;
using UnityEngine;

namespace Voronoi
{
    public class VoronoiGenerator3D : VoronoiGenerator
    {
        static readonly float numthreads3D = 8.0f;
        Vector3[] seeds;
        public VoronoiGenerator3D(int _resolution, int _seedNum, bool _invert = true) : base(_resolution, _seedNum, _invert)
        {
            voronoiCS = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/CS/JFA_VoronoiNoise_3D.compute");
            // Find all kernel
            clearKernel = voronoiCS.FindKernel("Clear");
            initKernel = voronoiCS.FindKernel("InitSeeds");
            solverKernel = voronoiCS.FindKernel("JFA");
            minMaxKernel = voronoiCS.FindKernel("GlobalMinMax");
            normalizeKernel = voronoiCS.FindKernel("Normalize");

            pixels = new RenderTexture(resolution * 3, resolution * 3, 0)
            {
                format = RenderTextureFormat.ARGBHalf,
                enableRandomWrite = true,
                volumeDepth = resolution * 3,
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            pixels.Create();
            voronoiCS.SetTexture(clearKernel, "pixels", pixels);
            voronoiCS.SetTexture(initKernel, "pixels", pixels);
            voronoiCS.SetTexture(solverKernel, "pixels", pixels);

            seeds = SeedGenerator.GenerateCompleteRandom3DSeed(seedNum);
            seedBuffer = new ComputeBuffer(seeds.Length, Marshal.SizeOf(typeof(Vector3)));
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
            voronoiCS.SetInt("offset", resolution);
            voronoiCS.SetInt("numSeeds", seeds.Length);
            voronoiCS.SetBool("invert", invert);

            dst = new RenderTexture(resolution, resolution, 0)
            {
                format = RenderTextureFormat.ARGBHalf,
                enableRandomWrite = true,
                volumeDepth = resolution,
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
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
            int numGroupsTex = Mathf.CeilToInt(resolution * 3 / numthreads3D);
            voronoiCS.Dispatch(clearKernel, numGroupsTex, numGroupsTex, numGroupsTex);

            // Initilize seeds into pixels
            int numGroupsInit = Mathf.CeilToInt(seeds.Length / 512f);
            voronoiCS.Dispatch(initKernel, numGroupsInit, 1, 1);

            int step = Mathf.CeilToInt(Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(resolution * 3))));
            int iter = 0;
            while (true)
            {
                voronoiCS.SetInt("step", step);
                voronoiCS.Dispatch(solverKernel, numGroupsTex, numGroupsTex, numGroupsTex);
                iter++;
                if (step / 2.0f < 1)
                    break;
                else
                {
                    step = Mathf.CeilToInt(step / 2.0f);
                }
            }
            for (int i = 0; i < resolution; i++)
            {
                Graphics.CopyTexture(pixels, i + resolution, 0, resolution, resolution, resolution, resolution, dst, i, 0, 0, 0);
            }
            int numGroupNormalize = Mathf.CeilToInt(resolution / numthreads3D);
            voronoiCS.Dispatch(minMaxKernel, numGroupNormalize, numGroupNormalize, numGroupNormalize);
            voronoiCS.Dispatch(normalizeKernel, numGroupNormalize, numGroupNormalize, numGroupNormalize);
            int[] minmax = { 0, 0 };
            minMaxBuffer.GetData(minmax);
            Debug.Log($"Voronoi3D Time:{timer.ElapsedMilliseconds}ms");
            return dst;
        }
    }
}
