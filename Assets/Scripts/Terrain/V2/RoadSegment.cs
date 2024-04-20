using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class RoadSegment : MonoBehaviour
{
    [SerializeField] Mesh2D shape2D;

    [Range(2, 32)]
    [SerializeField] int edgeRingCount = 8;

    [Range(0, 1)]
    [SerializeField] float t = 0;
    [SerializeField] bool showRingShape = true;

    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;

    Mesh mesh;

    Vector3 GetPos(int i )
    {
        switch(i)
        {
            case 0: return startPoint.position;
            case 1: return startPoint.TransformPoint(Vector3.forward * transform.localScale.z);
            case 2: return endPoint.TransformPoint(Vector3.back * transform.localScale.z);
            case 3: return endPoint.position;
            default: return Vector3.zero;
        }
    }

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Road Segment";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void Update()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        mesh.Clear();

        //Vertices
        float uSpan = shape2D.CalculUsSpan();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        for (int ring = 0; ring < edgeRingCount; ring++)
        {
            float t = ring / (edgeRingCount - 1f);
            OrientedPoint op = GetBezierOrientedPoint(t);
            for (int i = 0; i < shape2D.VertexCount; i++)
            {
                verts.Add(op.LocalToWorldPos(shape2D.vertices[i].point));
                normals.Add(op.LocalToWorldVec(shape2D.vertices[i].normal));
                uvs.Add(new Vector2(shape2D.vertices[i].u, t * GetApproxLength() / uSpan));
            }
        }

        //Triangles
        List<int> triIndices = new List<int>();
        for (int ring = 0; ring < edgeRingCount - 1; ring++)
        {
            int rootIndex = ring * shape2D.VertexCount;
            int nextRootIndex = (ring + 1) * shape2D.VertexCount;
            for (int line = 0; line < shape2D.LineCount; line += 2)
            {
                int lineIndexA = shape2D.lineIndices[line];
                int lineIndexB = shape2D.lineIndices[line + 1];
                int currentA = rootIndex + lineIndexA;
                int currentB = rootIndex + lineIndexB;
                int nextA = nextRootIndex + lineIndexA;
                int nextB = nextRootIndex + lineIndexB;
                triIndices.Add(currentA);
                triIndices.Add(nextA);
                triIndices.Add(nextB);
                triIndices.Add(currentA);
                triIndices.Add(nextB);
                triIndices.Add(currentB);
            }
        }
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triIndices, 0);
    }

    public void OnDrawGizmos()
    {
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.05f);

            Handles.DrawBezier(GetPos(0), GetPos(3), GetPos(1), GetPos(2), Color.red, EditorGUIUtility.whiteTexture, 1f);
        }

        OrientedPoint op = GetBezierOrientedPoint(t);
        //Handles.PositionHandle(op.position, op.rotation);

        if(showRingShape)
        {
            Vector3[] verts = shape2D.vertices.Select(v => op.LocalToWorldPos(v.point)).ToArray();
            for(int i = 0; i < shape2D.lineIndices.Length; i+=2)
            {
                Vector3 a = verts[shape2D.lineIndices[i]];
                Vector3 b = verts[shape2D.lineIndices[i+1]];
                Gizmos.DrawLine(a, b);
            }
        }
    }

    OrientedPoint GetBezierOrientedPoint(float t)
    {
        Vector3[] pts = new Vector3[4];
        for (int i = 0; i < 4; i++) pts[i] = GetPos(i);
        float omt = 1f-t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 position =	pts[0] * ( omt2 * omt ) +
                            pts[1] * ( 3f * omt2 * t ) +
                            pts[2] * ( 3f * omt * t2 ) +
                            pts[3] * ( t2 * t );

        Vector3 tangent =   pts[0] * ( -omt2 ) +
                            pts[1] * ( 3 * omt2 - 2 * omt ) +
                            pts[2] * ( -3 * t2 + 2 * t ) +
                            pts[3] * ( t2 );
        Vector3 up = Vector3.Lerp(startPoint.up, endPoint.up, t).normalized;
        Quaternion rotation = Quaternion.LookRotation(tangent, up);
        return new OrientedPoint(position, rotation);
    }

    float GetApproxLength(int precision = 8)
    {
        Vector3[] points = new Vector3[precision];
        for(int i = 0; i < precision; i++)
        {
            float t = i / (precision - 1f);
            points[i] = GetBezierOrientedPoint(t).position;
        }
        float distance = 0;
        for(int i = 0; i < precision - 1; i++)
        {
            distance += Vector3.Distance(points[i], points[i+1]);
        }
        return distance;
    }
}
