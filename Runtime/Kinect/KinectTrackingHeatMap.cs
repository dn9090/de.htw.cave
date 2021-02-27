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
	/// The component allows the visual representation of
	/// the tracking quality in different areas.
	/// Needs to be attached to a <see cref="KinectTrackingArea"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Tracking Heat Map")]
	[RequireComponent(typeof(KinectTrackingArea))]
	public sealed class KinectTrackingHeatMap : MonoBehaviour
	{
		[SerializeField]
		private float m_GridSize;

		private KinectTrackingArea m_TrackingArea;

		private Rect m_GridArea;

		public void Awake()
		{
			this.m_TrackingArea = GetComponent<KinectTrackingArea>();
			var size = this.m_TrackingArea.volume.size;
			this.m_GridArea = new Rect(Vector2.zero, new Vector3(size.x, size.z));
		}

		public void Reset()
		{
			this.m_GridSize = 0.25f;
		}
	}
}
