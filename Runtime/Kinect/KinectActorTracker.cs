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
	/// This component can be attached to the <see cref="KinectManager"/>
	/// and is responsible for creating <see cref="KinectActor"/> instances
	/// for each tracked person. The <see cref="KinectActor"/> is
	/// constructed according to the <see cref="constructionType"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Actor Tracker")]
	[RequireComponent(typeof(KinectManager))]
	public sealed class KinectActorTracker : MonoBehaviour, IEnumerable<KinectActor>
	{
		public event Action<KinectActor> onActorCreated;

		public event Action<KinectActor> onActorDestroy;

		/// <summary>
		/// Number of currently tracked actors.
		/// </summary>
		public int actorCount => this.m_ActorCount;

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

		private KinectActor[] m_Actors;

		private int m_ActorCount;

		private long m_Frame;

		public void Awake()
		{
			this.m_Manager = GetComponent<KinectManager>();
			this.m_Manager.onSensorOpen += PrepareTracking;
			this.m_Manager.onSensorClose += StopTracking;
			enabled = false;
		}

		public void FixedUpdate()
		{
			TrackActors();
		}

		public void LateUpdate()
		{
			TrackActors();
		}

		public KinectActor[] GetActors()
		{
			if(this.m_ActorCount == 0)
				return Array.Empty<KinectActor>();

			var actors = new KinectActor[this.m_ActorCount];
			Array.Copy(this.m_Actors, actors, this.m_ActorCount);

			return actors;
		}

		public KinectActor GetLongestTracked()
		{
			KinectActor actor = null;
			float time = Time.time;

			for(int i = 0; i < this.m_ActorCount; ++i)
			{
				if(this.m_Actors[i].createdAt < time)
				{
					time = this.m_Actors[i].createdAt;
					actor = this.m_Actors[i];
				}
			}

			return actor;
		}

		public KinectActor GetClosestToPoint(Vector3 point)
		{
			KinectActor actor = null;
			float distance = float.MaxValue;

			for(int i = 0; i < this.m_ActorCount; ++i)
			{
				float sqr = (this.m_Actors[i].bounds.center - point).sqrMagnitude;

				if(sqr < distance)
				{
					distance = sqr;
					actor = this.m_Actors[i];
				}
			}

			return actor;
		}

		public KinectActor GetClosestToPoint(Vector3 point, JointType type)
		{
			KinectActor actor = null;
			float distance = float.MaxValue;

			for(int i = 0; i < this.m_ActorCount; ++i)
			{
				Vector3 position = this.m_Actors[i].transform.TransformPoint(this.m_Actors[i].bodyFrame[type].position);
				float sqr = (position - point).sqrMagnitude;

				if(sqr < distance)
				{
					distance = sqr;
					actor = this.m_Actors[i];
				}
			}

			return actor;
		}

		public IEnumerator<KinectActor> GetEnumerator() => new Enumerator(this.m_Actors, this.m_ActorCount);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private void PrepareTracking()
		{
			enabled = true;
			this.m_Actors = new KinectActor[this.m_Manager.trackingCapacity * 2]; // Allocate double buffer.
		}

		private void StopTracking()
		{
			enabled = false;
		}

		private void TrackActors()
		{
			var frame = this.m_Manager.AcquireFrames(out Body[] bodies, out FaceFrameResult[] faces, out int bodyCount);

			// Only update the tracking data if the
			// acquired frame is new.
			if(frame > this.m_Frame)
			{
				TrackActors(bodies, faces, bodyCount);
				this.m_Frame = frame;
			}
		}

		private void TrackActors(Body[] bodies, FaceFrameResult[] faces, int bodyCount)
		{
			var floorClipPlane = this.m_Manager.floorClipPlane;

			// According to the book "Beginning Kinect Programming with the Microsoft Kinect SDK"
			// (even though it is the Kinect v1) we know that:
			// "The skeleton tracking engine assigns each skeleton a unique identifier.
			// This identifier is an integer, which incrementally grows with each new skeleton.
			// Do not expect the value assigned to the next new skeleton to grow sequentially,
			// but rather the next value will be greater than the previous."
			// The idea here is that the actor can be found with an early exit
			// because the highest tracking id in the actors can never be higher than the highest in the bodies.

			int bufferStart = this.m_Actors.Length / 2;
			int bufferEnd = bufferStart + this.m_ActorCount;
			int bodyIndex = 0;

			// Copy the current actors in the backup buffer.
			Array.Copy(this.m_Actors, 0, this.m_Actors, bufferStart, this.m_ActorCount);
			this.m_ActorCount = 0;

			for(int i = bufferStart; i < bufferEnd; ++i)
			{
				// Update tracking data if the actor tracking id matches the body tracking id.
				if(bodyIndex < bodyCount && bodies[bodyIndex].GetTrackingIdFast() == this.m_Actors[i].trackingId)
				{
					this.m_Actors[this.m_ActorCount++] = this.m_Actors[i];
					this.m_Actors[i].UpdateTrackingData(bodies[bodyIndex], faces[bodyIndex], floorClipPlane);

					++bodyIndex;
					continue;
				}

				// Destroy actor if no matching tracking id can be found.
				this.onActorDestroy?.Invoke(this.m_Actors[i]);
				Destroy(this.m_Actors[i].gameObject);
			}

			// Create a new actor for each new tracking id.
			for(int i = bodyIndex; i < bodyCount; ++i)
			{
				var actor = KinectActorBuilder
					.Construct("Kinect Actor #" + bodies[i].GetTrackingIdFast(), transform, this.constructionType, this.prefab)
					.Build();
				this.m_Actors[this.m_ActorCount++] = actor;
				
				actor.UpdateTrackingData(bodies[i], faces[i], floorClipPlane);
				this.onActorCreated?.Invoke(actor);
			}

			Array.Clear(this.m_Actors, this.m_ActorCount, this.m_Actors.Length - this.m_ActorCount);
		}

		public struct Enumerator : IEnumerator<KinectActor>, IEnumerator
		{
			private KinectActor[] m_Actors;
			private int m_Index;
			private int m_Count;
			private KinectActor m_Current;

			internal Enumerator(KinectActor[] actors, int count)
			{
				this.m_Actors = actors;
				this.m_Index = 0;
				this.m_Count = count;
				this.m_Current = default;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if ((uint)this.m_Index < (uint)this.m_Count)
				{
					this.m_Current = this.m_Actors[this.m_Index++];
					return true;
				}

				this.m_Current = default;
				this.m_Index = this.m_Count + 1;
				return false;
			}

			public KinectActor Current => this.m_Current;

			object IEnumerator.Current => this.m_Current;

			void IEnumerator.Reset()
			{
				this.m_Index = 0;
				this.m_Current = default;
			}
		}
	}
}
