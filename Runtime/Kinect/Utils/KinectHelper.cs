using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Kinect;
using UnityEngine;

namespace Htw.Cave.Kinect.Utils
{
	public static class KinectHelper
	{
		public static readonly JointType[] parentJointTypes = {
			JointType.SpineBase, JointType.SpineBase, JointType.SpineShoulder, JointType.Neck, // Spine to head.
			JointType.SpineShoulder, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, // Shoulder to left wrist.
			JointType.SpineShoulder, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, // Shoulder to right wrist.
			JointType.SpineBase, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, // Spine to left foot.
			JointType.SpineBase, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, // Spine to right foot.
			JointType.SpineMid, // Only shoulder.
			JointType.HandLeft, JointType.HandLeft, // Left hand tip and thumb. 
			JointType.HandRight, JointType.HandRight, // Left hand tip and thumb. 
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static JointType GetParentJointType(JointType jointType) => parentJointTypes[(int)jointType];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 SensorFloorPlaneYOffset(UnityEngine.Vector4 floorClipPlane) =>
			Vector3.up * floorClipPlane.w;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion CalculateFloorRotationCorrection(UnityEngine.Vector4 floorClipPlane)
		{
			Vector3 up = floorClipPlane;
			Vector3 right = Vector3.Cross(up, Vector3.forward);
			Vector3 forward = Vector3.Cross(right, up);

			return Quaternion.LookRotation(new Vector3(forward.x, -forward.y, forward.z), new Vector3(up.x, up.y, -up.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion InferrRotationFromParentPosition(Vector3 position, Vector3 parentPosition)
		{
			Vector3 direction = position - parentPosition;
			Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
			Vector3 normal = Vector3.Cross(perpendicular, direction);

			// In the reference implementation on github.com/kinect/samples the
			// normal is the forward and the direction the upwards vector but
			// then the hand tip and thumb are not pointing along the forward axis:
			// Quaternion.LookRotation(normal, direction);

			return Quaternion.LookRotation(direction + 0.0001f * Vector3.forward, normal + 0.0001f * Vector3.up);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DistanceFromCameraSpacePoint(CameraSpacePoint point, UnityEngine.Vector4 floorClipPlane) =>
			(floorClipPlane.x * point.X + floorClipPlane.y * point.Y + floorClipPlane.z * point.Z + floorClipPlane.w) /
			(floorClipPlane.x * floorClipPlane.x + floorClipPlane.y * floorClipPlane.y + floorClipPlane.z * floorClipPlane.z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnityEngine.Vector3 CameraSpacePointToRealSpace(CameraSpacePoint point, UnityEngine.Vector4 floorClipPlane) =>
			new Vector3(-point.X, DistanceFromCameraSpacePoint(point, floorClipPlane), point.Z); // Mirror on the x axis.

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Quaternion OrientationToRealSpace(Windows.Kinect.Vector4 orientation) =>
			new Quaternion(orientation.X, -orientation.Y, -orientation.Z, orientation.W); // Mirror on the x axis.
	}
}
