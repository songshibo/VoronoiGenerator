using UnityEngine;

namespace Voronoi
{
    public class VoronoiGenerator
    {
        public int resolution, seedNum;
        public bool invert;
        public ComputeShader voronoiCS;
        public RenderTexture pixels, dst;
        public ComputeBuffer seedBuffer, minMaxBuffer;
        public int clearKernel, initKernel, solverKernel, minMaxKernel, normalizeKernel;
        public VoronoiGenerator(int _resolution, int _seedNum, bool _invert = true)
        {
            resolution = _resolution;
            seedNum = _seedNum;
            invert = _invert;
        }

        public virtual RenderTexture Solve()
        {
            return dst;
        }

        public void Release()
        {
            if (pixels != null)
                pixels.Release();

            if (dst != null)
                dst.Release();

            if (seedBuffer != null)
                seedBuffer.Release();

            if (minMaxBuffer != null)
                minMaxBuffer.Release();
        }
    }
}
