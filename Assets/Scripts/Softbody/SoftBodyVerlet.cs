using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftBodyVerlet : MonoBehaviour
{

    [Range(2, 100)]
    public int segmentCount = 20;
    [Range(0.5f, 1f)]
    public float radius = 1f;
    [Range(1, 100)]
    public int solverIterations = 1;
    Vector2[] prevPoints, curPoints;
    Vector3[] accumDis;
    float segmentLength, area, circumfrence; //, segmentLengthSquared;


    private Vector3[] vertices;
    private GameObject[] gameObjects;


    public GameObject eyes;

    [Range(0f, 1f)]
    public float eyesLevel = 0f;

    private Vector2 gravity = new Vector2(0, -9.81f);
    private float rotation = 0f;

    private Vector2 force = Vector2.zero;

    void Start()
    { 
        gameObjects = new GameObject[segmentCount];
        prevPoints = new Vector2[segmentCount]; curPoints = new Vector2[segmentCount]; accumDis = new Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            prevPoints[i] = Quaternion.Euler(0, 0, ((float)i / segmentCount) * 360f) * Vector2.up;
            curPoints[i] = prevPoints[i];


            gameObjects[i] = new GameObject();
            gameObjects[i].name = "Particle_" + (i + 1);
          //  gameObjects[i].transform.parent = this.transform;
            Rigidbody2D rb = gameObjects[i].AddComponent<Rigidbody2D>();
            rb.position = curPoints[i];
            //rb.angularDrag = 0.5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CircleCollider2D collider = gameObjects[i].AddComponent<CircleCollider2D>();
            collider.radius = 0.01f;

        }



        vertices = new Vector3[segmentCount + 1];
        int[] triangles = new int[segmentCount * 3];

        vertices[0] = new Vector3(0, 0, 0);

        for (int i = 0; i < segmentCount; ++i)
        {
            vertices[i + 1] = new Vector3(curPoints[i].x, curPoints[i].y, 0);


            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 2;
            triangles[3 * i + 2] = i + 1;
        }

        triangles[triangles.Length - 2] = 1;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        GetComponent<MeshFilter>().mesh = mesh;

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotation += 1f;
            gravity = Quaternion.Euler(0, 0, rotation) * new Vector2(0, -9.81f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rotation -= 1f;
            gravity = Quaternion.Euler(0, 0, rotation) * new Vector2(0, -9.81f);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            force = -gravity.normalized * 75;
        }

        Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));

        Vector2 avg = Vector2.zero;
        for (int i = 0; i < segmentCount; ++i)
        {
            avg += curPoints[i];
        }
        avg /= segmentCount;

        Vector3 p = Camera.main.transform.position;
        p.x = avg.x;
        Camera.main.transform.position = p;

        Vector3 pos = this.transform.position;
        pos.x = avg.x;
        pos.y = avg.y;
        transform.position = pos;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        
        for (int i = 0; i < segmentCount; i++)
        {
           curPoints[i] = gameObjects[i].GetComponent<Rigidbody2D>().position;
           gameObjects[i].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }


        //Calculate relevant circle intrinsics...
        area = Mathf.PI * radius * radius;
        circumfrence = 2 * Mathf.PI * radius;
        segmentLength = circumfrence / curPoints.Length;
        float segmentLengthSquared = segmentLength * segmentLength;



        //Integrate the points into the future
        for (int i = 0; i < curPoints.Length; i++)
        {
            Vector2 tempPos = curPoints[i];
            curPoints[i] += (curPoints[i] - prevPoints[i]) + ( (gravity + force) * Time.fixedDeltaTime * Time.fixedDeltaTime) ;
            prevPoints[i] = tempPos;
        }

        force = Vector2.zero;

        //Accumulate displacements from constraints in accumulation buffer
        for (int k = 0; k < solverIterations; k++)
        {
            for (int i = 0; i < curPoints.Length; i++)
            {
                int j = (i == 0 ? curPoints.Length - 1 : i - 1);
                Vector3 offset = (curPoints[j] - curPoints[i]);
                //offset *= segmentLengthSquared / (Vector3.Dot(offset, offset) + segmentLengthSquared) - 0.5f; //Approximation; makes it squishier
                offset *= ((segmentLength - offset.magnitude) / offset.magnitude) * 0.5f; //Exact solution; more expensive, makes it more solid
                accumDis[i] += new Vector3(-offset.x, -offset.y, 1f);
                accumDis[j] += new Vector3(offset.x, offset.y, 1f);
            }

            //Calculate area of polygon and compare to desired area
            float sum = 0f;
            for (int i = 0; i < curPoints.Length; i++)
            {
                int j = (i == 0 ? curPoints.Length - 1 : i - 1);
                sum += (curPoints[i].x + curPoints[j].x) * (curPoints[i].y - curPoints[j].y);
            }
            float deltaArea = (sum * 0.5f < area * 2f) ? area - (sum * 0.5f) : 0f; //Explosion resistance
            float dilationDistance =  deltaArea / circumfrence;
            //Dilate the polygon by the distance required to acheieve the desired area
            for (int i = 0; i < curPoints.Length; i++)
            {
                float d = (curPoints[i == 0 ? curPoints.Length - 1 : i - 1] - curPoints[i == curPoints.Length - 1 ? 0 : i + 1]).magnitude;
                Vector2 normal = Vector3.Cross(Vector3.forward, curPoints[i == 0 ? curPoints.Length - 1 : i - 1] -
                                                                curPoints[i == curPoints.Length - 1 ? 0 : i + 1]).normalized;
                 accumDis[i] += (Vector3)(normal * dilationDistance) + Vector3.forward;
            }

            //Apply constraints
            for (int i = 0; i < curPoints.Length; i++)
            {
               // if (accumDis[i] != Vector3.zero) { accumDis[i] /= accumDis[i][2]; }
                curPoints[i] += new Vector2(accumDis[i][0], accumDis[i][1]);
                accumDis[i] = Vector3.zero;
            }

        }

        for (int i = 0; i < segmentCount; i++)
        {
            gameObjects[i].GetComponent<Rigidbody2D>().MovePosition(curPoints[i]);
        }

        RedrawMesh();
    }



    void RedrawMesh()
    {
        Vector3 avg = Vector3.zero;
        for (int i = 0; i < segmentCount; ++i)
        {
            vertices[i + 1] = transform.InverseTransformPoint(new Vector3(curPoints[i].x, curPoints[i].y));
            avg += vertices[i + 1];
        }

        avg /= segmentCount;
        vertices[0] = avg;
        GetComponent<MeshFilter>().mesh.vertices = vertices;
    }


    void OnDrawGizmos()
    {
        if (curPoints != null && curPoints.Length > 0)
        {
            for (int i = 0; i < curPoints.Length; i++)
            {
                Gizmos.DrawSphere(curPoints[i], 0.01f);
            }

            for (int i = 0; i < curPoints.Length; i++)
            {
                Vector2 normal = Vector3.Cross(Vector3.forward, curPoints[i == 0 ? curPoints.Length - 1 : i - 1] -
                                                                curPoints[i == curPoints.Length - 1 ? 0 : i + 1]).normalized;
                Gizmos.DrawLine(curPoints[i], curPoints[i] + normal * 0.5f); //Draw the normals of the polygon
            }
        }
    }
}
