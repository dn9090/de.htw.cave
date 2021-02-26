using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	public enum KinectActorConstructionType
	{
		Empty,
		Basic,
		Full,
		Prefab
	}

	[AddComponentMenu("Htw.Cave/Kinect/Kinect Actor Tracker")]
	[RequireComponent(typeof(KinectManager))]
	public sealed class KinectActorTracker : MonoBehaviour
	{
		public event Action<KinectActor> onActorCreated;

		public event Action<KinectActor> onActorDestroy;

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

		private KinectBodyFrame m_BodyFrame;

		private int m_ActorCount;

		private long m_Frame;

		public void Awake()
		{
			this.m_Manager = GetComponent<KinectManager>();
			this.m_Manager.onSensorOpen += PrepareTracking;
			this.m_Manager.onSensorClose += StopTracking;
			this.m_BodyFrame = KinectBodyFrame.Create();
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
			// because the tracking id in the actors can never be higher than in the bodies.

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
					this.m_BodyFrame.RefreshBodyData(bodies[bodyIndex], faces[bodyIndex], floorClipPlane);
					this.m_Actors[this.m_ActorCount++] = this.m_Actors[i];
					this.m_Actors[i].UpdateTrackingData(ref this.m_BodyFrame);

					++bodyIndex;
					continue;
				}

				// Destroy actor if no matching tracking id can be found.
				this.m_Actors[i].UpdateTrackingId(0);
				this.onActorDestroy?.Invoke(this.m_Actors[i]);
				Destroy(this.m_Actors[i].gameObject);
			}

			// Create a new actor for each new tracking id.
			for(int i = bodyIndex; i < bodyCount; ++i)
			{
				this.m_BodyFrame.RefreshBodyData(bodies[i], faces[i], floorClipPlane);

				var actor = ConstructActor();
				this.m_Actors[this.m_ActorCount++] = actor;
				
				actor.UpdateTrackingData(ref this.m_BodyFrame);
				this.onActorCreated?.Invoke(actor);
			}

			Array.Clear(this.m_Actors, this.m_ActorCount, this.m_Actors.Length - this.m_ActorCount);
		}

		private KinectActor ConstructActor()
		{
			var trackingId = this.m_BodyFrame.body.GetTrackingIdFast();
			var name = "Kinect Actor #" + trackingId;
			var actor = KinectActor.Create(name, transform, this.constructionType, this.prefab);

			actor.UpdateTrackingId(trackingId);

			return actor;
		}
	}
}
