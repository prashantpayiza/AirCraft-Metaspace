using UnityEngine;
using System.Collections;

public class GDrive: MonoBehaviour {
	public enum TDriveType { basic, propeller, rotor, tailrotor, turbofan, sin, cos, rotor_basic, forward_rotor2, up_rotor2, right_rotor2 };
	public enum TDrivePowerControlBy { defaultvalue, throttle, pivot, elevator, ailerons, rudder };
	
	[HideInInspector]public Vector3 lastPosition = Vector3.zero;
	
	public GDrive.TDriveType type = GDrive.TDriveType.basic;
	[HideInInspector]public float drive_rpm = 0.0f;
	[HideInInspector]public float drive_shaft = 0.0f;
	[HideInInspector]public float drive_output = 0.0f;
	public bool powered = true;
	public float poweredFactor = 0.0f;
	public float poweredFactorFilter = 0.005f;
	public GDrive.TDrivePowerControlBy powerControlBy = GDrive.TDrivePowerControlBy.defaultvalue;
	public string powerControlByPivot = "";
	public GDrive.TDrivePowerControlBy powerCollectiveControlBy = GDrive.TDrivePowerControlBy.defaultvalue;
	public string powerCollectiveControlByPivot = "";
	public GDrive.TDrivePowerControlBy powerCyclicForwardControlBy = GDrive.TDrivePowerControlBy.defaultvalue;
	public string powerCyclicForwardControlByPivot = "";
	public GDrive.TDrivePowerControlBy powerCyclicSideControlBy = GDrive.TDrivePowerControlBy.defaultvalue;
	public string powerCyclicSideControlByPivot = "";
	//public float throttleReverse = -0.5f;
	public float throttleIdle = 0.0f;
	public float throttleMax = 1.0f;
	public float throttleAfterburner = 1.4f;
	//public float throttleWatts = 1000.0f;
	//public float throttleBiasOffset = 0.05f;
	//public float throttleBiasMultiplier = 1.1f;
	public float throttleRpmConversionRatio = 1.0f;
	public float throttleRpmConversionFilter = 0.005f;
	public float theoreticalEnginePower = 1000.0f;
	//public float theoreticalTargetRpms = 350.0f;
	//public float theoreticalTargetRpms = 3000.0f;
	public float theoreticalTargetRpms = 700.0f;
	public float throttleBiasOffset = 0.05f;
	public float throttleBiasMultiplier = 1.1f;
	//public float throttleBiasMultiplier = 0.8f;
	//public float throttleForwardRearOffset = -1.9f;
	//public float throttleForwardRearOffset = -0.9f;
	public float throttleForwardRearOffset = -1.1f;
	//public float throttleForwardRearOffset = 0.0f;
	public float throttleLeftRightOffset = 0.0f;
	public float yawForceMultiplier = 50000.0f;
	public float dumperForceMultiplier = 1.0f;
	//public float dumperForceMultiplier = 0.75f;
	//public float commonFilter = 0.03f;
	//public float commonFilter = 0.5f;
	public float commonFilter = 0.9f;
	public float pitchFilter = 0.1f;
	public float rollFilter = 0.1f;
	public float yawFilter = 0.1f;
	public float dumperFilter = 0.1f;
	public int bladeNumber = 3;
	public float bladeMass = 1.0f;
	public float bladeLength = 0.5f;
	public float bladeWidthMin = 0.05f;
	public float bladeWidthMax = 0.15f;
	public float bladeDepth = 0.05f;
	public float bladeAngleYaw = 20.0f;
	public float bladeAnglePitch = 1.0f;
	public float bladeShapeCoefficient = 0.9f;
	public bool bladeWashEnabled = true;
	[HideInInspector]public float bladeWashSpeed = 0.0f;
	public float bladeWashRadiusNormal = 3.0f;
	public float bladeWashRadiusTangent = 20.0f;
	public float bladeWashFactor = 0.25f;
	public float bladeWashSpread = 30.0f;
	[HideInInspector]public Vector3 rotorGyroscopicLastUp = Vector3.up;
	[HideInInspector]public float rotorGyroscopicCoefficient = 0.5f;
	public float rotorCollectiveBias = 0.5f;
	public float rotorCollectiveCoefficient = 1.0f;
	public float rotorCyclicForwardCoefficient = 0.5f;
	public float rotorCyclicSideCoefficient = 0.5f;
	public float rotorAutorotationCoefficient = 0.5f;
	public Vector3 basicRotorTailOffset = new Vector3(0.0f, -1.2f, -12.0f);
	//public float basicRotorBladeForcePoint = 0.5f;
	public float basicRotorBladeForcePoint = 0.1f;
	public float basicRotorAirdensityBias = 1.201194f;
	public float basicRotorAirdensityExp = 3.3f;
	public float overspeedPitchFactor = 45.0f;
	public float overspeedPitchFactorExponent = 2.0f;
	public float overspeedPitchFactorAtSpeed = 350.0f;
	public float overspeedPitchFactorMin = 0.1f;
	public float overspeedPitchFactorMax = 20.0f;
	public float overspeedPitchFactorOffsetSpeed = -85.0f;
	public float overspeedPitchFactorOffsetValue = -0.1f;
	public float overspeedPitchFactorMultiplier = 1.0f;
	public float overspeedPitchFactorVibration = 1.0f;
	public int overspeedPitchFactorVibratorId = 3;
	public float overspeedRollFactor = 45.0f;
	public float overspeedRollFactorExponent = 3.0f;
	public float overspeedRollFactorAtSpeed = 575.0f;
	public float overspeedRollFactorMin = 0.1f;
	public float overspeedRollFactorMax = 100.0f;
	public float overspeedRollFactorOffsetSpeed = -65.0f;
	public float overspeedRollFactorOffsetValue = -0.1f;
	public float overspeedRollFactorMultiplier = 1.0f;
	public float overspeedRollFactorVibration = 1.0f;
	public int overspeedRollFactorVibratorId = 4;
	public float overspeedYawFactor = 8000000.0f;
	public float overspeedYawFactorExponent = 100.0f;
	public float overspeedYawFactorAtSpeed = 700.0f;
	public float overspeedYawFactorMin = 1.0f;
	public float overspeedYawFactorMax = 1000000.0f;
	public float overspeedYawFactorOffsetSpeed = 0.0f;
	public float overspeedYawFactorOffsetValue = -1.0f;
	public float overspeedYawFactorMultiplier = 1.0f;
	public float overspeedYawFactorVibration = 1.0f;
	public int overspeedYawFactorVibratorId = 5;
	//public float throttle_speed_multiplier = 0.25f;
	public float climbReductionWithSpeed = 0.15f;
	//public float throttle_speed_multiplier_at_speed = 100.0f;
	public float climbReductionWithSpeedAtSpeed = 50.0f;
	//public float throttle_climb_multiplier = 0.25f;
	//public float throttle_climb_multiplier = 0.15f;
	public float climbReductionWithVerticalSpeed = 0.1f;
	public float climbReductionWithVerticalSpeedAtSpeed = 5.5f;
	public float rollReductionWithRollSpeed = 0.1f;
	public float rollReductionWithRollSpeedAtSpeed = 1.75f;
	public float rollReductionWithMassFactor = 0.1f;
	public float basicRotorVariationMultiplier = 1.0f;
	public float basicRotorVelocityFactor = 10.0f;
	public float basicRotorProjectedVelocityFactor = 100.0f;
	public float basicRotorProjectedVelocityMultiplier = 10.0f;
	//public float mainrotor_perpendicularvelocity_factor = 1000.0f;
	//public float mainrotor_perpendicularvelocity_multiplier = 10.0f;
	public float basicRotorProjectedVelocityForwardOffset = 205.5f;
	public float basicRotorProjectedVelocityRearOffset = 205.5f;
	public float basicRotorProjectedVelocityLeftOffset = 290.0f;
	public float basicRotorProjectedVelocityRightOffset = 290.0f;
	public string shaftOutputPivotId = "engine";
	[HideInInspector]public LineRenderer lineRenderer = null;
	
