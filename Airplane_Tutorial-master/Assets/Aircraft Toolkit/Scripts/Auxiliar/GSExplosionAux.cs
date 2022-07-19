using UnityEngine;
using System.Collections;

public class GSExplosionAux: MonoBehaviour {
	private bool debugEnabled = false;
	private bool alive = false;
	private bool isActive = true;
	private GSGunAux controller = null;
	private GSExplosionAux nextSleeped = null;
	private float totalLifeTime = 0.0f;
	private float lifeTime = -1.0f;
	public AudioSource explosionSound = null;
	public bool ownSound = true;
	[HideInInspector]public float size = 1.0f;
	[HideInInspector]public Vector3 speed = Vector3.zero;
	private Vector3 scale = Vector3.one;
	void FixedUpdate() {
		if (alive) {
			if (lifeTime >= 0.0f) {
				lifeTime -= Time.fixedDeltaTime;
				scale.x = size * lifeTime / totalLifeTime;
				scale.y = size * lifeTime / totalLifeTime;
				scale.z = size * lifeTime / totalLifeTime;
				gameObject.transform.localScale = scale;
				gameObject.transform.position += speed * Time.fixedDeltaTime;
				if (lifeTime < 0.0f) {
					alive = false;
					if (debugEnabled) Debug.Log("GSExplosionAux: explosion timeout");
					if (controller != null) if (controller.OnExplosionDied(this)) {
						gameObject.SetActive(isActive = false);
						return;
					}
					controller = null;
					nextSleeped = null;
					Object.Destroy(this);
				}
			}
		} else {
			if (isActive) {
				gameObject.SetActive(isActive = false);
			}
		}
	}
	public bool explosionWakeUp(GSGunAux controller, float lifeTime, bool debugEnabled) {
		if (alive) return false;
		if (ownSound && (explosionSound == null)) {
			if (gameObject.GetComponent("AudioSource") != null) {
				explosionSound = (AudioSource)gameObject.GetComponent("AudioSource");
			}
		}
		this.debugEnabled = debugEnabled;
		this.alive = true;
		this.totalLifeTime = lifeTime;
		this.lifeTime = lifeTime;
		this.controller = controller;
		gameObject.SetActive(isActive = true);
		if (explosionSound != null) explosionSound.Play();
		return alive;
	}
	public bool explosionSleep(GSExplosionAux nextSleeped) {
		this.nextSleeped = nextSleeped;
		return true;
	}
	public GSExplosionAux explosionNextSleeped() {
		return nextSleeped;
	}
}
