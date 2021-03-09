using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Actor Trigger")]
	public sealed class KinectActorTrigger : MonoBehaviour
	{
		public event Action<KinectActor> onTriggerEnter;

		public event Action<KinectActor> onTriggerExit;

		/// <summary>
		/// The <see cref="KinectActor"/> closest to the
		/// center of the area.
		/// </summary>
		public KinectActor closestToCenter => this.m_ClosestToCenter;

		/// <summary>
		/// All <see cref="KinectActor"/> instances in the area.
		/// </summary>
		public IReadOnlyList<KinectActor> actors => this.m_Actors;

		/// <summary>
		/// Gets the bounds of the volume in world space.
		/// </summary>
		public Bounds bounds => new Bounds(transform.position, transform.lossyScale);

		[SerializeField]
		private KinectActorTracker m_ActorTracker;

		private KinectActor m_ClosestToCenter;

		private List<KinectActor> m_Actors; // Because there are < 10 objects max it should be faster than HashSet.

		public void Awake()
		{
			this.m_Actors = new List<KinectActor>();
		}

		public void Update()
		{
			var bounds = this.bounds;
			var minDistance = float.MaxValue;

			foreach(var actor in this.m_ActorTracker)
			{
				var position = actor.transform.position;
				var distance = (bounds.center - position).sqrMagnitude;

				if(distance < minDistance)
				{
					minDistance = distance;
					this.m_ClosestToCenter = actor;
				}

				if(bounds.Contains(position))
				{
					if(!this.m_Actors.Contains(actor))
					{
						this.m_Actors.Add(actor);
						this.onTriggerEnter?.Invoke(actor);
					}
				} else {
					int index = this.m_Actors.IndexOf(actor);

					if(index != -1)
					{
						this.m_Actors.RemoveAt(index);
						this.onTriggerExit?.Invoke(actor);
					}
				}
			}
		}
	}
}
