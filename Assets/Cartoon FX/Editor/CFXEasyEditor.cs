using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

public class CFXEasyEditor : EditorWindow
{
	static private CFXEasyEditor SingleWindow;
	
	[MenuItem("GameObject/CartoonFX Easy Editor")]
	static void ShowWindow()
	{
		if(SingleWindow == null)
		{
			SingleWindow = (CFXEasyEditor)ScriptableObject.CreateInstance(typeof(CFXEasyEditor));
			SingleWindow.title = "CartoonFX Easy Editor";
			SingleWindow.minSize = new Vector2(300,420);
			SingleWindow.maxSize = new Vector2(300,420);
			SingleWindow.position = new Rect(200,200,300,420);
		}
		
		SingleWindow.ShowUtility();
		SingleWindow.Focus();
	}
	
	private bool IncludeChildren = true;
	private bool AffectAlpha = true;
	private float ScalingValue = 2.0f;
	private float LTScalingValue = 100.0f;
	private Color ColorValue = Color.white;
	private Color ColorValue2 = Color.white;
	
	//Module copying
	private ParticleSystem sourceObject;
	private Color ColorSelected = new Color(0.8f,0.95f,1.0f,1.0f);
	private bool[] b_modules = new bool[16];
	
	void OnDisable()
	{
		SingleWindow = null;
	}
	
	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0,0,this.position.width - 8,this.position.height));
		GUILayout.Space(4);
		
		GUILayout.Label("CARTOON FX Easy Editor", EditorStyles.boldLabel);
		GUILayout.Label("Easily change properties of any Particle System!", EditorStyles.miniLabel);
		
		//Separator
		GUI.Box(new Rect(4,62,this.position.width - 12,3),"");
		
	//----------------------------------------------------------------
		
		IncludeChildren = GUILayout.Toggle(IncludeChildren, new GUIContent("Include Children", "If checked, changes will affect every Particle Systems from each child of the selected GameObject(s)"));
		GUILayout.Space(8);
		
	//----------------------------------------------------------------
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button(new GUIContent("Scale Size", "Changes the size of the Particle System(s) and other values accordingly (speed, gravity, etc.)"), GUILayout.Width(120)))
		{
			applyScale();
		}
		GUILayout.Label("Multiplier:",GUILayout.Width(110));
		ScalingValue = EditorGUILayout.FloatField(ScalingValue);
		if(ScalingValue <= 0) ScalingValue = 0.1f;
		GUILayout.EndHorizontal();
		
	//----------------------------------------------------------------
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button(new GUIContent("Set Duration", "Changes the duration of the Particle System(s) (if you want quicker or longer effects, 100% = default duration)"), GUILayout.Width(120)))
		{
			applyScaleLifetime();
		}
		GUILayout.Label("Duration (%):",GUILayout.Width(110));
		LTScalingValue = EditorGUILayout.FloatField(LTScalingValue);
		if(LTScalingValue < 0.1f) LTScalingValue = 0.1f;
		else if(LTScalingValue > 9999) LTScalingValue = 9999;
		GUILayout.EndHorizontal();
		
	//----------------------------------------------------------------
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button(new GUIContent("Change Tint Color", "Changes the tint color of the Particle System(s) (Not reversible!)\nSecond Color is used when Start Color is 'Random Between Two Colors'."),GUILayout.Width(120)))
		{
			applyColor();
		}
		ColorValue = EditorGUILayout.ColorField(ColorValue);
		ColorValue2 = EditorGUILayout.ColorField(ColorValue2);
		AffectAlpha = GUILayout.Toggle(AffectAlpha, new GUIContent("Alpha", "If checked, the alpha value will also be changed"));
		GUILayout.EndHorizontal();
		
	//----------------------------------------------------------------
		
		GUILayout.Space(8);
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button(new GUIContent("Loop Effect", "Loop the effect (might not work properly on some effects such as explosions)"), EditorStyles.miniButtonLeft, GUILayout.Width(120)))
		{
			loopEffect(true);
		}
		if(GUILayout.Button(new GUIContent("Unloop Effect", "Remove looping from the effect"), EditorStyles.miniButtonRight, GUILayout.Width(120)))
		{
			loopEffect(false);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

	//----------------------------------------------------------------
		
		//Separator
		GUI.Box(new Rect(4,160,this.position.width - 12,3),"");
		GUILayout.Space(6);
		
	//----------------------------------------------------------------
		
		GUILayout.Label("COPY MODULES", EditorStyles.boldLabel);
		GUILayout.Label("Copy properties from a Particle System to others!", EditorStyles.miniLabel);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Source Object:", GUILayout.Width(110));
		sourceObject = (ParticleSystem)EditorGUILayout.ObjectField(sourceObject, typeof(ParticleSystem), true);
		GUILayout.EndHorizontal();
		
		EditorGUILayout.LabelField("Modules to Copy:");
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("ALL", EditorStyles.miniButtonLeft, GUILayout.Width(120)))
		{
			for(int i = 0; i < b_modules.Length; i++) b_modules[i] = true;
		}
		if(GUILayout.Button("NONE", EditorStyles.miniButtonRight, GUILayout.Width(120)))
		{
			for(int i = 0; i < b_modules.Length; i++) b_modules[i] = false;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.Space(4);
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = b_modules[0] ? ColorSelected : Color.white;	if(GUILayout.Button("Initial", EditorStyles.toolbarButton, GUILayout.Width(70))) b_modules[0] = !b_modules[0];
		GUI.color = b_modules[1] ? ColorSelected : Color.white;	if(GUILayout.Button("Emission", EditorStyles.toolbarButton, GUILayout.Width(70))) b_modules[1] = !b_modules[1];
		GUI.color = b_modules[2] ? ColorSelected : Color.white;	if(GUILayout.Button("Shape", EditorStyles.toolbarButton, GUILayout.Width(70))) b_modules[2] = !b_modules[2];
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = b_modules[3] ? ColorSelected : Color.white;	if(GUILayout.Button("Velocity", EditorStyles.toolbarButton, GUILayout.Width(70))) b_modules[3] = !b_modules[3];
		GUI.color = b_modules[4] ? ColorSelected : Color.white;	if(GUILayout.Button("Limit Velocity", EditorStyles.toolbarButton, GUILayout.Width(100))) b_modules[4] = !b_modules[4];
		GUI.color = b_modules[5] ? ColorSelected : Color.white;	if(GUILayout.Button("Force", EditorStyles.toolbarButton, GUILayout.Width(70))) b_modules[5] = !b_modules[5];
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = b_modules[6] ? ColorSelected : Color.white;	if(GUILayout.Button("Color over Lifetime", EditorStyles.toolbarButton, GUILayout.Width(120))) b_modules[6] = !b_modules[6];
		GUI.color = b_modules[7] ? ColorSelected : Color.white;	if(GUILayout.Button("Color by Speed", EditorStyles.toolbarButton, GUILayout.Width(120))) b_modules[7] = !b_modules[7];
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = b_modules[8] ? ColorSelected : Color.white;	if(GUILayout.Button("Size over Lifetime", EditorStyles.toolbarButton, GUILayout.Width(120))) b_modules[8] = !b_modules[8];
		GUI.color = b_modules[9] ? ColorSelected : Color.white;	if(GUILayout.Button("Size by Speed", EditorStyles.toolbarButton, GUILayout.Width(120))) b_modules[9] = !b_modules[9];
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = b_modules[10] ? ColorSelected : Color.white;	if(GUILayout.Button("Rotation over Lifetime", EditorStyles.toolbarButton, GUILayout.Width(120))) b_modules[10] = !b_modules[10];
		GUI.color = b_modules[11] ? ColorSelected : Color.white;	if(GUILayout.Button("Rotation by Speed", EditorStyles.toolbarButton, GUILayout.Width(120))) b_modules[11] = !b_modules[11];
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = b_modules[12] ? ColorSelected : Color.white;	if(GUILayout.Button("Collision", EditorStyles.toolbarButton, GUILayout.Width(100))) b_modules[12] = !b_modules[12];
		GUI.color = b_modules[13] ? ColorSelected : Color.white;	if(GUILayout.Button("Sub Emitters", EditorStyles.toolbarButton, GUILayout.Width(100))) b_modules[13] = !b_modules[13];
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUI.color = b_modules[14] ? ColorSelected : Color.white;	if(GUILayout.Button("Texture Animation", EditorStyles.toolbarButton, GUILayout.Width(110))) b_modules[14] = !b_modules[14];
		GUI.color = b_modules[15] ? ColorSelected : Color.white;	if(GUILayout.Button("Renderer", EditorStyles.toolbarButton, GUILayout.Width(90))) b_modules[15] = !b_modules[15];
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUI.color = Color.white;
		
		GUILayout.Space(4);
		if(GUILayout.Button("Copy properties to selected Object(s)"))
		{
			bool foundPs = false;
			foreach(GameObject go in Selection.gameObjects)
			{
				ParticleSystem[] systems;
				if(IncludeChildren)		systems = go.GetComponentsInChildren<ParticleSystem>(true);
				else 					systems = go.GetComponents<ParticleSystem>();
				
				if(systems.Length == 0) continue;
				
				foundPs = true;
				foreach(ParticleSystem system in systems)	CopyModules(sourceObject, system);
			}
			
			if(!foundPs)
			{
				Debug.LogWarning("CartoonFX Easy Editor: No Particle System found in the selected GameObject(s)!");
			}
		}
		
	//----------------------------------------------------------------
		
		GUILayout.Space(8);
		GUILayout.EndArea();
	}
	
	//Loop effects
	private void loopEffect(bool setLoop)
	{
		foreach(GameObject go in Selection.gameObjects)
		{
			//Scale Shuriken Particles Values
			ParticleSystem[] systems;
			if(IncludeChildren)		systems = go.GetComponentsInChildren<ParticleSystem>(true);
			else 					systems = go.GetComponents<ParticleSystem>();
			
			foreach(ParticleSystem ps in systems)
			{
				ps.loop = setLoop;
			}
		}
	}
	
	//Scale Size
	private void applyScale()
	{
		foreach(GameObject go in Selection.gameObjects)
		{
			//Scale Shuriken Particles Values
			ParticleSystem[] systems;
			if(IncludeChildren)		systems = go.GetComponentsInChildren<ParticleSystem>(true);
			else 					systems = go.GetComponents<ParticleSystem>();
			
			foreach(ParticleSystem ps in systems)
			{
				ScaleParticleValues(ps, go);
			}
			
			//Scale Lights' range
			Light[] lights = go.GetComponentsInChildren<Light>();
			foreach(Light light in lights)
			{
				light.range *= ScalingValue;
				light.transform.localPosition *= ScalingValue;
			}
		}
	}
	
	//Change Color
	private void applyColor()
	{
		foreach(GameObject go in Selection.gameObjects)
		{
			ParticleSystem[] systems;
			if(IncludeChildren)		systems = go.GetComponentsInChildren<ParticleSystem>(true);
			else 					systems = go.GetComponents<ParticleSystem>();
			
			foreach(ParticleSystem ps in systems)
			{
				SerializedObject psSerial = new SerializedObject(ps);
				if(!AffectAlpha)
				{
					psSerial.FindProperty("InitialModule.startColor.maxColor").colorValue = new Color(ColorValue.r, ColorValue.g, ColorValue.b, psSerial.FindProperty("InitialModule.startColor.maxColor").colorValue.a);
					psSerial.FindProperty("InitialModule.startColor.minColor").colorValue = new Color(ColorValue2.r, ColorValue2.g, ColorValue2.b, psSerial.FindProperty("InitialModule.startColor.minColor").colorValue.a);
				}
				else
				{
					psSerial.FindProperty("InitialModule.startColor.maxColor").colorValue = ColorValue;
					psSerial.FindProperty("InitialModule.startColor.minColor").colorValue = ColorValue2;
				}
				psSerial.ApplyModifiedProperties();
			}
		}
	}

	
	//Scale Lifetime only
	private void applyScaleLifetime()
	{
		foreach(GameObject go in Selection.gameObjects)
		{
			ParticleSystem[] systems;
			if(IncludeChildren)		systems = go.GetComponentsInChildren<ParticleSystem>(true);
			else 					systems = go.GetComponents<ParticleSystem>();
			
			//Scale Lifetime
			foreach(ParticleSystem ps in systems)
			{
				ps.playbackSpeed = (100.0f/LTScalingValue);
			}
		}
	}
	
	//Copy Selected Modules
	private void CopyModules(ParticleSystem source, ParticleSystem dest)
	{
		if(source == null)
		{
			Debug.LogWarning("CartoonFX Easy Editor: Select a source Particle System to copy properties from first!");
			return;
		}
		
		SerializedObject psSource = new SerializedObject(source);
		SerializedObject psDest = new SerializedObject(dest);
		
		//Inial Module
		if(b_modules[0])
		{
			psDest.FindProperty("prewarm").boolValue = psSource.FindProperty("prewarm").boolValue;
			psDest.FindProperty("lengthInSec").floatValue = psSource.FindProperty("lengthInSec").floatValue;
			psDest.FindProperty("moveWithTransform").boolValue = psSource.FindProperty("moveWithTransform").boolValue;
			
			GenericModuleCopy(psSource.FindProperty("InitialModule"), psDest.FindProperty("InitialModule"));
			
			dest.startDelay = source.startDelay;
			dest.loop = source.loop;
			dest.playOnAwake = source.playOnAwake;
			dest.playbackSpeed = source.playbackSpeed;
			dest.emissionRate = source.emissionRate;
			dest.startSpeed = source.startSpeed;
			dest.startSize = source.startSize;
			dest.startColor = source.startColor;
			dest.startRotation = source.startRotation;
			dest.startLifetime = source.startLifetime;
			dest.gravityModifier = source.gravityModifier;
		}
		
		//Emission
		if(b_modules[1])	GenericModuleCopy(psSource.FindProperty("EmissionModule"), psDest.FindProperty("EmissionModule"));
		
		//Shape
		if(b_modules[2])	GenericModuleCopy(psSource.FindProperty("ShapeModule"), psDest.FindProperty("ShapeModule"));
		
		//Velocity
		if(b_modules[3])	GenericModuleCopy(psSource.FindProperty("VelocityModule"), psDest.FindProperty("VelocityModule"));
		
		//Velocity Clamp
		if(b_modules[4])	GenericModuleCopy(psSource.FindProperty("ClampVelocityModule"), psDest.FindProperty("ClampVelocityModule"));
		
		//Force
		if(b_modules[5])	GenericModuleCopy(psSource.FindProperty("ForceModule"), psDest.FindProperty("ForceModule"));
		
		//Color
		if(b_modules[6])	GenericModuleCopy(psSource.FindProperty("ColorModule"), psDest.FindProperty("ColorModule"));
		
		//Color Speed
		if(b_modules[7])	GenericModuleCopy(psSource.FindProperty("ColorBySpeedModule"), psDest.FindProperty("ColorBySpeedModule"));
		
		//Size
		if(b_modules[8])	GenericModuleCopy(psSource.FindProperty("SizeModule"), psDest.FindProperty("SizeModule"));
		
		//Size Speed
		if(b_modules[9])	GenericModuleCopy(psSource.FindProperty("SizeBySpeedModule"), psDest.FindProperty("SizeBySpeedModule"));
		
		//Rotation
		if(b_modules[10])	GenericModuleCopy(psSource.FindProperty("RotationModule"), psDest.FindProperty("RotationModule"));
		
		//Rotation Speed
		if(b_modules[11])	GenericModuleCopy(psSource.FindProperty("RotationBySpeedModule"), psDest.FindProperty("RotationBySpeedModule"));
		
		//Collision
		if(b_modules[12])	GenericModuleCopy(psSource.FindProperty("CollisionModule"), psDest.FindProperty("CollisionModule"));
		
		//Sub Emitters
		if(b_modules[13])	SubModuleCopy(psSource, psDest);
		
		//Texture Animation
		if(b_modules[14])	GenericModuleCopy(psSource.FindProperty("UVModule"), psDest.FindProperty("UVModule"));
		
		//Renderer
		if(b_modules[15])
		{
			ParticleSystemRenderer rendSource = source.GetComponent<ParticleSystemRenderer>();
			ParticleSystemRenderer rendDest = dest.GetComponent<ParticleSystemRenderer>();
			
			psSource = new SerializedObject(rendSource);
			psDest = new SerializedObject(rendDest);
			
			SerializedProperty ss = psSource.GetIterator();
			ss.Next(true);
			
			SerializedProperty sd = psDest.GetIterator();
			sd.Next(true);
			
			GenericModuleCopy(ss, sd, false);
		}
	}
	
	//Copy One Module's Values
	private void GenericModuleCopy(SerializedProperty ss, SerializedProperty sd, bool depthBreak = true)
	{
		while(true)
		{
			//Next Property
			if(!ss.NextVisible(true))
			{
				break;
			}
			sd.NextVisible(true);
			
			//If end of module: break
			if(depthBreak && ss.depth == 0)
			{
				break;
			}
			
			bool found = true;
			
			switch(ss.propertyType)
			{
				case SerializedPropertyType.Boolean : 			sd.boolValue = ss.boolValue; break;
				case SerializedPropertyType.Integer : 			sd.intValue = ss.intValue; break;
				case SerializedPropertyType.Float : 			sd.floatValue = ss.floatValue; break;
				case SerializedPropertyType.Color : 			sd.colorValue = ss.colorValue; break;
				case SerializedPropertyType.Bounds : 			sd.boundsValue = ss.boundsValue; break;
				case SerializedPropertyType.Enum : 				sd.enumValueIndex = ss.enumValueIndex; break;
				case SerializedPropertyType.ObjectReference : 	sd.objectReferenceValue = ss.objectReferenceValue; break;
				case SerializedPropertyType.Rect : 				sd.rectValue = ss.rectValue; break;
				case SerializedPropertyType.String : 			sd.stringValue = ss.stringValue; break;
				case SerializedPropertyType.Vector2 : 			sd.vector2Value = ss.vector2Value; break;
				case SerializedPropertyType.Vector3 : 			sd.vector3Value = ss.vector3Value; break;
				case SerializedPropertyType.AnimationCurve : 	sd.animationCurveValue = ss.animationCurveValue; break;
				
				default: found = false; break;
			}
			
			if(!found)
			{
				found = true;
				
				switch(ss.type)
				{
					default: found = false; break;
				}
			}
		}
		
		//Apply Changes
		sd.serializedObject.ApplyModifiedProperties();
		
		ss.Dispose();
		sd.Dispose();
	}
	
	//Specific Copy for Sub Emitters Module (duplicate Sub Particle Systems)
	private void SubModuleCopy(SerializedObject source, SerializedObject dest)
	{
		dest.FindProperty("SubModule.enabled").boolValue = source.FindProperty("SubModule.enabled").boolValue;
		
		GameObject copy;
		if(source.FindProperty("SubModule.subEmitterBirth").objectReferenceValue != null)
		{
			//Duplicate sub Particle Emitter
			copy = (GameObject)Instantiate((source.FindProperty("SubModule.subEmitterBirth").objectReferenceValue as ParticleSystem).gameObject);
			
			//Set as child of destination
			Vector3 localPos = copy.transform.localPosition;
			Vector3 localScale = copy.transform.localScale;
			Vector3 localAngles = copy.transform.localEulerAngles;
			copy.transform.parent = (dest.targetObject as ParticleSystem).transform;
			copy.transform.localPosition = localPos;
			copy.transform.localScale = localScale;
			copy.transform.localEulerAngles = localAngles;
			
			//Assign as sub Particle Emitter
			dest.FindProperty("SubModule.subEmitterBirth").objectReferenceValue = copy;
		}
		
		if(source.FindProperty("SubModule.subEmitterDeath").objectReferenceValue != null)
		{
			//Duplicate sub Particle Emitter
			copy = (GameObject)Instantiate((source.FindProperty("SubModule.subEmitterDeath").objectReferenceValue as ParticleSystem).gameObject);
			
			//Set as child of destination
			Vector3 localPos = copy.transform.localPosition;
			Vector3 localScale = copy.transform.localScale;
			Vector3 localAngles = copy.transform.localEulerAngles;
			copy.transform.parent = (dest.targetObject as ParticleSystem).transform;
			copy.transform.localPosition = localPos;
			copy.transform.localScale = localScale;
			copy.transform.localEulerAngles = localAngles;
			
			//Assign as sub Particle Emitter
			dest.FindProperty("SubModule.subEmitterDeath").objectReferenceValue = copy;
		}
		
		if(source.FindProperty("SubModule.subEmitterCollision").objectReferenceValue != null)
		{
			//Duplicate sub Particle Emitter
			copy = (GameObject)Instantiate((source.FindProperty("SubModule.subEmitterCollision").objectReferenceValue as ParticleSystem).gameObject);
			
			//Set as child of destination
			Vector3 localPos = copy.transform.localPosition;
			Vector3 localScale = copy.transform.localScale;
			Vector3 localAngles = copy.transform.localEulerAngles;
			copy.transform.parent = (dest.targetObject as ParticleSystem).transform;
			copy.transform.localPosition = localPos;
			copy.transform.localScale = localScale;
			copy.transform.localEulerAngles = localAngles;
			
			//Assign as sub Particle Emitter
			dest.FindProperty("SubModule.subEmitterCollision").objectReferenceValue = copy;
		}
		
		//Apply Changes
		dest.ApplyModifiedProperties();
	}
	
	//Scale System
	private void ScaleParticleValues(ParticleSystem ps, GameObject parent)
	{
		//Particle System
		ps.startSize *= ScalingValue;
		ps.gravityModifier *= ScalingValue;
		if(ps.startSpeed > 0.01f) ps.startSpeed *= ScalingValue;
		if(ps.gameObject != parent)
			ps.transform.localPosition *= ScalingValue;
		
		SerializedObject psSerial = new SerializedObject(ps);
		
		//Scale Velocity Module
		if(psSerial.FindProperty("VelocityModule.enabled").boolValue)
		{
			psSerial.FindProperty("VelocityModule.x.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("VelocityModule.x.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("VelocityModule.x.maxCurve").animationCurveValue);
			psSerial.FindProperty("VelocityModule.y.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("VelocityModule.y.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("VelocityModule.y.maxCurve").animationCurveValue);
			psSerial.FindProperty("VelocityModule.z.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("VelocityModule.z.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("VelocityModule.z.maxCurve").animationCurveValue);
		}
		
		//Scale Limit Velocity Module
		if(psSerial.FindProperty("ClampVelocityModule.enabled").boolValue)
		{
			psSerial.FindProperty("ClampVelocityModule.x.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.x.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.x.maxCurve").animationCurveValue);
			psSerial.FindProperty("ClampVelocityModule.y.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.y.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.y.maxCurve").animationCurveValue);
			psSerial.FindProperty("ClampVelocityModule.z.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.z.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.z.maxCurve").animationCurveValue);
			
			psSerial.FindProperty("ClampVelocityModule.magnitude.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.magnitude.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("ClampVelocityModule.magnitude.maxCurve").animationCurveValue);
		}
		
		//Scale Force Module
		if(psSerial.FindProperty("ForceModule.enabled").boolValue)
		{
			psSerial.FindProperty("ForceModule.x.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("ForceModule.x.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("ForceModule.x.maxCurve").animationCurveValue);
			psSerial.FindProperty("ForceModule.y.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("ForceModule.y.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("ForceModule.y.maxCurve").animationCurveValue);
			psSerial.FindProperty("ForceModule.z.scalar").floatValue *= ScalingValue;
			IterateKeys(psSerial.FindProperty("ForceModule.z.minCurve").animationCurveValue);
			IterateKeys(psSerial.FindProperty("ForceModule.z.maxCurve").animationCurveValue);
		}
		
		//Scale Shape Module
		if(psSerial.FindProperty("ShapeModule.enabled").boolValue)
		{
			psSerial.FindProperty("ShapeModule.boxX").floatValue *= ScalingValue;
			psSerial.FindProperty("ShapeModule.boxY").floatValue *= ScalingValue;
			psSerial.FindProperty("ShapeModule.boxZ").floatValue *= ScalingValue;
			psSerial.FindProperty("ShapeModule.radius").floatValue *= ScalingValue;
			
			//Create a new scaled Mesh if there is a Mesh reference
			//(ShapeModule.type 6 == Mesh)
			if(psSerial.FindProperty("ShapeModule.type").intValue == 6)
			{
				Object obj = psSerial.FindProperty("ShapeModule.m_Mesh").objectReferenceValue;
				if(obj != null)
				{
					Mesh mesh = (Mesh)obj;
					string newMeshPath = AssetDatabase.GetAssetPath(mesh) + " x"+ScalingValue+" (scaled).asset";
					Mesh scaledMesh = (Mesh)AssetDatabase.LoadAssetAtPath(newMeshPath, typeof(Mesh));
					if(scaledMesh == null)
					{
						scaledMesh = DuplicateAndScaleMesh(mesh);
						AssetDatabase.CreateAsset(scaledMesh, newMeshPath);
					}
					
					//Apply new Mesh
					psSerial.FindProperty("ShapeModule.m_Mesh").objectReferenceValue = scaledMesh;
				}
			}
		}
		
		//Apply Modified Properties
		psSerial.ApplyModifiedProperties();
	}
	
	//Iterate and Scale Keys (Animation Curve)
	private void IterateKeys(AnimationCurve curve)
	{
		for(int i = 0; i < curve.keys.Length; i++)
		{
			curve.keys[i].value *= ScalingValue;
		}
	}
	
	//Create Scaled Mesh
	private Mesh DuplicateAndScaleMesh(Mesh mesh)
	{
		Mesh scaledMesh = new Mesh();
		
		Vector3[] scaledVertices = new Vector3[mesh.vertices.Length];
		for(int i = 0; i < scaledVertices.Length; i++)
		{
			scaledVertices[i] = mesh.vertices[i] * ScalingValue;
		}
		scaledMesh.vertices = scaledVertices;
		
		scaledMesh.normals = mesh.normals;
		scaledMesh.tangents = mesh.tangents;
		scaledMesh.triangles = mesh.triangles;
		scaledMesh.uv = mesh.uv;
		scaledMesh.uv2 = mesh.uv2;
		scaledMesh.colors = mesh.colors;
		
		return scaledMesh;
	}
}
