using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Htw.Cave.Projectors
{
	/// <summary>
	/// Defines how a camera will render the stereoscopic image.
	/// </summary>
	[CreateAssetMenu(fileName = "New Projector Configuration", menuName = "Htw.Cave/Projector Configuration", order = 21)]
	public class ProjectorConfiguration : ScriptableObject
	{
		[SerializeField]
		private int displayId;
		public int DisplayId
		{
			get => this.displayId;
			set => this.displayId = value;
		}

		[SerializeField]
		private string displayName;
		public string DisplayName
		{
			get => this.displayName;
			set => this.displayName = value;
		}

		[SerializeField]
		private float width;
		public float Width
		{
			get => this.width;
			set => this.width = value;
		}

		[SerializeField]
		private float height;
		public float Height
		{
			get => this.height;
			set => this.height = value;
		}

		[SerializeField]
		private float fieldOfView;
		public float FieldOfView
		{
			get => this.fieldOfView;
			set => this.fieldOfView = value;
		}

		[SerializeField]
		private bool invertStereo;
		public bool InvertStereo
		{
			get => this.invertStereo;
			set => this.invertStereo = value;
		}

		[SerializeField]
		private bool isLeft;
		public bool IsLeft 
		{
			get => this.isLeft;
			set => this.isLeft = value;
		}

		// The following fields describe the distortion on the screen caused by the projector.
		// The range of values from 0,0 to 1,1 describes the complete image and the values given here
		// describe the square that is visible on the screen.
		[SerializeField]
		private Vector2 bottomLeft;

		public Vector2 BottomLeft
		{
			get => this.bottomLeft;
			set => this.bottomLeft = value;
		}

		[SerializeField]
		private Vector2 bottomRight;

		public Vector2 BottomRight
		{
			get => this.bottomRight;
			set => this.bottomRight = value;
		}

		[SerializeField]
		private Vector2 topLeft;

		public Vector2 TopLeft
		{
			get => this.topLeft;
			set => this.topLeft = value;
		}

		[SerializeField]
		private Vector2 topRight;

		public Vector2 TopRight
		{
			get => this.topRight;
			set => this.topRight = value;
		}
	}
}
