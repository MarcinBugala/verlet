using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloth : Verlet {

	private Mesh mesh = new Mesh();
	private int segments;
	private float width;
	private float height;

	public MeshFilter meshFilter;
	private List<Color> colors = new List<Color>();

	public Color color1 = Color.red;
	public Color color2 = Color.blue;

	public Cloth(Vector3 origin, float width, float height, int segments, int pinMod, float stiffness) : base(12, 1, -0.981f)  {
		
		this.segments = segments;
		this.width = width;
		this.height = height;
		Composite composite = new Composite();

		float xStride = width / segments;
		float yStride = height / segments;

		for (int y = 0; y < segments; ++y) {
			for (int x = 0; x < segments; ++x) {
				float px =  origin.x + x * xStride - width / 2;
				float py = 	origin.y -y * yStride;
				composite.particles.Add(new Particle(new Vector2(px, py)));
				colors.Add(new Color(Random.value, Random.value, Random.value));

				if (x > 0) {
					composite.constraints.Add(new DistanceConstraint(composite.particles[y * segments + x], composite.particles[y * segments + x - 1], stiffness));
				}

				if (y > 0) {
					composite.constraints.Add(new DistanceConstraint(composite.particles[y * segments + x], composite.particles[ (y - 1) * segments + x], stiffness));
				}
			}
		}

		for (int x = 0; x < segments;x++) {
			if (x % pinMod == 0) {
				composite.constraints.Add(new PinConstraint(composite.particles[x]));
			}
		}

		this.composites.Add(composite);

	}

	override public void Update() {
		base.Update();

		CreateMesh();
	}


	private void CreateMesh() {
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Color> colors = new List<Color>();

		foreach (Composite c in composites) {
			for (int i = 0; i + 1< segments; ++i) {
				for (int j = 0; j + 1 < segments; ++j) {
					int ind = i + j * segments;

					int verts = vertices.Count;

					vertices.Add(c.particles[ind].pos);
					vertices.Add(c.particles[ind + 1].pos);
					vertices.Add(c.particles[ind + segments].pos);
					vertices.Add(c.particles[ind + segments + 1].pos);

					triangles.Add(verts + 0);
					triangles.Add(verts + 1);
					triangles.Add(verts + 2);

					triangles.Add(verts + 1);
					triangles.Add(verts + 3);
					triangles.Add(verts + 2);

					Vector2 d = c.particles[ind + segments + 1].pos - c.particles[ind].pos;
					float off = (d.x + d.y) * 0.5f;
					float stride = Mathf.Min(width, height) * 0.5f / segments;
					float coeff = Mathf.Abs(off) / stride;
					Color col = Color.Lerp(color1, color2, coeff);
					colors.Add(col);
					colors.Add(col);
					colors.Add(col);
					colors.Add(col);

				}
			}
		}


		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetColors(colors);

		meshFilter.mesh = mesh;
	}


	public void SetNearestParticle(Vector3 pos, float radius) {
		float min = float.PositiveInfinity;
		Particle result = null;
		foreach (Composite composite in composites) {
			foreach (Particle p in composite.particles) {
				float d = (p.pos - pos).magnitude;
				if ( d < min) {
					min = d;
					result = p;
				}
			}
		}

		nearestParticle = result;
	}

	public void SetMousePosition(Vector3 pos) {
		mousePos = pos;
	}

	public void OnMouseRelease() {
		nearestParticle = null;
	}
}
