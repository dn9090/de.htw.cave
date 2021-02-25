using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Windows.Kinect;

namespace Htw.Cave.Kinect.Utils
{
	internal static class BodyHelper
	{
		public static int SortAndCount(Body[] bodies)
		{
			// Null check can be avoided because GetAndRefreshBodyData
			// internally creates them.

			for(int i = 1; i < bodies.Length; ++i)
			{
				var body = bodies[i];
				var index = i;

				for(; index > 0 && Compare(body, bodies[index - 1]) <= 0; --index)
					bodies[index] = bodies[index - 1];

				bodies[index] = body;
			}

			for(int i = 0; i < bodies.Length; ++i)
				if(!bodies[i].GetIsTrackedFast())
					return i;
				
			return bodies.Length;
		}

		public static int Compare(Body a, Body b)
		{
			if(!a.GetIsTrackedFast() && !b.GetIsTrackedFast())
				return 0;

			if(!a.GetIsTrackedFast())
				return 1;

			if(!b.GetIsTrackedFast())
				return -1;

			return a.GetTrackingIdFast().CompareTo(b.GetTrackingIdFast());
		}
	}
}
