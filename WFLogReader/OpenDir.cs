using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace WFLogReader
{
    class OpenDir
    {
        private ParamsView parametres;
        private List<string> blocked;

        public Queue<string> workQueue;
        public Queue<string> logQueue;

        public OpenDir(ParamsView parametres)
        {
            this.parametres = parametres;
            workQueue = new Queue<string>();
            logQueue = new Queue<string>();
            blocked = new List<string>();
            blocked = parametres.BlackList;
        }

        public void get(string Url)
        {
            while (workQueue.Count != 0)
            {
                string dataPath = workQueue.Dequeue();
                WebRequest req = WebRequest.Create(Url + dataPath);
                HttpWebResponse resp = (HttpWebResponse) req.GetResponse();
                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string respons = sr.ReadToEnd();
                sr.Close();
                List<string> items = parsDirResult(respons);
                foreach (string item in items)
                {
                    if (!isBloked(item) && !isRoot(item, dataPath) && !getName(item).Contains("."))
                    {
                        workQueue.Enqueue(item);
                    }
                    if (isNeeded(getName(item)) && getName(item).Contains(".log"))
                    {
                        logQueue.Enqueue(item);
                    }
                    if (isKpiNeeded())
                    {
                        if ((getName(item).Contains("KPI") || getName(item).Contains("kpi")) && getName(item).Contains(".log"))
                        {
                            logQueue.Enqueue(item);
                        }
                    }
                }
            }
        }

        public string openLog(string Url)
        {
            string log = logQueue.Dequeue();
            string dataPost = generateDataPost(log);
            string urlPost = generateUrlPost(Url);
            OpenFile of = new OpenFile();
            parametres.logName = getName(log);
            string result = of.POST(urlPost, dataPost, parametres);
            return result;
        }

        public List<string> parsDirResult(string respons)
        {
            Regex firstReg = new Regex("href=\".+?\"");
            Regex seconsReg = new Regex("\".+?\"");
            MatchCollection matches = firstReg.Matches(respons);
            List<string> items = new List<string>();
            for (var i = 1; i < matches.Count; i++)
            {
                items.Add(seconsReg.Match(matches[i].Value).Value.Trim("\"".ToCharArray()));
            }

            return items;
        }

        public string generateDataPost(string lineRespons)
        {
            string dataPost = "&action=&rand=124&";
            int count = parametres.min * parametres.outputSymb;
            dataPost = dataPost + lineRespons.Replace(@"/logreader/logreader.jsp?", "") + "&count=" + count;
            return dataPost;
        }

        public string generateUrlPost(string url)
        {
            return url + @"/logreader/main.jsp";
        }
        
        public string getName(string way)
        {
            string name;
            var last = way.LastIndexOf("/");
            name = way.Substring(last + 1);
            return name;
        }

        public bool isRoot(string path, string rootPath)
        {
            var countFolder = 0;
            var countRoot = 0;
            for (var j = 0; j < rootPath.Length; j++)
            {
                if (rootPath[j] == '/')
                {
                    countRoot++;
                }
            }
            for (var i = 0; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    countFolder++;
                }
            }
            if (countFolder <= countRoot)
            {
                blocked.Add(getName(rootPath));
                return true;
            }
            else
            {
                return false;
            }
            //return (countFolder <= countRoot);
        }

        public bool isBloked(string path)
        {
            foreach(string item in blocked)
            {
                if (getName(path).Contains(item))
                {
                    return true;                
                }
            }
            return false;
        }

        public bool isNeeded(string name)
        {
            int countNeededLogs = 0;
            foreach (string logName in parametres.neededLogs)
            {
                if (name == logName)
                {
                    countNeededLogs++;
                }
            }
            return (countNeededLogs != 0);
        }

        public bool isKpiNeeded()
        {
            var count = 0;
            foreach (string log in parametres.neededLogs)
            {
                if (log == "kpi.log")
                {
                    count++;
                }
            }
            if (count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
