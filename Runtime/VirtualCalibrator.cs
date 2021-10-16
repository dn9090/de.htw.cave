using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Htw.Cave.SimpleTcp;

namespace Htw.Cave
{
	[AddComponentMenu("Htw.Cave/Virtual Calibrator")]
	[RequireComponent(typeof(VirtualEnvironment))]
	public sealed partial class VirtualCalibrator : MonoBehaviour
	{
		/// <summary>
		/// Available calibration messages.
		/// </summary>
		public static class Message
		{
			public const int Disconnect = 1;

			public const int Sync = 10;
			
			public const int Calibration = 11;
			
			public const int ShowHelpers = 30;

			public const int LockCameras = 31;

			public const int OnlyCameraDisplay = 32;
		}
		
		/// <summary>
		/// Stores the calibration data of multiple cameras.
		/// Used for sending and receiving calibrations, as well as
		/// for loading and saving calibrations from or to disk.
		/// </summary>
		public struct Package
		{
			public bool isEmpty => this.calibrations == null || calibrations.Length == 0;

			public string timestamp;

			public VirtualOutputTarget outputTarget;

			public int highestVirtualDisplay;

			public VirtualCamera.Calibration[] calibrations;

			public Package(DateTime timestamp, VirtualOutputTarget outputTarget,
				int highestActiveDisplay, VirtualCamera.Calibration[] calibrations)
			{
				this.timestamp = timestamp.ToString("o");
				this.outputTarget = outputTarget;
				this.highestVirtualDisplay = highestActiveDisplay;
				this.calibrations = calibrations;
			}
		}

		/// <summary>/
		/// The localhost address.
		/// </summary>
		public static string localhost = "127.0.0.1";
	
		/// <summary>
		/// The port used to establish a connection between
		/// the calibration editor and application.
		/// </summary>
		public static ushort port = 55555;

		private SimpleTcpServer m_Server;
		
		private VirtualEnvironment m_Environment;
		
		public void Awake()
		{
			this.m_Server = new SimpleTcpServer();
			this.m_Environment = GetComponent<VirtualEnvironment>();
			
			RenderSingleCamera(-1);
		}

		public void OnEnable()
		{
			if(!this.m_Server.Listen(port))
				Debug.LogError($"Failed to listen on port {port}.", this);
		}

		public void Start()
		{
			// Try to load the latest calibration in the standalone,
			// so that we do not need to connect with the server everytime.
			if(!Application.isEditor && TryLoadCalibrationsFromDisk(out VirtualOutputTarget outputTarget,
				out VirtualCamera.Calibration[] calibrations))
			{
				VirtualUtility.MatchAndApplyCalibrations(this.m_Environment, calibrations);
				this.m_Environment.SetOutputTarget(outputTarget);
			}
		}
		
		public void Update()
		{
			while(this.m_Server.messageQueue.TryDequeue(out SimpleTcpMessage message))
				Execute(message);
		}
		
		public void OnDisable()
		{
			this.m_Server.SendMessage(new SimpleTcpMessage(Message.Disconnect));
			this.m_Server.Stop();
		}
		
		private void Execute(SimpleTcpMessage message)
		{
			switch(message.type)
			{
				case Message.Calibration:
				{
					var json = message.GetString();
					var package = JsonUtility.FromJson<Package>(json);

					VirtualUtility.MatchAndApplyCalibrations(this.m_Environment, package.calibrations);
					this.m_Environment.SetOutputTarget(package.outputTarget);

					try
					{
						File.WriteAllText(GetPersistentCalibrationFilePath(), json);
					} catch {
						Debug.LogError("Failed to write calibration to disk.", this);
					}

					goto case Message.Sync;
				}
				case Message.Sync:
				{
					var calibrations = VirtualUtility.CollectCalibrations(this.m_Environment);
					var highestActiveDisplay = VirtualUtility.GetHighestVirtualDisplay(calibrations);
					var package = new Package(DateTime.Now, this.m_Environment.outputTarget, highestActiveDisplay, calibrations);
					var json = JsonUtility.ToJson(package);
					
					this.m_Server.SendMessage(new SimpleTcpMessage(Message.Calibration, json));
					this.m_Server.SendMessage(new SimpleTcpMessage(Message.ShowHelpers, this.showHelpers));
					this.m_Server.SendMessage(new SimpleTcpMessage(Message.LockCameras, this.m_Environment.lockCameras));
					this.m_Server.SendMessage(new SimpleTcpMessage(Message.OnlyCameraDisplay, this.m_RenderSingleCameraDisplay));
					break;
				}
				case Message.ShowHelpers:
					this.showHelpers = message.GetBool();
					this.m_Server.SendMessage(new SimpleTcpMessage(Message.ShowHelpers, this.showHelpers));
					break;
				case Message.LockCameras:
					this.m_Environment.lockCameras = message.GetBool();
					if(this.m_Environment.lockCameras)
						this.m_Environment.LockCamerasToPosition(this.m_Environment.center);
					this.m_Server.SendMessage(new SimpleTcpMessage(Message.LockCameras, this.m_Environment.lockCameras));
					break;
				case Message.OnlyCameraDisplay:
					RenderSingleCamera(message.GetInt());
					this.m_Server.SendMessage(new SimpleTcpMessage(Message.OnlyCameraDisplay, this.m_RenderSingleCameraDisplay));
					break;
				default:
					Debug.LogWarning("Received unknown calibration message: " + message.type, this);
					break;
			}
		}

		internal static bool TryLoadCalibrationsFromDisk(out VirtualOutputTarget outputTarget,
			out VirtualCamera.Calibration[] calibrations)
		{
			try
			{
				var json = File.ReadAllText(GetPersistentCalibrationFilePath());
				var package = JsonUtility.FromJson<Package>(json);

				outputTarget = package.outputTarget;
				calibrations = package.calibrations;
				return true;
			} catch {
				outputTarget = default;
				calibrations = default;
				return false;
			}
		}
		
		internal static string GetPersistentCalibrationFilePath()
		{
			// Use the buildGUID to identify the application. If there is a new build,
			// the calibration of older builds should not be used because the
			// keys of the serialized JSON can change.
			var name = Application.buildGUID + ".calibration.json";
			return Path.Combine(Application.persistentDataPath, name);
		}
	}
}
