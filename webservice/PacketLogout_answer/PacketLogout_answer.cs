using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace PacketLogout_answer
{
    public class PacketLogout_answer
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

        private string check_value;
        public string Check_value
        {
            get { return check_value; }
            set { check_value = value; }
        }

        public void ReadXML(PacketLogout_ask.PacketLogout_ask plas, string epwd)
        {
            Delivery_man = plas.Delivery_man.ToString();
            Tid = plas.Tid.ToString();
            MD5 md5Hash = MD5.Create();
            Check_value = GenClass.GenClass.GetMd5Hash(md5Hash, epwd);
        }
    }
}
