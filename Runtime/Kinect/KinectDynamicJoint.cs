using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Represents a generic <see cref="Windows.Kinect.Joint"/> of a <see cref="Body"/>.
	/// The <see cref="transform"/> position and rotation will
	/// be updated according to the tracked position and rotation of the <see cref="jointType"/>
	/// which can be changed dynamically at runtime.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Dynamic Joint")]
	public class KinectDynamicJoint : KinectTrackable
	{
		/// <summary>
		/// Defines the tracked joint.
		/// </summary>
		public JointType jointType;

		protected override TrackingState UpdateTrackingData(ref KinectBodyFrame bodyFrame)
		{
			var joint = bodyFrame[jointType];

			if(joint.trackingState != TrackingState.NotTracked)
			{
				transform.localPosition = joint.position;
				transform.localRotation = joint.rotation;
			}

			return joint.trackingState;
		}
	}
}
