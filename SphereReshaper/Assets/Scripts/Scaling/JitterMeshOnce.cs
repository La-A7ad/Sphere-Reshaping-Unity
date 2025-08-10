using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class JitterMeshOnce : MonoBehaviour {
    [Range(0f, 0.15f)] public float amplitude = 0.04f;
    [Range(0.1f, 4f)]  public float frequency = 1.8f;
    public int seed = 12345;

    void Start() {
        var mf = GetComponent<MeshFilter>();
        var src = mf.sharedMesh;
        var mesh = Instantiate(src);
        var v = mesh.vertices; var n = mesh.normals;

        var rnd = new System.Random(seed);
        float ox = (float)rnd.NextDouble()*10f, oy = (float)rnd.NextDouble()*10f, oz = (float)rnd.NextDouble()*10f;

        for (int i = 0; i < v.Length; i++) {
            var p = v[i].normalized;
            float a = Mathf.PerlinNoise(p.x*frequency + ox, p.y*frequency + oy);
            float b = Mathf.PerlinNoise(p.y*frequency + oy, p.z*frequency + oz);
            float t = 0.5f*(a+b);
            float delta = (t*2f - 1f) * amplitude;
            v[i] = v[i] + n[i] * delta;
        }

        mesh.vertices = v; mesh.RecalculateNormals(); mesh.RecalculateBounds();
        mf.sharedMesh = mesh;
        Destroy(this);
    }
}
