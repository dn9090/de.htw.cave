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

		protected override TrackingState UpdateTrackingData(in KinectFrameBuffer frameBuffer)
		{
			var joint = frameBuffer.joints[JointType.Head];

			// Use the shoulder positions as fallback if
			// the head is not tracked.
			if(joint.TrackingState == TrackingState.NotTracked)
			{
				joint = frameBuffer.joints[JointType.SpineShoulder];

				if(joint.TrackingState == TrackingState.NotTracked)
					return TrackingState.NotTracked;
				
				transform.localPosition = joint.JointPositionRealSpace(frameBuffer.floor)
					+ Vector3.up * 0.3f;
				transform.localRotation = Quaternion.Lerp(
					frameBuffer.jointOrientations[JointType.ShoulderLeft].JointRotation(),
					frameBuffer.jointOrientations[JointType.ShoulderRight].JointRotation(),
					0.5f);
				
				return TrackingState.Inferred;
			}

			transform.localPosition = joint.JointPositionRealSpace(frameBuffer.floor);

			if(frameBuffer.face != null)
			{
				transform.localRotation = frameBuffer.face.FaceRotation();
				this.m_WearingGlasses = frameBuffer.face.FaceProperties[FaceProperty.WearingGlasses];
			} else {
				transform.localRotation = frameBuffer.jointOrientations[JointType.Head].JointRotation();
			}

			return joint.TrackingState;
		}
	}
}
