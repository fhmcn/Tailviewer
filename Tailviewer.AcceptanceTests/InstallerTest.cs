﻿using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Tailviewer.AcceptanceTests
{
	[TestFixture]
	public sealed class InstallerTest
		: SystemtestBase
	{
		private string _installationPath;

		[SetUp]
		public void Setup()
		{
			var dir = TestContext.CurrentContext.TestDirectory;
			var testName = TestContext.CurrentContext.Test.Name;
			_installationPath = Path.Combine(dir, testName);
		}

		[Test]
		[Description("Verifies that the graphical installer can start and doesn't exit on its own - also verifies that the log doesn't contain any errors")]
		public void TestRunGraphicalInstaller()
		{
			// We first try to run the graphical installer.
			// If there's a serious problem, the process will exit on its own and the test will fail.
			StartInstaller();

			// However there might be less serious problems which should've then been logged:
			var logFileName = Path.Combine(Constants.AppDataLocalFolder, "Installation.log");
			var logFile = ReadFile(logFileName);
			if (logFile.IndexOf("error", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				Console.WriteLine(logFile);
				Assert.Fail("Expected installer log to not contain any errors");
			}
		}

		private string ReadFile(string logFileName)
		{
			using (var stream = File.Open(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		[Test]
		[Description(
			"Verifies that a fresh install into a completely empty directory yields a runnable tailviewer application")]
		public void TestFreshInstall()
		{
			InstallInto(_installationPath);
			Execute(_installationPath);
		}

		private void StartInstaller()
		{
			var startInfo = new ProcessStartInfo(InstallerPath);
			var process = Process.Start(startInfo);
			try
			{
				if (process.WaitForExit(5000))
				{
					Assert.Fail("Expected process with id {0} to not terminate on its own, but it did with exitCode {1}",
						process.Id,
						process.ExitCode);
				}
			}
			finally
			{
				process.Kill();
			}
		}

		private static void Execute(string installationPath)
		{
			var path = Path.Combine(installationPath, "Tailviewer.exe");
			var process = Process.Start(path);
			const string reason = "because the application shouldn't exit within the first five seconds";
			process.WaitForExit(5000);

			if (process.HasExited)
			{
				PrintLogFile();
				process.HasExited.Should().BeFalse(reason);
			}
			else
			{
				try
				{
					process.Kill();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		private static void PrintLogFile()
		{
			var fname = Path.Combine(Constants.AppDataLocalFolder, "Tailviewer.log");
			Console.WriteLine(File.ReadAllText(fname));
		}
	}
}