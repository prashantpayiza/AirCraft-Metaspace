using UnityEngine;
using System.Collections;

public class GSurface: MonoBehaviour {
	public enum TSurfaceShapeType {
		default_shape, custom_shape, parent_shape, plane, sphere, halfsphere, cone, cube, rhombus, cylinder, long_cylinder, short_cylinder, streamline, half_streamline, naca_profile, tsagib_profile, npleq_profile, npleqh_profile, nplec_profile, nplech_profile, parsec_profile, rosner_profile, biconvex, wedge, cambered_plate, vandevooren, newman, joukovsky, helmboldkeune, horten, unknown
	};
	public enum TSurfaceBehaviourType {
		default_behaviour, parent_behaviour, non_laminar_analysis, laminar_analysis
	};
	
	//public GAircraft.LabeledSurfaceDesc surfaceProperties;
	
	public string id = "";
	[HideInInspector]public Vector3 lastPosition = new Vector3(0.0f, 0.0f, 0.0f);
	[HideInInspector]public Vector3 drag = new Vector3(0.0f, 0.0f, 0.0f);
	[HideInInspector]public Vector3 lift = new Vector3(0.0f, 0.0f, 0.0f);
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
	public bool surfaceDebug = false;
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
	
	public static float dragFromTSurfaceShapeTypeH(TSurfaceShapeType shape, string shape_parameter, TSurfaceShapeType parent_shape, string parent_shape_parameter, float custom_value, float default_value) {
		if (shape == TSurfaceShapeType.parent_shape) shape = parent_shape;
		switch (shape) {
		case TSurfaceShapeType.custom_shape:
			return custom_value;
		case TSurfaceShapeType.plane:
			return 0.01f;
		case TSurfaceShapeType.halfsphere:
		case TSurfaceShapeType.cambered_plate:
			return 0.42f;
		case TSurfaceShapeType.sphere:
			return 0.47f;
		case TSurfaceShapeType.cone:
			return 0.5f;
		case TSurfaceShapeType.cube:
			return 1.05f;
		case TSurfaceShapeType.rhombus:
		case TSurfaceShapeType.wedge:
			return 0.80f;
		case TSurfaceShapeType.long_cylinder:
			return 0.82f;
		case TSurfaceShapeType.short_cylinder:
			return 1.15f;
		case TSurfaceShapeType.cylinder:
			return 0.99f;
		case TSurfaceShapeType.half_streamline:
		case TSurfaceShapeType.tsagib_profile:
		case TSurfaceShapeType.parsec_profile:
		case TSurfaceShapeType.rosner_profile:
			return 0.09f;
		case TSurfaceShapeType.streamline:
		case TSurfaceShapeType.naca_profile:
		case TSurfaceShapeType.npleq_profile:
		case TSurfaceShapeType.npleqh_profile:
		case TSurfaceShapeType.nplec_profile:
		case TSurfaceShapeType.nplech_profile:
		case TSurfaceShapeType.biconvex:
		case TSurfaceShapeType.vandevooren:
		case TSurfaceShapeType.newman:
		case TSurfaceShapeType.joukovsky:
		case TSurfaceShapeType.horten:
			return 0.04f;
		case TSurfaceShapeType.helmboldkeune:
			return 0.04f * 2.0f;
		default:
			return default_value;
		}
	}
	public static float dragFromTSurfaceShapeTypeV(TSurfaceShapeType shape, string shape_parameter, TSurfaceShapeType parent_shape, string parent_shape_parameter, float custom_value, float default_value) {
		if (shape == TSurfaceShapeType.parent_shape) shape = parent_shape;
		switch (shape) {
		case TSurfaceShapeType.custom_shape:
			return custom_value;
		case TSurfaceShapeType.plane:
		case TSurfaceShapeType.cambered_plate:
			return 1.15f;
		case TSurfaceShapeType.halfsphere:
			return 0.74f;
		case TSurfaceShapeType.sphere:
		case TSurfaceShapeType.cone:
		case TSurfaceShapeType.long_cylinder:
		case TSurfaceShapeType.short_cylinder:
		case TSurfaceShapeType.cylinder:
			return 0.47f;
		case TSurfaceShapeType.cube:
			return 1.05f;
		case TSurfaceShapeType.rhombus:
		case TSurfaceShapeType.wedge:
			return 0.80f;
		case TSurfaceShapeType.half_streamline:
		case TSurfaceShapeType.tsagib_profile:
		case TSurfaceShapeType.parsec_profile:
		case TSurfaceShapeType.rosner_profile:
			return 1.15f;
		case TSurfaceShapeType.streamline:
		case TSurfaceShapeType.naca_profile:
		case TSurfaceShapeType.npleq_profile:
		case TSurfaceShapeType.npleqh_profile:
		case TSurfaceShapeType.nplec_profile:
		case TSurfaceShapeType.nplech_profile:
		case TSurfaceShapeType.biconvex:
		case TSurfaceShapeType.vandevooren:
		case TSurfaceShapeType.newman:
		case TSurfaceShapeType.joukovsky:
		case TSurfaceShapeType.horten:
			return 1.15f;
		case TSurfaceShapeType.helmboldkeune:
			return 1.15f;
		default:
			return default_value;
		}
	}
	public static float liftFromTSurfaceShapeTypeH(TSurfaceShapeType shape, string shape_parameter, TSurfaceShapeType parent_shape, string parent_shape_parameter, float custom_value, float default_value) {
		if (shape == TSurfaceShapeType.parent_shape) shape = parent_shape;
		switch (shape) {
		case TSurfaceShapeType.custom_shape:
			return custom_value;
		case TSurfaceShapeType.plane:
		case TSurfaceShapeType.halfsphere:
		case TSurfaceShapeType.sphere:
		case TSurfaceShapeType.cone:
		case TSurfaceShapeType.cube:
		case TSurfaceShapeType.rhombus:
		case TSurfaceShapeType.long_cylinder:
		case TSurfaceShapeType.short_cylinder:
		case TSurfaceShapeType.cylinder:
		case TSurfaceShapeType.streamline:
		case TSurfaceShapeType.naca_profile:
		case TSurfaceShapeType.npleq_profile:
		case TSurfaceShapeType.npleqh_profile:
		case TSurfaceShapeType.nplec_profile:
		case TSurfaceShapeType.nplech_profile:
		case TSurfaceShapeType.biconvex:
		case TSurfaceShapeType.wedge:
		case TSurfaceShapeType.cambered_plate:
		case TSurfaceShapeType.vandevooren:
		case TSurfaceShapeType.newman:
		case TSurfaceShapeType.joukovsky:
		case TSurfaceShapeType.helmboldkeune:
		case TSurfaceShapeType.horten:
			return 0.0f;
		case TSurfaceShapeType.half_streamline:
		case TSurfaceShapeType.tsagib_profile:
		case TSurfaceShapeType.parsec_profile:
		case TSurfaceShapeType.rosner_profile:
			return 0.08f;
		default:
			return default_value;
		}
	}
	public static float liftFromTSurfaceShapeTypeV(TSurfaceShapeType shape, string shape_parameter, TSurfaceShapeType parent_shape, string parent_shape_parameter, float custom_value, float default_value) {
		if (shape == TSurfaceShapeType.parent_shape) shape = parent_shape;
		switch (shape) {
		case TSurfaceShapeType.custom_shape:
			return custom_value;
		case TSurfaceShapeType.plane:
		case TSurfaceShapeType.halfsphere:
		case TSurfaceShapeType.sphere:
		case TSurfaceShapeType.cone:
		case TSurfaceShapeType.cube:
		case TSurfaceShapeType.rhombus:
		case TSurfaceShapeType.long_cylinder:
		case TSurfaceShapeType.short_cylinder:
		case TSurfaceShapeType.cylinder:
		case TSurfaceShapeType.half_streamline:
		case TSurfaceShapeType.streamline:
		case TSurfaceShapeType.naca_profile:
		case TSurfaceShapeType.tsagib_profile:
		case TSurfaceShapeType.npleq_profile:
		case TSurfaceShapeType.npleqh_profile:
		case TSurfaceShapeType.nplec_profile:
		case TSurfaceShapeType.nplech_profile:
		case TSurfaceShapeType.parsec_profile:
		case TSurfaceShapeType.rosner_profile:
		case TSurfaceShapeType.biconvex:
		case TSurfaceShapeType.wedge:
		case TSurfaceShapeType.cambered_plate:
		case TSurfaceShapeType.vandevooren:
		case TSurfaceShapeType.newman:
		case TSurfaceShapeType.joukovsky:
		case TSurfaceShapeType.helmboldkeune:
		case TSurfaceShapeType.horten:
			return 0.0f;
		default:
			return default_value;
		}
	}
	
