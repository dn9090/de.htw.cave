using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Projectors;

namespace Htw.Cave.VirtualEnvironmentSetup.Modules
{
	public class ProjectorModule : Module
	{
		[System.Serializable]
		public enum ProjectorTemplate
		{
			Basic,
			Cube,
			Custom
		}

		[Flags]
		[System.Serializable]
		public enum ProjectorSelection
		{
			Front = 1 << 0,
			Back = 1 << 1,
			Top = 1 << 2,
			Bottom = 1 << 3,
			Left = 1 << 4,
			Right = 1 << 5
		}

		public override string DisplayName { get => "Projectors"; }
		public override bool IsSkippable { get => false; }

		private ProjectorTemplate template;
		private ProjectorSelection selection;
		private CameraTarget target;
		private float width;
		private float height;
		private float length;
		private Vector3 position;

		public ProjectorModule()
		{
			this.template = ProjectorTemplate.Basic;
			this.selection = (ProjectorSelection)0;
			this.width = 3f;
			this.height = 2.45f;
			this.length = 3f;
			this.position = SceneCameraRay();
		}

		public override void OnGUI(ModuleMaker maker)
		{
			WindowDesignUtility.SectionLabel("Surfaces");

			EditorGUILayout.BeginHorizontal();
			this.template = (ProjectorTemplate)EditorGUILayout.EnumPopup("Template", this.template);
			TemplateToSelection();

			using(new EditorGUI.DisabledScope(this.template != ProjectorTemplate.Custom))
				this.selection = (ProjectorSelection)EditorGUILayout.EnumFlagsField(this.selection);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			WindowDesignUtility.SectionLabel("Rendering");

			this.target = (CameraTarget)EditorGUILayout.EnumPopup("Camera Target", this.target);

			EditorGUILayout.Separator();

			WindowDesignUtility.SectionLabel("Dimensions");

			bool wideMode = EditorGUIUtility.wideMode;
			EditorGUIUtility.wideMode = true;

			this.width = EditorGUILayout.FloatField("Width (m)", this.width);
			this.height = EditorGUILayout.FloatField("Height (m)", this.height);
			this.length = EditorGUILayout.FloatField("Length (m)", this.length);
			this.position = EditorGUILayout.Vector3Field("Position", this.position);

			EditorGUIUtility.wideMode = wideMode;
		}

		public override void Build(ModuleMaker maker, GameObject root, GameObject head)
		{
			ProjectorBrain brain = root.AddComponent<ProjectorBrain>();
			brain.transform.position = this.position;

			ProjectorEyes eyes = head.AddComponent<ProjectorEyes>();
			eyes.transform.parent = brain.transform;
			eyes.transform.localPosition = new Vector3(0f, 1.8f, 0f);

			ProjectorMount mount = (new GameObject("Mount")).AddComponent<ProjectorMount>();
			mount.transform.parent = brain.transform;
			mount.transform.localPosition = eyes.transform.localPosition;
			mount.Eyes = eyes;

			if(this.selection.HasFlag(ProjectorSelection.Front))
			{
				ProjectorPlane plane = CreatePlane(brain, new Vector3(0, this.height * 0.5f, this.length * 0.5f), Vector3.zero);
				ProjectorCamera camera = CreateCamera(mount, plane);
				CreateCameraConfiguration(camera, SelectionToDisplayId(ProjectorSelection.Front), this.width, this.height);
			}

			if(this.selection.HasFlag(ProjectorSelection.Back))
			{
				ProjectorPlane plane = CreatePlane(brain, new Vector3(0, this.height * 0.5f, -this.length * 0.5f), Vector3.up * 180f);
				ProjectorCamera camera = CreateCamera(mount, plane);
				CreateCameraConfiguration(camera, SelectionToDisplayId(ProjectorSelection.Back), this.width, this.height);
			}

			if(this.selection.HasFlag(ProjectorSelection.Top))
			{
				ProjectorPlane plane = CreatePlane(brain, new Vector3(0, this.height, 0), Vector3.right * -90f);
				ProjectorCamera camera = CreateCamera(mount, plane);
				CreateCameraConfiguration(camera, SelectionToDisplayId(ProjectorSelection.Top), this.width, this.length);
			}

			if(this.selection.HasFlag(ProjectorSelection.Bottom))
			{
				ProjectorPlane plane = CreatePlane(brain, Vector3.zero, Vector3.right * 90f);
				ProjectorCamera camera = CreateCamera(mount, plane);
				CreateCameraConfiguration(camera, SelectionToDisplayId(ProjectorSelection.Bottom), this.width, this.length);
			}

			if(this.selection.HasFlag(ProjectorSelection.Left))
			{
				ProjectorPlane plane = CreatePlane(brain, new Vector3(-this.width * 0.5f, this.height * 0.5f, 0), Vector3.up * -90f);
				ProjectorCamera camera = CreateCamera(mount, plane);
				CreateCameraConfiguration(camera, SelectionToDisplayId(ProjectorSelection.Left), this.length, this.height);
			}

			if(this.selection.HasFlag(ProjectorSelection.Right))
			{
				ProjectorPlane plane = CreatePlane(brain, new Vector3(this.width * 0.5f, this.height * 0.5f, 0), Vector3.up * 90f);
				ProjectorCamera camera = CreateCamera(mount, plane);
				CreateCameraConfiguration(camera, SelectionToDisplayId(ProjectorSelection.Right), this.length, this.height);
			}

			string assetPath = ModuleMaker.AssetPath + "/Projector Settings.asset";
			ProjectorSettings asset = AssetDatabase.LoadAssetAtPath<ProjectorSettings>(assetPath);

			if(asset == null)
			{
				asset = ScriptableObject.CreateInstance<ProjectorSettings>();
				asset.CameraTarget = this.target;
				asset.ForceFullScreen = false;
				asset.StereoSeparation = 0.064f;
				asset.StereoConvergence = 10f;
				asset.NearClipPlane = 0.01f;
				asset.FarClipPlane = 1000f;
				AssetDatabase.CreateAsset(asset, assetPath);
			}

			brain.Settings = asset;
		}

