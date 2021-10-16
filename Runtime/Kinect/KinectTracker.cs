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
	/// The construction type defines how a newly
	/// tracked actor will be created and which components are added.
	/// </summary>
	public enum KinectActorConstructionType
	{
		Basic,
		Full,
		Prefab
	}

	/// <summary>
	/// This component can be attached to the <see cref="KinectManager"/>
	/// and is responsible for creating <see cref="KinectActor"/> instances
	/// for each tracked person. The <see cref="KinectActor"/> is
	/// constructed according to the <see cref="constructionType"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Tracker")]
	[RequireComponent(typeof(KinectManager))]
	public sealed class KinectTracker : MonoBehaviour
	{
		public event Action<KinectActor> onCreateActor;

		public event Action<KinectActor> onDestroyActor;

		/// <summary>
		/// List of currently tracked actors.
		/// </summary>
		public IReadOnlyList<KinectActor> actors => this.m_Actors;
		
		/// <summary>
		/// Defines how a <see cref="KinectActor"/> is created when a
		/// new person is tracked.
		/// </summary>
		public KinectActorConstructionType constructionType;

		/// <summary>
		/// The custom prefab that will be cloned when the <see cref="constructionType"/> 
		/// is set to <see cref="KinectActorConstructionType.Prefab"/>.
		/// </summary>
		public KinectActor prefab;

		private KinectManager m_Manager;
		
		private List<KinectActor> m_Actors;
		
		private long m_Frame;
		
		public void Awake()
		{
			this.m_Manager = GetComponent<KinectManager>();
			this.m_Manager.onSensorOpen += () => enabled = true;
			this.m_Manager.onSensorClose += () => enabled = false;
			this.m_Actors = new List<KinectActor>(this.m_Manager.trackingCapacity);
			
			enabled = false;
		}
		
		public void Update()
		{
			var frame = this.m_Manager.AcquireFrames(out Body[] bodies, out FaceFrameResult[] faces, out int bodyCount);

			// Only update the tracking data if the
			// acquired frame is new.
			if(frame > this.m_Frame)
			{
				TrackActors(bodies, faces, bodyCount, frame);
				this.m_Frame = frame;
			}
		}
		
		public void OnDisable()
		{			
			foreach(var actor in this.m_Actors)
			{
				this.onDestroyActor?.Invoke(actor);
				Destroy(actor.gameObject);
			}
			
			this.m_Actors.Clear();
		}

		public void Reset()
		{
			this.constructionType = KinectActorConstructionType.Full;
			this.prefab = null;
		}
		
		private void TrackActors(Body[] bodies, FaceFrameResult[] faces, int bodyCount, long frame)
		{
			// Normally this would be cleaner with a double buffer
			// (which is simple with Span<> unfortunately Unity is stuck with .NET 4.x)
			// but due to the fact that only 8 bodies can be tracked
			// two passes is just as fast.
			
			var lastActorCount = this.m_Actors.Count;
									
			for(int i = 0; i < bodyCount; ++i)
			{
				var trackingId = bodies[i].GetTrackingIdFast();
				var actor = FindActorByTrackingId(trackingId);
				
				if(actor == null)
				{
					actor = ConstructActor("Kinect Actor #" + trackingId);
					this.m_Actors.Add(actor);
					actor.OnUpdateTrackingData(this.m_Manager, bodies[i], faces[i], frame);
					onCreateActor?.Invoke(actor);
					continue;
				}
				
				actor.OnUpdateTrackingData(this.m_Manager, bodies[i], faces[i], frame);
			}
			
			for(int i = lastActorCount - 1; i >= 0; --i)
			{
				var actor = this.m_Actors[i];
			
				// Check if the actor is too old (not updated in this frame).
				if(actor.updatedAtFrame < frame)
				{
					this.m_Actors.RemoveAt(i);
					this.onDestroyActor?.Invoke(actor);
					Destroy(actor.gameObject);
				}
			}
		}
		
		private KinectActor ConstructActor(string name)
		{
			KinectActor actor = null;

			if(this.constructionType == KinectActorConstructionType.Prefab)
			{
				actor = Instantiate<KinectActor>(prefab, transform);
				actor.name = name;
			} else {
				actor = new GameObject(name).AddComponent<KinectActor>();
				actor.transform.parent = transform;
				
				if(this.constructionType == KinectActorConstructionType.Full)
				{
					var skeleton = actor.gameObject.AddComponent<KinectSkeleton>();
					skeleton.CreateJointTree(JointType.SpineBase);
				}
			}
			
			return actor;
		}
		
		private KinectActor FindActorByTrackingId(ulong trackingId)
		{
			for(int i = 0; i < this.m_Actors.Count; ++i)
				if(trackingId == this.m_Actors[i].trackingId)
					return this.m_Actors[i];
			return null;
		}
	}
}
