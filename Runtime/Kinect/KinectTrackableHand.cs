using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;

namespace Htw.Cave.Kinect
{
	[SerializeField]
	public enum HandType
	{
		Left,
		Right
	}

	/// <summary>
	/// Specialized to updating the transform and state based on a tracked hand.
	/// Notice that the forward axis is based on the wrist direction due to tracking
	/// issues with the hand direction while holding a controller.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Trackable Hand")]
	[RequireComponent(typeof(Rigidbody))]
	public class KinectTrackableHand : KinectTrackable
	{
		[SerializeField]
		private HandType handType;
		public HandType HandType
		{
			get => this.handType;
			set => this.handType = value;
		}

		public HandState HandState { get; private set; }

		public void Awake()
		{
			this.HandState = HandState.NotTracked;
		}

		public void Update()
		{
			if(base.actor.IsTracked)
			{
				if(this.handType == HandType.Left && base.actor.IsLeftHandTracked())
				{
					transform.localPosition = base.actor.GetLeftHandPosition();
					transform.forward = transform.TransformDirection(base.actor.GetLeftWristDirection());
					this.HandState = base.actor.GetLeftHandState();
				} else if(this.handType == HandType.Right && base.actor.IsRightHandTracked()) {
					transform.localPosition = base.actor.GetRightHandPosition();
					transform.forward = transform.TransformDirection(base.actor.GetRightWristDirection());
					this.HandState = base.actor.GetRightHandState();
				}
			}
		}

		public void Reset()
		{
			base.GetComponent<Rigidbody>().isKinematic = true;

			if(base.GetComponent<Collider>() != null)
				base.GetComponent<Collider>().isTrigger = true;
		}
	}
}
