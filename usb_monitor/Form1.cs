using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.IO;
using System.IO.Ports;
using System.Management;



namespace usb_monitor
{
    public partial class Form1 : Form
    {
        UsbDeviceFinder MyUsbFinder;

        public static UsbDevice MyUsbDevice;
        List<string> data = new List<string>();
        int i;
        private bool trying;
        private int method;
        private int rate;
        private List<USBDeviceInfo> devices_libusb;
        private List<USBDeviceInfo> devices_com;
        private bool reading;
        private SerialPort _serialPort;

        public Form1()
        {

            InitializeComponent();
            log("Приложение запущено");
            ReadEndpointID[] endpoints =
            {
                ReadEndpointID.Ep01, ReadEndpointID.Ep02, ReadEndpointID.Ep03,
                ReadEndpointID.Ep04, ReadEndpointID.Ep05, ReadEndpointID.Ep06
            };

            int[] rates =
            {110, 150, 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200};
            comboBox2.DataSource = endpoints;
            comboBox3.DataSource = rates;
            comboBox3.SelectedIndex = 6;
            rate = (int)comboBox3.SelectedValue;
            method = 2;
            get_list();
            comboBox1.SelectedIndex = -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            get_list();
        }

        private void get_list()
        {
            textBox1.Text = "";
            devices_libusb = GetUSBDevices(1);
            devices_com = GetUSBDevices(2);
            List<USBDeviceInfo> tmp_dev = new List<USBDeviceInfo>();
            tmp_dev.AddRange(devices_libusb);
            tmp_dev.AddRange(devices_com);
            foreach (USBDeviceInfo usbDevice in tmp_dev)
            {
                textBox1.Text += String.Format("VID: {0}\r\nPID: {1}\r\nDescription: {2}\r\nIf COM Device: {3}\r\n\r\n",
                    usbDevice.VID, usbDevice.PID, usbDevice.Description, usbDevice.IfCOM);
            }
            if (method == 1)
            {
                comboBox1.DataSource = devices_libusb;
                comboBox1.DisplayMember = "Name";
                comboBox1.SelectedIndex = -1;
            }
            if (method == 2)
            {
                comboBox1.DataSource = devices_com;
                comboBox1.DisplayMember = "Name";
                comboBox1.SelectedIndex = -1;
            }
        }

        static List<USBDeviceInfo> GetUSBDevices(int type)
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();
            ManagementObjectCollection collection;

            if (type == 1)

            {
                using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                    collection = searcher.Get();
                foreach (var device in collection)
                {
                    devices.Add(new USBDeviceInfo(
                        (string)device.GetPropertyValue("DeviceID"),
                        (string)device.GetPropertyValue("PNPDeviceID"),
                        (string)device.GetPropertyValue("Description"),
                        (string)device.GetPropertyValue("Name")
                        ));
                }

                collection.Dispose();
            }

            if (type == 2)
            {
                using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM[0-9]%'"))
                    collection = searcher.Get();
                foreach (var device in collection)
                {
                    Console.WriteLine("{0}, {1}, {2}", device.GetPropertyValue("DeviceID"), device.GetPropertyValue("PNPDeviceID"), device.GetPropertyValue("Name"));
                    devices.Add(new USBDeviceInfo(
                        (string)device.GetPropertyValue("DeviceID"),
                        (string)device.GetPropertyValue("PNPDeviceID"),
                        (string)device.GetPropertyValue("Description"),
                        (string)device.GetPropertyValue("Name")
                        ));

                }
                collection.Dispose();
            }
            return devices;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (method == 1)
                connect();
        }

        //
        //переделать всё
        //

