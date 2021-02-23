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
	public readonly struct KinectTrackingData
	{
		public readonly Body body;

		public readonly FaceFrameResult face;

		public readonly UnityEngine.Vector4 floor;
	
		public readonly UnityEngine.Vector3 coordinateOrigin;

		public KinectTrackingData(Body body, FaceFrameResult face, UnityEngine.Vector4 floor, UnityEngine.Vector3 coordinateOrigin)
		{
			this.body = body;
			this.face = face;
			this.floor = floor;
			this.coordinateOrigin = coordinateOrigin;
		}
	}

	[AddComponentMenu("Htw.Cave/Kinect/Kinect Actor")]
	public sealed class KinectActor : MonoBehaviour
	{
		public ulong trackingId => this.m_TrackingId;

		public bool isTracked => this.m_IsTracked;

		/// <summary>
		/// Gets the amount a body is leaning, which is a number between -1 (leaning left or back)
		/// and 1 (leaning right or front).
		/// Leaning left and right corresponds to X movement and leaning back and forwarcorresponds to Y movement.
		/// </summary>
		public UnityEngine.Vector2 lean => this.m_Lean;

		/// <summary>
		/// The time at the creation of the component.
		/// </summary>
		public float createdAt => this.m_CreatedAt;

		public bool deactivateNotTracked;

		private ulong m_TrackingId;

		private bool m_IsTracked;

		private UnityEngine.Vector2 m_Lean;

		private float m_CreatedAt;

		private KinectTrackable[] m_Trackables;

		private int m_TrackableCount;

		public void Awake()
		{
			this.m_CreatedAt = Time.time;
			
			OnTransformChildrenChanged();
		}

		public void UpdateTrackingState(ulong trackingId, bool isTracked)
		{
			this.m_TrackingId = trackingId;
			this.m_IsTracked = isTracked;
			gameObject.SetActive(isTracked);
		}

		public void UpdateTrackingData(in KinectTrackingData trackingData)
		{
			this.m_Lean = trackingData.body.LeanDirection();

			for(int i = 0; i < this.m_TrackableCount; ++i)
				this.m_Trackables[i].UpdateTrackingData(in trackingData, deactivateNotTracked);
		}

		public void OnTransformChildrenChanged()
		{
			if(this.m_Trackables == null || transform.childCount > this.m_Trackables.Length)
				this.m_Trackables = new KinectTrackable[transform.childCount];

			// @Todo: This can be optimized with a growing capacity and
			// a comparison which checks if the joint is already in the array.

			this.m_TrackableCount = 0;

			for(int i = 0; i < transform.childCount; ++i)
			{
				if(transform.GetChild(i).TryGetComponent<KinectTrackable>(out KinectTrackable trackable))
					this.m_Trackables[this.m_TrackableCount++] = trackable;
			}
		}

		public KinectTrackable[] GetTrackables()
		{
			var trackables = new KinectTrackable[this.m_TrackableCount];
			Array.Copy(this.m_Trackables, trackables, this.m_TrackableCount);

			return trackables; 
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
							KinectTrackable.Create<KinectJoint>("Kinect Joint", gameObject.transform)
								.jointType = jointType;
						}
						break;
				}		
			}

			return gameObject.AddComponent<KinectActor>();
		}
	}
}
