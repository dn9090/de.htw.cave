using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.VirtualEnvironmentSetup
{
	public abstract class Module
	{
		public bool SkipInBuild { get; set; }

		public abstract string DisplayName { get; }
		public abstract bool IsSkippable { get; }

		public virtual void OnHeaderGUI() { }
		public abstract void OnGUI(ModuleMaker maker);
		public abstract void Build(ModuleMaker maker, GameObject root, GameObject head);
	}
}
