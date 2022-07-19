using UnityEngine;
using System.Collections;

public class GSGunAux: MonoBehaviour {
	public bool debugEnabled = false;
	[HideInInspector]public GAircraft parent = null;
	public GAircraft.TAxisSource inputShotSource = GAircraft.TAxisSource.unity_axis;
	public KeyCode inputShotKey = KeyCode.Return;
	public string inputShotSourceUnityAxis = "Fire1";
	public float inputShotRate = 1.0f;
	private float inputShotRate_value = 1.0f;
	public GSGunAux dualGunCouple = null;
	public AudioSource shotSound = null;
	public bool ownSound = true;
	public bool raycastEnabled = true;
	public float raycastMinDistance = 30.0f;
	public LayerMask raycastLayermask = -24;
	private Rigidbody parentRigidbody = null;
	public GameObject gunBarrel = null;
	public float gunVibration = 250.0f;
	public GameObject gunHalo = null;
	public float gunHaloLifetime = 0.1f;
	public bool shotAfterGunHalo = true;
	private float gunHaloRemainingLifetime = -1.0f;
	private bool gunHaloEnabled = true;
	public GameObject gunYawPivot = null;
	public float gunYawPivotOffset = 0.0f;
	public float gunYawPivotMin = -180.0f;
	public float gunYawPivotMax = 180.0f;
	public float gunYawPivotVelocity = 50.0f;
	public GameObject gunPitchPivot = null;
	public float gunPitchPivotOffset = 0.0f;
	public float gunPitchPivotMin = -180.0f;
	public float gunPitchPivotMax = 180.0f;
	public float gunPitchPivotVelocity = 50.0f;
	private float applyYaw = 0.0f;
	private float currentYaw = 0.0f;
	private float applyPitch = 0.0f;
	private float currentPitch = 0.0f;
	[HideInInspector]public Vector3 targetPosition = Vector3.zero;
	[HideInInspector]public Vector3 targetDirection = Vector3.zero;
	[HideInInspector]public Vector3 targetForwardRight = Vector3.zero;
	[HideInInspector]public Vector3 targetRight = Vector3.zero;
	private Quaternion aircraftRotation;
	private Quaternion gunRotation;
	private Quaternion tmpGunRotation;
	private Vector3 tmpGunRotationEulerAngles;
	
	public GSProjectileAux projectileClass = null;
	public float projectileBornAtDistance = 1.0f;
	public float projectileLifetime = 30.0f;
	public float projectileInitialSpeed = 500.0f;
	public Vector3 projectileAcceleration = Vector3.zero;
	public Vector3 projectileLocalAcceleration = Vector3.zero;
	public float projectileParallelDrag = 0.0f;
	public float projectilePerpendicularDrag = 0.05f;
	private GSProjectileAux firstSleepedProjectile = null;

	public GSExplosionAux explosionClass = null;
	public float explosionLifetime = 5.0f;
	public float explosionSize = 1.0f;
	public float explosionForce = 0.0f;
	public Vector3 explosionSpeed = Vector3.zero;
	private GSExplosionAux firstSleepedExplosion = null;
	
	float angleBetween(Vector3 v1, Vector3 v2, Vector3 worldup) {
		Vector3 crossProduct;
		if (Vector3.Dot(crossProduct = Vector3.Cross(v1, v2), worldup) > 0.0f) {
			return Mathf.Asin(crossProduct.magnitude) * 180.0f / Mathf.PI;
		} else {
			return -Mathf.Asin(crossProduct.magnitude) * 180.0f / Mathf.PI;
		}
	}
	
	private bool startedOk = false;
	void Start() {
		if (gunBarrel == null) gunBarrel = gameObject;
		if (ownSound && (shotSound == null)) {
			if (gameObject.GetComponent("AudioSource") != null) {
				shotSound = (AudioSource)gameObject.GetComponent("AudioSource");
			}
		}
		Transform transform = gameObject.transform;
		int maxLoops = 99;
		while (transform != null) {
			--maxLoops; if (maxLoops < 0) break;
			if (transform.gameObject.GetComponent("GAircraft")) {
				if (parent == null) parent = (GAircraft)transform.gameObject.GetComponent("GAircraft");
			}
			if (transform.gameObject.GetComponent("Rigidbody")) {
				if (parentRigidbody == null) parentRigidbody = (Rigidbody)transform.gameObject.GetComponent("Rigidbody");
			}
			transform = transform.parent;
			if (transform == null) break;
		}
		if (gunHalo != null) gunHalo.SetActive(false);
		startedOk = true;
	}
	
