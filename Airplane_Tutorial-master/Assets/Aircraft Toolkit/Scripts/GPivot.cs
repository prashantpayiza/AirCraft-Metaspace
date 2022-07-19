using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GPivot: MonoBehaviour {
	public enum TAxisOrientation { forward, right, up };
	public enum TAxisSource { none, dummy, any, elevator, ailerons, rudder, gearsdown, flapsdown, brakes, engine, throttle, altimeter, vario, rpm, velocity, heading, gs };
	
	public string id = "";
	public GPivot.TAxisOrientation rotationPivotAxis = GPivot.TAxisOrientation.right;
	public float rotationAroundForwardOffset = 0.0f, rotationAroundRightOffset = 0.0f, rotationAroundUpOffset = 0.0f;
	public GPivot.TAxisSource ch1Source = GPivot.TAxisSource.none;
	public string ch1SourceName = "none";
	public float ch1PivotAngleWhenMin = 0.0f, ch1PivotAngleWhenMax = 0.0f, ch1PivotTurnsPerUnit = 1.0f;
	public GPivot.TAxisSource ch2Source = GPivot.TAxisSource.none;
	public string ch2SourceName = "none";
	public float ch2PivotAngleWhenMin = 0.0f, ch2PivotAngleWhenMax = 0.0f, ch2PivotTurnsPerUnit = 1.0f;
	public GPivot.TAxisSource ch3Source = GPivot.TAxisSource.none;
	public string ch3SourceName = "none";
	public float ch3PivotAngleWhenMin = 0.0f, ch3PivotAngleWhenMax = 0.0f, ch3PivotTurnsPerUnit = 1.0f;
	[HideInInspector]public Vector3 localEulerAngles = Vector3.zero;
	public float limitMin = -999999999.999f, limitMax = 999999999.999f;
	
	private static Dictionary<string, float> anyPivots = null;
	
	public static TAxisSource toTAxisSource(string s) {
		if ("none".Equals(s)) return TAxisSource.none;
		if ("dummy".Equals(s)) return TAxisSource.dummy;
		if ("elevator".Equals(s)) return TAxisSource.elevator;
		if ("elevators".Equals(s)) return TAxisSource.elevator;
		if ("aileron".Equals(s)) return TAxisSource.ailerons;
		if ("ailerons".Equals(s)) return TAxisSource.ailerons;
		if ("rudder".Equals(s)) return TAxisSource.rudder;
		if ("gears".Equals(s)) return TAxisSource.gearsdown;
		if ("gearsdown".Equals(s)) return TAxisSource.gearsdown;
		if ("flaps".Equals(s)) return TAxisSource.flapsdown;
		if ("flapsdown".Equals(s)) return TAxisSource.flapsdown;
		if ("brakes".Equals(s)) return TAxisSource.brakes;
		if ("engine".Equals(s)) return TAxisSource.engine;
		if ("throttle".Equals(s)) return TAxisSource.throttle;
		if ("altimeter".Equals(s)) return TAxisSource.altimeter;
		if ("vario".Equals(s)) return TAxisSource.vario;
		if ("rpm".Equals(s)) return TAxisSource.rpm;
		if ("velocity".Equals(s)) return TAxisSource.velocity;
		if ("heading".Equals(s)) return TAxisSource.heading;
		if ("gs".Equals(s)) return TAxisSource.gs;

		if (anyPivots == null) anyPivots = new Dictionary<string, float>();
		return TAxisSource.any;
	}
	public static string fromTAxisSource(TAxisSource s) {
		switch(s) {
			case TAxisSource.none: return "none";
			case TAxisSource.dummy: return "dummy";
			case TAxisSource.elevator: return "elevator";
			case TAxisSource.ailerons: return "ailerons";
			case TAxisSource.rudder: return "rudder";
			case TAxisSource.gearsdown: return "gearsdown";
			case TAxisSource.flapsdown: return "flapsdown";
			case TAxisSource.brakes: return "brakes";
			case TAxisSource.engine: return "engine";
			case TAxisSource.throttle: return "throttle";
			case TAxisSource.altimeter: return "altimeter";
			case TAxisSource.vario: return "vario";
			case TAxisSource.rpm: return "rpm";
			case TAxisSource.velocity: return "velocity";
			case TAxisSource.heading: return "heading";
			case TAxisSource.gs: return "gs";
			default: return "any";
		}
	}
	
	public static float toTAxisValue(string s, GAircraft sm, string default_axis) {
		if ("".Equals(default_axis)) return 0.0f;
		if ("".Equals(s)) return toTAxisValue(default_axis, sm, default_axis);
		if ("none".Equals(s)) return 0.0f;
		if ("dummy".Equals(s)) return 0.0f;
		if ("elevator".Equals(s)) return sm.inputElevator_output;
		if ("elevators".Equals(s)) return sm.inputElevator_output;
		if ("aileron".Equals(s)) return sm.inputAilerons_output;
		if ("ailerons".Equals(s)) return sm.inputAilerons_output;
		if ("rudder".Equals(s)) return sm.inputRudder_output;
		if ("gears".Equals(s)) return sm.inputGears_output;
		if ("gearsdown".Equals(s)) return sm.inputGears_output;
		if ("flaps".Equals(s)) return sm.inputFlaps_output;
		if ("flapsdown".Equals(s)) return sm.inputFlaps_output;
		if ("brakes".Equals(s)) return sm.inputBrakes_output;
		if ("engine".Equals(s)) return sm.inputThrottle_output;
		if ("throttle".Equals(s)) return sm.inputThrottle_output;
		if ("altimeter".Equals(s)) return sm.gaugesAltimeter_output;
		if ("vario".Equals(s)) return sm.gaugesVario_output;
		if ("rpm".Equals(s)) return sm.gaugesRpm_output;
		if ("velocity".Equals(s)) return sm.gaugesAirspeed_output;
		if ("airspeed".Equals(s)) return sm.gaugesAirspeed_output;
		if ("heading".Equals(s)) return sm.gaugesHeading_output;
		if ("gs".Equals(s)) return sm.gaugesGs_output;
		return GPivot.getAnyPivot(s);
	}

	public static bool setAnyPivot(string pivotName, float pivotValue) {
		if (anyPivots == null) anyPivots = new Dictionary<string, float>();

		if (anyPivots.ContainsKey(pivotName)) anyPivots[pivotName] = pivotValue;
		else anyPivots.Add(pivotName, pivotValue);
		return true;
	}
	
	public static bool delAnyPivot(string pivotName) {
		if (anyPivots == null) anyPivots = new Dictionary<string, float>();

		if (!anyPivots.ContainsKey(pivotName)) return false;
		anyPivots.Remove(pivotName);
		return true;
	}
	
	public static float getAnyPivot(string pivotName) {
		if (anyPivots == null) anyPivots = new Dictionary<string, float>();

		if (anyPivots.ContainsKey(pivotName)) return anyPivots[pivotName];
		else return 0.0f;
	}
}
