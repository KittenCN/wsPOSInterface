using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PacketWash_answer
{
    public class PacketWash_answer
    {
        public PacketWash_answer()
        { }

        //支付提示或打印信息 
        private string pay_msg;
        public string Pay_msg
        {
            get { return pay_msg; }
            set { pay_msg = value; }
        }

        //效验信息
        private string check_value;
        public string Check_value
        {
            get { return check_value; }
            set { check_value = value; }
        }

        public void ReadXML(PacketWash_answer ptan, string epwd, string pay_msg)
        {
            Pay_msg = pay_msg;
            MD5 md5Hash = MD5.Create();
            Check_value = GenClass.GenClass.GetMd5Hash(md5Hash, epwd);
        }
    }
}
