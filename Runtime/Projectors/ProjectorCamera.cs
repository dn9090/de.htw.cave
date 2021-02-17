using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Globalization;

namespace Htw.Cave.Projectors
{
	/// <summary>
	/// Renders a stereoscopic image with a specialized projection matrix based
	/// on a projection plane.
	/// </summary>
	//[AddComponentMenu("Htw.Cave/Projectors/Projector Camera")]
	[RequireComponent(typeof(Camera))]
	public sealed class ProjectorCamera : MonoBehaviour
	{
		[SerializeField]
		private ProjectorConfiguration configuration;
		public ProjectorConfiguration Configuration
		{
			get => this.configuration;
			set => this.configuration = value;
		}

		[SerializeField]
		private ProjectorPlane plane;
		public ProjectorPlane Plane
		{
			get => this.plane;
			set => this.plane = value;
		}

		private Camera cam;
		public Camera Camera
		{
			get => this.cam;
		}

		private ProjectorMount mount;
		private Vector3[] planePoints;
		private Vector3 vr, vu, vn;

		private Quaternion previousRotation;

		public void Awake()
		{
			if (this.configuration == null)
				throw new UnityException("Missing " + nameof(ProjectorConfiguration) + " in " + nameof(ProjectorCamera) + " component.");

			if (this.cam == null)
				FindCamera();

			this.mount = base.GetComponentInParent<ProjectorMount>();
			this.vr = this.vu = this.vn = Vector3.zero;
		}

		private Matrix4x4 viewMat;
		private Matrix4x4 bimberMat;


		public void Start()
		{
			// New: initial view in different directions => UpdateCameraProjection()
			viewMat = this.cam.worldToCameraMatrix;

			configuration.BottomLeft = new Vector2(-1f, -1f); // setting outside using...
			configuration.BottomRight = new Vector2(1f, -1f);
			configuration.TopLeft = new Vector2(-1f, 1f);
			configuration.TopRight = new Vector2(1f, 1f);
			UpdateBimberMatrix();

			// restore
			using (StreamReader inputFile = new StreamReader(Path.Combine(GameObject.FindGameObjectsWithTag("CAVE")[0].GetComponent<ProjectorBrain>().Settings.ConfigurationPath, 
																		  configuration.DisplayName + ".txt")))
			{
				string line;

				if ((line = inputFile.ReadLine()) != null)
				{
					// Debug.Log(configuration.DisplayName + line);
					string pattern = ":";
					string[] elements = System.Text.RegularExpressions.Regex.Split(line, pattern);
					configuration.BottomLeft = new Vector2(float.Parse(elements[0]), float.Parse(elements[1]));
					configuration.BottomRight = new Vector2(float.Parse(elements[2]), float.Parse(elements[3]));
					configuration.TopLeft = new Vector2(float.Parse(elements[4]), float.Parse(elements[5]));
					configuration.TopRight = new Vector2(float.Parse(elements[6]), float.Parse(elements[7]));
					UpdateBimberMatrix();
				}
				else
				{
					Debug.Log("error while reading calibration file: " + configuration.DisplayName + ".txt");
				}
			}
		}

		public void SaveCalibration()
		{
			using (StreamWriter outputFile = new StreamWriter(Path.Combine(GameObject.FindGameObjectsWithTag("CAVE")[0].GetComponent<ProjectorBrain>().Settings.ConfigurationPath, configuration.DisplayName + ".txt")))
			{
				outputFile.Write("{0:0.000}", configuration.BottomLeft.x); outputFile.Write(":");
				outputFile.Write("{0:0.000}", configuration.BottomLeft.y); outputFile.Write(":");
				outputFile.Write("{0:0.000}", configuration.BottomRight.x); outputFile.Write(":");
				outputFile.Write("{0:0.000}", configuration.BottomRight.y); outputFile.Write(":");
				outputFile.Write("{0:0.000}", configuration.TopLeft.x); outputFile.Write(":");
				outputFile.Write("{0:0.000}", configuration.TopLeft.y); outputFile.Write(":");
				outputFile.Write("{0:0.000}", configuration.TopRight.x); outputFile.Write(":");
				outputFile.Write("{0:0.000}", configuration.TopRight.y);
			}
		}

