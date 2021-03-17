using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public class VoronoiViewer : MonoBehaviour
    {
        VoronoiGenerator voronoiGenerator;
        public VoronoiType voronoiType;
        public int resolution, seedNum;
        public bool invert;
        // For visualize 3D voronoi texture only
        ComputeShader sliceViewer;
        RenderTexture slice;
        int sliceKernel;
        public int sliceLayer = 0;

        void Start()
        {
            switch (voronoiType)
            {
                case VoronoiType.TwoDiemension:
                    voronoiGenerator = new VoronoiGenerator2D(resolution, seedNum, invert);
                    break;
                case VoronoiType.ThreeDiemension:
                    voronoiGenerator = new VoronoiGenerator3D(resolution, seedNum, invert);
                    sliceViewer = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/CS/TextureProcessorCS.compute");
                    sliceKernel = sliceViewer.FindKernel("Export2DSlice");
                    slice = new RenderTexture(resolution, resolution, 0)
                    {
                        dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                        enableRandomWrite = true,
                        wrapMode = TextureWrapMode.Repeat
                    };
                    slice.Create();
                    sliceViewer.SetTexture(sliceKernel, "Slice", slice);
                    sliceViewer.SetInt("resolution", resolution);
                    VoronoiGenerator3D generator3D = voronoiGenerator as VoronoiGenerator3D;
                    sliceViewer.SetTexture(sliceKernel, "Tex3D", generator3D.Solve());
                    GameObject.Find("Plane").GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", slice);
                    break;
                default:
                    break;
            }
        }

        void FixedUpdate()
        {
            switch (voronoiType)
            {
                case VoronoiType.TwoDiemension:
                    VoronoiGenerator2D generator2D = voronoiGenerator as VoronoiGenerator2D;
                    GameObject.Find("Plane").GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", generator2D.Solve());
                    break;
                case VoronoiType.ThreeDiemension:
                    sliceViewer.SetInt("layer", Mathf.Clamp(sliceLayer, 0, resolution - 1));
                    int numGroup = Mathf.CeilToInt(resolution / 32.0f);
                    sliceViewer.Dispatch(sliceKernel, numGroup, numGroup, 1);
                    break;
                default:
                    break;
            }
        }

        private void OnDestroy()
        {
            if (slice != null)
                slice.Release();
            voronoiGenerator.Release();
        }
    }
}
