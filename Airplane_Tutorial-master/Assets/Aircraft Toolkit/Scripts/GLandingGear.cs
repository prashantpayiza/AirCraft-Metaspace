using UnityEngine;
using System.Collections;

[RequireComponent(typeof(WheelCollider))]public class GLandingGear: MonoBehaviour {
	public enum GLandingGearRotatingAxis { none, right, up, forward };
	
	private GAircraft gAircraft = null;
	private WheelCollider wheelcollider = null;
	private float brakeForce_filtered = 0.0f;
	private float brakeForce_unfiltered = 0.0f;
	public float brakeForce_filter = 0.25f;
	public float brakeForce = 1000.0f;
	public float brakeForceHighSpeed = 1000.0f;
	public float brakeForceHighSpeedThreshold = 999.0f;
	public float brakeForceHighSpeedExponent = 0.0f;
	public bool enabledParkAutoBrake = true;
	public float parkAutoBrakeSpeedThreshold = 2.0f;
	public bool parkAutoBrakeSpeedThresholdForce = true;
	public float parkAutoBrakeForce = 25.0f;
	public float brakeResidualForce = 1.0f;
	public float brakeBrokenForce = 100000.0f;
	public float frictionExponentBaseCoefficient = 2.0f;
	public float frictionExponentDivisorCoefficient = 30.0f;
	private float forwardFrictionExtremumValue = 20000.0f;
	private float forwardFrictionAsymptoteValue = 10000.0f;
	private float sidewaysFrictionExtremumValue = 20000.0f;
	private float sidewaysFrictionAsymptoteValue = 10000.0f;
	private WheelFrictionCurve forwardFriction;
	private WheelFrictionCurve sidewaysFriction;
	public GameObject visibleWheelRotating = null;
	public GLandingGearRotatingAxis visibleWheelRotatingAxis = GLandingGearRotatingAxis.none;
	public float visibleWheelRotatingRpmMultiplier = 1.0f;
	public GameObject visibleWheelSuspension = null;
	public bool enableTorque = false;
	public string torqueUsePivot = "throttle";
	public float torqueUsePivotMultiplier = 10.0f;

	private Vector3 visibleWheelInitialPositionRelative = Vector3.zero;
	private WheelHit hit;
	private Vector3 hitPosition = Vector3.zero;
	
	bool FindGAircraft(GameObject obj, int maxdeep) {
		GAircraft gsm = (GAircraft)obj.GetComponent("GAircraft");
		if (gsm != null) gAircraft = gsm;
		if (gsm != null) return true;
		if (obj.transform.parent == null) return false;
		if (maxdeep <= 0) return false;
		--maxdeep;
		return FindGAircraft(obj.transform.parent.gameObject, maxdeep);
	}
	
	void Start() {
		if (!FindGAircraft(gameObject, 29)) gAircraft = null;
		wheelcollider = (WheelCollider)gameObject.GetComponent("WheelCollider");
		if (wheelcollider != null) {
			forwardFrictionExtremumValue = wheelcollider.forwardFriction.extremumValue;
			forwardFrictionAsymptoteValue = wheelcollider.forwardFriction.asymptoteValue;
			forwardFriction = wheelcollider.forwardFriction;
			sidewaysFrictionExtremumValue = wheelcollider.sidewaysFriction.extremumValue;
			sidewaysFrictionAsymptoteValue = wheelcollider.sidewaysFriction.asymptoteValue;
			sidewaysFriction = wheelcollider.sidewaysFriction;
		}
		if (visibleWheelSuspension != null) {
			visibleWheelInitialPositionRelative = visibleWheelSuspension.transform.localPosition;
		}
	}
	
	void FixedUpdate() {
		if (wheelcollider != null) {
			if (gAircraft != null) {
				if (gAircraft.isCrashed) brakeForce_unfiltered = Mathf.Abs(brakeBrokenForce);
				else if (parkAutoBrakeSpeedThresholdForce && enabledParkAutoBrake && (gAircraft.speed < parkAutoBrakeSpeedThreshold)) brakeForce_unfiltered = Mathf.Abs(parkAutoBrakeForce);
				else if (Mathf.Abs(gAircraft.inputBrakes_output) > 0.1f) {
					if (gAircraft.speed > brakeForceHighSpeedThreshold) brakeForce_unfiltered = Mathf.Abs(brakeForceHighSpeed * Mathf.Pow(gAircraft.speed, brakeForceHighSpeedExponent)) * gAircraft.inputBrakes_output;
					else brakeForce_unfiltered = Mathf.Abs(brakeForce) * gAircraft.inputBrakes_output;
				} else if (enabledParkAutoBrake && (gAircraft.speed < parkAutoBrakeSpeedThreshold)) brakeForce_unfiltered = Mathf.Abs(parkAutoBrakeForce);
				else brakeForce_unfiltered = brakeResidualForce;
				brakeForce_filtered = brakeForce_filtered * (1f - brakeForce_filter) + brakeForce_unfiltered * brakeForce_filter;
				wheelcollider.brakeTorque = brakeForce_filtered;
				float speed = gAircraft.speed;
				if (gAircraft.isCrashed) speed = 10.0f;
				forwardFriction.extremumValue = forwardFrictionExtremumValue * Mathf.Pow(frictionExponentBaseCoefficient, -speed / frictionExponentDivisorCoefficient);
				forwardFriction.asymptoteValue = forwardFrictionAsymptoteValue * Mathf.Pow(frictionExponentBaseCoefficient, -speed / frictionExponentDivisorCoefficient);
				sidewaysFriction.extremumValue = sidewaysFrictionExtremumValue * Mathf.Pow(frictionExponentBaseCoefficient, -speed / frictionExponentDivisorCoefficient);
				sidewaysFriction.asymptoteValue = sidewaysFrictionAsymptoteValue * Mathf.Pow(frictionExponentBaseCoefficient, -speed / frictionExponentDivisorCoefficient);
				wheelcollider.forwardFriction = forwardFriction;
				wheelcollider.sidewaysFriction = sidewaysFriction;
				if (enableTorque) {
					float pivotValue;
					switch (torqueUsePivot) {
					case "throttle":
						pivotValue = gAircraft.inputThrottle_output * torqueUsePivotMultiplier;
						break;
					default:
						pivotValue = GPivot.getAnyPivot(torqueUsePivot) * torqueUsePivotMultiplier;
						break;
					}
					wheelcollider.motorTorque = pivotValue;
				}
			}
			if (visibleWheelSuspension != null) {
				if (wheelcollider.GetGroundHit(out hit)) {
					hitPosition.x = hit.point.x;
					hitPosition.y = hit.point.y;
					hitPosition.z = hit.point.z;
				} else {
					hitPosition = wheelcollider.transform.position - wheelcollider.transform.up * (wheelcollider.suspensionDistance + wheelcollider.radius);
				}
				visibleWheelSuspension.transform.position = hitPosition;
				hitPosition = visibleWheelSuspension.transform.localPosition;
				hitPosition.x = visibleWheelInitialPositionRelative.x;
				hitPosition.z = visibleWheelInitialPositionRelative.z;
				visibleWheelSuspension.transform.localPosition = hitPosition;
			}
			if (visibleWheelRotating != null) {
				switch (visibleWheelRotatingAxis) {
				case GLandingGearRotatingAxis.forward:
					visibleWheelRotating.transform.RotateAroundLocal(Vector3.forward, wheelcollider.rpm / 60.0f / Mathf.PI / 4.0f);
					break;
				case GLandingGearRotatingAxis.right:
					visibleWheelRotating.transform.RotateAroundLocal(Vector3.right, wheelcollider.rpm / 60.0f / Mathf.PI / 4.0f);
					break;
				case GLandingGearRotatingAxis.up:
					visibleWheelRotating.transform.RotateAroundLocal(Vector3.up, wheelcollider.rpm / 60.0f / Mathf.PI / 4.0f);
					break;
				}
			}
		}
	}
}