		public void FindCamera()
		{
			this.cam = base.GetComponent<Camera>();

			if (this.cam == null)
				this.cam = base.GetComponentInChildren<Camera>();
		}

		public void ActivateCameraDisplay()
		{
			this.cam.rect = new Rect(0f, 0f, 1f, 1f);

#if UNITY_EDITOR
			this.cam.targetDisplay = this.configuration.DisplayId;
#else
			if(Display.displays.Length > this.configuration.DisplayId)
			{
				Display display = Display.displays[this.configuration.DisplayId];
				display.Activate();
				this.cam.targetDisplay = this.configuration.DisplayId;
			}
#endif
		}

		public void ResizeCameraViewport(int viewports)
		{
			float size = 1f / viewports;

			this.cam.targetDisplay = 0;
			this.cam.rect = new Rect(this.configuration.DisplayId * size, 0f, size, 1f);
		}

		public void SetCameraClipPlanes(float near, float far)
		{
			this.cam.nearClipPlane = near;
			this.cam.farClipPlane = far;
		}

		public void SetCameraStereo(float convergence, float separation)
		{
			this.cam.stereoConvergence = convergence;
			this.cam.stereoSeparation = separation;
		}


		// inspired by http://csharphelper.com/blog/2014/10/solve-a-system-of-equations-with-gaussian-elimination-in-c/
		private bool solve(float[][] arr)
		{
			// Solving m * x = c;  arr must have two extra columns for vector c and for result, 
			//	| A1  B1...  N1 C1 0 |
			//  | A2  B2...  N2 C2 0 |
			//  |      ...         0 |
			//  | Am  Bm...  Nm Cm 0 |

			int num_rows = arr.Length;
			int num_cols = num_rows; // quadratic
			const float tiny = 0.00001f;

			// Start solving.
			for (int r = 0; r < num_rows - 1; r++)
			{
				// Zero out all entries in column r after this row.
				// See if this row has a non-zero entry in column r.
				if (Math.Abs(arr[r][r]) < tiny)
				{
					// Too close to zero. Try to swap with a later row.
					for (int r2 = r + 1; r2 < num_rows; r2++)
					{
						if (Math.Abs(arr[r2][r]) > tiny)
						{
							// This row will work. Swap them.
							for (int c = 0; c <= num_cols; c++)
							{
								float tmp = arr[r][c];
								arr[r][c] = arr[r2][c];
								arr[r2][c] = tmp;
							}
							break;
						}
					}
				}

				// If this row has a non-zero entry in column r, use it.
				if (Math.Abs(arr[r][r]) > tiny)
				{
					// Zero out this column in later rows.
					for (int r2 = r + 1; r2 < num_rows; r2++)
					{
						float factor = -arr[r2][r] / arr[r][r];
						for (int c = r; c <= num_cols; c++)
						{
							arr[r2][c] = arr[r2][c] + factor * arr[r][c];
						}
					}
				}
			}

			// Display the upper-triangular array.

			// See if we have a solution.
			if (arr[num_rows - 1][num_cols - 1] == 0) // TJ Test auf Gleichheit mit 0, geht das in C# so ?
 			{
				// We have no solution.
				// See if all of the entries in this row are 0.
				bool all_zeros = true;
				for (int c = 0; c <= num_cols + 1; c++)
				{
					if (arr[num_rows - 1][c] != 0)
					{
						all_zeros = false;
						break;
					}
				}
				if (all_zeros)
				{
					Debug.Log("The solution is not unique");
				}
				else
				{
					Debug.Log("There is no solution");
				}

				return false;
			}
			else
			{
				// Backsolve.
				for (int r = num_rows - 1; r >= 0; r--)
				{
					float tmp = arr[r][num_cols];
					for (int r2 = r + 1; r2 < num_rows; r2++)
					{
						tmp -= arr[r][r2] * arr[r2][num_cols + 1];
					}
					arr[r][num_cols + 1] = tmp / arr[r][r];
				}

				return true;
			}
		}

