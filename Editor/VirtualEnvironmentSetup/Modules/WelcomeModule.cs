using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.VirtualEnvironmentSetup.Modules
{
	public class WelcomeModule : Module
	{
		public override string DisplayName { get => ""; }
		public override bool IsSkippable { get => false; }

		private Texture2D logo;

		public WelcomeModule()
		{
			this.logo = Resources.Load<Texture2D>("Textures/cave_logo");
		}

		public override void OnGUI(ModuleMaker maker)
		{
			WindowDesignUtility.DrawTexture(this.logo);

			EditorGUILayout.LabelField("Welcome to the CAVE Automated Virtual Enviroment Setup.", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(
			"The next steps allow you to configure your own virtual enviroment. "
			+ "Some functionalities are optional and you can choose if the environment should support them.",
			EditorStyles.wordWrappedLabel);
		}

		public override void Build(ModuleMaker maker, GameObject root, GameObject head)
		{
			ModuleMaker.AssetPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder("Assets", ModuleMaker.AssetPath));
			root.name = "Virtual Environment";
			head.name = "Head";

			head.AddComponent<AudioListener>();
		}
	}
}
