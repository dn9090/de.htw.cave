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
		/// Gets whether or not the tracked person is wearing glasses.
		/// </summary>
		public DetectionResult wearingGlasses => this.m_WearingGlasses;

		private DetectionResult m_WearingGlasses;

		protected override TrackingState UpdateTrackingData(ref KinectBodyFrame bodyFrame)
		{
			var joint = bodyFrame[JointType.Head];

			// Use the shoulder positions as fallback if
			// the head is not tracked.
			if(joint.trackingState == TrackingState.NotTracked)
			{
				joint = bodyFrame[JointType.SpineShoulder];

				if(joint.trackingState == TrackingState.NotTracked)
					return TrackingState.NotTracked;
				
				transform.localPosition = joint.position + Vector3.up * 0.25f;
				transform.localRotation = Quaternion.Lerp(
					bodyFrame[JointType.ShoulderLeft].rotation,
					bodyFrame[JointType.ShoulderRight].rotation,
					0.5f);

				return TrackingState.Inferred;
			}

			transform.localPosition = joint.position;
			transform.localRotation = joint.rotation;
			transform.localEulerAngles += Vector3.right * 90f;

			if(bodyFrame.face != null)
			{
				// For some reason nothing will work...
				// (github.com/igentuman, ExtractFaceRotationInDegrees)
				// transform.localRotation = bodyFrame.face.GetFaceRotation();
				this.m_WearingGlasses = bodyFrame.face.FaceProperties[FaceProperty.WearingGlasses];
			}

			return joint.trackingState;
		}
	}
}
