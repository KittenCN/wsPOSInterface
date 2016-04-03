using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PacketLogout_ask
{
    public class PacketLogout_ask
    {
        private string delivery_man;
        public string Delivery_man
        {
            get { return delivery_man; }
            set { delivery_man = value; }
        }

        private string tid;
        public string Tid
        {
            get { return tid; }
            set { tid = value; }
        }

        private string total_times;
        public string Total_times
        {
            get { return total_times; }
            set { total_times = value; }
        }

        private string total_amount;
        public string Total_amount
        {
            get { return total_times; }
            set { total_amount = value; }
        }

        private string check_value;
        public string Check_value
        {
            get { return check_value; }
            set { check_value = value; }
        }

        public void ReadXML(XmlNode xnXML)
        {
            Delivery_man = xnXML.SelectSingleNode("delivery_man").InnerText;
            Tid = xnXML.SelectSingleNode("tid").InnerText;
            Total_times = xnXML.SelectSingleNode("total_times").InnerText;
            Total_amount = xnXML.SelectSingleNode("total_amount").InnerText;
            Check_value = xnXML.SelectSingleNode("check_value").InnerText;
        }
    }
}
