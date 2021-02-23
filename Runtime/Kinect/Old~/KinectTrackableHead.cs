using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Specialized to updating the transform based on a tracked head or shoulder.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Trackable Head")]
	public class KinectTrackableHead : KinectTrackable
	{
		public void Update()
		{
			if(base.actor.IsTracked)
			{
				if(base.actor.IsHeadTracked())
				{
					transform.localPosition = base.actor.GetHeadPosition();
					transform.localRotation = base.actor.GetFaceRotation();
				} else if(base.actor.IsShoulderTracked()) {
					transform.localPosition = base.actor.GetShoulderPosition() + Vector3.up * 0.3f;
					transform.localRotation = base.actor.GetShoulderRotation();
				}
			}
		}
	}
}
