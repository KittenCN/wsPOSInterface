using System.Security.Cryptography;
using System.Xml;

namespace PacketLogin_answer
{
    public class PacketLogin_answer
    {
        public PacketLogin_answer()
        { }

        ///公司id
        private string company_id;
        public string Company_id
        {
            get { return company_id; }
            set { company_id = value; }
        }

        ///员工号
        private string delivery_man;
        public string Delivery_man
        {
            get { return delivery_man; }
            set { delivery_man = value; }
        }

        ///员工登录密码;32位md5摘要(大写)
        private string password;
        public string Passwoed
        {
            get { return password; }
            set { password = value; }
        }

        ///所属站点信息
        private string delivery_info;
        public string Delivery_info
        {
            get { return delivery_info; }
            set { delivery_info = value; }
        }

        ///效验数据 md5(request_time+terminal_id+delivery_man+password+signs)
        private string check_value;
        public string Check_value
        {
            get { return check_value; }
            set { check_value = value; }
        }

        /// <summary>
        /// 本次使用的DES公钥
        /// </summary>
        private string public_key;
        public string Public_key
        {
            get { return public_key; }
            set { public_key = value; }
        }

        private string chenk_batch_no;
        public string Chenk_batch_no
        {
            get { return chenk_batch_no; }
            set { chenk_batch_no = value; }
        }

        /// <summary>
        /// 公钥生成及回传消息定义
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="epwd"></param>
        /// <param name="pKey"></param>
        public void ReadXML(XmlNode xn, string epwd,string pKey,string priKey,string BatchNum)
        {
            Delivery_man = xn.SelectSingleNode("delivery_man").InnerText;
            if(xn.SelectSingleNode("company_id")==null)
            { Company_id = "9999"; }
            else
            { Company_id = xn.SelectSingleNode("company_id").InnerText; }           
            Delivery_info = "测试站点";
            MD5 md5Hash = MD5.Create();
            Check_value = GenClass.GenClass.GetMd5Hash(md5Hash, epwd);
            //EnDeCode.EnDeCode EDC = new EnDeCode.EnDeCode();
            //Public_key = EDC.DesEncrypt(EDC.GetHexString(16),pKey);
            Public_key = priKey;
            Chenk_batch_no = BatchNum;
        }

    }
}
