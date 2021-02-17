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
	/// Provides the required functions to automatically retrieve data
	/// from the Kinect sensor.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Manager")]
	public sealed class KinectManager : MonoBehaviour
	{
		public KinectSensor Sensor { get; private set; }
		public Windows.Kinect.Vector4 Floor { get; private set; }
		public Body[] Bodies { get; private set; }
		public FaceFrameResult[] FaceFrameResults { get; private set; }

		private MultiSourceFrameReader multiSourceFrameReader;
		private FaceFrameSource[] faceFrameSources;
		private FaceFrameReader[] faceFrameReaders;

		public static FaceFrameFeatures FullFaceFrameFeatures()
		{
			FaceFrameFeatures faceFrameFeatures =
				FaceFrameFeatures.BoundingBoxInColorSpace
				| FaceFrameFeatures.PointsInColorSpace
				| FaceFrameFeatures.BoundingBoxInInfraredSpace
				| FaceFrameFeatures.PointsInInfraredSpace
				| FaceFrameFeatures.RotationOrientation
				| FaceFrameFeatures.FaceEngagement
				| FaceFrameFeatures.Glasses
				| FaceFrameFeatures.Happy
				| FaceFrameFeatures.LeftEyeClosed
				| FaceFrameFeatures.RightEyeClosed
				| FaceFrameFeatures.LookingAway
				| FaceFrameFeatures.MouthMoved
				| FaceFrameFeatures.MouthOpen;

			return faceFrameFeatures;
		}

		public static FaceFrameFeatures RequiredFaceFrameFeatures()
		{
			FaceFrameFeatures faceFrameFeatures =
				FaceFrameFeatures.BoundingBoxInColorSpace
				| FaceFrameFeatures.PointsInColorSpace
				| FaceFrameFeatures.BoundingBoxInInfraredSpace
				| FaceFrameFeatures.PointsInInfraredSpace
				| FaceFrameFeatures.RotationOrientation
				| FaceFrameFeatures.Glasses
				| FaceFrameFeatures.LookingAway;

			return faceFrameFeatures;
		}

		public void Awake()
		{
			this.Sensor = KinectSensor.GetDefault();
		}

		public void OnEnable()
		{
			InitializeBodyReaders();
			InitializeFaceReaders();
			OpenSensor();
		}

		public void OnDisable()
		{
			StopBodyReaders();
			StopFaceReaders();
			CloseSensor();
		}

		public void AcquireFrames()
		{
			AcquireBodyFrames();
			AcquireFaceFrames();
		}

		private void OpenSensor()
		{
			if(!this.Sensor.IsOpen)
				this.Sensor.Open();
		}

		private void CloseSensor()
		{
			if(this.Sensor.IsOpen)
				this.Sensor.Close();

			this.Sensor = null;
		}

		private void InitializeBodyReaders()
		{
			this.multiSourceFrameReader = this.Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.BodyIndex | FrameSourceTypes.Depth);
			this.Bodies = new Body[this.Sensor.BodyFrameSource.BodyCount];
		}

		private void InitializeFaceReaders()
		{
			this.FaceFrameResults = new FaceFrameResult[this.Sensor.BodyFrameSource.BodyCount];
			this.faceFrameSources = new FaceFrameSource[this.Sensor.BodyFrameSource.BodyCount];
			this.faceFrameReaders = new FaceFrameReader[this.Sensor.BodyFrameSource.BodyCount];

			FaceFrameFeatures faceFrameFeatures = RequiredFaceFrameFeatures();

			for(int i = this.faceFrameSources.Length - 1; i >= 0; --i)
			{
				this.faceFrameSources[i] = FaceFrameSource.Create(this.Sensor, 0, faceFrameFeatures);
				this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
			}
		}

		private void AcquireBodyFrames()
		{
			if(this.multiSourceFrameReader == null)
				return;

			MultiSourceFrame multiFrame = this.multiSourceFrameReader.AcquireLatestFrame();

			if(multiFrame == null)
				return;

			using(BodyFrame bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
			{
				if(bodyFrame == null)
					return;

				bodyFrame.GetAndRefreshBodyData(this.Bodies);
				this.Floor = bodyFrame.FloorClipPlane;
			}

			multiFrame = null;
		}

		private void AcquireFaceFrames()
		{
			if(this.Bodies == null)
				return;

			for (int i = this.Sensor.BodyFrameSource.BodyCount - 1; i >= 0; --i)
			{
				if(!this.faceFrameSources[i].IsTrackingIdValid)
				{
					if (this.Bodies[i] != null && this.Bodies[i].IsTracked)
						this.faceFrameSources[i].TrackingId = this.Bodies[i].TrackingId;

					continue;
				}

				using(FaceFrame faceFrame = this.faceFrameReaders[i].AcquireLatestFrame())
				{
					if(faceFrame == null)
						continue;

					if(faceFrame.FaceFrameResult != null)
						this.FaceFrameResults[i] = faceFrame.FaceFrameResult;
				}
			}
		}

		private void StopBodyReaders()
		{
			if(this.multiSourceFrameReader != null)
			{
				this.multiSourceFrameReader.Dispose();
				this.multiSourceFrameReader = null;
			}
		}

		private void StopFaceReaders()
		{
			for(int i = this.faceFrameSources.Length - 1; i >= 0; --i)
			{
				if (this.faceFrameReaders[i] != null)
				{
					this.faceFrameReaders[i].Dispose();
					this.faceFrameReaders[i] = null;
				}

				if (this.faceFrameSources[i] != null)
					this.faceFrameSources[i] = null;
			}
		}
	}
}
