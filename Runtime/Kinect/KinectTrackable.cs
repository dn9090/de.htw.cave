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
	/// Base class of a trackable <see cref="GameObject"/>.
	/// The component retrieves the tracking data from a specific <see cref="KinectActor"/>.
	/// </summary>
	public abstract class KinectTrackable : MonoBehaviour
	{
		/// <summary>
		/// Defines the parameters that are used by the filter
		/// when <see cref="filterPositionAndRotation"/> is set.
		/// </summary>
		public static OneEuroParams filterParams = new OneEuroParams(1f); 

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

		/// <summary>
		/// Defines if the tracking data is filtered before updating
		/// the <see cref="transform.localPosition"/> and <see cref="transform.localRotation"/>.
		/// </summary>
		public bool filterPositionAndRotation;

		private TrackingState m_TrackingState;

		private KinectActor m_Actor;

		private OneEuroFilter3 m_PositionFilter;

		private OneEuroFilter4 m_RotationFilter;

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

		internal void UpdateTrackingData(ref KinectBodyFrame bodyFrame, ref Bounds bounds)
		{
			this.m_TrackingState = UpdateTrackingData(ref bodyFrame);

			if(this.filterPositionAndRotation)
			{
				transform.localPosition = this.m_PositionFilter.Filter(transform.localPosition, 30f, in filterParams);
				transform.localRotation = this.m_RotationFilter.Filter(transform.localRotation, 30f, in filterParams);
			}

			bounds.Encapsulate(transform.position);
		}

		protected abstract TrackingState UpdateTrackingData(ref KinectBodyFrame bodyFrame);

		public static T Create<T>(string name, Transform parent = null, Action<T> action = null) where T : KinectTrackable
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			
			var component = gameObject.AddComponent<T>();
			action?.Invoke(component);
			
			return component;
		}
	}
}