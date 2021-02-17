using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Converts Kinect 2.0 data to Unity.
	/// </summary>
	public static class KinectExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Vector4 ToUnityVector4(this Windows.Kinect.Vector4 vector)
		{
			return new UnityEngine.Vector4(vector.X, vector.Y, vector.Z, vector.W);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Quaternion ToUnityQuaternion(this Windows.Kinect.Vector4 vector)
		{
			return new UnityEngine.Quaternion(vector.X, vector.Y, vector.Z, vector.W);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Quaternion LeftHanded(this UnityEngine.Quaternion quaternion)
		{
			return new Quaternion(-quaternion.x, -quaternion.y, quaternion.z, quaternion.w);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DistanceFromCameraSpacePoint(this UnityEngine.Vector4 vector, CameraSpacePoint point)
		{
			return (vector.x * point.X + vector.y * point.Y + vector.z * point.Z + vector.w) / (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Vector3 CameraSpacePointToRealSpace(this CameraSpacePoint point, UnityEngine.Vector3 origin, UnityEngine.Vector4 floor)
		{
			return new Vector3(origin.x + point.X, floor.DistanceFromCameraSpacePoint(point), origin.z - point.Z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Vector3 JointPositionRealSpace(this Body body, JointType jointType, UnityEngine.Vector3 origin, UnityEngine.Vector4 floor)
		{
			return body.Joints[jointType].Position.CameraSpacePointToRealSpace(origin, floor);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Quaternion JointRotation(this Body body, JointType jointType)
		{
			return body.JointOrientations[jointType].Orientation.ToUnityQuaternion().LeftHanded();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TrackingState JointTrackingState(this Body body, JointType jointType)
		{
			return body.Joints[jointType].TrackingState;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Quaternion FaceRotation(this FaceFrameResult face)
		{
			return face.FaceRotationQuaternion.ToUnityQuaternion().LeftHanded();
		}
	}
}
