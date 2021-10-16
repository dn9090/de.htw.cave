using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Htw.Cave.Kinect;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave
{
	[AddComponentMenu("Htw.Cave/Virtual Eye Tracking")]
	[RequireComponent(typeof(VirtualEnvironment))]
	public class VirtualEyeTracking : MonoBehaviour
	{
		public static OneEuroParams filterParams = new OneEuroParams(1f);
	
		public KinectTracker tracker
		{
			get => this.m_Tracker;
			set
			{
				StopTracking();
				this.m_Tracker = value;
				StartTracking();
			}
		}
		
		public KinectActor actor => this.m_Actor;
	
		[SerializeField]
		private KinectTracker m_Tracker;
	
		private VirtualEnvironment m_Environment;
		
		private KinectActor m_Actor;
		
		private OneEuroFilter3 m_PositionFilter;
		
		private OneEuroFilter4 m_RotationFilter;
		
		public void Awake()
		{
			this.m_Environment = GetComponent<VirtualEnvironment>();
		}
		
		public void OnEnable()
		{
			StartTracking();
		}
		
		public void Update()
		{
			if(this.m_Actor == null || !this.m_Environment.Contains(this.m_Actor.bounds.center))
				SetActor(FindActorInEnvironment());
		}
		
		public void OnDisable()
		{
			StopTracking();
		}
		
		private void StartTracking()
		{
			if(this.m_Tracker != null)
			{
				this.m_Tracker.onCreateActor += OnCreateActor;
				this.m_Tracker.onDestroyActor += OnDestroyActor;
			}
		}
		
		private void StopTracking()
		{
			if(this.m_Tracker != null)
			{
				this.m_Tracker.onCreateActor -= OnCreateActor;
				this.m_Tracker.onDestroyActor -= OnDestroyActor;
			}
		
			this.m_Actor = null;
		}
		
		private void OnCreateActor(KinectActor actor)
		{
			if(this.m_Actor == null && this.m_Environment.Contains(actor.bounds.center))
				SetActor(actor);
		}
		
		private void OnDestroyActor(KinectActor actor)
		{
			if(this.m_Actor == actor)
				SetActor(FindActorInEnvironment());
		}
		
		private void SetActor(KinectActor actor)
		{
			if(this.m_Actor != null)
				this.m_Actor.onTrackingDataUpdated -= OnTrackingUpdated;
			
			this.m_Actor = actor;
			
			if(this.m_Actor != null)
				this.m_Actor.onTrackingDataUpdated += OnTrackingUpdated;
		}
		
		private KinectActor FindActorInEnvironment()
		{
			KinectActor best = null;
			float createdAt = float.MaxValue; 
		
			foreach(var actor in this.m_Tracker.actors)
			{
				if(actor.createdAt < createdAt
				&& this.m_Environment.Contains(actor.bounds.center))
				{
					best = actor;
					createdAt = actor.createdAt;
				}
			}

			return best;
		}
		
		private void OnTrackingUpdated()
		{
			var head = this.m_Actor.GetJoint(JointType.Head);
			
			switch(head.trackingState)
			{
				case TrackingState.Tracked:
					var faceRotation = this.m_Actor.bodyFrame.faceRotation;
						
					SetFilteredTransform(this.m_Environment.eyes.transform,
						 head.position, faceRotation.IsZero() ? head.rotation : faceRotation);
					break;
				case TrackingState.Inferred:
					SetFilteredTransform(this.m_Environment.eyes.transform,
						 head.position, head.rotation);
					break;
				case TrackingState.NotTracked:
					var shoulder = this.m_Actor.GetJoint(JointType.SpineShoulder);
					
					SetFilteredTransform(this.m_Environment.eyes.transform,
						shoulder.position + Vector3.up * 0.3f, shoulder.rotation);
					break;
			}
		}
		
		private void SetFilteredTransform(Transform target, Vector3 position, Quaternion rotation)
		{
			target.position = this.m_PositionFilter.Filter(position, KinectHelper.frameTime, in filterParams);
			target.rotation = this.m_RotationFilter.Filter(rotation, KinectHelper.frameTime, in filterParams);
		}
	}
}
