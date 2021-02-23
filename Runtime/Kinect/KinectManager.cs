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
	// @Review: How about running the manager in a separate thread?

	// @Todo: Add frame number (if the threaded thing works).

	public enum FaceFrameFeatureType
	{
		Required,
		Full
	}


	public class KinectReader
	{
		public KinectReader(KinectSensor sensor)
		{
			
		}

	}

	/// <summary>
	/// Provides the required functions to automatically retrieve data
	/// from the Kinect sensor.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Manager")]
	public sealed class KinectManager : MonoBehaviour
	{
		public const float FrameTime = 0.33f;

		public event Action onSensorOpen;

		public event Action onSensorClose;

		public KinectSensor sensor => this.m_Sensor;

		public UnityEngine.Vector4 floor => this.m_Floor.ToUnityVector4();

		public int trackingCapacity => this.m_Bodies == null ? 0 : this.m_Bodies.Length;

		public Vector3 sensorPosition;

		public FaceFrameFeatureType faceFrameFeatureType;

		private KinectSensor m_Sensor;

		private Windows.Kinect.Vector4 m_Floor;

		private MultiSourceFrameReader m_MultiSourceFrameReader;

		private Body[] m_Bodies;

		private int m_TrackedBodyCount;

		private TimeSpan m_TimeStamp;

		private FaceFrameSource[] m_FaceFrameSources;

		private FaceFrameReader[] m_FaceFrameReaders;

		private FaceFrameResult[] m_FaceFrameResults;

		private float m_NextFrameTime;

		public void Start()
		{
			try
			{
				this.m_Sensor = KinectSensor.GetDefault();
			} catch {
#if UNITY_EDITOR
				Debug.LogError("The Kinect v2 SDK was not properly installed.");
#endif
				this.m_Sensor = null;
			}

			if(this.m_Sensor != null)
				OnEnable();
			else
				enabled = false;
		}

		public void OnEnable()
		{
			if(this.m_Sensor != null)
			{
				InitializeBodyReaders();
				InitializeFaceReaders();
				OpenSensor();
				this.onSensorOpen?.Invoke();
			}
		}

		public void OnDisable()
		{
			if(this.m_Sensor != null)
			{
				StopBodyReaders();
				StopFaceReaders();
				CloseSensor();
				this.onSensorClose?.Invoke();
			}
		}

		public void RestartSensor()
		{
			OnDisable();
			OnEnable();
		}

		public TimeSpan AcquireFrames(out Body[] bodies, out FaceFrameResult[] faceFrames, out int count)
		{
			if(Time.unscaledTime > this.m_NextFrameTime)
			{
				AcquireBodyFrames();
				AcquireFaceFrames();

				this.m_NextFrameTime = Time.unscaledTime + FrameTime;
			}

			bodies = this.m_Bodies;
			faceFrames = this.m_FaceFrameResults;
			count = this.m_TrackedBodyCount;

			return this.m_TimeStamp;
		}

		private void OpenSensor()
		{
			if(!this.m_Sensor.IsOpen)
				this.m_Sensor.Open();
		}

		private void CloseSensor()
		{
			if(this.m_Sensor.IsOpen)
				this.m_Sensor.Close();

			this.m_Sensor = null;
		}

		private void InitializeBodyReaders()
		{
			this.m_MultiSourceFrameReader = this.m_Sensor.OpenMultiSourceFrameReader(
				FrameSourceTypes.Body |
				FrameSourceTypes.BodyIndex |
				FrameSourceTypes.Depth);
			this.m_Bodies = new Body[this.m_Sensor.BodyFrameSource.BodyCount];
		}

		private void InitializeFaceReaders()
		{
			this.m_FaceFrameResults = new FaceFrameResult[this.m_Sensor.BodyFrameSource.BodyCount];
			this.m_FaceFrameSources = new FaceFrameSource[this.m_Sensor.BodyFrameSource.BodyCount];
			this.m_FaceFrameReaders = new FaceFrameReader[this.m_Sensor.BodyFrameSource.BodyCount];

			FaceFrameFeatures faceFrameFeatures = faceFrameFeatureType == FaceFrameFeatureType.Required
				? RequiredFaceFrameFeatures()
				: FullFaceFrameFeatures();

			for(int i = 0; i < this.m_FaceFrameSources.Length; ++i)
			{
				this.m_FaceFrameSources[i] = FaceFrameSource.Create(this.m_Sensor, 0, faceFrameFeatures);
				this.m_FaceFrameReaders[i] = this.m_FaceFrameSources[i].OpenReader();
			}
		}

		private void AcquireBodyFrames()
		{
			if(this.m_MultiSourceFrameReader == null)
				return;

			MultiSourceFrame multiFrame = this.m_MultiSourceFrameReader.AcquireLatestFrame();

			if(multiFrame == null)
				return;

			using(BodyFrame bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
			{
				if(bodyFrame == null)
					return;

				// @Optimize: Read the book and look for additional info on the
				// guarantees of GetAndRefreshBodyData. Maybe it is possible
				// to skip the sorting completely.

				bodyFrame.GetAndRefreshBodyData(this.m_Bodies);

				this.m_TrackedBodyCount = BodyHelper.SortAndCount(this.m_Bodies);
				this.m_TimeStamp = bodyFrame.RelativeTime;
				this.m_Floor = bodyFrame.FloorClipPlane;
			}

			// In the documentation the MultiSourceFrame implements IDisposable
			// but this is not true for the provided scripts. Instead the finalizer
			// needs to be called to cleanup the resources.
			multiFrame = null; 
		}

		private void AcquireFaceFrames()
		{
			if(this.m_Bodies == null)
				return;

			for(int i = 0; i < this.m_TrackedBodyCount; ++i)
			{
				this.m_FaceFrameSources[i].TrackingId = this.m_Bodies[i].TrackingId;

				using(FaceFrame faceFrame = this.m_FaceFrameReaders[i].AcquireLatestFrame())
				{
					if(faceFrame == null)
						continue;

					if(faceFrame.FaceFrameResult != null)
						this.m_FaceFrameResults[i] = faceFrame.FaceFrameResult;
				}
			}
		}

		private void StopBodyReaders()
		{
			if(this.m_MultiSourceFrameReader != null)
			{
				this.m_MultiSourceFrameReader.Dispose();
				this.m_MultiSourceFrameReader = null;
			}
		}

		private void StopFaceReaders()
		{
			for(int i = 0; i < this.m_FaceFrameSources.Length; ++i)
			{
				if(this.m_FaceFrameReaders[i] != null)
				{
					this.m_FaceFrameReaders[i].Dispose();
					this.m_FaceFrameReaders[i] = null;
				}

				if(this.m_FaceFrameSources[i] != null)
					this.m_FaceFrameSources[i] = null;
			}
		}

		private static FaceFrameFeatures RequiredFaceFrameFeatures() =>
			FaceFrameFeatures.BoundingBoxInColorSpace |
			FaceFrameFeatures.PointsInColorSpace |
			FaceFrameFeatures.BoundingBoxInInfraredSpace |
			FaceFrameFeatures.PointsInInfraredSpace |
			FaceFrameFeatures.RotationOrientation |
			FaceFrameFeatures.Glasses |
			FaceFrameFeatures.LookingAway;

		private static FaceFrameFeatures FullFaceFrameFeatures() =>
			FaceFrameFeatures.BoundingBoxInColorSpace |
			FaceFrameFeatures.PointsInColorSpace |
			FaceFrameFeatures.BoundingBoxInInfraredSpace |
			FaceFrameFeatures.PointsInInfraredSpace |
			FaceFrameFeatures.RotationOrientation |
			FaceFrameFeatures.FaceEngagement |
			FaceFrameFeatures.Glasses |
			FaceFrameFeatures.Happy |
			FaceFrameFeatures.LeftEyeClosed |
			FaceFrameFeatures.RightEyeClosed |
			FaceFrameFeatures.LookingAway |
			FaceFrameFeatures.MouthMoved |
			FaceFrameFeatures.MouthOpen;
	}
}
