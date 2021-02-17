using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JoyconLib;

namespace Htw.Cave.Joycons
{
	/// <summary>
	/// Responsible providing the detected Joy-Con controller and
	/// initializing the Joy-Con bindings.
	/// </summary>
	[RequireComponent(typeof(JoyconManager))]
	public class JoyconController : MonoBehaviour
	{
		private static bool initialized = false;

		public static List<Joycon> all;
		public static List<Joycon> All
		{
			get
			{
				if(!initialized)
					Initialize();

				return all;
			}
		}

		public static Joycon First { get; set; }
		public static Joycon Left { get; set; }
		public static Joycon Right { get; set; }

		public static Joycon GetLeftOrFirst()
		{
			return Left == null ? First : Left;
		}

		public static Joycon GetRightOrFirst()
		{
			return Right == null ? First : Right;
		}

		public static List<Joycon> GetLeftJoycons()
		{
			return All.Where(j => j.isLeft).ToList();
		}

		public static List<Joycon> GetRightJoycons()
		{
			return All.Where(j => !j.isLeft).ToList();
		}

		private static void Initialize()
		{
			if(initialized)
				return;

			if(JoyconManager.Instance == null)
				throw new UnityException("Either the JoyconManager is missing or you accessed it before Start.");

			all = JoyconManager.Instance.joycons;
			First = all.FirstOrDefault();
			Left = all.FirstOrDefault(j => j.isLeft);
			Right = all.FirstOrDefault(j => !j.isLeft);
		}

		[SerializeField]
		private JoyconBinding binding;
		public JoyconBinding Binding
		{
			get => this.binding;
			set => this.binding = value;
		}

		public void Awake()
		{
			initialized = false;
		}

		public void OnEnable()
		{
			this.binding.Activate();
		}

		public void Start()
		{
			Initialize();
		}

		public void OnDestroy()
		{
			initialized = false;
		}
	}
}
