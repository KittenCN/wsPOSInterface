using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SMS
{
    class returnsms
    {
        private string returnstatus;
        public string ReturnStatus
        {
            get { return returnstatus; }
            set { returnstatus = value; }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private string payinfo;
        public string PayInfo
        {
            get { return payinfo; }
            set { payinfo = value; }
        }

        private string overage;
        public string Overage
        {
            get { return overage; }
            set { overage = value; }
        }

        private string sendtotal;
        public string SendTotal
        {
            get { return sendtotal; }
            set { sendtotal = value; }
        }

        public void ReadXML(XmlNode xn)
        {
            ReturnStatus = xn.SelectSingleNode("returnstatus").InnerText;
            Message = xn.SelectSingleNode("message").InnerText;
            //PayInfo = xn.SelectSingleNode("remainpoint").InnerText;
            //Overage = xn.SelectSingleNode("taskID").InnerText;
            //SendTotal = xn.SelectSingleNode("successCounts").InnerText;
        }
    }
}
