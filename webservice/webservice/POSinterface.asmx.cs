﻿using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data;
using System.Xml;

namespace WebService
{
    /// <summary>
    /// POSinterface 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://wstest2.china-ess.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    [System.Web.Script.Services.ScriptService]

    //接口头
    public class POSHeader_in : SoapHeader
    {
        public string transaction_id;
        public string requester;
        public string target;
        public string request_time;
        public string terminal_eqno;
        public string terminal_id;
        public string system_serial;
        public string version;
        public string ext_attributes;
    }

    public class POSHeader_out:SoapHeader
    {
        public string transaction_id;
        public string requester;
        public string target;
        public string request_time;
        public string system_serial;
        public string terminal_eqno;
        public string terminal_id;
        public string resp_code;
        public string resp_msg;
        public string ext_attributes;
    }

    //接口主体
    public class POSinterface : System.Web.Services.WebService
    {
        string LinkString = "Server=localhost;user id=root;password=;Database=chenkuserdb37;Port=3308;charset=utf8;";
        string STDHead = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Transaction>";
        string STDFoot = "</Transaction>";
        string PacketHead_Error = "<Transaction_Header><check_value>9197F7C3A6A0C84DD2CEA55CFE7CE9CE</check_value><requester>1111111111</requester><resp_code>11</resp_code><resp_msg>系统失败</resp_msg><system_serial>130306000674</system_serial><target>301310070118940</target><terminal_eqno>00000000</terminal_eqno><terminal_id>37677018</terminal_id><transaction_id>SAD001</transaction_id></Transaction_Header>";
        string PacketBody_Error = "<Transaction_Body><transaction_id>TRANS001</transaction_id><company_id></company_id><delivery_info>分拣中心</delivery_info><delivery_man>test</delivery_man><delivery_name>test</delivery_name></Transaction_Body>";

