using UnityEngine;
using System.Collections;
namespace PseudoTools {
    public static class CameraUtils {
        private static Mesh quadMesh;
        public static Mesh QuadMesh {
            get {
                if(quadMesh == null) {
                    quadMesh = new Mesh();
                }
                AllocateQuadMesh(ref quadMesh, -0.1f, Camera.main);
                return quadMesh;
            }
        }
        public static Vector3 MainPos {
            get {
                return VectorUtils.V32(Camera.main.transform.position);
            }
        }
        public static Camera AddRenderCamera(GameObject go, LayerMask cullingMask) {
            var camera  = go.GetComponent<Camera>();
            if(camera == null) camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0,0,0,0);
            camera.cullingMask = cullingMask;
            camera.orthographic = true;
            camera.depth = -2;
            return camera;
        }
        public static void CopyCameraPosAndSize (Camera camera, Camera target) {
            camera.transform.position = target.transform.position;
            camera.orthographicSize = target.orthographicSize;
        }
        public static void SetTargetTexture(Camera camera, RenderTexture targetTexture) {
            camera.targetTexture = targetTexture;
        }
        public static void AllocateQuadMesh(ref Mesh quadMesh, float pointZ, Camera camera) {
            quadMesh.MarkDynamic();
            Vector2 cameraSize = VectorUtils.Multiply(new Vector2(camera.aspect, 1), new Vector2(1,1) * camera.orthographicSize);
            Vector3[] vertices = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            // vertices
            // 1  3
            // 0  2
            // uv
            // (0,0) (1,0)
            // (0,1) (1,1)
            int k = 0;
            for(int i = -1; i <= 1; i += 2) {
                for(int j = -1; j <= 1; j += 2) {
                    vertices[k] = VectorUtils.Multiply(new Vector3(i, j, pointZ), VectorUtils.V23(cameraSize, 1));
                    uvs[k] = new Vector2((1 + i) / 2, (1 - j) / 2);
                    k++;
                }
            }
            int[] triangles = {0, 1, 3, 0, 3, 2};
            quadMesh.vertices = vertices;
            quadMesh.uv = uvs;
            quadMesh.triangles = triangles;
            quadMesh.RecalculateNormals();
        }
    }
}