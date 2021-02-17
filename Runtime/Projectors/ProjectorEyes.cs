using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Htw.Cave.Projectors
{
	/// <summary>
	/// Responsible for transforming stereoscopic eyes from local to world space.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Projectors/Projector Eyes")]
	public sealed class ProjectorEyes : MonoBehaviour
	{
		public bool Invert { get; set; }
		public Vector3 Anchor => transform.position;
		public Vector3 Left => this.Invert ? right.position : left.position;
		public Vector3 Right => this.Invert ? left.position : right.position;

		private Transform left;
		private Transform right;
		private float separation;

		public void Awake()
		{
			this.Invert = false;

			this.left = (new GameObject("Left Eye")).transform;
			this.left.parent = transform;

			this.right = (new GameObject("Right Eye")).transform;
			this.right.parent = transform;

			SetSeperation(this.separation);
		}

		public void SetSeperation(float separation)
		{
			this.separation = separation;

			if(this.left != null && this.right != null)
			{
				Vector3 s = new Vector3(this.separation * 0.5f, 0f, 0f);
				this.left.localPosition = -s;
				this.right.localPosition = s;
			}
		}
	}
}
