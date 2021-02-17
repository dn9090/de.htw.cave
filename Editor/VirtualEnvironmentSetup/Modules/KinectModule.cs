using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Kinect;

namespace Htw.Cave.VirtualEnvironmentSetup.Modules
{
	public class KinectModule : Module
	{
		public override string DisplayName { get => "Microsoft Kinect 2.0"; }
		public override bool IsSkippable { get => true; }

		private Texture2D background;
		private Vector3 sensorLocation;
		private float trackingAreaWidth;
		private float trackingAreaLength;
		private bool trackableHead;
		private bool trackableHands;

		public KinectModule()
		{
			this.background = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			this.sensorLocation = new Vector3(0f, 2.5f, 1.5f);
			this.trackingAreaWidth = 2f;
			this.trackingAreaLength = 2f;
			this.trackableHead = true;
			this.trackableHands = true;

			this.background.SetPixel(0, 0, new Color(0.275f, 0.145f, 0.537f));
			this.background.Apply();
		}

		public override void OnHeaderGUI()
		{
			WindowDesignUtility.DrawTitle("KINECT for Windows", Color.white, this.background);
		}

		public override void OnGUI(ModuleMaker maker)
		{
			WindowDesignUtility.SectionLabel("Sensor");

			bool wideMode = EditorGUIUtility.wideMode;
			EditorGUIUtility.wideMode = true;

			this.sensorLocation = EditorGUILayout.Vector3Field("Location", this.sensorLocation);

			EditorGUIUtility.wideMode = wideMode;

			EditorGUILayout.Separator();

			WindowDesignUtility.SectionLabel("Tracking Area");

			this.trackingAreaWidth = EditorGUILayout.FloatField("Width (m)", this.trackingAreaWidth);
			this.trackingAreaLength = EditorGUILayout.FloatField("Length (m)", this.trackingAreaLength);


			EditorGUILayout.Separator();

			WindowDesignUtility.SectionLabel("Trackables");

			this.trackableHead = EditorGUILayout.Toggle("Head", this.trackableHead);
			this.trackableHands = EditorGUILayout.Toggle("Hands", this.trackableHands);
		}

		public override void Build(ModuleMaker maker, GameObject root, GameObject head)
		{
			if(this.trackableHead)
				head.AddComponent<KinectTrackableHead>();

			if(this.trackableHands)
			{
				GameObject leftHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				leftHand.name = "Left Hand";
				leftHand.transform.parent = root.transform;
				leftHand.transform.SetSiblingIndex(1);
				leftHand.transform.localPosition = new Vector3(-0.3f, 1f, 0f);
				leftHand.GetComponent<SphereCollider>().radius = 0.1f;
				leftHand.AddComponent<KinectTrackableHand>().HandType = HandType.Left;
				MonoBehaviour.DestroyImmediate(leftHand.GetComponent<MeshRenderer>());

				GameObject rightHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				rightHand.name = "Right Hand";
				rightHand.transform.parent = root.transform;
				rightHand.transform.SetSiblingIndex(2);
				rightHand.transform.localPosition = new Vector3(0.3f, 1f, 0f);
				rightHand.GetComponent<SphereCollider>().radius = 0.1f;
				rightHand.AddComponent<KinectTrackableHand>().HandType = HandType.Right;
				MonoBehaviour.DestroyImmediate(rightHand.GetComponent<MeshRenderer>());
			}

			GameObject kinect = new GameObject("Kinect Play Area");
			kinect.transform.parent = root.transform;
			kinect.transform.localPosition = Vector3.zero;

			KinectPlayArea playArea = kinect.AddComponent<KinectPlayArea>();

			string assetPath = ModuleMaker.AssetPath + "/Kinect Settings.asset";
			KinectSettings asset = AssetDatabase.LoadAssetAtPath<KinectSettings>(assetPath);

			if(asset == null)
			{
				asset = ScriptableObject.CreateInstance<KinectSettings>();
				asset.SensorLocation = this.sensorLocation;
				asset.TrackingArea = new Rect(0f, 0f, this.trackingAreaWidth, this.trackingAreaLength);
				AssetDatabase.CreateAsset(asset, assetPath);
			}

			playArea.Settings = asset;
		}
	}
}