	public bool OnExplosionDied(GSExplosionAux explosion) {
		if (explosion.explosionSleep(firstSleepedExplosion)) {
			firstSleepedExplosion = explosion;
			return true;
		} else {
			return false;
		}
	}
	public bool explosionPlace(Vector3 position, Quaternion rotation, float size, Vector3 speed) {
		GSExplosionAux explosionClone;
		if (firstSleepedExplosion == null) {
			if (debugEnabled) Debug.Log("No more sleeped explosions, instantiating explosion...");
			if (explosionClass != null) explosionClone = (GSExplosionAux)Instantiate(explosionClass);
			else return false;
		} else {
			explosionClone = firstSleepedExplosion;
			firstSleepedExplosion = explosionClone.explosionNextSleeped();
		}
		explosionClone.explosionWakeUp(this, explosionLifetime, debugEnabled);
		explosionClone.transform.position = position;
		explosionClone.transform.rotation = rotation;
		explosionClone.size = size;
		explosionClone.speed = speed;
		GWindBasic.blowSet(position, explosionForce, explosionLifetime, 7777.0f, 1.75f);
		return true;
	}

	public bool OnProjectileImpact(GSProjectileAux projectile, Collision collision) {
		explosionPlace(projectile.gameObject.transform.position, projectile.gameObject.transform.rotation, explosionSize, explosionSpeed);
		if (projectile.projectileSleep(firstSleepedProjectile)) {
			firstSleepedProjectile = projectile;
			return true;
		} else {
			return false;
		}
	}
	public bool OnProjectileDied(GSProjectileAux projectile) {
		if (projectile.projectileSleep(firstSleepedProjectile)) {
			firstSleepedProjectile = projectile;
			return true;
		} else {
			return false;
		}
	}
	public bool projectileShot() {
		if (debugEnabled) Debug.Log("GSGunAux: shot performed");
		gunHaloEnabled = true;
		if (gunHalo != null) {
			gunHalo.SetActive(true);
			gunHaloRemainingLifetime = gunHaloLifetime;
			if (!shotAfterGunHalo) projectileLaunch();
		} else {
			projectileLaunch();
		}
		if (shotSound != null) shotSound.Play();
		return true;
	}
		
	public bool projectileLaunch() {
		GSProjectileAux projectileClone;
		if (firstSleepedProjectile == null) {
			if (debugEnabled) Debug.Log("No more sleeped projectiles, instantiating projectile...");
			if (projectileClass != null) projectileClone = (GSProjectileAux)Instantiate(projectileClass);
			else return false;
		} else {
			projectileClone = firstSleepedProjectile;
			firstSleepedProjectile = projectileClone.projectileNextSleeped();
		}
		projectileClone.projectileWakeUp(this, projectileLifetime, debugEnabled);
		projectileClone.transform.position = gunBarrel.transform.position + gunBarrel.transform.forward * projectileBornAtDistance;
		projectileClone.transform.rotation = gunBarrel.transform.rotation;
		if (parentRigidbody != null) projectileClone.gameObject.GetComponent<Rigidbody>().velocity = parentRigidbody.velocity + gunBarrel.transform.forward * projectileInitialSpeed;
		else projectileClone.gameObject.GetComponent<Rigidbody>().velocity = gunBarrel.transform.forward * projectileInitialSpeed;
		projectileClone.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		projectileClone.acceleration = projectileAcceleration;
		projectileClone.localAcceleration = projectileLocalAcceleration;
		projectileClone.dragParallel = projectileParallelDrag;
		projectileClone.dragPerpendicular = projectilePerpendicularDrag;
		if (parentRigidbody != null) parentRigidbody.AddForceAtPosition(-projectileClone.gameObject.GetComponent<Rigidbody>().velocity * projectileClone.gameObject.GetComponent<Rigidbody>().mass / parentRigidbody.mass * 10000.0f, gunBarrel.transform.position);
		if (parent != null) parent.vibrationSet(gunBarrel.transform.forward, -gunVibration * projectileInitialSpeed * projectileClone.gameObject.GetComponent<Rigidbody>().mass / parentRigidbody.mass, 1.0f, 777f);
		return true;
	}
	
	public bool dualGunCoupleHasBeenFired() {
		if (inputShotRate_value < (1f / inputShotRate) * 0.5f) inputShotRate_value = (1f / inputShotRate) * 0.5f;
		return true;
	}
	
