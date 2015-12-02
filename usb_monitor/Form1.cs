using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using USBHIDDRIVER;
using System.Threading;

namespace usb_monitor
{
    public partial class Form1 : Form
    {
        string[] s;
        string current_us;
        static string abc ="";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //список всех hid устройств

            USBInterface usb = new USBInterface("_");
            s = usb.getDeviceList();

            //мини парсер 

            for (int x = 0; x < s.Length; x++)
            {
                string tmp = "";
                for (int i = s[x].IndexOf("vid"), j = 0; i < s[x].IndexOf("vid") + 8; i++, j++)
                {
                    tmp += Convert.ToString(s[x][i]);
                }
                tmp += " ";

                for (int i = s[x].IndexOf("pid"), j = 0; i < s[x].IndexOf("pid") + 8; i++, j++)
                {
                    tmp += Convert.ToString(s[x][i]);
                }

                s[x] = tmp + " ";
            }

            comboBox1.Enabled = true;
            comboBox1.DataSource = s;
            comboBox1.SelectedIndex = -1;
        }

        static void event_cacher(object sender, EventArgs a)
        {
            byte[] currentRecord = null;
            currentRecord = (byte[])USBHIDDRIVER.USBInterface.usbBuffer[0];
            for (int i = 0; i < currentRecord.Length; i++)
            {
                abc += currentRecord[i];
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            current_us = (string)comboBox1.SelectedValue;
            textBox1.Text = "";

            //без понятия, как прочитать то, что отправляет устройство
            if (comboBox1.SelectedIndex != -1)
            {
                USBInterface us = new USBInterface(current_us.Split(' ')[0], current_us.Split(' ')[1]);
                string[] s;
                //us.enableUsbBufferEvent(new System.EventHandler(event_cacher));
                us.Connect();
                us.startRead();
                Thread.Sleep(5);
                byte[] currentRecord = null;
                for (int i = 0; i < 200; i++)
                {
                    if (USBInterface.usbBuffer.Count != 0)
                    {
                        currentRecord = (byte[])USBInterface.usbBuffer[0];
                        for (int j = 0; j < currentRecord.Length; j++)
                        {
                            abc += currentRecord[j];
                        }
                    }
                    Thread.Sleep(2);
                }
                us.stopRead();
                textBox1.Text = abc;
            }
        }
    }
}