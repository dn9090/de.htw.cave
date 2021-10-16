using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Htw.Cave.Kinect;

namespace Htw.Cave
{
	[AddComponentMenu("Htw.Cave/Virtual Environment")]
	public sealed class VirtualEnvironment : MonoBehaviour
	{
		/// <summary>
		/// Returns the current dimensions of the environment.
		/// Use <see cref="Resize"/> to change the dimensions.
		/// </summary>
		public Vector3 dimensions => this.m_Dimensions;

		/// <summary>
		/// The center point of the environment in world space.
		/// </summary>
		public Vector3 center => transform.TransformPoint(Vector3.up * 0.5f * dimensions.y);

		/// <summary>
		/// The bounds or volume of environment in local space
		/// based on the <see cref="dimensions"/>.
		/// </summary>
		public Bounds localBounds => new Bounds(Vector3.up * 0.5f * dimensions.y, dimensions);
		
		/// <summary>
		/// Returns the current camera render output target.
		/// </summary>
		public VirtualOutputTarget outputTarget => this.m_OutputTarget;

		/// <summary>
		/// All screens of the environment.
		/// </summary>
		public VirtualScreen[] screens => this.m_Screens;
		
		/// <summary>
		/// All detected cameras inside the environment.
		/// </summary>
		public VirtualCamera[] cameras => this.m_Cameras;

		/// <summary>
		/// The eyes from which the projection will be calculated.
		/// </summary>
		public VirtualEyes eyes
		{
			get => this.m_Eyes;
			set => this.m_Eyes = value;
		}

		/// <summary>
		/// Defines the eye seperation which is used for the stereo projection.
		/// </summary>
		public float eyeSeperation;
		
		/// <summary>
		/// Near clip plane of the perspective off-center projection.
		/// </summary>
		public float nearClipPlane;
		
		/// <summary>
		/// Far clip plane of the perspective off-center projection.
		/// </summary>
		public float farClipPlane;

		/// <summary>
		/// Lock the cameras to the current position.
		/// If set to <c>true<c/>, the cameras will stop following the eye position.
		/// </summary>
		public bool lockCameras;
				
		[SerializeField]
		private Vector3 m_Dimensions;

		[SerializeField]
		private VirtualOutputTarget m_OutputTarget;
		
		[SerializeField]
		private VirtualEyes m_Eyes;

		private VirtualCamera[] m_Cameras;
				
		private VirtualScreen[] m_Screens;

		public void OnEnable()
		{
			// If the environment is enabled the first time, auto assign
			// the virtual displays, otherwise keep the configuration.
			var keepVirtualDisplays = this.m_Cameras != null;
			this.m_Cameras = GetComponentsInChildren<VirtualCamera>();

			SetOutputTarget(this.m_OutputTarget, keepVirtualDisplays);
			Resize(this.m_Dimensions);
		}
		
		public void LateUpdate()
		{		
			for(int i = 0; i < this.m_Cameras.Length; ++i)
			{
				var screen = GetScreen(this.m_Cameras[i].screenKind);
				var eyePosition = this.lockCameras
					? this.m_Cameras[i].transform.position
					: this.m_Eyes.GetPosition(this.m_Cameras[i].stereoTarget, this.eyeSeperation);
				
				this.m_Cameras[i].UpdateCameraProjection(screen, eyePosition, this.nearClipPlane, this.farClipPlane);
				this.m_Cameras[i].UpdateCameraTransform(screen, eyePosition);
			}
		}
		
		public void Reset()
		{
			this.eyeSeperation = 0.06f;
			this.nearClipPlane = 0.1f;
			this.farClipPlane = 1000f;
			this.lockCameras = false;
			this.m_Dimensions = GetDefaultDimensions();
			this.m_Eyes = GetComponentInChildren<VirtualEyes>();
		}

		/// <summary>
		/// Resizes the environment to the given dimensions.
		/// Rebuilds the screens automatically.
		/// </summary>
		/// <param name="dimensions">The target dimensions.</param>
		public void Resize(Vector3 dimensions)
		{
			this.m_Dimensions = dimensions;
			
			// Do not rebuild screens if an environment is
			// instantiated or created in the editor.
			if(Application.isPlaying)
			{
				if(this.m_Screens == null)
				{
					this.m_Screens = new VirtualScreen[6];
				} else {
					for(int i = 0; i < this.m_Screens.Length; ++i)
						Destroy(this.m_Screens[i].gameObject);
				}
				
				for(int i = 0; i < this.m_Screens.Length; ++i)
					this.m_Screens[i] = VirtualUtility.CreateScreen((VirtualScreen.Kind)i, this.dimensions, transform);
			}
		}

		/// <summary>
		/// Sets <see cref="lockCameras"/> to <c>true</c> and positions the cameras
		/// to the target position.
		/// </summary>
		/// <param name="position">The locked target position.</param>
		public void LockCamerasToPosition(Vector3 position)
		{
			this.lockCameras = true;

			for(int i = 0; i < this.m_Cameras.Length; ++i)
			{
				var screen = GetScreen(this.m_Cameras[i].screenKind);
				this.m_Cameras[i].UpdateCameraTransform(screen, position);
			}
		}

		/// <summary>
		/// Sets the <see cref="VirtualCamera.Calibration.outputTarget"> for every
		/// camera in the environment.
		/// </summary>
		/// <param name="outputTarget">The camera output target.</param>
		public void SetOutputTarget(VirtualOutputTarget outputTarget) => SetOutputTarget(outputTarget, true);

		internal void SetOutputTarget(VirtualOutputTarget outputTarget, bool keepVirtualDisplays)
		{
			float viewportSize = 1f / this.m_Cameras.Length; // Only for split viewports.

			for(int i = 0; i < this.m_Cameras.Length; ++i)
			{
				var calibration = this.m_Cameras[i].GetCalibration();
				calibration.viewportSize = viewportSize;
				calibration.outputTarget = outputTarget;
				calibration.virtualDisplay = keepVirtualDisplays ? calibration.virtualDisplay : i;

				this.m_Cameras[i].ApplyCalibration(calibration);
			}

			this.m_OutputTarget = outputTarget;
		}

		/// <summary>
		/// Returns the <see cref="VirtualScreen"> component instance of
		/// the given <see cref="VirtualScreen.Kind">. 
		/// </summary>
		/// <param name="kind">The screen kind.</param>
		/// <returns>The screen component.</returns>
		public VirtualScreen GetScreen(VirtualScreen.Kind kind) => this.m_Screens[(int)kind];
		
		/// <summary>
		/// Returns if a given point is inside the <see cref="localBounds">.
		/// </summary>
		/// <param name="point">The position of the point.</param>
		/// <returns>If inside the environment <c>true</c> otherwise <c>false</c>.</returns>
		public bool Contains(Vector3 point) => localBounds.Contains(transform.InverseTransformPoint(point));

		/// <summary>
		/// Returns the default dimensions of the HTW CAVE.
		/// </summary>
		/// <returns>The dimensions in meters.</returns>
		public static Vector3 GetDefaultDimensions() => new Vector3(3f, 2.45f, 3f);
	}
}
