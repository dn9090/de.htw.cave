using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Htw.Cave.Kinect.Utils
{
	public readonly struct OneEuroParams
	{
		public readonly float minCutoff;

		public readonly float beta;

		public readonly float derivateCutoff;

		public OneEuroParams(float minCutoff, float beta = 0f, float derivateCutoff = 1f)
		{
			this.minCutoff = minCutoff;
			this.beta = beta;
			this.derivateCutoff = derivateCutoff;
		}
	}

	public struct OneEuroFilter3
	{
		private OneEuroFilter m_X;

		private OneEuroFilter m_Y;

		private OneEuroFilter m_Z;

		public Vector3 Filter(Vector3 value, float rate, in OneEuroParams parameters) =>
			new Vector3(
				this.m_X.Filter(value.x, rate, in parameters),
				this.m_Y.Filter(value.y, rate, in parameters),
				this.m_Z.Filter(value.z, rate, in parameters));
	}

	public struct OneEuroFilter4
	{
		private OneEuroFilter m_X;

		private OneEuroFilter m_Y;

		private OneEuroFilter m_Z;

		private OneEuroFilter m_W;

		public Vector4 Filter(Vector4 value, float rate, in OneEuroParams parameters) =>
			new Vector4(
				this.m_X.Filter(value.x, rate, in parameters),
				this.m_Y.Filter(value.y, rate, in parameters),
				this.m_Z.Filter(value.z, rate, in parameters),
				this.m_W.Filter(value.w, rate, in parameters));

		public Quaternion Filter(Quaternion value, float rate, in OneEuroParams parameters) =>
			new Quaternion(
				this.m_X.Filter(value.x, rate, in parameters),
				this.m_Y.Filter(value.y, rate, in parameters),
				this.m_Z.Filter(value.z, rate, in parameters),
				this.m_W.Filter(value.w, rate, in parameters));
	}

	internal struct LowPassFilter
	{
		public float hatXPrev;

		public float Filter(float x, float alpha) => this.hatXPrev = alpha * x + (1 - alpha) * this.hatXPrev;
	}

	public struct OneEuroFilter
	{
		private LowPassFilter m_XFilter;

		private LowPassFilter m_DxFilter;

		private bool m_Initialized;

		public float Filter(float x, float rate, in OneEuroParams parameters)
		{
			if(!this.m_Initialized)
			{
				this.m_DxFilter.hatXPrev = 0f;
				this.m_XFilter.hatXPrev = x;
				return x;
			}

			this.m_Initialized = true;

			float dx = (x - this.m_XFilter.hatXPrev) * rate;
			float edx = this.m_DxFilter.Filter(dx, Alpha(rate, parameters.derivateCutoff));
			float cutoff = parameters.minCutoff + parameters.beta * Math.Abs(edx);

			return this.m_XFilter.Filter(x, Alpha(rate, cutoff));
		}

		public static float Freq(float lastTime, float currentTime) => 1f / (currentTime - lastTime);

		private static float Alpha(float rate, float cutoff)
		{
			float te = 1f / rate;
			float tau = 1f / (2f * Mathf.PI * cutoff);
			return 1f / (1f + tau / te);
		}
	}
}
