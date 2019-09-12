using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Composite {

	public List<Particle> particles = new List<Particle>();
	public List<IConstraint> constraints = new List<IConstraint>();

}
