using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JoyconLib;

namespace Htw.Cave.Joycons
{
	[Serializable]
	public enum JoyconAxisType
	{
		Button,
		Stick
	}

	/// <summary>
	/// Matches a Joy-Con axis with an Input axis.
	/// </summary>
	[Serializable]
	public class JoyconScheme
	{
		public string name = "";
		public JoyconAxisType axis;
		public Joycon.Button button;

		public JoyconScheme(string name, JoyconAxisType axis, Joycon.Button button = Joycon.Button.STICK)
		{
			this.name = name;
			this.axis = axis;
			this.button = button;
		}
	}

	/// <summary>
	/// Binds the provided Joy-Con axes to the Input axes.
	/// </summary>
	[CreateAssetMenu(fileName = "New Joycon Binding", menuName = "Htw.Cave/Joycon Binding", order = 40)]
	public class JoyconBinding : ScriptableObject
	{
		public const string Identifier = "Joycon ";

		private static Dictionary<string, JoyconAxis> bindings;

		public static void Bind(string name, JoyconAxis axis)
		{
			if(!string.IsNullOrEmpty(name) && axis != null)
				bindings.Add(name, axis);
		}

		public static bool IsLeftAxis(string axis)
		{
			return axis[axis.Length - 1] == 'L';
		}

		public static bool IsRightAxis(string axis)
		{
			return axis[axis.Length - 1] == 'R';
		}

		public static JoyconAxis ResolveAxis(string axis)
		{
			return bindings[axis];
		}

		public static string ResolveName(string axis, bool isLeft = true)
		{
			if(!axis.StartsWith(Identifier))
				axis = Identifier + axis;

			char t = axis[axis.Length - 1];

			if(t != 'L' && t != 'R')
				axis = isLeft ? axis + " L" : axis + " R";

			return axis;
		}

		public static string ResolveName(string axis, Joycon joycon)
		{
			return ResolveName(axis, joycon == null ? true : joycon.isLeft);
		}

		[SerializeField]
		private List<JoyconScheme> schemes;
		public List<JoyconScheme> Schemes
		{
			get => this.schemes;
			set => this.schemes = value;
		}

		public void Awake()
		{
			this.schemes = new  List<JoyconScheme>();

			AddDefaultSchemes();
		}

		public void Activate()
		{
			bindings = new Dictionary<string, JoyconAxis>();

			foreach(JoyconScheme scheme in this.schemes)
			{
				JoyconAxis axis = null;

				if(scheme.axis == JoyconAxisType.Stick)
					axis = new JoyconStick(IsLeftAxis(scheme.name));
				else if (scheme.axis == JoyconAxisType.Button)
					axis = new JoyconButton(scheme.button);

				Bind(scheme.name, axis);
			}
		}

		public void Add(JoyconScheme scheme)
		{
			if(scheme != null && !Exists(scheme))
				this.schemes.Add(scheme);
		}

		public bool Exists(string name)
		{
			return this.schemes.Count(s => s.name == name) > 0;
		}

		public bool Exists(JoyconScheme scheme)
		{
			return Exists(scheme.name);
		}

		private void AddDefaultSchemes()
		{
			Add(new JoyconScheme("Trigger L", JoyconAxisType.Button, Joycon.Button.SHOULDER_2));
			Add(new JoyconScheme("Trigger R", JoyconAxisType.Button, Joycon.Button.SHOULDER_2));
			Add(new JoyconScheme("Bumper L", JoyconAxisType.Button, Joycon.Button.SHOULDER_1));
			Add(new JoyconScheme("Bumper R", JoyconAxisType.Button, Joycon.Button.SHOULDER_1));
			Add(new JoyconScheme("Stick L", JoyconAxisType.Button, Joycon.Button.STICK));
			Add(new JoyconScheme("Stick R", JoyconAxisType.Button, Joycon.Button.STICK));
			Add(new JoyconScheme("Up L", JoyconAxisType.Button, Joycon.Button.DPAD_UP));
			Add(new JoyconScheme("Up R", JoyconAxisType.Button, Joycon.Button.DPAD_UP));
			Add(new JoyconScheme("Right L", JoyconAxisType.Button, Joycon.Button.DPAD_RIGHT));
			Add(new JoyconScheme("Right R", JoyconAxisType.Button, Joycon.Button.DPAD_RIGHT));
			Add(new JoyconScheme("Down L", JoyconAxisType.Button, Joycon.Button.DPAD_DOWN));
			Add(new JoyconScheme("Down R", JoyconAxisType.Button, Joycon.Button.DPAD_DOWN));
			Add(new JoyconScheme("Left L", JoyconAxisType.Button, Joycon.Button.DPAD_LEFT));
			Add(new JoyconScheme("Left R", JoyconAxisType.Button, Joycon.Button.DPAD_LEFT));
			Add(new JoyconScheme("SL L", JoyconAxisType.Button, Joycon.Button.SL));
			Add(new JoyconScheme("SL R", JoyconAxisType.Button, Joycon.Button.SL));
			Add(new JoyconScheme("SR L", JoyconAxisType.Button, Joycon.Button.SR));
			Add(new JoyconScheme("SR R", JoyconAxisType.Button, Joycon.Button.SR));
			Add(new JoyconScheme("Minus L", JoyconAxisType.Button, Joycon.Button.MINUS));
			Add(new JoyconScheme("Minus R", JoyconAxisType.Button, Joycon.Button.PLUS));
			Add(new JoyconScheme("Home L", JoyconAxisType.Button, Joycon.Button.CAPTURE));
			Add(new JoyconScheme("Home R", JoyconAxisType.Button, Joycon.Button.HOME));
		}
	}
}
