using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Holds data required to convert the real space provided by the Kinect sensor
	/// to Unity space.
	/// </summary>
	[CreateAssetMenu(fileName = "New Kinect Settings", menuName = "Htw.Cave/Kinect Settings", order = 30)]
	public class KinectSettings : ScriptableObject
	{
		[SerializeField]
		private Vector3 sensorLocation;
		public Vector3 SensorLocation
		{
			get => this.sensorLocation;
			set => this.sensorLocation = value;
		}

		[SerializeField]
		private Rect trackingArea;
		public Rect TrackingArea
		{
			get
			{
				Rect area = this.trackingArea;
				area.center -= area.size * 0.5f;
				return area;
			}
			set => this.trackingArea = value;
		}
	}
}
