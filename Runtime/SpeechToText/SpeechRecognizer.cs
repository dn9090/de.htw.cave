using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace Htw.Cave.SpeechToText
{
	/// <summary>
	/// Provides event driven functionallity to the Windows speech recognizer.
	/// </summary>
	[AddComponentMenu("Htw.Cave/SpeechToText/Speech Recognizer")]
	public sealed class SpeechRecognizer : MonoBehaviour
	{
		[SerializeField]
		private float initialTimeout;
		public float InitialTimeout
		{
			get { return this.initialTimeout; }
			set { this.initialTimeout = value; }
		}

		[SerializeField]
		private float automaticTimeout;
		public float AutomaticTimeout
		{
			get { return this.automaticTimeout; }
			set { this.automaticTimeout = value; }
		}

		public event Action Begin;
		public event Action<string> Hypothesis;
		public event Action<string> Recognized;
		public event Action Completed;

		private DictationRecognizer dictationRecognizer;

		public void Awake()
		{
			this.dictationRecognizer = new DictationRecognizer();
			this.dictationRecognizer.InitialSilenceTimeoutSeconds = this.initialTimeout;
			this.dictationRecognizer.AutoSilenceTimeoutSeconds = this.automaticTimeout;

			this.dictationRecognizer.DictationHypothesis += (text) => {
				if(this.Hypothesis != null)
					this.Hypothesis(text);
			};
			this.dictationRecognizer.DictationResult += (text, confidence) => {
				if(this.Recognized != null)
					this.Recognized(text);
			};
			this.dictationRecognizer.DictationComplete += (completionCause) => {
				if(this.Completed != null)
					this.Completed();
			};
		}

		public void Reset()
		{
			this.initialTimeout = 3f;
			this.automaticTimeout = 2f;
		}

		public void Recognize()
		{
			if(PhraseRecognitionSystem.isSupported && this.dictationRecognizer.Status == SpeechSystemStatus.Stopped)
			{
				this.dictationRecognizer.Start();

				if(this.Begin != null)
					this.Begin();
			}
		}
	}
}
