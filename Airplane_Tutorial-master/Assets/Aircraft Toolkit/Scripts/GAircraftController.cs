using UnityEngine;
using System.Collections;

///
/// IMPORTANT!: this file is still in alpha state, use with caution, in future releases will be completed!
///

public class GAircraftController: MonoBehaviour {
	
	GAircraft aircraft = null;
	public enum TAircraftControllerType { bypass, helicopter_autolevel, airplane_autolevel, airplane_goto, airplaned_goto, airplane2_goto, airplaned, airplane_fast_autolevel, basic_touch_control, airplanep_goto };
	
	public TAircraftControllerType type = GAircraftController.TAircraftControllerType.bypass;
	private GameObject target = null;
	public string targetGameObject = "Write here yours GameObject target";
	
	public float pitchLimit = 25.0f;
	public float pitchMultiplier = 2.0f;
	public float yawLimit = 55.0f;
	public float yawMultiplier = 1.5f;
	public float rollLimit = 45.0f;
	public float rollMultiplier = 1.5f;

	void Start() {
		aircraft = (GAircraft)gameObject.GetComponent("GAircraft");
		if (GameObject.Find(targetGameObject) != null) target = GameObject.Find(targetGameObject);
	}
	
	private float auto_ailerons = 0.0f;
	private float auto_ailerons_filtered = 0.0f;
	private float auto_ailerons_filter_coeficient = 0.01f;
	private float auto_elevator = 0.0f;
	private float auto_elevator_filtered = 0.0f;
	private float auto_elevator_filter_coeficient = 0.01f;
	private float auto_rudder = 0.0f;
	private float auto_rudder_filtered = 0.0f;
	private float auto_rudder_filter_coeficient = 0.01f;
	private float auto_rudder_target = 0.0f;
	private float auto_throttle = 0.0f;
	private float auto_throttle_filtered = 0.0f;
	private float auto_throttle_filter_coeficient = 0.01f;
	private float auto_throttle_target = 0.0f;
	
