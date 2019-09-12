using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinConstraint : IConstraint {

	public Particle p;
	public Vector2 pos;

	public PinConstraint(Particle p) {
		this.p = p;
		this.pos = p.pos;
	}

	public void Relax() {
		p.pos = pos;	
	}
}
