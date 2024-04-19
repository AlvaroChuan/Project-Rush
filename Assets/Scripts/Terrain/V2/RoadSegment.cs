using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter))]
public class RoadSegment : MonoBehaviour
{
    [SerializeField] Mesh2D shape2D;

    [Range(0, 1)]
    [SerializeField] float t = 0;
    [SerializeField] Transform[] ControlPoints;

    Vector3 GetPos(int i ) => ControlPoints[i].position;

    public void OnDrawGizmos()
    {
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.05f);

            Handles.DrawBezier(GetPos(0), GetPos(3), GetPos(1), GetPos(2), Color.red, EditorGUIUtility.whiteTexture, 1f);
        }
        Handles.PositionHandle(GetBezierOrientedPoint(t), GetBezierOrientation(t));
    }

    Quaternion GetBezierOrientation(float t)
    {
        Vector3 tanget = GetBezierTangent(t);
        return Quaternion.LookRotation(tanget);
    }

    Vector3 GetBezierOrientedPoint(float t)
    {
        Vector3[] pts = new Vector3[4];
        for (int i = 0; i < 4; i++) pts[i] = GetPos(i);
        float omt = 1f-t;
        float omt2 = omt * omt;
        float t2 = t * t;
        return	pts[0] * ( omt2 * omt ) +
                pts[1] * ( 3f * omt2 * t ) +
                pts[2] * ( 3f * omt * t2 ) +
                pts[3] * ( t2 * t );
    }

    Vector3 GetBezierTangent(float t )
    {
        Vector3[] pts = new Vector3[4];
        for (int i = 0; i < 4; i++) pts[i] = GetPos(i);
        float omt = 1f-t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 tangent = pts[0] * ( -omt2 ) +
                          pts[1] * ( 3 * omt2 - 2 * omt ) +
                          pts[2] * ( -3 * t2 + 2 * t ) +
                          pts[3] * ( t2 );
        return tangent.normalized;
    }
}