	public static TSurfaceBehaviourType behaviourFromTSurfaceBehaviourType(TSurfaceBehaviourType behaviour, TSurfaceBehaviourType parent_behaviour, TSurfaceBehaviourType default_behaviour) {
		switch (behaviour) {
		case TSurfaceBehaviourType.parent_behaviour:
			return parent_behaviour;
		case TSurfaceBehaviourType.default_behaviour:
			return default_behaviour;
		default:
			return behaviour;
		}
	}

	public static bool laminarFromTSurfaceBehaviourType(TSurfaceBehaviourType behaviour) {
		switch (behaviour) {
		case TSurfaceBehaviourType.laminar_analysis:
			return true;
		case TSurfaceBehaviourType.non_laminar_analysis:
		default:
			return false;
		}
	}
	
	public static TSurfaceShapeType shapeTypeFromParameterReturnEndPosition(string parameter, TSurfaceShapeType previous_shape, out int endPosition) {
		string matchstring = "";

		if (parameter.Contains(matchstring = "default_shape")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.default_shape;
		} else if (parameter.Contains(matchstring = "default")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.default_shape;
		} else if (parameter.Contains(matchstring = "custom_shape")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.custom_shape;
		} else if (parameter.Contains(matchstring = "custom")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.custom_shape;
		} else if (parameter.Contains(matchstring = "parent_shape")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.parent_shape;
		} else if (parameter.Contains(matchstring = "parent")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.parent_shape;
		} else if (parameter.Contains(matchstring = "plane")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.plane;
		} else if (parameter.Contains(matchstring = "halfsphere")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.halfsphere;
		} else if (parameter.Contains(matchstring = "sphere")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.sphere;
		} else if (parameter.Contains(matchstring = "cone")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.cone;
		} else if (parameter.Contains(matchstring = "cube")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.cube;
		} else if (parameter.Contains(matchstring = "rhombus")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.rhombus;
		} else if (parameter.Contains(matchstring = "long_cylinder")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.long_cylinder;
		} else if (parameter.Contains(matchstring = "longcylinder")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.long_cylinder;
		} else if (parameter.Contains(matchstring = "short_cylinder")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.short_cylinder;
		} else if (parameter.Contains(matchstring = "shortcylinder")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.short_cylinder;
		} else if (parameter.Contains(matchstring = "cylinder")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.cylinder;
		} else if (parameter.Contains(matchstring = "half_streamline")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.half_streamline;
		} else if (parameter.Contains(matchstring = "halfstreamline")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.half_streamline;
		} else if (parameter.Contains(matchstring = "streamline")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.streamline;
		} else if (parameter.Contains(matchstring = "naca_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.naca_profile;
		} else if (parameter.Contains(matchstring = "nacaprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.naca_profile;
		} else if (parameter.Contains(matchstring = "naca")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.naca_profile;
		} else if (parameter.Contains(matchstring = "tsagib_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.tsagib_profile;
		} else if (parameter.Contains(matchstring = "tsagibprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.tsagib_profile;
		} else if (parameter.Contains(matchstring = "tsagib")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.tsagib_profile;
		} else if (parameter.Contains(matchstring = "npleq_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.npleq_profile;
		} else if (parameter.Contains(matchstring = "npleqprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.npleq_profile;
		} else if (parameter.Contains(matchstring = "npleq")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.npleq_profile;
		} else if (parameter.Contains(matchstring = "npleqh_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.npleqh_profile;
		} else if (parameter.Contains(matchstring = "npleqhprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.npleqh_profile;
		} else if (parameter.Contains(matchstring = "npleqh")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.npleqh_profile;
		} else if (parameter.Contains(matchstring = "nplec_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.nplec_profile;
		} else if (parameter.Contains(matchstring = "nplecprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.nplec_profile;
		} else if (parameter.Contains(matchstring = "nplec")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.nplec_profile;
		} else if (parameter.Contains(matchstring = "nplech_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.nplech_profile;
		} else if (parameter.Contains(matchstring = "nplechprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.nplech_profile;
		} else if (parameter.Contains(matchstring = "nplech")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.nplech_profile;
		} else if (parameter.Contains(matchstring = "parsec_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.parsec_profile;
		} else if (parameter.Contains(matchstring = "parsecprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.parsec_profile;
		} else if (parameter.Contains(matchstring = "parsec")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.parsec_profile;
		} else if (parameter.Contains(matchstring = "rosner_profile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.rosner_profile;
		} else if (parameter.Contains(matchstring = "rosnerprofile")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.rosner_profile;
		} else if (parameter.Contains(matchstring = "rosner")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.rosner_profile;
		} else if (parameter.Contains(matchstring = "biconvex")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.biconvex;
		} else if (parameter.Contains(matchstring = "wedge")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.wedge;
		} else if (parameter.Contains(matchstring = "cambered_plate")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.cambered_plate;
		} else if (parameter.Contains(matchstring = "camberedplate")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.cambered_plate;
		} else if (parameter.Contains(matchstring = "vandevooren")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.vandevooren;
		} else if (parameter.Contains(matchstring = "newman")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.newman;
		} else if (parameter.Contains(matchstring = "joukovsky")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.joukovsky;
		} else if (parameter.Contains(matchstring = "helmboldkeune")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.helmboldkeune;
		} else if (parameter.Contains(matchstring = "horten")) {
			endPosition = parameter.IndexOf(matchstring) + matchstring.Length;
			return TSurfaceShapeType.horten;
		} else {
			endPosition = 0;
			return previous_shape;
		}
	}
	public static TSurfaceShapeType shapeTypeFromParameter(string parameter, TSurfaceShapeType previous_shape) {
		int endPosition;
		return shapeTypeFromParameterReturnEndPosition(parameter, previous_shape, out endPosition);
	}
	public static TSurfaceShapeType toTSurfaceShapeType(string s) {
		return shapeTypeFromParameter(s, TSurfaceShapeType.unknown);
	}
	public static string fromTSurfaceShapeType(TSurfaceShapeType s) {
		switch(s) {
		case TSurfaceShapeType.default_shape:
			//return "default_shape";
			return "default";
		case TSurfaceShapeType.custom_shape:
			//return "custom_shape";
			return "custom";
		case TSurfaceShapeType.parent_shape:
			//return "parent_shape";
			return "parent";
		case TSurfaceShapeType.plane:
			return "plane";
		case TSurfaceShapeType.halfsphere:
			return "halfsphere";
		case TSurfaceShapeType.sphere:
			return "sphere";
		case TSurfaceShapeType.cone:
			return "cone";
		case TSurfaceShapeType.cube:
			return "cube";
		case TSurfaceShapeType.rhombus:
			return "rhombus";
		case TSurfaceShapeType.long_cylinder:
			return "long_cylinder";
		case TSurfaceShapeType.short_cylinder:
			return "short_cylinder";
		case TSurfaceShapeType.cylinder:
			return "cylinder";
		case TSurfaceShapeType.half_streamline:
			return "half_streamline";
		case TSurfaceShapeType.streamline:
			return "streamline";
		case TSurfaceShapeType.naca_profile:
			return "naca_profile";
		case TSurfaceShapeType.tsagib_profile:
			return "tsagib_profile";
		case TSurfaceShapeType.npleq_profile:
			return "npleq_profile";
		case TSurfaceShapeType.npleqh_profile:
			return "npleqh_profile";
		case TSurfaceShapeType.nplec_profile:
			return "nplec_profile";
		case TSurfaceShapeType.nplech_profile:
			return "nplech_profile";
		case TSurfaceShapeType.parsec_profile:
			return "parsec_profile";
		case TSurfaceShapeType.rosner_profile:
			return "rosner_profile";
		case TSurfaceShapeType.biconvex:
			return "biconvex";
		case TSurfaceShapeType.wedge:
			return "wedge";
		case TSurfaceShapeType.cambered_plate:
			return "cambered_plate";
		case TSurfaceShapeType.vandevooren:
			return "vandevooren";
		case TSurfaceShapeType.newman:
			return "newman";
		case TSurfaceShapeType.joukovsky:
			return "joukovsky";
		case TSurfaceShapeType.helmboldkeune:
			return "helmboldkeune";
		case TSurfaceShapeType.horten:
			return "horten";
		default:
			return "unknown";
		}
	}
}
