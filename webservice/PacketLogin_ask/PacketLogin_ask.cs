using System.Data;
using System.Xml;

namespace PacketLogin_ask
{
    public class PacketLogin_ask
    {
        public PacketLogin_ask()
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

        ///效验数据 md5(request_time+terminal_id+delivery_man+password+signs)
        private string check_value;
        public string Check_value
        {
            get { return check_value; }
            set { check_value = value; }
        }

        //登录配置
        public static bool setLogin(string in_XML,string LinkString)
        {
            string str_mysql = "";
            PacketLogin_ask pla = new PacketLogin_ask();
            DataSet DS;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(in_XML);
            XmlNode xn = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");

            if (xn != null)
            {
                if (xn.SelectSingleNode("company_id") == null)
                { pla.Company_id = "9999"; }
                else
                { pla.Company_id = xn.SelectSingleNode("company_id").InnerText; }

                if (xn.SelectSingleNode("delivery_man") == null)
                { pla.Delivery_man = "9999"; }
                else
                { pla.Delivery_man = xn.SelectSingleNode("delivery_man").InnerText; }

                if (xn.SelectSingleNode("password") == null)
                { pla.Passwoed = "9999"; }
                else
                { pla.Passwoed = xn.SelectSingleNode("password").InnerText; }

                if (xn.SelectSingleNode("check_value") == null)
                { pla.Check_value = "9999"; }
                else
                { pla.Check_value = xn.SelectSingleNode("check_value").InnerText; }

                return true;
            }
            else
            {
                return false;
            }
            //if (pla.Check_value != null && pla.Check_value != "") //判断效验值,待补充
            //{
            //    str_mysql = "select count(skf195) as coutnum from skt8 where skf195='" + pla.Delivery_man + "' and skf196='" + pla.Passwoed + "'";
            //    DS = MySqlHelper.MySqlHelper.Query(str_mysql, LinkString);
            //    if (DS.Tables[0].Rows[0].ItemArray[0].ToString() != null && int.Parse(DS.Tables[0].Rows[0].ItemArray[0].ToString()) > 0)
            //    {
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //else
            //{
            //    return false;
            //}
        }

        //single login
        //0:username or password error
        //1:login success
        //2:pos status error
        //3:tid error
        public static int singleLogin(string LinkString,string company_id,string tid,string delivery_man,string password)
        {
            string str_mysql = "";
            int int_result = 1;
            DataSet DS;
            //check pos status
            str_mysql = "select flag from skt8 where skf97='" + tid + "'";
            DS = MySqlHelper.MySqlHelper.Query(str_mysql, LinkString);
            if(DS.Tables[0].Rows[0].ItemArray[0].ToString() == null)
            {
                int_result = 3;
            }
            else
            {
                if (int.Parse(DS.Tables[0].Rows[0].ItemArray[0].ToString()) == 0)
                {
                    int_result = 2;
                }
                else
                {
                    str_mysql = "select count(skf97) from skt8 where skf97='" + tid + "' and skf104=1 and skf195='" + delivery_man + "' and skf196='" + password + "'";
                    DS = MySqlHelper.MySqlHelper.Query(str_mysql, LinkString);
                    if (DS.Tables[0].Rows[0].ItemArray[0].ToString() != null && int.Parse(DS.Tables[0].Rows[0].ItemArray[0].ToString()) > 0)
                    {
                        int_result = 1;
                    }
                    else
                    {
                        int_result = 0;
                    }
                }
                
            }

            return int_result;
        }
    }
}