        [WebMethod(Description = "测试_交易接口_多包统一")]
        public string POS_Interface(string in_string="")
        {
            string str_result = "";
            int SqlResult = 0;
            PacketHead_Ask.PacketHead_Ask phas = new PacketHead_Ask.PacketHead_Ask();
            PacketHead_Answer.PacketHead_Answer phan = new PacketHead_Answer.PacketHead_Answer();
            
            if (GenClass.GenClass.CheckXML(in_string))
            {
                string SqlString = "insert into skt13(skf151,skf152) select '" + in_string + "',now() ";
                SqlResult = MySqlHelper.MySqlHelper.ExecuteSql(SqlString, LinkString);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(in_string);

                // 得到根节点
                XmlNode xn = xmlDoc.SelectSingleNode("Transaction/Transaction_Header");

                // 获取头值,写入应答头值
                phas.ReadXML(xn);
                phan.ReadXMLfromASK(phas);

                if (phas.Transaction_id!=null && phas.Transaction_id!="")
                { 
                    switch (phas.Transaction_id)
                    {
                        //用户登录
                        case "TRANS001":
                            if (PacketLogin_ask.PacketLogin_ask.setLogin(in_string,LinkString))
                            {
                                PacketLogin_answer.PacketLogin_answer plan = new PacketLogin_answer.PacketLogin_answer();

                                phan.Gen_Answer_XML(true, "", "");                       

                                XmlNode xn_login = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");
                                if (xn_login != null)
                                {
                                    string epwd = phan.Request_time + plan.Delivery_man;
                                    plan.ReadXML(xn_login, epwd);
                                    str_result=XMLHelper.XMLHelper.Create_XML_Head("TRANS001", phan, plan);
                                }
                                else
                                {
                                    R_string(STDHead + PacketHead_Error + PacketBody_Error + STDFoot);
                                    return STDHead + PacketHead_Error + PacketBody_Error + STDFoot;
                                }
                            }
                            else
                            {
                                PacketLogin_answer.PacketLogin_answer plan = new PacketLogin_answer.PacketLogin_answer();

                                phan.Gen_Answer_XML(false, "账号密码错误", "");

                                XmlNode xn_login = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");
                                if (xn_login != null)
                                {
                                    string epwd = phan.Request_time + plan.Delivery_man;
                                    plan.ReadXML(xn_login, epwd);
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS001", phan, plan);
                                }
                                else
                                {
                                    R_string(STDHead + PacketHead_Error + PacketBody_Error + STDFoot);
                                    return STDHead + PacketHead_Error + PacketBody_Error + STDFoot;
                                }
                            }
                            break;
                    
                        //订单合并信息查询,待完成
                        case "TRANS003":
                            //XmlNode xn_order = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");
                            //if(xn_order!=null)
                            //{
                            //    if(xn_order.SelectSingleNode("check_value").InnerText!=null && xn_order.SelectSingleNode("check_value").InnerText !="") //判断效验值,待补充
                            //    {
                            //        PacketOrder_ask.PacketOrder_ask poas = new PacketOrder_ask.PacketOrder_ask();

                            //        poas.Trans_type = xn_order.SelectSingleNode("trans_type").InnerText;
                            //        switch (poas.Trans_type)
                            //        {
                            //            case "01"://查询
                            //                string str_mysql = "";
                            //                DataSet DS;
                            //                int int_sqlresult = 0;

                            //                poas.Order_count = int.Parse(xn_order.SelectSingleNode("order_count").InnerText);
                            //                poas.Order_set = xn_order.SelectSingleNode("order_set").InnerText;

                            //                str_mysql = "select order_no,pay_amt from order_info where order_no in (" + poas.Order_set + ")";
                            //                DS = MySqlHelper.MySqlHelper.Query(str_mysql, LinkString);
                            //                int_sqlresult = DS.Tables[0].Rows.Count;

                            //                break;
                            //            case "02"://揽件
                            //                break;                                
                            //        }
                            //    }
                            //}
                            break;
                    
                        //消费通知
                        case "TRANS004":
                            XmlNode xn_trans004 = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");
               
                            if(xn_trans004!=null)
                            {
                                if(xn_trans004.SelectSingleNode("check_value").InnerText!=null && xn_trans004.SelectSingleNode("check_value").InnerText != "")  //判断效验值,待补充
                                {
                                    PacketTrans_ask.PacketTrans_ask ptas = new PacketTrans_ask.PacketTrans_ask();
                                    string str_mysql = "";

                                    ptas.ReadXML(xn_trans004);

                                    string test_str_mysql = "select * from skt14 where skf158='" + ptas.Order_no + "'";
                                    DataSet test_DS;
                                    test_DS = MySqlHelper.MySqlHelper.Query(test_str_mysql, LinkString);
                                    if (test_DS.Tables[0].Rows.Count == 0)
                                    {
                                        str_mysql = "insert into skt14(skf158,skf159,skf160,skf161,skf162,skf163,skf164,skf165,skf166,skf167,skf168,skf169,skf170,skf171,skf172,skf173,skf174,skf197,skf198) ";
                                        str_mysql = str_mysql + " value('" + ptas.Order_no + "','" + ptas.Pay_type + "','" + ptas.Trans_type + "'," + "";
                                        str_mysql = str_mysql + "," + "" + ",'" + ptas.Mid + "','" + ptas.Tid + "','" + ptas.Cardacc_s + "'," + ptas.Amount;
                                        str_mysql = str_mysql + "," + ptas.Pay_amt + "," + ptas.Discount + ",'" + ptas.Pos_serial + "','" + ptas.Pos_setbat;
                                        str_mysql = str_mysql + "','" + ptas.Hostserial + "','" + ptas.Authcode + "','" + ptas.Transtime + "','" + ptas.Check_value + "','" + ptas.Cardnum + "','" + ptas.Cardpass + "')";

                                        int int_result = MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);

                                        if (int_result > 0)
                                        {
                                            phan.Gen_Answer_XML(true, "", "");

                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                            ptan.ReadXML(ptan, epwd, "交易成功");
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);

                                            //调用相关过程,待开发

                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(false, "交易失败", "");

                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                            ptan.ReadXML(ptan, epwd, "交易失败");
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                                        }
                                    }
                                    else
                                    {
                                        phan.Gen_Answer_XML(false, "交易失败,交易号重复", "");

                                        PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                        string epwd = phan.Request_time + ptan.Pay_msg;
                                        ptan.ReadXML(ptan, epwd, "交易失败,交易号重复");
                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                                    }
                                }
                                else
                                {
                                    phan.Gen_Answer_XML(false, "交易失败", "");

                                    PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                    string epwd = phan.Request_time + ptan.Pay_msg;
                                    ptan.ReadXML(ptan, epwd, "交易失败");
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                                }
                            }
                            else
                            {
                                phan.Gen_Answer_XML(false, "交易失败", "");

                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                string epwd = phan.Request_time + ptan.Pay_msg;
                                ptan.ReadXML(ptan, epwd, "交易失败");
                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                            }
                            break;

                        //撤销通知
                        case "TRANS005":
                            XmlNode xn_trans005 = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");

                            if (xn_trans005 != null)
                            {
                                if (xn_trans005.SelectSingleNode("check_value").InnerText != null && xn_trans005.SelectSingleNode("check_value").InnerText != "")  //判断效验值,待补充
                                {
                                    PacketTrans_ask.PacketTrans_ask ptas = new PacketTrans_ask.PacketTrans_ask();
                                    string str_mysql = "";

                                    ptas.ReadXML(xn_trans005);

                                    string test_str_mysql = "select * from skt14 where skf158='" + ptas.Order_no + "'";
                                    DataSet test_DS;
                                    test_DS = MySqlHelper.MySqlHelper.Query(test_str_mysql, LinkString);
                                    if (test_DS.Tables[0].Rows.Count == 0)
                                    {
                                        str_mysql = "insert into skt14(skf158,skf159,skf160,skf161,skf162,skf163,skf164,skf165,skf166,skf167,skf168,skf169,skf170,skf171,skf172,skf173,skf174,skf197,skf198) ";
                                        str_mysql = str_mysql + " value('" + ptas.Order_no + "','" + ptas.Pay_type + "','" + ptas.Trans_type + "'," + "";
                                        str_mysql = str_mysql + "," + "" + ",'" + ptas.Mid + "','" + ptas.Tid + "','" + ptas.Cardacc_s + "'," + ptas.Amount;
                                        str_mysql = str_mysql + "," + ptas.Pay_amt + "," + ptas.Discount + ",'" + ptas.Pos_serial + "','" + ptas.Pos_setbat;
                                        str_mysql = str_mysql + "','" + ptas.Hostserial + "','" + ptas.Authcode + "','" + ptas.Transtime + "','" + ptas.Check_value + "','" + ptas.Cardnum + "','" + ptas.Cardpass + "')";

                                        int int_result = MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);

                                        if (int_result > 0)
                                        {
                                            phan.Gen_Answer_XML(true, "", "");

                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                            ptan.ReadXML(ptan, epwd, "交易成功");
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);

                                            //调用相关过程,待开发

                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(false, "交易失败", "");

                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                            ptan.ReadXML(ptan, epwd, "交易失败");
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                        }
                                    }
                                    else
                                    {
                                        phan.Gen_Answer_XML(false, "交易失败,交易号重复", "");

                                        PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                        string epwd = phan.Request_time + ptan.Pay_msg;
                                        ptan.ReadXML(ptan, epwd, "交易失败,交易号重复");
                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                    }
                                }
                                else
                                {
                                    phan.Gen_Answer_XML(false, "交易失败", "");

                                    PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                    string epwd = phan.Request_time + ptan.Pay_msg;
                                    ptan.ReadXML(ptan, epwd, "交易失败");
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                }
                            }
                            else
                            {
                                phan.Gen_Answer_XML(false, "交易失败", "");

                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                string epwd = phan.Request_time + ptan.Pay_msg;
                                ptan.ReadXML(ptan, epwd, "交易失败");
                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                            }
                            break;

