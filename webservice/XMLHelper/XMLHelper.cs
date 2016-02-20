using System.IO;
using System.Xml;

namespace XMLHelper
{
    public class XMLHelper
    {
        public static string Create_XML_Head(string XMLnode, PacketHead_Answer.PacketHead_Answer phan, object an)
        {
            string str_result = "";
            string STDHead = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Transaction>";
            string STDFoot = "</Transaction>";
            string PacketHead_Error = "<Transaction_Header><check_value>9197F7C3A6A0C84DD2CEA55CFE7CE9CE</check_value><requester>1111111111</requester><resp_code>11</resp_code><resp_msg>系统失败</resp_msg><system_serial>130306000674</system_serial><target>301310070118940</target><terminal_eqno>00000000</terminal_eqno><terminal_id>37677018</terminal_id><transaction_id>SAD001</transaction_id></Transaction_Header>";
            string PacketBody_Error = "<Transaction_Body><transaction_id>TRANS001</transaction_id><company_id></company_id><delivery_info>分拣中心</delivery_info><delivery_man>test</delivery_man><delivery_name>test</delivery_name></Transaction_Body>";


            //xml文档  
            XmlDocument xmlDocnew = new XmlDocument();
            XmlDeclaration dec = xmlDocnew.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDocnew.AppendChild(dec);

            //创建根节点   
            XmlElement root = xmlDocnew.CreateElement("Transaction");
            xmlDocnew.AppendChild(root);

            //节点及元素  
            XmlNode Transaction_Header = xmlDocnew.CreateElement("Transaction_Header");
            XmlElement transaction_id = GenClass.GenClass.GetXmlElement(xmlDocnew, "transaction_id", phan.Transaction_id);
            XmlElement requester = GenClass.GenClass.GetXmlElement(xmlDocnew, "requester", phan.Requester);
            XmlElement target = GenClass.GenClass.GetXmlElement(xmlDocnew, "target", phan.Target);
            XmlElement request_time = GenClass.GenClass.GetXmlElement(xmlDocnew, "request_time", phan.Request_time);
            XmlElement system_serial = GenClass.GenClass.GetXmlElement(xmlDocnew, "system_serial", phan.System_serial);
            XmlElement terminal_eqno = GenClass.GenClass.GetXmlElement(xmlDocnew, "terminal_eqno", phan.Terminal_eqno);
            //XmlElement terminal_id = GenClass.GenClass.GetXmlElement(xmlDocnew, "terminal_id", phan.Terminal_id);
            XmlElement resp_code = GenClass.GenClass.GetXmlElement(xmlDocnew, "resp_code", phan.Resp_code);
            XmlElement resp_msg = GenClass.GenClass.GetXmlElement(xmlDocnew, "resp_msg", phan.Resp_msg);
            XmlElement ext_attributes = GenClass.GenClass.GetXmlElement(xmlDocnew, "ext_attributes", phan.Ext_attributes);

            Transaction_Header.AppendChild(transaction_id);
            Transaction_Header.AppendChild(requester);
            Transaction_Header.AppendChild(target);
            Transaction_Header.AppendChild(request_time);
            Transaction_Header.AppendChild(system_serial);
            Transaction_Header.AppendChild(terminal_eqno);
            //Transaction_Header.AppendChild(terminal_id);
            Transaction_Header.AppendChild(resp_code);
            Transaction_Header.AppendChild(resp_msg);
            Transaction_Header.AppendChild(ext_attributes);
            root.AppendChild(Transaction_Header);

            XmlNode Transaction_Body = xmlDocnew.CreateElement("Transaction_Body");

            switch (XMLnode)
            {
                case "TRANS001":
                    {
                        PacketLogin_answer.PacketLogin_answer plan = an as PacketLogin_answer.PacketLogin_answer;

                        XmlElement delivery_man = GenClass.GenClass.GetXmlElement(xmlDocnew, "delivery_man", plan.Delivery_man);
                        //XmlElement company_id = GenClass.GenClass.GetXmlElement(xmlDocnew, "company_id", plan.Company_id);
                        //XmlElement delivery_info = GenClass.GenClass.GetXmlElement(xmlDocnew, "delivery_info", plan.Delivery_info);
                        XmlElement check_value = GenClass.GenClass.GetXmlElement(xmlDocnew, "check_value", plan.Check_value);

                        Transaction_Body.AppendChild(delivery_man);
                        //Transaction_Body.AppendChild(company_id);
                        //Transaction_Body.AppendChild(delivery_info);
                        Transaction_Body.AppendChild(check_value);
                        root.AppendChild(Transaction_Body);

                        StringWriter sw = new StringWriter();
                        xmlDocnew.Save(sw);
                        str_result = sw.ToString();
                        break;
                    }
                case "TRANS003":
                    {
                        break;
                    }

                case "TRANS004":
                    {
                        PacketTrans_answer.PacketTrans_answer ptan = an as PacketTrans_answer.PacketTrans_answer;

                        XmlElement pay_msg = GenClass.GenClass.GetXmlElement(xmlDocnew, "pay_msg", ptan.Pay_msg);
                        XmlElement check_value = GenClass.GenClass.GetXmlElement(xmlDocnew, "check_value", ptan.Check_value);

                        Transaction_Body.AppendChild(pay_msg);
                        Transaction_Body.AppendChild(check_value);
                        root.AppendChild(Transaction_Body);

                        StringWriter sw = new StringWriter();
                        xmlDocnew.Save(sw);
                        str_result = sw.ToString();
                        break;
                    }
                case "TRANS005":
                    {
                        goto case "TRANS004";
                    }
                default:
                    {
                        str_result = STDHead + PacketHead_Error + PacketBody_Error + STDFoot;
                        break;
                    }
            }
            return str_result;
        }
    }
}