		// Mapping: p00 to left bottom, p01 to left top, p11 to right top, p10 to right bottom 
		private Matrix4x4 genBimberMatrix(Vector3 p00, Vector3 p01, Vector3 p11, Vector3 p10)
		{
			//     Matrix A4x4   from Bimber/Raskar "Spatial Augmented Reality", A.K. Peter 2004, pp. 116
			//                                 (-1,1) *-------------------------* (1,1)      -1            p00       p10      1
			//                                         \     p01      p11      /                 *-----------*---------*-----*
			//    +----------------                     \    *----------*     /                     \         \       /     |
			//    | H11 H12  0  H13                      \   |          |    /                         \       \     /     |
			//    | H21 H22  0  H23                       \  *----------*   /                             \     \   /     |
			//    |  0   0   d   0                         \ p00      p10  /                                 \   \ /     |
			//    | H31 H32  0  H33                 (-1,-1) *-------------* (1,-1)                              \ *Eye  |
			//                                                                                                     \   |
			// with H = homography matrix                                                                Projector  *
			// scaling of depth buffer with d = 1-|H31|-|H32|

			// Homography: see also: https://math.stackexchange.com/questions/494238/how-to-compute-homography-matrix-h-from-corresponding-points-2d-2d-planar-homog
			// | w00*p00.x w01*p01.x w11*p11.x w10*p10.x |   | H11 H12 H13 |   | v00*a00.x v01*a01.x v11*a11.x v10*a10.x |
			// | w00*p00.y w01*p01.y w11*p11.y w10*p10.y | = | H21 H22 H23 | * | v00*a00.y v01*a01.y v11*a11.y v10*a10.y |
			// |   w00       w01       w11       w10     |   | H31 H32 H33 |   |   v00       v01       v11       v10     |
			//
			// setting v00 ... v10 to one, a00 to (-1,-1), a01 to (-1,1), a11 to (1,1), a10 to (1,-1), vertices of normalized device coordinate system
			//
			// | w00*p00.x w01*p01.x w11*p11.x w10*p10.x |   | H11 H12 H13 |   | -1 -1  1  1 |
			// | w00*p00.y w01*p01.y w11*p11.y w10*p10.y | = | H21 H22 H23 | * | -1  1  1 -1 |
			// |   w00       w01       w11       w10     |   | H31 H32 H33 |   |  1  1  1  1 |  w set to 1

			// p00.x = w00*p00.x / w00 = (H11 * -1 + H12 * -1 * H13 * 1) / (H31 * -1 + H32 * -1 + H33 * 1) 
			// p00.y = w00*p00.y / w00 = (H21 * -1 + H22 * -1 * H23 * 1) / (H31 * -1 + H32 * -1 + H33 * 1) 
			// p01 ... p10 analog
			//  | * (H31 * -1 + H32 * -1 + H33 * 1),  - (H11 * -1 + H12 * -1 * H13 * 1)
			// => (H31 * -1 * p00.x + H32 * -1 * p00.x + H33 * 1 * p00.x) + (H11 * 1 + H12 * 1 + H13 * -1) = 0
			// other 7 equations analog
			// => (H11 H12 H13 ... H33) * (1 1 -1 0 0 0 -p00.x -p00.x p00.x) = 0
			// other 7 equations analog
			// => M(8,9) * H(9) = 0(8), one Hi,j undetermined, setting H33 to one
			// => M(8,8) * H(8) = - LastColumn of M
			// | m11 m12 m13 |   | x1 |    | 0 |             | m11 m12 |   | x1 |   | m13 |   | 0 |
			// | m21 m22 m23 | * | x2 | =  | 0 | , x3 = 1 => | m21 m22 | * | x2 | + | m23 | = | 0 |
			//                   | x3 | 

			//p00 = new Vector2(-1f, -1f); p01 = new Vector2(-1f, 1f); p11 = new Vector2(1f, 1f); p10 = new Vector2(1f, -1f);

			//test();

			// creates a matrix initialized to all 0.0s
			int cols = 10;
			int rows = 8;
			float[][] m = new float[rows][];
			for (int i = 0; i < rows; ++i)
				m[i] = new float[cols]; // auto init to 0.0

			m[0][0] = 1f; m[0][1] = 1f; m[0][2] = -1f; m[0][6] = -p00.x; m[0][7] = -p00.x; // "m[0][8] = p00.x" goes to b, 3 ... 5 zero
			m[1][3] = 1f; m[1][4] = 1f; m[1][5] = -1f; m[1][6] = -p00.y; m[1][7] = -p00.y; // "m[1][8] = p00.y" goes to b, 0 ... 2 zero
			m[2][0] = 1f; m[2][1] = -1f; m[2][2] = -1f; m[2][6] = -p01.x; m[2][7] = p01.x; // "m[2][8] = p01.x" goes to b, 3 ... 5 zero
			m[3][3] = 1f; m[3][4] = -1f; m[3][5] = -1f; m[3][6] = -p01.y; m[3][7] = p01.y; // "m[3][8] = p01.y" goes to b, 0 ... 2 zero
			m[4][0] = -1f; m[4][1] = -1f; m[4][2] = -1f; m[4][6] = p11.x; m[4][7] = p11.x; // "m[4][8] = p11.x" goes to b, 3 ... 5 zero
			m[5][3] = -1f; m[5][4] = -1f; m[5][5] = -1f; m[5][6] = p11.y; m[5][7] = p11.y; // "m[5][8] = p11.y" goes to b, 0 ... 2 zero
			m[6][0] = -1f; m[6][1] = 1f; m[6][2] = -1f; m[6][6] = p10.x; m[6][7] = -p10.x; // "m[6][8] = p10.x" goes to b, 3 ... 5 zero
			m[7][3] = -1f; m[7][4] = 1f; m[7][5] = -1f; m[7][6] = p10.y; m[7][7] = -p10.y; // "m[7][8] = p10.y" goes to b, 0 ... 2 zero

			m[0][8] = -p00.x; m[1][8] = -p00.y; m[2][8] = -p01.x; m[3][8] = -p01.y; m[4][8] = -p11.x; m[5][8] = -p11.y; m[6][8] = -p10.x; m[7][8] = -p10.y;

			if (solve(m))
			{
				//Debug.Log(String.Format("Solution is h =  {0} {1} {2} {3} {4} {5} {6} {7} 1", m[0][9], m[1][9], m[2][9], m[3][9], m[4][9], m[5][9], m[6][9], m[7][9]));
				Matrix4x4 mat = Matrix4x4.zero;

				mat[0, 0] = m[0][9]; mat[0, 1] = m[1][9]; mat[0, 3] = m[2][9];
				mat[1, 0] = m[3][9]; mat[1, 1] = m[4][9]; mat[1, 3] = m[5][9];
				mat[3, 0] = m[6][9]; mat[3, 1] = m[7][9]; mat[3, 3] = 1f;
				mat[2, 2] = 1.0f - Mathf.Abs(mat[3, 0]) - Mathf.Abs(mat[3, 1]);  // to avoid z overflow (see Bimber paper)

				/*
				// Check result:
				float rndz = 0.3f;
				Vector4 bl = new Vector4(-1f, -1f, rndz, 1f);
				Vector4 br = new Vector4(1f, -1f, rndz, 1f);
				Vector4 tl = new Vector4(-1f, 1f, rndz, 1f);
				Vector4 tr = new Vector4(1f, 1f, rndz, 1f);
				Vector4 res = mat * bl; Debug.Log(String.Format("check BL =  {0} {1}", res.x - p00.x, res.y - p00.y));
				res = mat * br; Debug.Log(String.Format("check BR =  {0} {1}", res.x - p10.x, res.y - p10.y));
				res = mat * tl; Debug.Log(String.Format("check TL =  {0} {1}", res.x - p01.x, res.y - p01.y));
				res = mat * tr; Debug.Log(String.Format("check TR =  {0} {1}", res.x - p11.x, res.y - p11.y));
				*/
				return mat;
			}
			else
			{
				return Matrix4x4.identity;
			}

		}

