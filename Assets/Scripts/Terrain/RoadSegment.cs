using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class RoadSegment : MonoBehaviour
{
    [SerializeField] private Mesh2D shape2D;

    [Range(2, 32)]
    [SerializeField] private int edgeRingCount = 8;

    [Range(0, 1)]
    [SerializeField] private float t = 0;
    [SerializeField] private int bezierSegment = 0;
    [SerializeField] private bool showRingShape = true;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private Transform[] controlPoints;

    private Mesh mesh;

    private Vector3 GetPos(int i )
    {
        if(i % 3 == 0) return controlPoints[i / 3].position;
        else if((i - 1) % 3 == 0) return controlPoints[(i - 1)/ 3].TransformPoint(Vector3.forward * transform.localScale.z);
        else return controlPoints[(i + 1) / 3].TransformPoint(Vector3.back * transform.localScale.z);
    }

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Road Segment";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void Update()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        mesh.Clear();

        //Vertices
        float uSpan = shape2D.CalculUsSpan();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int bezierSegment = 0; bezierSegment < controlPoints.Length - 1; bezierSegment++)
        {
            for (int ring = 0; ring < edgeRingCount; ring++)
            {
                float t = ring / (edgeRingCount - 1f);
                OrientedPoint op = GetBezierOrientedPoint(t, bezierSegment);
                for (int i = 0; i < shape2D.VertexCount; i++)
                {
                    verts.Add(transform.InverseTransformPoint(op.LocalToWorldPos(shape2D.vertices[i].point)));
                    normals.Add(transform.InverseTransformDirection(op.LocalToWorldVec(shape2D.vertices[i].normal)));
                    uvs.Add(new Vector2(shape2D.vertices[i].u, t * GetApproxLength() / uSpan));
                }
            }
        }

        //Triangles
        List<int> triIndices = new List<int>();
        for (int ring = 0; ring < (edgeRingCount * (controlPoints.Length - 1)) - 1; ring++)
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
        for (int i = 0; i < 4 + (3 * (controlPoints.Length - 2)); i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.2f);
        }
        for(int i = 0; i < controlPoints.Length - 1; i++)
        {
            Handles.DrawBezier(GetPos(0 + (3 * i)), GetPos(3 + (3 * i)), GetPos(1 + (3 * i)), GetPos(2 + (3 * i)), Color.red, EditorGUIUtility.whiteTexture, 1f);
        }

        OrientedPoint op = GetBezierOrientedPoint(t, bezierSegment);
        //Handles.PositionHandle(op.position, op.rotation);

        if(showRingShape)
        {
            Vector3[] verts = shape2D.vertices.Select(v => op.LocalToWorldPos(v.point)).ToArray();
            for(int i = 0; i < shape2D.LineCount; i+=2)
            {
                Vector3 a = verts[shape2D.lineIndices[i]];
                Vector3 b = verts[shape2D.lineIndices[i+1]];
                Gizmos.DrawLine(a, b);
            }
        }
    }

    private OrientedPoint GetBezierOrientedPoint(float t, int bezierSegment)
    {
        Vector3[] pts = new Vector3[4];
        for (int i = 0; i < 4; i++) pts[i] = GetPos(i + (3 * bezierSegment));
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
        Vector3 up = Vector3.Lerp(controlPoints[0 + (1 * bezierSegment)].up, controlPoints[1 + (1 * bezierSegment)].up, t).normalized;
        Quaternion rotation = Quaternion.LookRotation(tangent, up);
        return new OrientedPoint(position, rotation);
    }


    private float GetApproxLength(int precision = 8)
    {
        Vector3[] points = new Vector3[precision * (controlPoints.Length - 1)];
        for(int i = 0; i < controlPoints.Length - 1; i++)
        {
            for (int j = 0; j < precision; j++)
            {
                float t = j / (precision - 1f);
                points[j + (precision * i)] = GetBezierOrientedPoint(t, i).position;
            }
        }
        float distance = 0;
        for(int i = 0; i < precision - 1; i++)
        {
            distance += Vector3.Distance(points[i], points[i+1]);
        }
        return distance;
    }
}