		private void TemplateToSelection()
		{
			switch(this.template)
			{
				case ProjectorTemplate.Basic:
					this.selection = ProjectorSelection.Front | ProjectorSelection.Bottom | ProjectorSelection.Left | ProjectorSelection.Right;
					break;
				case ProjectorTemplate.Cube:
					this.selection = (ProjectorSelection)~0;
					break;
			}
		}

		private int SelectionToDisplayId(ProjectorSelection selection)
		{
			int i = 0;

			if(this.selection.HasFlag(ProjectorSelection.Left))
			{
				if(selection == ProjectorSelection.Left)
					return i;

				++i;
			}

			if(this.selection.HasFlag(ProjectorSelection.Front))
			{
				if(selection == ProjectorSelection.Front)
					return i;

				++i;
			}

			if(this.selection.HasFlag(ProjectorSelection.Right))
			{
				if(selection == ProjectorSelection.Right)
					return i;

				++i;
			}

			if(this.selection.HasFlag(ProjectorSelection.Back))
			{
				if(selection == ProjectorSelection.Back)
					return i;

				++i;
			}

			if(this.selection.HasFlag(ProjectorSelection.Bottom))
			{
				if(selection == ProjectorSelection.Bottom)
					return i;

				++i;
			}

			if(this.selection.HasFlag(ProjectorSelection.Top))
			{
				if(selection == ProjectorSelection.Top)
					return i;

				++i;
			}

			return i;
		}

		private string RotationToName(Vector3 eulerAngles)
		{
			if(Mathf.Approximately(eulerAngles.y, 90f))
				return "Right";

			if(Mathf.Approximately(eulerAngles.y, -90f) || Mathf.Approximately(eulerAngles.y, 270f))
				return "Left";

			if(Mathf.Approximately(eulerAngles.x, 90f))
				return "Bottom";

			if(Mathf.Approximately(eulerAngles.x, -90f) || Mathf.Approximately(eulerAngles.x, 270f))
				return "Top";

			if(Mathf.Approximately(eulerAngles.y, 180f) || Mathf.Approximately(eulerAngles.y, -180f))
				return "Back";

			return "Front";
		}

		private ProjectorPlane CreatePlane(ProjectorBrain brain, Vector3 position, Vector3 angles)
		{
			GameObject go = new GameObject();
			go.transform.parent = brain.transform;
			go.transform.localPosition = position;
			go.transform.localEulerAngles = angles;

			go.name = "Plane " + RotationToName(go.transform.localEulerAngles);

			return go.AddComponent<ProjectorPlane>();
		}

		private ProjectorCamera CreateCamera(ProjectorMount mount, ProjectorPlane plane)
		{
			GameObject go = new GameObject();
			go.transform.parent = mount.transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.LookAt(plane.transform);
			go.AddComponent<Camera>();

			// small hack to archive 90 degree angles
			Vector3 angles = go.transform.localEulerAngles / 90f;
			angles = new Vector3(Mathf.Round(angles.x), Mathf.Round(angles.y), Mathf.Round(angles.z));
			go.transform.localEulerAngles = angles * 90f;

			go.name = "Camera " + RotationToName(go.transform.localEulerAngles);

			ProjectorCamera camera = go.AddComponent<ProjectorCamera>();
			camera.Plane = plane;

			return camera;
		}

		private void CreateCameraConfiguration(ProjectorCamera camera, int id, float width, float height)
		{
			string name = RotationToName(camera.transform.localEulerAngles);

			string assetPath = ModuleMaker.AssetPath + "/Projector Configuration " + name + ".asset";
			ProjectorConfiguration asset = AssetDatabase.LoadAssetAtPath<ProjectorConfiguration>(assetPath);

			if(asset == null)
			{
				asset = ScriptableObject.CreateInstance<ProjectorConfiguration>();
				asset.DisplayId = id;
				asset.DisplayName = name;
				asset.Width = width;
				asset.Height = height;
				asset.FieldOfView = 90f;
				asset.InvertStereo = false;
				AssetDatabase.CreateAsset(asset, assetPath);
			}

			camera.Configuration = asset;
		}

		private Vector3 SceneCameraRay()
		{
			Camera cam = SceneView.lastActiveSceneView.camera;

			RaycastHit hit;

			if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 100f))
				return hit.point;

			return cam.transform.position + cam.transform.forward * 10;
		}
	}
}
