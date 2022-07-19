using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GSCameraAux: MonoBehaviour {
	
	GAircraft sm = null;
	public Camera attachToCamera = null;
	Image crossair = null;
	public Light sunlight = null;
	public bool sunlightAttach = true;
	public float sunlightAttachDistance = 100.0f;
	public float sunlightAttachFilter = 0.01f;
	public float inputHorizontalMouseSensivity = 3.4f;
	public float inputVerticalMouseSensivity = 2.1f;
	public float inputWheelMouseSensivity = 2.1f;
	public KeyCode inputExternalCameraKeyForToggle = KeyCode.C;
	private bool inputExternalCameraKeyForToggled = false;
	private int currentCameraPosition = 0;
	private int countCameraPositions = 0;
	public KeyCode inputFixedPositionCameraKeyForToggle = KeyCode.V;
	private bool inputFixedPositionCameraKeyForToggled = false;
	public bool enableRecoverVehicle = true;
	public bool enableRecoverVehicleReset = false;
	public Vector3 enableRecoverVehiclePosition = Vector3.zero;
	public Vector3 enableRecoverVehicleRotation = Vector3.zero;
	public KeyCode inputRecoverVehicleKeyForToggle = KeyCode.O;
	private bool inputRecoverVehicleKeyForToggled = false;
	private bool cameraFixedPositionEnabled = false;
	private bool cameraExternalEnabled_lastValue = true;
	private bool cameraOnFixedUpdate = true;
	private int cameraOnFixedUpdate_count = 0;
	public GameObject cameraAlternateFollow = null;
	public bool globalSimulationScaleApplyToCamera = true;
	private float globalSimulationScale = 1.0f;
	public float cameraExternalMinHeight = -93.0f;
	public bool cameraExternalMinHeightAutoProbe = true;
	public LayerMask cameraExternalMinHeightAutoProbeLayermask = -24;
	private float cameraExternalMinHeightScale = 1.0f;
	public float cameraExternalMinHeightAutoProbeDelta = 5.0f;
	public float cameraExternalMinHeightAutoProbeDeltaProbe = 1000.0f;
	public bool cameraExternalEnabled = false;
	public float cameraExternalNearClipPlane = 2.0f;
	public float cameraExternalFarClipPlane = 8000.0f;
	public float cameraExternalFilter = 0.1f;
	private float cameraExternalDistance = 2.0f;
	public float cameraExternalDistanceMin = 0.4f;
	public float cameraExternalDistanceMax = 101.0f;
	public float cameraExternalDistanceStep = 1.25f;
	public bool cameraExternalUpWorld = true;
	public float cameraInternalSensivity = 1.35f;
	public float cameraInternalNearClipPlane = 0.175f;
	public float cameraInternalFarClipPlane = 5000.0f;
	public float cameraInternalFilter = 1.0f;
	private float cameraInternalFov = 60.0f;
	public float cameraInternalFovMin = 15.0f;
	public float cameraInternalFovMax = 110.0f;
	public float cameraInternalFovStep = 15.0f;
	public bool cameraInternalUpWorld = false;
	public bool cameraInternalUpWorldMix = true;
	public float cameraInternalUpWorldMixing = 0.5f;
	public bool cameraInternalDontVertical = false;
	public bool cameraInternalDontVerticalMix = true;
	public float cameraInternalDontVerticalMixing = 0.5f;
	public float cameraInternalGsMixing = 0.05f;
	public float cameraInternalGsMax = 0.7f;
	public bool cameraInternalGsPosition = true;
	public float cameraInternalGsPositionDelta = 0.075f;
	public float cameraInternalGsPositionHDelta = 0.1f;
	public bool cameraInternalGsRotation = true;
	public float cameraInternalGsRotationDelta = 0.2f;
	public bool enableSpecialForce = true;
	public float specialForceMagnitude = 10000.0f;
	public KeyCode specialForceApplyZeroRotate = KeyCode.U;
	public KeyCode specialForceApplyLeft = KeyCode.J;
	public KeyCode specialForceApplyRight = KeyCode.L;
	public KeyCode specialForceApplyForward = KeyCode.I;
	public KeyCode specialForceApplyForceBack = KeyCode.K;
	public KeyCode specialForceApplyForceUp = KeyCode.Y;
	public KeyCode specialForceApplyDown = KeyCode.H;
	public bool fogRenderingEnabled = false;
	private FogMode initialFogMode = FogMode.Linear;
	private float initialFogValue = 0.0f;
	public float fog01 = 0.0f;
	public float extremeFogValue = -1.0f;
	public bool skyboxRenderingEnabled = false;
	private Color skyboxRenderingColor;
	private Color skyboxRenderingColorTop;
	public float skyboxRenderingColorTopDeltaHeight = 10000.0f;
	public float skyboxRenderingColorUpwardsScatteringDistance = 30000.0f;
	public float skyboxRenderingColorDownwardsScatteringDistance = 3000000.0f;
	public static Color skyboxRenderingColor0Default { get { return new Color(105.0f / 256.0f, 105.0f / 256.0f, 105.0f / 256.0f, 1f); } }
	public static Color skyboxRenderingColor1Default { get { return new Color(125.0f / 256.0f, 175.0f / 256.0f, 255.0f / 256.0f, 1f); } }
	public static Color skyboxRenderingColor2Default { get { return new Color(49.0f / 256.0f, 77.0f / 256.0f, 121.0f / 256.0f, 1f); } }
	public static Color skyboxRenderingColor3Default { get { return new Color(0.0f, 0.0f, 0.0f, 1f); } }
	public bool skyboxRenderingColor0Enabled = false;
	public Color skyboxRenderingColor0 = skyboxRenderingColor0Default;
	public float skyboxRenderingColor0Height = 0.0f;
	public float skyboxRenderingColor0Distance = 1000.0f;
	public Color skyboxRenderingColor1 = skyboxRenderingColor1Default;
	public float skyboxRenderingColor1Height = 1500.0f;
	public Color skyboxRenderingColor2 = skyboxRenderingColor2Default;
	public float skyboxRenderingColor2Height = 15000.0f;
	public Color skyboxRenderingColor3 = skyboxRenderingColor3Default;
	public float skyboxRenderingColor3Height = 45000.0f;
	[System.NonSerializedAttribute]public int skyboxAngleColors;
	[System.NonSerializedAttribute]public Color32[] skyboxAngleColor = null;
	[System.NonSerializedAttribute]public int[,] skyboxSideAngles;
	[System.NonSerializedAttribute]public int[,] skyboxTopAngles;
	[System.NonSerializedAttribute]public int[,] skyboxBottomAngles;
	private Color tmp_skyboxRenderingColorm1;
	private Color tmp_skyboxRenderingColor0;
	private Color tmp_skyboxRenderingColor1;
	private Color tmp_skyboxRenderingColor2;
	private Color tmp_skyboxRenderingColor3;
	private Color tmp_skyboxRenderingColor4;
	public int skyboxTextureWidth = 64;
	public int skyboxTextureHeight = 64;
	public int skyboxRenderNth = 5;
	public int skyboxRenderSidesPerNth = 2;
	private int skyboxRenderNth_count = 0;
	private int skyboxRenderNth_tex = 0;
	private Skybox skyboxComponent = null;
	private Material skyboxMaterial = null;
	private Texture2D _FrontTex = null;
	private Texture2D _LeftTex = null;
	private Texture2D _RightTex = null;
	private Texture2D _BackTex = null;
	private Texture2D _UpTex = null;
	private Texture2D _DownTex = null;
	[System.NonSerializedAttribute]private Color32[] pixels = null;

	public bool searchSounds = true;
	[HideInInspector]public Text speedmeter;
	[HideInInspector]public Text autopilotmeter;
	[HideInInspector]public GameObject scene;
	[HideInInspector]public GameObject ground;
	[HideInInspector]public GameObject clouds;
	
	private AudioSource[] soundmin = null;
	private string soundmin_pivotId = "";
	private float soundmin_pivotScale = 1.0f;
	private float soundmin_pitchBase = 1.0f;
	private float soundmin_pitchScale = 1.0f;
	private int soundmin_count;
	private int soundmin_countmax = 10;
	private AudioSource[] soundmax = null;
	private string soundmax_pivotId = "";
	private float soundmax_pivotScale = 1.0f;
	private float soundmax_pitchBase = 0.1f;
	private float soundmax_pitchScale = 1.0f;
	private int soundmax_count;
	private int soundmax_countmax = 10;
	private AudioSource[] soundstall = null;
	private int soundstall_count;
	private int soundstall_countmax = 10;
	private bool iscrashed = false;
	private AudioSource[] soundcrash = null;
	private int soundcrash_count;
	private int soundcrash_countmax = 10;
	
	public string searchScreenCursorObjectName = "Screen cursor";
	public string searchScreenSpeedMeterObjectName = "Screen speed meter";
	public string searchScreenAutopilotMeterObjectName = "Screen autopilot meter";
	public string searchSceneObjectName = "Scene";
	public float searchSceneObjectGranularity = 1500.0f;
	public string searchGroundObjectName = "Scene ground";
	public float searchGroundObjectGranularity = 150.0f;
	public string searchCloudsObjectName = "Scene clouds";
	public float searchCloudsObjectGranularity = 1500.0f;
	public float searchCloudsWindSpeedMultiplier = 5.0f;
	
	private Vector3 worldup = Vector3.up;
	private Vector3 cameraposition = Vector3.zero;
	private Quaternion cameraorientation = Quaternion.identity;
	private bool camerafixed = false;
	private Vector3 cameralookat = Vector3.zero;
	private Vector3 cameraup = Vector3.up;

	private Vector3 cameraposition_filtered = Vector3.zero;
	private Vector3 cameralookat_filtered = Vector3.zero;
	private Vector3 cameraup_filtered = Vector3.up;
	private float camera_filter = 0.01f;
	
	int scheduleRecoverVehicle = 0;
	public bool recoverVehicle() {
		if (sm != null) {
			sm.isCrashed = false;
			if (enableRecoverVehicleReset) {
				sm.placeAt(enableRecoverVehiclePosition, enableRecoverVehicleRotation, true, 0.0f * sm.globalSimulationScale, true, 0.0f);
			} else {
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					sm.placeAt(gameObject.transform.position + new Vector3(0.0f, 500.0f * sm.globalSimulationScale, 0.0f), new Vector3(-30.0f, gameObject.transform.eulerAngles.y, 0.0f), 100.0f * sm.globalSimulationScale);
				} else {
					sm.placeAt(gameObject.transform.position + new Vector3(0.0f, 50.0f * sm.globalSimulationScale, 0.0f), new Vector3(-30.0f, gameObject.transform.eulerAngles.y, 0.0f), 100.0f * sm.globalSimulationScale);
				}
			}
			sm.ignore_check_simulation_broken();
		}
		return true;
	}

	void FindNodes(GameObject e, int r) {
		GameObject ch;
		for (int i = 0; i < e.transform.childCount; ++i) {
			ch = e.transform.GetChild(i).gameObject;
			if (ch.activeSelf) {
				if (searchSounds) {
					if (ch.name.Contains("_soundmin_")) {
						if (soundmin_count < soundmin_countmax) {
							soundmin[soundmin_count] = (AudioSource)ch.GetComponent("AudioSource");
							if (soundmin[soundmin_count] != null) {
								soundmin[soundmin_count].loop = true;
								soundmin[soundmin_count].volume = 0;
								soundmin_pivotId = GAircraft.ReplaceStringIgnoreCase(ch.name, "pivotId", soundmin_pivotId);
								soundmin_pivotScale = GAircraft.ReplaceFloatIgnoreCase(ch.name, "pivotScale", soundmin_pivotScale);
								soundmin_pitchBase = GAircraft.ReplaceFloatIgnoreCase(ch.name, "pitchBase", soundmin_pitchBase);
								soundmin_pitchScale = GAircraft.ReplaceFloatIgnoreCase(ch.name, "pitchScale", soundmin_pitchScale);
								soundmin[soundmin_count].Play((ulong)Mathf.FloorToInt(soundmin_count * 1.7f * 44100.0f));
								++soundmin_count;
							}
						}
					}
					if (ch.name.Contains("_soundmax_")) {
						if (soundmax_count < soundmax_countmax) {
							soundmax[soundmax_count] = (AudioSource)ch.GetComponent("AudioSource");
							if (soundmax[soundmax_count] != null) {
								soundmax[soundmax_count].loop = true;
								soundmax[soundmax_count].volume = 0;
								soundmax_pivotId = GAircraft.ReplaceStringIgnoreCase(ch.name, "pivotId", soundmax_pivotId);
								soundmax_pivotScale = GAircraft.ReplaceFloatIgnoreCase(ch.name, "pivotScale", soundmax_pivotScale);
								soundmax_pitchBase = GAircraft.ReplaceFloatIgnoreCase(ch.name, "pitchBase", soundmax_pitchBase);
								soundmax_pitchScale = GAircraft.ReplaceFloatIgnoreCase(ch.name, "pitchScale", soundmax_pitchScale);
								soundmax[soundmax_count].Play((ulong)Mathf.FloorToInt(soundmax_count * 1.9f * 44100.0f));
								++soundmax_count;
							}
						}
					}
					if (ch.name.Contains("_soundstall_")) {
						if (soundstall_count < soundstall_countmax) {
							soundstall[soundstall_count] = (AudioSource)ch.GetComponent("AudioSource");
							if (soundstall[soundstall_count] != null) {
								soundstall[soundstall_count].loop = true;
								soundstall[soundstall_count].volume = 0;
								soundstall[soundstall_count].Play((ulong)Mathf.FloorToInt(soundstall_count * 1.9f * 44100.0f));
								++soundstall_count;
							}
						}
					}
					if (ch.name.Contains("_soundcrash_")) {
						if (soundcrash_count < soundcrash_countmax) {
							soundcrash[soundcrash_count] = (AudioSource)ch.GetComponent("AudioSource");
							if (soundcrash[soundcrash_count] != null) {
								soundcrash[soundcrash_count].loop = false;
								soundcrash[soundcrash_count].volume = 0;
								++soundcrash_count;
							}
						}
					}
				}
	
				if (r < 10) FindNodes(ch, r + 1);
			}
		}
	}
	
	void Skybox_Start() {
		if (skyboxComponent == null) skyboxComponent = (Skybox)((attachToCamera == null) ? Camera.main : attachToCamera).gameObject.GetComponent("Skybox");
		if (skyboxComponent == null) (attachToCamera ?? Camera.main).gameObject.AddComponent<Skybox>();
		if (skyboxComponent == null) skyboxComponent = (Skybox)((attachToCamera == null) ? Camera.main : attachToCamera).gameObject.GetComponent("Skybox");
		if (skyboxComponent != null) {
			if (skyboxMaterial == null) skyboxMaterial = new Material(Shader.Find("RenderFX/Skybox"));
			if (skyboxMaterial != null) {
				int w = skyboxTextureWidth;
				int h = skyboxTextureHeight;
				if (_FrontTex == null) _FrontTex = new Texture2D(w, h, TextureFormat.ARGB32, false);
				if (_LeftTex == null) _LeftTex = new Texture2D(w, h, TextureFormat.ARGB32, false);
				if (_RightTex == null) _RightTex = new Texture2D(w, h, TextureFormat.ARGB32, false);
				if (_BackTex == null) _BackTex = new Texture2D(w, h, TextureFormat.ARGB32, false);
				if (_UpTex == null) _UpTex = new Texture2D(w, h, TextureFormat.ARGB32, false);
				if (_DownTex == null) _DownTex = new Texture2D(w, h, TextureFormat.ARGB32, false);
				_FrontTex.wrapMode = TextureWrapMode.Clamp;
				_LeftTex.wrapMode = TextureWrapMode.Clamp;
				_RightTex.wrapMode = TextureWrapMode.Clamp;
				_BackTex.wrapMode = TextureWrapMode.Clamp;
				_UpTex.wrapMode = TextureWrapMode.Clamp;
				_DownTex.wrapMode = TextureWrapMode.Clamp;
				skyboxMaterial.SetTexture("_FrontTex", _FrontTex);
				skyboxMaterial.SetTexture("_LeftTex", _LeftTex);
				skyboxMaterial.SetTexture("_RightTex", _RightTex);
				skyboxMaterial.SetTexture("_BackTex", _BackTex);
				skyboxMaterial.SetTexture("_UpTex", _UpTex);
				skyboxMaterial.SetTexture("_DownTex", _DownTex);
				if (pixels == null) pixels = new Color32[w * h];
				skyboxAngleColors = Mathf.FloorToInt(Mathf.Sqrt(w * w + h * h) + Mathf.Max(w, h)) * 4 + 1;
				if (skyboxAngleColor == null) skyboxAngleColor = new Color32[skyboxAngleColors];
				if (skyboxTopAngles == null) skyboxTopAngles = new int[w, h];
				if (skyboxSideAngles == null) skyboxSideAngles = new int[w, h];
				if (skyboxBottomAngles == null) skyboxBottomAngles = new int[w, h];
				float topangle = 0.0f;
				float sideangle = 0.0f;
				float bottomangle = 0.0f;
				float x;
				float y;
				for (int i = 0; i < w; ++i) {
					for (int j = 0; j < h; ++j) {
						x = (i - w / 2.0f) * 2.0f / w;
						y = (j - h / 2.0f) * 2.0f / h;
						pixels[i + j * w].a = 255;
						topangle = Mathf.Atan(+1.0f / Mathf.Sqrt(x * x + y * y));
						skyboxTopAngles[i, j] = Mathf.RoundToInt(skyboxAngleColors * ((topangle + Mathf.PI / 2.0f) / Mathf.PI));
						if (skyboxTopAngles[i, j] < 0) skyboxTopAngles[i, j] = 0;
						if (skyboxTopAngles[i, j] >= skyboxAngleColors) skyboxTopAngles[i, j] = skyboxAngleColors - 1;
						sideangle = Mathf.Atan(y / Mathf.Sqrt(1.0f + x * x));
						skyboxSideAngles[i, j] = Mathf.RoundToInt(skyboxAngleColors * ((sideangle + Mathf.PI / 2.0f) / Mathf.PI));
						if (skyboxSideAngles[i, j] < 0) skyboxSideAngles[i, j] = 0;
						if (skyboxSideAngles[i, j] >= skyboxAngleColors) skyboxSideAngles[i, j] = skyboxAngleColors - 1;
						bottomangle = Mathf.Atan(-1.0f / Mathf.Sqrt(x * x + y * y));
						skyboxBottomAngles[i, j] = Mathf.RoundToInt(skyboxAngleColors * ((bottomangle + Mathf.PI / 2.0f) / Mathf.PI));
						if (skyboxBottomAngles[i, j] < 0) skyboxBottomAngles[i, j] = 0;
						if (skyboxBottomAngles[i, j] >= skyboxAngleColors) skyboxBottomAngles[i, j] = skyboxAngleColors - 1;
					}
				}
			} else {
				Debug.Log("Failed to create material from shader RenderFX/Skybox");
			}
			skyboxComponent.material = skyboxMaterial;
		}
	}
	
	void Start() {
		if (GameObject.Find(searchScreenCursorObjectName) != null) crossair = (Image)((GameObject.Find(searchScreenCursorObjectName)).GetComponent<Image>());
		if (GameObject.Find(searchScreenSpeedMeterObjectName) != null) speedmeter = (Text)((GameObject.Find(searchScreenSpeedMeterObjectName)).GetComponent<Text>());
		if (GameObject.Find(searchScreenAutopilotMeterObjectName) != null) autopilotmeter = (Text)((GameObject.Find(searchScreenAutopilotMeterObjectName)).GetComponent<Text>());
		scene = GameObject.Find(searchSceneObjectName);
		ground = GameObject.Find(searchGroundObjectName);
		clouds = GameObject.Find(searchCloudsObjectName);
		cameraExternalEnabled_lastValue = !cameraExternalEnabled;
		
		if (searchSounds) {
			soundmin = new AudioSource[soundmin_countmax];
			soundmax = new AudioSource[soundmax_countmax];
			soundstall = new AudioSource[soundstall_countmax];
			soundcrash = new AudioSource[soundcrash_countmax];
			soundmin_count = 0;
			soundmax_count = 0;
			soundstall_count = 0;
			soundcrash_count = 0;
		}
		FindNodes(gameObject, 0);

		sm = (GAircraft)gameObject.GetComponent("GAircraft");
		if (enableRecoverVehicle) {
			enableRecoverVehiclePosition = sm.transform.position;
			enableRecoverVehicleRotation = sm.transform.eulerAngles;
		}
		
		initialFogMode = RenderSettings.fogMode;
		switch (initialFogMode) {
		case FogMode.Linear:
			initialFogValue = RenderSettings.fogEndDistance;
			break;
		case FogMode.Exponential:
		case FogMode.ExponentialSquared:
			initialFogValue = RenderSettings.fogDensity;
			break;
		}
		if (extremeFogValue < 0.0f) extremeFogValue = initialFogValue;
	}
	
	void Update() {
		if (globalSimulationScaleApplyToCamera && (sm != null)) globalSimulationScale = sm.globalSimulationScale;
		if (Input.GetKeyUp(inputExternalCameraKeyForToggle)) inputExternalCameraKeyForToggled = true;
		if (Input.GetKeyUp(inputFixedPositionCameraKeyForToggle)) inputFixedPositionCameraKeyForToggled = true;
		if (Input.GetKeyUp(inputRecoverVehicleKeyForToggle)) inputRecoverVehicleKeyForToggled = true;
		ProcessSkybox();
		if (GAircraft.isSimulationPaused) {
			ProcessSounds(Time.deltaTime);
			return;
		}
		if (!cameraOnFixedUpdate) ProcessCamera(Time.deltaTime);
	}
	void FixedUpdate() {
		if (globalSimulationScaleApplyToCamera && (sm != null)) globalSimulationScale = sm.globalSimulationScale;
		if (GAircraft.isSimulationPaused) {
			return;
		}
		if (cameraOnFixedUpdate) ProcessCamera(Time.fixedDeltaTime);
	}
	
	bool getSkyboxRenderingColor(float height, bool renderSpace, out Color output) {
		if (height < skyboxRenderingColor1Height) {
			output = skyboxRenderingColor1;
		} else if (height < skyboxRenderingColor2Height) {
			output = skyboxRenderingColor1 * (skyboxRenderingColor2Height - height) / (skyboxRenderingColor2Height - skyboxRenderingColor1Height) + skyboxRenderingColor2 * (height - skyboxRenderingColor1Height) / (skyboxRenderingColor2Height - skyboxRenderingColor1Height);
		} else {
			if (renderSpace) {
				if (height < skyboxRenderingColor3Height) {
					output = skyboxRenderingColor2 * (skyboxRenderingColor3Height - height) / (skyboxRenderingColor3Height - skyboxRenderingColor2Height) + skyboxRenderingColor3 * (height - skyboxRenderingColor2Height) / (skyboxRenderingColor3Height - skyboxRenderingColor2Height);
				} else {
					output = skyboxRenderingColor3;
				}
			} else {
				output = skyboxRenderingColor2;
			}
		}
		return true;
	}
	
	bool PreProcessSkyboxTexture(float height) {
		float ahm1 = 0.0f, ah1, ah2, ah3;
		int hm1 = 0, h0, h1, h2, h3, h4;
		float hrm1 = 0.0f, hr1, hr2, hr3, hr4;
		if (skyboxRenderingColor0Enabled) tmp_skyboxRenderingColorm1 = skyboxRenderingColor0;
		tmp_skyboxRenderingColor0 = skyboxRenderingColor;
		tmp_skyboxRenderingColor1 = skyboxRenderingColor1;
		tmp_skyboxRenderingColor2 = skyboxRenderingColor2;
		tmp_skyboxRenderingColor3 = skyboxRenderingColor3;
		tmp_skyboxRenderingColor4 = skyboxRenderingColorTop;
		if ((height + skyboxRenderingColorTopDeltaHeight) < skyboxRenderingColor3Height) tmp_skyboxRenderingColor3 = skyboxRenderingColorTop;
		if ((height + skyboxRenderingColorTopDeltaHeight) < skyboxRenderingColor2Height) tmp_skyboxRenderingColor2 = skyboxRenderingColorTop;
		if ((height + skyboxRenderingColorTopDeltaHeight) < skyboxRenderingColor1Height) tmp_skyboxRenderingColor1 = skyboxRenderingColorTop;
		if (skyboxRenderingColor0Enabled) {
			if ((skyboxRenderingColor0Height - height) > 0) ahm1 = Mathf.Atan((skyboxRenderingColor0Height - height) / skyboxRenderingColor0Distance);
			else ahm1 = Mathf.Atan((skyboxRenderingColor0Height - height) / skyboxRenderingColor0Distance);
		}
		if ((skyboxRenderingColor1Height - height) > 0) ah1 = Mathf.Atan((skyboxRenderingColor1Height - height) / skyboxRenderingColorUpwardsScatteringDistance);
		else ah1 = Mathf.Atan((skyboxRenderingColor1Height - height) / skyboxRenderingColorDownwardsScatteringDistance);
		if ((skyboxRenderingColor2Height - height) > 0) ah2 = Mathf.Atan((skyboxRenderingColor2Height - height) / skyboxRenderingColorUpwardsScatteringDistance);
		else ah2 = Mathf.Atan((skyboxRenderingColor2Height - height) / skyboxRenderingColorDownwardsScatteringDistance);
		if ((skyboxRenderingColor3Height - height) > 0) ah3 = Mathf.Atan((skyboxRenderingColor3Height - height) / skyboxRenderingColorUpwardsScatteringDistance);
		else ah3 = Mathf.Atan((skyboxRenderingColor3Height - height) / skyboxRenderingColorDownwardsScatteringDistance);
		if (skyboxRenderingColor0Enabled) hm1 = Mathf.RoundToInt(skyboxAngleColors * ((ahm1 + Mathf.PI / 2.0f) / Mathf.PI));
		h1 = Mathf.RoundToInt(skyboxAngleColors * ((ah1 + Mathf.PI / 2.0f) / Mathf.PI));
		h2 = Mathf.RoundToInt(skyboxAngleColors * ((ah2 + Mathf.PI / 2.0f) / Mathf.PI));
		h3 = Mathf.RoundToInt(skyboxAngleColors * ((ah3 + Mathf.PI / 2.0f) / Mathf.PI));
		h0 = h1;
		h4 = h3;
		if (skyboxRenderingColor0Enabled) if (h0 < hm1 + 4) h0 = hm1 + 4;
		if (h1 < h0 + 4) h1 = h0 + 4;
		if (h2 < h1 + 4) h2 = h1 + 4;
		if (h3 < h2 + 4) h3 = h2 + 4;
		if (h4 < h3 + 4) h4 = h3 + 4;
		if (skyboxRenderingColor0Enabled) hrm1 = 255.0f / (h0 - hm1);
		hr1 = 255.0f / (h1 - h0);
		hr2 = 255.0f / (h2 - h1);
		hr3 = 255.0f / (h3 - h2);
		hr4 = 255.0f / (h4 - h3);
		if (skyboxRenderingColor0Enabled) {
			for (int i = 0; i < skyboxAngleColors; ++i) {
				if (i < hm1) {
					skyboxAngleColor[i] = tmp_skyboxRenderingColorm1;
				} else if (i < h0) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor0.r * (i - hm1) + tmp_skyboxRenderingColorm1.r * (h0 - i)) * hrm1);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor0.g * (i - hm1) + tmp_skyboxRenderingColorm1.g * (h0 - i)) * hrm1);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor0.b * (i - hm1) + tmp_skyboxRenderingColorm1.b * (h0 - i)) * hrm1);
				} else if (i < h1) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor1.r * (i - h0) + tmp_skyboxRenderingColor0.r * (h1 - i)) * hr1);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor1.g * (i - h0) + tmp_skyboxRenderingColor0.g * (h1 - i)) * hr1);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor1.b * (i - h0) + tmp_skyboxRenderingColor0.b * (h1 - i)) * hr1);
				} else if (i < h2) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor2.r * (i - h1) + tmp_skyboxRenderingColor1.r * (h2 - i)) * hr2);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor2.g * (i - h1) + tmp_skyboxRenderingColor1.g * (h2 - i)) * hr2);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor2.b * (i - h1) + tmp_skyboxRenderingColor1.b * (h2 - i)) * hr2);
				} else if (i < h3) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor3.r * (i - h2) + tmp_skyboxRenderingColor2.r * (h3 - i)) * hr3);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor3.g * (i - h2) + tmp_skyboxRenderingColor2.g * (h3 - i)) * hr3);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor3.b * (i - h2) + tmp_skyboxRenderingColor2.b * (h3 - i)) * hr3);
				} else if (i < h4) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor4.r * (i - h3) + tmp_skyboxRenderingColor3.r * (h4 - i)) * hr4);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor4.g * (i - h3) + tmp_skyboxRenderingColor3.g * (h4 - i)) * hr4);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor4.b * (i - h3) + tmp_skyboxRenderingColor3.b * (h4 - i)) * hr4);
				} else {
					skyboxAngleColor[i] = tmp_skyboxRenderingColor4;
				}
			}
		} else {
			for (int i = 0; i < skyboxAngleColors; ++i) {
				if (i < h0) {
					skyboxAngleColor[i] = tmp_skyboxRenderingColor0;
				} else if (i < h1) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor1.r * (i - h0) + tmp_skyboxRenderingColor0.r * (h1 - i)) * hr1);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor1.g * (i - h0) + tmp_skyboxRenderingColor0.g * (h1 - i)) * hr1);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor1.b * (i - h0) + tmp_skyboxRenderingColor0.b * (h1 - i)) * hr1);
				} else if (i < h2) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor2.r * (i - h1) + tmp_skyboxRenderingColor1.r * (h2 - i)) * hr2);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor2.g * (i - h1) + tmp_skyboxRenderingColor1.g * (h2 - i)) * hr2);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor2.b * (i - h1) + tmp_skyboxRenderingColor1.b * (h2 - i)) * hr2);
				} else if (i < h3) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor3.r * (i - h2) + tmp_skyboxRenderingColor2.r * (h3 - i)) * hr3);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor3.g * (i - h2) + tmp_skyboxRenderingColor2.g * (h3 - i)) * hr3);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor3.b * (i - h2) + tmp_skyboxRenderingColor2.b * (h3 - i)) * hr3);
				} else if (i < h4) {
					skyboxAngleColor[i].r = (byte)((tmp_skyboxRenderingColor4.r * (i - h3) + tmp_skyboxRenderingColor3.r * (h4 - i)) * hr4);
					skyboxAngleColor[i].g = (byte)((tmp_skyboxRenderingColor4.g * (i - h3) + tmp_skyboxRenderingColor3.g * (h4 - i)) * hr4);
					skyboxAngleColor[i].b = (byte)((tmp_skyboxRenderingColor4.b * (i - h3) + tmp_skyboxRenderingColor3.b * (h4 - i)) * hr4);
				} else {
					skyboxAngleColor[i] = tmp_skyboxRenderingColor4;
				}
			}
		}
		return true;
	}
	
	bool ProcessSkyboxTexture(Texture2D tex, float height, int mode) {
		if (pixels == null) Skybox_Start();
		int w, h;
		w = tex.width;
		h = tex.height;
		for (int j = 0; j < h; ++j) {
			switch (mode) {
				case -1:
					for (int i = 0; i < w; ++i) {
						pixels[i + j * w] = skyboxAngleColor[skyboxBottomAngles[i, j]];
					}
					break;
				case 0:
					for (int i = 0; i < w; ++i) {
						pixels[i + j * w] = skyboxAngleColor[skyboxSideAngles[i, j]];
					}
					break;
				case 1:
					for (int i = 0; i < w; ++i) {
						pixels[i + j * w] = skyboxAngleColor[skyboxTopAngles[i, j]];
					}
					break;
			}
		}
		tex.SetPixels32(pixels);
		tex.Apply(false);
		return true;
	}
	
	void ProcessSkybox() {
		float smheight;
		smheight = ((sm != null) ? sm.height : 0.0f);
		if (fogRenderingEnabled) {
			if (skyboxRenderingColor0Enabled) RenderSettings.fogColor = skyboxRenderingColor0;
			else RenderSettings.fogColor = skyboxRenderingColor;
			
			switch (initialFogMode) {
			case FogMode.Linear:
				RenderSettings.fogEndDistance = initialFogValue * (1.0f - fog01) + extremeFogValue * fog01;
				break;
			case FogMode.Exponential:
			case FogMode.ExponentialSquared:
				RenderSettings.fogDensity = initialFogValue * (1.0f - fog01) + extremeFogValue * fog01;
				break;
			}
		}
		if (skyboxRenderingEnabled) {
			if (skyboxComponent == null) Skybox_Start();
			
			((attachToCamera == null) ? Camera.main : attachToCamera).clearFlags = CameraClearFlags.Skybox;
			getSkyboxRenderingColor(gameObject.transform.position.y + ((sm != null) ? sm.height : 0.0f), false, out skyboxRenderingColor);
			((attachToCamera == null) ? Camera.main : attachToCamera).backgroundColor = skyboxRenderingColor;
			--skyboxRenderNth_count;
			if (skyboxRenderNth_count <= 0) {
				skyboxRenderNth_count = skyboxRenderNth;
				
				getSkyboxRenderingColor(gameObject.transform.position.y + smheight + skyboxRenderingColorTopDeltaHeight, true, out skyboxRenderingColorTop);
				
				PreProcessSkyboxTexture(gameObject.transform.position.y + smheight);
				for (int i = 0; i < skyboxRenderSidesPerNth; ++i) {
					++skyboxRenderNth_tex;
					if (skyboxRenderNth_tex > 5) skyboxRenderNth_tex = 0;
					switch (skyboxRenderNth_tex) {
						case 0:
							if (_FrontTex != null) ProcessSkyboxTexture(_FrontTex, gameObject.transform.position.y + smheight, 0);
							break;
						case 1:
							if (_LeftTex != null) ProcessSkyboxTexture(_LeftTex, gameObject.transform.position.y + smheight, 0);
							break;
						case 2:
							if (_RightTex != null) ProcessSkyboxTexture(_RightTex, gameObject.transform.position.y + smheight, 0);
							break;
						case 3:
							if (_BackTex != null) ProcessSkyboxTexture(_BackTex, gameObject.transform.position.y + smheight, 0);
							break;
						case 4:
							if (_UpTex != null) ProcessSkyboxTexture(_UpTex, gameObject.transform.position.y + smheight, 1);
							break;
						case 5:
							if (_DownTex != null) ProcessSkyboxTexture(_DownTex, gameObject.transform.position.y + smheight, -1);
							break;
					}
				}
			}
		}
	}
	
	float gs_old = 0.0f;
	float hgs_old = 0.0f;
	void ProcessCamera(float timeLapsus) {
		if (enableSpecialForce && Input.GetKey(specialForceApplyZeroRotate)) {
			gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
		if (scheduleRecoverVehicle > 0) { --scheduleRecoverVehicle; if (scheduleRecoverVehicle == 0) recoverVehicle(); }
		if (enableRecoverVehicle && inputRecoverVehicleKeyForToggled) {
			inputRecoverVehicleKeyForToggled = false;
			recoverVehicle();
		}

		if (cameraOnFixedUpdate_count > 0) {
			--cameraOnFixedUpdate_count;
		} else {
			if (inputExternalCameraKeyForToggled) {
				inputExternalCameraKeyForToggled = false;
				if (cameraExternalEnabled) {
					cameraExternalEnabled = false;
					if ((sm != null) && sm.globalDebugNodes) Debug.Log("Internal Camera " + currentCameraPosition.ToString() + " Selected");
				} else {
					++currentCameraPosition;
					if (currentCameraPosition >= countCameraPositions) {
						currentCameraPosition = 0;
						cameraExternalEnabled = true;
						if ((sm != null) && sm.globalDebugNodes) Debug.Log("External Camera Selected");
					} else {
						if ((sm != null) && sm.globalDebugNodes) Debug.Log("Internal Camera " + currentCameraPosition.ToString() + " Selected");
					}
				}
				cameraOnFixedUpdate_count = 50;
			}
		}
		if (inputFixedPositionCameraKeyForToggled) {
			inputFixedPositionCameraKeyForToggled = false;
			cameraFixedPositionEnabled = !cameraFixedPositionEnabled;
			if (cameraFixedPositionEnabled) cameraposition = cameraposition + gameObject.GetComponent<Rigidbody>().velocity * 4.0f;
		}
		
		if (enableSpecialForce && Input.GetKey(specialForceApplyLeft)) gameObject.GetComponent<Rigidbody>().AddForce(-gameObject.transform.right * specialForceMagnitude);
		if (enableSpecialForce && Input.GetKey(specialForceApplyRight)) gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.right * specialForceMagnitude);

		if (enableSpecialForce && Input.GetKey(specialForceApplyForward)) gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.forward * specialForceMagnitude);
		if (enableSpecialForce && Input.GetKey(specialForceApplyForceBack)) gameObject.GetComponent<Rigidbody>().AddForce(-gameObject.transform.forward * specialForceMagnitude);
		if (enableSpecialForce && Input.GetKey(specialForceApplyForceUp)) gameObject.GetComponent<Rigidbody>().AddForce(gameObject.transform.up * specialForceMagnitude);
		if (enableSpecialForce && Input.GetKey(specialForceApplyDown)) gameObject.GetComponent<Rigidbody>().AddForce(-gameObject.transform.up * specialForceMagnitude);
		
		if (cameraExternalEnabled != cameraExternalEnabled_lastValue) {
			if (cameraExternalEnabled) {
				((attachToCamera == null) ? Camera.main : attachToCamera).nearClipPlane = cameraExternalNearClipPlane * globalSimulationScale;
				((attachToCamera == null) ? Camera.main : attachToCamera).farClipPlane = cameraExternalFarClipPlane * globalSimulationScale;
	
			} else {
				((attachToCamera == null) ? Camera.main : attachToCamera).nearClipPlane = cameraInternalNearClipPlane * globalSimulationScale;
				((attachToCamera == null) ? Camera.main : attachToCamera).farClipPlane = cameraInternalFarClipPlane * globalSimulationScale;
			}
			cameraExternalEnabled_lastValue = cameraExternalEnabled;
		}
		
		float mouseh;
		float mousev;
		
		mouseh = (Input.mousePosition.x - Screen.width / 2.0f) / Screen.width;
		mousev = (Input.mousePosition.y - Screen.height / 2.0f) / Screen.height;
		if (mouseh > 0.5f) mouseh = 0.5f;
		if (mouseh < -0.5f) mouseh = -0.5f;
		if (mousev > 0.5f) mousev = 0.5f;
		if (mousev < -0.5f) mousev = -0.5f;
		
		float gs;
		gs = gs_old * (1.0f - cameraInternalGsMixing) + sm.gaugesGs_output * cameraInternalGsMixing;
		if (gs > cameraInternalGsMax) gs = cameraInternalGsMax;
		if (gs < -cameraInternalGsMax) gs = -cameraInternalGsMax;
		gs_old = gs;
			
		float hgs;
		hgs = hgs_old * (1.0f - cameraInternalGsMixing) + sm.gaugesHGs_output * cameraInternalGsMixing;
		if (hgs > cameraInternalGsMax) hgs = cameraInternalGsMax;
		if (hgs < -cameraInternalGsMax) hgs = -cameraInternalGsMax;
		hgs_old = hgs;
		
		Vector3 position = gameObject.transform.position;
		if (cameraAlternateFollow != null) position = cameraAlternateFollow.transform.position;

		if (cameraExternalEnabled) {
		
			if ((Input.GetAxis("Mouse ScrollWheel") > 0) && (cameraExternalDistance > cameraExternalDistanceMin)) {
				cameraExternalDistance -= cameraExternalDistanceStep * inputWheelMouseSensivity / 10.0f;
			}
			if ((Input.GetAxis("Mouse ScrollWheel") < 0) && (cameraExternalDistance < cameraExternalDistanceMax)) {
				cameraExternalDistance += cameraExternalDistanceStep * inputWheelMouseSensivity / 10.0f;
			}

			if (!cameraFixedPositionEnabled) cameraposition = position + new Vector3(
				-gameObject.transform.forward.x * 20.0f - gameObject.transform.right.x * 40.0f * mouseh * inputHorizontalMouseSensivity / 3.0f,
				-40.0f * (mousev * inputVerticalMouseSensivity / 3.0f - 0.25f),
				-gameObject.transform.forward.z * 20.0f - gameObject.transform.right.z * 40.0f * mouseh * inputHorizontalMouseSensivity / 3.0f
			) * cameraExternalDistance * cameraExternalMinHeightScale * globalSimulationScale;
			cameralookat = position;
			
			if (cameraExternalUpWorld) cameraup = worldup;
			else cameraup = gameObject.transform.up;

			if (cameraFixedPositionEnabled) ((attachToCamera == null) ? Camera.main : attachToCamera).fieldOfView = 20.0f;
			else ((attachToCamera == null) ? Camera.main : attachToCamera).fieldOfView = 80.0f;
			camera_filter = cameraExternalFilter;
			cameraOnFixedUpdate = true;

		} else {
			
			if ((Input.GetAxis("Mouse ScrollWheel") > 0) && (cameraInternalFov > cameraInternalFovMin)) {
				cameraInternalFov -= cameraInternalFovStep * inputWheelMouseSensivity * cameraInternalSensivity / 10.0f;
			}
			if ((Input.GetAxis("Mouse ScrollWheel") < 0) && (cameraInternalFov < cameraInternalFovMax)) {
				cameraInternalFov += cameraInternalFovStep * inputWheelMouseSensivity * cameraInternalSensivity / 10.0f;
			}
			
			cameraorientation = gameObject.transform.rotation;
			camerafixed = false;
			countCameraPositions = 0;
			if (!cameraFixedPositionEnabled) cameraposition = gameObject.transform.TransformPoint(0.0f, 0.4f, -0.4f);
			if (!cameraFixedPositionEnabled) {
				if ((sm != null) && (sm.labeled_surfacemisc != null)) {
					for (int i = 0; i < sm.labeled_surfacemisc_count; ++i) {
						if (sm.labeled_surfacemisc[i].cameraPosition) {
							if (currentCameraPosition == countCameraPositions) {
								cameraposition = sm.labeled_surfacemisc[i].gameObject.transform.position;
								cameraorientation = sm.labeled_surfacemisc[i].gameObject.transform.rotation;
								camerafixed = !sm.labeled_surfacemisc[i].cameraCanRotate;
							}
							++countCameraPositions;
						}
					}
				}
			}
			if (!camerafixed) {
				if (cameraInternalGsPosition) cameraposition -= gameObject.transform.up * gs * cameraInternalGsPositionDelta;
				if (cameraInternalGsPosition) cameraposition -= gameObject.transform.right * hgs * cameraInternalGsPositionHDelta;
			}
			
			if (cameraAlternateFollow != null) cameraposition = cameraAlternateFollow.transform.position;
			
			float looky;
			if (cameraInternalDontVertical && cameraInternalDontVerticalMix) {
				if (cameraInternalGsRotation) looky = -Mathf.Sin(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity * (1.0f - cameraInternalDontVerticalMixing) + gs * cameraInternalGsRotationDelta);
				else looky = -Mathf.Sin(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity * (1.0f - cameraInternalDontVerticalMixing));
			} else if (cameraInternalDontVertical) {
				if (cameraInternalGsRotation) looky = -Mathf.Sin(gs * cameraInternalGsRotationDelta);
				else looky = -Mathf.Sin(0.0f);
			} else {
				if (cameraInternalGsRotation) looky = -Mathf.Sin(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity + gs * cameraInternalGsRotationDelta);
				else looky = -Mathf.Sin(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity);
			}

			float looky2;
			if (cameraInternalDontVertical && cameraInternalDontVerticalMix) {
				if (cameraInternalGsRotation) looky2 = Mathf.Cos(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity * (1.0f - cameraInternalDontVerticalMixing) + gs * cameraInternalGsRotationDelta);
				else looky2 = Mathf.Cos(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity * (1.0f - cameraInternalDontVerticalMixing));
			} else if (cameraInternalDontVertical) {
				if (cameraInternalGsRotation) looky2 = Mathf.Cos(gs * cameraInternalGsRotationDelta);
				else looky2 = Mathf.Cos(0.0f);
			} else {
				if (cameraInternalGsRotation) looky2 = Mathf.Cos(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity + gs * cameraInternalGsRotationDelta);
				else looky2 = Mathf.Cos(-mousev * inputVerticalMouseSensivity * cameraInternalSensivity);
			}
			
			if (cameraAlternateFollow != null) {
				cameralookat = cameraposition + cameraAlternateFollow.transform.rotation * (new Vector3(
					-Mathf.Cos(-Mathf.PI / 2.0f - mouseh * inputHorizontalMouseSensivity * cameraInternalSensivity) * looky2,
					looky,
					-Mathf.Sin(-Mathf.PI / 2.0f - mouseh * inputHorizontalMouseSensivity * cameraInternalSensivity) * looky2
				));
				if (cameraInternalUpWorld && cameraInternalUpWorldMix) cameraup = worldup * cameraInternalUpWorldMixing + cameraAlternateFollow.transform.up * (1.0f - cameraInternalUpWorldMixing);
				else if (cameraInternalUpWorld) cameraup = worldup;
				else {
					cameraup = cameraAlternateFollow.transform.up;
				}
				if (cameraInternalGsRotation) cameraup -= cameraAlternateFollow.transform.right * hgs * cameraInternalGsRotationDelta;
			} else {
				if (camerafixed) {
					cameralookat = cameraposition + cameraorientation * Vector3.forward;
					cameraup = gameObject.transform.up;
				} else {
					cameralookat = cameraposition + cameraorientation * (new Vector3(
						-Mathf.Cos(-Mathf.PI / 2.0f - mouseh * inputHorizontalMouseSensivity * cameraInternalSensivity) * looky2,
						looky,
						-Mathf.Sin(-Mathf.PI / 2.0f - mouseh * inputHorizontalMouseSensivity * cameraInternalSensivity) * looky2
					));
					if (cameraInternalUpWorld && cameraInternalUpWorldMix) cameraup = worldup * cameraInternalUpWorldMixing + gameObject.transform.up * (1.0f - cameraInternalUpWorldMixing);
					else if (cameraInternalUpWorld) cameraup = worldup;
					else {
						cameraup = gameObject.transform.up;
					}
					if (cameraInternalGsRotation) cameraup -= gameObject.transform.right * hgs * cameraInternalGsRotationDelta;
				}
			}
			
			((attachToCamera == null) ? Camera.main : attachToCamera).fieldOfView = cameraInternalFov;
			camera_filter = cameraInternalFilter;
			cameraOnFixedUpdate = false;

		}
		
		if (sunlightAttach) if (sunlight != null) {
			sunlight.transform.position = (1.0f - sunlightAttachFilter) * sunlight.transform.position + sunlightAttachFilter * (((attachToCamera == null) ? Camera.main : attachToCamera).transform.position - sunlight.transform.forward * sunlightAttachDistance);
			
		}
		
		cameraposition_filtered = cameraposition_filtered * (1.0f - camera_filter) + cameraposition * camera_filter;
		cameralookat_filtered = cameralookat_filtered * (1.0f - camera_filter) + cameralookat * camera_filter;
		cameraup_filtered = cameraup_filtered * (1.0f - camera_filter) + cameraup * camera_filter;
		((attachToCamera == null) ? Camera.main : attachToCamera).transform.position = cameraposition_filtered;
		((attachToCamera == null) ? Camera.main : attachToCamera).transform.LookAt(cameralookat_filtered, cameraup_filtered);
		
		cameraExternalMinHeightScale = 1.0f;
		if (cameraExternalEnabled) {
			Vector3 pos = ((attachToCamera == null) ? Camera.main : attachToCamera).transform.position;
			if (cameraExternalMinHeightAutoProbe) {
				RaycastHit hit;
				if (Physics.Raycast(pos + Vector3.up * cameraExternalMinHeightAutoProbeDeltaProbe, -Vector3.up, out hit, 999999999.9f, cameraExternalMinHeightAutoProbeLayermask)) {
					if (pos.y < hit.point.y + cameraExternalMinHeightAutoProbeDelta) {
						cameraExternalMinHeightScale *= 1.0f / ((hit.point.y + cameraExternalMinHeightAutoProbeDelta - pos.y) * 0.1f + 1.0f);
						pos.y = hit.point.y + cameraExternalMinHeightAutoProbeDelta * cameraExternalMinHeightScale;
					}
				}
			}
			if (pos.y < cameraExternalMinHeight) {
				cameraExternalMinHeightScale *= 1.0f / ((cameraExternalMinHeight - pos.y) * 1.0f + 1.0f);
				pos.y = cameraExternalMinHeight;
			}

			((attachToCamera == null) ? Camera.main : attachToCamera).transform.position = pos;
		}

		if (crossair != null) {
			Cursor.visible = false;
			float crossair_size = 64.0f;
			//crossair.pixelInset = new Rect(Input.mousePosition.x - (Screen.width / 2.0f) - crossair_size / 2.0f, Input.mousePosition.y - Screen.height / 2.0f - crossair_size / 2.0f, crossair_size, crossair_size);		
		}
		
		if (speedmeter != null) {
			//speedmeter.pixelOffset = new Vector2(-Screen.width / 2.0f + 10.0f, Screen.height / 2.0f - 10.0f);
			float temperature = 15.0f;
			float speedofsound = 331.3f + 0.606f * temperature;
			if (sm != null) {
				speedmeter.text = "" + "speed: " + (Mathf.Floor(sm.speed / sm.globalSimulationScale * 3.6f * 100.0f) / 100.0f).ToString() + " km/h | " + (Mathf.Floor(sm.speed / sm.globalSimulationScale * 1.94384449f * 100.0f) / 100.0f).ToString() + " knots | M" + (Mathf.Floor(sm.speed / sm.globalSimulationScale / speedofsound * 100.0f) / 100.0f).ToString() +  " | " + (Mathf.Floor(sm.speed / sm.globalSimulationScale * 100.0f) / 100.0f).ToString() + " m/s | alternate method: " + (Mathf.Floor(gameObject.GetComponent<Rigidbody>().velocity.magnitude / sm.globalSimulationScale * 100.0f) / 100.0f).ToString() + " m/s\nheight " + (Mathf.Floor((gameObject.transform.position.y + ((sm != null) ? sm.height : 0.0f) + 102.84f)) / sm.globalSimulationScale).ToString() +  "m | " + (Mathf.Floor(((gameObject.transform.position.y + ((sm != null) ? sm.height : 0.0f) + 102.84f) / 0.3048f)) / sm.globalSimulationScale).ToString() +  " feet | vertical distance to ground: " + (Mathf.Floor(sm.distanceToGround / sm.globalSimulationScale * 100.0f) / 100.0f).ToString() + " m\nwind: " + (Mathf.Floor(GWindBasic.windAt(gameObject.transform.position).magnitude / sm.globalSimulationScale * 3.6f * 100.0f) / 100.0f).ToString() + "km/h | " + (Mathf.Floor(GWindBasic.windAt(gameObject.transform.position).magnitude / sm.globalSimulationScale * 1.94384449f * 100.0f) / 100.0f).ToString() + " knots\nsensivity: " + sm.inputSensivity.ToString() + " | trim ailerons: " + sm.inputAileronsTrim.ToString() + " | trim elevator: " + sm.inputElevatorTrim.ToString() + " | trim rudder: " + sm.inputRudderTrim.ToString();
				if (sm.stall > 0.0f) speedmeter.text += "\nSTALL: " + Mathf.RoundToInt(sm.stall * 100.0f).ToString() + "%";
			} else {
				speedmeter.text = "Cannot locate GAircraft attached to the same object where GSCameraAux is attached.";
			}
		}
		
		if (autopilotmeter != null) {
			//autopilotmeter.pixelOffset = new Vector2(-Screen.width / 2.0f + 10.0f, -Screen.height / 2.0f + 10.0f + 10.0f);
		}
		
		ProcessGround(timeLapsus);
		ProcessClouds(timeLapsus);
		ProcessSounds(timeLapsus);
	}
	void ProcessGround(float timeLapsus) {
		if (ground != null) {
			float granularity = searchGroundObjectGranularity;
			Vector3 groundposition = ground.transform.position;
			if (groundposition.x - gameObject.transform.position.x > granularity) groundposition.x -= granularity;
			if (groundposition.x - gameObject.transform.position.x < -granularity) groundposition.x += granularity;
			if (groundposition.z - gameObject.transform.position.z > granularity) groundposition.z -= granularity;
			if (groundposition.z - gameObject.transform.position.z < -granularity) groundposition.z += granularity;
			ground.transform.position = groundposition;
		}
	}
	public void ProcessClouds(float timeLapsus) {
		if (clouds != null) {
			float granularity = searchCloudsObjectGranularity;
			Vector3 cloudsposition = clouds.transform.position;
			Vector3 windspeed = GWindBasic.windAt(cloudsposition) * searchCloudsWindSpeedMultiplier * timeLapsus;
			cloudsposition += windspeed;
			if (cloudsposition.x - gameObject.transform.position.x > granularity) cloudsposition.x -= granularity;
			if (cloudsposition.x - gameObject.transform.position.x < -granularity) cloudsposition.x += granularity;
			if (cloudsposition.z - gameObject.transform.position.z > granularity) cloudsposition.z -= granularity;
			if (cloudsposition.z - gameObject.transform.position.z < -granularity) cloudsposition.z += granularity;
			clouds.transform.position = cloudsposition;
		}
	}
	public static void ProcessSounds_getVolumeAndPitch(float inputmin, float inputmax, float soundmin_pivotScale, float soundmin_pitchBase, float soundmin_pitchScale, float soundmax_pivotScale, float soundmax_pitchBase, float soundmax_pitchScale, out float volume_soundmin, out float volume_soundmax, out float pitch_soundmin, out float pitch_soundmax) {
		volume_soundmin = inputmin / soundmin_pivotScale;
		volume_soundmax = inputmax / soundmax_pivotScale;
		pitch_soundmin = soundmin_pitchBase + volume_soundmin * 2.0f * soundmin_pitchScale;
		volume_soundmin = volume_soundmin * 2.0f * GAircraft.globalVolume;
		if (volume_soundmax > 0.5f) {
			pitch_soundmax = soundmax_pitchBase + volume_soundmax * 1.25f * soundmax_pitchScale;
			volume_soundmax = ((volume_soundmax - 0.5f) * 2.0f * 2.0f) * GAircraft.globalVolume;
		} else {
			pitch_soundmax = soundmax_pitchBase;
			volume_soundmax = 0.0f;
		}
		if (GAircraft.isSimulationPaused) {
			volume_soundmin = 0.0f;
			volume_soundmax = 0.0f;
		}
	}
	public static void ProcessSounds_getVolumeAndPitch(float input, float pivotScale, out float volume_soundmin, out float volume_soundmax, out float pitch_soundmin, out float pitch_soundmax) {
		float soundmin_pivotScale = pivotScale;
		float soundmin_pitchBase = 1.0f;
		float soundmin_pitchScale = 1.0f;
		float soundmax_pivotScale = pivotScale;
		float soundmax_pitchBase = 1.0f;
		float soundmax_pitchScale = 1.0f;
		ProcessSounds_getVolumeAndPitch(input, input, soundmin_pivotScale, soundmin_pitchBase, soundmin_pitchScale, soundmax_pivotScale, soundmax_pitchBase, soundmax_pitchScale, out volume_soundmin, out volume_soundmax, out pitch_soundmin, out pitch_soundmax);
	}
	public static void ProcessSounds_getVolumeAndPitch(float input, out float volume_soundmin, out float volume_soundmax, out float pitch_soundmin, out float pitch_soundmax) {
		ProcessSounds_getVolumeAndPitch(input, 20000.0f, out volume_soundmin, out volume_soundmax, out pitch_soundmin, out pitch_soundmax);
	}
	
	float ProcessSounds_granularityPitch = 0.01f;
	float ProcessSounds_granularityTime = 0.25f;
	float ProcessSounds_granularityTime2 = 5.25f;
	float ProcessSounds_granularityTimeRemaining = 0.0f;
	float ProcessSounds_granularityTime2Remaining = 0.0f;
	void ProcessSounds(float timeLapsus) {
		bool processPitchWithGranularity = false, processPitchWithoutGranularity = false;
		if (ProcessSounds_granularityTimeRemaining >= 0f) {
			ProcessSounds_granularityTimeRemaining -= timeLapsus;
		} else {
			ProcessSounds_granularityTimeRemaining += ProcessSounds_granularityTime;
			processPitchWithGranularity = true;
		}
		if (ProcessSounds_granularityTime2Remaining >= 0f) {
			ProcessSounds_granularityTime2Remaining -= timeLapsus;
		} else {
			ProcessSounds_granularityTime2Remaining += ProcessSounds_granularityTime2;
			processPitchWithoutGranularity = true;
		}
		if (searchSounds) {
			float inputmin;
			float inputmax;
			float volume_soundmin;
			float volume_soundmax;
			float volume_soundstall;
			float volume_soundcrash;
			float pitch_soundmin;
			float pitch_soundmax;
			if (sm != null) {
				inputmin = GPivot.toTAxisValue(soundmin_pivotId, sm, "throttle");
				inputmax = GPivot.toTAxisValue(soundmax_pivotId, sm, "throttle");
				volume_soundstall = sm.stall;
				ProcessSounds_getVolumeAndPitch(inputmin, inputmax, soundmin_pivotScale, soundmin_pitchBase, soundmin_pitchScale, soundmax_pivotScale, soundmax_pitchBase, soundmax_pitchScale, out volume_soundmin, out volume_soundmax, out pitch_soundmin, out pitch_soundmax);
				for (int i = 0; i < soundmin_count; ++i) {
					if (sm.isCrashed) volume_soundmin = 0.0f;
					soundmin[i].volume = volume_soundmin;
					if (processPitchWithoutGranularity) {
						if (soundmin[i].volume > 0.0f) soundmin[i].pitch = pitch_soundmin;
					} else if (processPitchWithGranularity) {
						if (soundmin[i].volume > 0.0f) {
							if (Mathf.Abs(soundmin[i].pitch - pitch_soundmin) >= ProcessSounds_granularityPitch) {
								soundmin[i].pitch = pitch_soundmin;
							}
						}
					}
				}
				for (int i = 0; i < soundmax_count; ++i) {
					if (sm.isCrashed) volume_soundmax = 0.0f;
					soundmax[i].volume = volume_soundmax;
					if (processPitchWithoutGranularity) {
						if (soundmax[i].volume > 0.0f) soundmax[i].pitch = pitch_soundmax;
					} else if (processPitchWithGranularity) {
						if (soundmax[i].volume > 0.0f) {
							if (Mathf.Abs(soundmax[i].pitch - pitch_soundmax) >= ProcessSounds_granularityPitch) {
								soundmax[i].pitch = pitch_soundmax;
							}
						}
					}
				}
				for (int i = 0; i < soundstall_count; ++i) {
					if (sm.isCrashed) volume_soundstall = 0.0f;
					soundstall[i].volume = volume_soundstall;
				}
				if (!iscrashed) {
					if (sm.isCrashed) {
						iscrashed = true;
						volume_soundcrash = 1.0f;
						for (int i = 0; i < soundcrash_count; ++i) {
							soundcrash[i].volume = volume_soundcrash;
							soundcrash[i].Play((ulong)Mathf.FloorToInt(i * 1.7f * 44100.0f));
						}
					}
				} else {
					if (!sm.isCrashed) {
						iscrashed = false;
					}
				}
			}
		}
	}
}
