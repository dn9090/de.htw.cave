using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Htw.Cave.Projectors
{
    /// <summary>
    /// Responsible for starting and updating the containing cameras and
    /// setting up stereoscopic support.
    /// </summary>
    [AddComponentMenu("Htw.Cave/Projectors/Projector Brain")]
    public sealed class ProjectorBrain : MonoBehaviour
    {
        [SerializeField]
        private ProjectorSettings settings;
        public ProjectorSettings Settings
        {
            get => this.settings;
            set => this.settings = value;
        }

        private ProjectorMount mount;
        private ProjectorCamera[] cameras;

        public void Awake()
        {
            this.mount = base.GetComponentInChildren<ProjectorMount>();

            if (this.mount == null)
            {
                base.enabled = false;
                throw new InvalidOperationException("Missing ProjectorMount in children.");
            }

            this.cameras = this.mount.Cameras;

            InitializeScreen();
            InitializeCameras();
            FindAndInitializeEyes();
        }

        public void LateUpdate()
        {
            foreach (ProjectorCamera cam in this.cameras)
                cam.UpdateCameraProjection();
        }

        private void InitializeScreen()
        {
            if (this.settings.ForceFullScreen)
            {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.fullScreen = true;
            }
        }

        private void InitializeCameras()
        {
            if (this.cameras.Where(cam => cam.Configuration == null).Count() > 0)
                throw new InvalidOperationException("Missing " + nameof(ProjectorConfiguration) + " in one or more " + nameof(ProjectorCamera) + " components.");

            int viewports = this.cameras.Max(cam => cam.Configuration.DisplayId) + 1;

            foreach (ProjectorCamera cam in this.cameras)
            {
                cam.FindCamera();

                if (this.settings.CameraTarget == CameraTarget.MultiDisplay)
                    cam.ActivateCameraDisplay();
                else
                    cam.ResizeCameraViewport(viewports);

                cam.SetCameraClipPlanes(this.settings.NearClipPlane, this.settings.FarClipPlane);
                cam.SetCameraStereo(this.settings.StereoConvergence, this.settings.StereoSeparation);
            }
        }

        private void FindAndInitializeEyes()
        {
            ProjectorEyes[] eyes = base.GetComponentsInChildren<ProjectorEyes>();

            foreach (ProjectorEyes eye in eyes)
                eye.SetSeperation(this.settings.StereoSeparation);
        }

        public void UpdateCameraClipPlanes(float nearClipPlane, float farClipPlane)
        {
			this.settings.NearClipPlane += nearClipPlane;
			this.settings.FarClipPlane += farClipPlane;

            foreach (ProjectorCamera cam in this.cameras)
				cam.SetCameraClipPlanes(this.settings.NearClipPlane, this.settings.FarClipPlane);
        }

        public void UpdateCameraStereo(float convergence, float seperation)
        {
			this.settings.StereoConvergence += convergence;
			this.settings.StereoSeparation += seperation;

			/*
			Has no actual effect on the projection. Uncomment if setting camera parameters becomes necessary
            foreach (ProjectorCamera cam in this.cameras)
				cam.SetCameraStereo(this.settings.StereoConvergence, this.settings.StereoSeparation);
			*/

			FindAndInitializeEyes();
        }

		public void ResetCameraClipPlanes()
		{
			this.settings.NearClipPlane = 0.1f;
			this.settings.FarClipPlane = 1000;

			foreach (ProjectorCamera cam in this.cameras)
				cam.SetCameraClipPlanes(this.settings.NearClipPlane, this.settings.FarClipPlane);
		}

		public void ResetCameraStereo()
		{
			this.settings.StereoConvergence = 10;
			this.settings.StereoSeparation = 0.064f;

			/*
			Has no actual effect on the projection. Uncomment if setting camera parameters becomes necessary
            foreach (ProjectorCamera cam in this.cameras)
				cam.SetCameraStereo(this.settings.StereoConvergence, this.settings.StereoSeparation);
			*/

			FindAndInitializeEyes();
		}
    }
}
