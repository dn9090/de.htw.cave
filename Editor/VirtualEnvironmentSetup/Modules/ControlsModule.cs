using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Controls;
using Htw.Cave.Kinect;
using Htw.Cave.Joycons;

namespace Htw.Cave.VirtualEnvironmentSetup.Modules
{
	public class ControlsModule : Module
	{
		public override string DisplayName { get => "Controls"; }
		public override bool IsSkippable { get => false; }

		private bool firstPerson;
		private bool teleporter;
		private bool freeLook;

		public ControlsModule()
		{
			this.firstPerson = true;
			this.teleporter = true;
		}

		public override void OnGUI(ModuleMaker maker)
		{
			WindowDesignUtility.SectionLabel("Physical Controls");

			bool joycon = maker.IsModuleAvailable<JoyconModule>();

			using(new EditorGUI.DisabledScope(!joycon))
			{
				this.firstPerson = EditorGUILayout.Toggle("First Person", this.firstPerson);

				if(!joycon)
					EditorGUILayout.HelpBox("Nintendo Joy-Con support required.", MessageType.Warning);
			}

			bool kinect = maker.IsModuleAvailable<KinectModule>();

			using(new EditorGUI.DisabledScope(!joycon || !kinect))
			{
				this.teleporter = EditorGUILayout.Toggle("Teleporter", this.teleporter);

				if(!joycon || !kinect)
					EditorGUILayout.HelpBox("Nintendo Joy-Con and Microsoft Kinect 2.0 support required.", MessageType.Warning);
			}

			EditorGUILayout.Separator();

			using(new EditorGUI.DisabledScope(this.firstPerson || this.teleporter))
			{
				WindowDesignUtility.SectionLabel("Non-Physical Controls");

				this.freeLook = EditorGUILayout.Toggle("Free Look", this.freeLook);

				if(!joycon)
					EditorGUILayout.HelpBox("Nintendo Joy-Con support required.", MessageType.Warning);
			}
		}

		public override void Build(ModuleMaker maker, GameObject root, GameObject head)
		{
			if(maker.IsModuleAvailable<JoyconModule>())
			{
				if(this.firstPerson)
				{
					AttachFirstPerson(root);
					TryAttachHeightControlledCollider(head);
				}

				if(this.teleporter && maker.IsModuleAvailable<KinectModule>())
				{
					AttachTeleporter(root);
					TryAttachHeightControlledCollider(head);
				}

				bool physicals = this.firstPerson || this.teleporter;

				if(!physicals && this.freeLook)
					AttachFreeLook(root);
			}
		}

		private void AttachFirstPerson(GameObject root)
		{
			root.AddComponent<FirstPersonControls>();
		}

		private void AttachTeleporter(GameObject root)
		{
			Transform parent = root.transform;

			KinectTrackableHand hand = root.GetComponentsInChildren<KinectTrackableHand>().FirstOrDefault(h => h.HandType == HandType.Left);

			if(hand != null)
				parent = hand.transform;

			Teleporter teleporter = (new GameObject("Teleporter")).AddComponent<Teleporter>();
			teleporter.transform.parent = parent;
			teleporter.transform.localPosition = hand == null ? Vector3.up : Vector3.zero;
			teleporter.transform.localEulerAngles = Vector3.zero;
			root.AddComponent<TeleporterControls>().Teleporter = teleporter;
		}

		private void AttachFreeLook(GameObject root)
		{
			root.AddComponent<FreeLookControls>();
		}

		private void TryAttachHeightControlledCollider(GameObject head)
		{
			if(head.GetComponent<HeightControlledCollider>() == null)
				head.AddComponent<HeightControlledCollider>();
		}
	}
}
