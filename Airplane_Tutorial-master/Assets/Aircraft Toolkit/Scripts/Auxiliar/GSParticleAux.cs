using UnityEngine;
using System.Collections;

public class GSParticleAux: MonoBehaviour {

	private GAircraft gAircraft = null;

	void Start() {
		gAircraft = GAircraft.findGAircraft(gameObject, 29);
	}
	
	void Update() {
		gameObject.GetComponent<ParticleSystem>().emissionRate = Mathf.FloorToInt(100.0f * gAircraft.inputThrottle_output);
	}
}
