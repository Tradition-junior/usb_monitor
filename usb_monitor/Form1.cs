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
        UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(Convert.ToInt32("0x1366", 16), Convert.ToInt32("0x0105", 16));

        public static UsbDevice MyUsbDevice;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            string[] vid = new string[allDevices.Count];
            string[] pid = new string[allDevices.Count];
            string[] s = new string[allDevices.Count];
            int i = 0;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                int j = 0;
                if (usbRegistry.Open(out MyUsbDevice))
                {
                    s[i] = MyUsbDevice.Info.ToString();
                    j = s[i].IndexOf("Vendor") + 9;
                    while (s[i][j] != '\n')
                    {
                        vid[i] += s[i][j];
                        j++;
                    }

                    j = s[i].IndexOf("ProductID") + 10;
                    while (s[i][j] != '\n')
                    {
                        pid[i] += s[i][j];
                        j++;
                    }
                    textBox1.Text = textBox1.Text + s[i];
                }
                i++;

            }
            
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            ErrorCode ec = ErrorCode.None;
            try
            {
                //открываем поток
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    wholeUsbDevice.SetConfiguration(1);
                    wholeUsbDevice.ClaimInterface(0);
                }

                //читает 1ый эндпоинт
                UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                byte[] readBuffer = new byte[1024];
                int bytesRead;

                //Возвращает данные или ошибку, если через 5 секунд ничего не было возвращено
                ec = reader.Read(readBuffer, 5000, out bytesRead);
                if (bytesRead == 0) throw new Exception(string.Format("{0}:No more bytes!", ec));

                textBox2.Text = textBox2.Text + "\n" + Encoding.Default.GetString(readBuffer, 0, bytesRead);

            }
            catch (Exception ex)
            {
                //кидает ошибку и останавливает таймер при ошибке
                timer1.Stop();
                MessageBox.Show((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                //закрывает поток
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

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }
    }
}