	void FixedUpdate() {
		float cthrottle_filter_coef;
		float cthrottle_automanual_coef;
		float cthrottle_coef;
		float crudder_filter_coef;
		float crudder_coef;
		float crudder_automanual_coef;
		float celevator_coef;
		float celevator_automanual_coef;
		float cailerons_coef;
		float cailerons_automanual_coef;
		Vector3 angles = Vector3.zero;
		//Vector3 angles_speed = Vector3.zero;
		//Vector3 angles_speed;
		float ay = 0.0f;
		float axz = 0.0f;

		Vector3 fwd;
		Vector3 rgt;
		Vector3 tgt;
		Vector3 tgt2;

		float anglezlimit;
		float anglezmultiplier;
		float anglexlimit;
		float anglexmultiplier;
		
		switch (type) {
		default: case TAircraftControllerType.bypass:
			//GPivot.setAnyPivot("cailerons", GPivot.getAnyPivot("ailerons"));
			//GPivot.setAnyPivot("celevator", GPivot.getAnyPivot("elevator"));
			//GPivot.setAnyPivot("crudder", GPivot.getAnyPivot("rudder"));
			//GPivot.setAnyPivot("cthrottle", GPivot.getAnyPivot("throttle"));
			GPivot.setAnyPivot("cailerons", aircraft.inputAilerons_output);
			GPivot.setAnyPivot("celevator", aircraft.inputElevator_output);
			GPivot.setAnyPivot("crudder", aircraft.inputRudder_output);
			GPivot.setAnyPivot("cthrottle", aircraft.inputThrottle_output);
			break;
		case TAircraftControllerType.helicopter_autolevel:
			//cthrottle_filter_coef = 0.005f;
			cthrottle_filter_coef = 0.01f;
			cthrottle_automanual_coef = 0.4f;
			//cthrottle_coef = 0.5f;
			cthrottle_coef = 0.4f;
			//crudder_filter_coef = 0.005f;
			crudder_filter_coef = 0.1f;
			//crudder_coef = 1.0f;
			crudder_coef = 0.4f;
			crudder_automanual_coef = 0.2f;
			//celevator_coef = 1.0f;
			//celevator_coef = 0.15f;
			//celevator_coef = 0.04f;
			celevator_coef = 0.01f;
			celevator_automanual_coef = 0.2f;
			//cailerons_coef = 1.0f;
			//cailerons_coef = 0.15f;
			//cailerons_coef = 0.04f;
			cailerons_coef = 0.01f;
			cailerons_automanual_coef = 0.2f;

			angles = gameObject.transform.eulerAngles;

			if (angles.x > 180.0f) angles.x = angles.x - 360.0f;
			if (angles.y > 180.0f) angles.y = angles.y - 360.0f;
			if (angles.z > 180.0f) angles.z = angles.z - 360.0f;
			if (auto_rudder_target < angles.y - 180.0f) auto_rudder_target = auto_rudder_target + 360.0f;
			if (auto_rudder_target > angles.y + 180.0f) auto_rudder_target = auto_rudder_target - 360.0f;
			auto_rudder_target = auto_rudder_target * (1.0f - crudder_filter_coef) + angles.y * crudder_filter_coef;
			auto_throttle_target = auto_throttle_target * (1.0f - cthrottle_filter_coef) + gameObject.transform.position.y * cthrottle_filter_coef;
			auto_ailerons = 0.5f + angles.z * cailerons_coef;
			if (auto_ailerons < 0.0f) auto_ailerons = 0.0f;
			if (auto_ailerons > 1.0f) auto_ailerons = 1.0f;
			auto_elevator = 0.5f + angles.x * celevator_coef;
			if (auto_elevator < 0.0f) auto_elevator = 0.0f;
			if (auto_elevator > 1.0f) auto_elevator = 1.0f;
			auto_rudder = 0.5f + (auto_rudder_target - angles.y) * crudder_coef;
			if (auto_rudder < 0.0f) auto_rudder = 0.0f;
			if (auto_rudder > 1.0f) auto_rudder = 1.0f;
			auto_throttle = (auto_throttle_target - gameObject.transform.position.y) * cthrottle_coef;
			if (auto_throttle < 0.2f) auto_throttle = 0.2f;
			if (auto_throttle > 0.8f) auto_throttle = 0.8f;

			GPivot.setAnyPivot("cailerons", auto_ailerons * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef));
			GPivot.setAnyPivot("celevator", auto_elevator * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef));
			GPivot.setAnyPivot("crudder", auto_rudder * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef));
			GPivot.setAnyPivot("cthrottle", auto_throttle * cthrottle_automanual_coef + aircraft.inputThrottle_output * (1.0f - cthrottle_automanual_coef));

			aircraft.inputAilerons_internal = auto_ailerons * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef);
			aircraft.inputElevator_internal = auto_elevator * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef);
			aircraft.inputRudder_internal = auto_rudder * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef);
			aircraft.inputThrottle_output = auto_throttle * cthrottle_automanual_coef + (aircraft.inputThrottle_output + aircraft.inputThrottle_internal + aircraft.kThrottle_aft) * (1.0f - cthrottle_automanual_coef);
			//Debug.Log (aircraft.inputThrottle_internal);
			break;
		case TAircraftControllerType.airplane_autolevel:
			//cthrottle_filter_coef = 0.005f;
			cthrottle_filter_coef = 0.01f;
			cthrottle_automanual_coef = 0.4f;
			//cthrottle_coef = 0.5f;
			cthrottle_coef = 0.4f;
			//crudder_filter_coef = 0.005f;
			crudder_filter_coef = 0.01f;
			//crudder_coef = 1.0f;
			crudder_coef = 0.1f;
			//crudder_automanual_coef = 0.2f;
			crudder_automanual_coef = 0.0f;
			//celevator_coef = 1.0f;
			//celevator_coef = 0.15f;
			//celevator_coef = 0.04f;
			celevator_coef = 0.1f;
			//celevator_automanual_coef = 0.3f;
			celevator_automanual_coef = 0.7f;
			//cailerons_coef = 1.0f;
			//cailerons_coef = 0.15f;
			//cailerons_coef = 0.04f;
			cailerons_coef = 0.01f;
			cailerons_automanual_coef = 0.2f;

			angles = gameObject.transform.eulerAngles;
			if (gameObject.GetComponent<Rigidbody>().velocity.magnitude > 0.0f) {
				Vector3 velocitynoy = gameObject.GetComponent<Rigidbody>().velocity;
				if (velocitynoy.y > 0.0f) {
					velocitynoy.y = 0.0f;
					angles.x = -Vector3.Angle(velocitynoy, gameObject.GetComponent<Rigidbody>().velocity);
				} else {
					velocitynoy.y = 0.0f;
					angles.x = Vector3.Angle(velocitynoy, gameObject.GetComponent<Rigidbody>().velocity);
				}
			}

			if (angles.x > 180.0f) angles.x = angles.x - 360.0f;
			if (angles.y > 180.0f) angles.y = angles.y - 360.0f;
			if (angles.z > 180.0f) angles.z = angles.z - 360.0f;
			if (auto_rudder_target < angles.y - 180.0f) auto_rudder_target = auto_rudder_target + 360.0f;
			if (auto_rudder_target > angles.y + 180.0f) auto_rudder_target = auto_rudder_target - 360.0f;
			auto_rudder_target = auto_rudder_target * (1.0f - crudder_filter_coef) + angles.y * crudder_filter_coef;
			auto_throttle_target = auto_throttle_target * (1.0f - cthrottle_filter_coef) + gameObject.transform.position.y * cthrottle_filter_coef;
			auto_ailerons = 0.5f + angles.z * cailerons_coef;
			if (auto_ailerons < 0.0f) auto_ailerons = 0.0f;
			if (auto_ailerons > 1.0f) auto_ailerons = 1.0f;
			auto_elevator = 0.5f + 0.15f + angles.x * celevator_coef;
			if (auto_elevator < 0.0f) auto_elevator = 0.0f;
			if (auto_elevator > 1.0f) auto_elevator = 1.0f;
			auto_rudder = 0.5f + (auto_rudder_target - angles.y) * crudder_coef;
			if (auto_rudder < 0.0f) auto_rudder = 0.0f;
			if (auto_rudder > 1.0f) auto_rudder = 1.0f;
			auto_throttle = (auto_throttle_target - gameObject.transform.position.y) * cthrottle_coef;
			if (auto_throttle < 0.2f) auto_throttle = 0.2f;
			if (auto_throttle > 0.8f) auto_throttle = 0.8f;

			auto_ailerons_filtered = auto_ailerons_filtered * (1.0f - auto_ailerons_filter_coeficient) + auto_ailerons * auto_ailerons_filter_coeficient; 
			auto_elevator_filtered = auto_elevator_filtered * (1.0f - auto_elevator_filter_coeficient) + auto_elevator * auto_elevator_filter_coeficient; 
			auto_rudder_filtered = auto_rudder_filtered * (1.0f - auto_rudder_filter_coeficient) + auto_rudder * auto_rudder_filter_coeficient; 
			auto_throttle_filtered = auto_throttle_filtered * (1.0f - auto_throttle_filter_coeficient) + auto_throttle * auto_throttle_filter_coeficient; 

			GPivot.setAnyPivot("cailerons", auto_ailerons_filtered * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef));
			GPivot.setAnyPivot("celevator", auto_elevator_filtered * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef));
			GPivot.setAnyPivot("crudder", auto_rudder_filtered * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef));
			GPivot.setAnyPivot("cthrottle", auto_throttle_filtered * cthrottle_automanual_coef + aircraft.inputThrottle_output * (1.0f - cthrottle_automanual_coef));
			break;
		case TAircraftControllerType.airplane_goto:
		case TAircraftControllerType.airplanep_goto:
		case TAircraftControllerType.airplaned_goto:
			if (target == null) break;
			//cthrottle_filter_coef = 0.005f;
			cthrottle_filter_coef = 0.01f;
			cthrottle_automanual_coef = 0.4f;
			//cthrottle_coef = 0.5f;
			cthrottle_coef = 0.4f;
			//crudder_filter_coef = 0.005f;
			crudder_filter_coef = 0.01f;
			//crudder_coef = 1.0f;
			crudder_coef = 0.1f;
			//crudder_automanual_coef = 0.2f;
			crudder_automanual_coef = 0.0f;
			//celevator_coef = 1.0f;
			//celevator_coef = 0.15f;
			//celevator_coef = 0.04f;
			celevator_coef = 0.1f;
			//celevator_automanual_coef = 0.3f;
			celevator_automanual_coef = 0.7f;
			//cailerons_coef = 1.0f;
			//cailerons_coef = 0.15f;
			//cailerons_coef = 0.04f;
			cailerons_coef = 0.01f * (rollLimit / 45);
			cailerons_automanual_coef = 0.2f;
		
			fwd = gameObject.transform.forward;
			rgt = gameObject.transform.right;
			tgt = target.transform.position - gameObject.transform.position;
			tgt2 = tgt;

			anglezlimit = yawLimit;
			anglezmultiplier = yawMultiplier;
			anglexlimit = pitchLimit;
			anglexmultiplier = pitchMultiplier;

			fwd.y = 0.0f;
			rgt.y = 0.0f;
			tgt.y = 0.0f;
			axz = (tgt2.y > 0.0f) ? Vector3.Angle(tgt, tgt2) : -Vector3.Angle(tgt, tgt2);
			ay = (Vector3.Dot(tgt, rgt) > 0.0f) ? Vector3.Angle(fwd, tgt) : -Vector3.Angle(fwd, tgt);
			//if (ay > 20.0f) ay = 20.0f;
			//if (ay < -20.0f) ay = -20.0f;
		
			angles = gameObject.transform.eulerAngles;
			if (gameObject.GetComponent<Rigidbody>().velocity.magnitude > 0.0f) {
				Vector3 velocitynoy = gameObject.GetComponent<Rigidbody>().velocity;
				if (velocitynoy.y > 0.0f) {
					velocitynoy.y = 0.0f;
					angles.x = -Vector3.Angle(velocitynoy, gameObject.GetComponent<Rigidbody>().velocity);
				} else {
					velocitynoy.y = 0.0f;
					angles.x = Vector3.Angle(velocitynoy, gameObject.GetComponent<Rigidbody>().velocity);
				}
			}
			if (axz > anglexlimit) angles.x += anglexlimit * anglexmultiplier;
			else if (axz < -anglexlimit) angles.x -= anglexlimit * anglexmultiplier;
			else angles.x += axz * anglexmultiplier;
			

			if (angles.x > 180.0f) angles.x = angles.x - 360.0f;
			if (angles.y > 180.0f) angles.y = angles.y - 360.0f;
			if (angles.z > 180.0f) angles.z = angles.z - 360.0f;
		
			if (ay > anglezlimit) angles.z += anglezlimit * anglezmultiplier;
			else if (ay < -anglezlimit) angles.z -= anglezlimit * anglezmultiplier;
			else angles.z += ay * anglezmultiplier;
			//angles.x -= Mathf.Abs(ay / 10.0f);
			if (auto_rudder_target < angles.y - 180.0f) auto_rudder_target = auto_rudder_target + 360.0f;
			if (auto_rudder_target > angles.y + 180.0f) auto_rudder_target = auto_rudder_target - 360.0f;
			auto_rudder_target = auto_rudder_target * (1.0f - crudder_filter_coef) + angles.y * crudder_filter_coef;
			auto_throttle_target = auto_throttle_target * (1.0f - cthrottle_filter_coef) + gameObject.transform.position.y * cthrottle_filter_coef;
			auto_ailerons = 0.5f + angles.z * cailerons_coef;
			if (auto_ailerons < 0.0f) auto_ailerons = 0.0f;
			if (auto_ailerons > 1.0f) auto_ailerons = 1.0f;
			auto_elevator = 0.5f + 0.15f + angles.x * celevator_coef;
			if (auto_elevator < 0.0f) auto_elevator = 0.0f;
			if (auto_elevator > 1.0f) auto_elevator = 1.0f;
			auto_rudder = 0.5f + (auto_rudder_target - angles.y) * crudder_coef;
			if (auto_rudder < 0.0f) auto_rudder = 0.0f;
			if (auto_rudder > 1.0f) auto_rudder = 1.0f;
			auto_throttle = (auto_throttle_target - gameObject.transform.position.y) * cthrottle_coef;
			if (auto_throttle < 0.2f) auto_throttle = 0.2f;
			if (auto_throttle > 0.8f) auto_throttle = 0.8f;

			auto_ailerons_filtered = auto_ailerons_filtered * (1.0f - auto_ailerons_filter_coeficient) + auto_ailerons * auto_ailerons_filter_coeficient; 
			auto_elevator_filtered = auto_elevator_filtered * (1.0f - auto_elevator_filter_coeficient) + auto_elevator * auto_elevator_filter_coeficient; 
			auto_rudder_filtered = auto_rudder_filtered * (1.0f - auto_rudder_filter_coeficient) + auto_rudder * auto_rudder_filter_coeficient; 
			auto_throttle_filtered = auto_throttle_filtered * (1.0f - auto_throttle_filter_coeficient) + auto_throttle * auto_throttle_filter_coeficient; 
		
			switch (type) {
				case TAircraftControllerType.airplane_goto:
					GPivot.setAnyPivot("cailerons", auto_ailerons_filtered * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef));
					GPivot.setAnyPivot("celevator", auto_elevator_filtered * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef));
					GPivot.setAnyPivot("crudder", auto_rudder_filtered * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef));
					//GPivot.setAnyPivot("cthrottle", auto_throttle_filtered * cthrottle_automanual_coef + aircraft.inputThrottle_output * (1.0f - cthrottle_automanual_coef));
					GPivot.setAnyPivot("cthrottle", 1.0f);
					aircraft.inputThrottle_output = 1.0f;
	
					if (aircraft.speed > 100) aircraft.inputGears_internal_enabled = false;
					else aircraft.inputGears_internal_enabled = true;
					if (aircraft.speed > 100) aircraft.inputFlaps_internal_enabled = false;
					else aircraft.inputFlaps_internal_enabled = true;
					break;
				case TAircraftControllerType.airplanep_goto:
					GPivot.setAnyPivot("cailerons", auto_ailerons_filtered * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef));
					GPivot.setAnyPivot("celevator", auto_elevator_filtered * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef));
					GPivot.setAnyPivot("crudder", auto_rudder_filtered * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef));
					//GPivot.setAnyPivot("cthrottle", auto_throttle_filtered * cthrottle_automanual_coef + aircraft.inputThrottle_output * (1.0f - cthrottle_automanual_coef));
					GPivot.setAnyPivot("cthrottle", 1.0f);
					aircraft.inputThrottle_output = 1.0f;
				
					aircraft.inputAilerons_output = GPivot.getAnyPivot("cailerons");
					aircraft.inputElevator_output = GPivot.getAnyPivot("celevator");
					aircraft.inputRudder_output = GPivot.getAnyPivot("crudder");
	
					if (aircraft.speed > 100) aircraft.inputGears_internal_enabled = false;
					else aircraft.inputGears_internal_enabled = true;
					if (aircraft.speed > 100) aircraft.inputFlaps_internal_enabled = false;
					else aircraft.inputFlaps_internal_enabled = true;
					break;
				case TAircraftControllerType.airplaned_goto:
					//aircraft.inputAilerons_output = auto_ailerons_filtered * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef);
					//aircraft.inputElevator_output = auto_elevator_filtered * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef);
					//aircraft.inputRudder_output = auto_rudder_filtered * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef);
					//aircraft.inputAilerons_output = auto_ailerons_filtered;
					//aircraft.inputElevator_output = auto_elevator_filtered;
					//aircraft.inputRudder_output = auto_rudder_filtered;
					//aircraft.inputAilerons_internal = auto_ailerons_filtered;
					//aircraft.inputElevator_internal = auto_elevator_filtered;
					//aircraft.inputRudder_internal = auto_rudder_filtered;
					//aircraft.inputAilerons_internal = auto_ailerons_filtered * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef);
					//aircraft.inputElevator_internal = auto_elevator_filtered * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef);
					//aircraft.inputRudder_internal = auto_rudder_filtered * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef);
					//aircraft.inputThrottle_output = auto_throttle_filtered * cthrottle_automanual_coef + aircraft.inputThrottle_output * (1.0f - cthrottle_automanual_coef);
					//aircraft.inputThrottle_output = 1.0f;
	
					if (aircraft.speed > 100) aircraft.inputGears_internal_enabled = false;
					else aircraft.inputGears_internal_enabled = true;
					if (aircraft.speed > 100) aircraft.inputFlaps_internal_enabled = false;
					else aircraft.inputFlaps_internal_enabled = true;
					break;
			}
			break;
		//case TAircraftControllerType.missile_goto:
		//
		//	break;
		case TAircraftControllerType.airplane2_goto:
			if (target == null) break;
			//cthrottle_filter_coef = 0.005f;
			cthrottle_filter_coef = 0.01f;
			cthrottle_automanual_coef = 0.4f;
			//cthrottle_coef = 0.5f;
			cthrottle_coef = 0.4f;
			//crudder_filter_coef = 0.005f;
			crudder_filter_coef = 0.01f;
			//crudder_coef = 1.0f;
			crudder_coef = 0.1f;
			//crudder_automanual_coef = 0.2f;
			crudder_automanual_coef = 0.0f;
			//celevator_coef = 1.0f;
			//celevator_coef = 0.15f;
			//celevator_coef = 0.04f;
			celevator_coef = 0.1f;
			//celevator_automanual_coef = 0.3f;
			celevator_automanual_coef = 0.7f;
			//cailerons_coef = 1.0f;
			//cailerons_coef = 0.15f;
			//cailerons_coef = 0.04f;
			cailerons_coef = 0.01f;
			cailerons_automanual_coef = 0.2f;
		
			fwd = gameObject.transform.forward;
			rgt = gameObject.transform.right;
			tgt = target.transform.position - gameObject.transform.position;
			tgt2 = tgt;

			anglezlimit = yawLimit;
			anglezmultiplier = yawMultiplier;
			anglexlimit = pitchLimit;
			anglexmultiplier = pitchMultiplier;

			fwd.y = 0.0f;
			rgt.y = 0.0f;
			tgt.y = 0.0f;
			axz = (tgt2.y > 0.0f) ? Vector3.Angle(tgt, tgt2) : -Vector3.Angle(tgt, tgt2);
			ay = (Vector3.Dot(tgt, rgt) > 0.0f) ? Vector3.Angle(fwd, tgt) : -Vector3.Angle(fwd, tgt);
			//if (ay > 20.0f) ay = 20.0f;
			//if (ay < -20.0f) ay = -20.0f;
		
			angles = gameObject.transform.eulerAngles;
			//angles_speed = Quaternion.LookRotation(gameObject.rigidbody.velocity).eulerAngles;
		
			float anglelimit;
			float anglemult;
			float angles_speed_x;
			float angles_speed_z;
			float anglemix = 0.5f;
		
			float mouseh = 0.0f;
			float mousev = 0.0f;
		
			mouseh = (Input.mousePosition.x - Screen.width / 2.0f) / Screen.width;
			mousev = (Input.mousePosition.y - Screen.height / 2.0f) / Screen.height;
			mouseh = mouseh * 1.5f;
			mousev = mousev * 1.5f;

			if (mouseh > 0.12f) mouseh = 0.12f;
			if (mouseh < -0.12f) mouseh = -0.12f;
			if (mousev > 0.3f) mousev = 0.3f;
			if (mousev < -0.3f) mousev = -0.3f;
		
			anglelimit = 15.0f;
			anglemult = 0.5f;
			angles_speed_z = angles.z + mouseh * 140.0f * 3.0f;
			if (angles_speed_z > 180.0f) angles_speed_z = angles_speed_z - 360.0f;
			if (angles_speed_z > anglelimit) angles_speed_z = anglelimit;
			if (angles_speed_z < -anglelimit) angles_speed_z = -anglelimit;
			//Debug.Log (angles.x.ToString() + " // " + angles_speed_x.ToString());
		
			auto_ailerons = angles_speed_z / anglelimit * 0.5f * anglemult + 0.5f;
			auto_ailerons_filtered = auto_ailerons_filtered * (1.0f - auto_ailerons_filter_coeficient) + auto_ailerons * auto_ailerons_filter_coeficient; 
			//aircraft.inputAilerons_internal = auto_ailerons_filtered;
			aircraft.inputAilerons_internal = aircraft.inputAilerons_internal * (1.0f - anglemix) + auto_ailerons_filtered * anglemix;
			Debug.Log(aircraft.inputAilerons_internal.ToString() + " // " + auto_ailerons.ToString() + " // " + angles_speed_z.ToString() + " // " + angles.z.ToString());
		
			anglelimit = 45.0f;
			//anglemult = 0.5f;
			anglemult = 1.5f;
			//angles_speed_x = angles_speed.x;
			angles_speed_x = angles.x + mousev * 140.0f * 2.0f;
			if (angles_speed_x > 180.0f) angles_speed_x = angles_speed_x - 360.0f;
			if (angles_speed_x > anglelimit) angles_speed_x = anglelimit;
			if (angles_speed_x < -anglelimit) angles_speed_x = -anglelimit;
			//Debug.Log (angles.x.ToString() + " // " + angles_speed_x.ToString());

			//aircraft.inputElevator_internal = -angles_speed_x / anglelimit * 0.5f * anglemult + 0.5f;
			//auto_elevator = -angles_speed.x / anglelimit * 0.5f * anglemult + 0.5f;
			//auto_elevator = angles_speed_x / anglelimit * 0.5f * anglemult + 0.5f;
			auto_elevator = angles_speed_x / anglelimit * 0.5f * anglemult + 0.75f;
			auto_elevator_filtered = auto_elevator_filtered * (1.0f - auto_elevator_filter_coeficient) + auto_elevator * auto_elevator_filter_coeficient; 
			//aircraft.inputElevator_internal = auto_elevator_filtered;
			aircraft.inputElevator_internal = aircraft.inputElevator_internal * (1.0f - anglemix) + auto_elevator_filtered * anglemix;
			//Debug.Log(aircraft.inputElevator_internal.ToString() + " // " + auto_elevator.ToString());
		
			aircraft.inputRudder_internal = aircraft.inputRudder_internal * (1.0f - anglemix) + (mouseh * 1.2f + 0.5f) * anglemix;
			//Debug.Log (angles.ToString() + " // " + angles_speed.ToString());
		
		/*
			if (gameObject.rigidbody.velocity.magnitude > 0.0f) {
				Vector3 velocitynoy = gameObject.rigidbody.velocity;
				if (velocitynoy.y > 0.0f) {
					velocitynoy.y = 0.0f;
					angles.x = -Vector3.Angle(velocitynoy, gameObject.rigidbody.velocity);
				} else {
					velocitynoy.y = 0.0f;
					angles.x = Vector3.Angle(velocitynoy, gameObject.rigidbody.velocity);
				}
			}
			if (axz > anglexlimit) angles.x += anglexlimit * anglexmultiplier;
			else if (axz < -anglexlimit) angles.x -= anglexlimit * anglexmultiplier;
			else angles.x += axz * anglexmultiplier;
		*/
		/*
			if (angles.x > 180.0f) angles.x = angles.x - 360.0f;
			if (angles.y > 180.0f) angles.y = angles.y - 360.0f;
			if (angles.z > 180.0f) angles.z = angles.z - 360.0f;
		
			if (ay > anglezlimit) angles.z += anglezlimit * anglezmultiplier;
			else if (ay < -anglezlimit) angles.z -= anglezlimit * anglezmultiplier;
			else angles.z += ay * anglezmultiplier;
			//angles.x -= Mathf.Abs(ay / 10.0f);
			if (auto_rudder_target < angles.y - 180.0f) auto_rudder_target = auto_rudder_target + 360.0f;
			if (auto_rudder_target > angles.y + 180.0f) auto_rudder_target = auto_rudder_target - 360.0f;
			auto_rudder_target = auto_rudder_target * (1.0f - crudder_filter_coef) + angles.y * crudder_filter_coef;
			auto_throttle_target = auto_throttle_target * (1.0f - cthrottle_filter_coef) + gameObject.transform.position.y * cthrottle_filter_coef;
			auto_ailerons = 0.5f + angles.z * cailerons_coef;
			if (auto_ailerons < 0.0f) auto_ailerons = 0.0f;
			if (auto_ailerons > 1.0f) auto_ailerons = 1.0f;
			//auto_elevator = 0.5f + 0.15f + angles.x * celevator_coef;
			//if (auto_elevator < 0.0f) auto_elevator = 0.0f;
			//if (auto_elevator > 1.0f) auto_elevator = 1.0f;
			auto_rudder = 0.5f + (auto_rudder_target - angles.y) * crudder_coef;
			if (auto_rudder < 0.0f) auto_rudder = 0.0f;
			if (auto_rudder > 1.0f) auto_rudder = 1.0f;
			auto_throttle = (auto_throttle_target - gameObject.transform.position.y) * cthrottle_coef;
			if (auto_throttle < 0.2f) auto_throttle = 0.2f;
			if (auto_throttle > 0.8f) auto_throttle = 0.8f;

			auto_ailerons_filtered = auto_ailerons_filtered * (1.0f - auto_ailerons_filter_coeficient) + auto_ailerons * auto_ailerons_filter_coeficient; 
			auto_elevator_filtered = auto_elevator_filtered * (1.0f - auto_elevator_filter_coeficient) + auto_elevator * auto_elevator_filter_coeficient; 
			auto_rudder_filtered = auto_rudder_filtered * (1.0f - auto_rudder_filter_coeficient) + auto_rudder * auto_rudder_filter_coeficient; 
			auto_throttle_filtered = auto_throttle_filtered * (1.0f - auto_throttle_filter_coeficient) + auto_throttle * auto_throttle_filter_coeficient; 
		
			aircraft.inputAilerons_internal = auto_ailerons_filtered;
			aircraft.inputElevator_internal = auto_elevator_filtered;
			aircraft.inputRudder_internal = auto_rudder_filtered;
			aircraft.inputElevator_internal = auto_elevator_filtered;
			Debug.Log (aircraft.inputElevator_internal.ToString() + " // " + auto_elevator.ToString());
			*/
			//aircraft.inputAilerons_internal = auto_ailerons_filtered * cailerons_automanual_coef + aircraft.inputAilerons_output * (1.0f - cailerons_automanual_coef);
			//aircraft.inputElevator_internal = auto_elevator_filtered * celevator_automanual_coef + aircraft.inputElevator_output * (1.0f - celevator_automanual_coef);
			//aircraft.inputRudder_internal = auto_rudder_filtered * crudder_automanual_coef + aircraft.inputRudder_output * (1.0f - crudder_automanual_coef);
			//aircraft.inputThrottle_output = auto_throttle_filtered * cthrottle_automanual_coef + aircraft.inputThrottle_output * (1.0f - cthrottle_automanual_coef);
			//aircraft.inputThrottle_output = 1.0f;

			//if (aircraft.speed > 100) aircraft.inputGears_internal_enabled = false;
			//else aircraft.inputGears_internal_enabled = true;
			//if (aircraft.speed > 100) aircraft.inputFlaps_internal_enabled = false;
			//else aircraft.inputFlaps_internal_enabled = true;
			break;
		case TAircraftControllerType.airplaned:
			mouseh = (Input.mousePosition.x - Screen.width / 2.0f) / Screen.width;
			mousev = (Input.mousePosition.y - Screen.height / 2.0f) / Screen.height;
			mouseh = mouseh * 2.0f;
			mousev = mousev * 2.0f;
			//if (mouseh > 0.5f) mouseh = 0.5f;
			//if (mouseh < -0.5f) mouseh = -0.5f;
			if (mouseh > 0.15f) mouseh = 0.15f;
			if (mouseh < -0.15f) mouseh = -0.15f;
			if (mousev > 0.5f) mousev = 0.5f;
			if (mousev < -0.5f) mousev = -0.5f;
		
			anglemix = 0.5f;

			aircraft.inputElevator_internal = aircraft.inputElevator_internal * (1.0f - anglemix) + (mousev * 1.0f + 0.5f) * anglemix;
			aircraft.inputAilerons_internal = aircraft.inputAilerons_internal * (1.0f - anglemix) + (mouseh * 1.0f + 0.5f) * anglemix;
			aircraft.inputThrottle_output = Input.GetMouseButton(0) ? 1.0f : 0.0f;
			break;
		case TAircraftControllerType.airplane_fast_autolevel:
			//if (target == null) break;
			//cthrottle_filter_coef = 0.005f;
			cthrottle_filter_coef = 0.01f;
			cthrottle_automanual_coef = 0.4f;
			//cthrottle_coef = 0.5f;
			cthrottle_coef = 0.4f;
			//crudder_filter_coef = 0.005f;
			crudder_filter_coef = 0.01f;
			//crudder_coef = 1.0f;
			crudder_coef = 0.1f;
			//crudder_automanual_coef = 0.2f;
			crudder_automanual_coef = 0.0f;
			//celevator_coef = 1.0f;
			//celevator_coef = 0.15f;
			//celevator_coef = 0.04f;
			celevator_coef = 0.1f;
			//celevator_automanual_coef = 0.3f;
			celevator_automanual_coef = 0.7f;
			//cailerons_coef = 1.0f;
			//cailerons_coef = 0.15f;
			//cailerons_coef = 0.04f;
			cailerons_coef = 0.01f;
			cailerons_automanual_coef = 0.2f;
		
			fwd = gameObject.transform.forward;
			rgt = gameObject.transform.right;
			if (target != null) tgt = target.transform.position - gameObject.transform.position;
			else tgt = -gameObject.transform.position;
			tgt2 = tgt;

			anglezlimit = yawLimit;
			anglezmultiplier = yawMultiplier;
			anglexlimit = pitchLimit;
			anglexmultiplier = pitchMultiplier;

			fwd.y = 0.0f;
			rgt.y = 0.0f;
			tgt.y = 0.0f;
			axz = (tgt2.y > 0.0f) ? Vector3.Angle(tgt, tgt2) : -Vector3.Angle(tgt, tgt2);
			ay = (Vector3.Dot(tgt, rgt) > 0.0f) ? Vector3.Angle(fwd, tgt) : -Vector3.Angle(fwd, tgt);
			//if (ay > 20.0f) ay = 20.0f;
			//if (ay < -20.0f) ay = -20.0f;
		
			angles = gameObject.transform.eulerAngles;
			//angles_speed = Quaternion.LookRotation(gameObject.rigidbody.velocity).eulerAngles;
		
			anglemix = 0.5f;
		
			mouseh = 0.0f;
			mousev = 0.0f;
		
			anglelimit = 15.0f;
			anglemult = 0.5f;
			angles_speed_z = angles.z + mouseh * 140.0f * 3.0f;
			if (angles_speed_z > 180.0f) angles_speed_z = angles_speed_z - 360.0f;
			if (angles_speed_z > anglelimit) angles_speed_z = anglelimit;
			if (angles_speed_z < -anglelimit) angles_speed_z = -anglelimit;
			//Debug.Log (angles.x.ToString() + " // " + angles_speed_x.ToString());
		
			auto_ailerons = angles_speed_z / anglelimit * 0.5f * anglemult + 0.5f;
			auto_ailerons_filtered = auto_ailerons_filtered * (1.0f - auto_ailerons_filter_coeficient) + auto_ailerons * auto_ailerons_filter_coeficient; 
			//aircraft.inputAilerons_internal = auto_ailerons_filtered;
			aircraft.inputAilerons_internal = aircraft.inputAilerons_internal * (1.0f - anglemix) + auto_ailerons_filtered * anglemix;
			Debug.Log(aircraft.inputAilerons_internal.ToString() + " // " + auto_ailerons.ToString() + " // " + angles_speed_z.ToString() + " // " + angles.z.ToString());
		
			anglelimit = 45.0f;
			//anglemult = 0.5f;
			anglemult = 1.5f;
			//angles_speed_x = angles_speed.x;
			angles_speed_x = angles.x + mousev * 140.0f * 2.0f;
			if (angles_speed_x > 180.0f) angles_speed_x = angles_speed_x - 360.0f;
			if (angles_speed_x > anglelimit) angles_speed_x = anglelimit;
			if (angles_speed_x < -anglelimit) angles_speed_x = -anglelimit;
			//Debug.Log (angles.x.ToString() + " // " + angles_speed_x.ToString());

			//aircraft.inputElevator_internal = -angles_speed_x / anglelimit * 0.5f * anglemult + 0.5f;
			//auto_elevator = -angles_speed.x / anglelimit * 0.5f * anglemult + 0.5f;
			//auto_elevator = angles_speed_x / anglelimit * 0.5f * anglemult + 0.5f;
			auto_elevator = angles_speed_x / anglelimit * 0.5f * anglemult + 0.75f;
			auto_elevator_filtered = auto_elevator_filtered * (1.0f - auto_elevator_filter_coeficient) + auto_elevator * auto_elevator_filter_coeficient; 
			//aircraft.inputElevator_internal = auto_elevator_filtered;
			aircraft.inputElevator_internal = aircraft.inputElevator_internal * (1.0f - anglemix) + auto_elevator_filtered * anglemix;
			//Debug.Log(aircraft.inputElevator_internal.ToString() + " // " + auto_elevator.ToString());
		
			aircraft.inputRudder_internal = aircraft.inputRudder_internal * (1.0f - anglemix) + (mouseh * 1.2f + 0.5f) * anglemix;
			//Debug.Log (angles.ToString() + " // " + angles_speed.ToString());
			break;
		case TAircraftControllerType.basic_touch_control:
			mouseh = (Input.mousePosition.x - Screen.width / 2.0f) / Screen.width;
			mousev = (Input.mousePosition.y - Screen.height / 2.0f) / Screen.height;
			//mouseh = mouseh * 2.0f;
			mouseh = mouseh * 2.0f;
			mousev = mousev * 4.0f;
			//if (mouseh > 0.5f) mouseh = 0.5f;
			//if (mouseh < -0.5f) mouseh = -0.5f;

			//if (mouseh > 0.15f) mouseh = 0.15f;
			//if (mouseh < -0.15f) mouseh = -0.15f;
			//if (mousev > 0.5f) mousev = 0.5f;
			//if (mousev < -0.5f) mousev = -0.5f;

			if (mouseh > 0.5f) mouseh = 0.5f;
			if (mouseh < -0.5f) mouseh = -0.5f;
			if (mousev > 0.5f) mousev = 0.5f;
			if (mousev < -0.5f) mousev = -0.5f;
		
			anglemix = 0.5f;
			//aircraft.inputElevator_internal = aircraft.inputElevator_internal * (1.0f - anglemix) + (mousev * 1.0f + 0.5f) * anglemix;
			//aircraft.inputAilerons_internal = aircraft.inputAilerons_internal * (1.0f - anglemix) + (mouseh * 1.0f + 0.5f) * anglemix;
			//aircraft.inputElevator_internal = aircraft.inputElevator_internal * (1.0f - anglemix) + (mousev * 1.0f + 0.5f) * anglemix;
			//aircraft.inputAilerons_internal = aircraft.inputAilerons_internal * (1.0f - anglemix) + (mouseh * 1.0f + 0.5f) * anglemix;
			aircraft.inputElevator_output = aircraft.inputElevator_output * (1.0f - anglemix) + (mousev * 1.0f + 0.5f) * anglemix;
			aircraft.inputAilerons_output = aircraft.inputAilerons_output * (1.0f - anglemix) + (mouseh * 1.0f + 0.5f) * anglemix;
			aircraft.inputThrottle_output = Input.GetMouseButton(0) ? 1.0f : 0.0f;
			break;
		}
		
		if (aircraft.GetComponent("GSCameraAux") != null) {
			GSCameraAux camaux = (GSCameraAux)aircraft.GetComponent("GSCameraAux");
			if (camaux.autopilotmeter != null) {
				camaux.autopilotmeter.text = "AUTOPILOT / AUTOLEVEL / this module is still in alpha state, use with caution!: " + "(" + ay.ToString() + ", " + axz.ToString() + ") " + angles.ToString() + " -- " + auto_ailerons.ToString() + ", " + auto_elevator.ToString() + "; " + auto_rudder_target.ToString() + " == " + angles.y.ToString();
			}
		}
		//Debug.Log("(" + ay.ToString() + ", " + axz.ToString() + ") " + angles.ToString() + " -- " + auto_ailerons.ToString() + ", " + auto_elevator.ToString() + "; " + auto_rudder_target.ToString() + " == " + angles.y.ToString());
	}
}
