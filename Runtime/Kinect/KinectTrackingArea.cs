using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	public sealed class KinectTrackingArea
	{
		public event Action<KinectActor> onActorEntered;

		public event Action<KinectActor> onActorLeft;

		public void FixedUpdate()
		{
			// Problem: We need the tracking data for physics
			// but also we want to reduce the delay between the
			// physics and render update to guarantee that
			// the tracking person has the newest camera position for the render image.
			// FixedUpdate, LateUpdate (or better OnPreCull but this is only available on the camera)
		}
	}
}