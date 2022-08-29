using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;


namespace Log
	{

	public static class Log
		{

		private const string DATA_SUB_DIR = "\\data\\";

		private static TextWriter ltw = null;
		private static int no_writers = 0;
		private static Stopwatch sw = new Stopwatch();
		private static string log_dir = "";
		private static bool ts;
		private static object lock_obj = new object();


		public static void OpenLog(string file,bool timestamps)

		{
			DateTime now = DateTime.Now;

			if (ltw == null)
				{
				ts = timestamps;
				log_dir = Application.StartupPath +DATA_SUB_DIR + "\\" + now.Month + "." + now.Day + "." + now.Year + " " + now.Hour + "." + now.Minute + "\\";
				if (!Directory.Exists(log_dir))
					Directory.CreateDirectory(log_dir);
				ltw = File.CreateText(log_dir + file);
				if (ltw != null)
					{
					ltw.WriteLine(file);
					ltw.WriteLine(now.ToShortDateString() + " " + now.ToShortTimeString());
					ltw.WriteLine();
					ltw.Flush();
					sw.Restart();
					no_writers = 1;
					}
				}
			else
				no_writers += 1;
		}



		public static void CloseLog()

		{
			lock(lock_obj)
			{
			if ((ltw != null) && (no_writers == 1))
				{
				ltw.Close();
				sw.Stop();
				ltw = null;
				no_writers = 0;
				}
			else if (ltw != null)
				no_writers -= 1;
			}
		}



		public static void LogEntry(string entry)

		{
			lock(lock_obj)
			{
			if (ltw != null)
				{
				if (ts)
					ltw.Write(sw.ElapsedMilliseconds.ToString() + " ");
				ltw.WriteLine(entry);
				ltw.Flush();
				}
			}
		}



		public static void LogArrayList(string title,ArrayList al)

		{
			int i;

			lock(lock_obj)
			{
			if (ltw != null)
				{
				LogEntry(title);
				for (i = 0;i < al.Count;i++)
					{
					ltw.Write("\t" + i);
					ltw.WriteLine("\t" + al[i].ToString());
					}
				ltw.Flush();
				}
			}
		}



		public static void KeyLogEntry(string entry)

		{
			lock(lock_obj)
			{
			if (ltw != null)
				{
				ltw.WriteLine();
				if (ts)
					ltw.Write(sw.ElapsedMilliseconds.ToString() + " ");
				ltw.WriteLine(entry.ToUpper());
				ltw.WriteLine();
				ltw.Flush();
				}
			}
		}



		public static bool LogOpen()

		{
			bool rtn = false;

			if (ltw != null)
				rtn = true;
			return(rtn);
		}



		public static string LogDir()

		{
			return(log_dir);
		}

		}
	}
