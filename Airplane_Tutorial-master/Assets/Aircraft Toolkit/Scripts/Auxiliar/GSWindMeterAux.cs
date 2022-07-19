using UnityEngine;
using System.Collections;

public class GSWindMeterAux: MonoBehaviour {
	float t = 0.0f;
	
	public string searchPivotObjectName = "WindMeterPivot";
	public string searchScalerObjectName = "WindMeterScaler";
	public string searchNodeObjectName = "WindMeterNode";
	private GameObject pivot = null;
	private GameObject scaler = null;
	private GameObject node = null;
	public float pangle1 = 1.3f;
	public float pangle2 = 2.3f;
	public float pangle3 = 3.3f;
	public float vangle = 30.0f;
	public float vscale = 0.0f;
	public float globalSimulationScale = 1.0f;
	//private Vector3 neg_windspeed = Vector3.zero;
	private Vector3 neg_windspeed_filtered = Vector3.zero;
	private float neg_windspeed_filter = 0.01f;
	private Vector3 scaler_localScale = Vector3.zero;
	private Vector3 node_localEulerAngles = Vector3.zero;
	private Quaternion pivot_rotation = Quaternion.identity;
	
	void Start() {
		pivot = GameObject.Find(searchPivotObjectName);
		scaler = GameObject.Find(searchScalerObjectName);
		node = GameObject.Find(searchNodeObjectName);
	}
	
	void Update() {
		t += Time.fixedDeltaTime;
		
		if ((pivot != null) && (scaler != null) && (node != null)) {
			neg_windspeed_filtered = neg_windspeed_filtered * (1f - neg_windspeed_filter) + (-GSurfaceWindZone.windAt(gameObject.transform.position) / globalSimulationScale) * neg_windspeed_filter;
			float bias = Mathf.Pow(pangle1, -10.0f / neg_windspeed_filtered.magnitude) * (80.0f + 10.0f * Mathf.Sin(t * 2.0f));
			float noise1 = Mathf.Pow(pangle2, -10.0f / neg_windspeed_filtered.magnitude) * 6.0f * Mathf.Sin(t * 10.0f);
			float noise2 = Mathf.Pow(pangle3, -10.0f / neg_windspeed_filtered.magnitude) * 3.0f * Mathf.Sin(t * 100.0f);
			vangle = bias + noise1 + noise2;
			
			scaler_localScale.Set(1.0f, 1.0f, Mathf.Sin(vangle * 0.01745329f) + vscale);
			node_localEulerAngles.Set(-5.0f + 55.0f * Mathf.Cos(vangle * 0.01745329f), 0.0f, 0.0f);
			scaler.transform.localScale = scaler_localScale;
			node.transform.localEulerAngles = node_localEulerAngles;

			if ((neg_windspeed_filtered.x != 0) || (neg_windspeed_filtered.y != 0) || (neg_windspeed_filtered.z != 0)) {
				pivot_rotation.SetLookRotation(-neg_windspeed_filtered);
			} else {
				pivot_rotation.eulerAngles.Set(0.0f, 90.0f, 0.0f);
			}
			pivot.transform.rotation = pivot_rotation;
		}
	}
}
