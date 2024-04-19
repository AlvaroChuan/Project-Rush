using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtrudeShape
{
    public Vector2[] vert2Ds = new Vector2[]{
        new Vector2(0.0f, 0.0f),
        new Vector2(0.5f, 0.0f)
    };
    public Vector2[] normals = new Vector2[]{
        new Vector2(0.0f, 1.0f),
        new Vector2(0.0f, 1.0f)
    };
    public float[] us = new float[]{
        0.0f,
        1.0f
    };
    public int[] lines = new int[]{
        1, 0
    };

}
