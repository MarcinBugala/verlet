using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

	public ClothComponent clothComponent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Vector3 mp = Input.mousePosition;
			mp.z = clothComponent.gameObject.transform.position.z - Camera.main.transform.position.z;
			Vector3 pos = Camera.main.ScreenToWorldPoint(mp);
			pos = clothComponent.transform.InverseTransformPoint(pos);
			clothComponent.cloth.SetNearestParticle(pos, 3);

		}

		if (Input.GetMouseButton(0)) {
			
			Vector3 mp = Input.mousePosition;
			mp.z = clothComponent.gameObject.transform.position.z - Camera.main.transform.position.z;
			Vector3 pos = Camera.main.ScreenToWorldPoint(mp);

			pos = clothComponent.transform.InverseTransformPoint(pos);
			clothComponent.cloth.SetMousePosition(pos);
		}

		if (Input.GetMouseButtonUp(0)) {
			clothComponent.cloth.OnMouseRelease();
		}
	}
}
