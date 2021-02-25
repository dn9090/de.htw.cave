using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Htw.Cave.Kinect
{
	/// <summary>
	/// Visualizes <see cref="KinectActor.trackables"/> positions and rotations.
	/// Initializes automatically when added to a <see cref="KinectActor"/>.
	/// </summary>
	[AddComponentMenu("Htw.Cave/Kinect/Kinect Skeleton")]
	[RequireComponent(typeof(KinectActor))]
	public sealed class KinectSkeleton : MonoBehaviour
	{
		private KinectActor m_Actor;

		private Dictionary<KinectTrackable, GameObject> m_Bones;

		private Mesh m_Mesh;

		public void Awake()
		{
			this.m_Actor = GetComponent<KinectActor>();
			this.m_Bones = new Dictionary<KinectTrackable, GameObject>();
			this.m_Mesh = Resources.Load<Mesh>("Bone_Arrow");

			if(this.m_Mesh == null)
			{
				Debug.LogError("Failed to load mesh \"Bone_Arrow\" from the resources.");
				enabled = false;
			}
		}

		public void OnEnable()
		{
			this.m_Actor.onTrack += AddBone;
			this.m_Actor.onUntrack += RemoveBone;
		}

		public void OnDisable()
		{
			this.m_Actor.onTrack -= AddBone;
			this.m_Actor.onUntrack -= RemoveBone;
		}

		public void Start()
		{
			foreach(var trackable in this.m_Actor.trackables)
				AddBone(trackable);
		}

		private void AddBone(KinectTrackable trackable)
		{
			if(this.m_Bones.ContainsKey(trackable))
				return;

			var bone = new GameObject("Kinect Skeleton Bone");
			var filter = bone.AddComponent<MeshFilter>();
			var renderer = bone.AddComponent<MeshRenderer>();

			bone.transform.parent = trackable.transform;
			bone.transform.localPosition = Vector3.zero;
			bone.transform.localRotation = Quaternion.identity;
			bone.transform.localScale = new Vector3(3f, 3f, 3f);
			filter.mesh = this.m_Mesh;
			renderer.material.color = Color.green;
			renderer.shadowCastingMode = ShadowCastingMode.Off;

			this.m_Bones.Add(trackable, bone);
		}

		private void RemoveBone(KinectTrackable trackable)
		{
			if(this.m_Bones.TryGetValue(trackable, out GameObject bone))
			{
				Destroy(bone);
				this.m_Bones.Remove(trackable);
			}
		}
	}
}
