using System;
using System.Net;
using System.Text;
using System.Threading;
using Log;


namespace ServerInterface
	{
	
	static class DeviceComm
		{
		private static UdpClient udp = null;
		private static IPEndPoint server;


		public static bool Open(string cip,int cport,string dip,int dport)

		{
			bool rtn = false;

			if (udp == null)
				{
				udp = new UdpClient(cip,cport,dip,dport);
				if (udp.Connected())
					{
					server = udp.Server();
					rtn = true;
					}
				else
					{
					Log.Log.LogEntry("DeviceComm open failed.");
					}
				}
			else
				rtn = true;
			return(rtn);
		}



		public static void Close()

		{
			if ((udp != null) && (udp.Connected()))
				{
				udp.Close();
				udp = null;
				}
		}



		public static string SendCommand(string command,int timeout_count)

		{
			string rtn = "";
			byte[] cmd;
			byte[] rsp = new byte [UdpClient.MAX_DG_SIZE];
			ASCIIEncoding encode = new ASCIIEncoding();

//			Log.LogEntry(command);
			if (udp != null)
				{
				if (timeout_count < 20)
					timeout_count = 20;
				cmd = encode.GetBytes(command);
				if (udp.Send(cmd.Length,cmd,server))
					rtn = ReceiveResponse(timeout_count);
				else
					rtn = "fail UDP send failure";
				}
			else
				rtn = "fail UDP not open.";
			return(rtn);
		}



		private static string ReceiveResponse(int timeout_count)

		{
			byte[] rsp = new byte[UdpClient.MAX_DG_SIZE];
			ASCIIEncoding encode = new ASCIIEncoding();
			int len = 0;
			IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
			int count = 0;
			string rtn = "";

			do
				{
				len = udp.Receive(rsp.Length,rsp,ref ep);
				if (len > 0)
					rtn = encode.GetString(rsp,0,len);
				else
					{
					count += 1;
					if (count < timeout_count)
						Thread.Sleep(10);
					}
				}
			while ((len == 0) && (count < timeout_count));
			if (count == timeout_count)
				rtn = "fail UDP receive timedout";
//			Log.LogEntry(rtn);
			return(rtn);
		}


		}
	}
