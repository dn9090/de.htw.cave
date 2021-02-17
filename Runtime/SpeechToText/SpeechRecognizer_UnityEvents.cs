using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Htw.Cave.SpeechToText
{
	[AddComponentMenu("Htw.Cave/SpeechToText/Speech Recognizer (Unity Events)")]
	[RequireComponent(typeof(SpeechRecognizer))]
	public sealed class SpeechRecognizer_UnityEvents : MonoBehaviour
	{
		[Serializable]
		public sealed class Recognition_UnityEvent : UnityEvent<string> { }

		[SerializeField]
		private UnityEvent onBegin;
		public UnityEvent OnBegin
		{
			get { return this.onBegin; }
		}

		[SerializeField]
		private Recognition_UnityEvent onHypothesis;
		public Recognition_UnityEvent OnHypothesis
		{
			get { return this.onHypothesis; }
		}

		[SerializeField]
		private Recognition_UnityEvent onRecognized;
		public Recognition_UnityEvent OnRecognized
		{
			get { return this.onRecognized; }
		}

		[SerializeField]
		private UnityEvent onCompleted;
		public UnityEvent OnCompleted
		{
			get { return this.onCompleted; }
		}

		private SpeechRecognizer component;

		public void Awake()
		{
			this.component = base.GetComponent<SpeechRecognizer>();

			InitializeUnityEvents();
			AddUnityEvents();
		}

		public void OnDestroy()
		{
			RemoveUnityEvents();
		}

		private void Begin_UnityEvent()
		{
			this.onBegin.Invoke();
		}

		private void Hypothesis_UnityEvent(string e)
		{
			this.onHypothesis.Invoke(e);
		}

		private void Recognized_UnityEvent(string e)
		{
			this.onRecognized.Invoke(e);
		}

		private void Completed_UnityEvent()
		{
			this.onCompleted.Invoke();
		}

		private void InitializeUnityEvents()
		{
			if(this.onBegin == null)
				this.onBegin = new UnityEvent();

			if(this.onHypothesis == null)
				this.onHypothesis = new Recognition_UnityEvent();

			if(this.onRecognized == null)
				this.onRecognized = new Recognition_UnityEvent();

			if(this.onCompleted == null)
				this.onCompleted = new UnityEvent();
		}

		private void AddUnityEvents()
		{
			this.component.Begin += Begin_UnityEvent;
			this.component.Hypothesis += Hypothesis_UnityEvent;
			this.component.Recognized += Recognized_UnityEvent;
			this.component.Completed += Completed_UnityEvent;
		}

		private void RemoveUnityEvents()
		{
			this.component.Begin -= Begin_UnityEvent;
			this.component.Hypothesis -= Hypothesis_UnityEvent;
			this.component.Recognized -= Recognized_UnityEvent;
			this.component.Completed -= Completed_UnityEvent;
		}
	}
}
