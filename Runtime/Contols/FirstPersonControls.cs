using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Htw.Cave.Joycons;

namespace Htw.Cave.Controls
{
	/// <summary>
	/// Moves and rotates the rigidbody.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Controls/First Person Controls")]
	[RequireComponent(typeof(Rigidbody))]
	public sealed class FirstPersonControls : MonoBehaviour
	{
		[SerializeField]
		private float speed;
		public float Speed
		{
			get => this.speed;
			set => this.speed = value;
		}

		[SerializeField]
		private float snappyness;
		public float Snappyness
		{
			get => this.snappyness;
			set => this.snappyness = value;
		}

		[SerializeField]
		private float sensitivity;
		public float Sensitivity
		{
			get => this.sensitivity;
			set => this.sensitivity = value;
		}

		private Rigidbody rigid;

		public void Awake()
		{
			this.rigid = base.GetComponent<Rigidbody>();
			this.rigid.freezeRotation = true;
		}

		public void FixedUpdate()
		{
			Move(JoyconInput.GetAxis("Horizontal L"), JoyconInput.GetAxis("Vertical L"));
			Rotate(JoyconInput.GetAxis("Horizontal R"));
		}

		public void Reset()
		{
			this.speed = 5f;
			this.snappyness = 7f;
			this.sensitivity = 3f;
		}

		public void Move(float h, float v)
		{
			Vector3 direction = (transform.forward * v + transform.right * h).normalized;

			Vector3 targetVelocity = direction * this.speed;
			Vector3 deltaVelocity = targetVelocity - this.rigid.velocity;

			if(this.rigid.useGravity)
				deltaVelocity.y = 0;

			this.rigid.AddForce(deltaVelocity * this.snappyness, ForceMode.Acceleration);
		}

		public void Rotate(float y)
		{
			Vector3 rotation = new Vector3(0f, y, 0f);
			rotation = rotation * sensitivity;

			transform.Rotate(rotation);
		}
	}
}
