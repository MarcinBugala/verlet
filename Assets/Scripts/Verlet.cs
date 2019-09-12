using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Verlet  {

	public int stiffness = 3;
	public float friction = 0.9f;
	public float gravity = 0;
	public List<Composite> composites = new List<Composite>();
	protected Particle nearestParticle;
	protected Vector3 mousePos;

	public Verlet(int stiffness, float friction, float gravity) {
		this.stiffness = stiffness;
		this.friction = friction;
		this.gravity = gravity;
	}

	public void SetFriction(float value) {
		this.friction = value;
	}

	public void SetStiffness(int value) {
		this.stiffness = value;
	}

	virtual public void Update() {

		foreach (Composite c in composites) {
			foreach (Particle p in c.particles) {
				p.Move(friction, gravity);
			}

			if (nearestParticle != null) {
				nearestParticle.pos = mousePos;
			}
				
			for (int i = 0; i < stiffness; i++) {
				foreach (IConstraint constraint in c.constraints) {
					constraint.Relax();
				}
			}
		}


	}

}
