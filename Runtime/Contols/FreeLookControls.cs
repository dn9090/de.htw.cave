using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Htw.Cave.Joycons;

namespace Htw.Cave.Controls
{
	/// <summary>
	/// Moves a transform in space freely.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Controls/Free Look Controls")]
	public sealed class FreeLookControls : MonoBehaviour
	{
		[SerializeField]
		private float speed;
		public float Speed
		{
			get => this.speed;
			set => this.speed = value;
		}

		[SerializeField]
		private float sensitivity;
		public float Sensitivity
		{
			get => this.sensitivity;
			set => this.sensitivity = value;
		}

		public void Update()
		{
			Move(JoyconInput.GetAxis("Horizontal L"), JoyconInput.GetAxis("Vertical L"));
			Rotate(JoyconInput.GetAxis("Horizontal R"));
			Lift(JoyconInput.GetAxis("Vertical R"));
		}

		public void Move(float h, float v)
		{
			Vector3 direction = (transform.forward * v + transform.right * h).normalized;

			transform.position += direction * Time.deltaTime * this.speed;
		}

		public void Rotate(float y)
		{
			Vector3 rotation = new Vector3(0f, y, 0f);
			rotation = rotation * sensitivity;

			transform.Rotate(rotation);
		}

		public void Lift(float v)
		{
			transform.position += transform.up * v * Time.deltaTime * (this.speed * 0.5f);
		}

		public void Reset()
		{
			this.speed = 5f;
			this.sensitivity = 3f;
		}
	}
}
