using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GSCursorAux: MonoBehaviour {
	public enum TCursorMode { deltaPosition, deltaVelocity };
	public enum TRotationMode { none, pivotUp, worldUp };
	public TCursorMode cursorMode = TCursorMode.deltaPosition;
	public bool renderBack = false;
	public GameObject pivotGameobject = null;
	Rigidbody pivotRigidbody;
	public Vector3 localDeltaPosition = Vector3.zero;
	public Vector3 deltaPosition = Vector3.zero;
	public bool deltaPositionYaw = true;
	public float velocityDistance = 1.0f;
	public float velocityMin = 0.25f;
	[HideInInspector]public float angleCalculatorDeltaElementUpDistance = 100.0f;
	[HideInInspector]public float angleCalculatorDeltaElementForwardDistance = 0.0f;
	public Image targetGUITexture = null;
	public RotatableGUITexture targetRotatableGUITexture = null;
	public TRotationMode rotationMode = TRotationMode.pivotUp;
	GameObject pivot = null;
	Vector3 worldPosition = Vector3.zero;
	Vector3 screenPoint = Vector3.zero;
	Vector3 screenPoint2 = Vector3.zero;
	Vector3 screenPointD = Vector3.zero;
	Vector3 velocity = Vector3.zero;
	Quaternion yawRotation = Quaternion.identity;

	float angleBetween(Vector3 v1, Vector3 v2, Vector3 worldup) {
		Vector3 crossProduct;
		if (Vector3.Dot(crossProduct = Vector3.Cross(Vector3.Normalize(v1), Vector3.Normalize(v2)), Vector3.Normalize(worldup)) > 0.0f) {
			return Mathf.Asin(crossProduct.magnitude) * 180.0f / Mathf.PI;
		} else {
			return -Mathf.Asin(crossProduct.magnitude) * 180.0f / Mathf.PI;
		}
	}
	
	float angle2D(float x, float y) {
		if (x > 0f) {
			return Mathf.Atan(y / x) * 180f / Mathf.PI;
		} else if (x < 0f) {
			return (Mathf.PI + Mathf.Atan(y / x)) * 180f / Mathf.PI;
		} else {
			if (y > 0f) return 90f;
			else return -90f;
		}
	}
	
	void FixedUpdate() {
		pivot = pivotGameobject;
		if (pivot == null) pivot = this.gameObject;
		if (deltaPositionYaw) {
			yawRotation = Quaternion.Euler(0f, pivot.transform.rotation.eulerAngles.y, 0f);
			worldPosition = pivot.transform.TransformPoint(localDeltaPosition) + yawRotation * deltaPosition;
		} else {
			worldPosition = pivot.transform.TransformPoint(localDeltaPosition) + deltaPosition;
		}
		switch (cursorMode) {
		default:
		case TCursorMode.deltaPosition:
			screenPoint = Camera.main.WorldToViewportPoint(worldPosition);
			break;
		case TCursorMode.deltaVelocity:
			if (pivotGameobject != null) {
				pivotRigidbody = (Rigidbody)pivotGameobject.GetComponent("Rigidbody");
				if (pivotRigidbody != null) {
					velocity = pivotRigidbody.velocity;
					if (velocity.magnitude < velocityMin) velocity = pivotGameobject.transform.forward;
				} else {
					velocity = pivotGameobject.transform.forward;
				}
				screenPoint = Camera.main.WorldToViewportPoint(worldPosition + Vector3.Normalize(velocity) * velocityDistance);
			} else {
				screenPoint = Camera.main.WorldToViewportPoint(worldPosition + Vector3.forward);
				gameObject.SetActive(false);
			}
			break;
		}
		if (targetGUITexture != null) {
			if ((!renderBack) && (screenPoint.z < 0f)) {
				screenPoint.x = 10.0f;
				screenPoint.y = 10.0f;
			}
			screenPoint.z = targetGUITexture.transform.localPosition.z;
			targetGUITexture.transform.localPosition = screenPoint;
		}
		if (targetRotatableGUITexture != null) {
			if ((!renderBack) && (screenPoint.z < 0f)) {
				screenPoint.x = 10.0f;
				screenPoint.y = 10.0f;
			}
			screenPoint.z = targetRotatableGUITexture.transform.localPosition.z;
			switch (rotationMode) {
			case TRotationMode.pivotUp:
			case TRotationMode.worldUp:
				switch (rotationMode) {
				case TRotationMode.pivotUp:
					if (pivotGameobject != null) screenPoint2 = Camera.main.WorldToViewportPoint(worldPosition + pivotGameobject.transform.up * angleCalculatorDeltaElementUpDistance + pivotGameobject.transform.forward * angleCalculatorDeltaElementForwardDistance);
					else gameObject.SetActive(false);
					break;
				case TRotationMode.worldUp:
					screenPoint2 = Camera.main.WorldToViewportPoint(worldPosition + Vector3.up * angleCalculatorDeltaElementUpDistance + pivotGameobject.transform.forward * angleCalculatorDeltaElementForwardDistance);
					break;
				}
				screenPointD = screenPoint2 - screenPoint;
				screenPointD.z = 0f;
				targetRotatableGUITexture.angle = -angle2D(screenPointD.x, screenPointD.y) + 90f;
				break;
			}
			targetRotatableGUITexture.transform.localPosition = screenPoint;
		}
	}
}
