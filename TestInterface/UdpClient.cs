using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using Log;

namespace ServerInterface
	{
	/*	UDP CLIENT CLASS
		Filename: udpclient.cs
		Date:	10/18/05
		Copyright 2005 T. H. E. Solution LLC
		Author: Terry H. Ess
		Description:
			Provides udp client socket functions.
	*/

	public class UdpClient
	{
	
		// CONSTANT
		
		public static int MAX_DG_SIZE = 1500;

		
		// DATA
		
		private IPEndPoint ip_end;
		private Socket sock;
		private bool connected = false;
		private IPEndPoint server_ip_end;
		private IPEndPoint sserver_ip_end;
		private string last_error;
		private byte[] tc = new byte[UdpClient.MAX_DG_SIZE];
		

		// METHODS
		
		/*	SEND A DATAGRAM
			Description:
			Sends the indicated message as a UDP datagram.
		
			Passed parametes:	length - size of message to send
								buff - pointer to message
								
		
			Returned value:	message sent successfully (TRUE) or not (FALSE)
		*/

		public bool Send(int length,byte[] buff,IPEndPoint to)

		{
			bool rtn = false;
		
			if ((connected == true) && (length <= MAX_DG_SIZE))
				{
				try
					{
					if (sock.SendTo(buff,0,length,SocketFlags.None,to) == length)
						rtn = true;
					}
					
				catch
					{
					last_error = "UDP send failed.";
					}
				}
			return(rtn);
		}		



		/*	RECEIVE A DATAGRAM
			Description:
			Receives a UDP datagram if it is available.
		
			Passed parametes:	length - size of message to send
								buff - pointer to message
								from - buffer for sender information
								
		
			Returned value:	number of bytes received
		*/
		
		public int Receive(int length,byte[] buff,ref IPEndPoint from)
		
		{
			int rtn = 0;
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint senderep = (EndPoint)sender;
			
			if (connected)
				{
				try
					{
					if (sock.Available > 0)
						rtn = sock.ReceiveFrom(buff,length,SocketFlags.None,ref senderep);
					}
						
				catch (SocketException)
					{
					rtn = 0;
					connected = false;
					last_error = "UDP receive socket exception, connection closed.";
					}
						
				catch
					{
					rtn = 0;
					last_error = "UDP receive exeception.";
					}
				}
			if (rtn > 0)
				{
				from = sender;
				}
			return(rtn);
		}



		public void ClearReceive()

		{
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint senderep = (EndPoint)sender;

			
			if (connected)
				{
				try
				{
				if (sock.Available > 0)
					sock.ReceiveFrom(tc,tc.Length,SocketFlags.None,ref senderep);
				}
						
						
				catch
				{
				}

				}
		}




		public bool Connected()

		{
			return(connected);
		}



		public void Close()

		{
			if (connected)
				{
				sock.Close();
				connected = false;
				}
		}



		public IPEndPoint Server()
		
		{
			return(server_ip_end);
		}



		public IPEndPoint SServer()
		
		{
			return(sserver_ip_end);
		}



		/*	CONSTRUCTOR
			Description:
			Object constructor.
		
			Passed parametes:	none
		
			Returned value:	none
		*/
		
		public UdpClient(string ip_address,int port_no,string server_ip_address,int server_port_no)

		{
			IPAddress ip_addr;
			
			try
				{
				sock = new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
				ip_addr = IPAddress.Parse(ip_address);
				ip_end = new IPEndPoint(ip_addr ,port_no);
				sock.Bind(ip_end);
				ip_addr = IPAddress.Parse(server_ip_address);
				server_ip_end = new IPEndPoint(ip_addr, server_port_no);
				sserver_ip_end = new IPEndPoint(ip_addr, server_port_no + 1);
				connected = true;
				}
				
			catch(Exception ex)
				{
				sock.Close();
				connected = false;
//				MessageBox.Show("Exception: " + ex.Message,"Error");
				Log.Log.LogEntry("UdpClient exception: " + ex.Message);
				Log.Log.LogEntry("          stack trace: " + ex.StackTrace);
				last_error = "UDP open exception " + ex.Message;
				}
		}



		public string LastError()

		{
			return(last_error);
		}

	}
}
