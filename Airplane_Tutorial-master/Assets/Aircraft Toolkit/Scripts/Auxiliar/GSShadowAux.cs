using UnityEngine;
using System.Collections;

public class GSShadowAux: MonoBehaviour {

	private GameObject shadowcontainer;
	private GameObject shadow;
	private GameObject sunlight;

	public LayerMask searchShadowProjectsOverLayerMask = -24;
	public string searchShadowLightDirectionObjectName = "Sunlight";
	public string searchShadowContainerObjectName = "ShadowContainer";
	public string searchShadowChildObjectName = "Shadow";
	public float searchShadowAlpha = 0.2f;
	public float searchShadowSize = 10.0f;
	public float searchShadowSizeY = 0.33f;
	public float searchShadowDistanceMin = 25.0f;
	public float searchShadowDistanceMid = 125.0f;
	public float searchShadowDistanceMax = 1025.0f;
	public bool searchShadowProCompat = false;
	public float searchShadowDistanceMinFromCamera = 130.0f;
	public float searchShadowDistanceMinFromCameraTransition = 15.0f;
	private Vector3 shadow_scale;	
	
	void Start() {
		shadowcontainer = GameObject.Find(searchShadowContainerObjectName);
		shadow = GameObject.Find(searchShadowChildObjectName);
		sunlight = GameObject.Find(searchShadowLightDirectionObjectName);
		shadow_scale = new Vector3(searchShadowSize, searchShadowSize * searchShadowSizeY, searchShadowSize);
	}
	
	void FixedUpdate() {
		if ((shadow != null) && (shadowcontainer != null) && (sunlight != null)) {
			Vector3 originpos;
			Vector3 vectorpos;
			RaycastHit hit;
			originpos = gameObject.transform.position;
			vectorpos = sunlight.transform.forward;
			if (Physics.Raycast(originpos + vectorpos * searchShadowDistanceMin, vectorpos, out hit, 999999999.9f, searchShadowProjectsOverLayerMask)) {
				shadow.transform.localEulerAngles = gameObject.transform.localEulerAngles;
				shadowcontainer.transform.localScale = shadow_scale;
				shadowcontainer.transform.position = hit.point - vectorpos * 0.1f;
				float hitDistanceFromCamera = (shadowcontainer.transform.position - Camera.main.transform.position).magnitude;
				float alfa, beta;
				if (hit.distance < searchShadowDistanceMid) {
					alfa = (hit.distance - searchShadowDistanceMin) / (searchShadowDistanceMid - searchShadowDistanceMin);
				} else if (hit.distance < searchShadowDistanceMax) {
					alfa = (searchShadowDistanceMax - hit.distance) / (searchShadowDistanceMax - searchShadowDistanceMid);
				} else {
					alfa = 0.0f;
				}
				if (searchShadowProCompat) {
					if (hitDistanceFromCamera < searchShadowDistanceMinFromCamera) {
						beta = 0.0f;
					} else if (hitDistanceFromCamera < searchShadowDistanceMinFromCamera + searchShadowDistanceMinFromCameraTransition) {
						beta = (hitDistanceFromCamera - searchShadowDistanceMinFromCamera) / (searchShadowDistanceMinFromCameraTransition);
					} else {
						beta = 1.0f;
					}
					if (beta < alfa) alfa = beta;
				}
				
				if (alfa <= 0.0f) {
					shadow.GetComponent<Renderer>().enabled = false;
				} else {
					shadow.GetComponent<Renderer>().enabled = true;
					shadow.GetComponent<Renderer>().materials[0].SetFloat("_Alfa", 1.0f - alfa * searchShadowAlpha);
				}
			} else {
				shadow.GetComponent<Renderer>().enabled = false;
			}
		}
	}
}
