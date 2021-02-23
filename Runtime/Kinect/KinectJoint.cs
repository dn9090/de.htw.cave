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
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Joint")]
	public class KinectJoint : KinectTrackable
	{
		public JointType jointType;

		protected override TrackingState UpdateTrackingData(in KinectTrackingData trackingData)
		{
			var joint = trackingData.body.Joints[jointType];

			if(joint.TrackingState != TrackingState.NotTracked)
			{
				transform.localPosition = joint.JointPositionRealSpace(trackingData.coordinateOrigin, trackingData.floor);
				transform.localRotation = trackingData.body.JointOrientations[jointType].JointRotation();
			}
			
			return joint.TrackingState;
		}
	}
}
