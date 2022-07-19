using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]public class GSProjectileAux: MonoBehaviour {
	private bool debugEnabled = false;
	private bool alive = false;
	private bool isActive = true;
	private GSGunAux controller = null;
	private GSProjectileAux nextSleeped = null;
	private float lifeTime = -1.0f;
	[HideInInspector]public Vector3 acceleration = Vector3.zero;
	[HideInInspector]public Vector3 localAcceleration = Vector3.zero;
	[HideInInspector]public float dragParallel = 0.0f;
	[HideInInspector]public float dragPerpendicular = 0.01f;
	private Vector3 velocity = Vector3.zero;
	private Vector3 velocityProjection = Vector3.zero;
	private Vector3 velocityPerpendicular = Vector3.zero;
	public bool raycastImprovedCollision = true;
	public float raycastImprovedCollisionInitialDistance = -1f;
	public float raycastImprovedCollisionFinalDistance = -1f;
	public int raycastImprovedCollisionNthFrames = 5;
	private int raycastImprovedCollisionCurrentFrame = 0;
	public LayerMask raycastImprovedCollisionLayermask = -1;
	private RaycastHit raycastImprovedCollisionHit;
	public bool raycastImprovedCollisionApplyAtPosition = true;
	private Vector3 raycastImprovedCollisionApplyPosition = Vector3.zero;
	public bool raycastImprovedCollisionApplyDirectly = true;
	//public float raycastImprovedCollisionDetectedIn = -1f;
	public bool raycastImprovedCollisionProjection = true;
	private bool raycastImprovedCollisionProjected = false;
	private Vector3 raycastImprovedCollisionProjectionVector = Vector3.zero;
	

	private Vector3 calcXScale_xp = new Vector3(0.5f, 0.0f, 0.0f);
	private Vector3 calcXScale_xn = new Vector3(-0.5f, 0.0f, 0.0f);
	private Vector3 calcYScale_yp = new Vector3(0.0f, 0.5f, 0.0f);
	private Vector3 calcYScale_yn = new Vector3(0.0f, -0.5f, 0.0f);
	private Vector3 calcZScale_zp = new Vector3(0.0f, 0.0f, 0.5f);
	private Vector3 calcZScale_zn = new Vector3(0.0f, 0.0f, -0.5f);
	public float calcXScale(GameObject obj) {
		return (obj.transform.TransformPoint(calcXScale_xp) - obj.transform.TransformPoint(calcXScale_xn)).magnitude;
	}
	public float calcYScale(GameObject obj) {
		return (obj.transform.TransformPoint(calcYScale_yp) - obj.transform.TransformPoint(calcYScale_yn)).magnitude;
	}
	public float calcZScale(GameObject obj) {
		return (obj.transform.TransformPoint(calcZScale_zp) - obj.transform.TransformPoint(calcZScale_zn)).magnitude;
	}
	void OnCollisionEnter(Collision collision) {
		if (alive) {
			alive = false;
			if (debugEnabled) Debug.Log("GSProjectileAux: projectile impact!");
			if (controller != null) if (controller.OnProjectileImpact(this, collision)) {
				gameObject.SetActive(isActive = false);
				return;
			}
			controller = null;
			nextSleeped = null;
			Object.Destroy(this);
		}
	}
	void FixedUpdate() {
		if (alive) {
			if (lifeTime >= 0.0f) {
				lifeTime -= Time.fixedDeltaTime;
				if (acceleration.magnitude > 0.0f) GetComponent<Rigidbody>().AddForce(acceleration);
				if (localAcceleration.magnitude > 0.0f) GetComponent<Rigidbody>().AddRelativeForce(localAcceleration);
				if ((dragParallel > 0.0f) || (dragPerpendicular > 0.0f)) {
					velocity = GetComponent<Rigidbody>().velocity;
					velocityProjection = Vector3.Dot(velocity, gameObject.transform.forward) * gameObject.transform.forward;
					velocityPerpendicular = velocity - velocityProjection;
					if (dragParallel > 0.0f) velocityProjection = velocityProjection * (1f - Mathf.Clamp01(dragParallel * Time.fixedDeltaTime));
					if (dragPerpendicular > 0.0f) velocityPerpendicular = velocityPerpendicular * (1f - Mathf.Clamp01(dragPerpendicular * Time.fixedDeltaTime));
					GetComponent<Rigidbody>().velocity = velocityProjection + velocityPerpendicular;
				}
				if (lifeTime < 0.0f) {
					alive = false;
					if (debugEnabled) Debug.Log("GSProjectileAux: projectile timeout");
					if (controller != null) if (controller.OnProjectileDied(this)) {
						gameObject.SetActive(isActive = false);
						return;
					}
					controller = null;
					nextSleeped = null;
					Object.Destroy(this);
				}
				if (raycastImprovedCollision) {
					if (raycastImprovedCollisionProjected) {
						float projectedValue = Vector3.Dot(gameObject.transform.position - raycastImprovedCollisionApplyPosition, raycastImprovedCollisionProjectionVector);
						if (projectedValue >= 0f) {
							if (raycastImprovedCollisionApplyAtPosition) gameObject.transform.position = raycastImprovedCollisionApplyPosition;
							OnCollisionEnter(null);
						}
					}
					++raycastImprovedCollisionCurrentFrame;
					if (raycastImprovedCollisionCurrentFrame >= raycastImprovedCollisionNthFrames) {
						raycastImprovedCollisionCurrentFrame = 0;
						if (raycastImprovedCollisionInitialDistance < 0f) {
							float calcScale;
							raycastImprovedCollisionInitialDistance = 0f;
							if ((calcScale = calcXScale(gameObject)) > raycastImprovedCollisionInitialDistance) raycastImprovedCollisionInitialDistance = calcScale;
							if ((calcScale = calcYScale(gameObject)) > raycastImprovedCollisionInitialDistance) raycastImprovedCollisionInitialDistance = calcScale;
							if ((calcScale = calcZScale(gameObject)) > raycastImprovedCollisionInitialDistance) raycastImprovedCollisionInitialDistance = calcScale;
						}
						if (raycastImprovedCollisionFinalDistance < 0f) {
							raycastImprovedCollisionFinalDistance = gameObject.GetComponent<Rigidbody>().velocity.magnitude * Time.fixedDeltaTime * raycastImprovedCollisionNthFrames * 100.0f;
						}
						if (Physics.Raycast(gameObject.transform.position + gameObject.transform.forward * raycastImprovedCollisionInitialDistance, gameObject.transform.forward, out raycastImprovedCollisionHit, raycastImprovedCollisionFinalDistance, raycastImprovedCollisionLayermask)) {
							if (raycastImprovedCollisionApplyDirectly) {
								gameObject.transform.position = raycastImprovedCollisionHit.point;
								OnCollisionEnter(null);
							} else {
								raycastImprovedCollisionApplyPosition = raycastImprovedCollisionHit.point;
								raycastImprovedCollisionProjectionVector = gameObject.transform.forward;
								raycastImprovedCollisionProjected = true;
							}
						}
					}
				}
				/*
				if (raycastImprovedCollision) {
					if (raycastImprovedCollisionProjected) {
						float projectedValue = Vector3.Dot(gameObject.transform.position - raycastImprovedCollisionApplyPosition, raycastImprovedCollisionProjectionVector);
						if (projectedValue >= 0f) {
							if (raycastImprovedCollisionApplyAtPosition) gameObject.transform.position = raycastImprovedCollisionApplyPosition;
							OnCollisionEnter(null);
						}
					} else if (raycastImprovedCollisionDetectedIn < 0f) {
						++raycastImprovedCollisionCurrentFrame;
						if (raycastImprovedCollisionCurrentFrame >= raycastImprovedCollisionNthFrames) {
							raycastImprovedCollisionCurrentFrame = 0;
							if (raycastImprovedCollisionInitialDistance < 0f) {
								float calcScale;
								raycastImprovedCollisionInitialDistance = 0f;
								if ((calcScale = calcXScale(gameObject)) > raycastImprovedCollisionInitialDistance) raycastImprovedCollisionInitialDistance = calcScale;
								if ((calcScale = calcYScale(gameObject)) > raycastImprovedCollisionInitialDistance) raycastImprovedCollisionInitialDistance = calcScale;
								if ((calcScale = calcZScale(gameObject)) > raycastImprovedCollisionInitialDistance) raycastImprovedCollisionInitialDistance = calcScale;
							}
							if (raycastImprovedCollisionFinalDistance < 0f) {
								raycastImprovedCollisionFinalDistance = gameObject.rigidbody.velocity.magnitude * Time.fixedDeltaTime * raycastImprovedCollisionNthFrames * 100.0f;
							}
							if (Physics.Raycast(gameObject.transform.position + gameObject.transform.forward * raycastImprovedCollisionInitialDistance, gameObject.transform.forward, out raycastImprovedCollisionHit, raycastImprovedCollisionFinalDistance, raycastImprovedCollisionLayermask)) {
								if (raycastImprovedCollisionApplyDirectly) {
									gameObject.transform.position = raycastImprovedCollisionHit.point;
									OnCollisionEnter(null);
								} else {
									raycastImprovedCollisionDetectedIn = (raycastImprovedCollisionHit.point - gameObject.transform.position).magnitude / gameObject.rigidbody.velocity.magnitude;
									//Debug.Log("GSProjectileAux: raycastImprovedCollision detected in " + raycastImprovedCollisionDetectedIn.ToString() + "s");
									raycastImprovedCollisionApplyPosition = raycastImprovedCollisionHit.point;
									raycastImprovedCollisionProjectionVector = gameObject.transform.forward;
									if (raycastImprovedCollisionProjection) raycastImprovedCollisionProjected = true;
								}
							}
						}
					} else {
						raycastImprovedCollisionDetectedIn -= Time.fixedDeltaTime;
						if (raycastImprovedCollisionDetectedIn <= 0f) {
							if (raycastImprovedCollisionApplyAtPosition) gameObject.transform.position = raycastImprovedCollisionApplyPosition;
							OnCollisionEnter(null);
						}
					}
				}
				*/
			}
		} else {
			if (isActive) {
				gameObject.SetActive(isActive = false);
			}
		}
	}
	public bool projectileWakeUp(GSGunAux controller, float lifeTime, bool debugEnabled) {
		if (alive) return false;
		this.debugEnabled = debugEnabled;
		this.alive = true;
		this.lifeTime = lifeTime;
		this.controller = controller;
		this.raycastImprovedCollisionProjected = false;
		gameObject.SetActive(isActive = true);
		return alive;
	}
	public bool projectileSleep(GSProjectileAux nextSleeped) {
		this.nextSleeped = nextSleeped;
		return true;
	}
	public GSProjectileAux projectileNextSleeped() {
		return nextSleeped;
	}
}
