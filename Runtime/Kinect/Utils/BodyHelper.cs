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
			for(int i = 1; i < bodies.Length; ++i)
			{
				var body = bodies[i];
				var index = i;

				for(; index > 0 && Compare(body, bodies[index - 1]) <= 0; --index)
					bodies[index] = bodies[index - 1];

				bodies[index] = body;
			}

			for(int i = 0; i < bodies.Length; ++i)
				if(bodies[i] == null || !bodies[i].IsTracked)
					return i;
				
			return bodies.Length;
		}

		public static int Compare(Body a, Body b)
		{
			// This is ugly because nowhere in the documentation
			// is stated which guarantees GetAndRefreshBodyData offers.
			// So we need to null check everything beforehand.

			if(a == null && b == null)
				return 0;

			if(a == null)
				return 1;

			if(b == null)
				return -1;

			if(!a.IsTracked && !b.IsTracked)
				return 0;

			if(!a.IsTracked)
				return 1;

			if(!b.IsTracked)
				return -1;

			return a.TrackingId.CompareTo(b.TrackingId);
		}
	}
}
