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
	/// Represents the head joint of a tracked <see cref="Body"/>.
	/// If the head joint is not tracked the position and rotation will
	/// be inferred from other joints.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Head")]
	public class KinectHead : KinectTrackable
	{
		/// <summary>
		/// Returns <c>true</c> if the actual face is tracked otherwise <c>false</c>.
		/// It is possible that the <see cref="Windows.Kinect.JointType.Head"/> is tracked but the face is not.
		/// </summary>
		public bool isFaceTracked => this.m_IsFaceTracked;

		private bool m_IsFaceTracked;

		protected override TrackingState UpdateTrackingData(ref KinectBodyFrame bodyFrame)
		{
			var joint = bodyFrame[JointType.Head];

			transform.localPosition = joint.position;
			transform.localRotation = bodyFrame[JointType.Neck].rotation;

			// Use the shoulder positions and rotation as fallback if
			// the head is not tracked.
			if(joint.trackingState != TrackingState.Tracked)
			{
				joint = bodyFrame[JointType.SpineShoulder];
				
				if(joint.trackingState == TrackingState.NotTracked)
					return TrackingState.NotTracked;
					
				transform.localPosition = joint.position + Vector3.up * 0.25f;
				transform.localRotation = joint.rotation;

				return TrackingState.Inferred;
			}

			if(bodyFrame.face != null)
				transform.localRotation = KinectHelper.FaceRotationToRealSpace(bodyFrame.face.FaceRotationQuaternion);
			
			return joint.trackingState;
		}
	}
}
