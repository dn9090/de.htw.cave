using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Htw.Cave.Utilites;

namespace Htw.Cave.Joycons
{
	public static class JoyconEditorInternals
	{
		public class InputManagerEntry
		{
			public enum Kind { KeyOrButton, Mouse, Axis }
			public enum Axis { X, Y, Third, Fourth, Fifth, Sixth, Seventh, Eigth }
			public enum Joy { All, First, Second }

			public static InputManagerEntry Empty(string name)
			{
				return new InputManagerEntry {
					name = name
				};
			}

			public static InputManagerEntry Button(string name, string positive)
			{
				return new InputManagerEntry {
					name = name,
					btnPositive = positive,
					gravity = 1000f,
					deadZone = 0.001f,
					sensitivity = 1000f,
					kind = Kind.KeyOrButton
				};
			}

			public static InputManagerEntry Button(string name, string positive, string negative)
			{
				return new InputManagerEntry {
					name = name,
					btnNegative = negative,
					btnPositive = positive,
					gravity = 3,
					deadZone = 0.001f,
					sensitivity = 3,
					snap = true,
					kind = Kind.KeyOrButton
				};
			}

			public static InputManagerEntry Mouse(string name, Axis axis)
			{
				return new InputManagerEntry {
					name = name,
					gravity = 0f,
					deadZone = 0f,
					sensitivity = 0.1f,
					kind = Kind.Mouse,
					axis = axis
				};
			}

			public string name = "";
			public string desc = "";
			public string btnNegative = "";
			public string btnPositive = "";
			public string altBtnNegative = "";
			public string altBtnPositive = "";
			public float gravity = 0.0f;
			public float deadZone = 0.0f;
			public float sensitivity = 0.0f;
			public bool snap = false;
			public bool invert = false;
			public Kind kind = Kind.Axis;
			public Axis axis = Axis.X;
			public Joy joystick = Joy.All;
		}

		public static bool ExistsAxis(string name, InputManagerEntry.Kind kind, SerializedProperty axes)
		{
			for (var i = 0; i < axes.arraySize; ++i)
			{
				SerializedProperty spAxis = axes.GetArrayElementAtIndex(i);
				string axisName = spAxis.FindPropertyRelative("m_Name").stringValue;
				int kindValue = spAxis.FindPropertyRelative("type").intValue;
			if (axisName == name && (int)kind == kindValue)
				return true;
			}

			return false;
		}

		public static void CreateAxis(InputManagerEntry entry)
		{
			var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");

			if (assets.Length == 0)
				return;

			SerializedObject settings = new SerializedObject(assets[0]);

			// WARNING: This is a hack to create input axis. This is necessary
			//          because Unity protects the InputManager from changes.
			SerializedProperty axes = settings.FindProperty("m_Axes");

			if(ExistsAxis(entry.name, entry.kind, axes))
				return;

			int axesCount = axes.arraySize;

			axes.InsertArrayElementAtIndex(axesCount);
			SerializedProperty spAxis = axes.GetArrayElementAtIndex(axesCount);

			spAxis.FindPropertyRelative("m_Name").stringValue = entry.name;
			spAxis.FindPropertyRelative("descriptiveName").stringValue = entry.desc;
			spAxis.FindPropertyRelative("negativeButton").stringValue = entry.btnNegative;
			spAxis.FindPropertyRelative("altNegativeButton").stringValue = entry.altBtnNegative;
			spAxis.FindPropertyRelative("positiveButton").stringValue = entry.btnPositive;
			spAxis.FindPropertyRelative("altPositiveButton").stringValue = entry.altBtnPositive;
			spAxis.FindPropertyRelative("gravity").floatValue = entry.gravity;
			spAxis.FindPropertyRelative("dead").floatValue = entry.deadZone;
			spAxis.FindPropertyRelative("sensitivity").floatValue = entry.sensitivity;
			spAxis.FindPropertyRelative("snap").boolValue = entry.snap;
			spAxis.FindPropertyRelative("invert").boolValue = entry.invert;
			spAxis.FindPropertyRelative("type").intValue = (int)entry.kind;
			spAxis.FindPropertyRelative("axis").intValue = (int)entry.axis;
			spAxis.FindPropertyRelative("joyNum").intValue = (int)entry.joystick;

			settings.ApplyModifiedProperties();
		}

		public static void CreateAxis(params InputManagerEntry[] entries)
		{
			foreach(InputManagerEntry entry in entries)
				CreateAxis(entry);
		}
	}
}
