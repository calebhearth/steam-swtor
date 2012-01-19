using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace SteamSWToR
{
	class Program
	{
        /// <summary>
        /// Check that we are running at least Windows 6 (Vista)
        /// </summary>
        /// <returns>True if we are on Windows Vista or higher.</returns>
		static bool IsPreVista()
		{
            return System.Environment.OSVersion.Version.Major < 6;
		}

		static int Main(string[] args)
		{
			// If Operating System is before Vista, then exit
            if (IsPreVista())
            {
                MessageBox.Show("Windows Vista or higher is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

			if (args.Length == 0)
			{
				string pipeName = "swtorsteam";
				
				// run ourself as admin
				try
				{
					Process admin = new Process();
					admin.StartInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
					admin.StartInfo.Arguments = pipeName;
					admin.StartInfo.Verb = "runas";
					admin.Start();
				}
				catch(Exception e)
				{
					string errmsg = e.Message + "\n";
					errmsg += "Failed to escalate. Program will now exit.";
					MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return 1;
				}

				// run the swtor launcher
				try
				{
					Process launcher = new Process();
					launcher.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\launcher.exe";
					launcher.Start();
				}
				catch(Exception e)
				{
					string errmsg = e.Message + "\n";
					errmsg += "Launcher failed to begin. Is this exe in SWTOR's home directory? Program will now exit.";
					MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return 1;
				}

				// grab data from the commandline arguments
				string exe, arguments, workingdirectory;
				try
				{
					NamedPipeServerStream server = new NamedPipeServerStream(pipeName);
					server.WaitForConnection();

					StreamReader sr = new StreamReader(server);
					string cmdline = sr.ReadLine();

					// grab data from the commandline arguments
					exe = cmdline.Substring(1, cmdline.IndexOf('"', 1) - 1);
					arguments = cmdline.Substring(cmdline.IndexOf("\" ") + 2);
					workingdirectory = cmdline.Substring(1, cmdline.IndexOf("swtor.exe") - 1);
				}
				catch(Exception e)
				{
					string errmsg = e.Message + "\n";
					errmsg += "Failed to read command line arguments from other program. Program will now exit.";
					MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return 1;
				}

				try
				{
					// start swtor's client
					Process swtor = new Process();
					swtor.StartInfo.FileName = exe;
					swtor.StartInfo.Arguments = arguments;
					swtor.StartInfo.WorkingDirectory = workingdirectory;
					swtor.Start();
				}
				catch(Exception e)
				{
					string errmsg = e.Message + "\n";
					errmsg += "swtor.exe failed to begin. Is this exe in SWTOR's home directory? Program will now exit.";
					MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return 1;
				}

				// exit the program
				return 0;
			}
			else
			{
				// Connect to pipe server
				string pipeName = args[0];
				NamedPipeClientStream client = new NamedPipeClientStream(pipeName);
				try
				{
					client.Connect(10000);
				}
				catch (Exception e)
				{
					string errmsg = e.Message + "\n";
					errmsg += "Failed to connect to other program. Program will now exit.";
					MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return 1;
				}
				
				//Create the query
				ObjectQuery query = new ObjectQuery("Select * from Win32_Process Where Name =\"swtor.exe\"");

				while (true)
				{
					ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
					ManagementObjectCollection processList = searcher.Get();

					foreach (ManagementObject obj in processList)
					{
						string cmdline = obj.GetPropertyValue("CommandLine").ToString();

						if (cmdline.Contains("username"))
						{
							// kill the process
							obj.InvokeMethod("Terminate", null);

							// write command line to the pipe
							try
							{
								StreamWriter sw = new StreamWriter(client);
								sw.AutoFlush = true;
								sw.WriteLine(cmdline);
							}
							catch(Exception e)
							{
								string errmsg = e.Message + "\n";
								errmsg += "Failed to write commandline arguments to pipe. Program will now exit.";
								MessageBox.Show(errmsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
								return 1;
							}

							// exit the program
							return 0;
						}
					}

					Thread.Sleep(1000);
				}
			}
		}
	}
}