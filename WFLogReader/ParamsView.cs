using System;
using System.Collections.Generic;

namespace WFLogReader
{
    public class ParamsView
    {
        public int min;
        public DateTime currentTime;
        public DateTime startTime;
        public string server;
        public string path;
        public List<string> neededLogs;
        public string logName { get; set; }
        public int outputSymb { get; set; }
        public List<string> BlackList { get; set; }

        public ParamsView(string server, DateTime currentTime, DateTime startTime, int min, List<string> chekedLogs, string path)
        {
            this.currentTime = currentTime;
            this.server = server;
            this.min = min;
            this.path = path;
            this.startTime = startTime;
            //this.startTime = currentTime.AddMinutes(-min);

            string pref = string.Empty;
            string tMask = string.Empty;
            string emiasMask = string.Empty;

            neededLogs = new List<string>();
            foreach (string item in chekedLogs)
            {
                if (item == "admin-llo-soap.log" || item == "waiting-list.log")
                {
                    neededLogs.Add(item.Replace(".log", ".") + currentTime.Date.ToString("yyyy-MM-dd") + ".0.log");
                }
                else if (item == "emias.log")
                {
                    if (server == "ТПАК")
                    {
                        neededLogs.Add("temias03_yyyy_MM_dd_hh_mm.log");
                    }
                    else if (server == "КПАК")
                    {
                        neededLogs.Add("kpakemias_yyyy_MM_dd_HH_mm.log");
                        neededLogs.Add("kpakemias01_yyyy_MM_dd_HH_mm.log");
                        neededLogs.Add("kpakemias02_yyyy_MM_dd_HH_mm.log");
                    }
                    else
                    {
                        neededLogs.Add("temias04_yyyy_MM_dd_hh_mm.log");
                    }
                }
                else if (item == "t.log")
                {
                    if (server == "ТПАК")
                    {
                        neededLogs.Add("t3_yyyy_MM_dd_hh_mm.log");
                    }
                    else if (server == "ДО")
                    {
                        neededLogs.Add("t4_yyyy_MM_dd_HH_mm.log");
                    }
                    else
                    {
                    }
                }
                //else if (item == "emias.log" || item == "t.log")
                //{
                //    if (server == "ТПАК")
                //    {
                //        pref = "t";
                //        tMask = "3_yyyy_MM_dd_hh_mm.log";
                //        emiasMask = "3_yyyy_MM_dd_hh_mm.log";
                //    }
                //    else if (server == "КПАК")
                //    {
                //        pref = "kpak";
                //        tMask = "2_yyyy_MM_dd_HH_mm.log";
                //        emiasMask = "2_yyyy_MM_dd_HH_mm.log";
                //    }
                //    else
                //    {
                //        pref = "t";
                //        tMask = "4_yyyy_MM_dd_HH_mm.log";
                //        emiasMask = "4_yyyy_MM_dd_hh_mm.log";
                //    }
                //    if (item == "emias.log")
                //    {
                //        neededLogs.Add(pref + item.Replace(".log", "0") + emiasMask);
                //    }
                //    else
                //    {
                //        neededLogs.Add(item.Replace(".log", "") + tMask);
                //    }
                //}
                else
                {
                    neededLogs.Add(item);
                }
            }
        }

    }
}
