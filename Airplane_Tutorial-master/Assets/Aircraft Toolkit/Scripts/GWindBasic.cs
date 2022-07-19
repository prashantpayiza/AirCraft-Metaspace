using UnityEngine;
using System.Collections;

public class GWindBasic: MonoBehaviour, GWindInterface {
	
	public static GWindInterface windManager = null;
	public float biasSpeed = 1.0f;
	public float gust1Speed = 2.0f;
	public float gust1TimesPerSecond = 0.1f;
	public float gust2Speed = 2.0f;
	public float gust2TimesPerSecond = 0.03f;
	private float gust_t = 0.0f;
	private static Vector3 globalWindSpeed;
	private static Vector3 blowSource = Vector3.zero;
	private static float blowPeak = 0.0f;
	private static float blowAmmount = 0.0f;
	private static float blowDuration = 0.0f;
	private static float blowTime = 1.0f;
	private static float blowLastTime = 0.0f;
	private static float blowFrequency = 1.0f;
	private static float blowDistanceExponent = 1.5f;
	
	float calcWindForce() {
		return biasSpeed + Mathf.Sin(gust_t * gust1TimesPerSecond) * (gust1Speed - biasSpeed) / 2.0f + (gust1Speed - biasSpeed) / 2.0f + Mathf.Sin(gust_t * gust2TimesPerSecond) * (gust2Speed - biasSpeed) / 2.0f + (gust2Speed - biasSpeed) / 2.0f;
	}
	
	void Start() {
		globalWindSpeed = gameObject.transform.forward * calcWindForce();
		GWindBasic.windManager = this;
	}
	
	void FixedUpdate() {
		gust_t += Time.fixedDeltaTime;
		globalWindSpeed = gameObject.transform.forward * calcWindForce();
	}
	
	public Vector3 windAtImplementation(Vector3 position) {
		return globalWindSpeed;
	}
	
	public static Vector3 windAt(Vector3 position) {
		float time = Time.realtimeSinceStartup;
		if (time > blowLastTime + Time.fixedDeltaTime) {
			if (blowDuration > 0.0f) {
				blowDuration -= (time - blowLastTime);
				blowAmmount = blowPeak * (blowDuration / blowTime) * Mathf.Cos(blowDuration / blowTime * blowFrequency);
			} else {
				blowAmmount = 0.0f;
			}
			blowLastTime = time;
			//Vector3 blowForce2 = position - blowSource;
			//blowForce2 = blowAmmount * blowForce2 / Mathf.Pow(blowForce2.magnitude, blowDistanceExponent);
			//Debug.Log((blowDuration / blowTime).ToString() + "; " + blowForce2.ToString());
		}
		Vector3 blowForce = position - blowSource;
		blowForce = blowAmmount * blowForce / Mathf.Pow(blowForce.magnitude, blowDistanceExponent);
		if (windManager != null) return windManager.windAtImplementation(position) + blowForce;
		else return blowForce;
	}
	
	public static bool blowSet(Vector3 blowSource, float blowAmmount, float blowDuration, float blowFrequency, float blowDistanceExponent) {
		GWindBasic.blowSource = blowSource;
		GWindBasic.blowPeak = GWindBasic.blowAmmount = blowAmmount;
		GWindBasic.blowTime = GWindBasic.blowDuration = blowDuration;
		GWindBasic.blowFrequency = blowFrequency;
		GWindBasic.blowDistanceExponent = blowDistanceExponent;
		return true;
	}
}
