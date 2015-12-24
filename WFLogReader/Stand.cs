namespace WFLogReader
{
    class Stand
    {
        public string Name;
        public string URL;

        public Stand(string name, string url)
        {
            Name = name;
            URL = url;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
