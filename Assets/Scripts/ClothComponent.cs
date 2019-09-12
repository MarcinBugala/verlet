using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothComponent : MonoBehaviour {

	public Cloth cloth;
	[Range(0.8f, 1f)]
	public float friction = 1f;
	[Range(1, 12)]
	public int stiffness = 12;

	public Color color1;
	public Color color2;

	// Use this for initialization
	void Start () {
		cloth = new Cloth(Vector3.zero, 10, 10, 19, 6, 0.8f);
		cloth.meshFilter = this.GetComponent<MeshFilter>();
	}
	
	// Update is called once per frame
	void Update () {
		cloth.Update();
		cloth.SetFriction(friction);
		cloth.SetStiffness(stiffness);
		cloth.color1 = color1;
		cloth.color2 = color2;
	}
}
