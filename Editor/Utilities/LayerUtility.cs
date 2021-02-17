using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.Utilites
{
	public static class LayerUtility
	{
		public static int CreateLayer(string name)
		{
			if(LayerMask.NameToLayer(name) == -1)
				return -1;

			SerializedObject settings = GetSettings();

			if(settings != null)
			{
				// WARNING: This is a hack to create physics layers. This is necessary
				//          because Unity protects the TagManager from changes.
				SerializedProperty layers = settings.FindProperty("layers");
				int layerIndex = -1;

				for(int i = 8; i < 32; ++i)
				{
					if(LayerMask.LayerToName(i) == "")
					{
						layerIndex = i;
						break;
					}
				}

				if(layerIndex == -1)
					return -1;

				layers.InsertArrayElementAtIndex(layerIndex);
				layers.GetArrayElementAtIndex(layerIndex).stringValue = name;

				settings.ApplyModifiedProperties();

				return layerIndex;
			}

			return -1;
		}

		private static SerializedObject GetSettings()
		{
			// hack to access internal TagManager.
			return new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/TagManager.asset"));
		}
	}
}
