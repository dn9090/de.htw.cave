using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Htw.Cave.SimpleTcp;

namespace Htw.Cave.Calibration
{
	public class VirtualCalibratorClient
	{
		[Flags]
		public enum Status
		{
			Disconnected     = 0,
			Connected        = 1,
			ConnectionFailed = 2,
			Initialized      = 4 | Connected
		}

		public Status status => this.m_Status;

		public bool showHelpers => this.m_ShowHelpers;

		public bool lockCameras => this.m_LockCameras;

		public int singleRenderDisplay => this.m_SingleRenderDisplay;

		public VirtualCalibrator.Package package;

		private SimpleTcpClient m_Client;
						
		private Status m_Status;
				
		private bool m_ShowHelpers;

		private bool m_LockCameras;

		private int m_SingleRenderDisplay;
		
		public static string savedHostOrLocalhost
		{
			get
			{
				return EditorPrefs.HasKey("VIRTUAL_CALIBRATOR_HOSTNAME")
					? EditorPrefs.GetString("VIRTUAL_CALIBRATOR_HOSTNAME")
					: VirtualCalibrator.localhost;
			}
			set => EditorPrefs.SetString("VIRTUAL_CALIBRATOR_HOSTNAME", value);
		}

		public VirtualCalibratorClient()
		{
			this.m_Client = new SimpleTcpClient();
			this.m_Status = Status.Disconnected;
			this.m_ShowHelpers = false;
			this.m_LockCameras = false;
		}

		public bool HasStatus(Status status) => this.status.HasFlag(status);

		public void Refresh()
		{
			while(this.m_Client.messageQueue.TryDequeue(out SimpleTcpMessage message))
				Execute(message);
		}

		public void LocalConnect() => Connect(VirtualCalibrator.localhost, false);

		public void Connect(string host, bool save)
		{
			this.m_Status = Status.Disconnected;
			
			if(this.m_Client.Connect(host, VirtualCalibrator.port))
			{
				this.m_Status |= Status.Connected;
				Sync();
			} else {
				this.m_Status |= Status.ConnectionFailed;
			}

			if(save)
				savedHostOrLocalhost = host;
		}

		public void Disconnect()
		{
			this.m_Client.Stop();
			this.m_Status = Status.Disconnected;
			this.package = default;
		}

		public void Load(string file)
		{
			if(!string.IsNullOrEmpty(file))
			{
				var json = File.ReadAllText(file);
				var source = JsonUtility.FromJson<VirtualCalibrator.Package>(json);
				VirtualUtility.MatchAndOverwriteCalibrations(source.calibrations, this.package.calibrations);
			}
		}

		public void Save(string file)
		{
			if(!string.IsNullOrEmpty(file))
			{
				var json = JsonUtility.ToJson(this.package, prettyPrint: true);
				File.WriteAllText(file, json);
			}
		}

		public void Sync()
		{
			this.m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.Sync));
		}

		public void SendPackage()
		{
			var json = JsonUtility.ToJson(this.package);
			this.m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.Calibration, json));
		}

		public void SendShowHelpers(bool value)
		{
			this.m_ShowHelpers = value;
			this.m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.ShowHelpers, this.m_ShowHelpers));
		}

		public void SendLockCameras(bool value)
		{
			this.m_LockCameras = value;
			this.m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.LockCameras, this.m_LockCameras));
		}

		public void SendSingleRenderDisplay(int display)
		{
			this.m_SingleRenderDisplay = display;
			this.m_Client.SendMessage(new SimpleTcpMessage(VirtualCalibrator.Message.OnlyCameraDisplay, this.m_SingleRenderDisplay));
		}

		private void Execute(SimpleTcpMessage message)
		{
			switch(message.type)
			{
				case VirtualCalibrator.Message.Calibration:
					this.package = JsonUtility.FromJson<VirtualCalibrator.Package>(message.GetString());
					this.m_Status |= Status.Initialized;
					break;
				case VirtualCalibrator.Message.ShowHelpers:
					this.m_ShowHelpers = message.GetBool();
					break;
				case VirtualCalibrator.Message.LockCameras:
					this.m_LockCameras = message.GetBool();
					break;
				case VirtualCalibrator.Message.OnlyCameraDisplay:
					this.m_SingleRenderDisplay = message.GetInt();
					break;
				default:
					Debug.LogWarning("Received unknown calibration message: " + message.type);
					break;
			}
		}
	}
}