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
	/// be updated according to the tracked position and rotation of the <see cref="jointType"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Joint")]
	public class KinectJoint : KinectTrackable
	{
		/// <summary>
		/// Defines the tracked joint.
		/// </summary>
		public JointType jointType;

		protected override TrackingState UpdateTrackingData(in KinectFrameBuffer frameBuffer)
		{
			var joint = frameBuffer.joints[jointType];

			if(joint.TrackingState != TrackingState.NotTracked)
			{
				transform.localPosition = joint.JointPositionRealSpace(frameBuffer.floor);
				transform.localRotation = frameBuffer.jointOrientations[jointType].JointRotation();
			}

			return joint.TrackingState;
		}
	}
}
