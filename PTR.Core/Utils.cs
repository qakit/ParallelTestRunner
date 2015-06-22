using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PTR.Core
{
	public class Utils
	{
		public static string GetIpAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily.ToString() == "InterNetwork");
			return ip != null ? ip.ToString() : "127.0.0.1";
		}
	}
}
