using UnityEngine;
using System.Collections;

public class GTrail: MonoBehaviour {
	public bool trailEnabled = true;
	public enum TTrailMode { standard, throttle };
	[HideInInspector]public int surfaceId_int = -1;
	public GTrail.TTrailMode mode = GTrail.TTrailMode.standard;
	public string surfaceId = "";
	public float startWidth = 0.0f, endWidth = 0.05f;
	public Color startColor = Color.grey, endColor = Color.clear;
	public string materialName = "Particles/Additive";
	public float forceThreshold = 100.0f;
	public float speedThreshold = 150.0f;
	public float heightThreshold = 80000.0f;
	[HideInInspector]public LineRenderer lineRenderer = null;
	[HideInInspector]public Vector3[] linePoints = null;
	[HideInInspector]public bool[] linePointsEnabled = null;
	[HideInInspector]public int linePoint = 0;

	public static TTrailMode toTTrailMode(string s) {
		if ("standard".Equals(s)) return TTrailMode.standard;
		if ("throttle".Equals(s)) return TTrailMode.throttle;
		return TTrailMode.standard;
	}
	public static string fromTTrailMode(TTrailMode m) {
		switch(m) {
			case TTrailMode.standard: return "standard";
			case TTrailMode.throttle: return "throttle";
			default: return "standard";
		}
	}
}
