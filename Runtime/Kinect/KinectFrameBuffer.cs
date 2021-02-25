using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Represents a tracking frame of a <see cref="Body"/>.
	/// Includes cached access to the joint positions and orientations.
	/// </summary>
	public readonly struct KinectFrameBuffer
	{
		public readonly Body body;

		public readonly FaceFrameResult face;

		public readonly Dictionary<JointType, Joint> joints;

		public readonly Dictionary<JointType, JointOrientation> jointOrientations;

		public readonly UnityEngine.Vector4 floor;

		public static KinectFrameBuffer Empty() => new KinectFrameBuffer(new Dictionary<JointType, Joint>(), new Dictionary<JointType, JointOrientation>());

		public static void Refresh(ref KinectFrameBuffer frameBuffer, Body body, FaceFrameResult face, UnityEngine.Vector4 floor) =>
			frameBuffer = new KinectFrameBuffer(body, face, ref floor, in frameBuffer);

		private KinectFrameBuffer(Dictionary<JointType, Joint> joints, Dictionary<JointType, JointOrientation> jointOrientations)
		{
			this.body = null;
			this.face = null;
			this.joints = joints;
			this.jointOrientations = jointOrientations;
			this.floor = UnityEngine.Vector4.zero;
		}

		private KinectFrameBuffer(Body body, FaceFrameResult face, ref UnityEngine.Vector4 floor, in KinectFrameBuffer frameBuffer)
		{
			this.body = body;
			this.face = face;
			this.joints = frameBuffer.joints;
			this.jointOrientations = frameBuffer.jointOrientations;
			this.floor = floor;

			this.body.RefreshJointsFast(this.joints);
			this.body.RefreshJointOrientationsFast(this.jointOrientations);
		}
	}
}
