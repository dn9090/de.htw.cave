using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Htw.Cave.Projectors
{
	[Serializable]
	public enum CameraTarget
	{
		MultiDisplay,
		SplitViewport
	}

	/// <summary>
	/// Defines the render target of a group of stereoscopic cameras
	/// and shared render settings.
	/// </summary>
	[CreateAssetMenu(fileName = "New Projector Settings", menuName = "Htw.Cave/Projector Settings", order = 20)]
	public class ProjectorSettings : ScriptableObject
	{
		[SerializeField]
		private CameraTarget cameraTarget;
		public CameraTarget CameraTarget
		{
			get => this.cameraTarget;
			set => this.cameraTarget = value;
		}

		[SerializeField]
		private bool forceFullScreen;
		public bool ForceFullScreen
		{
			get => this.forceFullScreen;
			set => this.forceFullScreen = value;
		}

		[SerializeField]
		private float stereoSeparation;
		public float StereoSeparation
		{
			get => this.stereoSeparation;
			set => this.stereoSeparation = value;
		}

		[SerializeField]
		private float stereoConvergence;
		public float StereoConvergence
		{
			get => this.stereoConvergence;
			set => this.stereoConvergence = value;
		}

		[SerializeField]
		private float nearClipPlane;
		public float NearClipPlane
		{
			get => this.nearClipPlane;
			set => this.nearClipPlane = value;
		}

		[SerializeField]
		private float farClipPlane;
		public float FarClipPlane
		{
			get => this.farClipPlane;
			set => this.farClipPlane = value;
		}

		[SerializeField]
		private string configurationPath;
		public string ConfigurationPath
		{
			get => this.configurationPath;
			set => this.configurationPath = value;
		}
	}
}
