using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]public class GSRotorBreakAux: MonoBehaviour {
	[HideInInspector]public Vector3 startLocalPosition = Vector3.zero;
	public GAircraft parentGAircraft = null;
	public bool debugMessagesEnabled = false;
	
	void Start() {
		startLocalPosition = gameObject.transform.localPosition;
		Transform transform = gameObject.transform;
		int maxLoops = 99;
		if (parentGAircraft == null) while (transform != null) {
			--maxLoops; if (maxLoops < 0) break;
			if (transform.gameObject.GetComponent("GAircraft")) {
				parentGAircraft = (GAircraft)transform.gameObject.GetComponent("GAircraft");
				break;
			}
			transform = transform.parent;
		}
	}
	
	void FixedUpdate() {
		gameObject.transform.localPosition = startLocalPosition;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
	}
	
	void OnCollisionEnter(Collision collision) {
		if (debugMessagesEnabled) Debug.Log("rotor empieza colision" + Time.realtimeSinceStartup.ToString());
		if (parentGAircraft != null) {
			parentGAircraft.isCrashed = true;
		}
	}

	void OnCollisionStay(Collision collision) {
		if (debugMessagesEnabled) Debug.Log("rotor sigue colisionando" + Time.realtimeSinceStartup.ToString());
	}

	void OnCollisionExit(Collision collision) {
		if (debugMessagesEnabled) Debug.Log("rotor deja de colisionar" + Time.realtimeSinceStartup.ToString());
	}
}
