using UnityEngine;

[ExecuteInEditMode()]public class RotatableGUITexture : MonoBehaviour {
	public float angle = 0f;
	public Texture2D texture = null;
	public Color color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	public Rect pixelInset = new Rect(0f, 0f, 128f, 128f);
	public float leftBorder = 0f;
	public float rightBorder = 0f;
	public float topBorder = 0f;
	public float bottomBorder = 0f;
	public bool autoRefresh = true;
	Rect rect;
	Vector2 pivot;
	Color internalcolor = Color.black;
	Matrix4x4 matrixBackup;
	Color colorBackup;
	int depthBackup;
	
	void Start() {
		refresh();
	}
	
	public void refresh() {
		rect = new Rect(Screen.width * transform.localPosition.x + pixelInset.x, Screen.height * (1f - transform.localPosition.y) - pixelInset.y - pixelInset.height, pixelInset.width, pixelInset.height);
		pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
	}
	
	void OnGUI() {
		if (autoRefresh) refresh();
		else if (Application.isEditor) refresh();
		
		matrixBackup = GUI.matrix;
		colorBackup = GUI.color;
		depthBackup = GUI.depth;
		
		GUIUtility.RotateAroundPivot(angle, pivot);
		GUI.color = color;
		internalcolor.r = color.r * 2f;
		internalcolor.g = color.g * 2f;
		internalcolor.b = color.b * 2f;
		internalcolor.a = color.a * 2f;
		GUI.color = internalcolor;
		GUI.depth = Mathf.FloorToInt(transform.localPosition.z);
		GUI.DrawTexture(rect, texture);
		
		GUI.matrix = matrixBackup;
		GUI.color = colorBackup;
		GUI.depth = depthBackup;
	}
}