        private void connect()
        {
            if (comboBox1.SelectedIndex != -1)
                MyUsbFinder = new UsbDeviceFinder(Convert.ToInt32(devices_libusb[comboBox1.SelectedIndex].VID, 16),
                    Convert.ToInt32(devices_libusb[comboBox1.SelectedIndex].PID, 16));
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
                UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader((ReadEndpointID)comboBox2.SelectedItem);

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
                        data.Add(Encoding.Default.GetString(readBuffer, 0, bytesRead));
                    }
                }
                catch
                {
                    trying = false;
                }
                textBox2.AppendText(Encoding.Default.GetString(readBuffer, 0, bytesRead));
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

        private void new_connect()
        {
            reading = true;
            log(String.Format("Попытка подключиться к {0}", devices_com[comboBox1.SelectedIndex].COM));
            try
            {
                _serialPort = new SerialPort(devices_com[comboBox1.SelectedIndex].COM);
                //_serialPort = new SerialPort("COM3");
                _serialPort.BaudRate = rate;
                _serialPort.ReadTimeout = 1000;
                _serialPort.Open();
                Thread th = new Thread(read);
                log(String.Format("Подключились к {0}", devices_com[comboBox1.SelectedIndex].COM));
                th.Start();

            }
            catch (Exception ex)
            {
                log(String.Format("Сбой подключения к {0}. {1}", devices_com[comboBox1.SelectedIndex].COM, ex.ToString()));
            }

        }

        string tmp = "";
        byte[] all;
        int j = 0;
        private void read()
        {
            log("------Начало чтения-------");
            while (reading)
            {
                try
                {
                    int num = _serialPort.BytesToRead;
                    if (num != 0)
                    {
                        log(string.Format("Читаю"));
                        byte[] buffer = new byte[num];
                        _serialPort.Read(buffer, 0, num);
                        string ans = Encoding.Default.GetString(buffer);
                        SetText(ans);
                        tmp += ans;
                        log(string.Format("Байтов: {0}  Буфер: {1} ", num, ans.Normalize()));
                        //Строит график, если каждое число разделено признаком конца строки. Конец строки можно заменить на что угодно
                        //или вообще убрать. 
                        if (tmp.IndexOf('\n') > 0)
                        {
                            try
                            {
                                SetData(double.Parse(tmp.Replace('.', ',')));
                                //tmp.Remove('\n');
                                data.Add(tmp.TrimEnd());
                            }
                            catch
                            {
                            }
                            tmp = "";
                        }
                    }
                    
                }
                catch(Exception ex)
                {
                    log(string.Format("Чтение не удалось. {0}", ex.ToString()));
                }

            }
            log("------Конец чтения------");
        }

        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {

            try
            {
                if (textBox2.InvokeRequired)
                {
                    SetTextCallback d = SetText;
                    textBox2.Invoke(d, text);
                }
                else
                {

                    textBox2.AppendText(text);

                }
            }
            catch
            {
            }
        }

        delegate void SetDataCallback(double num);
        private void SetData(double num)
        {
            DataPoint temp = new DataPoint();
            if (chart1.InvokeRequired)
            {
                SetDataCallback d = SetData;
                chart1.Invoke(d, num);
            }
            else
            {
                try
                {
                    temp.SetValueXY(i, num);
                    i++;
                    chart1.Series[0].Points.Add(temp);
                }
                catch
                {
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Start();
            trying = true;
            if (method == 2)
            {
                new_connect();
                reading = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (method == 2)
            {
                reading = false;
                _serialPort.Close();
            }
            timer1.Stop();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }

        private void usbLabDotNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            method = 1;
            if (method == 2)
            {
                reading = false;
                _serialPort.Close();
            }
            comboBox1.DataSource = devices_libusb;
        }

 

        private void cOMPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            method = 2;
            timer1.Stop();
            comboBox1.DataSource = devices_com;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            rate = (int)comboBox3.SelectedValue;
            reading = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            reading = false;
            log("Приложение закрыто\r\n");
        }

        
        public static void log(string txt)
        {
            try
            {
                File.AppendAllText("log.txt", txt + "\r\n");
            }
            catch { }
        }
          
        private void пульсToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (string s in data)
            {
                File.AppendAllText(saveFileDialog1.FileName, s+"\r\n");
            }
        }
    }
}
