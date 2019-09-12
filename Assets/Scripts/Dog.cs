using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour {

	public VerletChain chainSystem;
	public GameObject post;
	public float speed = 1f;
	public float rotationSpeed = 1f;
	private Rigidbody rigidbody;
	private Vector3 target;
	private bool targetSet = false;

	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.useGravity = false;
	}

	public void SetTarget(Vector3 target) {


		var d = target - post.transform.position;
		if (d.magnitude > chainSystem.lineLength) {
			d = d.normalized * (chainSystem.lineLength + 1);
			target = d + post.transform.position;
		}



		var v1 = transform.forward;
		var v2 = (target - transform.position).normalized;
		float cross = Mathf.Abs(v1.x * v2.z - v1.z * v2.x);

		this.target = target;
		targetSet = true;

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 100, 1 << LayerMask.NameToLayer("Terrain"))) {
				
				SetTarget(hit.point);
			}
		}

		if (targetSet) {
			Vector3 diff = target - transform.position;
			diff.y = 0;
			float distance = diff.magnitude;

			var diffFromDogHouse = new Vector3(transform.position.x - chainSystem.chainStart.transform.position.x, 0f, transform.position.z - chainSystem.chainStart.transform.position.z);
			if (distance > 0.1f ) {
				diff.Normalize();
				Vector3 newVelocity = diff * speed;

				Vector3 velocityChange = newVelocity - rigidbody.velocity;
				velocityChange.y = 0;


				rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

				diff.y = 0;
				Rotate(diff.normalized);

				GetComponent<Animator>().SetBool("Run", true);

			
			}
			else {
				Stop();
			}

		}
	}

	void Stop(bool canLieDown = true) {
		rigidbody.AddForce(-rigidbody.velocity, ForceMode.VelocityChange);

		rigidbody.angularVelocity = Vector3.zero;
		targetSet = false;
		GetComponent<Animator>().SetBool("Run", false);
		rigidbody.useGravity = false;

	}


	void Rotate(Vector3 forward) {
		RaycastHit hit;
		int mask = 1 << LayerMask.NameToLayer("Terrain");
		if (Physics.Raycast(this.transform.position + new Vector3(0, 0.1f, 0), Vector3.down, out hit, 3f, mask)) {
			

			var right = Vector3.Cross(hit.normal, forward);
			var myForward = Vector3.Cross(right, hit.normal);
			var targetRotation = Quaternion.LookRotation(myForward, hit.normal);
			rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, Time.deltaTime * rotationSpeed);
		}

	}
}
