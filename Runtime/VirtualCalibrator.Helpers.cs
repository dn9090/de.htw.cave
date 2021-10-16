using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Htw.Cave.SimpleTcp;

namespace Htw.Cave
{
	public partial class VirtualCalibrator
	{
		public Shader helperShader;

		public bool showHelpers
		{
			get => this.m_Helpers != null;
			set
			{
				if(value && this.m_Helpers == null)
				{
					this.m_Helpers = InstantiateHelpers();
				} else if(!value && this.m_Helpers != null) {
					Destroy(this.m_Helpers);
					this.m_Helpers = null;
				}
			}
		}

		private GameObject m_Helpers;

		private Material m_HelperMaterial;

		private Texture2D m_QuadTexture;
		
		private Texture2D m_CubeTexture;

		private int m_RenderSingleCameraDisplay;

		public void Reset()
		{
			// Add unlit standard materials of other render piplines.
			this.helperShader = Shader.Find("Particles/Standard Unlit");
		}

		private void RenderSingleCamera(int display)
		{
			display = display < 0 ? -1 : display;

			if(this.m_RenderSingleCameraDisplay == display)
				return;

			this.m_RenderSingleCameraDisplay = display;

			if(display >= 0)
				StartCoroutine(RenderSingleCamera());
		}

		private IEnumerator RenderSingleCamera()
		{
			var display = this.m_RenderSingleCameraDisplay;

			yield return new WaitForEndOfFrame();
			yield return null;
			
			var cameras = this.m_Environment.cameras;
			var cullingMasks = new int[cameras.Length];
			var clearFlags = new CameraClearFlags[cameras.Length];
			var colors = new Color[cameras.Length];

			for(int i = 0; i < cameras.Length; ++i)
			{
				cullingMasks[i] = cameras[i].renderCamera.cullingMask;
				clearFlags[i]   = cameras[i].renderCamera.clearFlags;
				colors[i]       = cameras[i].renderCamera.backgroundColor;

				if(cameras[i].GetCalibration().virtualDisplay == display)
					continue;

				cameras[i].renderCamera.cullingMask = 0;
				cameras[i].renderCamera.clearFlags = CameraClearFlags.SolidColor;
				cameras[i].renderCamera.backgroundColor = Color.black;
			}

			while(display == this.m_RenderSingleCameraDisplay)
				yield return null;
			
			for(int i = 0; i < cameras.Length; ++i)
			{
				cameras[i].renderCamera.cullingMask     = cullingMasks[i];
				cameras[i].renderCamera.clearFlags      = clearFlags[i];
				cameras[i].renderCamera.backgroundColor = colors[i];
			}
		}
		
		private GameObject InstantiateHelpers()
		{
			if(this.m_HelperMaterial == null)
			{
				this.m_QuadTexture = Resources.Load<Texture2D>("CalibrationQuad@256x256");
				this.m_CubeTexture = Resources.Load<Texture2D>("CalibrationCube@256x256");
				this.m_HelperMaterial = new Material(this.helperShader);
			}

			var root = new GameObject("Virtual Calibrator Helpers");
			
			root.transform.parent = this.m_Environment.transform;
			root.transform.localPosition = Vector3.zero;
			root.transform.localRotation = Quaternion.identity;
			
			var screens = this.m_Environment.screens;
			
			var colors = new Color[] {
				Color.blue, Color.red, Color.magenta, Color.green, Color.cyan, Color.yellow
			};
			
			for(int i = 0; i < screens.Length; ++i)
			{
				var color = colors[i % colors.Length];
				
				CreateScreenTextHelper(screens[i], root.transform, color);
				CreateCubeDistanceHelper(screens[i], root.transform, color);
			
				var topLeft = new Vector3(screens[i].width * -0.5f, screens[i].height * 0.5f);
				CreateCornerQuadHelper(screens[i], topLeft, root.transform, color);
				
				var topRight = new Vector3(screens[i].width * 0.5f, screens[i].height * 0.5f);
				CreateCornerQuadHelper(screens[i], topRight, root.transform, color);
				
				var bottomLeft = new Vector3(screens[i].width * -0.5f, screens[i].height * -0.5f);
				CreateCornerQuadHelper(screens[i], bottomLeft, root.transform, color);
				
				var bottomRight = new Vector3(screens[i].width * 0.5f, screens[i].height * -0.5f);
				CreateCornerQuadHelper(screens[i], bottomRight, root.transform, color);
			}
			
			return root;
		}
		
		private void CreateScreenTextHelper(VirtualScreen screen, Transform parent, Color color)
		{
			var text = new GameObject("Virtual Text Helper").AddComponent<TextMesh>();
			text.text = screen.kind.ToString().ToUpper();
			text.color = color;
			text.anchor = TextAnchor.MiddleCenter;
			text.fontSize = 28;
			text.characterSize = 0.04f * screen.width;
			text.transform.parent = parent;
			text.transform.position = screen.transform.TransformPoint(Vector3.up * screen.height * 0.3f + Vector3.forward);
			text.transform.rotation = screen.transform.rotation;
		}
		
		private void CreateCornerQuadHelper(VirtualScreen screen, Vector3 position, Transform parent, Color color)
		{
			var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			quad.name = "Virtual Quad Helper";
			
			quad.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
			
			var renderer = quad.GetComponent<MeshRenderer>();
			renderer.material = this.m_HelperMaterial;
			renderer.material.color = color;
			renderer.material.mainTexture = this.m_QuadTexture;
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			
			var offsetX = Mathf.Sign(position.x) * -0.5f * quad.transform.localScale.x;
			var offsetY = Mathf.Sign(position.y) * -0.5f * quad.transform.localScale.y;
			
			quad.transform.position = screen.transform.TransformPoint(position + new Vector3(offsetX, offsetY, 0f));
			quad.transform.rotation = screen.transform.rotation;
			quad.transform.parent = parent;
		}
		
		private void CreateCubeDistanceHelper(VirtualScreen screen, Transform parent, Color color)
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.name = "Virtual Cube Helper";
			
			cube.transform.localScale = Vector3.one;
			
			var renderer = cube.GetComponent<MeshRenderer>();
			renderer.material = this.m_HelperMaterial;
			renderer.material.color = color;
			renderer.material.mainTexture = this.m_CubeTexture;
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		
			cube.transform.rotation = screen.transform.rotation;
			cube.transform.position = screen.transform.TransformPoint(new Vector3(0f, 0f, 5f));
			cube.transform.parent = parent;
		}
	}
}
