using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Contains the state and data for a tracked person.
	/// Also provides common methods and calculations.
	/// </summary>
	public sealed class KinectActor
	{
		public UnityEngine.Vector3 CoordinateOrigin { get; private set; }
		public UnityEngine.Vector4 Floor { get; private set; }
		public bool IsTracked { get; set; }
		public ulong TrackingId { get; private set; }
		public Body Body { get; private set; }
		public FaceFrameResult Face { get; private set; }

		public KinectActor(UnityEngine.Vector3 origin)
		{
			this.CoordinateOrigin = origin;
			this.Floor = UnityEngine.Vector4.zero;
			this.IsTracked = false;
			this.TrackingId = 0;
			this.Body = null;
			this.Face = null;
		}

		public void Update(Windows.Kinect.Vector4 floor, Body body, FaceFrameResult face)
		{
			this.Floor = floor.ToUnityVector4();
			this.Body = body;
			this.Face = face;
			this.IsTracked = body.IsTracked;
			this.TrackingId = body.TrackingId;
		}

		public UnityEngine.Vector3 GetJointPosition(JointType jointType) => this.Body.JointPositionRealSpace(jointType, CoordinateOrigin, this.Floor);

		public UnityEngine.Quaternion GetJointRotation(JointType jointType) => this.Body.JointRotation(jointType);

		public bool IsJointTracked(JointType jointType, bool inferred = true)
		{
			TrackingState state = this.Body.JointTrackingState(jointType);

			if(state == TrackingState.Tracked)
				return true;

			return inferred && state == TrackingState.Inferred;
		}

		public UnityEngine.Vector3 GetHeadPosition() => GetJointPosition(JointType.Head);

		public UnityEngine.Vector3 GetShoulderPosition() => GetJointPosition(JointType.SpineShoulder);

		public UnityEngine.Vector3 GetSpinePosition() => GetJointPosition(JointType.SpineMid);

		public UnityEngine.Vector3 GetLeftHandPosition() => GetJointPosition(JointType.HandLeft);

		public UnityEngine.Vector3 GetRightHandPosition() => GetJointPosition(JointType.HandRight);

		public UnityEngine.Vector3 GetEstimatedBodyPosition() => Vector3.Lerp(GetJointPosition(JointType.SpineBase), GetJointPosition(JointType.SpineShoulder), 0.5f);

		public UnityEngine.Quaternion GetFaceRotation() => this.Face.FaceRotation();

		public UnityEngine.Quaternion GetShoulderRotation() => Quaternion.Lerp(GetJointRotation(JointType.ShoulderLeft), GetJointRotation(JointType.ShoulderRight), 0.5f);

		public UnityEngine.Vector3 GetLeftWristDirection() => (GetJointPosition(JointType.WristLeft) - GetJointPosition(JointType.ElbowLeft)).normalized;

		public UnityEngine.Vector3 GetRightWristDirection() => (GetJointPosition(JointType.WristRight) - GetJointPosition(JointType.ElbowRight)).normalized;

		public UnityEngine.Vector3 GetLeftHandDirection() => (GetJointPosition(JointType.HandTipLeft) - GetJointPosition(JointType.HandLeft)).normalized;

		public UnityEngine.Vector3 GetRightHandDirection() => (GetJointPosition(JointType.HandTipRight) - GetJointPosition(JointType.HandRight)).normalized;

		public HandState GetLeftHandState() => this.Body.HandLeftState;

		public HandState GetRightHandState() => this.Body.HandRightState;

		public TrackingConfidence GetLeftHandConfidence() => this.Body.HandLeftConfidence;

		public TrackingConfidence GetRightHandConfidence() => this.Body.HandRightConfidence;

		public bool IsHeadTracked(bool inferred = true) => IsJointTracked(JointType.Head, inferred);

		public bool IsShoulderTracked(bool inferred = true) => IsJointTracked(JointType.SpineShoulder, inferred);

		public bool IsLeftHandTracked(bool inferred = true) => IsJointTracked(JointType.HandLeft, inferred);

		public bool IsRightHandTracked(bool inferred = true) => IsJointTracked(JointType.HandRight, inferred);

		public bool IsLeftHandClosed(bool lowConfidence = true) => GetLeftHandState() == HandState.Closed && (GetLeftHandConfidence() == TrackingConfidence.High || lowConfidence);

		public bool IsRightHandClosed(bool lowConfidence = true) => GetRightHandState() == HandState.Closed && (GetRightHandConfidence() == TrackingConfidence.High || lowConfidence);

		public bool IsLeftHandOpen(bool lowConfidence = true) => GetLeftHandState() == HandState.Open && (GetLeftHandConfidence() == TrackingConfidence.High || lowConfidence);

		public bool IsRightHandOpen(bool lowConfidence = true) => GetRightHandState() == HandState.Open && (GetRightHandConfidence() == TrackingConfidence.High || lowConfidence);
	}
}
