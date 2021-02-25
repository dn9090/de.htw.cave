using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	// @Readme: For disconnecting issues: https://social.msdn.microsoft.com/Forums/sqlserver/en-US/bcd775ef-64b0-4e94-8d26-ce297d6d60ea/kinect-v2-keeps-disconnecting-after-every-10-seconds-on-windows-10-1903?forum=kinectv2sdk
	// If the Kinect v2 is not recognized: https://skarredghost.com/2018/03/01/fix-kinect-v2-not-working-windows-10-creators-update/

	public enum FaceFrameFeatureType
	{
		Required,
		Full
	}

	/// <summary>
	/// Provides the required functions to automatically retrieve data
	/// from the <see cref="KinectSensor"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Manager")]
	public sealed class KinectManager : MonoBehaviour
	{
		private const float FrameTime = 1f / 30f;

		public event Action onSensorOpen;

		public event Action onSensorClose;

		/// <summary>
		/// Gets the connected <see cref="KinectSensor"/>.
		/// Can be <c>null</c> if no <see cref="KinectSensor"/> is found.
		/// </summary>
		public KinectSensor sensor => this.m_Sensor;

		/// <summary>
		/// The floor vector converted to <see cref="UnityEngine.Vector4"/>.
		/// </summary>
		public UnityEngine.Vector4 floor => this.m_Floor.ToUnityVector4();

		/// <summary>
		/// The maximum number of <see cref="Body"/> instances the system can track.
		/// </summary>
		public int trackingCapacity => this.m_Bodies == null ? 0 : this.m_Bodies.Length;

		/// <summary>
		/// Defines the feature set of the <see cref="FaceFrameSource"/>.
		/// </summary>
		public FaceFrameFeatureType faceFrameFeatureType;

		/// <summary>
		/// Calculates the tilt of the <see cref="KinectSensor"/> based on
		/// the <see cref="floor"/>.
		/// </summary>
		public float tilt => Mathf.Atan(-floor.z / floor.y) * (180.0f / Mathf.PI);

		private KinectSensor m_Sensor;

		private Windows.Kinect.Vector4 m_Floor;

		private MultiSourceFrameReader m_MultiSourceFrameReader;

		private Body[] m_Bodies;

		private TimeSpan m_RelativeTime;

		private FaceFrameSource[] m_FaceFrameSources;

		private FaceFrameReader[] m_FaceFrameReaders;

		private FaceFrameResult[] m_FaceFrameResults;

		private Stopwatch m_Stopwatch;

		private long m_Frame;

		private int m_TrackedBodyCount;

		public void Start()
		{
			this.m_Floor = new Windows.Kinect.Vector4 { X = 1, Y = 1, Z = 1, W = 1};
			this.m_Stopwatch = new Stopwatch();
			
			try
			{
				this.m_Sensor = KinectSensor.GetDefault();
			} catch {
#if UNITY_EDITOR
				UnityEngine.Debug.LogError("The Kinect v2 SDK was not properly installed.");
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

		public long AcquireFrames(out Body[] bodies, out FaceFrameResult[] faceFrames, out int bodyCount)
		{
			if(this.m_Stopwatch.ElapsedMilliseconds > FrameTime)
			{
				AcquireBodyFrames();
				AcquireFaceFrames();
				
				this.m_Stopwatch.Restart();
			}

			bodies = this.m_Bodies;
			faceFrames = this.m_FaceFrameResults;
			bodyCount = this.m_TrackedBodyCount;

			return this.m_Frame;
		}

		public long ForceAcquireFrames(out Body[] bodies, out FaceFrameResult[] faceFrames, out int bodyCount)
		{
			AcquireBodyFrames();
			AcquireFaceFrames();
				
			this.m_Stopwatch.Restart();

			bodies = this.m_Bodies;
			faceFrames = this.m_FaceFrameResults;
			bodyCount = this.m_TrackedBodyCount;

			return this.m_Frame;
		}

		private void OpenSensor()
		{
			if(!this.m_Sensor.IsOpen)
				this.m_Sensor.Open();
			this.m_Stopwatch.Start();
		}

		private void CloseSensor()
		{
			if(this.m_Sensor.IsOpen)
				this.m_Sensor.Close();

			this.m_Stopwatch.Stop();
			this.m_Sensor = null;
		}

		private void InitializeBodyReaders()
		{
			// If more sources are needed add them with:
			// this.m_Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.BodyIndex | FrameSourceTypes.Depth);

			this.m_MultiSourceFrameReader = this.m_Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
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
				this.m_RelativeTime = bodyFrame.RelativeTime;
				this.m_Floor = bodyFrame.FloorClipPlane;
				this.m_Frame += 1;
			}

			// In the documentation the MultiSourceFrame implements IDisposable
			// but this is not true for the provided scripts. Instead the finalizer
			// needs to be called to cleanup the resources.
			multiFrame = null; 
		}

		private void AcquireFaceFrames()
		{
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
