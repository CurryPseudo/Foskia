using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PseudoTools;
// Create a mesh by raycast
public class ViewMesh : MonoBehaviour {  
    public float radius;
    public int rayCount;
    public LayerMask layer; 
    public float binaryMaxDistance = 0.2f;
    public float binaryMaxDegree = 5;
    public float binaryMaxNormalDelta = 0.2f;
    public Mesh mesh;
    public float meshVerticesZ = -0.1f;
    public Vector2 pos {
        get {
            return VectorUtils.V32(transform.position);
        }
    }
    public void Awake() {
        if(mesh == null) mesh = new Mesh();
        mesh.MarkDynamic();
        CacMesh();
    }
    public void Start() {
    }
    public void Update() {
        CacMesh();
    }
    public List<Vector2> RaycastPoints() {
        float deltaDegree = 360.0f / (float) rayCount;    
        float lastDegree = 0;
        RaycastHit2D lastHit = new RaycastHit2D();
        List<Vector2> points = new List<Vector2>();
        Func<float, Vector2> degree2Dir = degree => {
            float rayRad = Mathf.Deg2Rad * degree;
            Vector2 rayDir = new Vector2(Mathf.Sin(rayRad), Mathf.Cos(rayRad));
            return rayDir;
        };
        Func<float, RaycastHit2D> angleRayCast = degree => {
            var rayDir = degree2Dir(degree);
            var hit = Physics2D.Raycast(pos, rayDir, radius, layer);
            return hit;
        };
        Action<RaycastHit2D, float> addHitPoint = (hit, degree) => {
            if(hit) {
                points.Add(hit.point);
            }
            else {
                points.Add(pos + degree2Dir(degree) * radius);
            }
        };
        Action<RaycastHit2D, float, RaycastHit2D, float> BinaryFindEdgeAndAddPoint = null;
        //Assume degree1 is bigger than degree2
        //hit1 result added, hit2 not yet
        BinaryFindEdgeAndAddPoint = (hit1, degree1, hit2, degree2) => {
            Func<RaycastHit2D, float> hitDis = hit => hit.collider == null ? radius : hit.distance;
            Func<RaycastHit2D, Vector2> hitNormal = hit => hit.collider == null ? Vector2.zero : hit.normal;
            if((Mathf.Abs(hitDis(hit1) - hitDis(hit2)) < binaryMaxDistance && (hitNormal(hit1) - hitNormal(hit2)).magnitude < binaryMaxNormalDelta) || degree2 - degree1 < binaryMaxDegree) {
                addHitPoint(hit2, degree2);
                return;
            }
            var midDegree = (degree1 + degree2) / 2;
            var midHit = angleRayCast(midDegree);
            BinaryFindEdgeAndAddPoint(hit1, degree1, midHit, midDegree);
            BinaryFindEdgeAndAddPoint(midHit, midDegree, hit2, degree2);
        };
        for(int i = 0; i < rayCount + 1; i++) {
            float rayDegree = deltaDegree * i;
            var hit = angleRayCast(rayDegree);
            if(i > 0) {
                BinaryFindEdgeAndAddPoint(lastHit, lastDegree, hit, rayDegree);
            }
            else {
                addHitPoint(hit, rayDegree);
            }
            lastHit = hit;
            lastDegree = rayDegree;
            

        }
        return points;
    }
    public List<Vector2> WithoutCastPoints(int pointCount) {
        var points = new List<Vector2>();
        float degree = 360 / (float)(pointCount - 1);
        for(int i = 0; i < pointCount; i++) {
            float rad = degree * (float)i * Mathf.Deg2Rad; 
            Vector2 point = radius * new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) + pos;
            points.Add(point);
        }
        return points;
    }
    public void CacMesh() {
        List<Vector2> points;
        if(layer == 0) {
            points = WithoutCastPoints(361);
        }
        else {
            points = RaycastPoints();
        }
        Vector3[] vertices = new Vector3[points.Count + 1];
        vertices[0] = v23(pos);
        for(int i = 0; i < points.Count; i++) {
            vertices[i + 1] = v23(points[i]);
        }
        int[] triangles = new int[(points.Count - 1) * 3];
        for(int i = 0; i < points.Count - 1; i++) {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }
        mesh.Clear(true);
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    public Vector3 v23(Vector2 v2) {
        return VectorUtils.V23(v2, meshVerticesZ);
    }
    public void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