	void FixedUpdate() {
		if (!startedOk) return;
		
		if (gunHaloRemainingLifetime >= 0.0f) {
			gunHaloRemainingLifetime -= Time.fixedDeltaTime;
			if (gunHaloRemainingLifetime < 0.0f) {
				if (gunHaloEnabled && (gunHalo != null)) gunHalo.SetActive(false);
				gunHaloEnabled = false;
				if (shotAfterGunHalo) projectileLaunch();
			}
		}
		
		if ((parent != null) && parent.isCrashed) return;

		if (raycastEnabled) {
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 999999999.9f, raycastLayermask)) {
				if (hit.distance < raycastMinDistance) {
					targetDirection = Vector3.Normalize(Camera.main.ScreenPointToRay(Input.mousePosition).direction);
					targetPosition = gunBarrel.transform.position + targetDirection * 1000.0f;
				} else {
					targetPosition = hit.point;
					targetDirection = Vector3.Normalize(targetPosition - gunBarrel.transform.position);
				}
			} else {
				targetDirection = Vector3.Normalize(Camera.main.ScreenPointToRay(Input.mousePosition).direction);
				targetPosition = gunBarrel.transform.position + targetDirection * 1000.0f;
			}
		} else {
			targetDirection = Vector3.Normalize(Camera.main.ScreenPointToRay(Input.mousePosition).direction);
			targetPosition = gunBarrel.transform.position + targetDirection * 1000.0f;
		}
		targetForwardRight = Vector3.Normalize(targetDirection - Vector3.Dot(targetDirection, gameObject.transform.up) * gameObject.transform.up);
		targetRight = Vector3.Cross(gameObject.transform.up, targetForwardRight);
		
		applyYaw = angleBetween(gameObject.transform.forward, targetForwardRight, gameObject.transform.up);
		if (applyYaw > currentYaw) currentYaw += gunYawPivotVelocity * Time.fixedDeltaTime;
		if (applyYaw < currentYaw) currentYaw -= gunYawPivotVelocity * Time.fixedDeltaTime;
		if (currentYaw < gunYawPivotMin) currentYaw = gunYawPivotMin;
		if (currentYaw > gunYawPivotMax) currentYaw = gunYawPivotMax;

		applyPitch = angleBetween(targetForwardRight, targetDirection, targetRight);
		if (applyPitch > currentPitch) currentPitch += gunPitchPivotVelocity * Time.fixedDeltaTime;
		if (applyPitch < currentPitch) currentPitch -= gunPitchPivotVelocity * Time.fixedDeltaTime;
		if (currentPitch < gunPitchPivotMin) currentPitch = gunPitchPivotMin;
		if (currentPitch > gunPitchPivotMax) currentPitch = gunPitchPivotMax;
		
		if (gunYawPivot != null) gunYawPivot.transform.localEulerAngles = new Vector3(0.0f, currentYaw, 0.0f);
		if (gunPitchPivot != null) gunPitchPivot.transform.localEulerAngles = new Vector3(currentPitch, 0.0f, 0.0f);
		
		bool fired = false;
		switch (inputShotSource) {
		case GAircraft.TAxisSource.keys:
		case GAircraft.TAxisSource.keys_exp:
			fired = Input.GetKey(inputShotKey);
			break;
		case GAircraft.TAxisSource.unity_axis:
		case GAircraft.TAxisSource.unity_axis_exp:
			fired = Input.GetButton(inputShotSourceUnityAxis);
			break;
		case GAircraft.TAxisSource.inv_unity_axis:
		case GAircraft.TAxisSource.inv_unity_axis_exp:
			fired = !Input.GetButton(inputShotSourceUnityAxis);
			break;
		case GAircraft.TAxisSource.mix:
		case GAircraft.TAxisSource.mix_exp:
			fired = Input.GetKey(inputShotKey) || Input.GetButton(inputShotSourceUnityAxis);
			break;
		case GAircraft.TAxisSource.inv_mix:
		case GAircraft.TAxisSource.inv_mix_exp:
			fired = Input.GetKey(inputShotKey) || !Input.GetButton(inputShotSourceUnityAxis);
			break;
		}
		if (fired) {
			if (inputShotRate_value <= 0.0f) {
				inputShotRate_value += (1f / inputShotRate);
				if (dualGunCouple != null) dualGunCouple.dualGunCoupleHasBeenFired();
				projectileShot();
			}
		}
		if (inputShotRate_value > 0.0f) inputShotRate_value -= Time.fixedDeltaTime;
	}
}
