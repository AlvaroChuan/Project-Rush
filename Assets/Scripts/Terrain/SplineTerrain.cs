using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineTerrain : MonoBehaviour
{

    private MeshFilter mf;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uvs;
    private int[] triangleIndices;
    private Vector3[] splinePoints;
    [SerializeField] private ExtrudeShape shape;
    [SerializeField] private SplineContainer splineContainer;
    private Spline spline;
    public int segments_per_unit;
    private OrientedPoint[] path;

    void Awake()
    {
        //Generate mesh components
        mf = gameObject.GetComponent<MeshFilter>();
        if( mf.sharedMesh == null ) mf.sharedMesh = new Mesh();
        Mesh mesh = mf.sharedMesh;
        spline = splineContainer.Splines[0];

        //Get the spline points
        splinePoints = new Vector3[4];
        // splinePoints[0] = spline.Knots.ElementAt(0).Position;
        // splinePoints[1] = splinePoints[0] + new Vector3(spline.Knots.ElementAt(0).TangentOut.x,
        //                                                 spline.Knots.ElementAt(0).TangentOut.y,
        //                                                 spline.Knots.ElementAt(0).TangentOut.z);
        // splinePoints[3] = spline.Knots.ElementAt(1).Position;
        // splinePoints[2] = splinePoints[3] + new Vector3(spline.Knots.ElementAt(1).TangentIn.x,
        //                                                 spline.Knots.ElementAt(1).TangentIn.y,
        //                                                 spline.Knots.ElementAt(1).TangentIn.z);
        Debug.Log(splinePoints[0]);
        Debug.Log(splinePoints[1]);
        Debug.Log(splinePoints[2]);
        Debug.Log(splinePoints[3]);

        //Calculate the oriented points
        path = new OrientedPoint[ segments_per_unit ];
        for (int i = 0; i < segments_per_unit; i++)
        {
        //    path[i] = new OrientedPoint(GetPoint(splinePoints, i), GetOrientation3D(splinePoints, i, Vector3.up));
        }

        //Generate the mesh
        shape = new ExtrudeShape();
        Extrude( mesh, shape, path );
    }

    OrientedPoint GetPoint(Vector3[] pts, float t)
    {
        float omt = 1f-t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 positiont =	pts[0] * ( omt2 * omt ) +
                            pts[1] * ( 3f * omt2 * t ) +
                            pts[2] * ( 3f * omt * t2 ) +
                            pts[3] * ( t2 * t );
        Vector3 tangent = pts[0] * ( -omt2 ) +
                          pts[1] * ( 3 * omt2 - 2 * omt ) +
                          pts[2] * ( -3 * t2 + 2 * t ) +
                          pts[3] * ( t2 );
        return new OrientedPoint(positiont, tangent.normalized);
    }

    Vector3 GetTangent( Vector3[] pts, float t )
    {
        float omt = 1f-t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 tangent = pts[0] * ( -omt2 ) +
                          pts[1] * ( 3 * omt2 - 2 * omt ) +
                          pts[2] * ( -3 * t2 + 2 * t ) +
                          pts[3] * ( t2 );
        return tangent.normalized;
    }

    Vector3 GetNormal2D( Vector3[] pts, float t )
    {
        Vector3 tng = GetTangent( pts, t );
        return new Vector3( -tng.y, tng.x, 0f );
    }

    Vector3 GetNormal3D( Vector3[] pts, float t, Vector3 up )
    {
        Vector3 tng = GetTangent( pts, t );
        Vector3 binormal = Vector3.Cross( up, tng ).normalized;
        return Vector3.Cross( tng, binormal );
    }

    Quaternion GetOrientation2D( Vector3[] pts, float t )
    {
        Vector3 tng = GetTangent( pts, t );
        Vector3 nrm = GetNormal2D( pts, t );
        return Quaternion.LookRotation( tng, nrm );
    }

    Quaternion GetOrientation3D( Vector3[] pts, float t, Vector3 up )
    {
        Vector3 tng = GetTangent( pts, t );
        Vector3 nrm = GetNormal3D( pts, t, up );
        return Quaternion.LookRotation( tng, nrm );
    }

    public void Extrude( Mesh mesh, ExtrudeShape shape, OrientedPoint[] path )
    {
        int vertsInShape = shape.vert2Ds.Length;
        int segments = path.Length - 1;
        int edgeLoops = path.Length;
        int vertCount = vertsInShape * edgeLoops;
        int triCount = shape.lines.Length * segments;
        int triIndexCount = triCount * 3;

        int[] triangleIndices 	= new int[ triIndexCount ];
        Vector3[] vertices 	    = new Vector3[ vertCount ];
        Vector3[] normals 		= new Vector3[ vertCount ];
        Vector2[] uvs 			= new Vector2[ vertCount ];

        /*
        foreach oriented point in the path
            foreach vertex in the 2D shape
                Add the vertex position, based on the oriented point
                Add the normal direction, based on the oriented point
                Add the UV. U is based on the shape, V is based on distance along the path
            end
        end
        */

        for( int i = 0; i < path.Length; i++ )
        {
            int offset = i * vertsInShape;
            for( int j = 0; j < vertsInShape; j++ )
            {
                int id = offset + j;
                vertices[id] = path[i].LocalToWorld( shape.vert2Ds[j] );
                normals[id] = path[i].LocalToWorldDirection( shape.normals[j] );
                uvs[id] = new Vector2( shape.us[j], i / ((float)edgeLoops) );
		    }
	    }

        /*
        foreach segment
            foreach line in the 2D shape
                Add two triangles with vertex indices based on the line indices
            end
        end
        */

	    int ti = 0;

        for( int i = 0; i < segments; i++ )
        {
            int offset = i * vertsInShape;
            for ( int l = 0; l < shape.lines.Length; l += 2 )
            {
                int a = offset + shape.lines[l] + vertsInShape;
                int b = offset + shape.lines[l];
                int c = offset + shape.lines[l+1];
                int d = offset + shape.lines[l+1] + vertsInShape;
                triangleIndices[ti] = a; 	ti++;
                triangleIndices[ti] = b; 	ti++;
                triangleIndices[ti] = c; 	ti++;
                triangleIndices[ti] = c; 	ti++;
                triangleIndices[ti] = d; 	ti++;
                triangleIndices[ti] = a; 	ti++;
            }
        }

        /* Generation code goes here */

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        mesh.normals = normals;
        mesh.uv = uvs;
    }
}
