using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Windows.Kinect;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectSkeletonJoint))]
	public class KinectSkeletonJointEditor : Editor
	{
		private KinectSkeletonJoint m_Me;

		public void OnEnable()
		{
			this.m_Me = (KinectSkeletonJoint)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			this.m_Me.jointType = (JointType)EditorGUILayout.EnumPopup("Joint Type", this.m_Me.jointType);
			this.m_Me.applyFilter = EditorGUILayout.Toggle("Filter", this.m_Me.applyFilter);
			
			EditorGUI.EndChangeCheck();
		}

		[DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy)]
		public static void DrawJointGizmos(KinectSkeletonJoint joint, GizmoType type)
		{
			Gizmos.color = KinectEditorUtils.darkGreen;
			Gizmos.DrawWireSphere(joint.transform.position, 0.02f);
		}
	}
}