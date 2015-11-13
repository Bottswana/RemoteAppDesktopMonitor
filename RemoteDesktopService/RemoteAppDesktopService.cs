using System;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;

namespace RemoteDesktopService
{
	public partial class RemoteAppDesktopService : ServiceBase
	{
		#region Service Template
		public RegistryMonitor Thread;

		public RemoteAppDesktopService()
		{
			InitializeComponent();

			_GetEventLog();
			Thread = new RegistryMonitor(this, this.ApplicationEventLog);
		}

		protected override void OnStart(string[] args)
		{
			var tThread = new Thread(new ThreadStart(Thread.MonitorRegistry))
			{
				IsBackground = true
			};

			tThread.Start();
		}

		protected override void OnStop()
		{
			Thread.Stop();
		}
		#endregion Service Tempalte

		#region Service Methods
		protected void _GetEventLog()
		{
			if( !EventLog.SourceExists("DesktopAppMonitor") )
			{
				EventLog.CreateEventSource("RemoteApp Desktop Monitor", "Application");
			}

			this.ApplicationEventLog.Source = "RemoteApp Desktop Monitor";
			this.ApplicationEventLog.Log = "Application";
		}
		#endregion Service Methods
	}

	public class RegistryMonitor
	{
		#region Class Setup
		private ManualResetEvent _Sleeper;
		private bool _ApplicationRunning;

		private RemoteAppDesktopService _Parent;
		private EventLog _Log;

		public RegistryMonitor(RemoteAppDesktopService tServ, EventLog tLog)
		{
			this._Sleeper = new ManualResetEvent(false);
			this._ApplicationRunning = true;

			this._Parent = tServ;
			this._Log = tLog;
		}

		public void Stop()
		{
			this._ApplicationRunning = false;
			this._Sleeper.Set();
		}
		#endregion Class Setup

		public void MonitorRegistry()
		{
			const string RegistryPath = @"Software\Microsoft\Windows NT\CurrentVersion\Terminal Server\CentralPublishedResources\PublishedFarms";
			var RootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			var TSKey = RootKey.OpenSubKey(RegistryPath, false);

			if( TSKey == null )
			{
				// Check we are actually a broker server (Did the key open properly?)
				this._Log.WriteEntry("This computer does not appear to be a Terminal Server Broker, as the registry keys for Terminal Server could not be found.\r\nThe service will now stop", EventLogEntryType.Error);
				this._Parent.Stop();
				return;
			}

			while( this._ApplicationRunning )
			{
				try
				{
					// Poll the ShowInPortal key and update if we need to (Based on the status of the ForcePortal key)
					foreach( var childKey in TSKey.GetSubKeyNames() )
					{
						using( var ThisNode = TSKey.OpenSubKey(childKey + @"\RemoteDesktops\" + childKey, true) )
						{
							var DesktopState	= (int) ThisNode.GetValue("ShowInPortal", 0);
							var ForceState		= (int) ThisNode.GetValue("ForcePortal", 0);

							if( (DesktopState != 1) && (ForceState == 1) )
							{
								this._Log.WriteEntry("Collection named " + childKey + " has Desktop Publishing disabled. Enabling");
								ThisNode.SetValue("ShowInPortal", 1);
							}

							ThisNode.Close();
						}
					}
				}
				catch( Exception Ex )
				{
					this._Log.WriteEntry("An exception has occoured: " + Ex.Message + "\r\n\r\n" + Ex.StackTrace, EventLogEntryType.Error);
				}

				// Wait for next check
				this._Sleeper.WaitOne(TimeSpan.FromSeconds(15));
			}

			// Close Registry Key
			TSKey.Close(); RootKey.Close();
			TSKey.Dispose(); RootKey.Dispose();
		}
	}
}
