using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	[SerializeField]
	public enum HandType
	{
		Left,
		Right
	}

	[AddComponentMenu("Htw.Cave/Kinect/Kinect Hand")]
	public class KinectHand : KinectTrackable
	{
		public HandState handState => this.m_HandState;

		public TrackingConfidence handConfidence => this.m_HandConfidence;

		public HandType handType;

		private HandState m_HandState;

		private TrackingConfidence m_HandConfidence;

		private Vector3 m_HandDirection;

		public void Awake()
		{
			this.m_HandState = HandState.NotTracked;
		}

		protected override TrackingState UpdateTrackingData(in KinectTrackingData trackingData)
		{
			var jointType = JointType.HandLeft;
			var forwardJointType = JointType.HandTipLeft;

			this.m_HandState = trackingData.body.HandLeftState;
			this.m_HandConfidence = trackingData.body.HandLeftConfidence;

			if(this.handType == HandType.Right)
			{
				jointType = JointType.HandRight;
				forwardJointType = JointType.HandTipRight;

				this.m_HandState = trackingData.body.HandRightState;
				this.m_HandConfidence = trackingData.body.HandRightConfidence;
			}

			var joint = trackingData.body.Joints[jointType];

			if(joint.TrackingState != TrackingState.NotTracked)
			{
				transform.localPosition = joint.JointPositionRealSpace(trackingData.coordinateOrigin, trackingData.floor);
				transform.localRotation = trackingData.body.JointOrientations[jointType].JointRotation();

				joint = trackingData.body.Joints[forwardJointType];

				// @Todo: Map to local space.
				transform.forward = (joint.JointPositionRealSpace(trackingData.coordinateOrigin, trackingData.floor) - transform.position).normalized;
			}

			return joint.TrackingState;
		}
	}
}