                        //异常件通知
                        case "TRANS006":
                            break;

                        //Error
                        default:
                            R_string(STDHead + PacketHead_Error + PacketBody_Error + STDFoot);
                            return STDHead + PacketHead_Error + PacketBody_Error + STDFoot;
                    }
                }

            }
            else
            {
                R_string(STDHead + PacketHead_Error + PacketBody_Error + STDFoot);
                return STDHead + PacketHead_Error + PacketBody_Error + STDFoot;
            }
            R_string(str_result);
            return str_result;
        }

        [WebMethod(Description ="测试_登录")]
        public bool Login_Interface(string companyid="",string username="",string password="")
        {
            string str_mysql = "select count(username) as coutnum from user_info where company_id='"+ companyid + "' and username='" + username + "' and password='" + password + "'";
            DataSet DS = MySqlHelper.MySqlHelper.Query(str_mysql, LinkString);
            if (DS.Tables[0].Rows[0].ItemArray[0].ToString() != null && int.Parse(DS.Tables[0].Rows[0].ItemArray[0].ToString()) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [WebMethod(Description ="测试_交易接口_单包_登录")]
        public string POS_Interface_Single_Login(string company_id,string tid,string delivery_man,string password,string check_value)
        {
            PacketLogin_ask.PacketLogin_ask plas = new PacketLogin_ask.PacketLogin_ask();
            int int_request = PacketLogin_ask.PacketLogin_ask.singleLogin(LinkString, company_id, tid, delivery_man, password);

            return "";
        }

        [WebMethod(Description = "测试_交易接口_单包_登出")]
        public string POS_Interface_Single_Logout()
        {
            return "";
        }

        [WebMethod(Description = "测试_交易接口_单包_消费")]
        public string POS_Interface_Single_Consupetion()
        {
            return "";
        }

        [WebMethod(Description = "测试_交易接口_单包_撤销")]
        public string POS_Interface_Single_Cancel()
        {
            return "";
        }

        [WebMethod(Description = "测试_交易接口_单包_退货")]
        public string POS_Interface_Single_UnSell()
        {
            return "";
        }

        [WebMethod(Description = "测试_交易接口_单包_储值")]
        public string POS_Interface_Single_Value()
        {
            return "";
        }

        //保存反馈的XML信息
        private void R_string(string in_string)
        {
            string str_mysql = "";

            str_mysql = "insert into request_string(request_string,create_datetime) ";
            str_mysql = str_mysql + " value('" + in_string + "',now())";
            int i= MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);
        }
    }
}
