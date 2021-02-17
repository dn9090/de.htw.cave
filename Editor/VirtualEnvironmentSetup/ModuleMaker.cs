using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Htw.Cave.VirtualEnvironmentSetup
{
	public class ModuleMaker
	{
		public static string AssetPath { get; set; }

		private List<Module> modules;
		private int current;

		public ModuleMaker()
		{
			this.modules = new List<Module>();
			this.current = 0;
		}

		public void Add(Module module)
		{
			if(!modules.Any(m => m.GetType() == module.GetType()))
				this.modules.Add(module);
		}

		public void Next()
		{
			++this.current;
		}

		public void Previous()
		{
			--this.current;
		}

		public void OnHeaderGUI()
		{
			Current().OnHeaderGUI();
		}

		public void OnGUI()
		{
			Current().OnGUI(this);
		}

		public void Build()
		{
			GameObject root = new GameObject();
			GameObject head = new GameObject();

			float step = 1f / this.modules.Count;

			for(int i = 0; i < this.modules.Count; ++i)
			{
				if(this.modules[i].SkipInBuild)
					continue;

				EditorUtility.DisplayProgressBar("Create Virtual Environment", "Building " + this.modules[i].DisplayName + " .", i * step);
				this.modules[i].Build(this, root, head);
			}

			EditorUtility.ClearProgressBar();

			EditorSceneManager.SaveOpenScenes();
		}

		public Module Current()
		{
			return this.modules[this.current];
		}

		public string Title()
		{
			return Current().DisplayName;
		}

		public float GetProgress()
		{
			return (1f / this.modules.Count) * this.current;
		}

		public bool HasPrevious()
		{
			return this.current > 0;
		}

		public bool HasNext()
		{
			return this.current < (this.modules.Count - 1);
		}

		public bool CanBeSkipped()
		{
			return Current().IsSkippable;
		}

		public bool IsSkippedInBuild()
		{
			return Current().SkipInBuild;
		}

		public void SkipInBuild(bool skip)
		{
			Current().SkipInBuild = skip;
		}

		public bool IsModuleAvailable<T>() where T : Module
		{
			Module module = modules.FirstOrDefault(m => m.GetType() == typeof(T));

			if(module != null)
				return !module.SkipInBuild;

			return false;
		}
	}
}
