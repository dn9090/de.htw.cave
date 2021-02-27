using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Windows.Kinect;

namespace Htw.Cave.Kinect
{
	public enum KinectActorTemplateType
	{
		Mixed,
		OnlyDynamic,
	}

	public enum KinectActorTemplate
	{
		Head,
		HeadAndHands,
		HeadAndLimbs,
		Full
	}

	[CustomEditor(typeof(KinectActor))]
	public class KinectActorEditor : Editor
	{
		private KinectActor m_Me;

		private KinectActorTemplateType m_TemplateType;

		private KinectActorTemplate m_Template;

		private bool m_Filter;

		public void OnEnable()
		{
			this.m_Me = (KinectActor)target;
			this.m_TemplateType = KinectActorTemplateType.Mixed;
			this.m_Template = KinectActorTemplate.HeadAndHands;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Tracking Id", this.m_Me.trackingId.ToString());
			EditorGUILayout.LabelField("Created At", this.m_Me.createdAt + "s");
			EditorGUILayout.LabelField("Height", this.m_Me.height + "m");

			if(!Application.isPlaying && !ContainsTrackables())
				OnTrackableGUI();
		}

		private void OnTrackableGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Configure Trackables", EditorStyles.boldLabel);	

			this.m_TemplateType = (KinectActorTemplateType)EditorGUILayout.EnumPopup("Template Type", this.m_TemplateType);
			EditorGUILayout.LabelField(GetTemplateTypeDesc(this.m_TemplateType), EditorStyles.helpBox);	

			EditorGUILayout.Space();

			this.m_Template = (KinectActorTemplate)EditorGUILayout.EnumPopup("Template", this.m_Template);
			EditorGUILayout.LabelField(GetTemplateDesc(this.m_Template), EditorStyles.helpBox);

			EditorGUILayout.Space();

			this.m_Filter = EditorGUILayout.Toggle("Filter Position and Rotation", this.m_Filter);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Apply"))
				ApplyTemplate();

			EditorGUILayout.EndHorizontal();

			EditorGUI.EndChangeCheck();
		}

		private bool ContainsTrackables()
		{
			for(int i = 0; i < this.m_Me.transform.childCount; ++i)
				if(this.m_Me.transform.GetChild(i).GetComponent<KinectTrackable>() != null)
					return true;
			return false;
		}

		private void ApplyTemplate()
		{
			var parent = this.m_Me.transform;
			var jointTypes = new HashSet<JointType>();

			jointTypes.Add(JointType.Head);;

			switch(this.m_Template)
			{
				case KinectActorTemplate.HeadAndHands:
					jointTypes.UnionWith(new JointType[] {
						JointType.HandLeft, JointType.WristLeft, JointType.ThumbLeft, JointType.HandTipLeft,
						JointType.HandRight, JointType.WristRight, JointType.ThumbRight, JointType.HandTipRight});
					break;
				case KinectActorTemplate.HeadAndLimbs:
					jointTypes.UnionWith(new JointType[] {
						JointType.HandLeft, JointType.ElbowLeft, JointType.ShoulderLeft, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft,
						JointType.HandRight, JointType.ElbowRight, JointType.ShoulderRight, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight});
					break;
			}

			foreach(var value in Enum.GetValues(typeof(JointType))) // @Todo: This can be hard-coded in the future.
			{
				var jointType = (JointType)value;

				if(this.m_Template != KinectActorTemplate.Full && !jointTypes.Contains(jointType))
					continue;

				KinectTrackable child = null;

				if(this.m_TemplateType == KinectActorTemplateType.Mixed)
				{
					switch(jointType)
					{
						case JointType.Head:
							child = KinectTrackable.Create<KinectHead>("Kinect Head", parent);
							break;
						case JointType.HandLeft:
							child = KinectTrackable.Create<KinectHand>("Kinect Hand Left", parent,
								(trackable) => { trackable.handType = HandType.Left; });
							break;
						case JointType.HandRight:
							child = KinectTrackable.Create<KinectHand>("Kinect Hand Right", parent,
								(trackable) => { trackable.handType = HandType.Right; });
							break;
					}
				}

				if(child == null)
					child = KinectTrackable.Create<KinectDynamicJoint>("Kinect Dynamic Joint (" + jointType + ")", parent,
						(trackable) => { trackable.jointType = jointType; });

				child.filterPositionAndRotation = this.m_Filter;			
			}
		}

		private static string GetTemplateTypeDesc(KinectActorTemplateType type)
		{
			switch(type)
			{
				case KinectActorTemplateType.Mixed:
					return "Adds special components for joints when available. Otherwise the default dynamic joint will be used.";
				case KinectActorTemplateType.OnlyDynamic:
					return "Only the dynamic joint components will be used to construct the actor.";
				default:
					return "Unknown template type.";
			}
		}

		private static string GetTemplateDesc(KinectActorTemplate template)
		{
			switch(template)
			{
				case KinectActorTemplate.Head:
					return "Only the head joint will be added.";
				case KinectActorTemplate.HeadAndHands:
					return "Includes the head and multiple joints for both hands.";
				case KinectActorTemplate.HeadAndLimbs:
					return "Includes the head and multiple joints for the limbs.";
				case KinectActorTemplate.Full:
					return "All available joints will be added.";
				default:
					return "Unknown template.";
			}
		}

		[DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.InSelectionHierarchy)]
		public static void DrawGizmos(KinectActor actor, GizmoType type)
		{
			Gizmos.color = new Color(0f, 0.7f, 0f, 1f);
			Gizmos.DrawWireCube(actor.bounds.center, actor.bounds.size);
		}
	}
}