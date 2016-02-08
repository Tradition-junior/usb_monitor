using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;


namespace usb_monitor
{
    public partial class Form1 : Form
    {
        UsbDeviceFinder MyUsbFinder;
        //= new UsbDeviceFinder(Convert.ToInt32("0x1366", 16), Convert.ToInt32("0x0105", 16));
        //UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(Convert.ToInt32("0x046D", 16), Convert.ToInt32("0xC517", 16));

        public static UsbDevice MyUsbDevice;
        device[] devices;
        List<int> data = new List<int>();
        int i;
        private bool trying;

        public Form1()
        {
            InitializeComponent();
            ReadEndpointID[] endpoints =
            {
                ReadEndpointID.Ep01, ReadEndpointID.Ep02, ReadEndpointID.Ep03,
                ReadEndpointID.Ep04, ReadEndpointID.Ep05, ReadEndpointID.Ep06
            };
            comboBox2.DataSource = endpoints;
            get_list();
            timer2.Start();
            comboBox1.SelectedIndex = -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            get_list();
        }

        private void get_list()
        {
            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            int tmp = comboBox1.SelectedIndex;
            devices = new device[allDevices.Count];
            int i = 0;
            textBox1.Clear();
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                if (usbRegistry.Open(out MyUsbDevice))
                {
                    devices[i] = new device(MyUsbDevice.Info.ToString());
                    textBox1.Text = textBox1.Text + devices[i].Info + "\r\n";
                }
                i++;
            }
            comboBox1.DataSource = devices;
            comboBox1.DisplayMember = "Name";
            comboBox1.SelectedIndex = -1;

        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
                MyUsbFinder = new UsbDeviceFinder(Convert.ToInt32(devices[comboBox1.SelectedIndex].Vid, 16),
                    Convert.ToInt32(devices[comboBox1.SelectedIndex].Pid, 16));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DataPoint temp = new DataPoint();
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
                UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader((ReadEndpointID) comboBox2.SelectedItem);

                byte[] readBuffer = new byte[1024];
                int bytesRead;

                //Возвращает данные или ошибку, если через 5 секунд ничего не было возвращено
                ec = reader.Read(readBuffer, 5000, out bytesRead);

                if (bytesRead == 0) throw new Exception(string.Format("{0}:No more bytes!", ec));
                try
                {
                    if (trying)
                    {
                        temp.SetValueXY(i,
                            Convert.ToDouble(Encoding.Default.GetString(readBuffer, 0, bytesRead).Replace('.', ',')));
                        i++;
                        chart1.Series[0].Points.Add(temp);
                        data.Add(bytesRead);
                    }
                }
                catch
                {
                    trying = false;
                }
                //string[] tmp = Encoding.Default.GetString(readBuffer, 0, bytesRead).Split(' ');
                //tmp[2] = tmp[2].Trim('\n', '\r', 'F');
                //string ans = "";
                //for(int i =0; i<3; i++)
                //{
                //    ans = ans + Convert.ToInt32(tmp[i], 16).ToString()+" ";
                //}
                //textBox2.AppendText(ans + "\r\n");
                textBox2.AppendText(Encoding.Default.GetString(readBuffer, 0, bytesRead));
              // textBox2.Text = ans;
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
            trying = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }

    }
}