using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle {

	public Vector3 pos;
	public Vector3 lastPos;

	public Particle(Vector3 pos) {
		this.pos = pos;
		this.lastPos = pos;
	}

	public void Move(float friction, float gravity) {
		Vector3 d = (pos - lastPos) * friction;

		lastPos = pos;
		pos += d;
		pos.y += gravity * Time.deltaTime;
	}
}
