using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle : MonoBehaviour
{

    public float width = 3f;
    public float height = 1f;

    Vector3[] vertices;

    // Start is called before the first frame update
    void Start()
    {
        vertices = new Vector3[4];
        vertices[0] = new Vector3(-width / 2, -height / 2, 0);
        vertices[1] = new Vector3(width / 2, -height / 2, 0);
        vertices[2] = new Vector3(width / 2, height / 2, 0);
        vertices[3] = new Vector3(-width / 2, height / 2, 0);

        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }


    public bool Inside(Vector2 p)
    {
      
        for (int i = 0; i < vertices.Length; ++i)
        {
            Vector3 p1 = transform.TransformPoint(vertices[i]);
            Vector3 p2 = transform.TransformPoint(vertices[(i + 1) % vertices.Length]);


            Vector2 v1 = new Vector2(p.x - p1.x, p.y - p1.y);
            Vector2 v2 = new Vector2(p2.x - p1.x, p2.y - p1.y);

            if ( v2.x * v1.y - v2.y * v1.x < 0)
            {
                return false;
            }
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
