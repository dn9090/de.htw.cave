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
	/// <summary>
	/// Represents a tracked person. Automatically updates
	/// <see cref="KinectTrackable"/> components in the child hierarchy
	/// with the provided tracking data from the <see cref="KinectActorTracker"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Actor")]
	public sealed class KinectActor : MonoBehaviour
	{
		public event Action onTrackingDataUpdated;
	
		/// <summary>
		/// The tracking id of the actor which corresponds to a
		/// tracking id of a body.
		/// </summary>
		public ulong trackingId => this.m_BodyFrame.trackingId;

		/// <summary>
		/// Provides the last updated body data.
		/// </summary>
		public KinectBodyFrame bodyFrame => this.m_BodyFrame;

		/// <summary>
		/// Gets the direction and strength a body is leaning, which is a number between -1 (leaning left or back)
		/// and 1 (leaning right or front).
		/// Leaning left and right corresponds to X movement and leaning back and forward corresponds to Y movement.
		/// </summary>
		public UnityEngine.Vector2 lean => this.m_BodyFrame.lean;
		
		/// <summary>
		/// Approximation of the persons height. This is the maximum detected distance
		/// between the head and the foot joints. Becomes more accurate the longer the person
		/// stands straight and the longer the person is tracked.
		/// </summary>
		public float height => this.m_Height;

		/// <summary>
		/// The world bounds of the actor.
		/// Includes head, spine and foot positions.
		/// </summary>
		public Bounds bounds => this.m_Bounds;
		

		/// <summary>
		/// The time at the creation of the component.
		/// </summary>
		public float createdAt => this.m_CreatedAt;
		
		/// <summary>
		/// The tracked joint positions and rotations in world space.
		/// </summary>
		public KinectJoint[] joints => this.m_Joints;
		
		internal long updatedAtFrame;
		
		private KinectBodyFrame m_BodyFrame;
		
		private KinectJoint[] m_Joints;

		private Bounds m_Bounds;

		private float m_CreatedAt;

		private float m_Height;

		public void Awake()
		{
			this.m_BodyFrame = new KinectBodyFrame();
			this.m_Joints = new KinectJoint[KinectHelper.jointTypeCount];
			this.m_CreatedAt = Time.time;
		}
		
		public KinectJoint GetJoint(JointType jointType) => this.m_Joints[(int)jointType];

		internal void OnUpdateTrackingData(KinectManager manager, Body body, FaceFrameResult face, long frame)
		{
			this.updatedAtFrame = frame;
			this.m_BodyFrame.RefreshFrameData(body, face, manager.floorClipPlane);
			
			KinectJoint.TransformJointData(this.m_BodyFrame.joints, this.m_Joints, manager.transform);

			RecalculatePositionAndBounds();

			this.onTrackingDataUpdated?.Invoke();
		}

		private void RecalculatePositionAndBounds()
		{		
			var joint = GetJoint(JointType.SpineBase);

			transform.position = joint.position;

			this.m_Bounds = new Bounds(joint.position, Vector3.zero);
		
			joint = GetJoint(JointType.Head);

			if(joint.trackingState != TrackingState.NotTracked)
				this.m_Bounds.Encapsulate(joint.position);
			
			joint = GetJoint(JointType.FootLeft);
		
			if(joint.trackingState != TrackingState.NotTracked)
				this.m_Bounds.Encapsulate(joint.position);

			joint = GetJoint(JointType.FootRight);

			if(joint.trackingState != TrackingState.NotTracked)
				this.m_Bounds.Encapsulate(joint.position);
			
			// Compensation because the head joint is in the middle of the head.
			this.m_Height = Mathf.Max(this.m_Height, this.m_Bounds.size.y + 0.1f);
		}
	}
}
