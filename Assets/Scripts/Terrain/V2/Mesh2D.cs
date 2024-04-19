using UnityEngine;
using UnityEngine.ProBuilder;

[CreateAssetMenu]
public class Mesh2D : ScriptableObject
{
    [System.Serializable]
    public class Vertex
    {
        public Vector2 point;
        public Vector2 normal;
        public float us;
    }

    public Vertex[] vertices;
    public int[] lineIndices;
}