	public static TDriveType toTDriveType(string s) {
		if ("basic".Equals(s)) return TDriveType.basic;
		if ("propeller".Equals(s)) return TDriveType.propeller;
		if ("rotor".Equals(s)) return TDriveType.rotor;
		if ("tailrotor".Equals(s)) return TDriveType.tailrotor;
		if ("turbofan".Equals(s)) return TDriveType.turbofan;
		if ("sin".Equals(s)) return TDriveType.sin;
		if ("cos".Equals(s)) return TDriveType.cos;
		if ("rotor_basic".Equals(s)) return TDriveType.rotor_basic;
		if ("basicrotor".Equals(s)) return TDriveType.rotor_basic;
		if ("forward_rotor2".Equals(s)) return TDriveType.forward_rotor2;
		if ("propeller2".Equals(s)) return TDriveType.forward_rotor2;
		if ("up_rotor2".Equals(s)) return TDriveType.up_rotor2;
		if ("rotor2".Equals(s)) return TDriveType.up_rotor2;
		if ("right_rotor2".Equals(s)) return TDriveType.right_rotor2;
		if ("tailrotor2".Equals(s)) return TDriveType.right_rotor2;
		return TDriveType.basic;
	}
	public static string fromTDriveType(TDriveType s) {
		switch(s) {
			case TDriveType.basic: return "basic";
			case TDriveType.propeller: return "propeller";
			case TDriveType.rotor: return "rotor";
			case TDriveType.tailrotor: return "tailrotor";
			case TDriveType.turbofan: return "turbofan";
			case TDriveType.sin: return "sin";
			case TDriveType.cos: return "cos";
			case TDriveType.rotor_basic: return "rotor_basic";
			case TDriveType.forward_rotor2: return "forward_rotor2";
			case TDriveType.up_rotor2: return "up_rotor2";
			case TDriveType.right_rotor2: return "right_rotor2";
			default: return "basic";
		}
	}

	public static TDrivePowerControlBy toTDrivePowerControlBy(string s) {
		if ("defaultvalue".Equals(s)) return TDrivePowerControlBy.defaultvalue;
		if ("default".Equals(s)) return TDrivePowerControlBy.defaultvalue;
		if ("throttle".Equals(s)) return TDrivePowerControlBy.throttle;
		if ("pivot".Equals(s)) return TDrivePowerControlBy.pivot;
		if ("elevator".Equals(s)) return TDrivePowerControlBy.elevator;
		if ("ailerons".Equals(s)) return TDrivePowerControlBy.ailerons;
		if ("rudder".Equals(s)) return TDrivePowerControlBy.rudder;
		return TDrivePowerControlBy.defaultvalue;
	}
	public static string fromTDrivePowerControlBy(TDrivePowerControlBy s) {
		switch(s) {
			case TDrivePowerControlBy.defaultvalue: return "defaultvalue";
			case TDrivePowerControlBy.throttle: return "throttle";
			case TDrivePowerControlBy.pivot: return "pivot";
			case TDrivePowerControlBy.elevator: return "elevator";
			case TDrivePowerControlBy.ailerons: return "ailerons";
			case TDrivePowerControlBy.rudder: return "rudder";
			default: return "throttle";
		}
	}
}
