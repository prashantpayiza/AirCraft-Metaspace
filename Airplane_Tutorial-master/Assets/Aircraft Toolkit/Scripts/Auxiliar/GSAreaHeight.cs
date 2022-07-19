using UnityEngine;
using System.Collections;

public class GSAreaHeight: MonoBehaviour {
	
	private float baseHeight = 0.0f;
	public float coefHeight = 0.01f;
	public GameObject viewer = null;
	private Vector3 position = Vector3.zero;
	
	void Start() {
		baseHeight = gameObject.transform.position.y;
	}
	
	void Update() {
		if (viewer != null) {
			position = gameObject.transform.position;
			if (viewer.transform.position.y > baseHeight) {
				position.y = baseHeight + (viewer.transform.position.y - baseHeight) * coefHeight;
			} else {
				position.y = baseHeight;
			}
			gameObject.transform.position = position;
		}
	}
}
