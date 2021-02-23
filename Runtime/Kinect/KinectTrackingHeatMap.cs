using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	[AddComponentMenu("Htw.Cave/Kinect/Visualizer/Kinect Tracking Heat Map")]
	[RequireComponent(typeof(KinectManager))]
	public sealed class KinectTrackingHeatMap : MonoBehaviour
	{
		[SerializeField]
		private Vector2 gridDimensions;

		[SerializeField]
		private float gridSize;

		public void Start()
		{
		}

		public void Reset()
		{
			this.gridDimensions = new Vector2(3f, 3f);
			this.gridSize = 0.25f;
		}

		public void OnValidate()
		{
			this.gridSize = Mathf.Clamp(gridSize, 0f, Mathf.Min(gridDimensions.x, gridDimensions.y));
		}
	}
}
