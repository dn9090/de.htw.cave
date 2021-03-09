using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Windows.Kinect;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Need to be aligned with <see cref="KinectActorEditor.Styles.bodyPart"/>.
	/// </summary>
	[Flags]
	internal enum KinectJointMask
	{
		None = 0,
		Torso = 1 << 0,
		Head = 1 << 1,
		LegLeft = 1 << 2,
		LegRight = 1 << 3,
		ArmLeft = 1 << 4,
		ArmRight = 1 << 5,
		HandLeft = 1 << 6,
		HandRight = 1 << 7,
		All = ~(-1 << 8)
	}

	[CustomEditor(typeof(KinectActor))]
	public class KinectActorEditor : Editor
	{
		/// <summary>
		/// Copied from AvatarMaskInspector.cs in the UnityCsReference.
		/// </summary>
		private static class Styles
		{
			public static GUIContent unityDude = EditorGUIUtility.IconContent("AvatarInspector/BodySIlhouette");
			
			public static GUIContent pickingTexture = EditorGUIUtility.IconContent("AvatarInspector/BodyPartPicker");

			public static GUIContent[] bodyPart =
			{
				EditorGUIUtility.IconContent("AvatarInspector/Torso"),

				EditorGUIUtility.IconContent("AvatarInspector/Head"),

				EditorGUIUtility.IconContent("AvatarInspector/LeftLeg"),
				EditorGUIUtility.IconContent("AvatarInspector/RightLeg"),

				EditorGUIUtility.IconContent("AvatarInspector/LeftArm"),
				EditorGUIUtility.IconContent("AvatarInspector/RightArm"),

				EditorGUIUtility.IconContent("AvatarInspector/LeftFingers"),
				EditorGUIUtility.IconContent("AvatarInspector/RightFingers")
			};

			public static Color[] maskBodyPartPicker =
			{
				new Color(  0 / 255.0f, 174 / 255.0f, 240 / 255.0f), // body
				new Color(171 / 255.0f, 160 / 255.0f,   0 / 255.0f), // head

				new Color(  0 / 255.0f, 255 / 255.0f, 255 / 255.0f), // ll
				new Color(247 / 255.0f, 151 / 255.0f, 121 / 255.0f), // rl

				new Color( 0 / 255.0f, 255 / 255.0f,   0 / 255.0f), // la
				new Color(86 / 255.0f, 116 / 255.0f, 185 / 255.0f), // ra

				new Color(255 / 255.0f,   255 / 255.0f,   0 / 255.0f), // lh
				new Color(130 / 255.0f,   202 / 255.0f, 156 / 255.0f), // rh

				new Color( 82 / 255.0f,    82 / 255.0f,  82 / 255.0f), // lfi
				new Color(255 / 255.0f,   115 / 255.0f, 115 / 255.0f), // rfi
				new Color(159 / 255.0f,   159 / 255.0f, 159 / 255.0f), // lhi
				new Color(202 / 255.0f,   202 / 255.0f, 202 / 255.0f), // rhi

				new Color(101 / 255.0f,   101 / 255.0f, 101 / 255.0f), // hi
			};
		}

		private KinectActor m_Me;

		private KinectJointMask m_JointMask;

		private bool m_Filter;

		public void OnEnable()
		{
			this.m_Me = (KinectActor)target;
			this.m_JointMask |= KinectJointMask.Head | KinectJointMask.HandLeft | KinectJointMask.HandRight;
			this.m_Filter = true;
		}

		public override void OnInspectorGUI()
		{
			if(Application.isPlaying)
			{
				EditorGUILayout.LabelField("Tracking Id", this.m_Me.trackingId.ToString());
				EditorGUILayout.LabelField("Created At", this.m_Me.createdAt + "s");
				EditorGUILayout.LabelField("Height", this.m_Me.height + "m");
			}
			
			if(!Application.isPlaying)
			{
				if(ContainsTrackables())
					EditorGUILayout.LabelField("Tracked Joints", CountActiveTrackables().ToString());
				else
					OnHumanuidGUI();	
			}	
		}

		private void OnHumanuidGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Configure Joints", EditorStyles.boldLabel);	

			OnHumaniodMaskGUI();

			EditorGUILayout.Space();

			this.m_Filter = EditorGUILayout.Toggle("Filter Position And Rotation", this.m_Filter);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			using(new EditorGUI.DisabledScope((int)this.m_JointMask == 0))
			{
				if(GUILayout.Button("Apply"))
					AddHumaniodJoints();
			}

			EditorGUILayout.EndHorizontal();

			EditorGUI.EndChangeCheck();
		}

		private void OnHumaniodMaskGUI()
		{
			if(!Styles.unityDude.image)
			{
				EditorGUILayout.LabelField("Loading humaniod failed.");
				return;
			}

			Rect rect = GUILayoutUtility.GetRect(Styles.unityDude, GUIStyle.none, GUILayout.MaxWidth(Styles.unityDude.image.width));
			rect.x += (EditorGUIUtility.currentViewWidth  - rect.width) / 2;

			Color oldColor = GUI.color;

			GUI.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
			GUI.DrawTexture(rect, Styles.unityDude.image);

			for(int i = 0; i < Styles.bodyPart.Length; ++i)
			{
				var flag = (KinectJointMask)(1 << i);
				GUI.color = this.m_JointMask.HasFlag(flag) ? Color.green : Color.red;
				
				if(Styles.bodyPart[i].image)
					GUI.DrawTexture(rect, Styles.bodyPart[i].image);
			}

			GUI.color = oldColor;

			PickHumaniodJoint(rect);
		}

		private void PickHumaniodJoint(Rect rect)
		{
			if(!Styles.pickingTexture.image)
				return;
			
			int id = GUIUtility.GetControlID(FocusType.Passive, rect);
			Event evt = Event.current;

			if(evt.GetTypeForControl(id) != EventType.MouseDown || !rect.Contains(evt.mousePosition))
				return;

			evt.Use();

			int x = (int)evt.mousePosition.x - (int)rect.x;
			int y = Styles.unityDude.image.height - ((int)evt.mousePosition.y - (int)rect.y);

			Texture2D pickTexture = Styles.pickingTexture.image as Texture2D;
			Color color = pickTexture.GetPixel(x, y);

			bool anyBodyPartPick = false;
			for (int i = 0; i < Styles.bodyPart.Length; i++)
			{
				if (Styles.maskBodyPartPicker[i] == color)
				{
					GUI.changed = true;

					var flag = (KinectJointMask)(1 << i);

					if(this.m_JointMask.HasFlag(flag))
						this.m_JointMask &= ~flag;
					else
						this.m_JointMask |= flag;

					anyBodyPartPick = true;
				}
			}

			if (!anyBodyPartPick)
			{
				this.m_JointMask = (int)this.m_JointMask == 0
					? KinectJointMask.All
					: KinectJointMask.None;

				GUI.changed = true;
			}
		}

		private void AddHumaniodJoints()
		{
			var parent = this.m_Me.transform;

			Action<KinectTrackable> applyFilter = (trackable) => { trackable.filterPositionAndRotation = this.m_Filter; };

			if(this.m_JointMask.HasFlag(KinectJointMask.Head))
			{
				KinectTrackable.Create<KinectHead>("Kinect Head", parent, applyFilter);
				KinectDynamicJoint.Create(PredefinedJointTypes.head.Where(jointType => jointType != JointType.Head).ToArray(), parent, applyFilter);
			}
			
			if(this.m_JointMask.HasFlag(KinectJointMask.Torso))
				KinectDynamicJoint.Create(PredefinedJointTypes.torso, parent, applyFilter);

			if(this.m_JointMask.HasFlag(KinectJointMask.LegLeft))
				KinectDynamicJoint.Create(PredefinedJointTypes.legLeft, parent, applyFilter);

			if(this.m_JointMask.HasFlag(KinectJointMask.LegRight))
				KinectDynamicJoint.Create(PredefinedJointTypes.legRight, parent, applyFilter);

			if(this.m_JointMask.HasFlag(KinectJointMask.ArmLeft))
				KinectDynamicJoint.Create(PredefinedJointTypes.armLeft, parent, applyFilter);

			if(this.m_JointMask.HasFlag(KinectJointMask.ArmRight))
				KinectDynamicJoint.Create(PredefinedJointTypes.armRight, parent, applyFilter);

			if(this.m_JointMask.HasFlag(KinectJointMask.HandLeft))
			{
				KinectTrackable.Create<KinectHand>("Kinect Hand Left", parent,
					(trackable) => { trackable.handType = HandType.Left; trackable.filterPositionAndRotation = this.m_Filter; });

				KinectDynamicJoint.Create(PredefinedJointTypes.handLeft.Where(jointType => jointType != JointType.HandLeft).ToArray(), parent, applyFilter);
			}

			if(this.m_JointMask.HasFlag(KinectJointMask.HandRight))
			{
				KinectTrackable.Create<KinectHand>("Kinect Hand Right", parent,
					(trackable) => { trackable.handType = HandType.Right; trackable.filterPositionAndRotation = this.m_Filter; });

				KinectDynamicJoint.Create(PredefinedJointTypes.handRight.Where(jointType => jointType != JointType.HandRight).ToArray(), parent, applyFilter);
			}
		}

		private bool ContainsTrackables()
		{
			for(int i = 0; i < this.m_Me.transform.childCount; ++i)
				if(this.m_Me.transform.GetChild(i).GetComponent<KinectTrackable>() != null)
					return true;
			return false;
		}

		private int CountActiveTrackables()
		{
			int count = 0;

			for(int i = 0; i < this.m_Me.transform.childCount; ++i)
			{
				var trackable = this.m_Me.transform.GetChild(i).GetComponent<KinectTrackable>();
				if(trackable != null && trackable.enabled)
					++count;
			}
				
			return count;
		}

		[DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.InSelectionHierarchy)]
		public static void DrawGizmos(KinectActor actor, GizmoType type)
		{
			Gizmos.color = new Color(0f, 0.7f, 0f, 1f);
			Gizmos.DrawWireCube(actor.bounds.center, actor.bounds.size);
		}
	}
}