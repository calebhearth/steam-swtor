using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace steamswtor
{
	class Program
	{
		static bool IsPreVista()
		{
			//Vista or higher check
			if (System.Environment.OSVersion.Version.Major < 6)
			{
				MessageBox.Show("Windows Vista or higher is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return true;
			}

			return false;
		}

		static void Main(string[] args)
		{
			// If Operating System is before Vista, then exit
			if (IsPreVista())
			return;

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
					return;
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
					return;
				}

				// loop waiting for our temp file to be filled with swtor's commandline arguments
				Console.WriteLine("Waiting for our other program to finish...");

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
					return;
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
					return;
				}

				// exit the program
				return;
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
					return;
				}
				
				//Create the query
				ObjectQuery query = new ObjectQuery("Select * from Win32_Process Where Name =\"swtor.exe\"");

				// check once a second for swtor.exe that the launcher starts when the user hit's play in the launcher
				Console.WriteLine("Waiting for launcher to start swtor...");
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
								return;
							}

							// exit the program
							return;
						}
					}

					Thread.Sleep(1000);
				}
			}
		}
	}
}