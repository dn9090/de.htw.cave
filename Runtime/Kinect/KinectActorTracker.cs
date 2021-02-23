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

		public KinectActorConstructionType constructionType;

		public KinectActor prefab;

		private KinectManager m_Manager;

		private KinectActor[] m_Actors;

		private int m_ActorCount;

		private TimeSpan m_TimeStamp;

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
			var actors = new KinectActor[this.m_ActorCount];
			Array.Copy(this.m_Actors, actors, this.m_ActorCount);

			return actors;
		}

		private void PrepareTracking()
		{
			enabled = true;
			this.m_Actors = new KinectActor[this.m_Manager.trackingCapacity * 2];
		}

		private void StopTracking()
		{
			enabled = false;
		}

		private void TrackActors()
		{
			var timeStamp = this.m_Manager.AcquireFrames(out Body[] bodies, out FaceFrameResult[] faces, out int count);

			// Only update the tracking data if the
			// acquired frame is new.
			if(timeStamp > this.m_TimeStamp)
			{
				TrackActors(bodies, faces, count);
				this.m_TimeStamp = timeStamp;
			}
		}

		private void TrackActors(Body[] bodies, FaceFrameResult[] faces, int count)
		{
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

			Array.Copy(this.m_Actors, 0, this.m_Actors, bufferStart, this.m_ActorCount);
			this.m_ActorCount = 0;

			for(int i = bufferStart; i < bufferEnd; ++i)
			{
				// Update tracking data if the actor tracking id matches the body tracking id.
				if(bodyIndex < count && bodies[bodyIndex].TrackingId == this.m_Actors[i].trackingId)
				{
					var data = new KinectTrackingData(bodies[bodyIndex], faces[bodyIndex], this.m_Manager.floor, this.m_Manager.sensorPosition);
					var actor = this.m_Actors[i];
					this.m_Actors[this.m_ActorCount++] = actor;
					actor.UpdateTrackingData(in data);

					++bodyIndex;
					continue;
				}

				// Destroy actor if no matching tracking id can be found.
				this.m_Actors[i].UpdateTrackingState(0, false);
				this.onActorDestroy?.Invoke(this.m_Actors[i]);
				Destroy(this.m_Actors[i].gameObject);
			}

			// Create a new actor for each new tracking id.
			for(int i = bodyIndex; i < count; ++i)
			{
				var actor = CreateActor(bodies[i], faces[i]);
				this.m_Actors[this.m_ActorCount++] = actor;
				this.onActorCreated?.Invoke(actor);
			}

			Array.Clear(this.m_Actors, this.m_ActorCount, this.m_Actors.Length - this.m_ActorCount);
		}

		private KinectActor CreateActor(Body body, FaceFrameResult face)
		{
			var name = "Kinect Actor #" + body.TrackingId;
			var actor = KinectActor.Create(name, transform, this.constructionType, this.prefab);
			var data = new KinectTrackingData(body, face, this.m_Manager.floor, this.m_Manager.sensorPosition);
			
			actor.UpdateTrackingState(body.TrackingId, true);
			actor.UpdateTrackingData(in data);

			return actor;
		}
	}
}
