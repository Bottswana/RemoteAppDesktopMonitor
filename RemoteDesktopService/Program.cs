using System;
using System.ServiceProcess;

namespace RemoteDesktopService
{
	static class Program
	{
		static void Main()
		{
			// Start the service
			ServiceBase.Run(new RemoteAppDesktopService());
		}
	}
}
