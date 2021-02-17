using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JoyconLib;

namespace Htw.Cave.Joycons
{

	/// <summary>
	/// Look alike implementation of Unity's legacy input system with
	/// Joy-Con support based on the Joy-Con bindings.
	/// </summary>
	public static class JoyconInput
	{
		public static float GetAxis(string axis)
		{
			float input = 0f;

			Joycon joycon = GetDevice(axis);

			if(joycon != null)
				input += JoyconBinding.ResolveAxis(axis).GetAxis(joycon);

			input += Input.GetAxis(JoyconBinding.ResolveName(axis, joycon));

			return Mathf.Clamp(input, -1f, 1f);
		}

		public static bool GetButton(string axis)
		{
			bool input = false;

			Joycon joycon = GetDevice(axis);

			if(joycon != null)
				input = JoyconBinding.ResolveAxis(axis).GetButton(joycon);

			return input ? true : Input.GetButton(JoyconBinding.ResolveName(axis, joycon));
		}

		public static bool GetButtonDown(string axis)
		{
			bool input = false;

			Joycon joycon = GetDevice(axis);

			if(joycon != null)
				input = JoyconBinding.ResolveAxis(axis).GetButtonDown(joycon);

			return input ? true : Input.GetButtonDown(JoyconBinding.ResolveName(axis, joycon));
		}

		public static bool GetButtonUp(string axis)
		{
			bool input = false;

			Joycon joycon = GetDevice(axis);

			if(joycon != null)
				input = JoyconBinding.ResolveAxis(axis).GetButtonUp(joycon);

			return input ? true : Input.GetButtonUp(JoyconBinding.ResolveName(axis, joycon));
		}

		public static Joycon GetDevice(string axis)
		{
			Joycon joycon = JoyconController.First;

			if(JoyconBinding.IsLeftAxis(axis))
				joycon = JoyconController.GetLeftOrFirst();
			else if(JoyconBinding.IsRightAxis(axis))
				joycon = JoyconController.GetRightOrFirst();

			return joycon;
		}
	}
}
