using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawerProcess
{
    public struct LogProcTimes
    {
        public int takeSem;
        public int takeMut;
        public int work;
        public int freeMut;
        public int freeSem;
    }

    public class LogProcess
    {
        private string Name { get; }
        private List<LogProcTimes> Records { get; }

        public LogProcess(string name, List<LogProcTimes> records)
        {
            this.Name = name;
            this.Records = records;
        }
    }

    public class Parcer
    {
        private static Parcer instance;
        private readonly string path;
        private readonly int startTime;
        private readonly int countFiles;
        private List<LogProcess> ReadProcesses { get; }
        private List<LogProcess> WriteProcesses { get; }

        private Parcer(string path, int countFiles)
        { 
            this.path = path;
            this.countFiles = countFiles;
            startTime = GetStartTimeByFile(path + @"\startfile.txt");
            ReadProcesses = GetLogs("Read", $@"{path}\readLogs\readLog");
            WriteProcesses = GetLogs("Write", $@"{path}\writeLogs\writeLog");

        }
        public static Parcer GetInstance(string path, int countFiles)
        {
            if (instance == null)
                instance = new Parcer(path, countFiles);
            return instance;
        }        
        private int ParceTime(string time)
        {
            string[] t = time.Split(':');

            return (int.Parse(t[0])*60 + int.Parse(t[1]) )* 1000 + int.Parse(t[2]);

        }
        
        private List<LogProcTimes> GetDataByFile(string pathLog_i) 
        {

            List<string> marks = new List<string>();
            List<LogProcTimes> recordings = new List<LogProcTimes>();

            using (StreamReader sr = new StreamReader(pathLog_i))
            {
                string markPr;
                while ((markPr = sr.ReadLine()) != null)
                {
                    marks.Add(markPr);
                    
                }
            }

            LogProcTimes log = new LogProcTimes();

            foreach (string m in marks)
            {
                
                if (!m.Equals(""))
                {
                    string[] temp = m.Split('|');

                    if (temp[0].Equals("TAKE"))
                    {
                        if (temp[1].Equals("Semaphore"))
                        {
                            log.takeSem = ParceTime(temp[2]) - startTime;
                        }
                        if (temp[1].Equals("Mutex"))
                        {
                            log.takeMut = ParceTime(temp[2]) - startTime;
                        }
                    }

                    else if(temp[0].Equals("Read") || temp[0].Equals("WRITE"))
                    { 
                        log.work = ParceTime(temp[2]) - startTime;

                    }

                    else if (temp[0].Equals("FREE"))
                    {
                        if (temp[1].Equals("Semaphore"))
                        {
                            log.freeSem = ParceTime(temp[2]) - startTime;
                        }
                        if (temp[1].Equals("Mutex"))
                        {
                            log.freeMut = ParceTime(temp[2]) - startTime;
                        }
                    }

                }

                if (m.Equals(""))
                {
                    recordings.Add(log);
                    log = new LogProcTimes();
                }

            
            }

            return recordings;

        }
        private int GetStartTimeByFile(string pathStartFile)
        {
            string t;
            using (StreamReader sr = new StreamReader(pathStartFile))
            {
                t = sr.ReadLine();
               
            }
            return ParceTime(t);
        }

        private List<LogProcess> GetLogs(string name, string pathFile)
        {
            List<LogProcess> res = new List<LogProcess>();
            for (int i = 0; i < countFiles; i++)
            {
                res.Add(new LogProcess($"{name}[{i}]", GetDataByFile($@"{pathFile}_{i}.txt")));
            }

            return res;
        }

        

    }
}
