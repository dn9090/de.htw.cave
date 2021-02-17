using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Htw.Cave.Joycons;

namespace Htw.Cave.Controls
{
	/// <summary>
	/// Updates the collider height in respect to the local height.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Controls/Height Controlled Collider")]
	[RequireComponent(typeof(CapsuleCollider))]
	public sealed class HeightControlledCollider : MonoBehaviour
	{
		private const float Spacing = 0.1f;

		private CapsuleCollider capsule;

		public void Awake()
		{
			this.capsule = base.GetComponent<CapsuleCollider>();
		}

		public void FixedUpdate()
		{
			CenterCollider();
		}

		public void Reset()
		{
			this.capsule = base.GetComponent<CapsuleCollider>();
			this.capsule.radius = 0.3f;
			CenterCollider();
		}

		private void CenterCollider()
		{
			Vector3 position = transform.localPosition;
			this.capsule.center = new Vector3(0f, position.y * -0.5f + (0.5f * Spacing), 0f);
			this.capsule.height = position.y + Spacing;
		}
	}
}
