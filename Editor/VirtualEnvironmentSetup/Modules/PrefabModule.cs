using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEditor;

namespace Htw.Cave.VirtualEnvironmentSetup.Modules
{
	public class PrefabModule : Module
	{
		public override string DisplayName { get => "Prefab"; }
		public override bool IsSkippable { get => false; }

		private string name;
		private bool installSDK;
		private bool exclusiveFullScreen;

		public PrefabModule()
		{
			this.name = "CAVE";
			this.installSDK = true;
			this.exclusiveFullScreen = true;
		}

		public override void OnGUI(ModuleMaker maker)
		{
			EditorGUILayout.LabelField("You are almost finished!", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(
			"The virtual enviroment will be packed into a Prefab which you can "
			+ "drag and drop to other scenes or projects where the package is installed.",
			EditorStyles.wordWrappedLabel);

			EditorGUILayout.Space();

			this.name = EditorGUILayout.TextField("Prefab Name", this.name);

			if(string.IsNullOrEmpty(this.name))
			{
				ModuleMaker.AssetPath = "Unnamed Assets";
				EditorGUILayout.HelpBox("Please enter a name.", MessageType.Error);
			} else {
				ModuleMaker.AssetPath = this.name + " Assets";
			}

			this.installSDK = EditorGUILayout.Toggle("Install Stereo SDK", this.installSDK);
			this.exclusiveFullScreen = EditorGUILayout.Toggle("Exclusive Full Screen", this.exclusiveFullScreen);
		}

		public override void Build(ModuleMaker maker, GameObject root, GameObject head)
		{
			if(string.IsNullOrEmpty(this.name))
				this.name = "Unnamed";

			root.name = this.name;

			GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ModuleMaker.AssetPath + "/" + this.name + ".prefab");
			EditorGUIUtility.PingObject(prefab);
			MonoBehaviour.DestroyImmediate(root);
			GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

			Undo.RegisterCreatedObjectUndo(instance, "Instantiate Virtual Environment");

			if(this.installSDK)
				InstallSDK();

			if(this.exclusiveFullScreen)
				PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
		}

		private void InstallSDK()
		{
			const string sdk = "stereo";
			BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;

			PlayerSettings.SetVirtualRealitySupported(buildTargetGroup, true);

			if(!PlayerSettings.GetVirtualRealitySDKs(buildTargetGroup).Contains(sdk))
				PlayerSettings.SetVirtualRealitySDKs(buildTargetGroup, new string[]{ sdk });
		}
	}
}
