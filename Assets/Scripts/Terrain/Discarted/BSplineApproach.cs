using UnityEngine;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class BSplineApproach : MonoBehaviour
{
    [SerializeField] Mesh2D shape2D;
    //[Range(2, 320)]
    //[SerializeField] int edgeRingCount = 8;
    [Range(0, 1)]
    [SerializeField] float t = 0;
    [SerializeField] bool showRingShape = true;
    [SerializeField] Transform[] controlPoints;
    private Mesh mesh;

    private Vector3 GetPos(int i) => controlPoints[i].position;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "BSpline Approach";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void OnDrawGizmos()
    {
        int resolution = 100;
        int degree = 2;

        for (int i = 0; i < controlPoints.Length; i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.05f);
        }

        if (controlPoints.Length > degree)
        {
            Vector3 previousPosition = GetBSplinePoint(0, controlPoints, degree);

            for (int i = 1; i <= resolution; i++)
            {
                float t = (float)i / resolution;
                Vector3 position = GetBSplinePoint(t, controlPoints, degree);
                Gizmos.DrawLine(previousPosition, position);
                previousPosition = position;
            }
        }


        OrientedPoint op = GetBSplineOrientedPoint(t, controlPoints);
        Handles.PositionHandle(op.position, op.rotation);

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

    public OrientedPoint GetBSplineOrientedPoint(float t, Transform[] controlPoints, int degree = 2)
    {
        //generate knotVector
        int knotCount = controlPoints.Length + degree + 1;
        float[] knotVector = new float[knotCount];
        for (int i = 0; i < knotCount; i++)
        {
            if (i < degree) knotVector[i] = 0;
            else if (i > controlPoints.Length) knotVector[i] = 1;
            else knotVector[i] = (i - degree) / (float)(controlPoints.Length - degree);
        }

        Vector3 result = Vector3.zero;
        Vector3 tangent = Vector3.zero;
        for (int i = 0; i < controlPoints.Length; i++)
        {
            result += controlPoints[i].position * BSplineAlgorithm(i, degree, knotVector, t);
            tangent += controlPoints[i].position * BasisFunctionDerivative(i, degree, t, knotVector);
        }

        return new OrientedPoint(result, Quaternion.LookRotation(tangent.normalized, Vector3.up)); //TODO: make that rotation of control points matters
    }

    public Vector3 GetBSplinePoint(float t, Transform[] controlPoints, int degree = 2)
    {
        //generate knotVector
        int knotCount = controlPoints.Length + degree + 1;
        float[] knotVector = new float[knotCount];
        for (int i = 0; i < knotCount; i++)
        {
            if (i < degree) knotVector[i] = 0;
            else if (i > controlPoints.Length) knotVector[i] = 1;
            else knotVector[i] = (i - degree) / (float)(controlPoints.Length - degree);
        }

        Vector3 result = Vector3.zero;
        for (int i = 0; i < controlPoints.Length; i++)
        {
            result += controlPoints[i].position * BSplineAlgorithm(i, degree, knotVector, t);
        }

        return result;
    }

    private float BSplineAlgorithm(int i, int degree, float[] knotVector, float t)
    {
        if (degree == 0)
        {
            if (knotVector[i] <= t && t < knotVector[i + 1]) return 1;
            else return 0;
        }
        else
        {
            float a = (t - knotVector[i]) / (knotVector[i + degree] - knotVector[i]);
            float b = (knotVector[i + degree + 1] - t) / (knotVector[i + degree + 1] - knotVector[i + 1]);
            return a * BSplineAlgorithm(i, degree - 1, knotVector, t) + b * BSplineAlgorithm(i + 1, degree - 1, knotVector, t);
        }
    }

    private float BasisFunctionDerivative(int i, int degree, float t, float[] knotVector)
    {
        float a = (t - knotVector[i]) / (knotVector[i + degree] - knotVector[i]);
        float b = (knotVector[i + degree + 1] - t) / (knotVector[i + degree + 1] - knotVector[i + 1]);

        return degree * (a * BSplineAlgorithm(i, degree - 1, knotVector, t) - b * BSplineAlgorithm(i + 1, degree - 1, knotVector, t));
    }

    private float GetApproxLength(int precision = 8)
    {
        Vector3[] points = new Vector3[precision];
        for(int i = 0; i < precision; i++)
        {
            float t = i / (precision - 1f);
            points[i] = GetBSplineOrientedPoint(t, controlPoints).position;
        }
        float distance = 0;
        for(int i = 0; i < precision - 1; i++)
        {
            distance += Vector3.Distance(points[i], points[i+1]);
        }
        return distance;
    }
}
