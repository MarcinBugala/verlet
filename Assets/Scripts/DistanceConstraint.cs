using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceConstraint : IConstraint {

	public Particle p1;
	public Particle p2;
	public float stiffness;
	public float distance;

	public DistanceConstraint(Particle p1, Particle p2, float stiffness) {
		this.p1 = p1;
		this.p2 = p2;
		this.stiffness = stiffness;
		this.distance = (p1.pos - p2.pos).magnitude;
	}

	public void Relax() {
		Vector3 diff = p2.pos - p1.pos;
		float m = diff.magnitude;
		float c =( (distance - m) / m ) / 2f * stiffness;
		diff *= c;
		p1.pos -= diff;
		p2.pos += diff;
	}
		
}
