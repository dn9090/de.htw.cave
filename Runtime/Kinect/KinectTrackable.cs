using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Represents a object that can be tracked by the Kinect sensor.
	/// </summary>
	public abstract class KinectTrackable : MonoBehaviour
	{
		protected KinectActor actor;

		public virtual void Start()
		{
			this.actor = KinectPlayArea.Actor;
		}
	}
}
