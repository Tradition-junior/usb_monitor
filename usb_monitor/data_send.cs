using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace usb_monitor
{
    public partial class data_send : Form
    {
        public event Handler data_set;
        public data_send()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            data_set(this, new data_sendEvents(textBox1.Text));
        }
    }

    public delegate void Handler(object sender, data_sendEvents e);

    public class data_sendEvents : EventArgs
    {
        private string data;
        public data_sendEvents(string data)
        {
            this.data = data;
        }

        public string Text
        {
            get { return data; }
        }

    }
}
