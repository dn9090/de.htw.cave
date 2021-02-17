using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JoyconLib;
using Htw.Cave.Joycons;

///TODO: Implement teleporter with Kinect v2.

namespace Htw.Cave.Controls
{
	/// <summary>
	/// Teleports the rigidbody to a save near location.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Controls/Teleporter Controls")]
	[RequireComponent(typeof(Rigidbody))]
	public sealed class TeleporterControls : MonoBehaviour
	{
		[SerializeField]
		private Teleporter teleporter;
		public Teleporter Teleporter
		{
			get => this.teleporter;
			set => this.teleporter = value;
		}

		private Rigidbody rigid;
		private Vector3 target;

		public void Awake()
		{
			this.rigid = base.GetComponent<Rigidbody>();
		}

		public void Start()
		{
			if(this.teleporter == null)
				base.enabled = false;
		}

		public void Update()
		{
			if(JoyconInput.GetButtonDown("Bumper L"))
			{
				this.rigid.isKinematic = true;
				this.teleporter.enabled = true;
			}

			if(JoyconInput.GetButtonUp("Bumper L"))
			{
				Vector3 position;

				if(this.teleporter.ValidTeleportPosition(out position))
					TeleportTo(position);

				this.rigid.isKinematic = false;
				this.teleporter.enabled = false;
			}
		}

		public void Reset()
		{
			this.teleporter = base.GetComponentInChildren<Teleporter>();
		}

		public void TeleportTo(Vector3 position)
		{
			Vector3 point = transform.position;
			RaycastHit hit;

			if(Physics.SphereCast(transform.position + transform.up * 0.01f, 0.01f, -transform.up, out hit))
				point = hit.point;

			Vector3 offset = transform.position - point;

			transform.position = position + offset;
		}
	}
}
