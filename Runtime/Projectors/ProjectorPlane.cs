using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Htw.Cave.Projectors
{
	/// <summary>
	/// Respresents a camera projection plane.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Projectors/Projector Plane")]
	public sealed class ProjectorPlane : MonoBehaviour
	{
		public bool HasChanged => transform.hasChanged;

		public Vector3[] TransformPlane(float width, float height) => new Vector3[]{
				transform.TransformPoint(new Vector3(-width * 0.5f, height * 0.5f, 0f)),
				transform.TransformPoint(new Vector3(width * 0.5f, height * 0.5f, 0f)),
				transform.TransformPoint(new Vector3(width * 0.5f, -height * 0.5f, 0f)),
				transform.TransformPoint(new Vector3(-width * 0.5f, -height * 0.5f, 0f))
			};
	}
}
