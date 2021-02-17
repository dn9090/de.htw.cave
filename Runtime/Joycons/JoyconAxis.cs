using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JoyconLib;

namespace Htw.Cave.Joycons
{
	/// <summary>
	/// Represents a generic gamepad axis.
	/// </summary>
	public abstract class JoyconAxis
	{
		public abstract float GetAxis(Joycon joycon);
		public abstract bool GetButton(Joycon joycon);
		public abstract bool GetButtonDown(Joycon joycon);
		public abstract bool GetButtonUp(Joycon joycon);
	}

	/// <summary>
	/// Implements a single axis Joy-Con thumbstick.
	/// </summary>
	public class JoyconStick : JoyconAxis
	{
		private int i;

		public JoyconStick(bool horizontal)
		{
			this.i = horizontal ? 1 : 0;
		}

		public override float GetAxis(Joycon joycon)
		{
			return joycon.GetStick()[this.i];
		}

		public override bool GetButton(Joycon joycon)
		{
			return !Mathf.Approximately(joycon.GetStick()[this.i], 0f);
		}

		public override bool GetButtonDown(Joycon joycon)
		{
			bool last = GetButton(joycon);
			joycon.Update();

			return !last && GetButton(joycon);
		}

		public override bool GetButtonUp(Joycon joycon)
		{
			bool last = GetButton(joycon);
			joycon.Update();

			return last && !GetButton(joycon);
		}
	}

	/// <summary>
	/// Implements a generic Joy-Con button as axis.
	/// </summary>
	public class JoyconButton : JoyconAxis
	{
		private Joycon.Button button;

		public JoyconButton(Joycon.Button button)
		{
			this.button = button;
		}

		public override float GetAxis(Joycon joycon)
		{
			return joycon.GetButton(button) ? 1f : 0f;
		}

		public override bool GetButton(Joycon joycon)
		{
			return joycon.GetButton(button);
		}

		public override bool GetButtonDown(Joycon joycon)
		{
			return joycon.GetButtonDown(button);
		}

		public override bool GetButtonUp(Joycon joycon)
		{
			return joycon.GetButtonUp(button);
		}
	}
}
