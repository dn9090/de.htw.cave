using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;

namespace Htw.Cave.Kinect
{
	[Serializable]
	[Flags]
	public enum TrackingMask
	{
		None = 0,
		Position = 1 << 0,
		Rotation = 1 << 1,
		All = ~0
	}

	[Serializable]
	[Flags]
	public enum JointConstrains
	{
		None = 0,
		X = 1 << 0,
		Y = 1 << 1,
		Z = 1 << 2,
		All = ~0
	}

	[Serializable]
	public enum RotationType
	{
		Joint,
		Face,
		Shoulder
	}

	/// <summary>
	/// Updates the transform based on a joint with the provided constrains.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Trackable Object")]
	public class KinectTrackableObject : KinectTrackable
	{
		[SerializeField]
		private TrackingMask tracking;
		public TrackingMask Tracking
		{
			get => this.tracking;
			set => this.tracking = value;
		}

		[SerializeField]
		private JointType jointType;
		public JointType JointType
		{
			get => this.jointType;
			set => this.jointType = value;
		}

		[SerializeField]
		private JointConstrains jointConstrains;
		public JointConstrains JointConstrains
		{
			get => this.jointConstrains;
			set => this.jointConstrains = value;
		}

		[SerializeField]
		private RotationType rotationType;
		public RotationType RotationType
		{
			get => this.rotationType;
			set => this.rotationType = value;
		}

		public void Update()
		{
			if(base.actor.IsTracked)
			{
				if(this.tracking.HasFlag(TrackingMask.Position))
					UpdatePosition();

				if(this.tracking.HasFlag(TrackingMask.Rotation))
					UpdateRotation();
			}
		}

		public void Reset()
		{
			this.tracking = TrackingMask.Position;
			this.jointType = JointType.SpineMid;
			this.jointConstrains = JointConstrains.None;
			this.rotationType = RotationType.Face;
		}

		private void UpdatePosition()
		{
			if(base.actor.IsJointTracked(this.jointType))
			{
				Vector3 position = base.actor.GetJointPosition(this.jointType);

				if(this.jointConstrains.HasFlag(JointConstrains.None))
					transform.localPosition = position;
				else
					ConstrainedPosition(position);
			}
		}

		private void UpdateRotation()
		{
			Quaternion rotation = transform.localRotation;

			switch(this.rotationType)
			{
				case RotationType.Joint:
					if(base.actor.IsJointTracked(this.jointType))
						rotation = base.actor.GetJointRotation(this.jointType);
					break;
				case RotationType.Face:
					if(base.actor.IsHeadTracked())
						rotation = base.actor.GetFaceRotation();
					break;
				case RotationType.Shoulder:
					if(base.actor.IsShoulderTracked())
						rotation = base.actor.GetShoulderRotation();
					break;
			}

			transform.localRotation = rotation;
		}

		private void ConstrainedPosition(Vector3 position)
		{
			transform.localPosition = new Vector3(
				this.jointConstrains.HasFlag(JointConstrains.X) ? transform.localPosition.x : position.x,
				this.jointConstrains.HasFlag(JointConstrains.Y) ? transform.localPosition.y : position.y,
				this.jointConstrains.HasFlag(JointConstrains.Z) ? transform.localPosition.z : position.z
			);
		}
	}
}
