using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usb_monitor
{
    class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
            parse();

        }

        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
        public string VID { get; private set; }
        public string PID { get; private set; }
        public string COM { get; private set; }
        public bool IfCOM { get; private set; }
        public string Name {
            get { return VID + " " + PID; }
        }
    public void parse()
        {
            string tmp = DeviceID + PnpDeviceID;
            int j = tmp.IndexOf("VID_");
            if (j != -1)
            {
                j += 4;
                VID = "";
                while (tmp[j] != '&')
                {
                    VID += tmp[j];
                    j++;
                }
            }
            else
                VID = "null";

            j = tmp.IndexOf("PID_");
            if (j != -1)
            {
                j += 4;
                PID = "";
                while (tmp[j] != '\\')
                {
                    PID += tmp[j];
                    j++;
                }
            }
            else
                VID = "null";

            j = tmp.IndexOf("COM");
            if (j != -1)
            {
                for (int i = 0; i < 4; i++, j++)
                {
                    COM += tmp[j];
                }
                IfCOM = true;
            }
            else
                COM = "null";
        }
    }
}
