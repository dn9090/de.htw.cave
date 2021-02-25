using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Htw.Cave.Kinect
{
	internal class LongestTimeTrackedComparer : IComparer<KinectActor>
	{
		public int Compare(KinectActor x, KinectActor y) => x.createdAt > y.createdAt ? 1 : -1;
	}

	internal class TrackingAreaDistanceComparer : IComparer<KinectActor>
	{
		private KinectTrackingArea m_TrackingArea;

		public TrackingAreaDistanceComparer(KinectTrackingArea trackingArea)
		{
			this.m_TrackingArea = trackingArea;
		}

		public int Compare(KinectActor x, KinectActor y)
		{
			var distanceX = (this.m_TrackingArea.volume.center - x.bounds.center).sqrMagnitude;
			var distanceY = (this.m_TrackingArea.volume.center - y.bounds.center).sqrMagnitude;
			
			return distanceX > distanceY ? -1 : 1;
		}
	}

	[Serializable]
	public enum KinectActorSelectionType
	{
		LongestTracked,
		ClosestToSensor,
		ClosestToMid
	}

	[AddComponentMenu("Htw.Cave/Kinect/Kinect Tracking Area")]
	public sealed class KinectTrackingArea : MonoBehaviour
	{
		public event Action<KinectActor> onActorChanged;

		public KinectActor actor => this.m_Actor;

		public KinectActorSelectionType selectionType
		{
			get => this.m_SelectionType;
			set => SetComparer(value);
		}

		public KinectActorTracker actorTracker;

		public Bounds volume;

		[SerializeField]
		private KinectActorSelectionType m_SelectionType;

		private KinectActor m_Actor;

		private List<KinectActor> m_Actors;

		private IComparer<KinectActor> m_Comparer;
		
		private bool m_ComparerNeedsRegularUpdate;

		public void Awake()
		{
			this.m_Actors = new List<KinectActor>();

			if(actorTracker == null)
				enabled = false;

			onActorChanged += (actor) => { if(actor == null) Debug.Log("Bye bye.."); else Debug.Log("Hiho " + actor.gameObject.name); };
		}

		public void OnEnable()
		{
			this.m_Actors.Clear();
			this.m_Actors.AddRange(this.actorTracker.GetActors());

			actorTracker.onActorCreated += ActorCreated;
			actorTracker.onActorDestroy += ActorDestroy;
		}

		public void OnDisable()
		{
			actorTracker.onActorCreated -= ActorCreated;
			actorTracker.onActorDestroy -= ActorDestroy;
		}

		public void Update()
		{
			var bounds = GetVolumeWorldSpace();

			if(this.m_ComparerNeedsRegularUpdate || !IsTrackedActorInBounds(bounds))
				SearchAndUpdateBestActor(bounds);
		}

		public void Reset()
		{
			this.volume = new Bounds(Vector3.zero, new Vector3(2f, 1f, 2f));
		}

		public Bounds GetVolumeWorldSpace() =>  new Bounds(transform.TransformPoint(this.volume.center), this.volume.size);

		private void ActorCreated(KinectActor actor)
		{
			this.m_Actors.Add(actor);
			SearchAndUpdateBestActor(GetVolumeWorldSpace());
		}

		private void ActorDestroy(KinectActor actor)
		{
			this.m_Actors.Remove(actor);

			if(this.m_Actor == actor)
				SearchAndUpdateBestActor(GetVolumeWorldSpace());
		}

		private void SetComparer(KinectActorSelectionType selectionType)
		{
			if(this.m_SelectionType == selectionType)
				return;

			this.m_SelectionType = selectionType;

			switch(this.m_SelectionType)
			{
				case KinectActorSelectionType.LongestTracked:
					this.m_Comparer = new LongestTimeTrackedComparer();
					this.m_ComparerNeedsRegularUpdate = false;
					break;
				case KinectActorSelectionType.ClosestToMid:
					this.m_Comparer = new TrackingAreaDistanceComparer(this);
					this.m_ComparerNeedsRegularUpdate = true;
					break;
				default:
					Debug.LogError("The selected comparer is not available.");
					break;
			}
		}

		private bool IsTrackedActorInBounds(Bounds bounds) => this.m_Actor != null && bounds.Intersects(this.m_Actor.bounds);

		private void SearchAndUpdateBestActor(Bounds bounds)
		{
			this.m_Actors.Sort(this.m_Comparer);
			
			for(int i = this.m_Actors.Count - 1; i >= 0; --i)
			{
				if(bounds.Intersects(this.m_Actors[i].bounds))
				{
					ChangeBestActor(this.m_Actors[i]);
					return;
				}
			}

			ChangeBestActor(null);
		}

		private void ChangeBestActor(KinectActor actor)
		{
			if(actor != this.m_Actor)
			{
				this.m_Actor = actor;
				this.onActorChanged?.Invoke(this.m_Actor);
			}
		}
	}
}