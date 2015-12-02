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
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;

namespace usb_monitor
{
    public partial class Form1 : Form
    {
        string s;
        string current_us;
        static string abc = "";

        public static UsbDevice MyUsbDevice;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ErrorCode ec = ErrorCode.None;
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            string vid = "";
            string pid = "";
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                int i = 0;
                if (usbRegistry.Open(out MyUsbDevice))
                {
                    s = MyUsbDevice.Info.ToString();
                    i = s.IndexOf("Vendor") + 9;
                    while (s[i] != '\n')
                    {
                        vid += s[i];
                        i++;
                    }

                    i = s.IndexOf("ProductID") + 10;
                    while (s[i] != '\n')
                    {
                        pid += s[i];
                        i++;
                    }

                    textBox1.Text = textBox1.Text + "\n" + s + vid + pid;
                }
            }
            UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(Convert.ToInt32("0x0810", 16), Convert.ToInt32("0x0003", 16));
            try
            {
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    wholeUsbDevice.SetConfiguration(1);
                    wholeUsbDevice.ClaimInterface(0);
                }
                UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                byte[] readBuffer = new byte[1024];
                while (ec == ErrorCode.None)
                {
                    int bytesRead;

                    ec = reader.Read(readBuffer, 5000, out bytesRead);

                    if (bytesRead == 0) throw new Exception(string.Format("{0}:No more bytes!", ec));
                    textBox1.Text = textBox1.Text + "\n" + Encoding.Default.GetString(readBuffer, 0, bytesRead);
                }

                Console.WriteLine("\r\nDone!\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;
                    UsbDevice.Exit();

                }
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}