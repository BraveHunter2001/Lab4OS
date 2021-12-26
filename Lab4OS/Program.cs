using Lab4OS.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4OS
{
    class Program
    {

		static IntPtr StartRWProcess(string executableFile, string targetFileName)
		{
			StartupInfo startupInfo = new StartupInfo();
			SECURITY_ATTRIBUTES securAttr = new SECURITY_ATTRIBUTES()
			{
				length = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES)),
				inheritHandle = true,
				securityDescriptor = IntPtr.Zero
			};
			var logFileHandle = Funcs.CreateFile(targetFileName,
				(uint)DesiredAccess.GENERIC_WRITE,
				(uint)ShareMode.FILE_SHARE_WRITE,
				securAttr,
				(uint)CreationDisposition.CREATE_ALWAYS,
				(uint)FileAttributes.FILE_ATTRIBUTE_NORMAL,
				IntPtr.Zero
			);

			startupInfo.cb = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(StartupInfo));
			startupInfo.hStdOutput = logFileHandle;
			startupInfo.hStdError = IntPtr.Zero;
			startupInfo.hStdInput = IntPtr.Zero;
			startupInfo.dwFlags |= (uint)STARTF.USESTDHANDLES;

			ProcessInfo procInfo = new ProcessInfo();
			bool mainProcess = Funcs.CreateProcess(executableFile,
				null,
				IntPtr.Zero,
				IntPtr.Zero,
				true,
				0,
				IntPtr.Zero,
				null,
				ref startupInfo,
				out procInfo);
			if (mainProcess)
				return procInfo.hProcess;
			else
				Funcs.PrintErrorIfExists();
			return IntPtr.Zero;
		}

		static string GetTime() => DateTime.Now.ToString("mm:ss:fff");

		unsafe static uint WriteLog(string str, IntPtr outHandle)
		{
			var outputStr = Encoding.UTF8.GetBytes(str);
			Funcs.WriteFile(outHandle, outputStr, (uint)outputStr.Length, out uint writtenBytes, null);
			return writtenBytes;
		}
		static void Main(string[] args)
        {
            Console.WriteLine("Start");
			const int pageSize = 4096;
			const int pageCount = 15;
			const int fileSize = pageCount * pageSize;
			const int processCount = 12;
			const int hProcessCount = processCount / 2;

			IntPtr[] writeSem = new IntPtr[pageSize];
			IntPtr[] readSem = new IntPtr[pageSize];

			var ioMutex = Funcs.CreateMutex(IntPtr.Zero, false, "IOMutex");

			var fileHandle = Funcs.CreateFile(@"C:\1\startfile.txt",
				(uint)DesiredAccess.GENERIC_READ | (uint)DesiredAccess.GENERIC_WRITE,
				(uint)ShareMode.FILE_SHARE_READ | (uint)ShareMode.FILE_SHARE_WRITE,
				null,
				(uint)CreationDisposition.CREATE_ALWAYS,
				0,
				IntPtr.Zero);

			WriteLog(GetTime(),fileHandle);

			var fileMapping = Funcs.CreateFileMapping(fileHandle, null, FileMapProtection.PageReadWrite, 0, (uint)fileSize, Constants.mappingName);

			Funcs.PrintErrorIfExists();
			List<IntPtr> procHandles = new List<IntPtr>();

			for (int i = 0; i < pageCount; i++)
			{
				string semName = "write_sem_" + i.ToString();
				writeSem[i] = Funcs.CreateSemaphore(IntPtr.Zero,1,1,semName);
					

				semName = "read_sem_" + i.ToString();

				readSem[i] = Funcs.CreateSemaphore(IntPtr.Zero, 1, 1, semName);
			}

			for (int i = 0; i < hProcessCount; i++)
			{
				string logName = @"C:\1\writeLogs\writeLog_" + i.ToString() + ".txt";
				procHandles.Add(StartRWProcess(@"C:\Users\Илья\source\repos\Lab4OS\Reader\bin\Debug\Reader.exe", logName));
			}

			for (int i = 0; i < hProcessCount; i++)
			{
				string logName = "C:\\1\\readLogs\\readLog_" + i.ToString() + ".txt";
				procHandles.Add(StartRWProcess(@"C:\Users\Илья\source\repos\Lab4OS\Writer\bin\Debug\Writer.exe", logName));
			}

            Console.WriteLine("Waiting...");
			Funcs.WaitForMultipleObjects((uint)procHandles.Count, procHandles.ToArray(), true, uint.MaxValue);
            Console.WriteLine("Done");

			foreach (var h in procHandles)
				Funcs.CloseHandle(h);

			Funcs.CloseHandle(fileMapping); 
			Funcs.CloseHandle(fileHandle);
			Funcs.CloseHandle(ioMutex);
			for (int i = 0; i < pageCount; i++)
			{
				Funcs.CloseHandle(writeSem[i]);
				Funcs.CloseHandle(readSem[i]);
			}
			Console.WriteLine("All processes finished.");
			Console.ReadKey();
		}
	}
}
