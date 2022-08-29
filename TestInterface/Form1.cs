using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Log;
using ServerInterface;
using Renci.SshNet;

namespace TestInterface
	{
	public partial class Form1 : Form
		{

		private const string HOST = "O1";
		private const int SERVER_PORT = 60000;
		private const string USER = "terry";
		private const string PASSWORD = "O1th51919";
		private const string RUN_OD_SERVER = "bash /home/terry/server/runserver.sh";

		private const int HAND_ID = 1;
		private const int SHAFT_ID = 2;
		private const int RED_BOX_ID = 1;
		private const int BLUE_BOX_ID = 2;
		private const int END_BLOCK_ID = 1;
		private const int OIH_SHAFT_ID = 1;
		private const int OIH_END_BLOCK_ID = 2;
		private const int CALIBRATE_ID = 1;
		private const string OD_MODELS_FILE = "odmodels.csv";
		private const int MAX_CPT_PIXEL_DISTANCE = 17;
		private const string SERVER_DIR = "V:\\server\\pics\\";
		private const int OD_SERVER_LOAD_WAIT = 6000;	//max recorded load time ~ 5.2 sec


		private struct accum_data
		{
			public int inferences;
			public int actual;
			public int detected;
			public int bad_detect;
			public ArrayList cpl;
		};

		private struct batch_accum_data
		{
			public int actual;
			public int detected;
			public int bad_detect;
			public int missed;
		};

		private static int batch = 1;
		private Pen[] pens = { System.Drawing.Pens.Red, System.Drawing.Pens.Blue, System.Drawing.Pens.Brown,
										 System.Drawing.Pens.Orange, System.Drawing.Pens.Black, System.Drawing.Pens.DarkViolet,
										 System.Drawing.Pens.DarkGreen, System.Drawing.Pens.DarkGray,System.Drawing.Pens.Gold,
										 System.Drawing.Pens.DeepPink,System.Drawing.Pens.DarkMagenta,System.Drawing.Pens.DarkOliveGreen,
										 System.Drawing.Pens.GreenYellow};
		private string[] targets = { "hand","hd hand", "containers", "parts", "objects in hand","calibrate","people"};
		private SortedList actuals;
		private bool all_models_loaded = false;
		private static SshClient client = null;
		private static Thread od_server = null;


		public Form1()

		{
			IPAddress[] ipa;
			string server_ip_address = "";
			string my_ip_address = "";
			string rsp,msg;

			InitializeComponent();
			Log.Log.OpenLog("Orin server interface.log", false);

			try
			{
			ipa = Dns.GetHostAddresses(Dns.GetHostName());
			foreach (IPAddress ip in ipa)
				{
				if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
					{
					my_ip_address = ip.ToString();
					break;
					}
				}
			ipa = Dns.GetHostAddresses(HOST);
			foreach (IPAddress ip in ipa)
				{
				if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
					{
					server_ip_address = ip.ToString();
					break;
					}
				}
			}

			catch (Exception ex)
			{
			msg = "Exception: " + ex.Message;
			Log.Log.LogEntry(msg);
			StatusTextBox.AppendText(msg + "\r\n");
			}

			if ((server_ip_address.Length > 0) && (my_ip_address.Length > 0) && StartServer())
				{
				if (DeviceComm.Open(my_ip_address,SERVER_PORT - 1,server_ip_address,SERVER_PORT))
					{
					Thread.Sleep(OD_SERVER_LOAD_WAIT);
					rsp = DeviceComm.SendCommand("hello", 100);
					if (rsp.StartsWith("OK"))
						{
						msg = "Server connection established";
						Log.Log.LogEntry(msg);
						StatusTextBox.AppendText(msg + "\r\n");
						this.Enabled = true;
						foreach (string name in targets)
							{
							ModelComboBox.Items.Add(name);
							}
						}
					else
						{
						msg = "Server connection could not be established.";
						Log.Log.LogEntry(msg);
						StatusTextBox.AppendText(msg + "\r\n");
						DeviceComm.Close();
						this.Enabled = false;
						}
					}
				}
			else
				this.Enabled = false;
		}



		private bool StartServer()

		{
			bool rtn = false;
			bool ssh_connect_done = false;
			int tries = 0;
			int try_limit = 2;

			if (HOST.Length > 0)
				{
				client = new SshClient(HOST, USER, PASSWORD);
				do
					{

					try
						{
						client.Connect();
						ssh_connect_done = true;
						rtn = true;
						}

					catch (Exception)
						{
						tries += 1;
						if (tries < try_limit)
							{
							Thread.Sleep(1000);
							}
						else
							ssh_connect_done = true;
						}

					}
				while (!ssh_connect_done);
				Log.Log.LogEntry("No ssh connection tries: " + (tries + 1));
				if (client.IsConnected)
					{
					Log.Log.LogEntry("SSH connection open");
					od_server = new Thread(RunODServer);
					od_server.Start();
					Thread.Sleep(1000);
					if (od_server.IsAlive)
						rtn = true;
					}
				else
					Log.Log.LogEntry("Could not connect ssh client.");
				}
			else
				Log.Log.LogEntry("Host not available.");
			return (rtn);

		}



		private static void RunODServer()

		{
			SshCommand cmd = null;

			try
			{
			cmd = client.RunCommand(RUN_OD_SERVER);
			}

			catch(Exception ex)
			{
			Log.Log.LogEntry("RunODServer exception: " + ex.Message);
			Log.Log.LogEntry("Stack trace: " + ex.StackTrace);
			if (cmd != null)
				Log.Log.LogEntry("command: " + cmd.CommandText + "  result: " + cmd.Result);
			}
		}



		private static void CloseODServer()

		{
			if ((client != null) && (od_server != null) && (od_server.IsAlive))
				{
				DeviceComm.SendCommand("exit", 100);
				od_server.Join();
				client.Disconnect();
				}
		}



		private bool ModelLoad(string def)

		{
			string rsp;
			bool rtn = false;

			rsp = DeviceComm.SendCommand("load," + def,10000);
			if (rsp.StartsWith("OK"))
				rtn = true;
			return(rtn);
		}



		private bool Unload()

		{
			string rsp;
			bool rtn = false;

			rsp = DeviceComm.SendCommand("unload",1000);
			if (rsp.StartsWith("OK"))
				rtn = true;
			return(rtn);
		}



		private bool LoadModel(string name)

		{
			bool rtn = false;
			string fname,line,msg,nline;
			TextReader tr;
			string[] data;
			Stopwatch sw = new Stopwatch();

			fname = Application.StartupPath + "\\" + OD_MODELS_FILE;
			if (File.Exists(fname))
				{
				tr = File.OpenText(fname);
				while ((line = tr.ReadLine()) != null)
					{
					if (line.StartsWith(name))
						{
						data = line.Split(',');
						if (data.Length == 3)
							{
							nline = data[0] + "," + Application.StartupPath + "\\," + data[1] + "," + data[2];
							sw.Start();
							if (ModelLoad(nline))
								{
								sw.Stop();
								msg = "  loaded OD model " + line + " in " + sw.ElapsedMilliseconds + " ms";
								Log.Log.LogEntry(msg);
								StatusTextBox.AppendText(msg + "\r\n");
								rtn = true;
								break;
								}
							else
								{
								msg = "  Attempt to load " + line + " failed.";
								Log.Log.LogEntry(msg);
								StatusTextBox.AppendText(msg + "\r\n");
								break;
								}
							}
						else
							{
							msg = "  Detected format error in " + OD_MODELS_FILE;
							Log.Log.LogEntry(msg);
							StatusTextBox.AppendText(msg + "\r\n");
							break;
							}
						}
					}
				if (line == null)
					{
					msg = "Could not find model.";
					Log.Log.LogEntry(msg);
					StatusTextBox.AppendText(msg + "\r\n");
					}
				}
			return (rtn);
		}



		private bool GetTagData(string filename)

		{
			bool rtn = false;
			XmlReader reader;
			string name;
			int i,xmax,xmin,ymax,ymin;
			accum_data ad;
			Point cpt = new Point();

			actuals = new SortedList();
			if (File.Exists(filename))
				{
				reader = XmlReader.Create(filename);
				while (reader.ReadToFollowing("name"))
					{
					reader.Read();
					name = reader.Value;
					object obj = actuals[name];
					if (obj != null)
						{
						ad = (accum_data) obj;
						ad.actual += 1;
						reader.ReadToFollowing("xmin");
						reader.Read();
						xmin = int.Parse(reader.Value);
						reader.ReadToFollowing("ymin");
						reader.Read();
						ymin = int.Parse(reader.Value);
						reader.ReadToFollowing("xmax");
						reader.Read();
						xmax = int.Parse(reader.Value);
						reader.ReadToFollowing("ymax");
						reader.Read();
						ymax = int.Parse(reader.Value);
						cpt.X = (xmax + xmin) / 2;
						cpt.Y = (ymax + ymin) / 2;
						ad.cpl.Add(cpt);
						i = actuals.IndexOfKey(name);
						actuals.SetByIndex(i,ad);
						}
					else
						{
						ad = new accum_data();
						ad.cpl = new ArrayList();
						ad.actual = 1;
						reader.ReadToFollowing("xmin");
						reader.Read();
						xmin = int.Parse(reader.Value);
						reader.ReadToFollowing("ymin");
						reader.Read();
						ymin = int.Parse(reader.Value);
						reader.ReadToFollowing("xmax");
						reader.Read();
						xmax = int.Parse(reader.Value);
						reader.ReadToFollowing("ymax");
						reader.Read();
						ymax = int.Parse(reader.Value);
						cpt.X = (xmax + xmin)/2;
						cpt.Y = (ymax + ymin)/2;
						ad.cpl.Add(cpt);
						actuals.Add(name,ad);
						}
					}
				}
			return(rtn);
		}



		private int Distance(Point pt1,Point pt2)

		{
			return((int) Math.Round(Math.Sqrt(Math.Pow(pt1.X - pt2.X,2) + Math.Pow(pt1.Y - pt2.Y,2))));
		}



		private void LOMButton_Click(object sender, EventArgs e)

		{
			Stopwatch sw = new Stopwatch();
			string msg;

			sw.Start();
			foreach (string name in targets)
				{
				LoadModel(name);
				Application.DoEvents();
				}
			sw.Stop();
			msg = "Load time: " + sw.ElapsedMilliseconds + " ms.";
			Log.Log.LogEntry(msg);
			StatusTextBox.AppendText(msg + "\r\n");
			if (ULUCheckBox.Checked)
				Unload();
		}



		private void RIButton_Click(object sender, EventArgs e)

		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			string fext,detect_target = "",rsp,fname;
			string[] files;
			Stopwatch sw = new Stopwatch(),sw2 = new Stopwatch();
			Bitmap bm;
			string[] boxes;
			int i,actual = 0,detected = 0,bad_detected = 0,missed_detect = 0,dist,min_dist;
			Graphics g;
			string[] values;
			int x, y, w, h, p,j,no_files = 0,id,indx, lowest_prob = 100,pos;
			long total_if_time = 0;
			string name, stat,sfname;
			object obj;
			accum_data ad;
			batch_accum_data bad;
			SortedList batchlist;
			Point cpt = new Point();
			SortedList missed = new SortedList();

			RIButton.Enabled = false;
			StatusTextBox.Clear();
			Application.DoEvents();
			fbd.RootFolder = Environment.SpecialFolder.MyComputer;
			fbd.SelectedPath = Application.StartupPath;
			if (fbd.ShowDialog() == DialogResult.OK)
				{
				batchlist = new SortedList();
				Log.Log.LogEntry("\r\nBATCH " + batch);
				StatusTextBox.AppendText("BATCH " + batch + "\r\n");
				fext = "jpg";
				detect_target = ModelComboBox.Text;
				if ((detect_target.Length > 0) && (all_models_loaded || LoadModel(detect_target)))
					{
					Log.Log.LogEntry("  detection target: " + detect_target);
					StatusTextBox.AppendText("  detection target: " + detect_target + "\r\n");
					Log.Log.LogEntry("  probability threshold: " + PTNumericUpDown.Value);
					StatusTextBox.AppendText("  probability threshold: " + PTNumericUpDown.Value + "\r\n");
					files = Directory.GetFiles(fbd.SelectedPath);
					foreach (string pfname in files)
						{
						if (pfname.EndsWith(fext))
							{
							no_files+= 1;
							Log.Log.LogEntry("\r\n  " + pfname);
							StatusTextBox.AppendText("\r\n  " + pfname + "\r\n");
							GetTagData(pfname.Replace(fext, "xml"));
							bm = new Bitmap(pfname);
							sw.Restart();
							pos = pfname.LastIndexOf("\\");
							name = pfname.Substring(pos + 1);
							sfname = SERVER_DIR + name;
							sw2.Restart();
							File.Copy(pfname,sfname,true);
							sw2.Stop();
							if ((detect_target == "hand") || (detect_target == "hd hand") || (detect_target == "calibrate") || (detect_target == "people"))
								rsp = DeviceComm.SendCommand(detect_target + "," + name + "," + PTNumericUpDown.Value + ",1", 1000);
							else
								rsp = DeviceComm.SendCommand(detect_target + "," + name + "," + PTNumericUpDown.Value + ",0", 1000);
							sw.Stop();
							total_if_time += sw.ElapsedMilliseconds;
							StatusTextBox.AppendText("  inference time (msec): " + sw.ElapsedMilliseconds + "\r\n");
							Log.Log.LogEntry("  inference time (msec): " + sw.ElapsedMilliseconds);
							Log.Log.LogEntry("  file copy time (msec): " + sw2.ElapsedMilliseconds);
							if (rsp.StartsWith("OK"))
								{
								StatusTextBox.AppendText("  response length:" + rsp.Length + "\r\n");
								Log.Log.LogEntry("  response length:" + rsp.Length);
								g = Graphics.FromImage(bm);
								boxes = rsp.Split('[');
								Log.Log.LogEntry("  " + (boxes.Length - 1) + "  detections");
								StatusTextBox.AppendText("  " + (boxes.Length - 1) + "  detections\r\n");
								for (i = 1; i < boxes.Length; i++)
									{
									name = "";
									x = y = w = h = 0;
									values = boxes[i].Split(',');
									if (values.Length == 5)
										{
										p = int.Parse(values[0]);
										x = int.Parse(values[1]);
										y = int.Parse(values[2]);
										w = int.Parse(values[3]);
										values[4] = values[4].Substring(0, values[4].Length - 1);
										h = int.Parse(values[4]);
										g.DrawRectangle(Pens.Red, x, y, w, h);
										Log.Log.LogEntry("  [" + p + "]  " + x + "," + y + "," + w + "," + h);
										StatusTextBox.AppendText("  [" + p + "]  " + x + "," + y + "," + w + "," + h + "\r\n");
										if ((detect_target == "hand") || (detect_target == "hd hand"))
											name = "hand";
										else if (detect_target == "calibrate")
											name = "calibrate";
										if (p < lowest_prob)
											lowest_prob = p;
										}
									else if (values.Length == 6)
										{
										p = int.Parse(values[0]);
										id = int.Parse(values[1]);
										x = int.Parse(values[2]);
										y = int.Parse(values[3]);
										w = int.Parse(values[4]);
										values[5] = values[5].Substring(0, values[5].Length - 1);
										h = int.Parse(values[5]);
										name = "?";
										if (detect_target == "parts")
											{
											if (id == SHAFT_ID)
												name = "shaft";
											else if (id == END_BLOCK_ID)
												name = "end block";
											}
										else if (detect_target == "objects in hand")
											{
											if (id == OIH_SHAFT_ID)
												name = "shaft";
											else if (id == OIH_END_BLOCK_ID)
												name = "end block";
											}
										else if (detect_target == "containers")
											{
											if (id == RED_BOX_ID)
												name = "red box";
											else if (id == BLUE_BOX_ID)
												name = "blue box";
											}
										else
											name = id.ToString();
										g.DrawRectangle(pens[id % pens.Length], x, y, w, h);
										Log.Log.LogEntry("  [" + p + "]  " + "( " + name + ") " + x + "," + y + "," + w + "," + h);
										StatusTextBox.AppendText("  [" + p + "]  " + "( " + name + ") " + pens[id % pens.Length].Color + "  " + x + "," + y + "," + w + "," + h + "\r\n");
										if (p < lowest_prob )
											lowest_prob = p;
										}
									else
										{
										Log.Log.LogEntry("  " + boxes[i].Substring(0, boxes[i].Length - 1));
										StatusTextBox.AppendText("  " + boxes[i].Substring(0, boxes[i].Length - 1) + "\r\n");
										}
									obj = actuals[name];
									if (obj != null)
										{
										ad = (accum_data)obj;
										bool match = false;
										cpt.X = x + (int)Math.Round((double)w / 2);
										cpt.Y = y + (int)Math.Round((double)h / 2);
										min_dist = 100;
										for (j = 0; j < ad.cpl.Count; j++)
											{
											Point pt = (Point)ad.cpl[j];
											dist = Distance(pt, cpt);
											if (dist < MAX_CPT_PIXEL_DISTANCE)
												{
												ad.detected += 1;
												match = true;
												ad.cpl.RemoveAt(j);
												break;
												}
											else if (dist < min_dist)
												min_dist = dist;
											}
										if (!match)
											{
											ad.bad_detect += 1;
											Log.Log.LogEntry("  no location match (min dist " + min_dist + ")");
											}
										ad.inferences += 1;
										id = actuals.IndexOfKey(name);
										actuals.SetByIndex(id, ad);
										}
									else
										{
										Log.Log.LogEntry("no actuals available");
										}
									}
								for (i = 0; i < actuals.Count; i++)
									{
									int miss;

									name = (string)actuals.GetKey(i);
									ad = (accum_data)actuals.GetByIndex(i);
									if (ad.actual > ad.inferences)
										miss = ad.actual - ad.inferences;
									else
										miss = 0;
									stat = "  " + name + ":  actual " + ad.actual + "  detected " + ad.detected + "  bad detection " + ad.bad_detect + "   missed " + miss;
									Log.Log.LogEntry(stat);
									StatusTextBox.AppendText(stat + "\r\n");
									obj = batchlist[name];
									if (obj != null)
										{
										bad = (batch_accum_data) obj;
										bad.actual += ad.actual;
										bad.detected += ad.detected;
										bad.bad_detect += ad.bad_detect;
										bad.missed += (miss);
										batchlist[name] = bad;
										}
									else
										{
										bad = new batch_accum_data();
										bad.actual = ad.actual;
										bad.detected = ad.detected;
										bad.bad_detect = ad.bad_detect;
										bad.missed = miss;
										batchlist.Add(name, bad);
										}
									}
								for (i = 0; i < missed.Count; i++)
									{
									name = (string)missed.GetKey(i);
									int no = (int)missed.GetByIndex(i);
									stat = "  " + name + ":  missed " + no;
									Log.Log.LogEntry(stat);
									StatusTextBox.AppendText(stat + "\r\n");
									obj = batchlist[name];
									if (obj != null)
										{
										bad = (batch_accum_data)obj;
										bad.missed += no;
										batchlist[name] = bad;
										}
									else
										{
										bad = new batch_accum_data();
										bad.missed = no;
										batchlist.Add(name,bad);
										}
									}
								}
							else
								{
								if (actuals.Count > 0)
									{
									for (i = 0; i < actuals.Count; i++)
										{
										name = (string) actuals.GetKey(i);
										ad = (accum_data) actuals.GetByIndex(i);
										stat = "  " + name + ": actual " + ad.cpl.Count + "   detected 0   bad detection 0   missed " + ad.cpl.Count;
										Log.Log.LogEntry(stat);
										StatusTextBox.AppendText(stat + "\r\n");
										obj = batchlist[name];
										if (obj != null)
											{
											bad = (batch_accum_data)obj;
											bad.actual += ad.cpl.Count;
											bad.missed += ad.cpl.Count;
											batchlist[name] = bad;
											}
										else
											{
											bad = new batch_accum_data();
											bad.actual = ad.cpl.Count;
											bad.missed += ad.cpl.Count;
											batchlist.Add(name, bad);
											}
										}
									}
								}
							indx = pfname.LastIndexOf("\\");
							fname = pfname.Substring(indx + 1);
							if (fext == "bmp")
								fname = fname.Replace("bmp", "jpg");
							bm.Save(Log.Log.LogDir() + "b" + batch + " " + fname, System.Drawing.Imaging.ImageFormat.Jpeg);
							Log.Log.LogEntry("  Saved " + "b" + batch + " " + fname);
							}
						}
					Log.Log.LogEntry("\r\n  Summary");
					Log.Log.LogEntry("  average inference time: " + ((double) total_if_time/no_files).ToString("F1") + " ms");
					batch += 1;
					for (i = 0; i < batchlist.Count; i++)
						{
						fname = (string)batchlist.GetKey(i);
						bad = (batch_accum_data) batchlist.GetByIndex(i);
						actual += bad.actual;
						detected += bad.detected;
						bad_detected += bad.bad_detect;
						missed_detect += bad.missed;
						stat = "  " + fname + ":  actual " + bad.actual + "   detected " + bad.detected + "   bad detections " + bad.bad_detect + "   missed " + bad.missed;
						Log.Log.LogEntry(stat);
						StatusTextBox.AppendText(stat + "\r\n");
						}
					stat = "  total:  actual " + actual + "   detected " + detected + "   bad detections " + bad_detected + "   missed " + missed_detect;
					Log.Log.LogEntry(stat);
					StatusTextBox.AppendText(stat + "\r\n");
					stat = "  detection rate " + ((double) detected/actual).ToString("F4") + "   bad detection rate " + ((double) bad_detected/(detected + bad_detected)).ToString("F4") + "   missed rate " + ((double)missed_detect / (detected + missed_detect)).ToString("F4");
					Log.Log.LogEntry(stat);
					StatusTextBox.AppendText(stat + "\r\n");
					stat = "  lowest probability: " + lowest_prob;
					Log.Log.LogEntry(stat);
					StatusTextBox.AppendText(stat + "\r\n");
					if (!all_models_loaded)
						Unload();
				}
			RIButton.Enabled = true;
			}

		}




		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
			
		{
			CloseODServer();
		}

		}
	}
