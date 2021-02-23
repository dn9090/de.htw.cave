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
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Head")]
	public class KinectHead : KinectTrackable
	{
		public DetectionResult wearingGlasses => this.m_WearingGlasses;

		private DetectionResult m_WearingGlasses;

		protected override TrackingState UpdateTrackingData(in KinectTrackingData trackingData)
		{
			var joint = trackingData.body.Joints[JointType.Head];

			// Use the shoulder positions as fallback if
			// the head is not tracked.
			if(joint.TrackingState == TrackingState.NotTracked)
			{
				joint = trackingData.body.Joints[JointType.SpineShoulder];

				if(joint.TrackingState == TrackingState.NotTracked)
					return TrackingState.NotTracked;
				
				transform.localPosition = joint.JointPositionRealSpace(trackingData.coordinateOrigin, trackingData.floor)
					+ Vector3.up * 0.3f;
				transform.localRotation = trackingData.body.JointOrientations[JointType.Head].JointRotation();
				
				return TrackingState.Inferred;
			}

			transform.localPosition = joint.JointPositionRealSpace(trackingData.coordinateOrigin, trackingData.floor);

			if(trackingData.face != null)
			{
				transform.localRotation = trackingData.face.FaceRotation();
				this.m_WearingGlasses = trackingData.face.FaceProperties[FaceProperty.WearingGlasses];
			} else {
				transform.localRotation = trackingData.body.JointOrientations[JointType.Head].JointRotation();
			}

			return joint.TrackingState;
		}
	}
}
