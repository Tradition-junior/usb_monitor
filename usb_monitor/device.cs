using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usb_monitor
{
    class device
    {
        string info;
        string vid;
        string pid;
        string manufacturer;
        string product;
        string name;

        public device(string info)
        {
            this.info = info;
            parse();
            name = manufacturer + " " + product;
        }

        public string Name
        {
            get { return name; }
        }
        public string Info
        {
            get { return info; }
            set { info = value; }
        }

        public string Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        public string Vid
        {
            get { return vid; }
            set { vid = value; }
        }

        void parse()
        {
            int j = 0;
            j = info.IndexOf("Vendor") + 9;
            string tmp = "";

            while (info[j] != '\r')
            {
                vid += info[j];
                j++;
            }
            
            j = info.IndexOf("ProductID") + 10;
            while (info[j] != '\r')
            {
                pid += info[j];
                j++;
            }


            j = info.IndexOf("ManufacturerString:") + 19;
            while (info[j] != '\r')
            {
                manufacturer += info[j];
                j++;
            }

            j = info.IndexOf("ProductString:") + 14;
            while (info[j] != '\r')
            {
                product += info[j];
                j++;
            }
        }
    }
}
