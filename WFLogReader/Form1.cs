using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WFLogReader
{
    public partial class Form1 : Form
    {
        private List<string> BlackList;
        private List<string> LogsList;
        private List<Stand> StandList;
        private List<Thread> ThreadList;

        public string path;
        public static string startFolder = @"/logreader/logreader.jsp?filename=/opt/oracle/user_projects/domains";

        public Form1()
        {
            InitializeComponent();
            path = ReadPath("Config\\Path.txt");
            textBox2.Text = path;
            radioButton1.Select();
            maskedTextBox1.Text = DateTime.Now.AddMinutes(-5).ToString("HH:mm");
            maskedTextBox2.Text = DateTime.Now.ToString("HH:mm");

            ThreadList = new List<Thread>();

            string BlackListPath = "Config\\BlackList.txt";
            BlackList = new List<string>();
            BlackList = LoadConfig(BlackListPath, BlackList);

            LogsList = new List<string>();
            string LogsListPath = "Config\\LogsList.txt";
            LogsList = LoadConfig(LogsListPath, LogsList);
            foreach (string log in LogsList)
            {
                checkedListBox1.Items.Insert(0,log);
            }
            checkedListBox1.SetItemChecked(0, true);

            StandList = new List<Stand>();
            string StandListPath = "Config\\StandsList.txt";
            List<string> tmpLogList = new List<string>();
            tmpLogList = LoadConfig(StandListPath, tmpLogList);
            foreach (string item in tmpLogList)
            {
                string[] configLine = item.Split(' ');
                Stand stand = new Stand(configLine[0], configLine[1]);
                StandList.Add(stand);
            }
            foreach (Stand item in StandList)
            {
                comboBox1.Items.Insert(0, item);
            }
            comboBox1.SelectedItem = comboBox1.Items[0];
        }

        private string ReadPath(string pathFile)
        {
            string resultPath;
            using (StreamReader sr = new StreamReader(pathFile, Encoding.Default))
            {
                resultPath = sr.ReadLine();
                sr.Close();
            }
            return resultPath;
        }

        private void SavePath()
        {

            using (StreamWriter writer = new StreamWriter("Config\\Path.txt", false))
            {
                writer.Write(textBox2.Text);
            }
        }

        private List<string> LoadConfig(string path, List<string> config)
        {
            using (StreamReader sr = new StreamReader(path, Encoding.Default))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (!s.Contains("###"))
                    {
                        config.Add(s.Trim());
                    }
                }
                sr.Close();
            }
            return config;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (Thread t in ThreadList)
            {
                t.Abort();
            }
            SavePath();
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label5.Text = "Выполнение...";

            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            int min;
            if (radioButton1.Checked)
            {
                startTime = DateTime.Now.Date.AddHours(DateTime.Parse(maskedTextBox1.Text).Hour).AddMinutes(DateTime.Parse(maskedTextBox1.Text).Minute);
                endTime = DateTime.Now.Date.AddHours(DateTime.Parse(maskedTextBox2.Text).Hour).AddMinutes(DateTime.Parse(maskedTextBox2.Text).Minute);
                min = Convert.ToInt32((DateTime.Now - startTime).TotalMinutes);
            }
            else
            {
                min = Convert.ToInt32(textBox1.Text);
                startTime = DateTime.Now.AddMinutes(-min);
                endTime = DateTime.Now;
            }

            List<string> list = new List<string>();
            foreach (string item in checkedListBox1.CheckedItems)
            {
                list.Add(item);
            }
            ParamsView paramsv = new ParamsView(comboBox1.Text, endTime, startTime, min, list, textBox2.Text);
            paramsv.outputSymb = int.Parse(textBox3.Text);
            paramsv.BlackList = BlackList;
            string url = (comboBox1.SelectedItem as Stand).URL;
            Config config = new Config(url, paramsv, label5);
            Thread thrd = new Thread(ThreadFunction);
            thrd.Start(config);
            ThreadList.Add(thrd);
            Thread PBthrd = new Thread(PBthread);
            PBthrd.Start();
            ThreadList.Add(PBthrd);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        public void PBthread()
        {
            for (int i = 0; i < 100; i++)
            {
                UpdateBar(i);
                Thread.Sleep(100);
            }
        }

        public void ThreadFunction(Object input)
        {
            OpenDir od = new OpenDir((input as Config).paramsv);
            od.workQueue.Enqueue(startFolder);
            od.get((input as Config).url);
            while (od.logQueue.Count != 0)
            {
                UpdateRichTextBox(od.openLog((input as Config).url));
            }
            UpdateBar(100);
            UpdateLabel("Готово!");
            UpdateBar(0);
        }

        public void UpdateBar(int num)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateBar), num);
            }
            else
            {
                progressBar1.Value = num;
            }
        }

        public void UpdateLabel(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateLabel), text);
            }
            else
            {
                label5.Text = text;
            }
        }

        public void UpdateRichTextBox(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateRichTextBox), text);
            }
            else
            {
                richTextBox1.AppendText(text);
            }
        }
        
        public void ChooseFolder()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            radioButton1.Select();
            richTextBox1.Text = string.Empty;
            maskedTextBox1.Text = DateTime.Now.AddMinutes(-5).ToString("HH:mm");
            maskedTextBox2.Text = DateTime.Now.ToString("HH:mm");
            for (var i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
            checkedListBox1.SetItemChecked(0, true);
            comboBox1.SelectedItem = comboBox1.Items[0];
            label5.Text = string.Empty;
            textBox1.Text = 5.ToString();
            textBox3.Text = 20000.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (Thread t in ThreadList)
            {
                t.Abort();
            }
            progressBar1.Value = 0;
            label5.Text = "Прервано.";
        }
    }
}
