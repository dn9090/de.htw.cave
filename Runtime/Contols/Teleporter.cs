using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Htw.Cave.Controls
{
	/// <summary>
	/// Looks for a possible teleport position and visualizes it via a curve.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Controls/Teleporter")]
	[RequireComponent(typeof(LineRenderer))]
	public sealed class Teleporter : MonoBehaviour
	{
		[SerializeField]
		private LayerMask collisionLayer;
		public LayerMask CollisionLayer
		{
			get => this.collisionLayer;
			set => this.collisionLayer = value;
		}

		[SerializeField]
		private float detectionDistance;
		public float DetectionDistance
		{
			get => this.detectionDistance;
			set => this.detectionDistance = value;
		}

		[SerializeField]
		private AnimationCurve rayCurve;
		public AnimationCurve RayCurve
		{
			get => this.rayCurve;
			set => this.rayCurve = value;
		}

		[SerializeField]
		private float curveMultiplier;
		public float CurveMultiplier
		{
			get => this.curveMultiplier;
			set => this.curveMultiplier = value;
		}

		[SerializeField]
		private int curveResolution;
		public int CurveResolution
		{
			get => this.curveResolution;
			set => this.curveResolution = value;
		}

		private LineRenderer rayRenderer;
		private	Transform areaPlane;
		private Vector3 rayPoint;
		private Vector3 rayNormal;
		private bool validPosition;

		public void Awake()
		{
			this.rayRenderer = base.GetComponent<LineRenderer>();
			this.rayRenderer.positionCount = this.curveResolution;
			this.rayRenderer.useWorldSpace = true;
			this.rayRenderer.material = Resources.Load<Material>("Teleporter/Ray");
			this.rayRenderer.enabled = false;
			this.areaPlane = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
			this.areaPlane.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Teleporter/Area");
			this.areaPlane.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
			this.areaPlane.transform.localScale = Vector3.one * 0.25f;
			this.areaPlane.parent = transform;
			Destroy(this.areaPlane.GetComponent<Collider>());
			this.validPosition = false;

			base.enabled = false;
		}

		public void OnEnable()
		{
			this.rayRenderer.enabled = true;
			CastRay();
		}

		public void Update()
		{
			CastRay();
		}

		public void OnDisable()
		{
			this.rayRenderer.enabled = false;

			if(this.areaPlane != null)
				this.areaPlane.gameObject.SetActive(false);

			this.validPosition = false;
		}

		public void Reset()
		{
			LineRenderer lineRenderer = base.GetComponent<LineRenderer>();
			lineRenderer.material = Resources.Load<Material>("Teleporter/Ray");
			lineRenderer.receiveShadows = false;
			lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
			lineRenderer.textureMode = LineTextureMode.Tile;
			lineRenderer.startWidth = 0.1f;
			lineRenderer.endWidth = 0.15f;
			lineRenderer.enabled = false;

			this.collisionLayer = 1;
			this.rayCurve = new AnimationCurve(
				new Keyframe(0f, 0.7f, 1.3f, 1.3f),
				new Keyframe(1f, 0f, -3.7f, -3.7f)
			);
			this.detectionDistance = 20f;
			this.curveMultiplier = 1.2f;
			this.curveResolution = 12;
		}

		public bool ValidTeleportPosition(out Vector3 position)
		{
			position = this.rayPoint;

			return this.validPosition;
		}

		private void CastRay()
		{
			if(transform.hasChanged)
			{
				if(TryRayPoint(transform.forward, out this.rayPoint, out this.rayNormal))
				{
					DrawRay(this.rayPoint);
					DrawArea(this.rayPoint, this.rayNormal);
					this.validPosition = true;
				} else {
					this.rayRenderer.enabled = false;
					this.areaPlane.gameObject.SetActive(false);
					this.validPosition = false;
				}
			} else {
				DrawRay(this.rayPoint);
				DrawArea(this.rayPoint, this.rayNormal);
			}
		}

		private void DrawRay(Vector3 target)
		{
			this.rayRenderer.enabled = true;

			for(int i = 0; i < this.curveResolution; ++i)
			{
				float percentage = (float)(i + 1) / (float)this.curveResolution;
				float eval = this.rayCurve.Evaluate(percentage);
				Vector3 lerp = Vector3.Lerp(transform.position, target, percentage);
				this.rayRenderer.SetPosition(i, new Vector3(lerp.x, Mathf.Lerp(target.y, transform.position.y * this.curveMultiplier, eval), lerp.z));
			}
		}

		private void DrawArea(Vector3 target, Vector3 normal)
		{
			this.areaPlane.gameObject.SetActive(true);
			this.areaPlane.transform.up = normal;
			this.areaPlane.position = target + transform.up * 0.1f;
		}

		private bool TryRayPoint(Vector3 direction, out Vector3 point, out Vector3 normal)
		{
			float distance = this.detectionDistance * 0.5f;

			point = Vector3.zero;
			normal = Vector3.zero;

			RaycastHit hit;

#if UNITY_EDITOR
			Debug.DrawRay(transform.position, direction * distance, Color.white, 0.02f);
			Debug.DrawRay(transform.position + direction * distance, Vector3.Slerp(direction, -transform.up, 0.5f) * (distance * 2f), Color.white, 0.02f);
#endif

			if(Physics.SphereCast(transform.position, 0.01f, direction, out hit, distance, this.collisionLayer.value))
			{
				point = hit.point;
				normal = hit.normal;
				return true;
			}

			Vector3 origin = transform.position + direction * distance;

			if(Physics.SphereCast(origin, 0.01f, Vector3.Slerp(direction, -transform.up, 0.5f), out hit, distance * 2f, this.collisionLayer.value))
			{
				point = hit.point;
				normal = hit.normal;
				return true;
			}

			return false;
		}
	}
}
