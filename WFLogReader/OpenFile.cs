using System.IO;
using System.Net;
using System.Text;

namespace WFLogReader
{
    public class OpenFile
    {
        public string POST(string Url, string Data, ParamsView parametres)
        {
            WebRequest req = WebRequest.Create(Url);
            req.Method = "POST";
            req.Timeout = 100000;
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] sentData = Encoding.GetEncoding(1251).GetBytes(Data);
            req.ContentLength = sentData.Length;
            Stream sendStream =req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();
            WebResponse res = req.GetResponse();
            Stream ReceiveStream = res.GetResponseStream();
            StreamReader sr = new StreamReader(ReceiveStream, Encoding.UTF8);
            string respons = sr.ReadToEnd();
            Logs log = new Logs(parametres);
            string result = log.parsLog(respons);
            return result;
        }
    }
}
