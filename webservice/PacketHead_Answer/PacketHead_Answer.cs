using System;

namespace PacketHead_Answer
{
    public class PacketHead_Answer
    {
        public PacketHead_Answer()
        { }

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

        ///响应时间yyyyMMddHHmmss
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

        ///响应吗
        private string resp_code;
        public string Resp_code
        {
            get { return resp_code; }
            set { resp_code = value; }
        }

        ///响应信息 
        private string resp_msg;
        public string Resp_msg
        {
            get { return resp_msg; }
            set { resp_msg = value; }
        }

        /////版本号
        //private string version;
        //public string Version
        //{
        //    get { return version; }
        //    set { version = value; }
        //}

        ///自定义字段
        private string ext_attributes;
        public string Ext_attributes
        {
            get { return ext_attributes; }
            set { ext_attributes = value; }
        }

        public void ReadXMLfromASK(PacketHead_Ask.PacketHead_Ask phas)
        {
            // 获取头值,写入应答头值           
            Transaction_id = phas.Transaction_id;
            Requester = phas.Requester;
            Target = phas.Target;
            Request_time = DateTime.Now.ToString("yyyyMMddHHmmss");
            System_serial = phas.System_serial;
            Terminal_eqno = phas.Terminal_eqno;
            //Terminal_id = phas.Terminal_id;
        }

        public void Gen_Answer_XML(Boolean bool_success, string in_Rmsg, string in_EXstring)
        {
            if (bool_success)
            {
                Resp_code = "00";
                Resp_msg = "交易成功";
                Ext_attributes = in_EXstring;
            }
            else
            {
                Resp_code = "01";
                Resp_msg = in_Rmsg;
                Ext_attributes = in_EXstring;
            }
        }
    }
}
