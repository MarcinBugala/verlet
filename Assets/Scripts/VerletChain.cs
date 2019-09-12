using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletChain : MonoBehaviour {

	class Point {
		public Vector3 pos;
		public Vector3 lastPos;
		public GameObject g;
		public GameObject follow;
		public bool isFixed = false;
	}

	public const float GRAV = -0.09f;
	public const float GROUND = 0f;
	public const float GROUND_BOUNCE = 0.5f;
	public const float DRAG = 0.9f;
	public const float FRIC = 0.999f;


	public int stiffness = 12;
	public Vector3 dumping = new Vector3(0.5f, 0.96f, 0.5f);

	private List<Point> points = new List<Point>();

	public int pointCount = 50;
	public float lineLength = 10f;
	private float segmentLength = 1f;


	public GameObject chainPrefab;
	public GameObject chainStart;
	public GameObject chainEnd;
	// Use this for initialization
	void Start () {
		int segments = pointCount - 1;
		segmentLength = lineLength / (float)segments;

		Vector3 d = chainEnd.transform.position - chainStart.transform.position;


		for (int i = 0; i < pointCount; i++) {
			GameObject link = Instantiate(chainPrefab, Vector3.zero, Quaternion.identity);
			link.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
			link.transform.localScale =  Vector3.one * lineLength / (segments  * 2);
			link.transform.parent = this.transform;

			float p = (float)i / (float)pointCount;
			Vector3 pos = chainStart.transform.position + (p * d);

			Point point = new Point();
			point.pos = pos;
			point.lastPos = pos;
			point.g = link;

			link.name = "Link_" + i.ToString();
			points.Add(point);

			if (i == 0) {
				point.isFixed = true;
				point.follow = chainStart;
			}
			else if (i + 1 == pointCount) {
				point.isFixed = true;
				point.follow = chainEnd;
			} 
			else {
				point.isFixed = false;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		MovePoints();
		ConstraintPoints();

		for (int i = 0; i < stiffness; ++i) {
			ConstraintLines();
		}
			
		foreach (var point in points) {
			point.g.transform.position = point.pos;
		}

		SetAngles();
	}


	void MovePoints() {
		foreach (var point in points) {
			MovePoint(point);
		}
	}

	void MovePoint(Point p) {
		if (p.isFixed) {
			p.pos = p.lastPos = p.follow.transform.position;
			return;
		}

		Vector3 d = (p.pos - p.lastPos) * DRAG;
	
		p.lastPos = p.pos;
		p.pos += d;
		p.pos.y += GRAV * Time.deltaTime;
	}

	void ConstraintPoints() {
		foreach (var point in points) {
			ConstraintPoint(point);
		}
	}

	void ConstraintPoint(Point p) {
		if (p.pos.y < GROUND) {
			Vector3 d = (p.pos - p.lastPos) * DRAG;
			float speed = d.magnitude;
			p.pos.y = GROUND;
			p.lastPos.y = GROUND + d.y * GROUND_BOUNCE;
			p.lastPos.x += d.x * FRIC;

		}
	}

	void ConstraintLines() {
		for (int i = 0; i + 1 < points.Count; ++i) {
			ConstraintLine(points[i], points[i + 1]);
		}
	}

	void ConstraintLine(Point p1, Point p2) {
		Vector3 d = p2.pos - p1.pos;
		float dist = d.magnitude;


		float fraction = ((segmentLength - dist) / dist ) / 2f;

		d *= fraction;

		if (p2.isFixed) {
			if (!p1.isFixed) {
				p1.pos -= 2 * d;
			}
		}
		else if (p1.isFixed) {
			if (!p2.isFixed) {
				p2.pos += 2 * d;
			}
		}
		else {
			p1.pos -= d;
			p2.pos += d;
		}
	}

	void SetAngles() {
		for (int i = 1; i + 1 < points.Count; ++i) {
			var diff = points[i + 1].pos - points[i - 1].pos;
			points[i].g.transform.rotation = Quaternion.LookRotation(diff) * Quaternion.Euler(new Vector3(90, 0, 90));
			if (i % 2 == 1) {
				points[i].g.transform.rotation *= Quaternion.Euler(new Vector3(90, 0, 0));
			}
		}
	}
}