		public void UpdateBimberMatrix()
		{
			Vector3 p00 = new Vector3(configuration.BottomLeft.x, configuration.BottomLeft.y, 1f);
			Vector3 p01 = new Vector3(configuration.TopLeft.x, configuration.TopLeft.y, 1f);
			Vector3 p11 = new Vector3(configuration.TopRight.x, configuration.TopRight.y, 1f);
			Vector3 p10 = new Vector3(configuration.BottomRight.x, configuration.BottomRight.y, 1f);
			bimberMat = genBimberMatrix(p00, p01, p11, p10);
		}

		private Matrix4x4 genProjectionMatrix(Vector3 eye)
		{
			float w = configuration.Width;
			float h = configuration.Height;

			// eye is the physical Eye in the physical CAVE, initial at 0, 1.8, 0 (mono)
			// can be moved with the cursor keys

			// We transform the physical Eye position (in world coordinates) to local plane coordinates
			Vector3 eyeLocal = this.plane.transform.worldToLocalMatrix.MultiplyPoint3x4(eye);

			// Frustum:
			// if we are on the right side (looking at front plane), eyeLocal.x is 1.5, then left has to be -3 and right is 0
			float l = -eyeLocal.x - 0.5f * w;
			float r = -eyeLocal.x + 0.5f * w;
			float b = -eyeLocal.y - 0.5f * h;
			float t = -eyeLocal.y + 0.5f * h;
			float near = -eyeLocal.z; // near has to be positive
			float far = 1000f; // should be set later !!!


			float nc = GameObject.FindGameObjectsWithTag("CAVE")[0].GetComponent<ProjectorBrain>().Settings.NearClipPlane;

			float s = 0.5f; // front plane 2 times nearer, need to see objects inside CAVE

			s = nc;

			//Matrices in Unity are column major, in OpenGL row major;
			// https://docs.unity3d.com/ScriptReference/Camera-projectionMatrix.html
			Matrix4x4 mat = Matrix4x4.zero;
			mat[0, 0] = (2f * near) / (r - l);
			mat[1, 1] = (2f * near) / (t - b);
			mat[0, 2] = (r + l) / (r - l);
			mat[1, 2] = (t + b) / (t - b);
			mat[2, 2] = -(far + s * near) / (far - s * near);
			mat[3, 2] = -1f;
			mat[2, 3] = (-2f * far * s * near) / (far - s * near);

			//test();

			//return Matrix4x4.Translate(-eyeLocal) * mat;
			return mat;
		}


		public void UpdateCameraProjection()
		{
			ProjectorEyes eyes = this.mount.Eyes;

			Quaternion change = previousRotation * Quaternion.Inverse(this.cam.transform.rotation);
			viewMat *= Matrix4x4.Rotate(change);
			previousRotation = this.cam.transform.rotation;

			Vector3 eyePosition = configuration.IsLeft ? eyes.Left : eyes.Right;
			Matrix4x4 mat = genProjectionMatrix(eyePosition);
			this.cam.projectionMatrix = bimberMat * mat;

			// This matrix is often referred to as "view matrix" in graphics literature.
			// Use this to calculate the Camera space position of GameObjects or to provide a custom Camera's location that is not based on the transform.
			// Note that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis.
			// If you change this matrix, the camera no longer updates its rendering based on its Transform.This lasts until you call ResetWorldToCameraMatrix.

			// We use this matrix since it can be set differently for both eyes !
			// Compensation of head movement, frustums should not move on the walls
			this.cam.worldToCameraMatrix = viewMat * Matrix4x4.Translate(-eyePosition);
		}
	}
}
