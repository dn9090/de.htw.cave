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

	/// <summary>
	/// Represents a hand joint of a tracked <see cref="Body"/>
	/// and provides access to the <see cref="HandState"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Hand")]
	public class KinectHand : KinectTrackable
	{
		/// <summary>
		/// Detection result of simple hand gestures.
		/// </summary>
		public HandState handState => this.m_HandState;

		/// <summary>
		/// The confidence of the <see cref="handState"/> detection.
		/// </summary>
		public TrackingConfidence handConfidence => this.m_HandConfidence;

		/// <summary>
		/// Defines the tracked hand.
		/// </summary>
		public HandType handType;

		private HandState m_HandState;

		private TrackingConfidence m_HandConfidence;

		private Vector3 m_HandDirection;

		public void Awake()
		{
			this.m_HandState = HandState.NotTracked;
		}

		protected override TrackingState UpdateTrackingData(ref KinectBodyFrame bodyFrame)
		{
			var jointType = JointType.HandLeft;
			//var forwardJointType = JointType.HandTipLeft;

			this.m_HandState = bodyFrame.body.HandLeftState;
			this.m_HandConfidence = bodyFrame.body.HandLeftConfidence;

			if(this.handType == HandType.Right)
			{
				jointType = JointType.HandRight;
				//forwardJointType = JointType.HandTipRight;

				this.m_HandState = bodyFrame.body.HandRightState;
				this.m_HandConfidence = bodyFrame.body.HandRightConfidence;
			}

			var joint = bodyFrame[jointType];
			
			if(joint.trackingState != TrackingState.NotTracked)
			{
				transform.localPosition = joint.position;
				transform.localRotation = joint.rotation;

				//joint = bodyFrame[forwardJointType];

				// @Todo: Map to local space.
				//transform.forward = transform.TransformDirection((joint.position - transform.localPosition).normalized);
			}

			return joint.trackingState;
		}
	}
}
