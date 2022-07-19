using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]public class GAircraft: MonoBehaviour {
	
	public static GAircraft singleton = null;
	
	public static float globalVolume = 1.0f;
	public bool globalRenderPhysicSurfaces = true;
	public bool globalRenderForceVectors = true;
	public bool globalApplyForceVectors = true;
	
	public enum TAxisSource { none, keys, keys_exp, unity_axis, inv_unity_axis, unity_axis_exp, inv_unity_axis_exp, user , mix, mix_exp, inv_mix, inv_mix_exp };
	public enum TSurfaceMethod { rigidbodyGetPointVelocity, rigidbodyGetPointVelocityWithPropwash, deltaFiltered, deltaFilteredWithPropwash };
	public enum TGroundEffectType { none, airplane, airplanesoft, airplanesoft2, helicopter, helicoptersoft, hovercraft };
	public enum TCrashType { none, airplane, helicopter };
	
	[HideInInspector]public float height = 0.0f;
	[HideInInspector]public float speed = 0.0f;
	[HideInInspector]public float stall = 0.0f;
	private float speed_filter = 0.05f;
	[HideInInspector]public float stalled_surfaces = 0.0f;
	[HideInInspector]public float laminar_surfaces = 0.0f;
	[HideInInspector]public float analysis_surfaces = 0.0f;
	[HideInInspector]public float total_surfaces = 0.0f;
	private Vector3 speed_lastPosition = Vector3.zero;
	[HideInInspector]public float distanceToGround = 0.0f;
	[HideInInspector]public float yPositionOfGround = 0.0f;
	
	private Vector3 vibrationDirection = Vector3.forward;
	private float vibrationAmplitudePeak = 0.0f;
	private float vibrationAmplitude = 0.0f;
	private float vibrationDuration = 1.0f;
	private float vibrationTime = 0.0f;
	private float vibrationFrequency = 1.0f;

	public float globalSimulationScale = 1.0f;
	public float globalDragMultiplier = 10.0f;
	public float globalLiftMultiplier = 10.0f;
	public GSurface.TSurfaceBehaviourType globalDefaultBehaviour = GSurface.TSurfaceBehaviourType.default_behaviour;
	public bool globalAnalysisLaminarEnabled = true;
	public float globalAnalysisLaminarStallMultiplier = 1.0f;
	public float globalAnalysisLaminarStallExponent = 1.65f;
	public GSurface.TSurfaceShapeType globalShapeDefaultSurface = GSurface.TSurfaceShapeType.plane;
	public string globalShapeDefaultSurfaceParameter = "";
	public float globalDefaultLaminarLimitAngle = 15.0f;	
	public float globalDefaultTurbulentLimitAngle = 35.0f;	
	public float globalDefaultStallLimitAngle = 33.0f;	
	public bool globalDebugNodes = true;
	public bool globalDebugMore = true;
	//private static bool globalStaticDebugNodes = true;
	
	[HideInInspector]public static bool GAircraftDropPointVelocity = false;
	private Vector3 rigidbodyVelocity = Vector3.zero;
	private Vector3 rigidbodyVelocity_lastValue = Vector3.zero;

	public float inputTrimStep = 0.05f;
	public float inputTrimMax = 0.5f;
	public float inputTrimMin = -0.5f;
	public KeyCode inputTrimKey = KeyCode.LeftShift;
	
	public float inputSensivity = 1.0f;
	public float inputSensivityStep = 0.05f;
	public float inputSensivityMin = 0.05f;
	public float inputSensivityMax = 1.0f;
	public KeyCode inputSensivityKeyForIncrement = KeyCode.KeypadPlus;
	public KeyCode inputSensivityKeyForDecrement = KeyCode.KeypadMinus;
	public float inputSensivityCoefficientSpeed = 1.000002f;
	public float inputSensivityCoefficientSpeedLimit = 0.25f;
	public enum TSensivityChannel { none, elevator, ailerons, rudder };

	[HideInInspector]public float inputElevator_internal = 0.5f;
	[HideInInspector]public float inputElevator_output = 0.5f;
	public TAxisSource inputElevatorSource = TAxisSource.inv_unity_axis;
	public float inputElevatorSourceExpK = 1.25f;
	public string inputElevatorSourceUnityAxis = "Vertical";
	public KeyCode inputElevatorKeyForIncrement = KeyCode.S;
	public KeyCode inputElevatorKeyForDecrement = KeyCode.W;
	public float inputElevatorKeySmoothFilter = 0.05f;
	public float inputElevatorGlobalSmoothFilter = 1.0f;
	public float inputElevatorTrim = 0.0f;
	public float inputElevatorSensivityCoefficientSpeed = 1.0f;

	[HideInInspector]public float inputAilerons_internal = 0.5f;
	[HideInInspector]public float inputAilerons_output = 0.5f;
	public TAxisSource inputAileronsSource = TAxisSource.unity_axis;
	public float inputAileronsSourceExpK = 1.25f;
	public string inputAileronsSourceUnityAxis = "Horizontal";
	public KeyCode inputAileronsKeyForIncrement = KeyCode.D;
	public KeyCode inputAileronsKeyForDecrement = KeyCode.A;
	public float inputAileronsKeySmoothFilter = 0.05f;
	public float inputAileronsGlobalSmoothFilter = 1.0f;
	public float inputAileronsTrim = 0.0f;
	public float inputAileronsSensivityCoefficientSpeed = 1.0f;

	[HideInInspector]public float inputRudder_internal = 0.5f;
	[HideInInspector]public float inputRudder_output = 0.5f;
	public TAxisSource inputRudderSource = TAxisSource.keys;
	public float inputRudderSourceExpK = 1.25f;
	public string inputRudderSourceUnityAxis = "Write here the Rudder Axisname";
	public KeyCode inputRudderKeyForIncrement = KeyCode.E;
	public KeyCode inputRudderKeyForDecrement = KeyCode.Q;
	public float inputRudderKeySmoothFilter = 0.05f;
	public float inputRudderGlobalSmoothFilter = 1.0f;
	public float inputRudderTrim = 0.0f;
	public float inputRudderSensivityCoefficientSpeed = 1.0f;

	[HideInInspector]public float inputThrottle_internal2 = 0.0f;
	[HideInInspector]public float inputThrottle_internal = 0.0f;
	[HideInInspector]public float inputThrottle_output = 0.0f;
	public TAxisSource inputThrottleSource = TAxisSource.keys;
	public string inputThrottleSourceUnityAxis = "Write here the Throttle Axisname";
	public float inputThrottleAfterburnerMultiplier = 2.0f;
	public TAxisSource inputThrottleAfterburnerSource = TAxisSource.keys;
	public string inputThrottleAfterburnerSourceUnityAxis = "Write here Throttle Afterburner Axisname";
	public float inputThrottleReverseMultiplier = -1.0f;
	public TAxisSource inputThrottleReverseSource = TAxisSource.keys;
	public string inputThrottleReverseSourceUnityAxis = "Write here Throttle Reverse Axisname";
	public float inputThrottleNitroMultiplier = 3.0f;
	public TAxisSource inputThrottleNitroSource = TAxisSource.keys;
	public string inputThrottleNitroSourceUnityAxis = "Write here Throttle Nitro Axisname";
	public float inputThrottleNitroTime = 5.0f;
	public float inputThrottleNitroFilter = 0.1f;
	public int inputThrottleNitroInitialCount = 5;
	public int inputThrottleNitroCount = 5;
	public string inputThrottleNitroSoundGameObjectName = "sound_nitro";
	private AudioSource inputThrottleNitroSoundAudioSource = null;
	public float inputThrottleSourceExpK = 1.25f;
	public float inputThrottleIncrementDecrementStep = 0.01f;
	public KeyCode inputThrottleKeyForIncrement = KeyCode.R;
	public KeyCode inputThrottleKeyForDecrement = KeyCode.T;
	public KeyCode inputThrottleKeyFor0p = KeyCode.Delete;
	public KeyCode inputThrottleKeyFor10p = KeyCode.Alpha1;
	public KeyCode inputThrottleKeyFor20p = KeyCode.Alpha2;
	public KeyCode inputThrottleKeyFor30p = KeyCode.Alpha3;
	public KeyCode inputThrottleKeyFor40p = KeyCode.Alpha4;
	public KeyCode inputThrottleKeyFor50p = KeyCode.Alpha5;
	public KeyCode inputThrottleKeyFor60p = KeyCode.Alpha6;
	public KeyCode inputThrottleKeyFor70p = KeyCode.Alpha7;
	public KeyCode inputThrottleKeyFor80p = KeyCode.Alpha8;
	public KeyCode inputThrottleKeyFor90p = KeyCode.Alpha9;
	public KeyCode inputThrottleKeyFor100p = KeyCode.Alpha0;
	public KeyCode inputThrottleKeyForAfterburner = KeyCode.Space;
	public KeyCode inputThrottleKeyForReverse = KeyCode.LeftControl;
	public KeyCode inputThrottleKeyForNitro = KeyCode.Tab;
	public float inputThrottleKeySmoothFilter = 0.05f;
	public float inputThrottleGlobalSmoothFilter = 1.0f;
	[HideInInspector]public float kThrottle_aft = 0.0f;
	[HideInInspector]public float kThrottle_rev = 0.0f;
	[HideInInspector]public float kThrottle_ntr = 0.0f;
	[HideInInspector]public float kThrottle_ntr_multiplier = 1.0f;
	[HideInInspector]public float kThrottle_ntr_time = 0.0f;

	private float inputGears_internal = 1.0f;
	[HideInInspector]public bool inputGears_internal_enabled = true;
	[HideInInspector]public float inputGears_output = 1.0f;
	public TAxisSource inputGearsSource = TAxisSource.keys;
	public string inputGearsSourceUnityAxis = "Write here the Gears Axisname";
	public float inputGearsSourceExpK = 1.25f;
	public KeyCode inputGearsKeyForToggle = KeyCode.G;
	public float inputGearsKeySmoothFilter = 0.01f;
	public float inputGearsGlobalSmoothFilter = 1.0f;

	private float inputFlaps_internal = 0.0f;
	[HideInInspector]public bool inputFlaps_internal_enabled = false;
	[HideInInspector]public float inputFlaps_output = 0.0f;
	public TAxisSource inputFlapsSource = TAxisSource.keys;
	public string inputFlapsSourceUnityAxis = "Write here the Flaps Axisname";
	public float inputFlapsSourceExpK = 1.25f;
	public KeyCode inputFlapsKeyForToggle = KeyCode.F;
	public float inputFlapsKeySmoothFilter = 0.05f;
	public float inputFlapsGlobalSmoothFilter = 1.0f;

	[HideInInspector]public float inputBrakes_internal = 0.0f;
	[HideInInspector]public float inputBrakes_output = 0.0f;
	public TAxisSource inputBrakesSource = TAxisSource.keys;
	public string inputBrakesSourceUnityAxis = "Write here the Brakes Axisname";
	public float inputBrakesSourceExpK = 1.25f;
	public KeyCode inputBrakesKeyForIncrement = KeyCode.B;
	public float inputBrakesKeySmoothFilter = 0.05f;
	public float inputBrakesGlobalSmoothFilter = 1.0f;

	private float inputTrails_internal = 0.0f;
	[HideInInspector]public bool inputTrails_internal_enabled = true;
	private float inputTrails_output = 0.0f;
	public TAxisSource inputTrailsSource = TAxisSource.keys;
	public string inputTrailsSourceUnityAxis = "Write here the Trails Axisname";
	public float inputTrailsSourceExpK = 1.25f;
	public KeyCode inputTrailsKeyForToggle = KeyCode.Z;
	public float inputTrailsKeySmoothFilter = 0.05f;
	public float inputTrailsGlobalSmoothFilter = 1.0f;
	
	private Vector3 center;
	
	private GSurface[] surfaces = null;
	private int surfaces_count = 0;
	private const int surfaces_maxcount = 100;
	
	[System.Serializable]public class LabeledSurfaceDesc {
		[HideInInspector]public GameObject gameObject = null;

		public string id = "";
		[HideInInspector]public Vector3 lastPosition = Vector3.zero;
		[HideInInspector]public Vector3 drag = Vector3.zero;
		[HideInInspector]public Vector3 lift = Vector3.zero;
		public bool surfaceEnable = true;
		public bool surfaceEnableThin = true, surfaceEnablePositive = true, surfaceEnableNegative = true;
		public bool surfaceEnableXPositive = false, surfaceEnableYPositive = false, surfaceEnableZPositive = false;
		public bool surfaceEnableXNegative = false, surfaceEnableYNegative = false, surfaceEnableZNegative = false;
		public GSurface.TSurfaceShapeType shapeXPositive = GSurface.TSurfaceShapeType.default_shape;
		public string shapeXPositiveParameter = "";
		public GSurface.TSurfaceShapeType shapeYPositive = GSurface.TSurfaceShapeType.default_shape;
		public string shapeYPositiveParameter = "";
		public GSurface.TSurfaceShapeType shapeZPositive = GSurface.TSurfaceShapeType.default_shape;
		public string shapeZPositiveParameter = "";
		public GSurface.TSurfaceShapeType shapeXNegative = GSurface.TSurfaceShapeType.default_shape;
		public string shapeXNegativeParameter = "";
		public GSurface.TSurfaceShapeType shapeYNegative = GSurface.TSurfaceShapeType.default_shape;
		public string shapeYNegativeParameter = "";
		public GSurface.TSurfaceShapeType shapeZNegative = GSurface.TSurfaceShapeType.default_shape;
		public string shapeZNegativeParameter = "";
		public GSurface.TSurfaceBehaviourType behaviourXPositive = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourYPositive = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourZPositive = GSurface.TSurfaceBehaviourType.parent_behaviour;
		public GSurface.TSurfaceBehaviourType behaviourXNegative = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourYNegative = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourZNegative = GSurface.TSurfaceBehaviourType.parent_behaviour;
		[HideInInspector]public float cachedXScale = 1.0f, cachedYScale = 1.0f, cachedZScale = 1.0f;
		[HideInInspector]public float cachedXSurface = 1.0f, cachedYSurface = 1.0f, cachedZSurface = 1.0f;
		public float coefficientXDragWhenXPositiveFlow = 1.0f, coefficientYDragWhenYPositiveFlow = 1.0f, coefficientZDragWhenZPositiveFlow = 1.0f;
		public float coefficientXDragWhenXNegativeFlow = 1.0f, coefficientYDragWhenYNegativeFlow = 1.0f, coefficientZDragWhenZNegativeFlow = 1.0f;
		public float coefficientXLiftWhenYPositiveFlow = 0.0f, coefficientXLiftWhenYNegativeFlow = 0.0f, coefficientXLiftWhenZPositiveFlow = 0.0f, coefficientXLiftWhenZNegativeFlow = 0.0f;
		public float coefficientYLiftWhenXPositiveFlow = 0.0f, coefficientYLiftWhenXNegativeFlow = 0.0f, coefficientYLiftWhenZPositiveFlow = 0.0f, coefficientYLiftWhenZNegativeFlow = 0.1f;
		public float coefficientZLiftWhenXPositiveFlow = 0.0f, coefficientZLiftWhenXNegativeFlow = 0.0f, coefficientZLiftWhenYPositiveFlow = 0.0f, coefficientZLiftWhenYNegativeFlow = 0.0f;
		[HideInInspector]public LineRenderer lineRenderer = null;
		public float propWashFactor = 1.0f;
		public float coefficientExponent = 2.0f;
		public bool surfaceDebug = false;
	}

	private LabeledSurfaceDesc[] labeled_surfaces = null;
	private int labeled_surfaces_count = 0;
	private const int labeled_surfaces_maxcount = 100;

	private GPivot[] surfacepivots = null;
	private int surfacepivots_count = 0;
	private const int surfacepivots_maxcount = 100;

	[System.Serializable]public class LabeledSurfacePivotDesc {
		public GameObject gameObject = null;

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
		public Vector3 localEulerAngles = Vector3.zero;
		public float limitMin = -999999999.999f, limitMax = 999999999.999f;
	}

	private LabeledSurfacePivotDesc[] labeled_surfacepivots = null;
	private int labeled_surfacepivots_count = 0;
	private const int labeled_surfacepivots_maxcount = 100;

	[System.Serializable]public class LabeledSurfaceMiscDesc {
		public GameObject gameObject = null;
		public bool lospeed = false;
		public bool hispeed = false;
		public bool cameraPosition = false;
		public bool cameraCanRotate = true;
		public string pivotId = "";
		public float rpmThreshold = 360.0f;
	}

	[HideInInspector]public LabeledSurfaceMiscDesc[] labeled_surfacemisc = null;
	[HideInInspector]public int labeled_surfacemisc_count = 0;
	private const int labeled_surfacemisc_maxcount = 100;
	
	private GDrive[] surfacedrives = null;
	private int surfacedrives_count = 0;
	private const int surfacedrives_maxcount = 100;

	[System.Serializable]public class LabeledSurfaceDriveDesc {
		public GameObject gameObject = null;
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
		//public float dumperForceMultiplier = 1.0f;
		//public float dumperForceMultiplier = 0.75f;
		public float dumperForceMultiplier = 0.875f;
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
		public float overspeedPitchFactorAtSpeed = 350.0f * 0.75f;
		public float overspeedPitchFactorMin = 0.1f;
		public float overspeedPitchFactorMax = 20.0f;
		public float overspeedPitchFactorOffsetSpeed = -85.0f;
		public float overspeedPitchFactorOffsetValue = -0.1f;
		public float overspeedPitchFactorMultiplier = 1.0f;
		public float overspeedPitchFactorVibration = 1.0f;
		public int overspeedPitchFactorVibratorId = 3;
		public float overspeedRollFactor = 45.0f;
		public float overspeedRollFactorExponent = 3.0f;
		public float overspeedRollFactorAtSpeed = 575.0f * 0.75f;
		public float overspeedRollFactorMin = 0.1f;
		public float overspeedRollFactorMax = 100.0f;
		public float overspeedRollFactorOffsetSpeed = -65.0f;
		public float overspeedRollFactorOffsetValue = -0.1f;
		public float overspeedRollFactorMultiplier = 1.0f;
		public float overspeedRollFactorVibration = 1.0f;
		public int overspeedRollFactorVibratorId = 4;
		public float overspeedYawFactor = 8000000.0f;
		public float overspeedYawFactorExponent = 100.0f;
		public float overspeedYawFactorAtSpeed = 700.0f * 0.75f;
		public float overspeedYawFactorMin = 1.0f;
		public float overspeedYawFactorMax = 1000000.0f;
		public float overspeedYawFactorOffsetSpeed = 0.0f;
		public float overspeedYawFactorOffsetValue = -1.0f;
		public float overspeedYawFactorMultiplier = 1.0f;
		public float overspeedYawFactorVibration = 1.0f;
		public int overspeedYawFactorVibratorId = 5;
		public float climbReductionWithSpeed = 0.15f;
		public float climbReductionWithSpeedAtSpeed = 50.0f;
		public float climbReductionWithVerticalSpeed = 0.1f;
		public float climbReductionWithVerticalSpeedAtSpeed = 5.5f;
		public float pitchReductionWithRollSpeed = 0.1f;
		public float pitchReductionWithRollSpeedAtSpeed = 1.75f;
		public float pitchReductionWithMassFactor = 0.001f;
		public float rollReductionWithRollSpeed = 2.0f;
		public float rollReductionWithRollSpeedAtSpeed = 2.5f;
		public float rollReductionWithRollSpeedSmooth = 10.0f;
		//public float rollReductionWithRollSpeedMultiplier = 0.05f;
		//public float rollReductionWithRollSpeedMultiplier = 0.001f;
		//public float rollReductionWithMassFactor = 0.005f;
		public float rollReductionWithMassFactor = 0.02f;
		public float rollReductionWithSpeed = 0.1f;
		public float rollReductionWithSpeedAtSpeed = 50.0f;
		public float rollReductionWithSpeedSmooth = 2.5f;
		public float basicRotorVariationMultiplier = 1.0f;
		public float basicRotorVelocityFactor = 10.0f;
		public float basicRotorProjectedVelocityFactor = 100.0f;
		public float basicRotorProjectedVelocityMultiplier = 10.0f;
		public float basicRotorProjectedVelocityForwardOffset = 205.5f;
		public float basicRotorProjectedVelocityRearOffset = 205.5f;
		public float basicRotorProjectedVelocityLeftOffset = 290.0f;
		public float basicRotorProjectedVelocityRightOffset = 290.0f;
		public string shaftOutputPivotId = "engine";
		[HideInInspector]public LineRenderer lineRenderer = null;
	}

	private LabeledSurfaceDriveDesc[] labeled_surfacedrives;
	private int labeled_surfacedrives_count = 0;
	private const int labeled_surfacedrives_maxcount = 100;

	private Vector3 addTrails_value = Vector3.zero;
	private GTrail[] surfacetrails = null;
	private int surfacetrails_count = 0;
	private const int surfacetrails_maxcount = 100;

	[System.Serializable]public class LabeledSurfaceTrailDesc {
		public GameObject gameObject = null;
		public GTrail.TTrailMode mode = GTrail.TTrailMode.standard;
		public int surfaceId_int = -1;
		public string surfaceId = "";
		public float startWidth = 0.0f, endWidth = 0.05f;
		public Color startColor = Color.grey, endColor = Color.clear;
		public string materialName = "Particles/Additive";
		public float forceThreshold = 100.0f;
		public float speedThreshold = 150.0f;
		public float heightThreshold = 80000.0f;
		public LineRenderer lineRenderer = null;
		public Vector3[] linePoints = null;
		public bool[] linePointsEnabled = null;
		public int linePoint = 0;
	}

	private LabeledSurfaceTrailDesc[] labeled_surfacetrails;
	private int labeled_surfacetrails_count = 0;
	private const int labeled_surfacetrails_maxcount = 100;

	private const int surfacetrails_points = 300;
	private int surfacetrails_interval_count = 0;
	private const int surfacetrails_interval_maxcount = 3;

	[HideInInspector]public float body_min_x = 0.0f;
	[HideInInspector]public float body_max_x = 0.0f;
	[HideInInspector]public float body_min_y = 0.0f;
	[HideInInspector]public float body_max_y = 0.0f;
	[HideInInspector]public float body_min_z = 0.0f;
	[HideInInspector]public float body_max_z = 0.0f;
	[HideInInspector]public Vector3 body_center;
	[HideInInspector]public float body_radius = 0.0f;
	
	public TSurfaceMethod kineticsSurfaceMethod = TSurfaceMethod.deltaFilteredWithPropwash;
	public float kineticsSurfaceDeltaFilter = 0.0f;

	private Vector3 kineticsInertiaTensor_original;
	private Vector3 kineticsInertiaTensor_tmp = Vector3.zero;
	private Vector3 kineticsInertiaTensor_lastValue = Vector3.zero;
	public Vector3 kineticsInertiaTensor = Vector3.zero;
	public Vector3 kineticsInertiaTensorScale = Vector3.one;
	public Vector3 kineticsMassRedistribution = Vector3.one;

	private float tmp_x = 0.0f, tmp_y = 0.0f, tmp_z = 0.0f, tmp_xz = 0.0f;
	private float tmp_x2 = 1.0f, tmp_y2 = 1.0f, tmp_z2 = 1.0f;
	private Vector3 tmp_v = Vector3.zero, tmp_v1 = Vector3.zero;
	private Quaternion tmp_q = Quaternion.identity;

	private float max_magnitude = 1.0f, max_magnitude_now = 0.0f;
	private float airdensity;
	private float kdrag, klift;
	private Vector3 lift, drag, lastPosition;
	private Vector3 neg_windspeed;
	private bool surfaceEnableThin = true, surfaceEnablePositive = true, surfaceEnableNegative = true;
	private bool surfaceEnableXPositive = false, surfaceEnableYPositive = false, surfaceEnableZPositive = false;
	private bool surfaceEnableXNegative = false, surfaceEnableYNegative = false, surfaceEnableZNegative = false;
	private GSurface.TSurfaceBehaviourType behaviourXPositive = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourYPositive = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourZPositive = GSurface.TSurfaceBehaviourType.parent_behaviour;
	private GSurface.TSurfaceBehaviourType behaviourXNegative = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourYNegative = GSurface.TSurfaceBehaviourType.parent_behaviour, behaviourZNegative = GSurface.TSurfaceBehaviourType.parent_behaviour;
	private bool surfaceDebug = false;
	private float cachedXScale = 1.0f, cachedYScale = 1.0f, cachedZScale = 1.0f;
	private float cachedXSurface = 1.0f, cachedYSurface = 1.0f, cachedZSurface = 1.0f;
	private float coefficientXDragWhenXPositiveFlow = 1.0f, coefficientYDragWhenYPositiveFlow = 1.0f, coefficientZDragWhenZPositiveFlow = 1.0f;
	private float coefficientXDragWhenXNegativeFlow = 1.0f, coefficientYDragWhenYNegativeFlow = 1.0f, coefficientZDragWhenZNegativeFlow = 1.0f;
	private float coefficientXLiftWhenYPositiveFlow = 0.0f, coefficientYLiftWhenXPositiveFlow = 0.0f, coefficientZLiftWhenXPositiveFlow = 0.1f;
	private float coefficientXLiftWhenYNegativeFlow = 0.0f, coefficientYLiftWhenXNegativeFlow = 0.0f, coefficientZLiftWhenXNegativeFlow = 0.1f;
	private float coefficientXLiftWhenZPositiveFlow = 0.0f, coefficientYLiftWhenZPositiveFlow = 0.0f, coefficientZLiftWhenYPositiveFlow = 0.1f;
	private float coefficientXLiftWhenZNegativeFlow = 0.0f, coefficientYLiftWhenZNegativeFlow = 0.0f, coefficientZLiftWhenYNegativeFlow = 0.1f;
	private Transform tmp_transform;
	private LineRenderer tmp_lineRenderer;
	private float tmp_propWashFactor;
	private float tmp_coefficientExponent;
	private Material tmp_lineRenderer_material;
	
	private Color kColorStall = new Color(1.0f, 1.0f, 0.6f, 0.25f);
	private Color kColorRed = new Color(0.9f, 0.2f, 0.2f, 0.25f);
	private Color kColorGreen = new Color(0.2f, 0.9f, 0.2f, 0.25f);
	private Color kColorBlue = new Color(0.2f, 0.2f, 0.9f, 0.25f);
	
	private float kineticsPropwash_internal = 0.0f;
	public float kineticsPropwashLimit = 0.25f;
	public float kineticsPropwashMultiplier = 1.05f;

	private Vector3 kineticsAngularVelocity_lastValue = Vector3.zero;
	public bool kineticsFiltersEnabled = true;
	public float kineticsDragFilter = 1.0f;
	public float kineticsDragForceByMassUnitLimit = 999999.999999f;
	public float kineticsLiftFilter = 1.0f;
	public float kineticsLiftForceByMassUnitLimit = 999999.999999f;
	public float kineticsAngularVelocityLimit = 2.0f;
	public float kineticsAngularVelocityFilter = 1.0f;

	public TGroundEffectType kineticsGroundEffect = TGroundEffectType.none;
	private float kineticsGroundEffectForce = 0.0f;
	public LayerMask kineticsGroundEffectLayermask = -24;
	public float kineticsGroundEffectProbeMinDistance = 0.01f;
	public float kineticsGroundEffectProbeMaxDistance = 20.0f;
	public float kineticsGroundEffectMaxValue = 10.0f;
	public float kineticsGroundEffectCoeficient = 0.01f;
	public float kineticsGroundEffectCoeficientDistance = 10.0f;
	
	[HideInInspector]public float gaugesShaft_output = 0.0f;
	[HideInInspector]public float gaugesAltimeter_output = 0.0f;
	public bool gaugesAltimeterGenerate = true;
	[HideInInspector]public float gaugesVario_output = 0.0f;
	private float gaugesVario_lastValue = 0.0f;
	public bool gaugesVarioGenerate = true;
	[HideInInspector]public float gaugesRpm_output = 0.0f;
	private float gaugesRpm_lastValue = 0.0f;
	public bool gaugesRpmGenerate = true;
	[HideInInspector]public float gaugesAirspeed_output = 0.0f;
	public bool gaugesAirspeedGenerate = true;
	[HideInInspector]public float gaugesHeading_output = 0.0f;
	public bool gaugesHeadingGenerate = true;
	[HideInInspector]public float gaugesGs_output = 0.0f;
	[HideInInspector]public float gaugesHGs_output = 0.0f;
	private Vector3 gaugesGs_lastValue = Vector3.zero;
	public bool gaugesGsGenerate = true;
	
	private Renderer engine_renderer;
	private float total_engine_force = 0.0f;

	private Vector3 calcXScale_xp = new Vector3(0.5f, 0.0f, 0.0f);
	private Vector3 calcXScale_xn = new Vector3(-0.5f, 0.0f, 0.0f);
	private Vector3 calcYScale_yp = new Vector3(0.0f, 0.5f, 0.0f);
	private Vector3 calcYScale_yn = new Vector3(0.0f, -0.5f, 0.0f);
	private Vector3 calcZScale_zp = new Vector3(0.0f, 0.0f, 0.5f);
	private Vector3 calcZScale_zn = new Vector3(0.0f, 0.0f, -0.5f);
	
	public bool log(string msg) {
		if (globalDebugNodes) {
			Debug.Log(msg);
			return true;
		} else {
			return false;
		}
	}
	
	System.Random vibratorsRnd = null;
	float vibratorsRndValidFor = 0.0f;
	//[System.Serializable]public class VibratorSubvibratorDesc {
	public class VibratorSubvibratorDesc {
		public float totalTime = 0.0f;
		public float remainingTime = 0.0f;
		public float frequency = 0.0f;
		public float amplitude = 0.0f;
	}
	//[System.Serializable]public class VibratorDesc {
	public class VibratorDesc {
		public float maxTime = 15.0f;
		public float minFrequency = 0.150f;
		public float maxFrequency = 750.0f;
		public float expFrequency = 2.500f;
		public float maxAmplitude = 1.000f;
		public float amplitudeToFrequencyRatio = 0.150f;
		public float saturationAt = 0.5f;
		public int subvibratorsCount = 0;
		public VibratorSubvibratorDesc[] subvibrators = null;
	}
	[HideInInspector]public VibratorDesc[] vibrators = null;
	[HideInInspector]public int vibratorCount = 16;
	[HideInInspector]public int vibratorDefault_subvibratorCount = 3;
	[HideInInspector]public int vibrator0_subvibratorCount = 3;
	[HideInInspector]public int vibrator1_subvibratorCount = 3;
	[HideInInspector]public int vibrator2_subvibratorCount = 3;
	[HideInInspector]public int vibrator3_subvibratorCount = 3;
	public float vibration(int vibratorid) {
		return vibration(vibratorid, Time.fixedDeltaTime);
	}
	public float vibration(int vibratorid, float deltaTime) {
		if (vibratorid >= vibratorCount) return 0.0f;
		if (vibrators == null) vibrators = new VibratorDesc[vibratorCount];
		if (vibrators[vibratorid] == null) {
			vibrators[vibratorid] = new VibratorDesc();
			switch (vibratorid) {
			default: vibrators[vibratorid].subvibratorsCount = vibratorDefault_subvibratorCount; break;
			case 0: vibrators[vibratorid].subvibratorsCount = vibrator0_subvibratorCount; break;
			case 1: vibrators[vibratorid].subvibratorsCount = vibrator1_subvibratorCount; break;
			case 2: vibrators[vibratorid].subvibratorsCount = vibrator2_subvibratorCount; break;
			case 3: vibrators[vibratorid].subvibratorsCount = vibrator3_subvibratorCount; break;
			}
			vibrators[vibratorid].subvibrators = new VibratorSubvibratorDesc[vibrators[vibratorid].subvibratorsCount];
			for (int i = 0; i < vibrators[vibratorid].subvibratorsCount; ++i) vibrators[vibratorid].subvibrators[i] = new VibratorSubvibratorDesc();
		}
		if ((vibratorsRnd == null) || (vibratorsRndValidFor <= 0.0f)) {
			vibratorsRnd = new System.Random(Mathf.FloorToInt(Time.realtimeSinceStartup * 1000.0f));
			vibratorsRndValidFor = (float)vibratorsRnd.NextDouble() * 60.0f;
		} else {
			vibratorsRndValidFor -= deltaTime;
		}
		float output = 0.0f;
		for (int i = 0; i < vibrators[vibratorid].subvibratorsCount; ++i) {
			if (vibrators[vibratorid].subvibrators[i].remainingTime <= 0) {
				vibrators[vibratorid].subvibrators[i].totalTime = (float)vibratorsRnd.NextDouble() * vibrators[vibratorid].maxTime;
				vibrators[vibratorid].subvibrators[i].remainingTime = vibrators[vibratorid].subvibrators[i].totalTime;
				vibrators[vibratorid].subvibrators[i].frequency = vibrators[vibratorid].minFrequency + (1.0f - Mathf.Pow((float)vibratorsRnd.NextDouble(), vibrators[vibratorid].expFrequency)) * (vibrators[vibratorid].maxFrequency - vibrators[vibratorid].minFrequency);
				vibrators[vibratorid].subvibrators[i].amplitude = (float)vibratorsRnd.NextDouble() * vibrators[vibratorid].maxAmplitude / Mathf.Pow(vibrators[vibratorid].subvibrators[i].frequency, vibrators[vibratorid].amplitudeToFrequencyRatio);
			} else {
				vibrators[vibratorid].subvibrators[i].remainingTime -= deltaTime;
			}
			output += Mathf.Sin(vibrators[vibratorid].subvibrators[i].remainingTime / vibrators[vibratorid].subvibrators[i].totalTime * Mathf.PI) * vibrators[vibratorid].subvibrators[i].amplitude * Mathf.Sin(vibrators[vibratorid].subvibrators[i].remainingTime * vibrators[vibratorid].subvibrators[i].frequency);
		}
		if (output < -vibrators[vibratorid].saturationAt) output = -vibrators[vibratorid].saturationAt;
		if (output > vibrators[vibratorid].saturationAt) output = vibrators[vibratorid].saturationAt;
		return output;
	}

	public float calcXScale(GameObject obj) {
		return (obj.transform.TransformPoint(calcXScale_xp) - obj.transform.TransformPoint(calcXScale_xn)).magnitude / globalSimulationScale;
	}
	public float calcYScale(GameObject obj) {
		return (obj.transform.TransformPoint(calcYScale_yp) - obj.transform.TransformPoint(calcYScale_yn)).magnitude / globalSimulationScale;
	}
	public float calcZScale(GameObject obj) {
		return (obj.transform.TransformPoint(calcZScale_zp) - obj.transform.TransformPoint(calcZScale_zn)).magnitude / globalSimulationScale;
	}

	bool FindIsSet(string s, string prefix, string suffix) {
		int cp, cpe;
		int prefixLength = prefix.Length;
		if ((cp = s.IndexOf(prefix)) > 0) {
			cpe = s.IndexOf(suffix, cp + prefixLength);
			if (cpe > 0) {
				return true;
			}
		}
		return false;
	}
	bool FindIsSetIgnoreCase(string s, string prefix, string suffix) {
		return FindIsSet(s.ToLower(), prefix.ToLower(), suffix.ToLower());
	}
	bool ReplaceIsSetIgnoreCase(string s, string id) {
		if (FindIsSetIgnoreCase(s, "_" + id + ":", "_")) {
			return true;
		} else {
			return false;
		}
	}
	static bool FindFloat(string s, string prefix, string suffix, out float ret_value) {
		int cp, cpe;
		int prefixLength = prefix.Length;
		if ((cp = s.IndexOf(prefix)) > 0) {
			cpe = s.IndexOf(suffix, cp + prefixLength);
			if (cpe > 0) {
				float.TryParse(s.Substring(cp + prefixLength, cpe - cp - prefixLength), out ret_value);
				return true;
			}
		}
		ret_value = 0.0f;
		return false;
	}
	static bool FindFloatIgnoreCase(string s, string prefix, string suffix, out float ret_value) {
		return FindFloat(s.ToLower(), prefix.ToLower(), suffix.ToLower(), out ret_value);
	}
	public static float ReplaceFloatIgnoreCase(string s, string id, float defaultvalue) {
		float tmpv;
		if (FindFloatIgnoreCase(s, "_" + id + ":", "_", out tmpv)) {
			if (singleton != null) singleton.log("    " + s + " " + id + " is [" + tmpv.ToString() + "]");
		} else {
			tmpv = defaultvalue;
		}
		return tmpv;
	}
	static bool FindString(string s, string prefix, string suffix, out string ret_value) {
		int cp, cpe;
		int prefixLength = prefix.Length;
		if ((cp = s.IndexOf(prefix)) > 0) {
			cpe = s.IndexOf(suffix, cp + prefixLength);
			if (cpe > 0) {
				ret_value = s.Substring(cp + prefixLength, cpe - cp - prefixLength);
				return true;
			}
		}
		ret_value = "";
		return false;
	}
	static bool FindStringIgnoreCase(string s, string prefix, string suffix, out string ret_value) {
		return FindString(s.ToLower(), prefix.ToLower(), suffix.ToLower(), out ret_value);
	}
	public static string ReplaceStringIgnoreCase(string s, string id, string defaultvalue) {
		string tmps;
		if (FindStringIgnoreCase(s, "_" + id + ":", "_", out tmps)) {
			if (singleton != null) singleton.log("    " + s + " " + id + " is [" + tmps + "]");
		} else {
			tmps = defaultvalue;
		}
		return tmps;
	}
	bool FindBool(string s, string prefix, string suffix, out bool ret_value) {
		string tmps;
		bool retv = FindString(s, prefix, suffix, out tmps);
		tmps = tmps.ToLower();
		if ((tmps.Equals("true")) || (tmps.Equals("1")) || (tmps.Equals("enabled"))) ret_value = true;
		else ret_value = false;
		return retv;
	}
	bool FindBoolIgnoreCase(string s, string prefix, string suffix, out bool ret_value) {
		return FindBool(s.ToLower(), prefix.ToLower(), suffix.ToLower(), out ret_value);
	}
	bool ReplaceBoolIgnoreCase(string s, string id, bool defaultvalue) {
		bool tmpb;
		if (FindBoolIgnoreCase(s, "_" + id + ":", "_", out tmpb)) {
			log("    " + s + " " + id + " is [" + (tmpb ? "true" : "false") + "]");
		} else {
			tmpb = defaultvalue;
		}
		return tmpb;
	}
	static public GAircraft findGAircraft(GameObject obj, int maxdeep) {
		GAircraft gsm = (GAircraft)obj.GetComponent("GAircraft");
		if (gsm != null) return gsm;
		if (obj.transform.parent == null) return null;
		if (maxdeep <= 0) return null;
		--maxdeep;
		return findGAircraft(obj.transform.parent.gameObject, maxdeep);
	}
	void SortCameras() {
		LabeledSurfaceMiscDesc tmp;
		int lastCameraPosition;
		int lastCameraCount = labeled_surfacemisc_count;
		for (int j = 0; j < lastCameraCount; ++j) {
			lastCameraPosition = -1;
			lastCameraCount = 0;
			for (int i = 0; i < labeled_surfacemisc_count; ++i) {
				if (labeled_surfacemisc[i].cameraPosition) {
					if (lastCameraPosition >= 0) {
						if (String.Compare(labeled_surfacemisc[lastCameraPosition].gameObject.name, labeled_surfacemisc[i].gameObject.name, true) > 0) {
							tmp = labeled_surfacemisc[lastCameraPosition];
							labeled_surfacemisc[lastCameraPosition] = labeled_surfacemisc[i];
							labeled_surfacemisc[i] = tmp;
						}
					}
					lastCameraPosition = i;
					++lastCameraCount;
				}
			}
		}
	}
	void FindNodes(GameObject e, int r) {
		GameObject ch;
		string tmps;
		if (r == 0) {
			surfaces = new GSurface[surfaces_maxcount];
			surfaces_count = 0;
			labeled_surfaces = new LabeledSurfaceDesc[labeled_surfaces_maxcount];
			labeled_surfaces_count = 0;
			surfacepivots = new GPivot[surfacepivots_maxcount];
			surfacepivots_count = 0;
			labeled_surfacepivots = new LabeledSurfacePivotDesc[labeled_surfacepivots_maxcount];
			labeled_surfacepivots_count = 0;
			surfacedrives = new GDrive[surfacedrives_maxcount];
			surfacedrives_count = 0;
			labeled_surfacedrives = new LabeledSurfaceDriveDesc[labeled_surfacedrives_maxcount];
			labeled_surfacedrives_count = 0;
			labeled_surfacemisc = new LabeledSurfaceMiscDesc[labeled_surfacemisc_maxcount];
			labeled_surfacemisc_count = 0;
			center = gameObject.GetComponent<Rigidbody>().centerOfMass;
		}
		for (int i = 0; i < e.transform.childCount; ++i) {
			ch = e.transform.GetChild(i).gameObject;
			if (ch.activeSelf) {
				GSurface sf = (GSurface)ch.GetComponent("GSurface");
				GPivot sp = (GPivot)ch.GetComponent("GPivot");
				GDrive sd = (GDrive)ch.GetComponent("GDrive");
				GCenter sc = (GCenter)ch.GetComponent("GCenter");
				if (sf != null) {
					tmp_v1 = gameObject.transform.InverseTransformPoint(ch.transform.position);
					if (tmp_v1.x < body_min_x) body_min_x = tmp_v1.x;
					if (tmp_v1.x > body_max_x) body_max_x = tmp_v1.x;
					if (tmp_v1.y < body_min_y) body_min_y = tmp_v1.y;
					if (tmp_v1.y > body_max_y) body_max_y = tmp_v1.y;
					if (tmp_v1.z < body_min_z) body_min_z = tmp_v1.z;
					if (tmp_v1.z > body_max_z) body_max_z = tmp_v1.z;
					if (surfaces_count < surfaces_maxcount) {
						surfaces[surfaces_count] = sf;
						surfaces[surfaces_count].lastPosition = ch.transform.position;
						surfaces[surfaces_count].cachedXScale = calcXScale(ch);
						surfaces[surfaces_count].cachedYScale = calcYScale(ch);
						surfaces[surfaces_count].cachedZScale = calcZScale(ch);
						surfaces[surfaces_count].cachedXSurface = surfaces[surfaces_count].cachedYScale * surfaces[surfaces_count].cachedZScale;
						surfaces[surfaces_count].cachedYSurface = surfaces[surfaces_count].cachedXScale * surfaces[surfaces_count].cachedZScale;
						surfaces[surfaces_count].cachedZSurface = surfaces[surfaces_count].cachedXScale * surfaces[surfaces_count].cachedYScale;
						if (ch.GetComponent("LineRenderer") == null) ch.AddComponent<LineRenderer>();
						surfaces[surfaces_count].lineRenderer = (LineRenderer)ch.GetComponent("LineRenderer");
						++surfaces_count;
					}
					log(ch.name + " has GSurface");
					if (!globalRenderPhysicSurfaces) ch.GetComponent<Renderer>().enabled = false;
				} else if (ch.name.Contains("_surface_")) {
					tmp_v1 = gameObject.transform.InverseTransformPoint(ch.transform.position);
					if (tmp_v1.x < body_min_x) body_min_x = tmp_v1.x;
					if (tmp_v1.x > body_max_x) body_max_x = tmp_v1.x;
					if (tmp_v1.y < body_min_y) body_min_y = tmp_v1.y;
					if (tmp_v1.y > body_max_y) body_max_y = tmp_v1.y;
					if (tmp_v1.z < body_min_z) body_min_z = tmp_v1.z;
					if (tmp_v1.z > body_max_z) body_max_z = tmp_v1.z;
					log(ch.name + " has <Labeled>Surface");
					if (labeled_surfaces_count < labeled_surfaces_maxcount) {
						labeled_surfaces[labeled_surfaces_count] = new LabeledSurfaceDesc();
						labeled_surfaces[labeled_surfaces_count].gameObject = ch;
						labeled_surfaces[labeled_surfaces_count].lastPosition = labeled_surfaces[labeled_surfaces_count].gameObject.transform.position;
						labeled_surfaces[labeled_surfaces_count].cachedXScale = calcXScale(ch);
						labeled_surfaces[labeled_surfaces_count].cachedYScale = calcYScale(ch);
						labeled_surfaces[labeled_surfaces_count].cachedZScale = calcZScale(ch);
						labeled_surfaces[labeled_surfaces_count].cachedXSurface = labeled_surfaces[labeled_surfaces_count].cachedYScale * labeled_surfaces[labeled_surfaces_count].cachedZScale;
						labeled_surfaces[labeled_surfaces_count].cachedYSurface = labeled_surfaces[labeled_surfaces_count].cachedXScale * labeled_surfaces[labeled_surfaces_count].cachedZScale;
						labeled_surfaces[labeled_surfaces_count].cachedZSurface = labeled_surfaces[labeled_surfaces_count].cachedXScale * labeled_surfaces[labeled_surfaces_count].cachedYScale;
						if (FindString(ch.name, "_id:", "_", out tmps)) {
							labeled_surfaces[labeled_surfaces_count].id = tmps;
							log("  " + ch.name + " id is [" + tmps + "]");
						}
						if (ch.name.Contains("_nolift_")) {
							labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenYPositiveFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenYNegativeFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenZPositiveFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenZNegativeFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenXPositiveFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenXNegativeFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenZPositiveFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenZNegativeFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenXPositiveFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenXNegativeFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenYPositiveFlow = 0.0f;
							labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenYNegativeFlow = 0.0f;
						}
						if (ch.name.Contains("_laminarflow_") || ch.name.Contains("_laminarflowx_")) {
							labeled_surfaces[labeled_surfaces_count].behaviourXPositive = GSurface.TSurfaceBehaviourType.laminar_analysis;
							labeled_surfaces[labeled_surfaces_count].behaviourXNegative = GSurface.TSurfaceBehaviourType.laminar_analysis;
						}
						if (ch.name.Contains("_laminarflow_") || ch.name.Contains("_laminarflowy_")) {
							labeled_surfaces[labeled_surfaces_count].behaviourYPositive = GSurface.TSurfaceBehaviourType.laminar_analysis;
							labeled_surfaces[labeled_surfaces_count].behaviourYNegative = GSurface.TSurfaceBehaviourType.laminar_analysis;
						}
						if (ch.name.Contains("_laminarflow_") || ch.name.Contains("_laminarflowz_")) {
							labeled_surfaces[labeled_surfaces_count].behaviourZPositive = GSurface.TSurfaceBehaviourType.laminar_analysis;
							labeled_surfaces[labeled_surfaces_count].behaviourZNegative = GSurface.TSurfaceBehaviourType.laminar_analysis;
						}
						if (ch.name.Contains("_debug_")) labeled_surfaces[labeled_surfaces_count].surfaceDebug = true;
						labeled_surfaces[labeled_surfaces_count].id = ReplaceStringIgnoreCase(ch.name, "id", labeled_surfaces[labeled_surfaces_count].id);
						labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shape", labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shape", labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shape", labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shape", labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shape", labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shape", labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapex", labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapex", labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapey", labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapey", labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapez", labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapez", labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapexpositive", labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapexnegative", labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapeypositive", labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapeynegative", labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapezpositive", labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapeznegative", labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapeXPositiveParameter", labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapeXNegativeParameter", labeled_surfaces[labeled_surfaces_count].shapeXNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapeYPositiveParameter", labeled_surfaces[labeled_surfaces_count].shapeYPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapeYNegativeParameter", labeled_surfaces[labeled_surfaces_count].shapeYNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter = ReplaceStringIgnoreCase(ch.name, "shapeZPositiveParameter", labeled_surfaces[labeled_surfaces_count].shapeZPositiveParameter);
						labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter = ReplaceStringIgnoreCase(ch.name, "shapeZNegativeParameter", labeled_surfaces[labeled_surfaces_count].shapeZNegativeParameter);
						labeled_surfaces[labeled_surfaces_count].shapeXPositive = GSurface.shapeTypeFromParameter(labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter, labeled_surfaces[labeled_surfaces_count].shapeXPositive);
						labeled_surfaces[labeled_surfaces_count].shapeXNegative = GSurface.shapeTypeFromParameter(labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter, labeled_surfaces[labeled_surfaces_count].shapeXNegative);
						labeled_surfaces[labeled_surfaces_count].shapeYPositive = GSurface.shapeTypeFromParameter(labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter, labeled_surfaces[labeled_surfaces_count].shapeYPositive);
						labeled_surfaces[labeled_surfaces_count].shapeYNegative = GSurface.shapeTypeFromParameter(labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter, labeled_surfaces[labeled_surfaces_count].shapeYNegative);
						labeled_surfaces[labeled_surfaces_count].shapeZPositive = GSurface.shapeTypeFromParameter(labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter, labeled_surfaces[labeled_surfaces_count].shapeZPositive);
						labeled_surfaces[labeled_surfaces_count].shapeZNegative = GSurface.shapeTypeFromParameter(labeled_surfaces[labeled_surfaces_count].shapeXPositiveParameter, labeled_surfaces[labeled_surfaces_count].shapeZNegative);
						log(
							"  " + ch.name + " shape is" +
							" [x+" + GSurface.fromTSurfaceShapeType(labeled_surfaces[labeled_surfaces_count].shapeXPositive) +
							", x-" + GSurface.fromTSurfaceShapeType(labeled_surfaces[labeled_surfaces_count].shapeXNegative) +
							", y+" + GSurface.fromTSurfaceShapeType(labeled_surfaces[labeled_surfaces_count].shapeYPositive) +
							", y-" + GSurface.fromTSurfaceShapeType(labeled_surfaces[labeled_surfaces_count].shapeYNegative) +
							", z+" + GSurface.fromTSurfaceShapeType(labeled_surfaces[labeled_surfaces_count].shapeZPositive) +
							", z-" + GSurface.fromTSurfaceShapeType(labeled_surfaces[labeled_surfaces_count].shapeZNegative) +
							"]"
						);
						labeled_surfaces[labeled_surfaces_count].surfaceEnable = ReplaceBoolIgnoreCase(ch.name, "surfaceEnable", labeled_surfaces[labeled_surfaces_count].surfaceEnable);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableThin = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableThin", labeled_surfaces[labeled_surfaces_count].surfaceEnableThin);
						labeled_surfaces[labeled_surfaces_count].surfaceEnablePositive = ReplaceBoolIgnoreCase(ch.name, "surfaceEnablePositive", labeled_surfaces[labeled_surfaces_count].surfaceEnablePositive);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableNegative = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableNegative", labeled_surfaces[labeled_surfaces_count].surfaceEnableNegative);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableXPositive = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableXPositive", labeled_surfaces[labeled_surfaces_count].surfaceEnableXPositive);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableYPositive = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableYPositive", labeled_surfaces[labeled_surfaces_count].surfaceEnableYPositive);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableZPositive = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableZPositive", labeled_surfaces[labeled_surfaces_count].surfaceEnableZPositive);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableXNegative = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableXNegative", labeled_surfaces[labeled_surfaces_count].surfaceEnableXNegative);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableYNegative = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableYNegative", labeled_surfaces[labeled_surfaces_count].surfaceEnableYNegative);
						labeled_surfaces[labeled_surfaces_count].surfaceEnableZNegative = ReplaceBoolIgnoreCase(ch.name, "surfaceEnableZNegative", labeled_surfaces[labeled_surfaces_count].surfaceEnableZNegative);
						labeled_surfaces[labeled_surfaces_count].coefficientXDragWhenXPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientXDrag", labeled_surfaces[labeled_surfaces_count].coefficientXDragWhenXPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientYDragWhenYPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientYDrag", labeled_surfaces[labeled_surfaces_count].coefficientYDragWhenYPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientZDragWhenZPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientZDrag", labeled_surfaces[labeled_surfaces_count].coefficientZDragWhenZPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientXDragWhenXNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientXDrag", labeled_surfaces[labeled_surfaces_count].coefficientXDragWhenXNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientYDragWhenYNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientYDrag", labeled_surfaces[labeled_surfaces_count].coefficientYDragWhenYNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientZDragWhenZNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientZDrag", labeled_surfaces[labeled_surfaces_count].coefficientZDragWhenZNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenYPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientXLiftWhenYPositiveFlow", labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenYPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenYNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientXLiftWhenYNegativeFlow", labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenYNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenZPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientXLiftWhenZPositiveFlow", labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenZPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenZNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientXLiftWhenZNegativeFlow", labeled_surfaces[labeled_surfaces_count].coefficientXLiftWhenZNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenXPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientYLiftWhenXPositiveFlow", labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenXPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenXNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientYLiftWhenXNegativeFlow", labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenXNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenZPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientYLiftWhenZPositiveFlow", labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenZPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenZNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientYLiftWhenZNegativeFlow", labeled_surfaces[labeled_surfaces_count].coefficientYLiftWhenZNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenXPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientZLiftWhenXPositiveFlow", labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenXPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenXNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientZLiftWhenXNegativeFlow", labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenXNegativeFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenYPositiveFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientZLiftWhenYPositiveFlow", labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenYPositiveFlow);
						labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenYNegativeFlow = ReplaceFloatIgnoreCase(ch.name, "coefficientZLiftWhenYNegativeFlow", labeled_surfaces[labeled_surfaces_count].coefficientZLiftWhenYNegativeFlow);
						if (ch.GetComponent("LineRenderer") == null) ch.AddComponent<LineRenderer>();
						labeled_surfaces[labeled_surfaces_count].lineRenderer = (LineRenderer)ch.GetComponent("LineRenderer");
						labeled_surfaces[labeled_surfaces_count].propWashFactor = ReplaceFloatIgnoreCase(ch.name, "wash", labeled_surfaces[labeled_surfaces_count].propWashFactor);
						labeled_surfaces[labeled_surfaces_count].propWashFactor = ReplaceFloatIgnoreCase(ch.name, "propWashFactor", labeled_surfaces[labeled_surfaces_count].propWashFactor);
						labeled_surfaces[labeled_surfaces_count].coefficientExponent = ReplaceFloatIgnoreCase(ch.name, "exponent", labeled_surfaces[labeled_surfaces_count].coefficientExponent);
						labeled_surfaces[labeled_surfaces_count].coefficientExponent = ReplaceFloatIgnoreCase(ch.name, "coefficientExponent", labeled_surfaces[labeled_surfaces_count].coefficientExponent);
						
						++labeled_surfaces_count;
					}
					if (ch.name.Contains("_hide_")) ch.GetComponent<Renderer>().enabled = false;
					else if (!globalRenderPhysicSurfaces) ch.GetComponent<Renderer>().enabled = false;
				} else if (ch.name.Contains("_hide_")) {
					ch.GetComponent<Renderer>().enabled = false;
				}
				if (sp != null) {
					if (surfacepivots_count < surfacepivots_maxcount) surfacepivots[surfacepivots_count++] = sp;
					log(ch.name + " has GPivot");
					//if (!gRenderBoxes_k) ch.renderer.enabled = false;
				} else if (ch.name.Contains("_pivot_") || ch.name.Contains("_pivotforward_") || ch.name.Contains("_pivotfwd_") || ch.name.Contains("_pivotup_") || ch.name.Contains("_pivotright_") || ch.name.Contains("_pivot:forward_") || ch.name.Contains("_pivot:fwd_") || ch.name.Contains("_pivot:up_") || ch.name.Contains("_pivot:right_")) {
					log(ch.name + " has <Labeled>Pivot");
					if (labeled_surfacepivots_count < labeled_surfacepivots_maxcount) {
						labeled_surfacepivots[labeled_surfacepivots_count] = new LabeledSurfacePivotDesc();
						labeled_surfacepivots[labeled_surfacepivots_count].gameObject = ch;
						if (ch.name.Contains("_forward_") || ch.name.Contains("_fwd_") || ch.name.Contains("_pivotforward_") || ch.name.Contains("_pivotfwd_") || ch.name.Contains("_pivot:forward_") || ch.name.Contains("_pivot:fwd_")) { labeled_surfacepivots[labeled_surfacepivots_count].rotationPivotAxis = GPivot.TAxisOrientation.forward; log("  " + ch.name + " axis_rotation is <forward>"); }
						if (ch.name.Contains("_up_") || ch.name.Contains("_pivotup_") || ch.name.Contains("_pivot:up_")) { labeled_surfacepivots[labeled_surfacepivots_count].rotationPivotAxis = GPivot.TAxisOrientation.up; log("  " + ch.name + " axis_rotation is <up>"); }
						if (ch.name.Contains("_right_") || ch.name.Contains("_pivotright_") || ch.name.Contains("_pivot:right_")) { labeled_surfacepivots[labeled_surfacepivots_count].rotationPivotAxis = GPivot.TAxisOrientation.right; log("  " + ch.name + " axis_rotation is <right>"); }
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset = ReplaceFloatIgnoreCase(ch.name, "offsetfwd", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset = ReplaceFloatIgnoreCase(ch.name, "offsetforward", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundUpOffset = ReplaceFloatIgnoreCase(ch.name, "offsetup", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundUpOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundRightOffset = ReplaceFloatIgnoreCase(ch.name, "offsetright", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundRightOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset = ReplaceFloatIgnoreCase(ch.name, "fwdoffset", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset = ReplaceFloatIgnoreCase(ch.name, "forwardoffset", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset = ReplaceFloatIgnoreCase(ch.name, "rotationAroundForwardOffset", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundForwardOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundUpOffset = ReplaceFloatIgnoreCase(ch.name, "upoffset", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundUpOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundUpOffset = ReplaceFloatIgnoreCase(ch.name, "rotationAroundUpOffset", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundUpOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundRightOffset = ReplaceFloatIgnoreCase(ch.name, "rightoffset", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundRightOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundRightOffset = ReplaceFloatIgnoreCase(ch.name, "rotationAroundRightOffset", labeled_surfacepivots[labeled_surfacepivots_count].rotationAroundRightOffset);
						labeled_surfacepivots[labeled_surfacepivots_count].ch1Source = GPivot.toTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch1SourceName = ReplaceStringIgnoreCase(ch.name, "axis1", (GPivot.fromTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch1Source).Equals("any")) ? labeled_surfacepivots[labeled_surfacepivots_count].ch1SourceName : GPivot.fromTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch1Source)));
						//if (labeled_surfacepivots[labeled_surfacepivots_count].ch1Source != GPivot.TAxisSource.any) if (labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotTurnsPerUnit == 0.0f) labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotTurnsPerUnit = 1.0f;
						labeled_surfacepivots[labeled_surfacepivots_count].ch1SourceName = ReplaceStringIgnoreCase(ch.name, "ch1SourceName", labeled_surfacepivots[labeled_surfacepivots_count].ch1SourceName);
						labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMin = ReplaceFloatIgnoreCase(ch.name, "axis1min", labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMin);
						labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMin = ReplaceFloatIgnoreCase(ch.name, "ch1PivotAngleWhenMin", labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMin);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "axis1min", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "ch1PivotAngleWhenMin", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "axis1max", labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "ch1PivotAngleWhenMax", labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotTurnsPerUnit = ReplaceFloatIgnoreCase(ch.name, "axis1mul", labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotTurnsPerUnit);
						labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotTurnsPerUnit = ReplaceFloatIgnoreCase(ch.name, "ch1PivotTurnsPerUnit", labeled_surfacepivots[labeled_surfacepivots_count].ch1PivotTurnsPerUnit);
						labeled_surfacepivots[labeled_surfacepivots_count].ch2Source = GPivot.toTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch2SourceName = ReplaceStringIgnoreCase(ch.name, "axis2", (GPivot.fromTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch2Source).Equals("any")) ? labeled_surfacepivots[labeled_surfacepivots_count].ch2SourceName : GPivot.fromTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch2Source)));
						//if (labeled_surfacepivots[labeled_surfacepivots_count].ch2Source != GPivot.TAxisSource.any) if (labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotTurnsPerUnit == 0.0f) labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotTurnsPerUnit = 1.0f;
						labeled_surfacepivots[labeled_surfacepivots_count].ch2SourceName = ReplaceStringIgnoreCase(ch.name, "ch2SourceName", labeled_surfacepivots[labeled_surfacepivots_count].ch2SourceName);
						labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMin = ReplaceFloatIgnoreCase(ch.name, "axis2min", labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMin);
						labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMin = ReplaceFloatIgnoreCase(ch.name, "ch2PivotAngleWhenMin", labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMin);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "axis2min", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "ch2PivotAngleWhenMin", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "axis2max", labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "ch2PivotAngleWhenMax", labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotTurnsPerUnit = ReplaceFloatIgnoreCase(ch.name, "axis2mul", labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotTurnsPerUnit);
						labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotTurnsPerUnit = ReplaceFloatIgnoreCase(ch.name, "ch2PivotTurnsPerUnit", labeled_surfacepivots[labeled_surfacepivots_count].ch2PivotTurnsPerUnit);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3Source = GPivot.toTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch3SourceName = ReplaceStringIgnoreCase(ch.name, "axis3", (GPivot.fromTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch3Source).Equals("any")) ? labeled_surfacepivots[labeled_surfacepivots_count].ch3SourceName : GPivot.fromTAxisSource(labeled_surfacepivots[labeled_surfacepivots_count].ch3Source)));
						//if (labeled_surfacepivots[labeled_surfacepivots_count].ch3Source != GPivot.TAxisSource.any) if (labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotTurnsPerUnit == 0.0f) labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotTurnsPerUnit = 1.0f;
						labeled_surfacepivots[labeled_surfacepivots_count].ch3SourceName = ReplaceStringIgnoreCase(ch.name, "ch3SourceName", labeled_surfacepivots[labeled_surfacepivots_count].ch3SourceName);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMin = ReplaceFloatIgnoreCase(ch.name, "axis3min", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMin);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMin = ReplaceFloatIgnoreCase(ch.name, "ch3PivotAngleWhenMin", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMin);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "axis3min", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "ch3PivotAngleWhenMin", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "axis3max", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax = ReplaceFloatIgnoreCase(ch.name, "ch3PivotAngleWhenMax", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotAngleWhenMax);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotTurnsPerUnit = ReplaceFloatIgnoreCase(ch.name, "axis3mul", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotTurnsPerUnit);
						labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotTurnsPerUnit = ReplaceFloatIgnoreCase(ch.name, "ch3PivotTurnsPerUnit", labeled_surfacepivots[labeled_surfacepivots_count].ch3PivotTurnsPerUnit);
						labeled_surfacepivots[labeled_surfacepivots_count].limitMin = ReplaceFloatIgnoreCase(ch.name, "min", labeled_surfacepivots[labeled_surfacepivots_count].limitMin);
						labeled_surfacepivots[labeled_surfacepivots_count].limitMin = ReplaceFloatIgnoreCase(ch.name, "limitMin", labeled_surfacepivots[labeled_surfacepivots_count].limitMin);
						labeled_surfacepivots[labeled_surfacepivots_count].limitMax = ReplaceFloatIgnoreCase(ch.name, "max", labeled_surfacepivots[labeled_surfacepivots_count].limitMax);
						labeled_surfacepivots[labeled_surfacepivots_count].limitMax = ReplaceFloatIgnoreCase(ch.name, "limitMax", labeled_surfacepivots[labeled_surfacepivots_count].limitMax);
						
						++labeled_surfacepivots_count;
					}
				}
				if (ch.name.Contains("_enginelospeed_")) {
					if (labeled_surfacemisc_count < labeled_surfacemisc_maxcount) {
						labeled_surfacemisc[labeled_surfacemisc_count] = new LabeledSurfaceMiscDesc();
						labeled_surfacemisc[labeled_surfacemisc_count].gameObject = ch;
						labeled_surfacemisc[labeled_surfacemisc_count].lospeed = true;
						log(ch.name + " has <Labeled>LoSpeed");
						labeled_surfacemisc[labeled_surfacemisc_count].pivotId = ReplaceStringIgnoreCase(ch.name, "pivotId", labeled_surfacemisc[labeled_surfacemisc_count].pivotId);
						labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold = ReplaceFloatIgnoreCase(ch.name, "rpmThreshold", labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold);
						++labeled_surfacemisc_count;
					}
				} else if (ch.name.Contains("_enginehispeed_")) {
					if (labeled_surfacemisc_count < labeled_surfacemisc_maxcount) {
						labeled_surfacemisc[labeled_surfacemisc_count] = new LabeledSurfaceMiscDesc();
						labeled_surfacemisc[labeled_surfacemisc_count].gameObject = ch;
						labeled_surfacemisc[labeled_surfacemisc_count].hispeed = true;
						log(ch.name + " has <Labeled>HiSpeed");
						labeled_surfacemisc[labeled_surfacemisc_count].pivotId = ReplaceStringIgnoreCase(ch.name, "pivotId", labeled_surfacemisc[labeled_surfacemisc_count].pivotId);
						labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold = ReplaceFloatIgnoreCase(ch.name, "rpmThreshold", labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold);
						++labeled_surfacemisc_count;
					}
				} else if (ch.name.Contains("_camera_")) {
					if (labeled_surfacemisc_count < labeled_surfacemisc_maxcount) {
						labeled_surfacemisc[labeled_surfacemisc_count] = new LabeledSurfaceMiscDesc();
						labeled_surfacemisc[labeled_surfacemisc_count].gameObject = ch;
						labeled_surfacemisc[labeled_surfacemisc_count].cameraPosition = true;
						labeled_surfacemisc[labeled_surfacemisc_count].cameraCanRotate = true;
						log(ch.name + " has <Labeled>CameraPosition");
						labeled_surfacemisc[labeled_surfacemisc_count].pivotId = ReplaceStringIgnoreCase(ch.name, "pivotId", labeled_surfacemisc[labeled_surfacemisc_count].pivotId);
						labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold = ReplaceFloatIgnoreCase(ch.name, "rpmThreshold", labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold);
						++labeled_surfacemisc_count;
					}
				} else if (ch.name.Contains("_fixedcamera_")) {
					if (labeled_surfacemisc_count < labeled_surfacemisc_maxcount) {
						labeled_surfacemisc[labeled_surfacemisc_count] = new LabeledSurfaceMiscDesc();
						labeled_surfacemisc[labeled_surfacemisc_count].gameObject = ch;
						labeled_surfacemisc[labeled_surfacemisc_count].cameraPosition = true;
						labeled_surfacemisc[labeled_surfacemisc_count].cameraCanRotate = false;
						log(ch.name + " has <Labeled>CameraPosition");
						labeled_surfacemisc[labeled_surfacemisc_count].pivotId = ReplaceStringIgnoreCase(ch.name, "pivotId", labeled_surfacemisc[labeled_surfacemisc_count].pivotId);
						labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold = ReplaceFloatIgnoreCase(ch.name, "rpmThreshold", labeled_surfacemisc[labeled_surfacemisc_count].rpmThreshold);
						++labeled_surfacemisc_count;
					}
				}
				if (sd != null) {
					if (surfacedrives_count < surfacedrives_maxcount) {
						surfacedrives[surfacedrives_count] = sd;
						if (ch.GetComponent("LineRenderer") == null) ch.AddComponent<LineRenderer>();
						surfacedrives[surfacedrives_count].lineRenderer = (LineRenderer)ch.GetComponent("LineRenderer");
						++surfacedrives_count;
					}
					log(ch.name + " has GDrive");
					//if (!gRenderBoxes_k) ch.renderer.enabled = false;
				} else if (ch.name.Contains("_drive_") || ch.name.Contains("_engine_")) {
					log(ch.name + " has <Labeled>Drive");
					if (labeled_surfacedrives_count < labeled_surfacedrives_maxcount) {
						labeled_surfacedrives[labeled_surfacedrives_count] = new LabeledSurfaceDriveDesc();
						labeled_surfacedrives[labeled_surfacedrives_count].gameObject = ch;
						string preset = ReplaceStringIgnoreCase(ch.name, "preset", "");
						if (preset.ToLower().Equals("attackHelicopter".ToLower())) {
							//_drive_type:basicrotor_mass:100_dryforce:12857_throttleRpmConversionFilter:0.00005_bladeLength:5_bladeWidth:0.4_bladeNumber:4_pivotid:drive1_
							labeled_surfacedrives[labeled_surfacedrives_count].type = GDrive.TDriveType.rotor_basic;
							labeled_surfacedrives[labeled_surfacedrives_count].bladeMass = 100.0f;
							labeled_surfacedrives[labeled_surfacedrives_count].bladeLength = 5.0f;
							labeled_surfacedrives[labeled_surfacedrives_count].bladeNumber = 4;
							labeled_surfacedrives[labeled_surfacedrives_count].throttleMax = labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner = 12857.0f;
							labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionFilter = 0.00005f;
							labeled_surfacedrives[labeled_surfacedrives_count].shaftOutputPivotId = "drive1";
						} else if (preset.ToLower().Equals("acrobatic".ToLower())) {
							//_drive_rpms:23000_aftForce:1_type:propeller_pivotid:drive1_
							labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionRatio = 23000.0f;
							labeled_surfacedrives[labeled_surfacedrives_count].throttleMax = labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner = 1.0f;
							labeled_surfacedrives[labeled_surfacedrives_count].shaftOutputPivotId = "drive1";
						}
						labeled_surfacedrives[labeled_surfacedrives_count].type = GDrive.toTDriveType(ReplaceStringIgnoreCase(ch.name, "type", GDrive.fromTDriveType(labeled_surfacedrives[labeled_surfacedrives_count].type)));
						switch (labeled_surfacedrives[labeled_surfacedrives_count].type) {
						case GDrive.TDriveType.forward_rotor2: case GDrive.TDriveType.up_rotor2: case GDrive.TDriveType.right_rotor2:
							labeled_surfacedrives[labeled_surfacedrives_count].theoreticalTargetRpms = ReplaceFloatIgnoreCase(ch.name, "rpms", labeled_surfacedrives[labeled_surfacedrives_count].theoreticalTargetRpms);
							labeled_surfacedrives[labeled_surfacedrives_count].theoreticalEnginePower = ReplaceFloatIgnoreCase(ch.name, "power", labeled_surfacedrives[labeled_surfacedrives_count].theoreticalEnginePower);
							break;
						case GDrive.TDriveType.rotor_basic:
							labeled_surfacedrives[labeled_surfacedrives_count].theoreticalTargetRpms = ReplaceFloatIgnoreCase(ch.name, "rpms", labeled_surfacedrives[labeled_surfacedrives_count].theoreticalTargetRpms);
							break;
						default:
							labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionRatio = ReplaceFloatIgnoreCase(ch.name, "rpms", labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionRatio);
							break;
						}
						labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner = ReplaceFloatIgnoreCase(ch.name, "dryForce", labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleMax = ReplaceFloatIgnoreCase(ch.name, "aftForce", labeled_surfacedrives[labeled_surfacedrives_count].throttleMax);
						//labeled_surfacedrives[labeled_surfacedrives_count].throttleReverse = ReplaceFloatIgnoreCase(ch.name, "reverse", labeled_surfacedrives[labeled_surfacedrives_count].throttleReverse);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleMax = ReplaceFloatIgnoreCase(ch.name, "force", labeled_surfacedrives[labeled_surfacedrives_count].throttleMax);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner = ReplaceFloatIgnoreCase(ch.name, "force", labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleIdle = ReplaceFloatIgnoreCase(ch.name, "idleForce", labeled_surfacedrives[labeled_surfacedrives_count].throttleIdle);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleMax = ReplaceFloatIgnoreCase(ch.name, "dryForce", labeled_surfacedrives[labeled_surfacedrives_count].throttleMax);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner = ReplaceFloatIgnoreCase(ch.name, "aftForce", labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionRatio = ReplaceFloatIgnoreCase(ch.name, "conversion", labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionRatio);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionFilter = ReplaceFloatIgnoreCase(ch.name, "conversionFilter", labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeMass = ReplaceFloatIgnoreCase(ch.name, "mass", labeled_surfacedrives[labeled_surfacedrives_count].bladeMass);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMin = ReplaceFloatIgnoreCase(ch.name, "bladeWidth", labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMin);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMax = ReplaceFloatIgnoreCase(ch.name, "bladeWidth", labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMax);
						//_powerControlBy:pivot_powerControlByPivot:cthrottle_powerCyclicForwardControlBy:pivot_powerCyclicForwardControlByPivot:celevator_powerCyclicSideControlBy:pivot_powerCyclicSideControlByPivot:cailerons_
						//_powerControlByPivot:cthrottle_powerCyclicForwardControlByPivot:celevator_powerCyclicSideControlByPivot:cailerons_
						labeled_surfacedrives[labeled_surfacedrives_count].powered = ReplaceBoolIgnoreCase(ch.name, "powered", labeled_surfacedrives[labeled_surfacedrives_count].powered);
						labeled_surfacedrives[labeled_surfacedrives_count].poweredFactor = ReplaceFloatIgnoreCase(ch.name, "poweredFactor", labeled_surfacedrives[labeled_surfacedrives_count].poweredFactor);
						labeled_surfacedrives[labeled_surfacedrives_count].poweredFactorFilter = ReplaceFloatIgnoreCase(ch.name, "poweredFactorFilter", labeled_surfacedrives[labeled_surfacedrives_count].poweredFactorFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].powerControlBy = ReplaceIsSetIgnoreCase(ch.name, "powerControlByPivot") ? GDrive.TDrivePowerControlBy.pivot : labeled_surfacedrives[labeled_surfacedrives_count].powerControlBy;
						labeled_surfacedrives[labeled_surfacedrives_count].powerControlBy = GDrive.toTDrivePowerControlBy(ReplaceStringIgnoreCase(ch.name, "powerControlBy", GDrive.fromTDrivePowerControlBy(labeled_surfacedrives[labeled_surfacedrives_count].powerControlBy)));
						labeled_surfacedrives[labeled_surfacedrives_count].powerControlByPivot = ReplaceStringIgnoreCase(ch.name, "powerControlByPivot", labeled_surfacedrives[labeled_surfacedrives_count].powerControlByPivot);
						labeled_surfacedrives[labeled_surfacedrives_count].powerCollectiveControlBy = ReplaceIsSetIgnoreCase(ch.name, "powerCollectiveControlByPivot") ? GDrive.TDrivePowerControlBy.pivot : labeled_surfacedrives[labeled_surfacedrives_count].powerCollectiveControlBy;
						labeled_surfacedrives[labeled_surfacedrives_count].powerCollectiveControlBy = GDrive.toTDrivePowerControlBy(ReplaceStringIgnoreCase(ch.name, "powerCollectiveControlBy", GDrive.fromTDrivePowerControlBy(labeled_surfacedrives[labeled_surfacedrives_count].powerCollectiveControlBy)));
						labeled_surfacedrives[labeled_surfacedrives_count].powerCollectiveControlByPivot = ReplaceStringIgnoreCase(ch.name, "powerCollectiveControlByPivot", labeled_surfacedrives[labeled_surfacedrives_count].powerCollectiveControlByPivot);
						labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicForwardControlBy = ReplaceIsSetIgnoreCase(ch.name, "powerCyclicForwardControlByPivot") ? GDrive.TDrivePowerControlBy.pivot : labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicForwardControlBy;
						labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicForwardControlBy = GDrive.toTDrivePowerControlBy(ReplaceStringIgnoreCase(ch.name, "powerCyclicForwardControlBy", GDrive.fromTDrivePowerControlBy(labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicForwardControlBy)));
						labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicForwardControlByPivot = ReplaceStringIgnoreCase(ch.name, "powerCyclicForwardControlByPivot", labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicForwardControlByPivot);
						labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicSideControlBy = ReplaceIsSetIgnoreCase(ch.name, "powerCyclicSideControlByPivot") ? GDrive.TDrivePowerControlBy.pivot : labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicSideControlBy;
						labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicSideControlBy = GDrive.toTDrivePowerControlBy(ReplaceStringIgnoreCase(ch.name, "powerCyclicSideControlBy", GDrive.fromTDrivePowerControlBy(labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicSideControlBy)));
						labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicSideControlByPivot = ReplaceStringIgnoreCase(ch.name, "powerCyclicSideControlByPivot", labeled_surfacedrives[labeled_surfacedrives_count].powerCyclicSideControlByPivot);
						//labeled_surfacedrives[labeled_surfacedrives_count].throttleReverse = ReplaceFloatIgnoreCase(ch.name, "throttleReverse", labeled_surfacedrives[labeled_surfacedrives_count].throttleReverse);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleIdle = ReplaceFloatIgnoreCase(ch.name, "throttleIdle", labeled_surfacedrives[labeled_surfacedrives_count].throttleIdle);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleMax = ReplaceFloatIgnoreCase(ch.name, "throttleMax", labeled_surfacedrives[labeled_surfacedrives_count].throttleMax);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner = ReplaceFloatIgnoreCase(ch.name, "throttleAfterburner", labeled_surfacedrives[labeled_surfacedrives_count].throttleAfterburner);
						//labeled_surfacedrives[labeled_surfacedrives_count].throttleWatts = ReplaceFloatIgnoreCase(ch.name, "throttleWatts", labeled_surfacedrives[labeled_surfacedrives_count].throttleWatts);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionRatio = ReplaceFloatIgnoreCase(ch.name, "throttleRpmConversionRatio", labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionRatio);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionFilter = ReplaceFloatIgnoreCase(ch.name, "throttleRpmConversionFilter", labeled_surfacedrives[labeled_surfacedrives_count].throttleRpmConversionFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].theoreticalTargetRpms = ReplaceFloatIgnoreCase(ch.name, "theoreticalTargetRpms", labeled_surfacedrives[labeled_surfacedrives_count].theoreticalTargetRpms);
						labeled_surfacedrives[labeled_surfacedrives_count].theoreticalEnginePower = ReplaceFloatIgnoreCase(ch.name, "theoreticalEnginePower", labeled_surfacedrives[labeled_surfacedrives_count].theoreticalEnginePower);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleBiasOffset = ReplaceFloatIgnoreCase(ch.name, "throttleBiasOffset", labeled_surfacedrives[labeled_surfacedrives_count].throttleBiasOffset);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleBiasMultiplier = ReplaceFloatIgnoreCase(ch.name, "throttleBiasMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].throttleBiasMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleForwardRearOffset = ReplaceFloatIgnoreCase(ch.name, "throttleForwardRearOffset", labeled_surfacedrives[labeled_surfacedrives_count].throttleForwardRearOffset);
						labeled_surfacedrives[labeled_surfacedrives_count].throttleLeftRightOffset = ReplaceFloatIgnoreCase(ch.name, "throttleLeftRightOffset", labeled_surfacedrives[labeled_surfacedrives_count].throttleLeftRightOffset);
						labeled_surfacedrives[labeled_surfacedrives_count].yawForceMultiplier = ReplaceFloatIgnoreCase(ch.name, "yawForceMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].yawForceMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].dumperForceMultiplier = ReplaceFloatIgnoreCase(ch.name, "dumperForceMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].dumperForceMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].commonFilter = ReplaceFloatIgnoreCase(ch.name, "commonFilter", labeled_surfacedrives[labeled_surfacedrives_count].commonFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].pitchFilter = ReplaceFloatIgnoreCase(ch.name, "pitchFilter", labeled_surfacedrives[labeled_surfacedrives_count].pitchFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].rollFilter = ReplaceFloatIgnoreCase(ch.name, "rollFilter", labeled_surfacedrives[labeled_surfacedrives_count].rollFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].yawFilter = ReplaceFloatIgnoreCase(ch.name, "yawFilter", labeled_surfacedrives[labeled_surfacedrives_count].yawFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].dumperFilter = ReplaceFloatIgnoreCase(ch.name, "dumperFilter", labeled_surfacedrives[labeled_surfacedrives_count].dumperFilter);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeMass = ReplaceFloatIgnoreCase(ch.name, "bladeMass", labeled_surfacedrives[labeled_surfacedrives_count].bladeMass);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeNumber = Mathf.RoundToInt(ReplaceFloatIgnoreCase(ch.name, "bladeNumber", labeled_surfacedrives[labeled_surfacedrives_count].bladeNumber));
						labeled_surfacedrives[labeled_surfacedrives_count].bladeLength = ReplaceFloatIgnoreCase(ch.name, "bladeLength", labeled_surfacedrives[labeled_surfacedrives_count].bladeLength);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMin = ReplaceFloatIgnoreCase(ch.name, "bladeWidthMin", labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMin);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMax = ReplaceFloatIgnoreCase(ch.name, "bladeWidthMax", labeled_surfacedrives[labeled_surfacedrives_count].bladeWidthMax);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeDepth = ReplaceFloatIgnoreCase(ch.name, "bladeDepth", labeled_surfacedrives[labeled_surfacedrives_count].bladeDepth);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeAngleYaw = ReplaceFloatIgnoreCase(ch.name, "bladeAngleYaw", labeled_surfacedrives[labeled_surfacedrives_count].bladeAngleYaw);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeAnglePitch = ReplaceFloatIgnoreCase(ch.name, "bladeAnglePitch", labeled_surfacedrives[labeled_surfacedrives_count].bladeAnglePitch);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeShapeCoefficient = ReplaceFloatIgnoreCase(ch.name, "bladeShapeCoefficient", labeled_surfacedrives[labeled_surfacedrives_count].bladeShapeCoefficient);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWashEnabled = ReplaceBoolIgnoreCase(ch.name, "bladeWashEnabled", labeled_surfacedrives[labeled_surfacedrives_count].bladeWashEnabled);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWashRadiusNormal = ReplaceFloatIgnoreCase(ch.name, "bladeWashRadiusNormal", labeled_surfacedrives[labeled_surfacedrives_count].bladeWashRadiusNormal);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWashRadiusTangent = ReplaceFloatIgnoreCase(ch.name, "bladeWashRadiusTangent", labeled_surfacedrives[labeled_surfacedrives_count].bladeWashRadiusTangent);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWashFactor = ReplaceFloatIgnoreCase(ch.name, "bladeWashFactor", labeled_surfacedrives[labeled_surfacedrives_count].bladeWashFactor);
						labeled_surfacedrives[labeled_surfacedrives_count].bladeWashSpread = ReplaceFloatIgnoreCase(ch.name, "bladeWashSpread", labeled_surfacedrives[labeled_surfacedrives_count].bladeWashSpread);
						labeled_surfacedrives[labeled_surfacedrives_count].rotorCollectiveBias = ReplaceFloatIgnoreCase(ch.name, "rotorCollectiveBias", labeled_surfacedrives[labeled_surfacedrives_count].rotorCollectiveBias);
						labeled_surfacedrives[labeled_surfacedrives_count].rotorCollectiveCoefficient = ReplaceFloatIgnoreCase(ch.name, "rotorCollectiveCoefficient", labeled_surfacedrives[labeled_surfacedrives_count].rotorCollectiveCoefficient);
						labeled_surfacedrives[labeled_surfacedrives_count].rotorCyclicForwardCoefficient = ReplaceFloatIgnoreCase(ch.name, "rotorCyclicForwardCoefficient", labeled_surfacedrives[labeled_surfacedrives_count].rotorCyclicForwardCoefficient);
						labeled_surfacedrives[labeled_surfacedrives_count].rotorCyclicSideCoefficient = ReplaceFloatIgnoreCase(ch.name, "rotorCyclicSideCoefficient", labeled_surfacedrives[labeled_surfacedrives_count].rotorCyclicSideCoefficient);
						labeled_surfacedrives[labeled_surfacedrives_count].rotorAutorotationCoefficient = ReplaceFloatIgnoreCase(ch.name, "rotorAutorotationCoefficient", labeled_surfacedrives[labeled_surfacedrives_count].rotorAutorotationCoefficient);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.x = ReplaceFloatIgnoreCase(ch.name, "basicRotorTailOffset.x", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.x);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.y = ReplaceFloatIgnoreCase(ch.name, "basicRotorTailOffset.y", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.y);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.z = ReplaceFloatIgnoreCase(ch.name, "basicRotorTailOffset.z", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.z);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.x = ReplaceFloatIgnoreCase(ch.name, "basicRotorTailOffsetX", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.x);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.y = ReplaceFloatIgnoreCase(ch.name, "basicRotorTailOffsetY", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.y);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.z = ReplaceFloatIgnoreCase(ch.name, "basicRotorTailOffsetZ", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorTailOffset.z);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorBladeForcePoint = ReplaceFloatIgnoreCase(ch.name, "basicRotorBladeForcePoint", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorBladeForcePoint);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorAirdensityBias = ReplaceFloatIgnoreCase(ch.name, "basicRotorAirdensityBias", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorAirdensityBias);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorAirdensityExp = ReplaceFloatIgnoreCase(ch.name, "basicRotorAirdensityExp", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorAirdensityExp);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactor = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactor", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactor);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorExponent = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorExponent", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorExponent);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorAtSpeed = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorAtSpeed", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorAtSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorMin = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorMin", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorMin);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorMax = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorMax", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorMax);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorOffsetSpeed = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorOffsetSpeed", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorOffsetSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorOffsetValue = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorOffsetValue", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorOffsetValue);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorMultiplier = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorVibration = ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorVibration", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorVibration);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorVibratorId = Mathf.FloorToInt(ReplaceFloatIgnoreCase(ch.name, "overspeedPitchFactorVibratorId", labeled_surfacedrives[labeled_surfacedrives_count].overspeedPitchFactorVibratorId));
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactor = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactor", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactor);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorExponent = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorExponent", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorExponent);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorAtSpeed = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorAtSpeed", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorAtSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorMin = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorMin", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorMin);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorMax = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorMax", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorMax);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorOffsetSpeed = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorOffsetSpeed", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorOffsetSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorOffsetValue = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorOffsetValue", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorOffsetValue);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorMultiplier = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorVibration = ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorVibration", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorVibration);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorVibratorId = Mathf.FloorToInt(ReplaceFloatIgnoreCase(ch.name, "overspeedRollFactorVibratorId", labeled_surfacedrives[labeled_surfacedrives_count].overspeedRollFactorVibratorId));
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactor = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactor", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactor);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorExponent = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorExponent", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorExponent);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorAtSpeed = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorAtSpeed", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorAtSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorMin = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorMin", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorMin);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorMax = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorMax", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorMax);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorOffsetSpeed = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorOffsetSpeed", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorOffsetSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorOffsetValue = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorOffsetValue", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorOffsetValue);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorMultiplier = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorVibration = ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorVibration", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorVibration);
						labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorVibratorId = Mathf.FloorToInt(ReplaceFloatIgnoreCase(ch.name, "overspeedYawFactorVibratorId", labeled_surfacedrives[labeled_surfacedrives_count].overspeedYawFactorVibratorId));
						labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithSpeed = ReplaceFloatIgnoreCase(ch.name, "climbReductionWithSpeed", labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithSpeedAtSpeed = ReplaceFloatIgnoreCase(ch.name, "climbReductionWithSpeedAtSpeed", labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithSpeedAtSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithVerticalSpeed = ReplaceFloatIgnoreCase(ch.name, "climbReductionWithVerticalSpeed", labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithVerticalSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithVerticalSpeedAtSpeed = ReplaceFloatIgnoreCase(ch.name, "climbReductionWithVerticalSpeedAtSpeed", labeled_surfacedrives[labeled_surfacedrives_count].climbReductionWithVerticalSpeedAtSpeed);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorVariationMultiplier = ReplaceFloatIgnoreCase(ch.name, "basicRotorVariationMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorVariationMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorVelocityFactor = ReplaceFloatIgnoreCase(ch.name, "basicRotorVelocityFactor", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorVelocityFactor);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityFactor = ReplaceFloatIgnoreCase(ch.name, "basicRotorProjectedVelocityFactor", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityFactor);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityMultiplier = ReplaceFloatIgnoreCase(ch.name, "basicRotorProjectedVelocityMultiplier", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityMultiplier);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityForwardOffset = ReplaceFloatIgnoreCase(ch.name, "basicRotorProjectedVelocityForwardOffset", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityForwardOffset);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityRearOffset = ReplaceFloatIgnoreCase(ch.name, "basicRotorProjectedVelocityRearOffset", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityRearOffset);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityLeftOffset = ReplaceFloatIgnoreCase(ch.name, "basicRotorProjectedVelocityLeftOffset", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityLeftOffset);
						labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityRightOffset = ReplaceFloatIgnoreCase(ch.name, "basicRotorProjectedVelocityRightOffset", labeled_surfacedrives[labeled_surfacedrives_count].basicRotorProjectedVelocityRightOffset);
						labeled_surfacedrives[labeled_surfacedrives_count].shaftOutputPivotId = ReplaceStringIgnoreCase(ch.name, "pivotId", labeled_surfacedrives[labeled_surfacedrives_count].shaftOutputPivotId);
						if (ch.GetComponent("LineRenderer") == null) ch.AddComponent<LineRenderer>();
						labeled_surfacedrives[labeled_surfacedrives_count].lineRenderer = (LineRenderer)ch.GetComponent("LineRenderer");
						++labeled_surfacedrives_count;
					}
					if (!globalRenderPhysicSurfaces) ch.GetComponent<Renderer>().enabled = false;
				}
				if (sc != null) {
					//center = sc;
					Rigidbody rb = (Rigidbody)gameObject.GetComponent("Rigidbody");
					if (rb != null) rb.centerOfMass = center = gameObject.transform.InverseTransformPoint(sc.transform.position);
					log(ch.name + " has GCenter");
					//if (!gRenderBoxes_k) ch.renderer.enabled = false;
				} else if (ch.name.Contains("_center_")) {
					//center = ch;
					Rigidbody rb = (Rigidbody)gameObject.GetComponent("Rigidbody");
					if (rb != null) rb.centerOfMass = center = gameObject.transform.InverseTransformPoint(ch.transform.position);
				}
				if (r < 10) FindNodes(ch, r + 1);
			}
		}
	}
	void FindNodes2ndPass(GameObject e, int r) {
		GameObject ch;
		if (r == 0) {
			surfacetrails = new GTrail[surfacetrails_maxcount];
			surfacetrails_count = 0;
			labeled_surfacetrails = new LabeledSurfaceTrailDesc[labeled_surfacetrails_maxcount];
			labeled_surfacetrails_count = 0;
		}
		for (int i = 0; i < e.transform.childCount; ++i) {
			ch = e.transform.GetChild(i).gameObject;
			if (ch.activeSelf) {
				GTrail st = (GTrail)ch.GetComponent("GTrail");
				if (st != null) {
					if (surfacetrails_count < surfacetrails_maxcount) {
						surfacetrails[surfacetrails_count] = st;
	
						for (int j = 0; j < surfaces_count; ++j) if (surfacetrails[surfacetrails_count].surfaceId.CompareTo(surfaces[j].id) == 0) surfacetrails[surfacetrails_count].surfaceId_int = j;
						for (int j = 0; j < surfaces_count; ++j) if (surfacetrails[surfacetrails_count].surfaceId.CompareTo(surfaces[j].id) == 0) surfacetrails[surfacetrails_count].surfaceId_int = j;
						if (ch.GetComponent("LineRenderer") == null) ch.AddComponent<LineRenderer>();
						//if (ch.GetComponent("LineRenderer") != null) {
							surfacetrails[surfacetrails_count].lineRenderer = (LineRenderer)ch.GetComponent("LineRenderer");
							surfacetrails[surfacetrails_count].linePoints = new Vector3[surfacetrails_points];
							surfacetrails[surfacetrails_count].linePointsEnabled = new bool[surfacetrails_points];
						//}
						for (int j = 0; j < surfacetrails_points; ++j) surfacetrails[surfacetrails_count].linePointsEnabled[j] = false;
						Shader shader;
						Material material;
						if (ch.GetComponent("LineRenderer") != null) {
							if ((shader = Shader.Find(surfacetrails[surfacetrails_count].materialName)) != null) {
								surfacetrails[surfacetrails_count].lineRenderer.material = material = new Material(shader);
							} else if ((material = (Material)Resources.Load(surfacetrails[surfacetrails_count].materialName)) != null) {
								surfacetrails[surfacetrails_count].lineRenderer.material = material;
							}
							shader = null;
							material = null;
							surfacetrails[surfacetrails_count].lineRenderer.material.mainTextureScale = new Vector2(surfacetrails_points * 1.0f, 1.0f);
							surfacetrails[surfacetrails_count].lineRenderer.SetColors(surfacetrails[surfacetrails_count].startColor, surfacetrails[surfacetrails_count].endColor);
							surfacetrails[surfacetrails_count].lineRenderer.SetWidth(surfacetrails[surfacetrails_count].startWidth * globalSimulationScale, surfacetrails[surfacetrails_count].endWidth * globalSimulationScale);
						}
						++surfacetrails_count;
					}
					log(ch.name + " has GTrail");
				} else if (ch.name.Contains("_trail_")) {
					log(ch.name + " has <Labeled>Surface");
					if (labeled_surfacetrails_count < labeled_surfacetrails_maxcount) {
						labeled_surfacetrails[labeled_surfacetrails_count] = new LabeledSurfaceTrailDesc();
						labeled_surfacetrails[labeled_surfacetrails_count].gameObject = ch;
						labeled_surfacetrails[labeled_surfacetrails_count].startWidth = ReplaceFloatIgnoreCase(ch.name, "width", labeled_surfacetrails[labeled_surfacetrails_count].startWidth);
						labeled_surfacetrails[labeled_surfacetrails_count].endWidth = ReplaceFloatIgnoreCase(ch.name, "width", labeled_surfacetrails[labeled_surfacetrails_count].endWidth);
						labeled_surfacetrails[labeled_surfacetrails_count].startWidth = ReplaceFloatIgnoreCase(ch.name, "startWidth", labeled_surfacetrails[labeled_surfacetrails_count].startWidth);
						labeled_surfacetrails[labeled_surfacetrails_count].endWidth = ReplaceFloatIgnoreCase(ch.name, "endWidth", labeled_surfacetrails[labeled_surfacetrails_count].endWidth);
						labeled_surfacetrails[labeled_surfacetrails_count].mode = GTrail.toTTrailMode(ReplaceStringIgnoreCase(ch.name, "mode", GTrail.fromTTrailMode(labeled_surfacetrails[labeled_surfacetrails_count].mode)));
						labeled_surfacetrails[labeled_surfacetrails_count].materialName = ReplaceStringIgnoreCase(ch.name, "material", labeled_surfacetrails[labeled_surfacetrails_count].materialName);
						labeled_surfacetrails[labeled_surfacetrails_count].materialName = ReplaceStringIgnoreCase(ch.name, "materialName", labeled_surfacetrails[labeled_surfacetrails_count].materialName);
						labeled_surfacetrails[labeled_surfacetrails_count].surfaceId = ReplaceStringIgnoreCase(ch.name, "surfaceId", labeled_surfacetrails[labeled_surfacetrails_count].surfaceId);
	
						for (int j = 0; j < surfaces_count; ++j) if (labeled_surfacetrails[labeled_surfacetrails_count].surfaceId.CompareTo(surfaces[j].id) == 0) labeled_surfacetrails[labeled_surfacetrails_count].surfaceId_int = j;
						for (int j = 0; j < labeled_surfaces_count; ++j) if (labeled_surfacetrails[labeled_surfacetrails_count].surfaceId.CompareTo(labeled_surfaces[j].id) == 0) labeled_surfacetrails[labeled_surfacetrails_count].surfaceId_int = j;
						if (ch.GetComponent("LineRenderer") == null) ch.AddComponent<LineRenderer>();
						//if (ch.GetComponent("LineRenderer") != null) {
							labeled_surfacetrails[labeled_surfacetrails_count].lineRenderer = (LineRenderer)ch.GetComponent("LineRenderer");
							labeled_surfacetrails[labeled_surfacetrails_count].linePoints = new Vector3[surfacetrails_points];
							labeled_surfacetrails[labeled_surfacetrails_count].linePointsEnabled = new bool[surfacetrails_points];
						//}
						for (int j = 0; j < surfacetrails_points; ++j) labeled_surfacetrails[labeled_surfacetrails_count].linePointsEnabled[j] = false;
						Shader shader;
						Material material;
						if (ch.GetComponent("LineRenderer") != null) {
							if ((shader = Shader.Find(labeled_surfacetrails[labeled_surfacetrails_count].materialName)) != null) {
								labeled_surfacetrails[labeled_surfacetrails_count].lineRenderer.material = material = new Material(shader);
							} else if ((material = (Material)Resources.Load(labeled_surfacetrails[labeled_surfacetrails_count].materialName)) != null) {
								labeled_surfacetrails[labeled_surfacetrails_count].lineRenderer.material = material;
							}
							shader = null;
							material = null;
							labeled_surfacetrails[labeled_surfacetrails_count].lineRenderer.material.mainTextureScale = new Vector2(surfacetrails_points * 1.0f, 1.0f);
							labeled_surfacetrails[labeled_surfacetrails_count].lineRenderer.SetColors(labeled_surfacetrails[labeled_surfacetrails_count].startColor, labeled_surfacetrails[labeled_surfacetrails_count].endColor);
							labeled_surfacetrails[labeled_surfacetrails_count].lineRenderer.SetWidth(labeled_surfacetrails[labeled_surfacetrails_count].startWidth * globalSimulationScale, labeled_surfacetrails[labeled_surfacetrails_count].endWidth * globalSimulationScale);
						}
						++labeled_surfacetrails_count;
					}
				}
				if (r < 10) FindNodes2ndPass(ch, r + 1);
			}
		}
	}

	void StartKinetics() {
		if ((kineticsInertiaTensor.x == 0.0f) && (kineticsInertiaTensor.y == 0.0f) && (kineticsInertiaTensor.z == 0.0f)) {
			kineticsInertiaTensor_lastValue = kineticsInertiaTensor = kineticsInertiaTensor_original = gameObject.GetComponent<Rigidbody>().inertiaTensor;
		}
	}
	void ProcessKinetics() {
		tmp_x2 = (body_max_x - body_min_x) * (body_max_x - body_min_x);
		tmp_y2 = (body_max_y - body_min_y) * (body_max_y - body_min_y);
		tmp_z2 = (body_max_z - body_min_z) * (body_max_z - body_min_z);
		if ((kineticsInertiaTensor.x != kineticsInertiaTensor_lastValue.x) || (kineticsInertiaTensor.y != kineticsInertiaTensor_lastValue.y) || (kineticsInertiaTensor.z != kineticsInertiaTensor_lastValue.z)) {
			kineticsInertiaTensor_tmp.Set(
				kineticsInertiaTensorScale.x * kineticsInertiaTensor.x * (tmp_y2 * kineticsMassRedistribution.y * kineticsMassRedistribution.y + tmp_z2 * kineticsMassRedistribution.z * kineticsMassRedistribution.z) / (tmp_y2 + tmp_z2),
				kineticsInertiaTensorScale.y * kineticsInertiaTensor.y * (tmp_x2 * kineticsMassRedistribution.x * kineticsMassRedistribution.x + tmp_z2 * kineticsMassRedistribution.z * kineticsMassRedistribution.z) / (tmp_x2 + tmp_z2),
				kineticsInertiaTensorScale.z * kineticsInertiaTensor.z * (tmp_x2 * kineticsMassRedistribution.x * kineticsMassRedistribution.x + tmp_y2 * kineticsMassRedistribution.y * kineticsMassRedistribution.y) / (tmp_x2 + tmp_y2)
			);
			gameObject.GetComponent<Rigidbody>().inertiaTensor = kineticsInertiaTensor_tmp;
		} else {
			kineticsInertiaTensor_tmp.Set(
				kineticsInertiaTensorScale.x * kineticsInertiaTensor_original.x * (tmp_y2 * kineticsMassRedistribution.y * kineticsMassRedistribution.y + tmp_z2 * kineticsMassRedistribution.z * kineticsMassRedistribution.z) / (tmp_y2 + tmp_z2),
				kineticsInertiaTensorScale.y * kineticsInertiaTensor_original.y * (tmp_x2 * kineticsMassRedistribution.x * kineticsMassRedistribution.x + tmp_z2 * kineticsMassRedistribution.z * kineticsMassRedistribution.z) / (tmp_x2 + tmp_z2),
				kineticsInertiaTensorScale.z * kineticsInertiaTensor_original.z * (tmp_x2 * kineticsMassRedistribution.x * kineticsMassRedistribution.x + tmp_y2 * kineticsMassRedistribution.y * kineticsMassRedistribution.y) / (tmp_x2 + tmp_y2)
			);
			gameObject.GetComponent<Rigidbody>().inertiaTensor = kineticsInertiaTensor_lastValue = kineticsInertiaTensor = kineticsInertiaTensor_tmp;
		}
	}
	
	float ProcessInputAxisPow(float input, float pow) {
		float output = (input - 0.5f) * 2.0f;
		if (output < 0) output = 0.5f - Mathf.Pow(-output, pow) * 0.5f;
		else output = 0.5f + Mathf.Pow(output, pow) * 0.5f;
		return output;
	}
	float ProcessInputSensivity(float input, TSensivityChannel channel) {
		float coefspeed;
		switch (channel) {
		case TSensivityChannel.elevator:
			coefspeed = Mathf.Pow(inputSensivityCoefficientSpeed * inputElevatorSensivityCoefficientSpeed, -speed * speed);
			break;
		case TSensivityChannel.ailerons:
			coefspeed = Mathf.Pow(inputSensivityCoefficientSpeed * inputAileronsSensivityCoefficientSpeed, -speed * speed);
			break;
		case TSensivityChannel.rudder:
			coefspeed = Mathf.Pow(inputSensivityCoefficientSpeed * inputRudderSensivityCoefficientSpeed, -speed * speed);
			break;
		default:
			coefspeed = Mathf.Pow(inputSensivityCoefficientSpeed, -speed * speed);
			break;
		}
		if (coefspeed < inputSensivityCoefficientSpeedLimit) coefspeed = inputSensivityCoefficientSpeedLimit;
		return (input - 0.5f) * inputSensivity * coefspeed + 0.5f;
	}
	float ProcessInputAxisClamp(float input, float min, float max) {
		if (input < min) return min;
		if (input > max) return max;
		return input;
	}
	void ProcessInput() {
		//if (Input.GetKey(KeyCode.N)) simulation_broken = true;
		if (isCrashed) {
			kThrottle_rev = 0.0f;
			kThrottle_aft = 0.0f;
			inputThrottle_internal = 0.0f;
			inputThrottle_output = 0.0f;
			return;
		}
		
		if (Input.GetKey(inputTrimKey) && Input.GetKeyDown(inputElevatorKeyForIncrement) && ((inputElevatorTrim + inputTrimStep) <= inputTrimMax)) inputElevatorTrim += inputTrimStep;
		if (Input.GetKey(inputTrimKey) && Input.GetKeyDown(inputElevatorKeyForDecrement) && ((inputElevatorTrim - inputTrimStep) >= inputTrimMin)) inputElevatorTrim -= inputTrimStep;
		if (Input.GetKey(inputTrimKey) && Input.GetKeyDown(inputAileronsKeyForIncrement) && ((inputAileronsTrim + inputTrimStep) <= inputTrimMax)) inputAileronsTrim += inputTrimStep;
		if (Input.GetKey(inputTrimKey) && Input.GetKeyDown(inputAileronsKeyForDecrement) && ((inputAileronsTrim - inputTrimStep) >= inputTrimMin)) inputAileronsTrim -= inputTrimStep;
		if (Input.GetKey(inputTrimKey) && Input.GetKeyDown(inputRudderKeyForIncrement) && ((inputRudderTrim + inputTrimStep) <= inputTrimMax)) inputRudderTrim += inputTrimStep;
		if (Input.GetKey(inputTrimKey) && Input.GetKeyDown(inputRudderKeyForDecrement) && ((inputRudderTrim - inputTrimStep) >= inputTrimMin)) inputRudderTrim -= inputTrimStep;
		if (Input.GetKeyDown(inputSensivityKeyForDecrement) && ((inputSensivity - inputSensivityStep) >= inputSensivityMin)) inputSensivity -= inputSensivityStep;
		if (Input.GetKeyDown(inputSensivityKeyForIncrement) && ((inputSensivity + inputSensivityStep) <= inputSensivityMax)) inputSensivity += inputSensivityStep;
			
		switch (inputThrottleAfterburnerSource) {
		case TAxisSource.user: kThrottle_aft = Mathf.Clamp01(kThrottle_aft); break;
		default: kThrottle_aft = 0.0f; break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
			kThrottle_aft = Input.GetKey(inputThrottleKeyForAfterburner) ? 1.0f : 0.0f; break;
		case TAxisSource.unity_axis:
			kThrottle_aft = Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f;
			break;
		case TAxisSource.inv_unity_axis:
			kThrottle_aft = -Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f;
			break;
		case TAxisSource.unity_axis_exp:
			kThrottle_aft = ProcessInputAxisPow(Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK);
			break;
		case TAxisSource.inv_unity_axis_exp:
			kThrottle_aft = ProcessInputAxisPow(-Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK);
			break;
		case TAxisSource.mix:
			kThrottle_aft = (Input.GetKey(inputThrottleKeyForAfterburner) ? 1.0f : 0.0f) + Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f - 0.5f;
			break;
		case TAxisSource.inv_mix:
			kThrottle_aft = (Input.GetKey(inputThrottleKeyForAfterburner) ? 1.0f : 0.0f) - Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f - 0.5f;
			break;
		case TAxisSource.mix_exp:
			kThrottle_aft = (Input.GetKey(inputThrottleKeyForAfterburner) ? 1.0f : 0.0f) + ProcessInputAxisPow(Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK) - 0.5f;
			break;
		case TAxisSource.inv_mix_exp:
			kThrottle_aft = (Input.GetKey(inputThrottleKeyForAfterburner) ? 1.0f : 0.0f) + ProcessInputAxisPow(-Input.GetAxis(inputThrottleAfterburnerSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK) - 0.5f;
			break;
		}
		kThrottle_aft = ProcessInputAxisClamp(kThrottle_aft, 0.0f, 1.0f);
		switch (inputThrottleNitroSource) {
		case TAxisSource.user: kThrottle_ntr = Mathf.Clamp01(kThrottle_ntr); break;
		default: kThrottle_ntr = 0.0f; break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
			kThrottle_ntr = Input.GetKey(inputThrottleKeyForNitro) ? 1.0f : 0.0f; break;
		case TAxisSource.unity_axis:
			kThrottle_ntr = Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f;
			break;
		case TAxisSource.inv_unity_axis:
			kThrottle_ntr = -Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f;
			break;
		case TAxisSource.unity_axis_exp:
			kThrottle_ntr = ProcessInputAxisPow(Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK);
			break;
		case TAxisSource.inv_unity_axis_exp:
			kThrottle_ntr = ProcessInputAxisPow(-Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK);
			break;
		case TAxisSource.mix:
			kThrottle_ntr = (Input.GetKey(inputThrottleKeyForNitro) ? 1.0f : 0.0f) + Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f - 0.5f;
			break;
		case TAxisSource.inv_mix:
			kThrottle_ntr = (Input.GetKey(inputThrottleKeyForNitro) ? 1.0f : 0.0f) - Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f - 0.5f;
			break;
		case TAxisSource.mix_exp:
			kThrottle_ntr = (Input.GetKey(inputThrottleKeyForNitro) ? 1.0f : 0.0f) + ProcessInputAxisPow(Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK) - 0.5f;
			break;
		case TAxisSource.inv_mix_exp:
			kThrottle_ntr = (Input.GetKey(inputThrottleKeyForNitro) ? 1.0f : 0.0f) + ProcessInputAxisPow(-Input.GetAxis(inputThrottleNitroSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK) - 0.5f;
			break;
		}
		kThrottle_ntr = ProcessInputAxisClamp(kThrottle_ntr, 0.0f, 1.0f);
		switch (inputThrottleReverseSource) {
		case TAxisSource.user: kThrottle_rev = Mathf.Clamp01(kThrottle_rev); break;
		default: kThrottle_rev = 0.0f; break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
			kThrottle_rev = Input.GetKey(inputThrottleKeyForReverse) ? 1.0f : 0.0f; break;
		case TAxisSource.unity_axis:
			kThrottle_rev = Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f;
			break;
		case TAxisSource.inv_unity_axis:
			kThrottle_rev = -Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f;
			break;
		case TAxisSource.unity_axis_exp:
			kThrottle_rev = ProcessInputAxisPow(Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK);
			break;
		case TAxisSource.inv_unity_axis_exp:
			kThrottle_rev = ProcessInputAxisPow(-Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK);
			break;
		case TAxisSource.mix:
			kThrottle_rev = (Input.GetKey(inputThrottleKeyForReverse) ? 1.0f : 0.0f) + Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f - 0.5f;
			break;
		case TAxisSource.inv_mix:
			kThrottle_rev = (Input.GetKey(inputThrottleKeyForReverse) ? 1.0f : 0.0f) - Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f - 0.5f;
			break;
		case TAxisSource.mix_exp:
			kThrottle_rev = (Input.GetKey(inputThrottleKeyForReverse) ? 1.0f : 0.0f) + ProcessInputAxisPow(Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK) - 0.5f;
			break;
		case TAxisSource.inv_mix_exp:
			kThrottle_rev = (Input.GetKey(inputThrottleKeyForReverse) ? 1.0f : 0.0f) + ProcessInputAxisPow(-Input.GetAxis(inputThrottleReverseSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK) - 0.5f;
			break;
		}
		kThrottle_rev = ProcessInputAxisClamp(kThrottle_rev, 0.0f, 1.0f);
		switch (inputThrottleSource) {
		case TAxisSource.user: inputThrottle_internal2 = Mathf.Clamp01(inputThrottle_internal2); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
			if (Input.GetKey(inputThrottleKeyForIncrement)) {
				inputThrottle_internal2 += inputThrottleIncrementDecrementStep;
				if (inputThrottle_internal2 > 1.0f) inputThrottle_internal2 = 1.0f;
			}
			if (Input.GetKey(inputThrottleKeyForDecrement)) {
				inputThrottle_internal2 -= inputThrottleIncrementDecrementStep;
				if (inputThrottle_internal2 < 0.0f) inputThrottle_internal2 = 0.0f;
			}
			if (Input.GetKey(inputThrottleKeyFor0p)) inputThrottle_internal2 = 0.0f;
			if (Input.GetKey(inputThrottleKeyFor10p)) inputThrottle_internal2 = 0.1f;
			if (Input.GetKey(inputThrottleKeyFor20p)) inputThrottle_internal2 = 0.2f;
			if (Input.GetKey(inputThrottleKeyFor30p)) inputThrottle_internal2 = 0.3f;
			if (Input.GetKey(inputThrottleKeyFor40p)) inputThrottle_internal2 = 0.4f;
			if (Input.GetKey(inputThrottleKeyFor50p)) inputThrottle_internal2 = 0.5f;
			if (Input.GetKey(inputThrottleKeyFor60p)) inputThrottle_internal2 = 0.6f;
			if (Input.GetKey(inputThrottleKeyFor70p)) inputThrottle_internal2 = 0.7f;
			if (Input.GetKey(inputThrottleKeyFor80p)) inputThrottle_internal2 = 0.8f;
			if (Input.GetKey(inputThrottleKeyFor90p)) inputThrottle_internal2 = 0.9f;
			if (Input.GetKey(inputThrottleKeyFor100p)) inputThrottle_internal2 = 1.0f;
			break;
		}
		switch (inputThrottleSource) {
		case TAxisSource.user: inputThrottle_internal = Mathf.Clamp01(inputThrottle_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
			inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (inputThrottle_internal2);
			break;
		case TAxisSource.unity_axis: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * ((Input.GetAxis(inputThrottleSourceUnityAxis) * 0.5f + 0.5f)); break;
		case TAxisSource.inv_unity_axis: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * ((-Input.GetAxis(inputThrottleSourceUnityAxis) * 0.5f + 0.5f)); break;
		case TAxisSource.unity_axis_exp: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (ProcessInputAxisPow(Input.GetAxis(inputThrottleSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK)); break;
		case TAxisSource.inv_unity_axis_exp: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (ProcessInputAxisPow(-Input.GetAxis(inputThrottleSourceUnityAxis) * 0.5f + 0.5f, inputThrottleSourceExpK)); break;
		case TAxisSource.mix: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (inputThrottle_internal2 + (Input.GetAxis(inputThrottleSourceUnityAxis) * 1.0f + 0.5f) - 0.5f); break;
		case TAxisSource.inv_mix: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (inputThrottle_internal2 + (-Input.GetAxis(inputThrottleSourceUnityAxis) * 1.0f + 0.5f) - 0.5f); break;
		case TAxisSource.mix_exp: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (inputThrottle_internal2 + ProcessInputAxisPow(Input.GetAxis(inputThrottleSourceUnityAxis) * 1.0f + 0.5f, inputThrottleSourceExpK) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (inputThrottle_internal2 + ProcessInputAxisPow(-Input.GetAxis(inputThrottleSourceUnityAxis) * 1.0f + 0.5f, inputThrottleSourceExpK) - 0.5f); break;
		default: inputThrottle_internal = inputThrottle_internal * (1.0f - inputThrottleGlobalSmoothFilter) + inputThrottleGlobalSmoothFilter * (0.0f); break;
		}
		inputThrottle_internal = ProcessInputAxisClamp(inputThrottle_internal, 0.0f, 1.0f);
		if ((inputThrottleSource != TAxisSource.user) && (inputThrottleAfterburnerSource != TAxisSource.user)) {
			if (kThrottle_rev > 0.0f) inputThrottle_output = inputThrottle_output * (1.0f - inputThrottleKeySmoothFilter) + kThrottle_rev * inputThrottleReverseMultiplier * inputThrottleKeySmoothFilter;
			else if (kThrottle_aft > 0.0f) inputThrottle_output = inputThrottle_output * (1.0f - inputThrottleKeySmoothFilter) + kThrottle_aft * inputThrottleAfterburnerMultiplier * inputThrottleKeySmoothFilter;
			else inputThrottle_output = inputThrottle_output * (1.0f - inputThrottleKeySmoothFilter) + inputThrottle_internal * inputThrottleKeySmoothFilter;
		}
		if (kThrottle_ntr_time > 0.0f) {
			kThrottle_ntr_time -= Time.fixedDeltaTime;
			kThrottle_ntr_multiplier = kThrottle_ntr_multiplier * (1.0f - inputThrottleNitroFilter) + inputThrottleNitroMultiplier * inputThrottleNitroFilter;
			//inputThrottle_output = inputThrottle_output * kThrottle_ntr_multiplier;
		} else if (kThrottle_ntr_multiplier > 1.01f) {
			kThrottle_ntr_multiplier = kThrottle_ntr_multiplier * (1.0f - inputThrottleNitroFilter) + 1.0f * inputThrottleNitroFilter;
			if (kThrottle_ntr_multiplier <= 1.01f) kThrottle_ntr_multiplier = 1.0f;
			//inputThrottle_output = inputThrottle_output * kThrottle_ntr_multiplier;
		} else {
			if (kThrottle_ntr > 0.25f) {
				if (inputThrottleNitroCount > 0) {
					kThrottle_ntr_time = inputThrottleNitroTime;
					--inputThrottleNitroCount;
				}
			}
		}
		//inputThrottle_internal = ProcessInputAxisClamp(inputThrottle_internal, 0.0f, 1.0f);

		switch (inputElevatorSource) {
		case TAxisSource.user: inputElevator_internal = Mathf.Clamp01(inputElevator_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
		case TAxisSource.mix_exp:
		case TAxisSource.inv_mix_exp:
			if (Input.GetKey(inputElevatorKeyForIncrement) && Input.GetKey(inputElevatorKeyForDecrement)) {
				inputElevator_internal = inputElevator_internal * (1.0f - inputElevatorKeySmoothFilter) + 0.5f * inputElevatorKeySmoothFilter;
			} else if (Input.GetKey(inputElevatorKeyForIncrement)) {
				inputElevator_internal = inputElevator_internal * (1.0f - inputElevatorKeySmoothFilter) + 1.0f * inputElevatorKeySmoothFilter;
			} else if (Input.GetKey(inputElevatorKeyForDecrement)) {
				inputElevator_internal = inputElevator_internal * (1.0f - inputElevatorKeySmoothFilter) + 0.0f * inputElevatorKeySmoothFilter;
			} else {
				inputElevator_internal = inputElevator_internal * (1.0f - inputElevatorKeySmoothFilter) + 0.5f * inputElevatorKeySmoothFilter;
			}
			break;
		}
		switch (inputElevatorSource) {
		case TAxisSource.user: inputElevator_output = Mathf.Clamp01(inputElevator_output); break;
		case TAxisSource.keys: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + inputElevator_internal, TSensivityChannel.elevator)); break;
		case TAxisSource.keys_exp: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + ProcessInputAxisPow(inputElevator_internal, inputElevatorSourceExpK), TSensivityChannel.elevator)); break;
		case TAxisSource.unity_axis: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.elevator)); break;
		case TAxisSource.inv_unity_axis: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim - Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.elevator)); break;
		case TAxisSource.unity_axis_exp: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + ProcessInputAxisPow(Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, inputElevatorSourceExpK), TSensivityChannel.elevator)); break;
		case TAxisSource.inv_unity_axis_exp: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + ProcessInputAxisPow(-Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, inputElevatorSourceExpK), TSensivityChannel.elevator)); break;
		case TAxisSource.mix: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + inputElevator_internal, TSensivityChannel.elevator) + ProcessInputSensivity(inputElevatorTrim + Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.elevator) - 0.5f); break;
		case TAxisSource.inv_mix: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + inputElevator_internal, TSensivityChannel.elevator) + ProcessInputSensivity(inputElevatorTrim - Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.elevator) - 0.5f); break;
		case TAxisSource.mix_exp: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + inputElevator_internal, TSensivityChannel.elevator) + ProcessInputSensivity(inputElevatorTrim + ProcessInputAxisPow(Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, inputElevatorSourceExpK), TSensivityChannel.elevator) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + inputElevator_internal, TSensivityChannel.elevator) + ProcessInputSensivity(inputElevatorTrim + ProcessInputAxisPow(-Input.GetAxis(inputElevatorSourceUnityAxis) * 0.5f + 0.5f, inputElevatorSourceExpK), TSensivityChannel.elevator) - 0.5f); break;
		default: inputElevator_output = inputElevator_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputElevatorGlobalSmoothFilter * (ProcessInputSensivity(inputElevatorTrim + 0.5f, TSensivityChannel.elevator)); break;
		}
		inputElevator_output = ProcessInputAxisClamp(inputElevator_output, 0.0f, 1.0f);
		
		switch (inputAileronsSource) {
		case TAxisSource.user: inputAilerons_internal = Mathf.Clamp01(inputAilerons_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
		case TAxisSource.mix_exp:
		case TAxisSource.inv_mix_exp:
			if (Input.GetKey(inputAileronsKeyForIncrement) && Input.GetKey(inputAileronsKeyForDecrement)) {
				inputAilerons_internal = inputAilerons_internal * (1.0f - inputAileronsKeySmoothFilter) + 0.5f * inputAileronsKeySmoothFilter;
			} else if (Input.GetKey(inputAileronsKeyForIncrement)) {
				inputAilerons_internal = inputAilerons_internal * (1.0f - inputAileronsKeySmoothFilter) + 1.0f * inputAileronsKeySmoothFilter;
			} else if (Input.GetKey(inputAileronsKeyForDecrement)) {
				inputAilerons_internal = inputAilerons_internal * (1.0f - inputAileronsKeySmoothFilter) + 0.0f * inputAileronsKeySmoothFilter;
			} else {
				inputAilerons_internal = inputAilerons_internal * (1.0f - inputAileronsKeySmoothFilter) + 0.5f * inputAileronsKeySmoothFilter;
			}
			break;
		}
		switch (inputAileronsSource) {
		case TAxisSource.user: inputAilerons_output = Mathf.Clamp01(inputAilerons_output); break;
		case TAxisSource.keys: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + inputAilerons_internal, TSensivityChannel.ailerons)); break;
		case TAxisSource.keys_exp: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + ProcessInputAxisPow(inputAilerons_internal, inputAileronsSourceExpK), TSensivityChannel.ailerons)); break;
		case TAxisSource.unity_axis: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.ailerons)); break;
		case TAxisSource.inv_unity_axis: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim - Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.ailerons)); break;
		case TAxisSource.unity_axis_exp: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + ProcessInputAxisPow(Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, inputAileronsSourceExpK), TSensivityChannel.ailerons)); break;
		case TAxisSource.inv_unity_axis_exp: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + ProcessInputAxisPow(-Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, inputAileronsSourceExpK), TSensivityChannel.ailerons)); break;
		case TAxisSource.mix: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + inputAilerons_internal, TSensivityChannel.ailerons) + ProcessInputSensivity(inputAileronsTrim + Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.ailerons) - 0.5f); break;
		case TAxisSource.inv_mix: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + inputAilerons_internal, TSensivityChannel.ailerons) + ProcessInputSensivity(inputAileronsTrim - Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.ailerons) - 0.5f); break;
		case TAxisSource.mix_exp: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + ProcessInputAxisPow(inputAilerons_internal, inputAileronsSourceExpK), TSensivityChannel.ailerons) + ProcessInputSensivity(inputAileronsTrim + ProcessInputAxisPow(Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, inputAileronsSourceExpK), TSensivityChannel.ailerons) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + ProcessInputAxisPow(inputAilerons_internal, inputAileronsSourceExpK), TSensivityChannel.ailerons) + ProcessInputSensivity(inputAileronsTrim + ProcessInputAxisPow(-Input.GetAxis(inputAileronsSourceUnityAxis) * 0.5f + 0.5f, inputAileronsSourceExpK), TSensivityChannel.ailerons) - 0.5f); break;
		default: inputAilerons_output = inputAilerons_output * (1.0f - inputAileronsGlobalSmoothFilter) + inputAileronsGlobalSmoothFilter * (ProcessInputSensivity(inputAileronsTrim + 0.5f, TSensivityChannel.ailerons)); break;
		}
		inputAilerons_output = ProcessInputAxisClamp(inputAilerons_output, 0.0f, 1.0f);
		
		switch (inputRudderSource) {
		case TAxisSource.user: inputRudder_internal = Mathf.Clamp01(inputRudder_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
		case TAxisSource.mix_exp:
		case TAxisSource.inv_mix_exp:
			if (Input.GetKey(inputRudderKeyForIncrement) && Input.GetKey(inputRudderKeyForDecrement)) {
				inputRudder_internal = inputRudder_internal * (1.0f - inputRudderKeySmoothFilter) + 0.5f * inputRudderKeySmoothFilter;
			} else if (Input.GetKey(inputRudderKeyForIncrement)) {
				inputRudder_internal = inputRudder_internal * (1.0f - inputRudderKeySmoothFilter) + 1.0f * inputRudderKeySmoothFilter;
			} else if (Input.GetKey(inputRudderKeyForDecrement)) {
				inputRudder_internal = inputRudder_internal * (1.0f - inputRudderKeySmoothFilter) + 0.0f * inputRudderKeySmoothFilter;
			} else {
				inputRudder_internal = inputRudder_internal * (1.0f - inputRudderKeySmoothFilter) + 0.5f * inputRudderKeySmoothFilter;
			}
			break;
		}
		switch (inputRudderSource) {
		case TAxisSource.user: inputRudder_output = Mathf.Clamp01(inputRudder_output); break;
		case TAxisSource.keys: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + inputRudder_internal, TSensivityChannel.rudder)); break;
		case TAxisSource.keys_exp: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + ProcessInputAxisPow(inputRudder_internal, inputRudderSourceExpK), TSensivityChannel.rudder)); break;
		case TAxisSource.unity_axis: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.rudder)); break;
		case TAxisSource.inv_unity_axis: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim - Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.rudder)); break;
		case TAxisSource.unity_axis_exp: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + ProcessInputAxisPow(Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, inputRudderSourceExpK), TSensivityChannel.rudder)); break;
		case TAxisSource.inv_unity_axis_exp: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + ProcessInputAxisPow(-Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, inputRudderSourceExpK), TSensivityChannel.rudder)); break;
		case TAxisSource.mix: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + inputRudder_internal, TSensivityChannel.rudder) + ProcessInputSensivity(inputRudderTrim + Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.rudder) - 0.5f); break;
		case TAxisSource.inv_mix: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + inputRudder_internal, TSensivityChannel.rudder) + ProcessInputSensivity(inputRudderTrim - Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, TSensivityChannel.rudder) - 0.5f); break;
		case TAxisSource.mix_exp: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + ProcessInputAxisPow(inputRudder_internal, inputRudderSourceExpK), TSensivityChannel.rudder) + ProcessInputSensivity(inputRudderTrim + ProcessInputAxisPow(Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, inputRudderSourceExpK), TSensivityChannel.rudder) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + ProcessInputAxisPow(inputRudder_internal, inputRudderSourceExpK), TSensivityChannel.rudder) + ProcessInputSensivity(inputRudderTrim + ProcessInputAxisPow(-Input.GetAxis(inputRudderSourceUnityAxis) * 0.5f + 0.5f, inputRudderSourceExpK), TSensivityChannel.rudder) - 0.5f); break;
		default: inputRudder_output = inputRudder_output * (1.0f - inputRudderGlobalSmoothFilter) + inputRudderGlobalSmoothFilter * (ProcessInputSensivity(inputRudderTrim + 0.5f, TSensivityChannel.rudder)); break;
		}
		inputRudder_output = ProcessInputAxisClamp(inputRudder_output, 0.0f, 1.0f);

		switch (inputGearsSource) {
		case TAxisSource.user: inputGears_internal = Mathf.Clamp01(inputGears_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
		case TAxisSource.mix_exp:
		case TAxisSource.inv_mix_exp:
			if (Input.GetKey(inputGearsKeyForToggle)) {
				if (inputGears_internal > 0.9f) {
					inputGears_internal_enabled = false;
				} else if (inputGears_internal < 0.1f) {
					inputGears_internal_enabled = true;
				}
			}
			if (inputGears_internal_enabled) {
				inputGears_internal = inputGears_internal * (1.0f - inputGearsKeySmoothFilter) + 1.0f * inputGearsKeySmoothFilter;
			} else {
				inputGears_internal = inputGears_internal * (1.0f - inputGearsKeySmoothFilter) + 0.0f * inputGearsKeySmoothFilter;
			}
			break;
		}
		switch (inputGearsSource) {
		case TAxisSource.user: inputGears_output = Mathf.Clamp01(inputGears_output); break;
		case TAxisSource.keys: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (inputGears_internal); break;
		case TAxisSource.keys_exp: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (ProcessInputAxisPow(inputGears_internal, inputGearsSourceExpK)); break;
		case TAxisSource.unity_axis: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.inv_unity_axis: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (-Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.unity_axis_exp: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (ProcessInputAxisPow(Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f, inputGearsSourceExpK)); break;
		case TAxisSource.inv_unity_axis_exp: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (ProcessInputAxisPow(-Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f, inputGearsSourceExpK)); break;
		case TAxisSource.mix: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (inputGears_internal + Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.inv_mix: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (inputGears_internal - Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.mix_exp: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (ProcessInputAxisPow(inputGears_internal, inputGearsSourceExpK) + ProcessInputAxisPow(Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f, inputGearsSourceExpK) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (ProcessInputAxisPow(inputGears_internal, inputGearsSourceExpK) + ProcessInputAxisPow(-Input.GetAxis(inputGearsSourceUnityAxis) * 0.5f + 0.5f, inputGearsSourceExpK) - 0.5f); break;
		default: inputGears_output = inputGears_output * (1.0f - inputGearsGlobalSmoothFilter) + inputGearsGlobalSmoothFilter * (1.0f); break;
		}
		inputGears_output = ProcessInputAxisClamp(inputGears_output, 0.0f, 1.0f);

		switch (inputFlapsSource) {
		case TAxisSource.user: inputFlaps_internal = Mathf.Clamp01(inputFlaps_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
		case TAxisSource.mix_exp:
		case TAxisSource.inv_mix_exp:
			if (Input.GetKey(inputFlapsKeyForToggle)) {
				if (inputFlaps_internal > 0.9f) {
					inputFlaps_internal_enabled = false;
				} else if (inputFlaps_internal < 0.1f) {
					inputFlaps_internal_enabled = true;
				}
			}
			if (inputFlaps_internal_enabled) {
				inputFlaps_internal = inputFlaps_internal * (1.0f - inputFlapsKeySmoothFilter) + 1.0f * inputFlapsKeySmoothFilter;
			} else {
				inputFlaps_internal = inputFlaps_internal * (1.0f - inputFlapsKeySmoothFilter) + 0.0f * inputFlapsKeySmoothFilter;
			}
			break;
		}
		switch (inputFlapsSource) {
		case TAxisSource.user: inputFlaps_output = Mathf.Clamp01(inputFlaps_output); break;
		case TAxisSource.keys: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (inputFlaps_internal); break;
		case TAxisSource.keys_exp: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (ProcessInputAxisPow(inputFlaps_internal, inputFlapsSourceExpK)); break;
		case TAxisSource.unity_axis: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.inv_unity_axis: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (-Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.unity_axis_exp: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (ProcessInputAxisPow(Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f, inputFlapsSourceExpK)); break;
		case TAxisSource.inv_unity_axis_exp: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (ProcessInputAxisPow(-Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f, inputFlapsSourceExpK)); break;
		case TAxisSource.mix: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (inputFlaps_internal + Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.inv_mix: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (inputFlaps_internal - Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.mix_exp: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (ProcessInputAxisPow(inputFlaps_internal, inputFlapsSourceExpK) + ProcessInputAxisPow(Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f, inputFlapsSourceExpK) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (ProcessInputAxisPow(inputFlaps_internal, inputFlapsSourceExpK) + ProcessInputAxisPow(-Input.GetAxis(inputFlapsSourceUnityAxis) * 0.5f + 0.5f, inputFlapsSourceExpK) - 0.5f); break;
		default: inputFlaps_output = inputFlaps_output * (1.0f - inputFlapsGlobalSmoothFilter) + inputFlapsGlobalSmoothFilter * (0.0f); break;
		}
		inputFlaps_output = ProcessInputAxisClamp(inputFlaps_output, 0.0f, 1.0f);

		switch (inputBrakesSource) {
		case TAxisSource.user: inputBrakes_internal = Mathf.Clamp01(inputBrakes_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
		case TAxisSource.mix_exp:
		case TAxisSource.inv_mix_exp:
			if (Input.GetKey(inputBrakesKeyForIncrement)) {
				inputBrakes_internal = inputBrakes_internal * (1.0f - inputBrakesKeySmoothFilter) + 1.0f * inputBrakesKeySmoothFilter;
			} else {
				inputBrakes_internal = inputBrakes_internal * (1.0f - inputBrakesKeySmoothFilter) + 0.0f * inputBrakesKeySmoothFilter;
			}
			break;
		}
		switch (inputBrakesSource) {
		case TAxisSource.user: inputBrakes_output = Mathf.Clamp01(inputBrakes_output); break;
		case TAxisSource.keys: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (inputBrakes_internal); break;
		case TAxisSource.keys_exp: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (ProcessInputAxisPow(inputBrakes_internal, inputBrakesSourceExpK)); break;
		case TAxisSource.unity_axis: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.inv_unity_axis: inputBrakes_output = inputBrakes_output * (1.0f - inputElevatorGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (-Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.unity_axis_exp: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (ProcessInputAxisPow(Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f, inputBrakesSourceExpK)); break;
		case TAxisSource.inv_unity_axis_exp: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (ProcessInputAxisPow(-Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f, inputBrakesSourceExpK)); break;
		case TAxisSource.mix: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (inputBrakes_internal + Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.inv_mix: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (inputBrakes_internal - Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.mix_exp: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (ProcessInputAxisPow(inputBrakes_internal, inputBrakesSourceExpK) + ProcessInputAxisPow(Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f, inputBrakesSourceExpK) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (ProcessInputAxisPow(inputBrakes_internal, inputBrakesSourceExpK) + ProcessInputAxisPow(-Input.GetAxis(inputBrakesSourceUnityAxis) * 0.5f + 0.5f, inputBrakesSourceExpK) - 0.5f); break;
		default: inputBrakes_output = inputBrakes_output * (1.0f - inputBrakesGlobalSmoothFilter) + inputBrakesGlobalSmoothFilter * (0.0f); break;
		}
		inputBrakes_output = ProcessInputAxisClamp(inputBrakes_output, 0.0f, 1.0f);

		switch (inputTrailsSource) {
		case TAxisSource.user: inputTrails_internal = Mathf.Clamp01(inputTrails_internal); break;
		case TAxisSource.keys:
		case TAxisSource.keys_exp:
		case TAxisSource.mix:
		case TAxisSource.inv_mix:
		case TAxisSource.mix_exp:
		case TAxisSource.inv_mix_exp:
			if (Input.GetKey(inputTrailsKeyForToggle)) {
				if (inputTrails_internal > 0.9f) {
					inputTrails_internal_enabled = false;
				} else if (inputTrails_internal < 0.1f) {
					inputTrails_internal_enabled = true;
				}
			}
			if (inputTrails_internal_enabled) {
				inputTrails_internal = inputTrails_internal * (1.0f - inputTrailsKeySmoothFilter) + 1.0f * inputTrailsKeySmoothFilter;
			} else {
				inputTrails_internal = inputTrails_internal * (1.0f - inputTrailsKeySmoothFilter) + 0.0f * inputTrailsKeySmoothFilter;
			}
			break;
		}
		switch (inputTrailsSource) {
		case TAxisSource.user: inputTrails_output = Mathf.Clamp01(inputTrails_output); break;
		case TAxisSource.keys: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (inputTrails_internal); break;
		case TAxisSource.keys_exp: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (ProcessInputAxisPow(inputTrails_internal, inputTrailsSourceExpK)); break;
		case TAxisSource.unity_axis: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.inv_unity_axis: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (-Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f); break;
		case TAxisSource.unity_axis_exp: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (ProcessInputAxisPow(Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f, inputTrailsSourceExpK)); break;
		case TAxisSource.inv_unity_axis_exp: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (ProcessInputAxisPow(-Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f, inputTrailsSourceExpK)); break;
		case TAxisSource.mix: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (inputTrails_internal + Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.inv_mix: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (inputTrails_internal - Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f - 0.5f); break;
		case TAxisSource.mix_exp: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (ProcessInputAxisPow(inputTrails_internal, inputTrailsSourceExpK) + ProcessInputAxisPow(Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f, inputTrailsSourceExpK) - 0.5f); break;
		case TAxisSource.inv_mix_exp: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (ProcessInputAxisPow(inputTrails_internal, inputTrailsSourceExpK) + ProcessInputAxisPow(-Input.GetAxis(inputTrailsSourceUnityAxis) * 0.5f + 0.5f, inputTrailsSourceExpK) - 0.5f); break;
		default: inputTrails_output = inputTrails_output * (1.0f - inputTrailsGlobalSmoothFilter) + inputTrailsGlobalSmoothFilter * (0.0f); break;
		}
		inputTrails_output = ProcessInputAxisClamp(inputTrails_output, 0.0f, 1.0f);
	}
	
	float floor_effect_add = 0.0f;
	float floor_effect_ctr = 0.0f;
	float total_effect_add = 0.0f;
	float total_effect_ctr = 0.0f;
	private string ProcessLiftnDragOfElement_kname = "";
	private float ProcessLiftnDragOfElement_klift;
	private float ProcessLiftnDragOfElement_turbulencex;
	private float ProcessLiftnDragOfElement_turbulencey;
	private float ProcessLiftnDragOfElement_turbulencez;
	private float ProcessLiftnDragOfElement_kturbulencex;
	private float ProcessLiftnDragOfElement_kturbulencey;
	private float ProcessLiftnDragOfElement_kturbulencez;
	void ProcessLiftnDragOfElement(GSurface gsurf, LabeledSurfaceDesc gsurfdesc) {
		drag.Set(0.0f, 0.0f, 0.0f);
		lift.Set(0.0f, 0.0f, 0.0f);
		ProcessLiftnDragOfElement_klift = 1.0f;
		ProcessLiftnDragOfElement_kname = "";

		if (gsurf != null) {
			tmp_transform = gsurf.transform;
			ProcessLiftnDragOfElement_kname = gsurf.name;
			if (GAircraftDropPointVelocity) gsurf.lastPosition = tmp_transform.position;
			lastPosition = gsurf.lastPosition;
			surfaceEnableThin = gsurf.surfaceEnableThin;
			surfaceEnablePositive = gsurf.surfaceEnablePositive;
			surfaceEnableNegative = gsurf.surfaceEnableNegative;
			surfaceEnableXPositive = gsurf.surfaceEnableXPositive;
			surfaceEnableYPositive = gsurf.surfaceEnableYPositive;
			surfaceEnableZPositive = gsurf.surfaceEnableZPositive;
			surfaceEnableXNegative = gsurf.surfaceEnableXNegative;
			surfaceEnableYNegative = gsurf.surfaceEnableYNegative;
			surfaceEnableZNegative = gsurf.surfaceEnableZNegative;
			behaviourXPositive = GSurface.behaviourFromTSurfaceBehaviourType(gsurf.behaviourXPositive, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourYPositive = GSurface.behaviourFromTSurfaceBehaviourType(gsurf.behaviourYPositive, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourZPositive = GSurface.behaviourFromTSurfaceBehaviourType(gsurf.behaviourZPositive, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourXNegative = GSurface.behaviourFromTSurfaceBehaviourType(gsurf.behaviourXNegative, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourYNegative = GSurface.behaviourFromTSurfaceBehaviourType(gsurf.behaviourYNegative, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourZNegative = GSurface.behaviourFromTSurfaceBehaviourType(gsurf.behaviourZNegative, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			surfaceDebug = gsurf.surfaceDebug;
			cachedXScale = gsurf.cachedXScale;
			cachedYScale = gsurf.cachedYScale;
			cachedZScale = gsurf.cachedZScale;
			cachedXSurface = gsurf.cachedXSurface;
			cachedYSurface = gsurf.cachedYSurface;
			cachedZSurface = gsurf.cachedZSurface;

			coefficientXDragWhenXPositiveFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurf.shapeXPositive, gsurf.shapeXPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientXDragWhenXPositiveFlow, 1.0f);
			coefficientYDragWhenYPositiveFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurf.shapeYPositive, gsurf.shapeYPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientYDragWhenYPositiveFlow, 1.0f);
			coefficientZDragWhenZPositiveFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurf.shapeZPositive, gsurf.shapeZPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientZDragWhenZPositiveFlow, 1.0f);
			coefficientXDragWhenXNegativeFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurf.shapeXNegative, gsurf.shapeXNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientXDragWhenXNegativeFlow, 1.0f);
			coefficientYDragWhenYNegativeFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurf.shapeYNegative, gsurf.shapeYNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientYDragWhenYNegativeFlow, 1.0f);
			coefficientZDragWhenZNegativeFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurf.shapeZNegative, gsurf.shapeZNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientZDragWhenZNegativeFlow, 1.0f);

			coefficientXLiftWhenYPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeYPositive, gsurf.shapeYPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientXLiftWhenYPositiveFlow, 0.0f);
			coefficientXLiftWhenYNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeYNegative, gsurf.shapeYNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientXLiftWhenYNegativeFlow, 0.0f);
			coefficientXLiftWhenZPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeZPositive, gsurf.shapeZPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientXLiftWhenZPositiveFlow, 0.0f);
			coefficientXLiftWhenZNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeZNegative, gsurf.shapeZNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientXLiftWhenZNegativeFlow, 0.0f);
			coefficientYLiftWhenXPositiveFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurf.shapeXPositive, gsurf.shapeXPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientYLiftWhenXPositiveFlow, 0.0f);
			coefficientYLiftWhenXNegativeFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurf.shapeXNegative, gsurf.shapeXNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientYLiftWhenXNegativeFlow, 0.0f);
			coefficientYLiftWhenZPositiveFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurf.shapeZPositive, gsurf.shapeZPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientYLiftWhenZPositiveFlow, 0.0f);
			coefficientYLiftWhenZNegativeFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurf.shapeZNegative, gsurf.shapeZNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientYLiftWhenZNegativeFlow, 0.1f);
			coefficientZLiftWhenXPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeXPositive, gsurf.shapeXPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientZLiftWhenXPositiveFlow, 0.0f);
			coefficientZLiftWhenXNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeXNegative, gsurf.shapeXNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientZLiftWhenXNegativeFlow, 0.0f);
			coefficientZLiftWhenYPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeYPositive, gsurf.shapeYPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientZLiftWhenYPositiveFlow, 0.0f);
			coefficientZLiftWhenYNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurf.shapeYNegative, gsurf.shapeYNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurf.coefficientZLiftWhenYNegativeFlow, 0.0f);
			tmp_lineRenderer = gsurf.lineRenderer;
			tmp_propWashFactor = gsurf.propWashFactor;
			tmp_coefficientExponent = gsurf.coefficientExponent;
		} else if (gsurfdesc != null) {
			tmp_transform = gsurfdesc.gameObject.transform;
			ProcessLiftnDragOfElement_kname = gsurfdesc.gameObject.name;
			if (GAircraftDropPointVelocity) gsurfdesc.lastPosition = tmp_transform.position;
			lastPosition = gsurfdesc.lastPosition;
			surfaceEnableThin = gsurfdesc.surfaceEnableThin;
			surfaceEnablePositive = gsurfdesc.surfaceEnablePositive;
			surfaceEnableNegative = gsurfdesc.surfaceEnableNegative;
			surfaceEnableXPositive = gsurfdesc.surfaceEnableXPositive;
			surfaceEnableYPositive = gsurfdesc.surfaceEnableYPositive;
			surfaceEnableZPositive = gsurfdesc.surfaceEnableZPositive;
			surfaceEnableXNegative = gsurfdesc.surfaceEnableXNegative;
			surfaceEnableYNegative = gsurfdesc.surfaceEnableYNegative;
			surfaceEnableZNegative = gsurfdesc.surfaceEnableZNegative;
			behaviourXPositive = GSurface.behaviourFromTSurfaceBehaviourType(gsurfdesc.behaviourXPositive, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourYPositive = GSurface.behaviourFromTSurfaceBehaviourType(gsurfdesc.behaviourYPositive, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourZPositive = GSurface.behaviourFromTSurfaceBehaviourType(gsurfdesc.behaviourZPositive, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourXNegative = GSurface.behaviourFromTSurfaceBehaviourType(gsurfdesc.behaviourXNegative, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourYNegative = GSurface.behaviourFromTSurfaceBehaviourType(gsurfdesc.behaviourYNegative, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			behaviourZNegative = GSurface.behaviourFromTSurfaceBehaviourType(gsurfdesc.behaviourZNegative, globalDefaultBehaviour, GSurface.TSurfaceBehaviourType.default_behaviour);
			surfaceDebug = gsurfdesc.surfaceDebug;
			cachedXScale = gsurfdesc.cachedXScale;
			cachedYScale = gsurfdesc.cachedYScale;
			cachedZScale = gsurfdesc.cachedZScale;
			cachedXSurface = gsurfdesc.cachedXSurface;
			cachedYSurface = gsurfdesc.cachedYSurface;
			cachedZSurface = gsurfdesc.cachedZSurface;

			coefficientXDragWhenXPositiveFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurfdesc.shapeXPositive, gsurfdesc.shapeXPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientXDragWhenXPositiveFlow, 1.0f);
			coefficientYDragWhenYPositiveFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurfdesc.shapeYPositive, gsurfdesc.shapeYPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientYDragWhenYPositiveFlow, 1.0f);
			coefficientZDragWhenZPositiveFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurfdesc.shapeZPositive, gsurfdesc.shapeZPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientZDragWhenZPositiveFlow, 1.0f);
			coefficientXDragWhenXNegativeFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurfdesc.shapeXNegative, gsurfdesc.shapeXNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientXDragWhenXNegativeFlow, 1.0f);
			coefficientYDragWhenYNegativeFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurfdesc.shapeYNegative, gsurfdesc.shapeYNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientYDragWhenYNegativeFlow, 1.0f);
			coefficientZDragWhenZNegativeFlow = GSurface.dragFromTSurfaceShapeTypeV(gsurfdesc.shapeZNegative, gsurfdesc.shapeZNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientZDragWhenZNegativeFlow, 1.0f);

			coefficientXLiftWhenYPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeYPositive, gsurfdesc.shapeYPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientXLiftWhenYPositiveFlow, 0.0f);
			coefficientXLiftWhenYNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeYNegative, gsurfdesc.shapeYNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientXLiftWhenYNegativeFlow, 0.0f);
			coefficientXLiftWhenZPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeZPositive, gsurfdesc.shapeZPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientXLiftWhenZPositiveFlow, 0.0f);
			coefficientXLiftWhenZNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeZNegative, gsurfdesc.shapeZNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientXLiftWhenZNegativeFlow, 0.0f);
			coefficientYLiftWhenXPositiveFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurfdesc.shapeXPositive, gsurfdesc.shapeXPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientYLiftWhenXPositiveFlow, 0.0f);
			coefficientYLiftWhenXNegativeFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurfdesc.shapeXNegative, gsurfdesc.shapeXNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientYLiftWhenXNegativeFlow, 0.0f);
			coefficientYLiftWhenZPositiveFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurfdesc.shapeZPositive, gsurfdesc.shapeZPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientYLiftWhenZPositiveFlow, 0.0f);
			coefficientYLiftWhenZNegativeFlow = GSurface.liftFromTSurfaceShapeTypeV(gsurfdesc.shapeZNegative, gsurfdesc.shapeZNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientYLiftWhenZNegativeFlow, 0.1f);
			coefficientZLiftWhenXPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeXPositive, gsurfdesc.shapeXPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientZLiftWhenXPositiveFlow, 0.0f);
			coefficientZLiftWhenXNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeXNegative, gsurfdesc.shapeXNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientZLiftWhenXNegativeFlow, 0.0f);
			coefficientZLiftWhenYPositiveFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeYPositive, gsurfdesc.shapeYPositiveParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientZLiftWhenYPositiveFlow, 0.0f);
			coefficientZLiftWhenYNegativeFlow = GSurface.liftFromTSurfaceShapeTypeH(gsurfdesc.shapeYNegative, gsurfdesc.shapeYNegativeParameter, globalShapeDefaultSurface, globalShapeDefaultSurfaceParameter, gsurfdesc.coefficientZLiftWhenYNegativeFlow, 0.0f);
			tmp_lineRenderer = gsurfdesc.lineRenderer;
			tmp_propWashFactor = gsurfdesc.propWashFactor;
			tmp_coefficientExponent = gsurfdesc.coefficientExponent;
		} else {
			return;
		}
		
		float kk = 0.0f;
		float kklimt = kineticsPropwashLimit * globalSimulationScale;
		float kkmult = kineticsPropwashMultiplier / kklimt * globalSimulationScale;
		kineticsPropwash_internal = 0.0f;
		for (int s = 0; s < labeled_surfacedrives_count; ++s) {
			if (labeled_surfacedrives[s].bladeWashEnabled) {
				tmp_v = (tmp_transform.position - labeled_surfacedrives[s].gameObject.transform.position) * globalSimulationScale;
				tmp_x = Vector3.Dot(tmp_v, labeled_surfacedrives[s].gameObject.transform.right);
				tmp_y = Vector3.Dot(tmp_v, labeled_surfacedrives[s].gameObject.transform.up);
				tmp_z = Vector3.Dot(tmp_v, labeled_surfacedrives[s].gameObject.transform.forward);
				if (tmp_z > 0.0f) kk = Mathf.Abs(1.0f / (tmp_x * tmp_x * tmp_x * tmp_x + tmp_y * tmp_y + tmp_z * tmp_z * tmp_z * tmp_z * tmp_z * tmp_z));
				else kk = Mathf.Abs(1.0f / (tmp_x * tmp_x * tmp_x * tmp_x + tmp_y * tmp_y + tmp_z * tmp_z * 0.1f));
				if (kk > kklimt) kk = kklimt;
				kineticsPropwash_internal += labeled_surfacedrives[s].bladeWashSpeed * kk * kkmult * 0.18f;
			}
		}
		kineticsPropwash_internal *= globalSimulationScale;
		
		lastPosition = lastPosition * (1.0f - kineticsSurfaceDeltaFilter * globalSimulationScale) + tmp_transform.position * kineticsSurfaceDeltaFilter * globalSimulationScale;
		
		switch (kineticsSurfaceMethod) {
		case TSurfaceMethod.rigidbodyGetPointVelocity:
			tmp_v = gameObject.GetComponent<Rigidbody>().GetPointVelocity(tmp_transform.position) + neg_windspeed;
			if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			break;
		case TSurfaceMethod.rigidbodyGetPointVelocityWithPropwash:
			tmp_v = gameObject.GetComponent<Rigidbody>().GetPointVelocity(tmp_transform.position) + neg_windspeed;
			if (kineticsPropwash_internal > 0.0f) tmp_v += (kineticsPropwash_internal) * gameObject.transform.forward * tmp_propWashFactor;
			if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			break;
		case TSurfaceMethod.deltaFiltered:
			tmp_v = (tmp_transform.position - lastPosition) / Time.fixedDeltaTime + neg_windspeed;
			if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			break;
		default: case TSurfaceMethod.deltaFilteredWithPropwash:
			tmp_v = (tmp_transform.position - lastPosition) / Time.fixedDeltaTime + neg_windspeed;
			if (kineticsPropwash_internal > 0.0f) tmp_v += (kineticsPropwash_internal) * gameObject.transform.forward * tmp_propWashFactor;
			if ((kineticsGroundEffect == TGroundEffectType.airplanesoft) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
				ProcessLiftnDragOfElement_klift = kineticsGroundEffectCoeficient * kineticsGroundEffectCoeficientDistance / (tmp_transform.position.y - yPositionOfGround);
				if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
				floor_effect_add += ProcessLiftnDragOfElement_klift;
				floor_effect_ctr += 1.0f;
			} else if (((kineticsGroundEffect == TGroundEffectType.airplanesoft2) || (kineticsGroundEffect == TGroundEffectType.helicoptersoft)) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
				if (Mathf.Abs(kineticsGroundEffectCoeficient) < float.MinValue) kineticsGroundEffectCoeficient = float.MinValue;
				if (Mathf.Abs(kineticsGroundEffectCoeficientDistance) < float.MinValue) kineticsGroundEffectCoeficientDistance = float.MinValue;
				ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue * Mathf.Pow(kineticsGroundEffectMaxValue / kineticsGroundEffectCoeficient, -(tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
				if (ProcessLiftnDragOfElement_klift > kineticsGroundEffectMaxValue) ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue;
				floor_effect_add += ProcessLiftnDragOfElement_klift;
				floor_effect_ctr += 1.0f;
			} else if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
			break;
		}
		if (GAircraftDropPointVelocity) tmp_v = Vector3.zero;
		else tmp_v /= globalSimulationScale;

		total_effect_add += tmp_v.magnitude;
		total_effect_ctr += 1.0f;
		tmp_x = Vector3.Dot(tmp_v, tmp_transform.right);
		tmp_y = Vector3.Dot(tmp_v, tmp_transform.up);
		tmp_z = Vector3.Dot(tmp_v, tmp_transform.forward);
		
		bool xlaminar = false;
		bool ylaminar = false;
		bool zlaminar = false;
		bool xstall = false;
		bool ystall = false;
		bool zstall = false;
		
		if (globalAnalysisLaminarEnabled) {
		
			float klaminar_limitangle = globalDefaultLaminarLimitAngle * Mathf.PI / 180.0f;
			float kturbulence_limitangle = globalDefaultTurbulentLimitAngle * Mathf.PI / 180.0f;
			float kstall_limitangle = globalDefaultStallLimitAngle * Mathf.PI / 180.0f;
			
			xlaminar = ((tmp_x > 0.0f) && (GSurface.laminarFromTSurfaceBehaviourType(behaviourXPositive))) || ((tmp_x < 0.0f) && (GSurface.laminarFromTSurfaceBehaviourType(behaviourXNegative)));
			if (xlaminar) {
				if (Mathf.Abs(tmp_x) > 0.0f) ProcessLiftnDragOfElement_turbulencex = Mathf.Atan(Mathf.Abs(Mathf.Sqrt(tmp_y * tmp_y + tmp_z * tmp_z) / tmp_x));
				else ProcessLiftnDragOfElement_turbulencex = Mathf.PI / 2.0f;
				if ((Mathf.Abs(tmp_x) > 5.0f) && (ProcessLiftnDragOfElement_turbulencex > kstall_limitangle)) xstall = true;
				if (ProcessLiftnDragOfElement_turbulencex < klaminar_limitangle) ProcessLiftnDragOfElement_turbulencex = 0.0f;
				else if (ProcessLiftnDragOfElement_turbulencex > kturbulence_limitangle) ProcessLiftnDragOfElement_turbulencex = 1.0f;
				else ProcessLiftnDragOfElement_turbulencex = (ProcessLiftnDragOfElement_turbulencex - klaminar_limitangle) / (kturbulence_limitangle - klaminar_limitangle);
				ProcessLiftnDragOfElement_kturbulencex = Mathf.Clamp01(1.0f - Mathf.Pow(ProcessLiftnDragOfElement_turbulencex, globalAnalysisLaminarStallExponent) * globalAnalysisLaminarStallMultiplier);
			} else {
				ProcessLiftnDragOfElement_kturbulencex = 1.0f;
			}
			
			ylaminar = ((tmp_y > 0.0f) && (GSurface.laminarFromTSurfaceBehaviourType(behaviourYPositive))) || ((tmp_y < 0.0f) && (GSurface.laminarFromTSurfaceBehaviourType(behaviourYNegative)));
			if (ylaminar) {
				if (Mathf.Abs(tmp_y) > 0.0f) ProcessLiftnDragOfElement_turbulencey = Mathf.Atan(Mathf.Abs(Mathf.Sqrt(tmp_x * tmp_x + tmp_z * tmp_z) / tmp_y));
				else ProcessLiftnDragOfElement_turbulencey = Mathf.PI / 2.0f;
				if ((Mathf.Abs(tmp_y) > 5.0f) && (ProcessLiftnDragOfElement_turbulencey > kstall_limitangle)) ystall = true;
				if (ProcessLiftnDragOfElement_turbulencey < klaminar_limitangle) ProcessLiftnDragOfElement_turbulencey = 0.0f;
				else if (ProcessLiftnDragOfElement_turbulencey > kturbulence_limitangle) ProcessLiftnDragOfElement_turbulencey = 1.0f;
				else ProcessLiftnDragOfElement_turbulencey = (ProcessLiftnDragOfElement_turbulencey - klaminar_limitangle) / (kturbulence_limitangle - klaminar_limitangle);
				ProcessLiftnDragOfElement_kturbulencey = Mathf.Clamp01(1.0f - Mathf.Pow(ProcessLiftnDragOfElement_turbulencey, globalAnalysisLaminarStallExponent) * globalAnalysisLaminarStallMultiplier);
			} else {
				ProcessLiftnDragOfElement_kturbulencey = 1.0f;
			}
	
			zlaminar = ((tmp_z > 0.0f) && (GSurface.laminarFromTSurfaceBehaviourType(behaviourZPositive))) || ((tmp_z < 0.0f) && (GSurface.laminarFromTSurfaceBehaviourType(behaviourZNegative)));
			if (zlaminar) {
				if (Mathf.Abs(tmp_z) > 0.0f) ProcessLiftnDragOfElement_turbulencez = Mathf.Atan(Mathf.Abs(Mathf.Sqrt(tmp_x * tmp_x + tmp_y * tmp_y) / tmp_z));
				else ProcessLiftnDragOfElement_turbulencez = Mathf.PI / 2.0f;
				if ((Mathf.Abs(tmp_z) > 5.0f) && (ProcessLiftnDragOfElement_turbulencez > kstall_limitangle)) zstall = true;
				if (ProcessLiftnDragOfElement_turbulencez < klaminar_limitangle) ProcessLiftnDragOfElement_turbulencez = 0.0f;
				else if (ProcessLiftnDragOfElement_turbulencez > kturbulence_limitangle) ProcessLiftnDragOfElement_turbulencez = 1.0f;
				else ProcessLiftnDragOfElement_turbulencez = (ProcessLiftnDragOfElement_turbulencez - klaminar_limitangle) / (kturbulence_limitangle - klaminar_limitangle);
				ProcessLiftnDragOfElement_kturbulencez = Mathf.Clamp01(1.0f - Mathf.Pow(ProcessLiftnDragOfElement_turbulencez, globalAnalysisLaminarStallExponent) * globalAnalysisLaminarStallMultiplier);
			} else {
				ProcessLiftnDragOfElement_kturbulencez = 1.0f;
			}
	
		} else {
			ProcessLiftnDragOfElement_kturbulencex = 1.0f;
			ProcessLiftnDragOfElement_kturbulencey = 1.0f;
			ProcessLiftnDragOfElement_kturbulencez = 1.0f;
		}
		
		if (surfaceEnableThin) {
			if ((cachedXScale < cachedYScale) && (cachedXScale < cachedZScale)) {
				if (surfaceEnablePositive) surfaceEnableXPositive |= true;
				if (surfaceEnableNegative) surfaceEnableXNegative |= true;
				if (globalRenderForceVectors) {
					if (ProcessLiftnDragOfElement_kturbulencex < 0.9f) {
						if (tmp_lineRenderer != null) tmp_lineRenderer.SetColors(kColorStall, kColorStall);
					} else {
						if (tmp_lineRenderer != null) tmp_lineRenderer.SetColors(kColorRed, kColorRed);
					}
				}
			} else if ((cachedYScale < cachedXScale) && (cachedYScale < cachedZScale)) {
				if (surfaceEnablePositive) surfaceEnableYPositive |= true;
				if (surfaceEnableNegative) surfaceEnableYNegative |= true;
				if (globalRenderForceVectors) {
					if (ProcessLiftnDragOfElement_kturbulencey < 0.9f) {
						if (tmp_lineRenderer != null) tmp_lineRenderer.SetColors(kColorStall, kColorStall);
					} else {
						if (tmp_lineRenderer != null) tmp_lineRenderer.SetColors(kColorGreen, kColorGreen);
					}
				}
			} else if ((cachedZScale < cachedXScale) && (cachedZScale < cachedYScale)) {
				if (surfaceEnablePositive) surfaceEnableZPositive |= true;
				if (surfaceEnableNegative) surfaceEnableZNegative |= true;
				if (globalRenderForceVectors) {
					if (ProcessLiftnDragOfElement_kturbulencez < 0.9f) {
						if (tmp_lineRenderer != null) tmp_lineRenderer.SetColors(kColorStall, kColorStall);
					} else {
						if (tmp_lineRenderer != null) tmp_lineRenderer.SetColors(kColorBlue, kColorBlue);
					}
				}
			}
		} else {
			if (surfaceEnablePositive) surfaceEnableXPositive |= true;
			if (surfaceEnableNegative) surfaceEnableXNegative |= true;
			if (surfaceEnablePositive) surfaceEnableYPositive |= true;
			if (surfaceEnableNegative) surfaceEnableYNegative |= true;
			if (surfaceEnablePositive) surfaceEnableZPositive |= true;
			if (surfaceEnableNegative) surfaceEnableZNegative |= true;
		}
		
		if (surfaceEnableXPositive && (tmp_x > 0)) {
			drag = ProcessLiftnDragOfElement_kturbulencex * (-tmp_x * Mathf.Pow(Mathf.Abs(tmp_x), tmp_coefficientExponent - 1.0f) * tmp_transform.right * coefficientXDragWhenXPositiveFlow * cachedXSurface * kdrag);
			total_surfaces += cachedXSurface;
			if (xstall) stalled_surfaces += cachedXSurface;
			if (xlaminar) {
				analysis_surfaces += cachedXSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencex * cachedXSurface;
			}
		}
		if (surfaceEnableXNegative && (tmp_x < 0)) {
			drag = ProcessLiftnDragOfElement_kturbulencex * (-tmp_x * Mathf.Pow(Mathf.Abs(tmp_x), tmp_coefficientExponent - 1.0f) * tmp_transform.right * coefficientXDragWhenXNegativeFlow * cachedXSurface * kdrag);
			total_surfaces += cachedXSurface;
			if (xstall) stalled_surfaces += cachedXSurface;
			if (xlaminar) {
				analysis_surfaces += cachedXSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencex * cachedXSurface;
			}
		}
		if (surfaceEnableYPositive && (tmp_y > 0)) {
			drag = ProcessLiftnDragOfElement_kturbulencey * (-tmp_y * Mathf.Pow(Mathf.Abs(tmp_y), tmp_coefficientExponent - 1.0f) * tmp_transform.up * coefficientYDragWhenYPositiveFlow * cachedYSurface * kdrag);
			total_surfaces += cachedYSurface;
			if (ystall) stalled_surfaces += cachedYSurface;
			if (ylaminar) {
				analysis_surfaces += cachedYSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencey * cachedYSurface;
			}
		}
		if (surfaceEnableYNegative && (tmp_y < 0)) {
			drag = ProcessLiftnDragOfElement_kturbulencey * (-tmp_y * Mathf.Pow(Mathf.Abs(tmp_y), tmp_coefficientExponent - 1.0f) * tmp_transform.up * coefficientYDragWhenYNegativeFlow * cachedYSurface * kdrag);
			total_surfaces += cachedYSurface;
			if (ystall) stalled_surfaces += cachedYSurface;
			if (ylaminar) {
				analysis_surfaces += cachedYSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencey * cachedYSurface;
			}
		}
		if (surfaceEnableZPositive && (tmp_z > 0)) {
			drag = ProcessLiftnDragOfElement_kturbulencez * (-tmp_z * Mathf.Pow(Mathf.Abs(tmp_z), tmp_coefficientExponent - 1.0f) * tmp_transform.forward * coefficientZDragWhenZPositiveFlow * cachedZSurface * kdrag);
			total_surfaces += cachedZSurface;
			if (zstall) stalled_surfaces += cachedZSurface;
			if (zlaminar) {
				analysis_surfaces += cachedZSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencez * cachedZSurface;
			}
		}
		if (surfaceEnableZNegative && (tmp_z < 0)) {
			drag = ProcessLiftnDragOfElement_kturbulencez * (-tmp_z * Mathf.Pow(Mathf.Abs(tmp_z), tmp_coefficientExponent - 1.0f) * tmp_transform.forward * coefficientZDragWhenZNegativeFlow * cachedZSurface * kdrag);
			total_surfaces += cachedZSurface;
			if (zstall) stalled_surfaces += cachedZSurface;
			if (zlaminar) {
				analysis_surfaces += cachedZSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencez * cachedZSurface;
			}
		}

		if (surfaceEnableXPositive && (tmp_y > 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencey * (tmp_y * Mathf.Pow(Mathf.Abs(tmp_y), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedYSurface * coefficientXLiftWhenYNegativeFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedYSurface;
			if (ystall) stalled_surfaces += cachedYSurface;
			if (ylaminar) {
				analysis_surfaces += cachedYSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencey * cachedYSurface;
			}
		}
		if (surfaceEnableXNegative && (tmp_y < 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencey * (-tmp_y * Mathf.Pow(Mathf.Abs(tmp_y), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedYSurface * coefficientXLiftWhenYPositiveFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedYSurface;
			if (ystall) stalled_surfaces += cachedYSurface;
			if (ylaminar) {
				analysis_surfaces += cachedYSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencey * cachedYSurface;
			}
		}
		if (surfaceEnableXPositive && (tmp_z > 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencez * (tmp_z * Mathf.Pow(Mathf.Abs(tmp_z), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedZSurface * coefficientXLiftWhenZNegativeFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedZSurface;
			if (zstall) stalled_surfaces += cachedZSurface;
			if (zlaminar) {
				analysis_surfaces += cachedZSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencez * cachedZSurface;
			}
		}
		if (surfaceEnableXNegative && (tmp_z < 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencez * (-tmp_z * Mathf.Pow(Mathf.Abs(tmp_z), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedZSurface * coefficientXLiftWhenZPositiveFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedZSurface;
			if (zstall) stalled_surfaces += cachedZSurface;
			if (zlaminar) {
				analysis_surfaces += cachedZSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencez * cachedZSurface;
			}
		}
		if (surfaceEnableYPositive && (tmp_x > 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencex * (tmp_x * Mathf.Pow(Mathf.Abs(tmp_x), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedXSurface * coefficientYLiftWhenXNegativeFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedXSurface;
			if (xstall) stalled_surfaces += cachedXSurface;
			if (xlaminar) {
				analysis_surfaces += cachedXSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencex * cachedXSurface;
			}
		}
		if (surfaceEnableYNegative && (tmp_x < 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencex * (-tmp_x * Mathf.Pow(Mathf.Abs(tmp_x), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedXSurface * coefficientYLiftWhenXPositiveFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedXSurface;
			if (xstall) stalled_surfaces += cachedXSurface;
			if (xlaminar) {
				analysis_surfaces += cachedXSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencex * cachedXSurface;
			}
		}
		if (surfaceEnableYPositive && (tmp_z > 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencez * (tmp_z * Mathf.Pow(Mathf.Abs(tmp_z), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedZSurface * coefficientYLiftWhenZNegativeFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedZSurface;
			if (zstall) stalled_surfaces += cachedZSurface;
			if (zlaminar) {
				analysis_surfaces += cachedZSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencez * cachedZSurface;
			}
		}
		if (surfaceEnableYNegative && (tmp_z < 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencez * (-tmp_z * Mathf.Pow(Mathf.Abs(tmp_z), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedZSurface * coefficientYLiftWhenZPositiveFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedZSurface;
			if (zstall) stalled_surfaces += cachedZSurface;
			if (zlaminar) {
				analysis_surfaces += cachedZSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencez * cachedZSurface;
			}
		}
		if (surfaceEnableZPositive && (tmp_x > 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencex * (tmp_x * Mathf.Pow(Mathf.Abs(tmp_x), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedXSurface * coefficientZLiftWhenXNegativeFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedXSurface;
			if (xstall) stalled_surfaces += cachedXSurface;
			if (xlaminar) {
				analysis_surfaces += cachedXSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencex * cachedXSurface;
			}
		}
		if (surfaceEnableZNegative && (tmp_x < 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencex * (-tmp_x * Mathf.Pow(Mathf.Abs(tmp_x), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedXSurface * coefficientZLiftWhenXPositiveFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedXSurface;
			if (xstall) stalled_surfaces += cachedXSurface;
			if (xlaminar) {
				analysis_surfaces += cachedXSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencex * cachedXSurface;
			}
		}
		if (surfaceEnableZPositive && (tmp_y > 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencey * (tmp_y * Mathf.Pow(Mathf.Abs(tmp_y), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedYSurface * coefficientZLiftWhenYNegativeFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedYSurface;
			if (ystall) stalled_surfaces += cachedYSurface;
			if (ylaminar) {
				analysis_surfaces += cachedYSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencey * cachedYSurface;
			}
		}
		if (surfaceEnableZNegative && (tmp_y < 0)) {
			lift = lift + ProcessLiftnDragOfElement_kturbulencey * (-tmp_y * Mathf.Pow(Mathf.Abs(tmp_y), tmp_coefficientExponent - 1.0f) * tmp_transform.up * cachedYSurface * coefficientZLiftWhenYPositiveFlow * klift * ProcessLiftnDragOfElement_klift);
			total_surfaces += cachedYSurface;
			if (ystall) stalled_surfaces += cachedYSurface;
			if (ylaminar) {
				analysis_surfaces += cachedYSurface;
				laminar_surfaces += ProcessLiftnDragOfElement_kturbulencey * cachedYSurface;
			}
		}

		if (drag.magnitude > max_magnitude_now) max_magnitude_now = drag.magnitude;
		if (lift.magnitude > max_magnitude_now) max_magnitude_now = lift.magnitude;
		
		if (simulationForcesActive && globalApplyForceVectors) {
			if (kineticsFiltersEnabled) if (Vector3.Magnitude(drag) > gameObject.GetComponent<Rigidbody>().mass * kineticsDragForceByMassUnitLimit / globalSimulationScale) drag = (drag / Vector3.Magnitude(drag)) * gameObject.GetComponent<Rigidbody>().mass * kineticsDragForceByMassUnitLimit / globalSimulationScale;
			if (kineticsFiltersEnabled) if (Vector3.Magnitude(lift) > gameObject.GetComponent<Rigidbody>().mass * kineticsLiftForceByMassUnitLimit / globalSimulationScale) lift = (lift / Vector3.Magnitude(lift)) * gameObject.GetComponent<Rigidbody>().mass * kineticsLiftForceByMassUnitLimit / globalSimulationScale;
		}
		
		if (globalRenderForceVectors) {
			if (tmp_lineRenderer != null) {
				tmp_lineRenderer.material = tmp_lineRenderer_material;
				tmp_lineRenderer.SetVertexCount(2);
				tmp_lineRenderer.SetWidth(0.1f * globalSimulationScale, 0.0f * globalSimulationScale);
				tmp_lineRenderer.SetPosition(0, tmp_transform.position);
				tmp_lineRenderer.SetPosition(1, tmp_transform.position + (drag + lift) / max_magnitude * 3.0f);
			}
		}

		if (gsurf != null) {
			gsurf.lastPosition = tmp_transform.position;
			gsurf.drag = gsurf.drag * (1.0f - kineticsDragFilter * globalSimulationScale) + drag * kineticsDragFilter * globalSimulationScale;
			gsurf.lift = gsurf.lift * (1.0f - kineticsLiftFilter * globalSimulationScale) + lift * kineticsLiftFilter * globalSimulationScale;
		} else if (gsurfdesc != null) {
			gsurfdesc.lastPosition = tmp_transform.position;
			gsurfdesc.drag = gsurfdesc.drag * (1.0f - kineticsDragFilter * globalSimulationScale) + drag * kineticsDragFilter * globalSimulationScale;
			gsurfdesc.lift = gsurfdesc.lift * (1.0f - kineticsLiftFilter * globalSimulationScale) + lift * kineticsLiftFilter * globalSimulationScale;
		}
		
		if (surfaceDebug) {
			string enabled_components = "";
			enabled_components += (surfaceEnableXPositive && (tmp_x > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableXNegative && (tmp_x < 0)) ? "1" : "0";
			enabled_components += (surfaceEnableYPositive && (tmp_y > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableYNegative && (tmp_y < 0)) ? "1" : "0";
			enabled_components += (surfaceEnableZPositive && (tmp_z > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableZNegative && (tmp_z < 0)) ? "1" : "0";
			enabled_components += "-";
			enabled_components += (surfaceEnableXPositive && (tmp_y > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableXNegative && (tmp_y < 0)) ? "1" : "0";
			enabled_components += (surfaceEnableXPositive && (tmp_z > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableXNegative && (tmp_z < 0)) ? "1" : "0";
			enabled_components += (surfaceEnableYPositive && (tmp_x > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableYNegative && (tmp_x < 0)) ? "1" : "0";
			enabled_components += (surfaceEnableYPositive && (tmp_z > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableYNegative && (tmp_z < 0)) ? "1" : "0";
			enabled_components += (surfaceEnableZPositive && (tmp_x > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableZNegative && (tmp_x < 0)) ? "1" : "0";
			enabled_components += (surfaceEnableZPositive && (tmp_y > 0)) ? "1" : "0";
			enabled_components += (surfaceEnableZNegative && (tmp_y < 0)) ? "1" : "0";
			if (globalDebugMore) log("surf name(" + ProcessLiftnDragOfElement_kname + ") flags(" + enabled_components + ") speed(" + tmp_x.ToString() + ", " + tmp_y.ToString() + ", " + tmp_z.ToString() + ") turbulence(" + ProcessLiftnDragOfElement_kturbulencex.ToString() + ", " + ProcessLiftnDragOfElement_kturbulencey.ToString() + ", " + ProcessLiftnDragOfElement_kturbulencez.ToString() + ") drag-n-lift" + drag.ToString() + " shape-lift" + lift.ToString() + "");
		}
	}
	
	void ProcessLiftnDragOfElement2ndPass(GSurface gsurf, LabeledSurfaceDesc gsurfdesc) {
		if (simulationForcesActive && globalApplyForceVectors) {
			if (!Input.GetKey(KeyCode.Z)) {
				if (gsurf != null) {
					if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition((gsurf.drag + gsurf.lift) * globalSimulationScale, gsurf.transform.position);
				} else if (gsurfdesc != null) {
					if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition((gsurfdesc.drag + gsurfdesc.lift) * globalSimulationScale, gsurfdesc.gameObject.transform.position);
				} else {
					return;
				}
			}
		}
	}

	void ProcessLiftnDrag_in_update() {
		if (kineticsFiltersEnabled) {
			kineticsAngularVelocity_lastValue.x = kineticsAngularVelocity_lastValue.x * (1.0f - kineticsAngularVelocityFilter) + gameObject.GetComponent<Rigidbody>().angularVelocity.x * kineticsAngularVelocityFilter;
			kineticsAngularVelocity_lastValue.y = kineticsAngularVelocity_lastValue.y * (1.0f - kineticsAngularVelocityFilter) + gameObject.GetComponent<Rigidbody>().angularVelocity.y * kineticsAngularVelocityFilter;
			kineticsAngularVelocity_lastValue.z = kineticsAngularVelocity_lastValue.z * (1.0f - kineticsAngularVelocityFilter) + gameObject.GetComponent<Rigidbody>().angularVelocity.z * kineticsAngularVelocityFilter;
			if (kineticsAngularVelocity_lastValue.magnitude > kineticsAngularVelocityLimit) kineticsAngularVelocity_lastValue = kineticsAngularVelocity_lastValue * (kineticsAngularVelocityLimit / kineticsAngularVelocity_lastValue.magnitude);
		}
	}
	
	void ProcessLiftnDrag() {
		neg_windspeed = -GWindBasic.windAt(gameObject.transform.position) - vibrationGet();
		airdensity = 1.196f * Mathf.Pow(0.00055f, (gameObject.transform.position.y + height) / 60000.0f);
		kdrag = airdensity * 0.5f * globalDragMultiplier;
		klift = airdensity * 0.5f * globalLiftMultiplier;
		
		Vector3 originpos;
		Vector3 vectorpos;
		RaycastHit hit;
		originpos = gameObject.transform.TransformPoint(center);
		vectorpos = Vector3.down;
		if (Physics.Raycast(originpos + vectorpos * kineticsGroundEffectProbeMinDistance, vectorpos, out hit, 999999999.9f, kineticsGroundEffectLayermask)) {
			distanceToGround = hit.distance;
			yPositionOfGround = hit.point.y;
		} else {
			distanceToGround = 999999999.9f;
		}
		if (GAircraftDropPointVelocity) speed_lastPosition = gameObject.transform.position;
		
		floor_effect_add = 0.0f;
		floor_effect_ctr = 0.0f;
		total_effect_add = 0.0f;
		total_effect_ctr = 0.0f;
		stalled_surfaces = 0.0f;
		laminar_surfaces = 0.0f;
		analysis_surfaces = 0.0f;
		total_surfaces = 0.0f;
		for (int i = 0; i < surfaces_count; ++i) {
			ProcessLiftnDragOfElement(surfaces[i], null);
		}
		for (int i = 0; i < labeled_surfaces_count; ++i) {
			ProcessLiftnDragOfElement(null, labeled_surfaces[i]);
		}
		for (int i = 0; i < surfaces_count; ++i) {
			ProcessLiftnDragOfElement2ndPass(surfaces[i], null);
		}
		for (int i = 0; i < labeled_surfaces_count; ++i) {
			ProcessLiftnDragOfElement2ndPass(null, labeled_surfaces[i]);
		}
		if (analysis_surfaces > 0.0f) {
			stall = stalled_surfaces / analysis_surfaces;
		} else {
			stall = 0.0f;
		}

		max_magnitude = max_magnitude * 0.999f + max_magnitude_now * 0.001f;
		if (max_magnitude < 0.1f) max_magnitude = 0.1f;
	}
	
	void ProcessPivots() {
		if (gaugesAltimeterGenerate) gaugesAltimeter_output = (gameObject.transform.position.y + height) / globalSimulationScale;
		if (gaugesVarioGenerate) gaugesVario_output = ((gameObject.transform.position.y + height - gaugesVario_lastValue) / Time.fixedDeltaTime) / globalSimulationScale;
		gaugesVario_lastValue = gameObject.transform.position.y + height;
		if (gaugesRpmGenerate) gaugesRpm_output = (gaugesShaft_output - gaugesRpm_lastValue) / 360.0f / Time.fixedDeltaTime * 60.0f / 4.0f;
		gaugesRpm_lastValue = gaugesShaft_output;
		speed = speed * (1.0f - speed_filter) + (gameObject.transform.position - speed_lastPosition).magnitude / Time.fixedDeltaTime * speed_filter;
		speed_lastPosition = gameObject.transform.position;
		if (gaugesAirspeedGenerate) {
			neg_windspeed = -GWindBasic.windAt(gameObject.transform.position) - vibrationGet();
			gaugesAirspeed_output = (rigidbodyVelocity.magnitude + neg_windspeed.magnitude) / globalSimulationScale;
		}
		if (gaugesHeadingGenerate) gaugesHeading_output = Mathf.Atan(gameObject.transform.forward.x / gameObject.transform.forward.z) * 180.0f / Mathf.PI;
		if (gaugesGsGenerate) gaugesGs_output = Vector3.Dot(rigidbodyVelocity - gaugesGs_lastValue, gameObject.transform.up) / globalSimulationScale;
		if (gaugesGsGenerate) gaugesHGs_output = Vector3.Dot(rigidbodyVelocity - gaugesGs_lastValue, gameObject.transform.right) / globalSimulationScale;
		gaugesGs_lastValue = rigidbodyVelocity;
		for (int i = 0; i < surfacepivots_count; ++i) {
			float angle = 0.0f;
			switch (surfacepivots[i].ch1Source) {
			case GPivot.TAxisSource.dummy: angle += surfacepivots[i].ch1PivotAngleWhenMin; break;
			case GPivot.TAxisSource.ailerons: angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (inputAilerons_output); break;
			case GPivot.TAxisSource.brakes: angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (inputBrakes_output); break;
			case GPivot.TAxisSource.elevator: angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (inputElevator_output); break;
			case GPivot.TAxisSource.flapsdown: angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (inputFlaps_output); break;
			case GPivot.TAxisSource.gearsdown: angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (inputGears_output); break;
			case GPivot.TAxisSource.rudder: angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (inputRudder_output); break;
			case GPivot.TAxisSource.throttle: angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (inputThrottle_output); break;
			case GPivot.TAxisSource.engine: angle += gaugesShaft_output; break;
			case GPivot.TAxisSource.altimeter: angle += (gaugesAltimeter_output - surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.vario: angle += (gaugesVario_output - surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.rpm: angle += (gaugesRpm_output - surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.velocity: angle += (gaugesAirspeed_output - surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.heading: angle += (gaugesHeading_output - surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.gs: angle += (gaugesGs_output - surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.any:
				float ivalue;
				ivalue = GPivot.getAnyPivot(surfacepivots[i].ch1SourceName);
				if (Mathf.Abs(surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) < 0.000001) {
					angle += (ivalue - surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch1PivotTurnsPerUnit;
				} else {
					angle += surfacepivots[i].ch1PivotAngleWhenMin + (surfacepivots[i].ch1PivotAngleWhenMax - surfacepivots[i].ch1PivotAngleWhenMin) * surfacepivots[i].ch1PivotTurnsPerUnit * (ivalue);
				}
				break;
			}
			switch (surfacepivots[i].ch2Source) {
			case GPivot.TAxisSource.dummy: angle += surfacepivots[i].ch2PivotAngleWhenMin; break;
			case GPivot.TAxisSource.ailerons: angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (inputAilerons_output); break;
			case GPivot.TAxisSource.brakes: angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (inputBrakes_output); break;
			case GPivot.TAxisSource.elevator: angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (inputElevator_output); break;
			case GPivot.TAxisSource.flapsdown: angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (inputFlaps_output); break;
			case GPivot.TAxisSource.gearsdown: angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (inputGears_output); break;
			case GPivot.TAxisSource.rudder: angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (inputRudder_output); break;
			case GPivot.TAxisSource.throttle: angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (inputThrottle_output); break;
			case GPivot.TAxisSource.engine: angle += gaugesShaft_output; break;
			case GPivot.TAxisSource.altimeter: angle += (gaugesAltimeter_output - surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.vario: angle += (gaugesVario_output - surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.rpm: angle += (gaugesRpm_output - surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.velocity: angle += (gaugesAirspeed_output - surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.heading: angle += (gaugesHeading_output - surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.gs: angle += (gaugesGs_output - surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.any:
				float ivalue;
				ivalue = GPivot.getAnyPivot(surfacepivots[i].ch2SourceName);
				if (Mathf.Abs(surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) < 0.000001) {
					angle += (ivalue - surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit;
				} else {
					angle += surfacepivots[i].ch2PivotAngleWhenMin + (surfacepivots[i].ch2PivotAngleWhenMax - surfacepivots[i].ch2PivotAngleWhenMin) * surfacepivots[i].ch2PivotTurnsPerUnit * (ivalue);
				}
				break;
			}
			switch (surfacepivots[i].ch3Source) {
			case GPivot.TAxisSource.dummy: angle += surfacepivots[i].ch3PivotAngleWhenMin; break;
			case GPivot.TAxisSource.ailerons: angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (inputAilerons_output); break;
			case GPivot.TAxisSource.brakes: angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (inputBrakes_output); break;
			case GPivot.TAxisSource.elevator: angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (inputElevator_output); break;
			case GPivot.TAxisSource.flapsdown: angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (inputFlaps_output); break;
			case GPivot.TAxisSource.gearsdown: angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (inputGears_output); break;
			case GPivot.TAxisSource.rudder: angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (inputRudder_output); break;
			case GPivot.TAxisSource.throttle: angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (inputThrottle_output); break;
			case GPivot.TAxisSource.engine: angle += gaugesShaft_output; break;
			case GPivot.TAxisSource.altimeter: angle += (gaugesAltimeter_output - surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.vario: angle += (gaugesVario_output - surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.rpm: angle += (gaugesRpm_output - surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.velocity: angle += (gaugesAirspeed_output - surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.heading: angle += (gaugesHeading_output - surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.gs: angle += (gaugesGs_output - surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.any:
				float ivalue;
				ivalue = GPivot.getAnyPivot(surfacepivots[i].ch3SourceName);
				if (Mathf.Abs(surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) < 0.000001) {
					angle += (ivalue - surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * surfacepivots[i].ch3PivotTurnsPerUnit;
				} else {
					angle += surfacepivots[i].ch3PivotAngleWhenMin + (surfacepivots[i].ch3PivotAngleWhenMax - surfacepivots[i].ch3PivotAngleWhenMin) * surfacepivots[i].ch3PivotTurnsPerUnit * (ivalue);
				}
				break;
			}
			if (angle < surfacepivots[i].limitMin) angle = surfacepivots[i].limitMin;
			if (angle > surfacepivots[i].limitMax) angle = surfacepivots[i].limitMax;
			GPivot.setAnyPivot(surfacepivots[i].id, angle);
			switch (surfacepivots[i].rotationPivotAxis) {
			case GPivot.TAxisOrientation.forward: surfacepivots[i].localEulerAngles.Set(surfacepivots[i].rotationAroundRightOffset, surfacepivots[i].rotationAroundUpOffset, surfacepivots[i].rotationAroundForwardOffset + angle); break;
			case GPivot.TAxisOrientation.right: surfacepivots[i].localEulerAngles.Set(surfacepivots[i].rotationAroundRightOffset + angle, surfacepivots[i].rotationAroundUpOffset, surfacepivots[i].rotationAroundForwardOffset); break;
			case GPivot.TAxisOrientation.up: surfacepivots[i].localEulerAngles.Set(surfacepivots[i].rotationAroundRightOffset, surfacepivots[i].rotationAroundUpOffset + angle, surfacepivots[i].rotationAroundForwardOffset); break;
			}
			surfacepivots[i].transform.localEulerAngles = surfacepivots[i].localEulerAngles;
		}
		for (int i = 0; i < labeled_surfacepivots_count; ++i) {
			float angle = 0.0f;
			switch (labeled_surfacepivots[i].ch1Source) {
			case GPivot.TAxisSource.dummy: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin; break;
			case GPivot.TAxisSource.ailerons: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (inputAilerons_output); break;
			case GPivot.TAxisSource.brakes: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (inputBrakes_output); break;
			case GPivot.TAxisSource.elevator: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (inputElevator_output); break;
			case GPivot.TAxisSource.flapsdown: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (inputFlaps_output); break;
			case GPivot.TAxisSource.gearsdown: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (inputGears_output); break;
			case GPivot.TAxisSource.rudder: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (inputRudder_output); break;
			case GPivot.TAxisSource.throttle: angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (inputThrottle_output); break;
			case GPivot.TAxisSource.engine: angle += gaugesShaft_output; break;
			case GPivot.TAxisSource.altimeter: angle += (gaugesAltimeter_output - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.vario: angle += (gaugesVario_output - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.rpm: angle += (gaugesRpm_output - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.velocity: angle += (gaugesAirspeed_output - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.heading: angle += (gaugesHeading_output - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.gs: angle += (gaugesGs_output - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch1PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.any:
				float ivalue;
				ivalue = GPivot.getAnyPivot(labeled_surfacepivots[i].ch1SourceName);
				if (Mathf.Abs(labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) < 0.000001) {
					angle += (ivalue - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch1PivotTurnsPerUnit;
				} else {
					angle += labeled_surfacepivots[i].ch1PivotAngleWhenMin + (labeled_surfacepivots[i].ch1PivotAngleWhenMax - labeled_surfacepivots[i].ch1PivotAngleWhenMin) * labeled_surfacepivots[i].ch1PivotTurnsPerUnit * (ivalue);
				}
				break;
			}
			switch (labeled_surfacepivots[i].ch2Source) {
			case GPivot.TAxisSource.dummy: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin; break;
			case GPivot.TAxisSource.ailerons: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (inputAilerons_output); break;
			case GPivot.TAxisSource.brakes: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (inputBrakes_output); break;
			case GPivot.TAxisSource.elevator: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (inputElevator_output); break;
			case GPivot.TAxisSource.flapsdown: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (inputFlaps_output); break;
			case GPivot.TAxisSource.gearsdown: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (inputGears_output); break;
			case GPivot.TAxisSource.rudder: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (inputRudder_output); break;
			case GPivot.TAxisSource.throttle: angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (inputThrottle_output); break;
			case GPivot.TAxisSource.engine: angle += gaugesShaft_output; break;
			case GPivot.TAxisSource.altimeter: angle += (gaugesAltimeter_output - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.vario: angle += (gaugesVario_output - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.rpm: angle += (gaugesRpm_output - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.velocity: angle += (gaugesAirspeed_output - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.heading: angle += (gaugesHeading_output - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.gs: angle += (gaugesGs_output - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch2PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.any:
				float ivalue;
				ivalue = GPivot.getAnyPivot(labeled_surfacepivots[i].ch2SourceName);
				if (Mathf.Abs(labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) < 0.000001) {
					angle += (ivalue - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch2PivotTurnsPerUnit;
				} else {
					angle += labeled_surfacepivots[i].ch2PivotAngleWhenMin + (labeled_surfacepivots[i].ch2PivotAngleWhenMax - labeled_surfacepivots[i].ch2PivotAngleWhenMin) * labeled_surfacepivots[i].ch2PivotTurnsPerUnit * (ivalue);
				}
				break;
			}
			switch (labeled_surfacepivots[i].ch3Source) {
			case GPivot.TAxisSource.dummy: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin; break;
			case GPivot.TAxisSource.ailerons: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (inputAilerons_output); break;
			case GPivot.TAxisSource.brakes: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (inputBrakes_output); break;
			case GPivot.TAxisSource.elevator: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (inputElevator_output); break;
			case GPivot.TAxisSource.flapsdown: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (inputFlaps_output); break;
			case GPivot.TAxisSource.gearsdown: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (inputGears_output); break;
			case GPivot.TAxisSource.rudder: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (inputRudder_output); break;
			case GPivot.TAxisSource.throttle: angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (inputThrottle_output); break;
			case GPivot.TAxisSource.engine: angle += gaugesShaft_output; break;
			case GPivot.TAxisSource.altimeter: angle += (gaugesAltimeter_output - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.vario: angle += (gaugesVario_output - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.rpm: angle += (gaugesRpm_output - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.velocity: angle += (gaugesAirspeed_output - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.heading: angle += (gaugesHeading_output - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.gs: angle += (gaugesGs_output - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch3PivotTurnsPerUnit; break;
			case GPivot.TAxisSource.any:
				float ivalue;
				ivalue = GPivot.getAnyPivot(labeled_surfacepivots[i].ch3SourceName);
				if (Mathf.Abs(labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) < 0.000001) {
					angle += (ivalue - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * 360.0f * labeled_surfacepivots[i].ch3PivotTurnsPerUnit;
				} else {
					angle += labeled_surfacepivots[i].ch3PivotAngleWhenMin + (labeled_surfacepivots[i].ch3PivotAngleWhenMax - labeled_surfacepivots[i].ch3PivotAngleWhenMin) * labeled_surfacepivots[i].ch3PivotTurnsPerUnit * (ivalue);
				}
				break;
			}
			if (angle < labeled_surfacepivots[i].limitMin) angle = labeled_surfacepivots[i].limitMin;
			if (angle > labeled_surfacepivots[i].limitMax) angle = labeled_surfacepivots[i].limitMax;
			GPivot.setAnyPivot(labeled_surfacepivots[i].id, angle);
			switch (labeled_surfacepivots[i].rotationPivotAxis) {
			case GPivot.TAxisOrientation.forward: labeled_surfacepivots[i].localEulerAngles.Set(labeled_surfacepivots[i].rotationAroundRightOffset, labeled_surfacepivots[i].rotationAroundUpOffset, labeled_surfacepivots[i].rotationAroundForwardOffset + angle); break;
			case GPivot.TAxisOrientation.right: labeled_surfacepivots[i].localEulerAngles.Set(labeled_surfacepivots[i].rotationAroundRightOffset + angle, labeled_surfacepivots[i].rotationAroundUpOffset, labeled_surfacepivots[i].rotationAroundForwardOffset); break;
			case GPivot.TAxisOrientation.up: labeled_surfacepivots[i].localEulerAngles.Set(labeled_surfacepivots[i].rotationAroundRightOffset, labeled_surfacepivots[i].rotationAroundUpOffset + angle, labeled_surfacepivots[i].rotationAroundForwardOffset); break;
			}
			labeled_surfacepivots[i].gameObject.transform.localEulerAngles = labeled_surfacepivots[i].localEulerAngles;
		}
	}
		
	float prop_v = 0.0f;
	float prop_vn = 0.0f;
	float prop_vnd = 0.0f;
	float prop_s = 0.0f;
	float prop_o = 0.0f;
	float prop_a = 0.0f;
	float prop_a_l = 0.0f;
	float prop_a_r = 0.0f;
	float prop_a_b = 0.0f;
	
	float engine_force_common_filtered = 0.0f;
	float engine_force_pitch_filtered = 0.0f;
	float engine_force_roll_filtered = 0.0f;
	float engine_force_yaw_filtered = 0.0f;
	float engine_force_dumper_filtered = 0.0f;
	float overspeed_tmp_z_filtered = 0.0f;
	float overspeed_tmp_z_filter = 0.45f;
	
	void SetDrives(float engine_throttle) {
		kThrottle_ntr_multiplier = 1.0f;
		kThrottle_ntr_time = 0.0f;

		for (int i = 0; i < surfacedrives_count; ++i) {
			surfacedrives[i].drive_shaft = 0.0f;
			surfacedrives[i].drive_rpm = engine_throttle * surfacedrives[i].throttleRpmConversionRatio;
		}
		for (int i = 0; i < labeled_surfacedrives_count; ++i) {
			labeled_surfacedrives[i].drive_shaft = 0.0f;
			labeled_surfacedrives[i].drive_rpm = engine_throttle * labeled_surfacedrives[i].throttleRpmConversionRatio;
		}
	}
	
	void ProcessDrives() {
		float engine_length = 0.0f;
		float engine_throttle = 0.0f;
		float engine_forward_throttle = 0.0f;
		float engine_side_throttle = 0.0f;
		total_engine_force = 0.0f;
		neg_windspeed = GWindBasic.windAt(gameObject.transform.position) + vibrationGet();
		kineticsGroundEffectForce = 0.0f;
		for (int i = 0; i < surfacedrives_count; ++i) {
			tmp_transform = surfacedrives[i].transform;
			surfacedrives[i].lastPosition = surfacedrives[i].lastPosition * (1.0f - kineticsSurfaceDeltaFilter * globalSimulationScale) + tmp_transform.position * kineticsSurfaceDeltaFilter * globalSimulationScale;
			
			switch (kineticsSurfaceMethod) {
			case TSurfaceMethod.rigidbodyGetPointVelocity:
				tmp_v = gameObject.GetComponent<Rigidbody>().GetPointVelocity(tmp_transform.position) - neg_windspeed;
				if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			case TSurfaceMethod.rigidbodyGetPointVelocityWithPropwash:
				tmp_v = gameObject.GetComponent<Rigidbody>().GetPointVelocity(tmp_transform.position) - neg_windspeed;
				if (kineticsPropwash_internal > 0.0f) tmp_v += (kineticsPropwash_internal) * gameObject.transform.forward * tmp_propWashFactor;
				if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			case TSurfaceMethod.deltaFiltered:
				tmp_v = (tmp_transform.position - surfacedrives[i].lastPosition) / Time.fixedDeltaTime - neg_windspeed;
				if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			default: case TSurfaceMethod.deltaFilteredWithPropwash:
				tmp_v = (tmp_transform.position - surfacedrives[i].lastPosition) / Time.fixedDeltaTime - neg_windspeed;
				if (kineticsPropwash_internal > 0.0f) tmp_v += (kineticsPropwash_internal) * gameObject.transform.forward * tmp_propWashFactor;
				if ((kineticsGroundEffect == TGroundEffectType.airplanesoft) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
					ProcessLiftnDragOfElement_klift = kineticsGroundEffectCoeficient * kineticsGroundEffectCoeficientDistance / (tmp_transform.position.y - yPositionOfGround);
					if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
					floor_effect_add += ProcessLiftnDragOfElement_klift;
					floor_effect_ctr += 1.0f;
				} else if (((kineticsGroundEffect == TGroundEffectType.airplanesoft2) || (kineticsGroundEffect == TGroundEffectType.helicoptersoft)) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
					if (Mathf.Abs(kineticsGroundEffectCoeficient) < float.MinValue) kineticsGroundEffectCoeficient = float.MinValue;
					if (Mathf.Abs(kineticsGroundEffectCoeficientDistance) < float.MinValue) kineticsGroundEffectCoeficientDistance = float.MinValue;
					ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue * Mathf.Pow(kineticsGroundEffectMaxValue / kineticsGroundEffectCoeficient, -(tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
					if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
					if (ProcessLiftnDragOfElement_klift > kineticsGroundEffectMaxValue) ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue;
					floor_effect_add += ProcessLiftnDragOfElement_klift;
					floor_effect_ctr += 1.0f;
				} else if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			}
			if (GAircraftDropPointVelocity) tmp_v = Vector3.zero;
			else tmp_v /= globalSimulationScale;
			
			switch (surfacedrives[i].powerControlBy) {
			default: case GDrive.TDrivePowerControlBy.defaultvalue:
				switch (surfacedrives[i].type) {
				default: engine_throttle = inputThrottle_output; break;
				case GDrive.TDriveType.tailrotor: case GDrive.TDriveType.right_rotor2: engine_throttle = inputRudder_output; break;
				}
				break;
			case GDrive.TDrivePowerControlBy.throttle:
				engine_throttle = inputThrottle_output;
				break;
			case GDrive.TDrivePowerControlBy.elevator:
				engine_throttle = inputElevator_output;
				break;
			case GDrive.TDrivePowerControlBy.ailerons:
				engine_throttle = inputAilerons_output;
				break;
			case GDrive.TDrivePowerControlBy.rudder:
				engine_throttle = inputRudder_output;
				break;
			case GDrive.TDrivePowerControlBy.pivot:
				engine_throttle = GPivot.getAnyPivot(surfacedrives[i].powerControlByPivot);
				break;
			}
			switch (surfacedrives[i].powerCyclicForwardControlBy) {
			default: case GDrive.TDrivePowerControlBy.defaultvalue:
				switch (surfacedrives[i].type) {
				default: engine_forward_throttle = inputElevator_output; break;
				}
				break;
			case GDrive.TDrivePowerControlBy.throttle:
				engine_forward_throttle = inputThrottle_output;
				break;
			case GDrive.TDrivePowerControlBy.elevator:
				engine_forward_throttle = inputElevator_output;
				break;
			case GDrive.TDrivePowerControlBy.ailerons:
				engine_forward_throttle = inputAilerons_output;
				break;
			case GDrive.TDrivePowerControlBy.rudder:
				engine_forward_throttle = inputRudder_output;
				break;
			case GDrive.TDrivePowerControlBy.pivot:
				engine_forward_throttle = GPivot.getAnyPivot(surfacedrives[i].powerCyclicForwardControlByPivot);
				break;
			}
			switch (surfacedrives[i].powerCyclicSideControlBy) {
			default: case GDrive.TDrivePowerControlBy.defaultvalue:
				switch (surfacedrives[i].type) {
				default: engine_side_throttle = inputAilerons_output; break;
				}
				break;
			case GDrive.TDrivePowerControlBy.throttle:
				engine_side_throttle = inputThrottle_output;
				break;
			case GDrive.TDrivePowerControlBy.elevator:
				engine_side_throttle = inputElevator_output;
				break;
			case GDrive.TDrivePowerControlBy.ailerons:
				engine_side_throttle = inputAilerons_output;
				break;
			case GDrive.TDrivePowerControlBy.rudder:
				engine_side_throttle = inputRudder_output;
				break;
			case GDrive.TDrivePowerControlBy.pivot:
				engine_side_throttle = GPivot.getAnyPivot(surfacedrives[i].powerCyclicSideControlByPivot);
				break;
			}
			switch (surfacedrives[i].type) {
			default:
				if (engine_throttle <= 1.0f) {
					surfacedrives[i].drive_rpm = (1.0f - surfacedrives[i].throttleRpmConversionFilter) * surfacedrives[i].drive_rpm + surfacedrives[i].throttleRpmConversionFilter * (surfacedrives[i].throttleIdle + (surfacedrives[i].throttleMax - surfacedrives[i].throttleIdle) * engine_throttle) * surfacedrives[i].throttleRpmConversionRatio * kThrottle_ntr_multiplier;
					if (globalRenderForceVectors) if (surfacedrives[i].lineRenderer != null) surfacedrives[i].lineRenderer.SetColors(Color.blue, Color.red);
				} else {
					surfacedrives[i].drive_rpm = (1.0f - surfacedrives[i].throttleRpmConversionFilter) * surfacedrives[i].drive_rpm + surfacedrives[i].throttleRpmConversionFilter * (surfacedrives[i].throttleMax + (surfacedrives[i].throttleAfterburner - surfacedrives[i].throttleMax) * (engine_throttle - 1.0f)) * surfacedrives[i].throttleRpmConversionRatio * kThrottle_ntr_multiplier;
					if (globalRenderForceVectors) if (surfacedrives[i].lineRenderer != null) surfacedrives[i].lineRenderer.SetColors(Color.blue, Color.yellow);
				}
				//if (kThrottle_ntr_multiplier > 1.01f) surfacedrives[i].drive_rpm = surfacedrives[i].drive_rpm * kThrottle_ntr_multiplier;
				break;
			case GDrive.TDriveType.rotor:
			case GDrive.TDriveType.tailrotor:
				surfacedrives[i].drive_rpm = (1.0f - surfacedrives[i].throttleRpmConversionFilter) * surfacedrives[i].drive_rpm + surfacedrives[i].throttleRpmConversionFilter * surfacedrives[i].throttleMax * surfacedrives[i].throttleRpmConversionRatio;
				if (globalRenderForceVectors) if (surfacedrives[i].lineRenderer != null) surfacedrives[i].lineRenderer.SetColors(Color.blue, Color.yellow);
				break;
			case GDrive.TDriveType.rotor_basic:
				/*
				if ((simulationFirstFrames_complete) && (surfacedrives[i].powered)) {
					surfacedrives[i].poweredFactor = surfacedrives[i].poweredFactor * (1.0f - surfacedrives[i].poweredFactorFilter) + 1.0f * surfacedrives[i].poweredFactorFilter;
				} else {
					surfacedrives[i].poweredFactor = surfacedrives[i].poweredFactor * (1.0f - surfacedrives[i].poweredFactorFilter) + 0.0f * surfacedrives[i].poweredFactorFilter;
				}
				
				ProcessLiftnDragOfElement_klift = 1.0f;
				if ((kineticsGroundEffect == TGroundEffectType.helicoptersoft) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
					if (Mathf.Abs(kineticsGroundEffectCoeficient) < float.MinValue) kineticsGroundEffectCoeficient = float.MinValue;
					if (Mathf.Abs(kineticsGroundEffectCoeficientDistance) < float.MinValue) kineticsGroundEffectCoeficientDistance = float.MinValue;
					ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue * Mathf.Pow(kineticsGroundEffectMaxValue / kineticsGroundEffectCoeficient, -(tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
					if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
					if (ProcessLiftnDragOfElement_klift > kineticsGroundEffectMaxValue) ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue;
				}

				tmp_x = Vector3.Dot(tmp_v, surfacedrives[i].gameObject.transform.right);
				tmp_y = Vector3.Dot(tmp_v, surfacedrives[i].gameObject.transform.up);
				tmp_z = Vector3.Dot(tmp_v, surfacedrives[i].gameObject.transform.forward);
				tmp_xz = Mathf.Sqrt(tmp_x * tmp_x + tmp_z * tmp_z);
				
				float mainrotor_rpm_multiplier = 1.0f * surfacedrives[i].poweredFactor;
				float mainrotor_proyectedvelocity_magnitude2 = Mathf.Pow(surfacedrives[i].basicRotorVelocityFactor, -surfacedrives[i].basicRotorProjectedVelocityFactor / tmp_xz) * surfacedrives[i].basicRotorProjectedVelocityMultiplier;
				float mainrotor_angularvelocity_dump = (100.0f - mainrotor_proyectedvelocity_magnitude2) / (100.0f + 5.0f);
				float mainrotor_proyectedvelocity_magnitude_bias = mainrotor_proyectedvelocity_magnitude2 / 6.0f;
				float mainrotor_proyectedvelocity_magnitude_forward = (surfacedrives[i].basicRotorProjectedVelocityForwardOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;
				float mainrotor_proyectedvelocity_magnitude_rear = (surfacedrives[i].basicRotorProjectedVelocityRearOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;
				float mainrotor_proyectedvelocity_magnitude_left = (surfacedrives[i].basicRotorProjectedVelocityLeftOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;
				float mainrotor_proyectedvelocity_magnitude_right = (surfacedrives[i].basicRotorProjectedVelocityRightOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;

				float engine_force_dumper = -Mathf.PI * surfacedrives[i].dumperForceMultiplier * surfacedrives[i].bladeLength * surfacedrives[i].bladeLength * kdrag * tmp_y * Mathf.Abs(tmp_y) * mainrotor_rpm_multiplier;
				if (tmp_v.y < 0.0f) engine_force_dumper *= ProcessLiftnDragOfElement_klift;
				gameObject.rigidbody.angularVelocity = gameObject.rigidbody.angularVelocity * mainrotor_angularvelocity_dump;

				float throttle_offset_excess = surfacedrives[i].throttleBiasOffset;
				float throttle_multiplier_excess = surfacedrives[i].throttleBiasMultiplier;
				float throttle_rear_multiplier_excess = surfacedrives[i].throttleForwardRearOffset;
				float throttle_right_multiplier_excess = surfacedrives[i].throttleLeftRightOffset;
				float throttle_multiplier = gameObject.rigidbody.mass * Physics.gravity.magnitude * throttle_multiplier_excess;
				float yaw_multiplier = surfacedrives[i].yawForceMultiplier;
				float up_projection = Vector3.Dot(Vector3.up, surfacedrives[i].gameObject.transform.up);
				
				float roll_vibration_value = surfacedrives[i].overspeedRollFactorVibration * vibration(surfacedrives[i].overspeedRollFactorVibratorId);
				float yaw_vibration_value = surfacedrives[i].overspeedYawFactorVibration * vibration(surfacedrives[i].overspeedYawFactorVibratorId);
				float pitch_vibration_value = surfacedrives[i].overspeedPitchFactorVibration * vibration(surfacedrives[i].overspeedPitchFactorVibratorId);
				
				if (surfacedrives[i].overspeedPitchFactorAtSpeed <= 0.1f) surfacedrives[i].overspeedPitchFactorAtSpeed = 0.1f;
				float throttle_overspeed_roll_factor_multiplier_value = gameObject.rigidbody.mass * (surfacedrives[i].overspeedRollFactorMultiplier + roll_vibration_value) * up_projection * up_projection * up_projection * mainrotor_rpm_multiplier;
				if (surfacedrives[i].overspeedRollFactorAtSpeed <= 0.1f) surfacedrives[i].overspeedRollFactorAtSpeed = 0.1f;
				float throttle_overspeed_yaw_factor_multiplier_value = gameObject.rigidbody.mass * (surfacedrives[i].overspeedYawFactorMultiplier + yaw_vibration_value) * mainrotor_rpm_multiplier;
				if (surfacedrives[i].overspeedYawFactorAtSpeed <= 0.1f) surfacedrives[i].overspeedYawFactorAtSpeed = 0.1f;
				float throttle_overspeed_pitch_factor_multiplier_value = gameObject.rigidbody.mass * (surfacedrives[i].overspeedPitchFactorMultiplier + pitch_vibration_value);
				
				float pitch_overspeed_factor_value = Mathf.Max(surfacedrives[i].overspeedPitchFactorMin, Mathf.Min(surfacedrives[i].overspeedPitchFactorMax, Mathf.Abs(Mathf.Pow(surfacedrives[i].overspeedPitchFactorExponent, Mathf.Max(0.0f, Mathf.Abs(tmp_z) + surfacedrives[i].overspeedPitchFactorOffsetSpeed) / surfacedrives[i].overspeedPitchFactorAtSpeed) - 1.0f))) + surfacedrives[i].overspeedPitchFactorOffsetValue;
				float yaw_overspeed_factor_value = Mathf.Max(surfacedrives[i].overspeedYawFactorMin, Mathf.Min(surfacedrives[i].overspeedYawFactorMax, Mathf.Abs(Mathf.Pow(surfacedrives[i].overspeedYawFactorExponent, Mathf.Max(0.0f, Mathf.Abs(tmp_z) + surfacedrives[i].overspeedYawFactorOffsetSpeed) / surfacedrives[i].overspeedYawFactorAtSpeed) - 1.0f))) + surfacedrives[i].overspeedYawFactorOffsetValue;
				float roll_overspeed_factor_value = Mathf.Max(surfacedrives[i].overspeedRollFactorMin, Mathf.Min(surfacedrives[i].overspeedRollFactorMax, Mathf.Abs(Mathf.Pow(surfacedrives[i].overspeedRollFactorExponent, Mathf.Max(0.0f, Mathf.Abs(tmp_z) + surfacedrives[i].overspeedRollFactorOffsetSpeed) / surfacedrives[i].overspeedRollFactorAtSpeed) - 1.0f))) + surfacedrives[i].overspeedRollFactorOffsetValue;

				if ((Mathf.Abs(yaw_overspeed_factor_value) > 0.1f) || (Mathf.Abs(pitch_overspeed_factor_value) > 0.1f)) {
					gameObject.rigidbody.velocity += gameObject.transform.right * yaw_overspeed_factor_value * yaw_vibration_value + gameObject.transform.up * pitch_overspeed_factor_value * pitch_vibration_value;
				}
				
				float engine_force_yaw = (inputRudder_output - 0.5f) * yaw_multiplier + throttle_overspeed_yaw_factor_multiplier_value * yaw_overspeed_factor_value;
				float engine_force_common;
				if ((((tmp_y > 0.0f) && (inputThrottle_output + throttle_offset_excess > 0.0f)) || ((tmp_y < 0.0f) && (inputThrottle_output + throttle_offset_excess < 0.0f)))) {
					engine_force_common = (inputThrottle_output + throttle_offset_excess) * throttle_multiplier * Mathf.Min(0.005f, Mathf.Pow(surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / surfacedrives[i].climbReductionWithSpeedAtSpeed) * Mathf.Pow(surfacedrives[i].climbReductionWithVerticalSpeed, Mathf.Abs(tmp_y) / surfacedrives[i].climbReductionWithVerticalSpeedAtSpeed));
				} else {
					engine_force_common = (inputThrottle_output + throttle_offset_excess) * throttle_multiplier * Mathf.Min(0.005f, Mathf.Pow(surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / surfacedrives[i].climbReductionWithSpeedAtSpeed) * Mathf.Pow(surfacedrives[i].climbReductionWithVerticalSpeed, -Mathf.Abs(tmp_y) / surfacedrives[i].climbReductionWithVerticalSpeedAtSpeed));
				}
				float roll_speed = gameObject.transform.InverseTransformDirection(gameObject.rigidbody.angularVelocity).z;
				float engine_force_pitch = (engine_forward_throttle - 0.5f) * 20f * 55.5f * (20.0f * Mathf.Min(0.1f, Mathf.Pow(surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / surfacedrives[i].climbReductionWithSpeedAtSpeed))) + throttle_overspeed_pitch_factor_multiplier_value * pitch_overspeed_factor_value;
				float engine_force_roll = (engine_side_throttle - 0.5f) * 100f * 1.9f * (20.0f * Mathf.Min(0.1f, Mathf.Pow(surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / surfacedrives[i].climbReductionWithSpeedAtSpeed))) + throttle_overspeed_roll_factor_multiplier_value * roll_overspeed_factor_value * Mathf.Sign(surfacedrives[i].overspeedRollFactor);
				if (roll_speed > 0.0f) {
					if (engine_force_roll < 0.0f) {
						engine_force_roll = engine_force_roll * Mathf.Pow(surfacedrives[i].rollReductionWithRollSpeed, Mathf.Abs(roll_speed) / surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
						//engine_force_roll = engine_force_roll + 0.001f * gameObject.rigidbody.mass * Mathf.Pow(surfacedrives[i].rollReductionWithRollSpeed, -Mathf.Abs(roll_speed) / surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
					}
				} else {
					if (engine_force_roll > 0.0f) {
						engine_force_roll = engine_force_roll * Mathf.Pow(surfacedrives[i].rollReductionWithRollSpeed, Mathf.Abs(roll_speed) / surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
						//engine_force_roll = engine_force_roll - 0.001f * gameObject.rigidbody.mass * Mathf.Pow(surfacedrives[i].rollReductionWithRollSpeed, -Mathf.Abs(roll_speed) / surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
					}
				}

				float engine_force_common_filter = surfacedrives[i].commonFilter;
				float engine_force_pitch_filter = surfacedrives[i].pitchFilter;
				float engine_force_roll_filter = surfacedrives[i].rollFilter;
				float engine_force_yaw_filter = surfacedrives[i].yawFilter;
				float engine_force_dumper_filter = surfacedrives[i].dumperFilter;
				engine_force_common_filtered = engine_force_common_filtered * (1f - engine_force_common_filter) + engine_force_common * engine_force_common_filter;
				engine_force_pitch_filtered = engine_force_pitch_filtered * (1f - engine_force_pitch_filter) + engine_force_pitch * engine_force_pitch_filter;
				engine_force_roll_filtered = engine_force_roll_filtered * (1f - engine_force_roll_filter) + engine_force_roll * engine_force_roll_filter;
				engine_force_yaw_filtered = engine_force_yaw_filtered * (1f - engine_force_yaw_filter) + engine_force_yaw * engine_force_yaw_filter;
				engine_force_dumper_filtered = engine_force_dumper_filtered * (1f - engine_force_dumper_filter) + engine_force_dumper * engine_force_dumper_filter;
				if (!((engine_force_common_filtered >= 0f) || (engine_force_common_filtered < 0f))) engine_force_common_filtered = 0.0f;
				if (!((engine_force_pitch_filtered >= 0f) || (engine_force_pitch_filtered < 0f))) engine_force_pitch_filtered = 0.0f;
				if (!((engine_force_roll_filtered >= 0f) || (engine_force_roll_filtered < 0f))) engine_force_roll_filtered = 0.0f;
				if (!((engine_force_yaw_filtered >= 0f) || (engine_force_yaw_filtered < 0f))) engine_force_yaw_filtered = 0.0f;
				if (!((engine_force_dumper_filtered >= 0f) || (engine_force_dumper_filtered < 0f))) engine_force_dumper_filtered = 0.0f;
				
				float engine_force_fr_limit = 100.0f * gameObject.rigidbody.mass;
				float engine_force_lr_limit = 150.0f * gameObject.rigidbody.mass;
				
				float engine_force_front, engine_force_rear, engine_force_left, engine_force_right;
				if (throttle_rear_multiplier_excess > 0.0f) engine_force_front = engine_force_dumper_filtered + ((1f + throttle_rear_multiplier_excess) * engine_force_common_filtered + engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_forward;
				else engine_force_front = engine_force_dumper_filtered + (engine_force_common_filtered + engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_forward;
				if (throttle_rear_multiplier_excess < 0.0f) engine_force_rear = engine_force_dumper_filtered + ((1f - throttle_rear_multiplier_excess) * engine_force_common_filtered - engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_rear;
				else engine_force_rear = engine_force_dumper_filtered + (engine_force_common_filtered - engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_rear;
				if (throttle_right_multiplier_excess < 0.0f) engine_force_left = engine_force_dumper_filtered + ((1f + throttle_right_multiplier_excess) * engine_force_common_filtered + engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_left;
				else engine_force_left = engine_force_dumper_filtered + (engine_force_common_filtered + engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_left;
				if (throttle_right_multiplier_excess < 0.0f) engine_force_right = engine_force_dumper_filtered + ((1f - throttle_right_multiplier_excess) * engine_force_common_filtered - engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_right;
				else engine_force_right = engine_force_dumper_filtered + (engine_force_common_filtered - engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_right;
				
				if (engine_force_front < -engine_force_fr_limit) engine_force_front = -engine_force_fr_limit;
				else if (engine_force_front > engine_force_fr_limit) engine_force_front = engine_force_fr_limit;
				if (engine_force_rear < -engine_force_fr_limit) engine_force_rear = -engine_force_fr_limit;
				else if (engine_force_rear > engine_force_fr_limit) engine_force_rear = engine_force_fr_limit;
				if (engine_force_left < -engine_force_lr_limit) engine_force_left = -engine_force_lr_limit;
				else if (engine_force_left > engine_force_lr_limit) engine_force_left = engine_force_lr_limit;
				if (engine_force_right < -engine_force_lr_limit) engine_force_right = -engine_force_lr_limit;
				else if (engine_force_right > engine_force_lr_limit) engine_force_right = engine_force_lr_limit;

				float virtual_density = Mathf.Pow(airdensity / surfacedrives[i].basicRotorAirdensityBias, surfacedrives[i].basicRotorAirdensityExp);
				if (simulationForcesActive) gameObject.rigidbody.AddForceAtPosition(virtual_density * surfacedrives[i].gameObject.transform.up * engine_force_front, surfacedrives[i].gameObject.transform.position + surfacedrives[i].gameObject.transform.forward * surfacedrives[i].basicRotorBladeForcePoint * surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.rigidbody.AddForceAtPosition(virtual_density * surfacedrives[i].gameObject.transform.up * engine_force_rear, surfacedrives[i].gameObject.transform.position - surfacedrives[i].gameObject.transform.forward *surfacedrives[i].basicRotorBladeForcePoint * surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.rigidbody.AddForceAtPosition(virtual_density * surfacedrives[i].gameObject.transform.up * engine_force_left, surfacedrives[i].gameObject.transform.position - surfacedrives[i].gameObject.transform.right * surfacedrives[i].basicRotorBladeForcePoint * surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.rigidbody.AddForceAtPosition(virtual_density * surfacedrives[i].gameObject.transform.up * engine_force_right, surfacedrives[i].gameObject.transform.position + surfacedrives[i].gameObject.transform.right * surfacedrives[i].basicRotorBladeForcePoint * surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.rigidbody.AddForceAtPosition(-virtual_density * surfacedrives[i].gameObject.transform.right * engine_force_yaw_filtered, surfacedrives[i].gameObject.transform.TransformPoint(surfacedrives[i].basicRotorTailOffset));
				
				surfacedrives[i].drive_rpm = mainrotor_rpm_multiplier * surfacedrives[i].theoreticalTargetRpms * (1f + surfacedrives[i].basicRotorVariationMultiplier * tmp_y / (surfacedrives[i].overspeedYawFactorAtSpeed + surfacedrives[i].overspeedRollFactorAtSpeed + surfacedrives[i].overspeedPitchFactorAtSpeed));
				*/
				break;
			case GDrive.TDriveType.forward_rotor2:
			case GDrive.TDriveType.up_rotor2:
			case GDrive.TDriveType.right_rotor2:
				break;
			}
			switch (surfacedrives[i].type) {
			default: case GDrive.TDriveType.basic: surfacedrives[i].drive_output = surfacedrives[i].drive_rpm; break;
			case GDrive.TDriveType.propeller:
				prop_v = surfacedrives[i].drive_rpm * surfacedrives[i].bladeLength * 0.052359878f;
				prop_a = 1.570796327f - surfacedrives[i].bladeAngleYaw * 0.017453293f;
				prop_vn = prop_v * Mathf.Cos(prop_a);
				prop_vnd = Vector3.Dot(rigidbodyVelocity + neg_windspeed, gameObject.transform.forward);
				if (prop_vn > prop_vnd) prop_vnd = prop_vn - prop_vnd;
				else prop_vnd = prop_vn;
				prop_s = 0.5f * surfacedrives[i].bladeLength * (surfacedrives[i].bladeWidthMin + surfacedrives[i].bladeWidthMax);
				prop_o = Mathf.Cos(surfacedrives[i].bladeAnglePitch * 0.017453293f) * prop_s * surfacedrives[i].bladeShapeCoefficient * kdrag * surfacedrives[i].bladeNumber;
				surfacedrives[i].drive_output = prop_o * prop_vnd * prop_vnd;
				surfacedrives[i].bladeWashSpeed = prop_vnd;
				break;
			case GDrive.TDriveType.rotor:
				log("rotor control: engine_throttle = " + engine_throttle.ToString() + "; engine_forward_throttle = " + engine_forward_throttle.ToString() + "; engine_side_throttle = " + engine_side_throttle.ToString() + ";");
				prop_v = surfacedrives[i].drive_rpm * surfacedrives[i].bladeLength * 0.052359878f;
				prop_a = 1.570796327f - surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + surfacedrives[i].rotorCollectiveBias) * surfacedrives[i].rotorCollectiveCoefficient + (engine_forward_throttle - 0.5f) * surfacedrives[i].rotorCyclicForwardCoefficient) * 0.017453293f;
				prop_a_b = 1.570796327f - surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + surfacedrives[i].rotorCollectiveBias) * surfacedrives[i].rotorCollectiveCoefficient - (engine_forward_throttle - 0.5f) * surfacedrives[i].rotorCyclicForwardCoefficient) * 0.017453293f;
				prop_a_l = 1.570796327f - surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + surfacedrives[i].rotorCollectiveBias) * surfacedrives[i].rotorCollectiveCoefficient + (engine_side_throttle - 0.5f) * surfacedrives[i].rotorCyclicSideCoefficient) * 0.017453293f;
				prop_a_r = 1.570796327f - surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + surfacedrives[i].rotorCollectiveBias) * surfacedrives[i].rotorCollectiveCoefficient - (engine_side_throttle - 0.5f) * surfacedrives[i].rotorCyclicSideCoefficient) * 0.017453293f;
				prop_vn = prop_v * Mathf.Cos((prop_a + prop_a_l + prop_a_r + prop_a_b) / 4.0f);
				prop_vnd = prop_vn;
				prop_s = 0.5f * surfacedrives[i].bladeLength * (surfacedrives[i].bladeWidthMin + surfacedrives[i].bladeWidthMax);
				prop_o = Mathf.Cos(surfacedrives[i].bladeAnglePitch * 0.017453293f) * prop_s * surfacedrives[i].bladeShapeCoefficient * kdrag * surfacedrives[i].bladeNumber;
				surfacedrives[i].drive_output = prop_o * prop_vnd * Mathf.Abs(prop_vnd);
				kineticsGroundEffectForce += prop_vn / (surfacedrives[i].gameObject.transform.position.y - yPositionOfGround);
				surfacedrives[i].drive_rpm = (1.0f - surfacedrives[i].throttleRpmConversionFilter) * surfacedrives[i].drive_rpm + surfacedrives[i].throttleRpmConversionFilter * (surfacedrives[i].throttleMax * surfacedrives[i].throttleRpmConversionRatio - surfacedrives[i].rotorAutorotationCoefficient * surfacedrives[i].drive_output * Mathf.Cos((prop_a + prop_a_l + prop_a_r + prop_a_b) / 4.0f));
				surfacedrives[i].bladeWashSpeed = 0.0f;
				break;
			case GDrive.TDriveType.tailrotor:
				prop_v = surfacedrives[i].drive_rpm * surfacedrives[i].bladeLength * 0.052359878f;
				prop_a = 1.570796327f - surfacedrives[i].bladeAngleYaw * (0.5f - engine_throttle) * 0.017453293f;
				prop_vn = prop_v * Mathf.Cos(prop_a);
				prop_vnd = prop_vn;
				prop_s = 0.5f * surfacedrives[i].bladeLength * (surfacedrives[i].bladeWidthMin + surfacedrives[i].bladeWidthMax);
				prop_o = Mathf.Cos(surfacedrives[i].bladeAnglePitch * 0.017453293f) * prop_s * surfacedrives[i].bladeShapeCoefficient * kdrag * surfacedrives[i].bladeNumber;
				surfacedrives[i].drive_output = prop_o * prop_vnd * Mathf.Abs(prop_vnd);
				surfacedrives[i].bladeWashSpeed = 0.0f;
				break;
			case GDrive.TDriveType.rotor_basic:
				break;
			case GDrive.TDriveType.forward_rotor2:
			case GDrive.TDriveType.up_rotor2:
			case GDrive.TDriveType.right_rotor2:
				break;
			case GDrive.TDriveType.sin:
			case GDrive.TDriveType.cos:
				surfacedrives[i].drive_output = 0.0f;
				surfacedrives[i].bladeWashSpeed = 0.0f;
				break;
			}
			surfacedrives[i].drive_shaft += surfacedrives[i].drive_rpm / 60.0f * Time.fixedDeltaTime; if (surfacedrives[i].drive_shaft > 3600000.0f) surfacedrives[i].drive_shaft -= 3600000.0f;
			if (isCrashed) {
				surfacedrives[i].drive_rpm = 0.0f;
				surfacedrives[i].drive_shaft = 0.0f;
			}
			if (surfacedrives[i].shaftOutputPivotId.Length > 0) switch (surfacedrives[i].type) {
			case GDrive.TDriveType.sin:
				GPivot.setAnyPivot(surfacedrives[i].shaftOutputPivotId, Mathf.Sin(surfacedrives[i].drive_shaft));
				break;
			case GDrive.TDriveType.cos:
				GPivot.setAnyPivot(surfacedrives[i].shaftOutputPivotId, Mathf.Cos(surfacedrives[i].drive_shaft));
				break;
			default:
				GPivot.setAnyPivot(surfacedrives[i].shaftOutputPivotId, surfacedrives[i].drive_shaft);
				GPivot.setAnyPivot(surfacedrives[i].shaftOutputPivotId + ".rpm", surfacedrives[i].drive_rpm);
				break;
			}
			total_engine_force += surfacedrives[i].drive_output;
			if (isCrashed) {
				total_engine_force = 0.0f;
			}
			switch (surfacedrives[i].type) {
			case GDrive.TDriveType.rotor:
				prop_vnd = prop_v * Mathf.Cos(prop_a);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, surfacedrives[i].gameObject.transform.position + surfacedrives[i].gameObject.transform.forward * surfacedrives[i].bladeLength * 0.5f);				
				prop_vnd = prop_v * Mathf.Cos(prop_a_b);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, surfacedrives[i].gameObject.transform.position - surfacedrives[i].gameObject.transform.forward * surfacedrives[i].bladeLength * 0.5f);
				prop_vnd = prop_v * Mathf.Cos(prop_a_l);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, surfacedrives[i].gameObject.transform.position - surfacedrives[i].gameObject.transform.right * surfacedrives[i].bladeLength * 0.5f);
				prop_vnd = prop_v * Mathf.Cos(prop_a_r);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, surfacedrives[i].gameObject.transform.position + surfacedrives[i].gameObject.transform.right * surfacedrives[i].bladeLength * 0.5f);
			
				surfacedrives[i].rotorGyroscopicCoefficient = (surfacedrives[i].bladeMass * surfacedrives[i].bladeNumber) / ((surfacedrives[i].bladeMass * surfacedrives[i].bladeNumber) + gameObject.GetComponent<Rigidbody>().mass);
				tmp_q = Quaternion.FromToRotation(surfacedrives[i].gameObject.transform.up, surfacedrives[i].rotorGyroscopicLastUp * surfacedrives[i].rotorGyroscopicCoefficient + surfacedrives[i].gameObject.transform.up * (1.0f - surfacedrives[i].rotorGyroscopicCoefficient));
				gameObject.transform.rotation = gameObject.transform.rotation * tmp_q;
				surfacedrives[i].rotorGyroscopicLastUp = surfacedrives[i].gameObject.transform.up;
				break;
			case GDrive.TDriveType.tailrotor:
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(surfacedrives[i].gameObject.transform.right * surfacedrives[i].drive_output * globalSimulationScale, surfacedrives[i].gameObject.transform.position);
				break;
			case GDrive.TDriveType.rotor_basic:
				break;
			case GDrive.TDriveType.forward_rotor2:
			case GDrive.TDriveType.up_rotor2:
			case GDrive.TDriveType.right_rotor2:
				break;
			default:
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(surfacedrives[i].gameObject.transform.forward * surfacedrives[i].drive_output * globalSimulationScale, surfacedrives[i].gameObject.transform.position);
				break;
			}

			if (globalRenderForceVectors) {
				if (surfacedrives[i].lineRenderer != null) {
					surfacedrives[i].lineRenderer.material = tmp_lineRenderer_material;
					surfacedrives[i].lineRenderer.SetVertexCount(2);
					surfacedrives[i].lineRenderer.SetWidth(0.5f * globalSimulationScale, 0.0f * globalSimulationScale);
					surfacedrives[i].lineRenderer.SetPosition(0, surfacedrives[i].transform.position);
					surfacedrives[i].lineRenderer.SetPosition(1, surfacedrives[i].transform.position - surfacedrives[i].transform.forward * engine_length);
				}
			}

			surfacedrives[i].lastPosition = tmp_transform.position;
		}
		for (int i = 0; i < labeled_surfacedrives_count; ++i) {
			tmp_transform = labeled_surfacedrives[i].gameObject.transform;
			labeled_surfacedrives[i].lastPosition = labeled_surfacedrives[i].lastPosition * (1.0f - kineticsSurfaceDeltaFilter * globalSimulationScale) + tmp_transform.position * kineticsSurfaceDeltaFilter * globalSimulationScale;

			switch (kineticsSurfaceMethod) {
			case TSurfaceMethod.rigidbodyGetPointVelocity:
				tmp_v = gameObject.GetComponent<Rigidbody>().GetPointVelocity(tmp_transform.position) - neg_windspeed;
				if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			case TSurfaceMethod.rigidbodyGetPointVelocityWithPropwash:
				tmp_v = gameObject.GetComponent<Rigidbody>().GetPointVelocity(tmp_transform.position) - neg_windspeed;
				if (kineticsPropwash_internal > 0.0f) tmp_v += (kineticsPropwash_internal) * gameObject.transform.forward * tmp_propWashFactor;
				if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			case TSurfaceMethod.deltaFiltered:
				tmp_v = (tmp_transform.position - labeled_surfacedrives[i].lastPosition) / Time.fixedDeltaTime - neg_windspeed;
				if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			default: case TSurfaceMethod.deltaFilteredWithPropwash:
				tmp_v = (tmp_transform.position - labeled_surfacedrives[i].lastPosition) / Time.fixedDeltaTime - neg_windspeed;
				if (kineticsPropwash_internal > 0.0f) tmp_v += (kineticsPropwash_internal) * gameObject.transform.forward * tmp_propWashFactor;
				if ((kineticsGroundEffect == TGroundEffectType.airplanesoft) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
					ProcessLiftnDragOfElement_klift = kineticsGroundEffectCoeficient * kineticsGroundEffectCoeficientDistance / (tmp_transform.position.y - yPositionOfGround);
					if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
					floor_effect_add += ProcessLiftnDragOfElement_klift;
					floor_effect_ctr += 1.0f;
				} else if (((kineticsGroundEffect == TGroundEffectType.airplanesoft2) || (kineticsGroundEffect == TGroundEffectType.helicoptersoft)) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
					if (Mathf.Abs(kineticsGroundEffectCoeficient) < float.MinValue) kineticsGroundEffectCoeficient = float.MinValue;
					if (Mathf.Abs(kineticsGroundEffectCoeficientDistance) < float.MinValue) kineticsGroundEffectCoeficientDistance = float.MinValue;
					ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue * Mathf.Pow(kineticsGroundEffectMaxValue / kineticsGroundEffectCoeficient, -(tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
					if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
					if (ProcessLiftnDragOfElement_klift > kineticsGroundEffectMaxValue) ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue;
					floor_effect_add += ProcessLiftnDragOfElement_klift;
					floor_effect_ctr += 1.0f;
				} else if ((kineticsGroundEffect == TGroundEffectType.airplane) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= Mathf.Sqrt(tmp_v.x * tmp_v.x + tmp_v.z * tmp_v.z) * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				else if ((kineticsGroundEffect == TGroundEffectType.helicopter) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) tmp_v.y -= kineticsGroundEffectForce * kineticsGroundEffectMaxValue / Mathf.Pow(1.0f / kineticsGroundEffectCoeficient, (tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
				break;
			}
			if (GAircraftDropPointVelocity) tmp_v = Vector3.zero;
			else tmp_v /= globalSimulationScale;
			
			switch (labeled_surfacedrives[i].powerControlBy) {
			default: case GDrive.TDrivePowerControlBy.defaultvalue:
				switch (labeled_surfacedrives[i].type) {
				default: engine_throttle = inputThrottle_output; break;
				case GDrive.TDriveType.tailrotor: case GDrive.TDriveType.right_rotor2: engine_throttle = inputRudder_output; break;
				}
				break;
			case GDrive.TDrivePowerControlBy.throttle:
				engine_throttle = inputThrottle_output;
				break;
			case GDrive.TDrivePowerControlBy.elevator:
				engine_throttle = inputElevator_output;
				break;
			case GDrive.TDrivePowerControlBy.ailerons:
				engine_throttle = inputAilerons_output;
				break;
			case GDrive.TDrivePowerControlBy.rudder:
				engine_throttle = inputRudder_output;
				break;
			case GDrive.TDrivePowerControlBy.pivot:
				engine_throttle = GPivot.getAnyPivot(labeled_surfacedrives[i].powerControlByPivot);
				break;
			}
			switch (labeled_surfacedrives[i].powerCyclicForwardControlBy) {
			default: case GDrive.TDrivePowerControlBy.defaultvalue:
				switch (labeled_surfacedrives[i].type) {
				default: engine_forward_throttle = inputElevator_output; break;
				}
				break;
			case GDrive.TDrivePowerControlBy.throttle:
				engine_forward_throttle = inputThrottle_output;
				break;
			case GDrive.TDrivePowerControlBy.elevator:
				engine_forward_throttle = inputElevator_output;
				break;
			case GDrive.TDrivePowerControlBy.ailerons:
				engine_forward_throttle = inputAilerons_output;
				break;
			case GDrive.TDrivePowerControlBy.rudder:
				engine_forward_throttle = inputRudder_output;
				break;
			case GDrive.TDrivePowerControlBy.pivot:
				engine_forward_throttle = GPivot.getAnyPivot(labeled_surfacedrives[i].powerCyclicForwardControlByPivot);
				break;
			}
			switch (labeled_surfacedrives[i].powerCyclicSideControlBy) {
			default: case GDrive.TDrivePowerControlBy.defaultvalue:
				switch (labeled_surfacedrives[i].type) {
				default: engine_side_throttle = inputAilerons_output; break;
				}
				break;
			case GDrive.TDrivePowerControlBy.throttle:
				engine_side_throttle = inputThrottle_output;
				break;
			case GDrive.TDrivePowerControlBy.elevator:
				engine_side_throttle = inputElevator_output;
				break;
			case GDrive.TDrivePowerControlBy.ailerons:
				engine_side_throttle = inputAilerons_output;
				break;
			case GDrive.TDrivePowerControlBy.rudder:
				engine_side_throttle = inputRudder_output;
				break;
			case GDrive.TDrivePowerControlBy.pivot:
				engine_side_throttle = GPivot.getAnyPivot(labeled_surfacedrives[i].powerCyclicSideControlByPivot);
				break;
			}
			switch (labeled_surfacedrives[i].type) {
			default:
				if (engine_throttle <= 1.0f) {
					labeled_surfacedrives[i].drive_rpm = (1.0f - labeled_surfacedrives[i].throttleRpmConversionFilter) * labeled_surfacedrives[i].drive_rpm + labeled_surfacedrives[i].throttleRpmConversionFilter * (labeled_surfacedrives[i].throttleIdle + (labeled_surfacedrives[i].throttleMax - labeled_surfacedrives[i].throttleIdle) * engine_throttle) * labeled_surfacedrives[i].throttleRpmConversionRatio * kThrottle_ntr_multiplier;
					if (globalRenderForceVectors) if (labeled_surfacedrives[i].lineRenderer != null) labeled_surfacedrives[i].lineRenderer.SetColors(Color.blue, Color.red);
				} else {
					labeled_surfacedrives[i].drive_rpm = (1.0f - labeled_surfacedrives[i].throttleRpmConversionFilter) * labeled_surfacedrives[i].drive_rpm + labeled_surfacedrives[i].throttleRpmConversionFilter * (labeled_surfacedrives[i].throttleMax + (labeled_surfacedrives[i].throttleAfterburner - labeled_surfacedrives[i].throttleMax) * (engine_throttle - 1.0f)) * labeled_surfacedrives[i].throttleRpmConversionRatio * kThrottle_ntr_multiplier;
					if (globalRenderForceVectors) if (labeled_surfacedrives[i].lineRenderer != null) labeled_surfacedrives[i].lineRenderer.SetColors(Color.blue, Color.yellow);
				}
				//if (kThrottle_ntr_multiplier > 1.01f) labeled_surfacedrives[i].drive_rpm = labeled_surfacedrives[i].drive_rpm * kThrottle_ntr_multiplier;
				break;
			case GDrive.TDriveType.rotor:
			case GDrive.TDriveType.tailrotor:
				labeled_surfacedrives[i].drive_rpm = (1.0f - labeled_surfacedrives[i].throttleRpmConversionFilter) * labeled_surfacedrives[i].drive_rpm + labeled_surfacedrives[i].throttleRpmConversionFilter * labeled_surfacedrives[i].throttleMax * labeled_surfacedrives[i].throttleRpmConversionRatio;
				if (globalRenderForceVectors) if (labeled_surfacedrives[i].lineRenderer != null) labeled_surfacedrives[i].lineRenderer.SetColors(Color.blue, Color.yellow);
				break;
			case GDrive.TDriveType.rotor_basic:
				if ((simulationFirstFrames_complete) && (labeled_surfacedrives[i].powered)) {
					labeled_surfacedrives[i].poweredFactor = labeled_surfacedrives[i].poweredFactor * (1.0f - labeled_surfacedrives[i].poweredFactorFilter) + 1.0f * labeled_surfacedrives[i].poweredFactorFilter;
				} else {
					labeled_surfacedrives[i].poweredFactor = labeled_surfacedrives[i].poweredFactor * (1.0f - labeled_surfacedrives[i].poweredFactorFilter) + 0.0f * labeled_surfacedrives[i].poweredFactorFilter;
				}
				
				ProcessLiftnDragOfElement_klift = 1.0f;
				if ((kineticsGroundEffect == TGroundEffectType.helicoptersoft) && (distanceToGround < kineticsGroundEffectProbeMaxDistance)) {
					if (Mathf.Abs(kineticsGroundEffectCoeficient) < float.MinValue) kineticsGroundEffectCoeficient = float.MinValue;
					if (Mathf.Abs(kineticsGroundEffectCoeficientDistance) < float.MinValue) kineticsGroundEffectCoeficientDistance = float.MinValue;
					ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue * Mathf.Pow(kineticsGroundEffectMaxValue / kineticsGroundEffectCoeficient, -(tmp_transform.position.y - yPositionOfGround) / kineticsGroundEffectCoeficientDistance);
					if (ProcessLiftnDragOfElement_klift < 1.0f) ProcessLiftnDragOfElement_klift = 1.0f;
					if (ProcessLiftnDragOfElement_klift > kineticsGroundEffectMaxValue) ProcessLiftnDragOfElement_klift = kineticsGroundEffectMaxValue;
				}

				tmp_x = Vector3.Dot(tmp_v, labeled_surfacedrives[i].gameObject.transform.right);
				tmp_y = Vector3.Dot(tmp_v, labeled_surfacedrives[i].gameObject.transform.up);
				tmp_z = Vector3.Dot(tmp_v, labeled_surfacedrives[i].gameObject.transform.forward);
				tmp_xz = Mathf.Sqrt(tmp_x * tmp_x + tmp_z * tmp_z);
				
				float mainrotor_rpm_multiplier = 1.0f * labeled_surfacedrives[i].poweredFactor;
				float mainrotor_proyectedvelocity_magnitude2 = Mathf.Pow(labeled_surfacedrives[i].basicRotorVelocityFactor, -labeled_surfacedrives[i].basicRotorProjectedVelocityFactor / tmp_xz) * labeled_surfacedrives[i].basicRotorProjectedVelocityMultiplier;
				float mainrotor_angularvelocity_dump = (100.0f - mainrotor_proyectedvelocity_magnitude2) / (100.0f + 5.0f);
				float mainrotor_proyectedvelocity_magnitude_bias = mainrotor_proyectedvelocity_magnitude2 / 6.0f;
				float mainrotor_proyectedvelocity_magnitude_forward = (labeled_surfacedrives[i].basicRotorProjectedVelocityForwardOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;
				float mainrotor_proyectedvelocity_magnitude_rear = (labeled_surfacedrives[i].basicRotorProjectedVelocityRearOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;
				float mainrotor_proyectedvelocity_magnitude_left = (labeled_surfacedrives[i].basicRotorProjectedVelocityLeftOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;
				float mainrotor_proyectedvelocity_magnitude_right = (labeled_surfacedrives[i].basicRotorProjectedVelocityRightOffset + mainrotor_proyectedvelocity_magnitude_bias) * mainrotor_rpm_multiplier;

				float engine_force_dumper = -Mathf.PI * labeled_surfacedrives[i].dumperForceMultiplier * labeled_surfacedrives[i].bladeLength * labeled_surfacedrives[i].bladeLength * kdrag * tmp_y * Mathf.Abs(tmp_y) * mainrotor_rpm_multiplier;
				if (tmp_v.y < 0.0f) engine_force_dumper *= ProcessLiftnDragOfElement_klift;
				
				float rigidbodyMass = gameObject.GetComponent<Rigidbody>().mass;

				float throttle_offset_excess = labeled_surfacedrives[i].throttleBiasOffset;
				float throttle_multiplier_excess = labeled_surfacedrives[i].throttleBiasMultiplier;
				float throttle_rear_multiplier_excess = labeled_surfacedrives[i].throttleForwardRearOffset;
				float throttle_right_multiplier_excess = labeled_surfacedrives[i].throttleLeftRightOffset;
				float throttle_multiplier = rigidbodyMass * Physics.gravity.magnitude * throttle_multiplier_excess;
				float yaw_multiplier = labeled_surfacedrives[i].yawForceMultiplier;
				float up_projection = Vector3.Dot(Vector3.up, labeled_surfacedrives[i].gameObject.transform.up);
				float upforward_projection = Vector3.Dot(Vector3.up, labeled_surfacedrives[i].gameObject.transform.forward);
				float upright_projection = Vector3.Dot(Vector3.up, labeled_surfacedrives[i].gameObject.transform.right);
				
				float roll_vibration_value = labeled_surfacedrives[i].overspeedRollFactorVibration * vibration(labeled_surfacedrives[i].overspeedRollFactorVibratorId);
				float yaw_vibration_value = labeled_surfacedrives[i].overspeedYawFactorVibration * vibration(labeled_surfacedrives[i].overspeedYawFactorVibratorId);
				float pitch_vibration_value = labeled_surfacedrives[i].overspeedPitchFactorVibration * vibration(labeled_surfacedrives[i].overspeedPitchFactorVibratorId);
				
				if (labeled_surfacedrives[i].overspeedPitchFactorAtSpeed <= 0.1f) labeled_surfacedrives[i].overspeedPitchFactorAtSpeed = 0.1f;
				float throttle_overspeed_roll_factor_multiplier_value = rigidbodyMass * (labeled_surfacedrives[i].overspeedRollFactorMultiplier + roll_vibration_value) * up_projection * up_projection * up_projection * mainrotor_rpm_multiplier;
				if (labeled_surfacedrives[i].overspeedRollFactorAtSpeed <= 0.1f) labeled_surfacedrives[i].overspeedRollFactorAtSpeed = 0.1f;
				float throttle_overspeed_yaw_factor_multiplier_value = rigidbodyMass * (labeled_surfacedrives[i].overspeedYawFactorMultiplier + yaw_vibration_value) * mainrotor_rpm_multiplier;
				if (labeled_surfacedrives[i].overspeedYawFactorAtSpeed <= 0.1f) labeled_surfacedrives[i].overspeedYawFactorAtSpeed = 0.1f;
				float throttle_overspeed_pitch_factor_multiplier_value = rigidbodyMass * (labeled_surfacedrives[i].overspeedPitchFactorMultiplier + pitch_vibration_value);
				
				//overspeed_tmp_z_filtered = tmp_z;
				overspeed_tmp_z_filtered = overspeed_tmp_z_filtered * (1f - overspeed_tmp_z_filter) + tmp_z * overspeed_tmp_z_filter;
				if (GAircraftDropPointVelocity) overspeed_tmp_z_filtered = 0.0f;
				float pitch_overspeed_factor_value = Mathf.Max(labeled_surfacedrives[i].overspeedPitchFactorMin, Mathf.Min(labeled_surfacedrives[i].overspeedPitchFactorMax, Mathf.Abs(Mathf.Pow(labeled_surfacedrives[i].overspeedPitchFactorExponent, Mathf.Max(0.0f, Mathf.Abs(overspeed_tmp_z_filtered) + labeled_surfacedrives[i].overspeedPitchFactorOffsetSpeed) / labeled_surfacedrives[i].overspeedPitchFactorAtSpeed) - 1.0f))) + labeled_surfacedrives[i].overspeedPitchFactorOffsetValue;
				float yaw_overspeed_factor_value = Mathf.Max(labeled_surfacedrives[i].overspeedYawFactorMin, Mathf.Min(labeled_surfacedrives[i].overspeedYawFactorMax, Mathf.Abs(Mathf.Pow(labeled_surfacedrives[i].overspeedYawFactorExponent, Mathf.Max(0.0f, Mathf.Abs(overspeed_tmp_z_filtered) + labeled_surfacedrives[i].overspeedYawFactorOffsetSpeed) / labeled_surfacedrives[i].overspeedYawFactorAtSpeed) - 1.0f))) + labeled_surfacedrives[i].overspeedYawFactorOffsetValue;
				float roll_overspeed_factor_value = Mathf.Max(labeled_surfacedrives[i].overspeedRollFactorMin, Mathf.Min(labeled_surfacedrives[i].overspeedRollFactorMax, Mathf.Abs(Mathf.Pow(labeled_surfacedrives[i].overspeedRollFactorExponent, Mathf.Max(0.0f, Mathf.Abs(overspeed_tmp_z_filtered) + labeled_surfacedrives[i].overspeedRollFactorOffsetSpeed) / labeled_surfacedrives[i].overspeedRollFactorAtSpeed) - 1.0f))) + labeled_surfacedrives[i].overspeedRollFactorOffsetValue;

				float engine_force_yaw = (inputRudder_output - 0.5f) * yaw_multiplier + throttle_overspeed_yaw_factor_multiplier_value * yaw_overspeed_factor_value;
				float engine_force_common;
				if ((((tmp_y > 0.0f) && (inputThrottle_output + throttle_offset_excess > 0.0f)) || ((tmp_y < 0.0f) && (inputThrottle_output + throttle_offset_excess < 0.0f)))) {
					engine_force_common = (inputThrottle_output + throttle_offset_excess) * throttle_multiplier * Mathf.Min(0.005f, Mathf.Pow(labeled_surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / labeled_surfacedrives[i].climbReductionWithSpeedAtSpeed) * Mathf.Pow(labeled_surfacedrives[i].climbReductionWithVerticalSpeed, Mathf.Abs(tmp_y) / labeled_surfacedrives[i].climbReductionWithVerticalSpeedAtSpeed));
				} else {
					engine_force_common = (inputThrottle_output + throttle_offset_excess) * throttle_multiplier * Mathf.Min(0.005f, Mathf.Pow(labeled_surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / labeled_surfacedrives[i].climbReductionWithSpeedAtSpeed) * Mathf.Pow(labeled_surfacedrives[i].climbReductionWithVerticalSpeed, -Mathf.Abs(tmp_y) / labeled_surfacedrives[i].climbReductionWithVerticalSpeedAtSpeed));
				}
				float roll_speed = gameObject.transform.InverseTransformDirection(gameObject.GetComponent<Rigidbody>().angularVelocity).z;
				float engine_force_pitch = (engine_forward_throttle - 0.5f) * 20f * 55.5f * (20.0f * Mathf.Min(0.1f, Mathf.Pow(labeled_surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / labeled_surfacedrives[i].climbReductionWithSpeedAtSpeed))) + throttle_overspeed_pitch_factor_multiplier_value * pitch_overspeed_factor_value;
				engine_force_pitch -= upforward_projection * gameObject.GetComponent<Rigidbody>().mass * Mathf.Min(0.1f, labeled_surfacedrives[i].pitchReductionWithMassFactor);
				float engine_force_roll;
				engine_force_roll = upright_projection * rigidbodyMass * Mathf.Min(0.1f, labeled_surfacedrives[i].rollReductionWithMassFactor);
				engine_force_roll *= Mathf.Pow(labeled_surfacedrives[i].rollReductionWithSpeed, Mathf.Pow(Mathf.Abs(tmp_xz), labeled_surfacedrives[i].rollReductionWithSpeedSmooth) / Mathf.Pow(labeled_surfacedrives[i].rollReductionWithSpeedAtSpeed, labeled_surfacedrives[i].rollReductionWithSpeedSmooth));
				engine_force_roll += (engine_side_throttle - 0.5f) * 100f * 1.9f * (20.0f * Mathf.Min(0.1f, Mathf.Pow(labeled_surfacedrives[i].climbReductionWithSpeed, Mathf.Abs(tmp_xz) / labeled_surfacedrives[i].climbReductionWithSpeedAtSpeed))) + throttle_overspeed_roll_factor_multiplier_value * roll_overspeed_factor_value * Mathf.Sign(labeled_surfacedrives[i].overspeedRollFactor);
				if (roll_speed > 0.0f) {
					if (engine_force_roll < 0.0f) {
						//engine_force_roll = engine_force_roll * Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeed, Mathf.Abs(roll_speed) / labeled_surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
						//engine_force_roll = engine_force_roll + labeled_surfacedrives[i].rollReductionWithRollSpeedMultiplier * rigidbodyMass * Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeed, -Mathf.Abs(roll_speed) / labeled_surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
						engine_force_roll = engine_force_roll + rigidbodyMass * (Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeed + 1f, Mathf.Pow(Mathf.Abs(roll_speed), labeled_surfacedrives[i].rollReductionWithRollSpeedSmooth) / Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeedAtSpeed, labeled_surfacedrives[i].rollReductionWithRollSpeedSmooth)) - 1f);
					}
				} else {
					if (engine_force_roll > 0.0f) {
						//engine_force_roll = engine_force_roll * Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeed, Mathf.Abs(roll_speed) / labeled_surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
						//engine_force_roll = engine_force_roll - labeled_surfacedrives[i].rollReductionWithRollSpeedMultiplier * rigidbodyMass * Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeed, -Mathf.Abs(roll_speed) / labeled_surfacedrives[i].rollReductionWithRollSpeedAtSpeed);
						engine_force_roll = engine_force_roll - rigidbodyMass * (Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeed + 1f, Mathf.Pow(Mathf.Abs(roll_speed), labeled_surfacedrives[i].rollReductionWithRollSpeedSmooth) / Mathf.Pow(labeled_surfacedrives[i].rollReductionWithRollSpeedAtSpeed, labeled_surfacedrives[i].rollReductionWithRollSpeedSmooth)) - 1f);
					}
				}
				if (engine_force_roll > 0.1f * rigidbodyMass) engine_force_roll = 0.1f * rigidbodyMass;
				if (engine_force_roll < -0.1f * rigidbodyMass) engine_force_roll = -0.1f * rigidbodyMass;
				
				//rollReductionWithHorizontalSpeed
				//up_projection
				//		public float rollReductionWithMassFactor = 0.1f;
				//Debug.Log("tmp_v: " + tmp_v.ToString() + "; upright_projection: " + upright_projection.ToString() + "; roll_speed: " + roll_speed.ToString() + "; engine_force_common:" + engine_force_common.ToString() + "; engine_force_pitch:" + engine_force_pitch.ToString() + "; engine_force_roll:" + engine_force_roll.ToString() + "");

				float engine_force_common_filter = labeled_surfacedrives[i].commonFilter;
				float engine_force_pitch_filter = labeled_surfacedrives[i].pitchFilter;
				float engine_force_roll_filter = labeled_surfacedrives[i].rollFilter;
				float engine_force_yaw_filter = labeled_surfacedrives[i].yawFilter;
				float engine_force_dumper_filter = labeled_surfacedrives[i].dumperFilter;
				engine_force_common_filtered = engine_force_common_filtered * (1f - engine_force_common_filter) + engine_force_common * engine_force_common_filter;
				engine_force_pitch_filtered = engine_force_pitch_filtered * (1f - engine_force_pitch_filter) + engine_force_pitch * engine_force_pitch_filter;
				engine_force_roll_filtered = engine_force_roll_filtered * (1f - engine_force_roll_filter) + engine_force_roll * engine_force_roll_filter;
				engine_force_yaw_filtered = engine_force_yaw_filtered * (1f - engine_force_yaw_filter) + engine_force_yaw * engine_force_yaw_filter;
				engine_force_dumper_filtered = engine_force_dumper_filtered * (1f - engine_force_dumper_filter) + engine_force_dumper * engine_force_dumper_filter;
				if (!((engine_force_common_filtered >= 0f) || (engine_force_common_filtered < 0f))) engine_force_common_filtered = 0.0f;
				if (!((engine_force_pitch_filtered >= 0f) || (engine_force_pitch_filtered < 0f))) engine_force_pitch_filtered = 0.0f;
				if (!((engine_force_roll_filtered >= 0f) || (engine_force_roll_filtered < 0f))) engine_force_roll_filtered = 0.0f;
				if (!((engine_force_yaw_filtered >= 0f) || (engine_force_yaw_filtered < 0f))) engine_force_yaw_filtered = 0.0f;
				if (!((engine_force_dumper_filtered >= 0f) || (engine_force_dumper_filtered < 0f))) engine_force_dumper_filtered = 0.0f;
				
				float engine_force_fr_limit = 100.0f * rigidbodyMass;
				float engine_force_lr_limit = 150.0f * rigidbodyMass;
				
				float engine_force_front, engine_force_rear, engine_force_left, engine_force_right;
				if (throttle_rear_multiplier_excess > 0.0f) engine_force_front = engine_force_dumper_filtered + ((1f + throttle_rear_multiplier_excess) * engine_force_common_filtered + engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_forward;
				else engine_force_front = engine_force_dumper_filtered + (engine_force_common_filtered + engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_forward;
				if (throttle_rear_multiplier_excess < 0.0f) engine_force_rear = engine_force_dumper_filtered + ((1f - throttle_rear_multiplier_excess) * engine_force_common_filtered - engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_rear;
				else engine_force_rear = engine_force_dumper_filtered + (engine_force_common_filtered - engine_force_pitch_filtered) * mainrotor_proyectedvelocity_magnitude_rear;
				if (throttle_right_multiplier_excess < 0.0f) engine_force_left = engine_force_dumper_filtered + ((1f + throttle_right_multiplier_excess) * engine_force_common_filtered + engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_left;
				else engine_force_left = engine_force_dumper_filtered + (engine_force_common_filtered + engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_left;
				if (throttle_right_multiplier_excess < 0.0f) engine_force_right = engine_force_dumper_filtered + ((1f - throttle_right_multiplier_excess) * engine_force_common_filtered - engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_right;
				else engine_force_right = engine_force_dumper_filtered + (engine_force_common_filtered - engine_force_roll_filtered) * mainrotor_proyectedvelocity_magnitude_right;
				
				if (engine_force_front < -engine_force_fr_limit) engine_force_front = -engine_force_fr_limit;
				else if (engine_force_front > engine_force_fr_limit) engine_force_front = engine_force_fr_limit;
				if (engine_force_rear < -engine_force_fr_limit) engine_force_rear = -engine_force_fr_limit;
				else if (engine_force_rear > engine_force_fr_limit) engine_force_rear = engine_force_fr_limit;
				if (engine_force_left < -engine_force_lr_limit) engine_force_left = -engine_force_lr_limit;
				else if (engine_force_left > engine_force_lr_limit) engine_force_left = engine_force_lr_limit;
				if (engine_force_right < -engine_force_lr_limit) engine_force_right = -engine_force_lr_limit;
				else if (engine_force_right > engine_force_lr_limit) engine_force_right = engine_force_lr_limit;
				
				float virtual_density = Mathf.Pow(airdensity / labeled_surfacedrives[i].basicRotorAirdensityBias, labeled_surfacedrives[i].basicRotorAirdensityExp);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(virtual_density * labeled_surfacedrives[i].gameObject.transform.up * engine_force_front, labeled_surfacedrives[i].gameObject.transform.position + labeled_surfacedrives[i].gameObject.transform.forward * labeled_surfacedrives[i].basicRotorBladeForcePoint * labeled_surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(virtual_density * labeled_surfacedrives[i].gameObject.transform.up * engine_force_rear, labeled_surfacedrives[i].gameObject.transform.position - labeled_surfacedrives[i].gameObject.transform.forward *labeled_surfacedrives[i].basicRotorBladeForcePoint * labeled_surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(virtual_density * labeled_surfacedrives[i].gameObject.transform.up * engine_force_left, labeled_surfacedrives[i].gameObject.transform.position - labeled_surfacedrives[i].gameObject.transform.right * labeled_surfacedrives[i].basicRotorBladeForcePoint * labeled_surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(virtual_density * labeled_surfacedrives[i].gameObject.transform.up * engine_force_right, labeled_surfacedrives[i].gameObject.transform.position + labeled_surfacedrives[i].gameObject.transform.right * labeled_surfacedrives[i].basicRotorBladeForcePoint * labeled_surfacedrives[i].bladeLength);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(-virtual_density * labeled_surfacedrives[i].gameObject.transform.right * engine_force_yaw_filtered, labeled_surfacedrives[i].gameObject.transform.TransformPoint(labeled_surfacedrives[i].basicRotorTailOffset));
				gameObject.GetComponent<Rigidbody>().angularVelocity = gameObject.GetComponent<Rigidbody>().angularVelocity * mainrotor_angularvelocity_dump;
				if ((Mathf.Abs(yaw_overspeed_factor_value) > 0.1f) || (Mathf.Abs(pitch_overspeed_factor_value) > 0.1f)) {
					gameObject.GetComponent<Rigidbody>().velocity += gameObject.transform.right * yaw_overspeed_factor_value * yaw_vibration_value + gameObject.transform.up * pitch_overspeed_factor_value * pitch_vibration_value;
				}
				
				labeled_surfacedrives[i].drive_rpm = mainrotor_rpm_multiplier * labeled_surfacedrives[i].theoreticalTargetRpms * (1f + labeled_surfacedrives[i].basicRotorVariationMultiplier * tmp_y / (labeled_surfacedrives[i].overspeedYawFactorAtSpeed + labeled_surfacedrives[i].overspeedRollFactorAtSpeed + labeled_surfacedrives[i].overspeedPitchFactorAtSpeed));
				break;
			case GDrive.TDriveType.forward_rotor2:
			case GDrive.TDriveType.up_rotor2:
			case GDrive.TDriveType.right_rotor2:
				break;
			}
			switch (labeled_surfacedrives[i].type) {
			default: case GDrive.TDriveType.basic: labeled_surfacedrives[i].drive_output = labeled_surfacedrives[i].drive_rpm; break;
			case GDrive.TDriveType.propeller:
				prop_v = labeled_surfacedrives[i].drive_rpm * labeled_surfacedrives[i].bladeLength * 0.052359878f;
				prop_a = 1.570796327f - labeled_surfacedrives[i].bladeAngleYaw * 0.017453293f;
				prop_vn = prop_v * Mathf.Cos(prop_a);
				prop_vnd = Vector3.Dot(rigidbodyVelocity + neg_windspeed, gameObject.transform.forward);
				if (prop_vn > prop_vnd) prop_vnd = prop_vn - prop_vnd;
				else prop_vnd = prop_vn;
				prop_s = 0.5f * labeled_surfacedrives[i].bladeLength * (labeled_surfacedrives[i].bladeWidthMin + labeled_surfacedrives[i].bladeWidthMax);
				prop_o = Mathf.Cos(labeled_surfacedrives[i].bladeAnglePitch * 0.017453293f) * prop_s * labeled_surfacedrives[i].bladeShapeCoefficient * kdrag * labeled_surfacedrives[i].bladeNumber;
				labeled_surfacedrives[i].drive_output = prop_o * prop_vnd * prop_vnd;
				labeled_surfacedrives[i].bladeWashSpeed = prop_vnd;
				break;
			case GDrive.TDriveType.rotor:
				prop_v = labeled_surfacedrives[i].drive_rpm * labeled_surfacedrives[i].bladeLength * 0.052359878f;

				prop_a = 1.570796327f - labeled_surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + labeled_surfacedrives[i].rotorCollectiveBias) * labeled_surfacedrives[i].rotorCollectiveCoefficient + (engine_forward_throttle - 0.5f) * labeled_surfacedrives[i].rotorCyclicForwardCoefficient) * 0.017453293f;
				prop_a_b = 1.570796327f - labeled_surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + labeled_surfacedrives[i].rotorCollectiveBias) * labeled_surfacedrives[i].rotorCollectiveCoefficient - (engine_forward_throttle - 0.5f) * labeled_surfacedrives[i].rotorCyclicForwardCoefficient) * 0.017453293f;
				prop_a_l = 1.570796327f - labeled_surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + labeled_surfacedrives[i].rotorCollectiveBias) * labeled_surfacedrives[i].rotorCollectiveCoefficient + (engine_side_throttle - 0.5f) * labeled_surfacedrives[i].rotorCyclicSideCoefficient) * 0.017453293f;
				prop_a_r = 1.570796327f - labeled_surfacedrives[i].bladeAngleYaw * ((engine_throttle - 0.5f + labeled_surfacedrives[i].rotorCollectiveBias) * labeled_surfacedrives[i].rotorCollectiveCoefficient - (engine_side_throttle - 0.5f) * labeled_surfacedrives[i].rotorCyclicSideCoefficient) * 0.017453293f;
				prop_vn = prop_v * Mathf.Cos((prop_a + prop_a_l + prop_a_r + prop_a_b) / 4.0f);
				prop_vnd = prop_vn;
				prop_s = 0.5f * labeled_surfacedrives[i].bladeLength * (labeled_surfacedrives[i].bladeWidthMin + labeled_surfacedrives[i].bladeWidthMax);
				prop_o = Mathf.Cos(labeled_surfacedrives[i].bladeAnglePitch * 0.017453293f) * prop_s * labeled_surfacedrives[i].bladeShapeCoefficient * kdrag * labeled_surfacedrives[i].bladeNumber;
				labeled_surfacedrives[i].drive_output = prop_o * prop_vnd * Mathf.Abs(prop_vnd);
				kineticsGroundEffectForce += prop_vn / (labeled_surfacedrives[i].gameObject.transform.position.y - yPositionOfGround);
				labeled_surfacedrives[i].drive_rpm = (1.0f - labeled_surfacedrives[i].throttleRpmConversionFilter) * labeled_surfacedrives[i].drive_rpm + labeled_surfacedrives[i].throttleRpmConversionFilter * (labeled_surfacedrives[i].throttleMax * labeled_surfacedrives[i].throttleRpmConversionRatio - labeled_surfacedrives[i].rotorAutorotationCoefficient * labeled_surfacedrives[i].drive_output * Mathf.Cos((prop_a + prop_a_l + prop_a_r + prop_a_b) / 4.0f));
				labeled_surfacedrives[i].bladeWashSpeed = 0.0f;
				break;
			case GDrive.TDriveType.tailrotor:
				prop_v = labeled_surfacedrives[i].drive_rpm * labeled_surfacedrives[i].bladeLength * 0.052359878f;
				prop_a = 1.570796327f - labeled_surfacedrives[i].bladeAngleYaw * (0.5f - engine_throttle) * 0.017453293f;
				prop_vn = prop_v * Mathf.Cos(prop_a);
				prop_vnd = prop_vn;
				prop_s = 0.5f * labeled_surfacedrives[i].bladeLength * (labeled_surfacedrives[i].bladeWidthMin + labeled_surfacedrives[i].bladeWidthMax);
				prop_o = Mathf.Cos(labeled_surfacedrives[i].bladeAnglePitch * 0.017453293f) * prop_s * labeled_surfacedrives[i].bladeShapeCoefficient * kdrag * labeled_surfacedrives[i].bladeNumber;
				labeled_surfacedrives[i].drive_output = prop_o * prop_vnd * Mathf.Abs(prop_vnd);
				labeled_surfacedrives[i].bladeWashSpeed = 0.0f;
				break;
			case GDrive.TDriveType.rotor_basic:
				break;
			case GDrive.TDriveType.forward_rotor2:
			case GDrive.TDriveType.up_rotor2:
			case GDrive.TDriveType.right_rotor2:
				break;
			case GDrive.TDriveType.sin:
			case GDrive.TDriveType.cos:
				labeled_surfacedrives[i].drive_output = 0.0f;
				labeled_surfacedrives[i].bladeWashSpeed = 0.0f;
				break;
			}
			labeled_surfacedrives[i].drive_shaft += labeled_surfacedrives[i].drive_rpm / 60.0f * Time.fixedDeltaTime; if (labeled_surfacedrives[i].drive_shaft > 3600000.0f) labeled_surfacedrives[i].drive_shaft -= 3600000.0f;
			if (isCrashed) {
				labeled_surfacedrives[i].drive_rpm = 0.0f;
				labeled_surfacedrives[i].drive_shaft = 0.0f;
			}
			if (labeled_surfacedrives[i].shaftOutputPivotId.Length > 0) switch (labeled_surfacedrives[i].type) {
			case GDrive.TDriveType.sin:
				GPivot.setAnyPivot(labeled_surfacedrives[i].shaftOutputPivotId, Mathf.Sin(labeled_surfacedrives[i].drive_shaft));
				break;
			case GDrive.TDriveType.cos:
				GPivot.setAnyPivot(labeled_surfacedrives[i].shaftOutputPivotId, Mathf.Cos(labeled_surfacedrives[i].drive_shaft));
				break;
			default:
				GPivot.setAnyPivot(labeled_surfacedrives[i].shaftOutputPivotId, labeled_surfacedrives[i].drive_shaft);
				GPivot.setAnyPivot(labeled_surfacedrives[i].shaftOutputPivotId + ".rpm", labeled_surfacedrives[i].drive_rpm);
				break;
			}
			total_engine_force += labeled_surfacedrives[i].drive_output;
			if (isCrashed) {
				total_engine_force = 0.0f;
			}
			switch (labeled_surfacedrives[i].type) {
			case GDrive.TDriveType.rotor:
				prop_vnd = prop_v * Mathf.Cos(prop_a);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(labeled_surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, labeled_surfacedrives[i].gameObject.transform.position + labeled_surfacedrives[i].gameObject.transform.forward * labeled_surfacedrives[i].bladeLength * 0.5f);				
				prop_vnd = prop_v * Mathf.Cos(prop_a_b);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(labeled_surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, labeled_surfacedrives[i].gameObject.transform.position - labeled_surfacedrives[i].gameObject.transform.forward * labeled_surfacedrives[i].bladeLength * 0.5f);
				prop_vnd = prop_v * Mathf.Cos(prop_a_l);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(labeled_surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, labeled_surfacedrives[i].gameObject.transform.position - labeled_surfacedrives[i].gameObject.transform.right * labeled_surfacedrives[i].bladeLength * 0.5f);
				prop_vnd = prop_v * Mathf.Cos(prop_a_r);
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(labeled_surfacedrives[i].gameObject.transform.up * (0.25f * prop_o * prop_vnd * Mathf.Abs(prop_vnd)) * globalSimulationScale, labeled_surfacedrives[i].gameObject.transform.position + labeled_surfacedrives[i].gameObject.transform.right * labeled_surfacedrives[i].bladeLength * 0.5f);
			
				labeled_surfacedrives[i].rotorGyroscopicCoefficient = (labeled_surfacedrives[i].bladeMass * labeled_surfacedrives[i].bladeNumber) / ((labeled_surfacedrives[i].bladeMass * labeled_surfacedrives[i].bladeNumber) + gameObject.GetComponent<Rigidbody>().mass);
				tmp_q = Quaternion.FromToRotation(labeled_surfacedrives[i].gameObject.transform.up, labeled_surfacedrives[i].rotorGyroscopicLastUp * labeled_surfacedrives[i].rotorGyroscopicCoefficient + labeled_surfacedrives[i].gameObject.transform.up * (1.0f - labeled_surfacedrives[i].rotorGyroscopicCoefficient));
				gameObject.transform.rotation = gameObject.transform.rotation * tmp_q;
				labeled_surfacedrives[i].rotorGyroscopicLastUp = labeled_surfacedrives[i].gameObject.transform.up;
				break;
			case GDrive.TDriveType.tailrotor:
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(labeled_surfacedrives[i].gameObject.transform.right * labeled_surfacedrives[i].drive_output * globalSimulationScale, labeled_surfacedrives[i].gameObject.transform.position);
				break;
			case GDrive.TDriveType.rotor_basic:
				break;
			case GDrive.TDriveType.forward_rotor2:
			case GDrive.TDriveType.up_rotor2:
			case GDrive.TDriveType.right_rotor2:
				break;
			default:
				if (simulationForcesActive) gameObject.GetComponent<Rigidbody>().AddForceAtPosition(labeled_surfacedrives[i].gameObject.transform.forward * labeled_surfacedrives[i].drive_output * globalSimulationScale, labeled_surfacedrives[i].gameObject.transform.position);
				break;
			}

			if (globalRenderForceVectors) {
				if (labeled_surfacedrives[i].lineRenderer != null) {
					labeled_surfacedrives[i].lineRenderer.material = tmp_lineRenderer_material;
					labeled_surfacedrives[i].lineRenderer.SetVertexCount(2);
					labeled_surfacedrives[i].lineRenderer.SetWidth(0.5f * globalSimulationScale, 0.0f * globalSimulationScale);
					labeled_surfacedrives[i].lineRenderer.SetPosition(0, labeled_surfacedrives[i].gameObject.transform.position);
					labeled_surfacedrives[i].lineRenderer.SetPosition(1, labeled_surfacedrives[i].gameObject.transform.position - labeled_surfacedrives[i].gameObject.transform.forward * engine_length);
				}
			}
			
			labeled_surfacedrives[i].lastPosition = tmp_transform.position;
		}
		for (int i = 0; i < labeled_surfacemisc_count; ++i) {
			engine_throttle = inputThrottle_output;
			engine_renderer = labeled_surfacemisc[i].gameObject.GetComponent<Renderer>();
			if (labeled_surfacemisc[i].lospeed) {
				if (engine_renderer != null) {
					if (GPivot.getAnyPivot(labeled_surfacemisc[i].pivotId + ".rpm") >= labeled_surfacemisc[i].rpmThreshold) engine_renderer.enabled = false;
					else engine_renderer.enabled = true;
				}
			} else if (labeled_surfacemisc[i].hispeed) {
				if (engine_renderer != null) {
					if (GPivot.getAnyPivot(labeled_surfacemisc[i].pivotId + ".rpm") >= labeled_surfacemisc[i].rpmThreshold) engine_renderer.enabled = true;
					else engine_renderer.enabled = false;
				}
			}
		}
	}
	
	public bool addTrails(Vector3 delta) {
		addTrails_value += delta;
		return true;
	}
	void ProcessTrails() {
		bool enable_emission = false;
		bool emit_new_point = false;
		if ((--surfacetrails_interval_count) <= 0) {
			surfacetrails_interval_count = surfacetrails_interval_maxcount;
			emit_new_point = true;
		}
		neg_windspeed = GWindBasic.windAt(gameObject.transform.position) * Time.fixedDeltaTime;
		
		int k, c, ic;
		
		for (int i = 0; i < surfacetrails_count; ++i) {
			tmp_transform = surfacetrails[i].gameObject.transform;

			if (emit_new_point) {
				++surfacetrails[i].linePoint; if (surfacetrails[i].linePoint >= surfacetrails_points) surfacetrails[i].linePoint = 0;
			}
			switch(surfacetrails[i].mode) {
			case GTrail.TTrailMode.standard:
				enable_emission = (inputTrails_output > 0.1f) && ((gameObject.transform.position.y + height) < surfacetrails[i].heightThreshold);
				break;
			case GTrail.TTrailMode.throttle:
				enable_emission = (inputThrottle_output > 0.1f) && ((gameObject.transform.position.y + height) < surfacetrails[i].heightThreshold);
				break;
			}
			if (!surfacetrails[i].trailEnabled) enable_emission = false;
			if (enable_emission) {
				surfacetrails[i].linePoints[surfacetrails[i].linePoint] = tmp_transform.position;
				surfacetrails[i].linePointsEnabled[surfacetrails[i].linePoint] = true;
			} else {
				surfacetrails[i].linePoints[surfacetrails[i].linePoint] = Vector3.zero;
				surfacetrails[i].linePointsEnabled[surfacetrails[i].linePoint] = false;
			}

			tmp_lineRenderer = surfacetrails[i].lineRenderer;

			c = 0;
			k = surfacetrails[i].linePoint;
			ic = k - 1;
			
			for (int j = 0; j < surfacetrails_points; ++j) {
				if (!surfacetrails[i].linePointsEnabled[k]) {
					if (c > 0) break;
					ic = k - 1;
				} else {
					++c;
					surfacetrails[i].linePoints[k] += neg_windspeed + ((k != surfacetrails[i].linePoint) ? addTrails_value : Vector3.zero);
					
					if (j > 0) {
						tmp_v1 = Vector3.zero;
						for (int s = 0; s < labeled_surfacedrives_count; ++s) {
							if (labeled_surfacedrives[s].bladeWashEnabled) {
								tmp_v = surfacetrails[i].linePoints[k] - labeled_surfacedrives[s].gameObject.transform.position;
								if (tmp_v.magnitude < labeled_surfacedrives[s].bladeWashRadiusTangent * globalSimulationScale) {
									kineticsPropwash_internal = labeled_surfacedrives[s].bladeWashSpeed * globalSimulationScale;
									surfacetrails[i].linePoints[k] -= (kineticsPropwash_internal * Time.fixedDeltaTime * (labeled_surfacedrives[s].bladeWashRadiusTangent - tmp_v.magnitude) * (labeled_surfacedrives[s].bladeWashRadiusTangent - tmp_v.magnitude) / labeled_surfacedrives[s].bladeWashRadiusTangent / labeled_surfacedrives[s].bladeWashRadiusTangent) * gameObject.transform.forward;
								}
							}
						}
						surfacetrails[i].linePoints[k] -= tmp_v1 * Time.fixedDeltaTime;
					}

				}
				--k; if (k < 0) k = surfacetrails_points - 1;
			}
			if (c >= 2) {
				if (tmp_lineRenderer != null) tmp_lineRenderer.SetVertexCount(c - 1);
				k = ic; if (k < 0) k = surfacetrails_points - 1;
				for (int j = 0; j < c - 1; ++j) {
					if (tmp_lineRenderer != null) tmp_lineRenderer.SetPosition(j, surfacetrails[i].linePoints[k]);
					--k; if (k < 0) k = surfacetrails_points - 1;
				}
			} else {
				if (tmp_lineRenderer != null) tmp_lineRenderer.SetVertexCount(0);
			}
		}

		for (int i = 0; i < labeled_surfacetrails_count; ++i) {
			tmp_transform = labeled_surfacetrails[i].gameObject.transform;

			if (emit_new_point) {
				++labeled_surfacetrails[i].linePoint; if (labeled_surfacetrails[i].linePoint >= surfacetrails_points) labeled_surfacetrails[i].linePoint = 0;
			}
			switch(labeled_surfacetrails[i].mode) {
			case GTrail.TTrailMode.standard:
				enable_emission = (inputTrails_output > 0.1f) && ((gameObject.transform.position.y + height) < labeled_surfacetrails[i].heightThreshold);
				break;
			case GTrail.TTrailMode.throttle:
				enable_emission = (inputThrottle_output > 0.1f) && ((gameObject.transform.position.y + height) < labeled_surfacetrails[i].heightThreshold);
				break;
			}
			if (enable_emission) {
				labeled_surfacetrails[i].linePoints[labeled_surfacetrails[i].linePoint] = tmp_transform.position;
				labeled_surfacetrails[i].linePointsEnabled[labeled_surfacetrails[i].linePoint] = true;
			} else {
				labeled_surfacetrails[i].linePoints[labeled_surfacetrails[i].linePoint] = Vector3.zero;
				labeled_surfacetrails[i].linePointsEnabled[labeled_surfacetrails[i].linePoint] = false;
			}

			tmp_lineRenderer = labeled_surfacetrails[i].lineRenderer;

			c = 0;
			k = labeled_surfacetrails[i].linePoint;
			ic = k - 1;
			for (int j = 0; j < surfacetrails_points; ++j) {
				if (!labeled_surfacetrails[i].linePointsEnabled[k]) {
					if (c > 0) break;
					ic = k - 1;
				} else {
					++c;
					labeled_surfacetrails[i].linePoints[k] += neg_windspeed + ((k != labeled_surfacetrails[i].linePoint) ? addTrails_value : Vector3.zero);

					if (j > 0) {
						tmp_v1 = Vector3.zero;
						for (int s = 0; s < labeled_surfacedrives_count; ++s) {
							if (labeled_surfacedrives[s].bladeWashEnabled) {
								tmp_v = labeled_surfacetrails[i].linePoints[k] - labeled_surfacedrives[s].gameObject.transform.position;
								if (tmp_v.magnitude < labeled_surfacedrives[s].bladeWashRadiusTangent * globalSimulationScale) {
									kineticsPropwash_internal = labeled_surfacedrives[s].bladeWashSpeed * globalSimulationScale;
									labeled_surfacetrails[i].linePoints[k] -= (kineticsPropwash_internal * Time.fixedDeltaTime * (labeled_surfacedrives[s].bladeWashRadiusTangent - tmp_v.magnitude) * (labeled_surfacedrives[s].bladeWashRadiusTangent - tmp_v.magnitude) / labeled_surfacedrives[s].bladeWashRadiusTangent / labeled_surfacedrives[s].bladeWashRadiusTangent) * gameObject.transform.forward;
								}
							}
						}
						labeled_surfacetrails[i].linePoints[k] -= tmp_v1 * Time.fixedDeltaTime;
					}
					
				}
				--k; if (k < 0) k = surfacetrails_points - 1;
			}
			if (c >= 2) {
				if (tmp_lineRenderer != null) tmp_lineRenderer.SetVertexCount(c - 1);
				k = ic; if (k < 0) k = surfacetrails_points - 1;
				for (int j = 0; j < c - 1; ++j) {
					if (tmp_lineRenderer != null) tmp_lineRenderer.SetPosition(j, labeled_surfacetrails[i].linePoints[k]);
					--k; if (k < 0) k = surfacetrails_points - 1;
				}
			} else {
				if (tmp_lineRenderer != null) tmp_lineRenderer.SetVertexCount(0);
			}
		}
		addTrails_value = Vector3.zero;
	}

	void Start() {
		if (gameObject.GetComponent<Rigidbody>() == null) {
			Debug.LogError("GAircraft needs to be attached to a RigidBody!");
		} else {
			if (GAircraft.singleton == null) GAircraft.singleton = this;
			if ((GameObject.Find(inputThrottleNitroSoundGameObjectName) != null) && (GameObject.Find(inputThrottleNitroSoundGameObjectName).GetComponent("AudioSource") != null)) {
				inputThrottleNitroSoundAudioSource = (AudioSource)GameObject.Find(inputThrottleNitroSoundGameObjectName).GetComponent("AudioSource");
			}
			tmp_lineRenderer_material = new Material(Shader.Find("Particles/Additive"));
			center = gameObject.GetComponent<Rigidbody>().centerOfMass;
			speed_lastPosition = gameObject.transform.position;
			body_min_x = 999999999.0f;
			body_max_x = -999999999.0f;
			body_min_y = 999999999.0f;
			body_max_y = -999999999.0f;
			body_min_z = 999999999.0f;
			body_max_z = -999999999.0f;
			log("GAircraft: looking for the airplane parts...");
			FindNodes(gameObject, 0);
			SortCameras();
			if ((surfaces_count + labeled_surfaces_count) < 1) {
				body_min_x = 0.0f;
				body_max_x = 0.0f;
				body_min_y = 0.0f;
				body_max_y = 0.0f;
				body_min_z = 0.0f;
				body_max_z = 0.0f;
			}
			if (Mathf.Abs(body_max_x - body_min_x) < 0.0001f) body_max_x = body_min_x + 0.0001f;
			if (Mathf.Abs(body_max_y - body_min_y) < 0.0001f) body_max_y = body_min_y + 0.0001f;
			if (Mathf.Abs(body_max_z - body_min_z) < 0.0001f) body_max_z = body_min_z + 0.0001f;
			body_center.Set((body_min_x + body_max_x) / 2.0f, (body_min_x + body_max_x) / 2.0f, (body_min_x + body_max_x) / 2.0f);
			body_radius = 0.0f;
			if (((body_max_x - body_min_x) / 2.0f) > body_radius) body_radius = (body_max_x - body_min_x) / 2.0f;
			if (((body_max_y - body_min_y) / 2.0f) > body_radius) body_radius = (body_max_y - body_min_y) / 2.0f;
			if (((body_max_z - body_min_z) / 2.0f) > body_radius) body_radius = (body_max_z - body_min_z) / 2.0f;
			FindNodes2ndPass(gameObject, 0);
			log("GAircraft: found " + (surfaces_count + labeled_surfaces_count).ToString() + " surfaces, " + (surfacepivots_count + labeled_surfacepivots_count).ToString() + " pivots and " + (surfacedrives_count + labeled_surfacedrives_count).ToString() + " drives" + ".");
			StartKinetics();
		}
	}
	
	bool checkPausedStatus() {
		if (wasPaused) {
			if ((!GAircraft.isPhysicsPaused) && (this.GetComponent<Rigidbody>() != null)) {
				this.GetComponent<Rigidbody>().isKinematic = false;
				wasPaused = false;
			}
		} else {
			if ((GAircraft.isPhysicsPaused) && (this.GetComponent<Rigidbody>() != null)) {
				this.GetComponent<Rigidbody>().isKinematic = true;
				wasPaused = true;
				ignoreCrashUntil = Time.realtimeSinceStartup;
			}
			if ((GAircraft.isPhysicsPaused) && (this.GetComponent<Rigidbody>() != null)) this.GetComponent<Rigidbody>().isKinematic = true;
		}
		if (wasPaused) return true;
		if (GAircraft.isSimulationPaused) ignoreCrashUntil = Time.realtimeSinceStartup;
		if (GAircraft.isPhysicsPaused) ignoreCrashUntil = Time.realtimeSinceStartup;
		return GAircraft.isSimulationPaused;
	}
	
	void Update() {
		if (checkPausedStatus()) return;
		ProcessLiftnDrag_in_update();
	}
	
	LabeledSurfaceDriveDesc iLabeledSurfaceDriveDesc;
	GDrive iGDrive;
	void FixedUpdate() {
		if (checkPausedStatus()) {
			if (inputThrottleNitroSoundAudioSource != null) inputThrottleNitroSoundAudioSource.volume = 0.0f;
			return;
		} else {
			if (inputThrottleNitroSoundAudioSource != null) inputThrottleNitroSoundAudioSource.volume = kThrottle_ntr_multiplier - 1.0f;
		}

		if (simulationFirstFrames > 0) {
			--simulationFirstFrames;
			GAircraftDropPointVelocity = true;
		} else {
			simulationFirstFrames_complete = true;
		}
			
		if (gameObject.GetComponent<Rigidbody>() != null) {
			if (GAircraftDropPointVelocity) rigidbodyVelocity = Vector3.zero; else rigidbodyVelocity = gameObject.GetComponent<Rigidbody>().velocity;
			if (GAircraftDropPointVelocity) rigidbodyVelocity_lastValue = Vector3.zero;
			simulationForcesActive = (simulationFirstFrames_complete && (!(isCrashed && (crashType == TCrashType.helicopter))));
			ProcessKinetics();
			ProcessInput();
			ProcessDrives();
			ProcessLiftnDrag();
			ProcessPivots();
			ProcessTrails();
			checkBreakOnFixedUpdate();
			GAircraftDropPointVelocity = false;
			rigidbodyVelocity_lastValue = rigidbodyVelocity;
		}
	}
	
	float vibrationLastTime = 0.0f;
	public Vector3 vibrationGet() {
		float time = Time.realtimeSinceStartup;
		if (time > vibrationLastTime + Time.fixedDeltaTime) {
			if (vibrationTime > 0.0f) {
				vibrationTime -= (time - vibrationLastTime);
				vibrationAmplitude = vibrationAmplitudePeak * (vibrationTime / vibrationDuration) * Mathf.Cos(vibrationTime / vibrationDuration * vibrationFrequency);
			} else {
				vibrationAmplitude = 0.0f;
			}
			vibrationLastTime = time;
		}
		return vibrationAmplitude * vibrationDirection;
	}
	
	public bool vibrationSet(Vector3 vibrationDirection, float vibrationAmplitude, float vibrationDuration, float vibrationFrequency) {
		this.vibrationDirection = vibrationDirection;
		this.vibrationAmplitudePeak = this.vibrationAmplitude = vibrationAmplitude;
		this.vibrationDuration = this.vibrationTime = vibrationDuration;
		this.vibrationFrequency = vibrationFrequency;
		return true;
	}
	
	public bool placeAt(Vector3 position, Vector3 eulerAngles, bool applySpeed, float newSpeed, bool applyThrottle, float startThrottle) {
		gameObject.transform.position = position;
		GAircraft.GAircraftDropPointVelocity = true;
		gameObject.transform.rotation = Quaternion.Euler(eulerAngles);
		if (applySpeed) {
			if (gameObject.GetComponent<Rigidbody>().isKinematic) gameObject.GetComponent<Rigidbody>().isKinematic = false;
			gameObject.GetComponent<Rigidbody>().velocity = gameObject.transform.forward * newSpeed;
			gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
		}
		if (applyThrottle) {
			SetDrives(inputThrottle_internal = inputThrottle_internal2 = startThrottle);
		}
		return true;
	}
	public bool placeAt(GameObject reference, bool applySpeed, float newSpeed, bool applyThrottle, float startThrottle) {
		return placeAt(reference.transform.position, reference.transform.eulerAngles, applySpeed, newSpeed, applyThrottle, startThrottle);
	}

	public bool placeAt(Vector3 position, Vector3 eulerAngles, bool applySpeed, float newSpeed) {
		return placeAt(position, eulerAngles, applySpeed, newSpeed, false, 0.0f);
	}
	public bool placeAt(GameObject reference, bool applySpeed, float newSpeed) {
		return placeAt(reference.transform.position, reference.transform.eulerAngles, applySpeed, newSpeed, false, 0.0f);
	}

	public bool placeAt(Vector3 position, Vector3 eulerAngles, bool applySpeed, float newSpeed, bool applyThrottle) {
		return placeAt(position, eulerAngles, applySpeed, newSpeed, applyThrottle, 0.0f);
	}
	public bool placeAt(GameObject reference, bool applySpeed, float newSpeed, bool applyThrottle) {
		return placeAt(reference.transform.position, reference.transform.eulerAngles, applySpeed, newSpeed, applyThrottle, 0.0f);
	}

	public bool placeAt(Vector3 position, Vector3 eulerAngles, bool applySpeed, bool applyThrottle, float startThrottle) {
		return placeAt(position, eulerAngles, applySpeed, 0.0f, applyThrottle, startThrottle);
	}
	public bool placeAt(GameObject reference, bool applySpeed, bool applyThrottle, float startThrottle) {
		return placeAt(reference.transform.position, reference.transform.eulerAngles, applySpeed, 0.0f, applyThrottle, startThrottle);
	}

	public bool placeAt(Vector3 position, Vector3 eulerAngles, float newSpeed, float startThrottle) {
		if ((startThrottle >= 0.0f) && (startThrottle <= 1.0f)) {
			if (Mathf.Abs(newSpeed) > 0.01f) return placeAt(position, eulerAngles, true, newSpeed, true, startThrottle);
			else return placeAt(position, eulerAngles, false, newSpeed, true, startThrottle);
		} else {
			if (Mathf.Abs(newSpeed) > 0.01f) return placeAt(position, eulerAngles, true, newSpeed, false, 0.0f);
			else return placeAt(position, eulerAngles, false, newSpeed, false, 0.0f);
		}
	}
	public bool placeAt(GameObject reference, float newSpeed, float startThrottle) {
		if ((startThrottle >= 0.0f) && (startThrottle <= 1.0f)) {
			if (Mathf.Abs(newSpeed) > 0.01f) return placeAt(reference.transform.position, reference.transform.eulerAngles, true, newSpeed, true, startThrottle);
			else return placeAt(reference.transform.position, reference.transform.eulerAngles, false, newSpeed, true, startThrottle);
		} else {
			if (Mathf.Abs(newSpeed) > 0.01f) return placeAt(reference.transform.position, reference.transform.eulerAngles, true, newSpeed, false, 0.0f);
			else return placeAt(reference.transform.position, reference.transform.eulerAngles, false, newSpeed, false, 0.0f);
		}
	}

	public bool placeAt(Vector3 position, Vector3 eulerAngles, float newSpeed) {
		if (Mathf.Abs(newSpeed) > 0.01f) return placeAt(position, eulerAngles, true, newSpeed, false, 0.0f);
		else return placeAt(position, eulerAngles, false, newSpeed, false, 0.0f);
	}
	public bool placeAt(GameObject reference, float newSpeed) {
		if (Mathf.Abs(newSpeed) > 0.01f) return placeAt(reference.transform.position, reference.transform.eulerAngles, true, newSpeed, false, 0.0f);
		else return placeAt(reference.transform.position, reference.transform.eulerAngles, false, newSpeed, false, 0.0f);
	}

	public bool placeAt(Vector3 position, Vector3 eulerAngles) {
		return placeAt(position, eulerAngles, false, 0.0f, false, 0.0f);
	}
	public bool placeAt(GameObject reference) {
		return placeAt(reference.transform.position, reference.transform.eulerAngles, false, 0.0f, false, 0.0f);
	}

	public static bool isSimulationPaused = false;
	private bool wasPaused = false;
	public static bool isPhysicsPaused = false;
	public int simulationFirstFrames = 100;
	private bool simulationFirstFrames_complete = false;
	private bool simulationForcesActive = false;
	[HideInInspector]public float ignoreCrashUntil = 0.0f;
	[HideInInspector]public bool isCrashed = false;
	public TCrashType crashType = TCrashType.airplane;
	public float crashAccelerationFlippedThreshold = 0.02f;
	public float crashAcceleration1Threshold = 9.5f;
	public float crashAcceleration1HoldTime = 1.0f;
	public float crashAcceleration2Threshold = 40.0f;
	public float crashAcceleration2HoldTime = 0.1f;

	private float crashAcceleration1HoldTime_count = 0;
	private float crashAcceleration2HoldTime_count = 0;
	void checkBreakOnFixedUpdate() {
		if ((crashType != TCrashType.none) && (Time.realtimeSinceStartup > ignoreCrashUntil)) {
			float accelerationMagnitude = (rigidbodyVelocity - rigidbodyVelocity_lastValue).magnitude;
			if (!isCrashed) {
				crashAcceleration1HoldTime_count += Time.fixedTime;
				crashAcceleration2HoldTime_count += Time.fixedTime;
			} else {
				crashAcceleration1HoldTime_count = 0.0f;
				crashAcceleration2HoldTime_count = 0.0f;
			}
			if (accelerationMagnitude > crashAcceleration1Threshold) {
				if (crashAcceleration1HoldTime_count > crashAcceleration1HoldTime) isCrashed = true;
			} else {
				crashAcceleration1HoldTime_count = 0.0f;
			}
			if (accelerationMagnitude > crashAcceleration2Threshold) {
				if (crashAcceleration2HoldTime_count > crashAcceleration2HoldTime) isCrashed = true;
			} else {
				crashAcceleration2HoldTime_count = 0.0f;
			}
			if (accelerationMagnitude < crashAccelerationFlippedThreshold) {
				if (transform.up.y < 0.0f) {
					isCrashed = true;
				}
			}
		}
	}
	public void ignore_check_simulation_broken() {
		ignoreCrashUntil = Time.realtimeSinceStartup + 1.0f;
	}
}