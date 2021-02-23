using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using Htw.Cave.Kinect.Utils;

namespace Htw.Cave.Kinect
{
	public class KinectReader
	{
		private KinectSensor m_Sensor;

		public KinectReader(KinectSensor sensor)
		{
			this.m_Sensor = sensor;
		}
		

	}
}
