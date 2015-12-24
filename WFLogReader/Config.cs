using System.Windows.Forms;

namespace WFLogReader
{
    class Config
    {
        public string url;
        public ParamsView paramsv;
        public Label label5;

        public Config(string url, ParamsView paramsv, Label label5)
        {
            this.url = url;
            this.paramsv = paramsv;
            this.label5 = label5;
        }
    }
}
