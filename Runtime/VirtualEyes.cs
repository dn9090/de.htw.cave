using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Htw.Cave.Kinect;

namespace Htw.Cave
{
	/// <summary>
	/// Responsible for transforming stereoscopic eyes from local to world space.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Virtual Eyes")]
	public sealed class VirtualEyes : MonoBehaviour
	{
		/// <summary>
		/// Defines the eye of a stereo or mono output target.
		/// </summary>
		public enum StereoTarget
		{
			Mono,
			Left,
			Right
		}
	
		/// <summary>
		/// Returns the eye position based on the <see cref="StereoTarget"/>.
		/// </summary>
		/// <param name="stereoTarget">The target eye (stereo) or the eye anchor (mono).</param>
		/// <param name="seperation">The distance between the eyes (ignored when using <see cref="StereoTarget.Mono"/>).</param>
		/// <returns>The eye position in world space.</returns>
		public Vector3 GetPosition(StereoTarget stereoTarget, float seperation = 0f)
		{
			switch(stereoTarget)
			{
				case StereoTarget.Mono:
					return transform.position;
				case StereoTarget.Left:
					return transform.TransformPoint(Vector3.right * -seperation * 0.5f);
				case StereoTarget.Right:
					return transform.TransformPoint(Vector3.right * seperation * 0.5f);
			}
			
			return Vector3.zero;
		}	
	}
}
