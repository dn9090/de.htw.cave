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
	/// Responsible for finding and updating actors in coordination
	/// with the manager.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Play Area")]
	[RequireComponent(typeof(KinectManager))]
	public class KinectPlayArea : MonoBehaviour
	{
		private const int FramesPerSecond = 30;

		private static KinectPlayArea instance;

		public static KinectManager Manager => instance.manager;
		public static KinectActor Actor => instance.actor;

		[SerializeField]
		private KinectSettings settings;
		public KinectSettings Settings
		{
			get => this.settings;
			set => this.settings = value;
		}

#if UNITY_EDITOR
		[SerializeField]
		private bool enableInEditor;
		public bool EnableInEditor
		{
			get => this.enableInEditor;
			set => this.enableInEditor = value;
		}
#endif

		private KinectManager manager;
		private KinectActor actor;
		private Rect area;
		private float simulatedDeltaTime;
		private float fixedTimeStep;

		public void Awake()
		{
			if(instance != null && instance != this)
				Destroy(this);

			instance = this;

			this.manager = base.GetComponent<KinectManager>();
			this.actor = new KinectActor(this.settings.SensorLocation);
			this.area = this.settings.TrackingArea;

#if UNITY_EDITOR
			this.manager.enabled = this.enableInEditor;
			base.enabled = this.enableInEditor;
#endif

			this.simulatedDeltaTime = 0f;
			this.fixedTimeStep = 1f / (float)FramesPerSecond;
		}

		public void OnEnable()
		{
			this.manager.enabled = true;
		}

		public void LateUpdate()
		{
			this.simulatedDeltaTime += Time.deltaTime;

			if(this.simulatedDeltaTime > this.fixedTimeStep)
			{
				this.manager.AcquireFrames();

				KeepTrackOfActor();

				if(!this.actor.IsTracked)
					FindActorInPlayArea();

				this.simulatedDeltaTime = 0f;
			}
		}

		public void OnDisable()
		{
			this.manager.enabled = false;
		}

		public void Reset()
		{
#if UNITY_EDITOR
			this.enableInEditor = false;
#endif
		}

		public bool Contains(Vector3 position)
		{
			return this.area.Contains(new Vector2(position.x, position.z));
		}

		public Vector3[] GetWorldCoordinates()
		{
#if UNITY_EDITOR
			this.area = this.settings.TrackingArea;
#endif
			return new Vector3[]{
				transform.TransformPoint(new Vector3(this.area.min.x, 0f, this.area.min.y)),
				transform.TransformPoint(new Vector3(this.area.max.x, 0f, this.area.min.y)),
				transform.TransformPoint(new Vector3(this.area.max.x, 0f, this.area.max.y)),
				transform.TransformPoint(new Vector3(this.area.min.x, 0f, this.area.max.y))
			};
		}

		private void KeepTrackOfActor()
		{
			Body[] bodies = this.manager.Bodies;
			FaceFrameResult[] faces = this.manager.FaceFrameResults;

			this.actor.IsTracked = false;

			for(int i = 0; i < bodies.Length; ++i)
			{
				if(bodies[i].TrackingId == this.actor.TrackingId)
				{
					this.actor.Update(this.manager.Floor, bodies[i], faces[i]);
					break;
				}
			}
		}

		private void FindActorInPlayArea()
		{
			Body[] bodies = this.manager.Bodies;
			FaceFrameResult[] faces = this.manager.FaceFrameResults;

			for(int i = 0; i < bodies.Length; ++i)
			{
				if(!bodies[i].IsTracked)
					continue;

				this.actor.Update(this.manager.Floor, bodies[i], faces[i]);

				if(Contains(this.actor.GetEstimatedBodyPosition()))
					break;
			}
		}
	}
}
