using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace WFLogReader
{
    public class Logs
    {
        private string fileName;
        private DateTime time;
        private DateTime startTime;
        private string path;

        public Logs(ParamsView parametres)
        {
            time = parametres.currentTime;
            startTime = parametres.startTime;
            fileName = "{" + parametres.server + "}" + parametres.logName.Replace(".log", "");
            path = parametres.path;
        }

        public string parsLog(string respons)
        {
            string result = "Лог сохранен без учета времени.";
            respons = respons.Replace("\n", "").Replace("&#039;", "\"").Replace("&#034;","\"");
            Regex firstReg = new Regex("<td>(?<Value>.*?)</td>");
            MatchCollection matches = firstReg.Matches(respons);
            List<string> logs = new List<string>();
            for (var i = 1; i < matches.Count; i++)
            {
                logs.Add(matches[i].Value.TrimStart("<td>".ToCharArray()).TrimEnd("</td>".ToCharArray()).Trim().Replace("&lt;", "<").Replace("&gt;", ">"));
            }

            logs = replaceDate(logs);
            logs = getCurrentDateLog(logs);//перепроверить как отрабатывает

            if (hasDatetime(logs))
            {
                logs = getTimedLog(logs);
                result = "Лог сохранен c учетом времени.";
            }
            path = path + "\\" + time.Date.ToString("yyyy-MM-dd");
            Directory.CreateDirectory(path);
            isErrror(logs);
            isWarning(logs);
            isException(logs);
            path = createPath(path);
            if (isNotEmpty(logs))
            {
                //Console.WriteLine(path);
                using (StreamWriter file = new StreamWriter(path))
                {
                    foreach (string line in logs)
                    {
                        file.WriteLine(line);
                    }
                }
            }
            else
            {
                result = "Лог не сохранен - отсутствует информация.";
            }
            return fileName + " - " + result + "\n";
        }

        private List<string> getTimedLog(List<string> logs)
        {
            List<string> resultLogs = new List<string>();
            foreach (string log in logs)
            {
                try
                {
                    DateTime curTime = Convert.ToDateTime(log.Substring(0, 19));
                    if (curTime >= startTime && curTime <= time)
                    {
                        resultLogs.Add(log);
                    }
                }
                catch (Exception) 
                {
                    if (resultLogs.Count != 0)
                    {
                        resultLogs.Add(log);
                    }
                }
            }
            return resultLogs;
        }

        private bool hasDatetime(List<string> logs)
        {
            bool hasTimeFormat = false;
            foreach (string log in logs)
            {
                try
                {
                    DateTime curTime = Convert.ToDateTime(log.Substring(0, 19));
                    hasTimeFormat = true;
                }
                catch (Exception)
                {
                }
            }
            return hasTimeFormat;
        }

        private void isErrror(List<string> logs)
        {
            foreach (string log in logs)
            {
                if (log.Contains("ERROR"))
                {
                    this.fileName = "[Er]" + this.fileName;
                    break;
                }
            }
        }

        private void isWarning(List<string> logs)
        {
            foreach (string log in logs)
            {
                if (log.Contains("WARN"))
                {
                    this.fileName = "[W]" + this.fileName;
                    break;
                }
            }
        }

        private void isException(List<string> logs)
        {
            foreach (string log in logs)
            {
                if (log.Contains("Exception"))
                {
                    this.fileName = "[Ex]" + this.fileName;
                    break;
                }
            }
        }

        private string createPath(string path)
        {
            path = path + "\\" + time.ToString("HH-mm") + " " + this.fileName;
            var count = 0;
            var tmpPath = path;
            while (File.Exists(tmpPath + ".txt"))
            {
                count++;
                tmpPath = path + " " + count;
            }
            path = tmpPath + ".txt";
            return path;
        }

        private bool isNotEmpty(List<string> logs)
        {
            return logs.Count != 0;
        }

        private DateTime getDate(string line)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
            DateTime date = new DateTime();
            string[] formats = { "HHmmssfff", "HH:mm:ss.fff", "dd/MMM/yyyy:HH:mm:ss", "HH:mm:ss,fff", "yyyy-MM-dd HH:mm:ss", "MMM dd, yyyy h:mm:ss tt", "yyyy-MM-dd HH:mm:ss,fff" };
            DateTime.TryParseExact(line, formats, null,
                                    DateTimeStyles.AllowWhiteSpaces |
                                    DateTimeStyles.AdjustToUniversal,
                                    out date);
            return date;
        }

        private List<string> replaceDate(List<string> logs) // костыль для замены кастомных форматов даты на стандартный
        {
            for (var i = 0; i < logs.Count; i++)
            {
                string tmpDate = string.Empty;
                DateTime date = new DateTime();
                string tmpLine = logs[i];
                try
                {
                    if (getDate(logs[i].Substring(0, 23)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 23);
                        date = getDate(logs[i].Substring(0, 23));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString());
                    }
                    else if (getDate(logs[i].Substring(0, 12)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 12);
                        date = getDate(logs[i].Substring(0, 12));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString());
                    }
                    else if (getDate(logs[i].Substring(5, 24)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 34).Trim();
                        date = getDate(logs[i].Substring(5, 24));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString() + " ");
                    }
                    else if (getDate(logs[i].Substring(0, 9)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 9);
                        date = getDate(logs[i].Substring(0, 9));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString());
                    }
                    else if (getDate("0" + logs[i].Substring(0, 8)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 8);
                        date = getDate("0" + logs[i].Substring(0, 8));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString());
                    }
                    else if (getDate(logs[i].Substring(5, 19)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 25);
                        date = getDate(logs[i].Substring(5, 19));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString());
                    }
                    else if (getDate(logs[i].Substring(16, 20)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 43);
                        date = getDate(logs[i].Substring(16, 20));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString());
                    }
                    else if (getDate(logs[i].Substring(21, 20)) != DateTime.MinValue) //ок
                    {
                        tmpDate = logs[i].Substring(0, 48);
                        date = getDate(logs[i].Substring(21, 20));
                        tmpLine = logs[i].Replace(tmpDate, date.ToString());
                    }
                    else { }
                }
                catch (Exception)
                {
                }
                logs[i] = tmpLine;
            }
            return logs;
        }

        private List<string> getCurrentDateLog(List<string> logs)
        {
            List<string> resLog = new List<string>();
            DateTime tmpDate = DateTime.Now.Date;

            foreach (string log in logs)
            {
                try
                {
                    DateTime curTime = Convert.ToDateTime(log.Substring(0, 19));
                    if (tmpDate <= curTime)
                    {
                        resLog.Add(log);
                        tmpDate = curTime;
                    }
                    else
                    {
                        resLog.Clear();
                        resLog.Add(log);
                        tmpDate = curTime;
                    }
                }
                catch (Exception)
                {
                    if (resLog.Count != 0)
                    {
                        resLog.Add(log);
                    }
                }
            }

            return resLog;
        }
    }
}
