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
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Actor")]
	public sealed class KinectActor : MonoBehaviour
	{
		public event Action<KinectTrackable> onTrack;

		public event Action<KinectTrackable> onUntrack;

		/// <summary>
		/// The tracking id of the actor which corresponds to a
		/// tracking id of a body.
		/// </summary>
		public ulong trackingId => this.m_TrackingId;

		/// <summary>
		/// Indicates whether the actor is being tracked.
		/// </summary>
		public bool isTracked => this.m_TrackingId > 0;

		/// <summary>
		/// Gets the amount a body is leaning, which is a number between -1 (leaning left or back)
		/// and 1 (leaning right or front).
		/// Leaning left and right corresponds to X movement and leaning back and forward corresponds to Y movement.
		/// </summary>
		public UnityEngine.Vector2 lean => this.m_Lean;

		/// <summary>
		/// Approximation of the persons height. This is the maximum detected distance
		/// between the head and the foot joints.
		/// </summary>
		public float height => this.m_Height;

		/// <summary>
		/// The world bounds of the actor.
		/// </summary>
		public Bounds bounds => this.m_Bounds;

		/// <summary>
		/// The time at the creation of the component.
		/// </summary>
		public float createdAt => this.m_CreatedAt;

		/// <summary>
		/// Set of objects that are tracked and updated by the actor.
		/// </summary>
		public ISet<KinectTrackable> trackables => this.m_Trackables;

		private ulong m_TrackingId;

		private UnityEngine.Vector2 m_Lean;

		private Bounds m_Bounds;

		private HashSet<KinectTrackable> m_Trackables;

		private float m_CreatedAt;

		private float m_Height;

		public void Awake()
		{
			this.m_Trackables = new HashSet<KinectTrackable>();
			this.m_CreatedAt = Time.time;
		}

		internal void UpdateTrackingId(ulong trackingId) => this.m_TrackingId = trackingId;

		internal void UpdateTrackingData(ref KinectBodyFrame bodyFrame)
		{
			transform.localPosition = Vector3.zero;

			RecalculateBounds(ref bodyFrame);

			this.m_Lean = bodyFrame.body.GetLeanDirection();
			this.m_Height = Mathf.Max(this.m_Height, this.m_Bounds.size.y + 0.1f); // Compensation because the head joint is in the middle of the head.
			
			foreach(var trackable in this.m_Trackables)
				trackable.UpdateTrackingData(ref bodyFrame, ref this.m_Bounds);
			
			Vector3 center = new Vector3(this.m_Bounds.center.x, 0f, this.m_Bounds.center.z);
			Vector3 translation = center - transform.position;
			transform.position = center;

			for(int i = 0; i < transform.childCount; ++i)
				transform.GetChild(i).localPosition -= translation;
		}

		internal void Track(KinectTrackable trackable)
		{
			if(this.m_Trackables.Contains(trackable))
				return;
			
			this.m_Trackables.Add(trackable);
			this.onTrack?.Invoke(trackable);
		}

		internal void Untrack(KinectTrackable trackable)
		{
			if(!this.m_Trackables.Contains(trackable))
				return;
			
			this.m_Trackables.Remove(trackable);
			this.onUntrack?.Invoke(trackable);
		}

		private void RecalculateBounds(ref KinectBodyFrame bodyFrame)
		{
			var joint = bodyFrame[JointType.SpineBase];

			this.m_Bounds = new Bounds(joint.position, Vector3.zero);
		
			joint = bodyFrame[JointType.Head];

			if(joint.trackingState != TrackingState.NotTracked)
				this.m_Bounds.Encapsulate(joint.position);
			
			joint = bodyFrame[JointType.FootLeft];
		
			if(joint.trackingState != TrackingState.NotTracked)
				this.m_Bounds.Encapsulate(joint.position);

			joint = bodyFrame[JointType.FootRight];

			if(joint.trackingState != TrackingState.NotTracked)
				this.m_Bounds.Encapsulate(joint.position);
		
			this.m_Bounds.center = transform.TransformPoint(this.m_Bounds.center);
		}

		public static KinectActor Create(string name, Transform parent, KinectActorConstructionType constructionType, KinectActor prefab = null)
		{
			if(constructionType == KinectActorConstructionType.Prefab)
			{
				var actor = MonoBehaviour.Instantiate<KinectActor>(prefab, parent);
				actor.gameObject.name = name;

				return actor;
			}
				
			var gameObject = new GameObject(name);
			gameObject.transform.parent = parent;

			foreach(var value in Enum.GetValues(typeof(JointType))) // @Todo: This can be hard-coded in the future.
			{
				var jointType = (JointType)value;

				switch(jointType)
				{
					case JointType.Head:
						KinectTrackable.Create<KinectHead>("Kinect Head", gameObject.transform);
						break;
					case JointType.HandLeft:
						KinectTrackable.Create<KinectHand>("Kinect Hand Left", gameObject.transform)
							.handType = HandType.Left;
						break;
					case JointType.HandRight:
						KinectTrackable.Create<KinectHand>("Kinect Hand Right", gameObject.transform)
							.handType = HandType.Right;
						break;
					default:
						if(constructionType == KinectActorConstructionType.Full)
						{
							KinectTrackable.Create<KinectDynamicJoint>("Kinect Dynamic Joint (" + jointType + ")", gameObject.transform)
								.jointType = jointType;
						}
						break;
				}		
			}

			return gameObject.AddComponent<KinectActor>();
		}
	}
}
