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
	public struct KinectBodyFrame
	{
		public Body body;

		public FaceFrameResult face;

		public KinectJoint[] joints;

		public KinectJoint this[JointType jointType] => this.joints[(int)jointType];

		private Dictionary<JointType, Windows.Kinect.Joint> m_Joints;

		private Dictionary<JointType, JointOrientation> m_JointOrientations;

		public static KinectBodyFrame Create() => new KinectBodyFrame(25);

		private KinectBodyFrame(int capacity)
		{
			this.body = null;
			this.face = null;
			this.joints = new KinectJoint[capacity];
			this.m_Joints = new Dictionary<JointType, Windows.Kinect.Joint>(capacity);
			this.m_JointOrientations = new Dictionary<JointType, JointOrientation>(capacity);
		}

		public void RefreshBodyData(Body body, FaceFrameResult face, UnityEngine.Vector4 floorClipPlane)
		{
			this.body = body;
			this.face = face;
			this.body.RefreshJointsFast(this.m_Joints);
			this.body.RefreshJointOrientationsFast(this.m_JointOrientations);
			KinectJoint.RefreshJointData(this.joints, floorClipPlane, this.m_Joints, this.m_JointOrientations);
		}
	}

	public readonly struct KinectJoint
	{
		public readonly Vector3 position;
		
		public readonly Quaternion rotation;

		public readonly TrackingState trackingState;

		public KinectJoint(Vector3 position, Quaternion rotation, TrackingState trackingState)
		{
			this.position = position;
			this.rotation = rotation;
			this.trackingState = trackingState;
		}

		public static void RefreshJointData(KinectJoint[] buffer, UnityEngine.Vector4 floorClipPlane,
			Dictionary<JointType, Windows.Kinect.Joint> joints, Dictionary<JointType, JointOrientation> jointOrientations)
		{
			var index = 0;

			for(int i = (int)JointType.SpineShoulder; i < buffer.Length; i = index++)
			{
				var jointType = (JointType)i;
				var joint = joints[jointType];
				var jointOrientation = jointOrientations[jointType];

				var position = KinectHelper.CameraSpacePointToRealSpace(joint.Position, floorClipPlane);
				var rotation = KinectHelper.OrientationToRealSpace(jointOrientation.Orientation);

				if(rotation.IsZero())
				{
					var parent = KinectHelper.GetParentJointType(jointType);
					rotation = KinectHelper.InferrRotationFromParentPosition(position, buffer[(int)parent].position);
				}
				
				buffer[i] = new KinectJoint(position, rotation, joint.TrackingState);
			}
		}
	}
}
