using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	public abstract class KinectTrackable : MonoBehaviour
	{
		public TrackingState trackingState => this.m_TrackingState;

		private TrackingState m_TrackingState;

		public void UpdateTrackingData(in KinectTrackingData trackingData, bool deactivateNotTracked)
		{
			this.m_TrackingState = UpdateTrackingData(in trackingData);
			gameObject.SetActive(!deactivateNotTracked || this.m_TrackingState != TrackingState.NotTracked);
		}

		protected abstract TrackingState UpdateTrackingData(in KinectTrackingData trackingData);

		public static T Create<T>(string name, Transform parent = null) where T : KinectTrackable
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			
			return gameObject.AddComponent<T>();
		}
	}
}