using Lab4OS;
using Lab4OS.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader
{
    class Reader
    {
        static string GetTime() => DateTime.Now.ToString("mm:ss:fff");

        unsafe static uint WriteLog(string str, IntPtr outHandle)
        {
            var outputStr = Encoding.UTF8.GetBytes(str);
            Funcs.WriteFile(outHandle, outputStr, (uint)outputStr.Length, out uint writtenBytes, null);
            return writtenBytes;
        }

        static void Main(string[] args)
        {
            const int pageSize = 4096;
            const int pageCount = 15;

            IntPtr[] writeSem = new IntPtr[pageSize];
            IntPtr[] readSem = new IntPtr[pageSize];


            IntPtr IOMutex = Funcs.OpenMutex(
                (uint)(MutexAccess.MUTEX_MODIFY_STATE | MutexAccess.SYNCHRONIZE),
                false,
                "IOMutex");

            var fileMap = Funcs.OpenFileMapping((uint)FileMapAccess.Read,
                false,
                Constants.mappingName);

            var view = Funcs.MapViewOfFile(fileMap, (uint)FileMapAccess.Read, 0, 0, (UIntPtr)(pageSize * pageCount));

            var stdOut = Funcs.GetStdHandle(Constants.STD_OUTPUT_HANDLE);

            // чтобы знать в какую страницу мы уже записали и считали
            for (int i = 0; i < pageCount; i++)
            {
                string semName = "write_sem_" + i.ToString();
                writeSem[i] = Funcs.OpenSemaphore((uint)SemaphoreAccess.SEMAPHORE_MODIFY_STATE | (uint)SemaphoreAccess.SYNCHRONIZE,
                    false,
                    semName);

                semName = "read_sem_" + i.ToString();

                readSem[i] = Funcs.OpenSemaphore((uint)SemaphoreAccess.SEMAPHORE_MODIFY_STATE | (uint)SemaphoreAccess.SYNCHRONIZE,
                    false,
                    semName);
            }

            Funcs.VirtualLock(view, (UIntPtr)(pageSize * pageCount));
         
            for (int i = 0; i < 3; i++)
            {
                var page = Funcs.WaitForMultipleObjects(pageCount, readSem, false, Constants.INFINITE);
                
                WriteLog($"TAKE|Semaphore|{GetTime()}" + "\n", stdOut);

                Funcs.WaitForSingleObject(IOMutex, Constants.INFINITE);
                WriteLog($"TAKE|Mutex|{GetTime()}" + "\n", stdOut);

                Funcs.SleepEx((uint)new Random().Next(1000) + 500, false);

                WriteLog($"Read|Page: {page.ToString()}|{GetTime()}" + "\n", stdOut);

                Funcs.ReleaseMutex(IOMutex);
                WriteLog($"FREE|Mutex|{GetTime()}" + "\n", stdOut);

                Funcs.ReleaseSemaphore(writeSem[page], 1, out int foo);
                WriteLog($"FREE|Semaphore|{GetTime()}" + "\n\n", stdOut);

            }

            Funcs.CloseHandle(IOMutex);
            Funcs.CloseHandle(fileMap);
            Funcs.CloseHandle(view);
            Funcs.CloseHandle(stdOut);


        }
    }
}
