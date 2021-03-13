using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Windows.Kinect;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	public enum KinectActorConstructionType
	{
		Empty,
		Basic,
		Full,
		Prefab
	}

	/// <summary>
	/// Builds a <see cref="KinectActor"/> with
	/// configurable <see cref="KinectTrackable"/> instances.
	/// </summary>
	public struct KinectActorBuilder
	{
		private static Mesh s_ArrowMesh;

		private static Material s_ArrowMaterial;

		private KinectActor m_Actor;

		private Transform m_Transform;

		private bool m_ApplyFilter;

		private bool m_ApplyVisualArrow;

		public KinectActorBuilder(KinectActor actor)
		{
			this.m_Actor = actor;
			this.m_Transform = actor.transform;
			this.m_ApplyFilter = false;
			this.m_ApplyVisualArrow = false;

			if(s_ArrowMesh == null)
			{
				s_ArrowMesh = Resources.Load<Mesh>("Joint_Arrow");
				s_ArrowMaterial = new Material(Shader.Find("Standard"));
				s_ArrowMaterial.color = Color.green;

				if(s_ArrowMesh == null)
					Debug.LogError("Failed to load mesh \"Joint_Arrow\" from the resources.");
			}
		}

		public static KinectActorBuilder Construct(string name, Transform parent)
		{
			var gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			return new KinectActorBuilder(gameObject.AddComponent<KinectActor>());
		}

		public static KinectActorBuilder Construct(string name, Transform parent, KinectActor prefab)
		{
			var actor = MonoBehaviour.Instantiate<KinectActor>(prefab, parent);
			actor.gameObject.name = name;
			return new KinectActorBuilder(actor);
		}

		public static KinectActorBuilder Construct(string name, Transform parent, KinectActorConstructionType constructionType, KinectActor prefab)
		{
			if(constructionType == KinectActorConstructionType.Prefab)
				return KinectActorBuilder.Construct(name, parent, prefab);
				
			var builder = KinectActorBuilder.Construct(name, parent);

			if(constructionType == KinectActorConstructionType.Basic
			|| constructionType == KinectActorConstructionType.Full)
			{
				builder
					.WithJoints(PredefinedJointTypes.head)
					.WithJoints(PredefinedJointTypes.handLeft)
					.WithJoints(PredefinedJointTypes.handRight);
			}
			
			if(constructionType == KinectActorConstructionType.Full)
			{
				builder
					.WithJoints(PredefinedJointTypes.torso)
					.WithJoints(PredefinedJointTypes.legLeft)
					.WithJoints(PredefinedJointTypes.legRight)
					.WithJoints(PredefinedJointTypes.armLeft)
					.WithJoints(PredefinedJointTypes.armRight);
			}

			return builder;
		}

		public KinectActorBuilder WithJoints(params JointType[] jointTypes)
		{
			foreach(var jointType in jointTypes)
			{
				var name = "Kinect " + jointType.MakeHumanReadable();

				switch(jointType)
				{
					case JointType.Head:
						KinectTrackable.Create<KinectHead>(name, this.m_Transform);
						break;
					case JointType.HandLeft:
						KinectTrackable.Create<KinectHand>(name, this.m_Transform, (trackable) => { trackable.handType = HandType.Left; });
						break;
					case JointType.HandRight:
						KinectTrackable.Create<KinectHand>(name, this.m_Transform, (trackable) => { trackable.handType = HandType.Right; });
						break;
					default:
						KinectTrackable.Create<KinectDynamicJoint>(name, this.m_Transform, (trackable) => { trackable.jointType = jointType; });
						break;
				}
			}

			return this;
		}

		public KinectActorBuilder WithFilter(bool value = true) 
		{
			this.m_ApplyFilter = value;
			return this;
		}

		public KinectActorBuilder WithJointVisualization(bool value = true) 
		{
			this.m_ApplyVisualArrow = value;
			return this;
		}

		public KinectActor Build()
		{
			if(this.m_ApplyFilter || this.m_ApplyVisualArrow)
			{
				var trackables = this.m_Actor.GetComponentsInChildren<KinectTrackable>();

				foreach(var trackable in trackables)
					BuildTrackable(trackable);
			}

			return this.m_Actor;
		}

		private void BuildTrackable(KinectTrackable trackable)
		{
			if(this.m_ApplyFilter)
				trackable.filterPositionAndRotation = this.m_ApplyFilter;
			
			if(this.m_ApplyVisualArrow)
			{
				var bone = new GameObject("Kinect Joint Visualization");
				var filter = bone.AddComponent<MeshFilter>();
				var renderer = bone.AddComponent<MeshRenderer>();

				bone.transform.parent = trackable.transform;
				bone.transform.localPosition = Vector3.zero;
				bone.transform.localRotation = Quaternion.identity;
				bone.transform.localScale = new Vector3(3f, 3f, 3f);
				filter.mesh = s_ArrowMesh;
				renderer.material = s_ArrowMaterial;
				renderer.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}
}
