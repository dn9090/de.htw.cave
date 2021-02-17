using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.VirtualEnvironmentSetup.Modules;

namespace Htw.Cave.VirtualEnvironmentSetup
{
	public class SetupWindow : EditorWindow
	{
		private ModuleMaker moduleMaker;

		[MenuItem("GameObject/Virtual Environment Setup", false, 21)]
		public static void ShowWindow()
		{
			SetupWindow window = EditorWindow.GetWindow<SetupWindow>(true, "Virtual Environment Setup");
			window.minSize = new Vector2(400f, 260f);
			window.maxSize = new Vector2(500f, 260f);
			window.Show();

			WindowDesignUtility.SizeX = window.maxSize.x;
			WindowDesignUtility.SizeY = window.maxSize.y;
		}

		public void OnEnable()
		{
			this.moduleMaker = new ModuleMaker();
			this.moduleMaker.Add(new WelcomeModule());
			this.moduleMaker.Add(new ProjectorModule());
			this.moduleMaker.Add(new KinectModule());
			this.moduleMaker.Add(new JoyconModule());
			this.moduleMaker.Add(new ControlsModule());
			this.moduleMaker.Add(new PrefabModule());
		}

		public void OnGUI()
		{
			HeaderGUI();
			WindowGUI();
			FooterGUI();
		}

		public void OnDisable()
		{
		}

		private void HeaderGUI()
		{
			WindowDesignUtility.ProgressBar(this.moduleMaker.GetProgress());
			GUILayout.Space(10f);
			this.moduleMaker.OnHeaderGUI();
		}

		private void WindowGUI()
		{
			if(this.moduleMaker.CanBeSkipped())
			{
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("Support " + this.moduleMaker.Title());

				bool support = EditorGUILayout.Toggle(!this.moduleMaker.IsSkippedInBuild());
				this.moduleMaker.SkipInBuild(!support);

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				using(new EditorGUI.DisabledScope(this.moduleMaker.IsSkippedInBuild()))
					this.moduleMaker.OnGUI();
			} else {
				this.moduleMaker.OnGUI();
			}
		}

		private void FooterGUI()
		{
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();

			if(this.moduleMaker.HasPrevious())
			{
				if(WindowDesignUtility.SmallButton("Back"))
				{
					this.moduleMaker.Previous();
					base.Repaint();
				}
			}

			GUILayout.FlexibleSpace();

			if(this.moduleMaker.HasNext())
			{
				if(WindowDesignUtility.SmallButton("Next"))
				{
					this.moduleMaker.Next();
					base.Repaint();
				}
			} else {
				if(WindowDesignUtility.SmallButton("Finish"))
				{
					this.moduleMaker.Build();
					base.Close();
				}
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}
