using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour {

	public int numberOfParticles = 3;
	public GameObject circlePrefab;
	public GameObject prevCirclePrefab;
	public GameObject linkPrefab;
	public Button startButton;
	public Text title;
	public Toggle toggle;
	public Image arrow;

	private bool isStarted = false;
	private Composite composite;
	private IEnumerator coroutine;


	private List<GameObject> circles = new List<GameObject>();
	private List<GameObject> prevCircles = new List<GameObject>();
	private List<GameObject> links = new List<GameObject>();
	private Verlet verlet = new Verlet(3, 0.98f, -100f);
	private List<Vector3> startingPositions = new List<Vector3>(); 
	// Use this for initialization
	void Start () {

		var sd = GetComponent<RectTransform>().sizeDelta;

		for (int i = 0; i < numberOfParticles; ++i) {
			GameObject circle = Instantiate(circlePrefab);
			circle.GetComponentInChildren<Text>().text = (i + 1).ToString();

			circle.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, sd.y / 2 - i * sd.y / (numberOfParticles - 1), 0);
			circles.Add(circle);

			GameObject prevCircle = Instantiate(prevCirclePrefab);
			prevCircle.GetComponentInChildren<Text>().text = (i + 1).ToString();
			prevCircle.GetComponent<RectTransform>().anchoredPosition = circle.GetComponent<RectTransform>().anchoredPosition;
			prevCircles.Add(prevCircle);



			if (i > 0) {
				GameObject link = Instantiate(linkPrefab);

				link.transform.position = circle.transform.position;
				links.Add(link);
			}
		}


		foreach (var link in links) {
			link.transform.SetParent(this.transform, false);
		}

		foreach (var prevCircle in prevCircles) {
			prevCircle.transform.SetParent(this.transform, false);
		}

		foreach (var circle in circles) {
			circle.transform.SetParent(this.transform, false);
		}

		composite = new Composite();

		for (int i = 0; i < circles.Count; ++i) {
			Particle p = new Particle(circles[i].transform.position);
			composite.particles.Add(p);
			startingPositions.Add(p.pos);
		}

		for (int i = 1; i < composite.particles.Count; ++i) {
			DistanceConstraint dc = new DistanceConstraint(composite.particles[i - 1], composite.particles[i], 1f);
			composite.constraints.Add(dc);
		}

		PinConstraint pc = new PinConstraint(composite.particles[0]);
		composite.constraints.Add(pc);

		verlet.composites.Add(composite);

		startButton.onClick.AddListener( OnStart);
	}

	void OnStart() {



		if (!isStarted) {

			Vector2 ap = circles[circles.Count - 1].GetComponent<RectTransform>().anchoredPosition;
			ap.x += 200;
			circles[circles.Count - 1].GetComponent<RectTransform>().anchoredPosition = ap;
			UpdateLinks();

			for (int i = 0 ; i < circles.Count; ++i) {
				verlet.composites[0].particles[i].pos = circles[i].transform.position;
			
			}

			isStarted = true;

			if (toggle.isOn) {
				coroutine = VerletCo();
				StartCoroutine(coroutine);
			}

			toggle.enabled = false;
			startButton.GetComponentInChildren<Text>().text = "Stop";

		}
		else {
			arrow.gameObject.SetActive(false);
			if (coroutine != null) {
				StopCoroutine(coroutine);
				coroutine = null;
			}
			isStarted = false;
			toggle.enabled = true;

			startButton.GetComponentInChildren<Text>().text = "Start";

			for (int i = 0; i < startingPositions.Count; ++i) {
				composite.particles[i].pos = startingPositions[i];
				composite.particles[i].lastPos = startingPositions[i];


			}

			foreach (var link in links) {
				link.GetComponent<Image>().color = linkPrefab.GetComponent<Image>().color;
			}

			UpdateView();
			UpdateLinks();
		}
	}



	IEnumerator VerletCo() {
		Debug.Break();
		yield return null;
		while (true) {
			for (int j = 0; j < verlet.stiffness; ++j) {
				for (int i =  0; i < composite.constraints.Count; i++) {
					IConstraint constraint = composite.constraints[i];
					if (constraint is DistanceConstraint) {
						DistanceConstraint dc = constraint as DistanceConstraint;
						int i1 = composite.particles.IndexOf(dc.p1);
						int i2 = composite.particles.IndexOf(dc.p2);
						title.text = "Iteration:" + (j + 1) + "/" + verlet.stiffness + " Relax nodes: " + (i1 + 1) + " and " + (i2 + 1);

						links[i].GetComponent<Image>().color = Color.red;
					}
					else if (constraint is PinConstraint) {
						PinConstraint pc = constraint as PinConstraint;
						int index = composite.particles.IndexOf(pc.p);
						title.text = "Iteration:" + (j + 1) + "/" + verlet.stiffness + " Pin node: " + (index + 1);
					}
					else {
						title.text = "";
					}
					constraint.Relax();
					UpdateView();
					UpdateLinks();
					yield return null;
					if (i < links.Count) {
						links[i].GetComponent<Image>().color = linkPrefab.GetComponent<Image>().color;;
					}
				}
			}

			for (int i = 0; i < verlet.composites[0].particles.Count; ++i) {
				title.text = "Move node: "  + (i + 1);
				Particle p = verlet.composites[0].particles[i];
				p.Move(verlet.friction, verlet.gravity);
				arrow.gameObject.SetActive(true);
				arrow.transform.position = p.pos;
				arrow.transform.rotation = Quaternion.LookRotation(Vector3.back, p.pos - p.lastPos);
				UpdateView();
				UpdateLinks();
				yield return null;
				arrow.gameObject.SetActive(false);
			}
		}

	}



	void UpdateView() {
		for (int i = 0 ; i < circles.Count; ++i) {
			prevCircles[i].transform.position = composite.particles[i].lastPos;
			circles[i].transform.position = composite.particles[i].pos;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!isStarted) {
			UpdateLinks();
		}

		if (isStarted && !toggle.isOn) {

			verlet.Update();
			UpdateView();
			UpdateLinks();
		}

	}

	void UpdateLinks() {
		for (int i = 0; i < links.Count; ++i) {
			links[i].transform.position = circles[i].transform.position;
			Vector3 diff = circles[i + 1].transform.position - circles[i].transform.position;
			links[i].transform.rotation = Quaternion.LookRotation(Vector3.back, diff);
			var sd = links[i].GetComponent<RectTransform>().sizeDelta;
			sd.y = (circles[i + 1].GetComponent<RectTransform>().anchoredPosition - circles[i].GetComponent<RectTransform>().anchoredPosition).magnitude;
			links[i].GetComponent<RectTransform>().sizeDelta = sd;
		}
	}
}
