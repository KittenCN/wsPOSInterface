using System.Xml;

namespace PacketHead_Ask
{
    public class PacketHead_Ask
    {
        public PacketHead_Ask()
        {

        }

        ///请求业务种类编号
        private string transaction_id;
        public string Transaction_id
        {
            get { return transaction_id; }
            set { transaction_id = value; }
        }

        ///请求方代码
        private string requester;
        public string Requester
        {
            get { return requester; }
            set { requester = value; }
        }

        ///应答方公司英文名称
        private string target;
        public string Target
        {
            get { return target; }
            set { target = value; }
        }

        ///请求时间yyyyMMddHHmmss
        private string request_time;
        public string Request_time
        {
            get { return request_time; }
            set { request_time = value; }
        }

        ///终端设备号
        private string terminal_eqno;
        public string Terminal_eqno
        {
            get { return terminal_eqno; }
            set { terminal_eqno = value; }
        }

        ///物流终端号
        //private string terminal_id;
        //public string Terminal_id
        //{
        //    get { return terminal_id; }
        //    set { terminal_id = value; }
        //}

        ///系统流水号YYMMDD+(00000000-99999999)
        private string system_serial;
        public string System_serial
        {
            get { return system_serial; }
            set { system_serial = value; }
        }

        ///版本号
        private string version;
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        ///自定义字段
        private string ext_attributes;
        public string Ext_attributes
        {
            get { return ext_attributes; }
            set { ext_attributes = value; }
        }

        public void ReadXML(XmlNode xn)
        {
            // 获取头值,写入应答头值
            if (xn != null)
            {
                Transaction_id = xn.SelectSingleNode("transaction_id").InnerText;
                Requester = xn.SelectSingleNode("requester").InnerText;
                Target = xn.SelectSingleNode("target").InnerText;
                Request_time = xn.SelectSingleNode("request_time").InnerText;
                Terminal_eqno = xn.SelectSingleNode("terminal_eqno").InnerText;
                //Terminal_id = xn.SelectSingleNode("terminal_id").InnerText;
                System_serial = xn.SelectSingleNode("system_serial").InnerText;
                Version = xn.SelectSingleNode("version").InnerText;
                Ext_attributes = xn.SelectSingleNode("ext_attributes").InnerText;
            }
            else
            {

            }
        }

    }
}
