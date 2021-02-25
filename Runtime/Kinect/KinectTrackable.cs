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
	/// Base class of a trackable <see cref="GameObject"/>.
	/// The component retrieves the tracking data from a specific <see cref="KinectActor"/>.
	/// </summary>
	public abstract class KinectTrackable : MonoBehaviour
	{
		/// <summary>
		/// Gets whether or not the component is actively tracked or if
		/// the tracking data is inferred.
		/// </summary>
		public TrackingState trackingState => this.m_TrackingState;

		/// <summary>
		/// The <see cref="KinectActor"/> that is responsible for updating
		/// the components tracking data.
		/// </summary>
		public KinectActor actor => this.m_Actor;

		private TrackingState m_TrackingState;

		private KinectActor m_Actor;

		public void Start()
		{
			OnTransformParentChanged();
		}

		public void OnTransformParentChanged()
		{
			if(this.m_Actor != null)
				this.m_Actor.Untrack(this);
			
			if(transform.parent.TryGetComponent<KinectActor>(out KinectActor actor))
			{
				this.m_Actor = actor;
				this.m_Actor.Track(this);
			}
		}

		internal void UpdateTrackingData(in KinectFrameBuffer frameBuffer, ref Bounds bounds)
		{
			this.m_TrackingState = UpdateTrackingData(in frameBuffer);
			bounds.Encapsulate(transform.position);
		}

		protected abstract TrackingState UpdateTrackingData(in KinectFrameBuffer frameBuffer);

		public static T Create<T>(string name, Transform parent = null) where T : KinectTrackable
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			
			return gameObject.AddComponent<T>();
		}
	}
}