using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data;
using System.Xml;
using System.IO;

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

    public class POSHeader_out : SoapHeader
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
        public string POS_Interface(string in_string = "")
        {
            //读取配置文件config.xml
            if(File.Exists(Server.MapPath("Config.xml")))
            {
                try
                {
                    XmlDocument xmlCon = new XmlDocument();
                    xmlCon.Load(Server.MapPath("Config.xml"));
                    XmlNode xnCon = xmlCon.SelectSingleNode("Config");
                    LinkString = xnCon.SelectSingleNode("LinkString").InnerText;
                }
                catch
                {
                    LinkString = "Server=localhost;user id=root;password=;Database=chenkuserdb37;Port=3308;charset=utf8;";
                }
            }
            else
            {
                LinkString = "Server=localhost;user id=root;password=;Database=chenkuserdb37;Port=3308;charset=utf8;";
            }

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

                if (phas.Transaction_id != null && phas.Transaction_id != "")
                {
                    switch (phas.Transaction_id)
                    {
                        //用户登录
                        case "TRANS001":
                            if (PacketLogin_ask.PacketLogin_ask.setLogin(in_string, LinkString))
                            {
                                PacketLogin_answer.PacketLogin_answer plan = new PacketLogin_answer.PacketLogin_answer();

                                phan.Gen_Answer_XML(true, "", "");

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
                            break;

                        //消费通知
                        case "TRANS004":
                            XmlNode xn_trans004 = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");

                            if (xn_trans004 != null)
                            {
                                if (xn_trans004.SelectSingleNode("check_value").InnerText != null && xn_trans004.SelectSingleNode("check_value").InnerText != "")  //判断效验值,待补充
                                {
                                    PacketTrans_ask.PacketTrans_ask ptas = new PacketTrans_ask.PacketTrans_ask();
                                    string str_mysql = "";

                                    ptas.ReadXML(xn_trans004);

                                    string test_str_mysql = "select * from skt14 where skf160='01' and skf158='" + ptas.Order_no + "'";
                                    DataSet test_DS;
                                    test_DS = MySqlHelper.MySqlHelper.Query(test_str_mysql, LinkString);
                                    if (test_DS.Tables[0].Rows.Count == 0)
                                    {
                                        str_mysql = "insert into skt14(skf158,skf159,skf160,skf161,skf162,skf163,skf164,skf165,skf166,skf167,skf168,skf169,skf170,skf171,skf172,skf173,skf174,skf197,skf198) ";
                                        str_mysql = str_mysql + " value('" + ptas.Order_no + "','" + ptas.Pay_type + "','" + ptas.Trans_type + "'," + ptas.Info_type;
                                        str_mysql = str_mysql + "," + ptas.Net_type + ",'" + ptas.Mid + "','" + ptas.Tid + "','" + ptas.Cardacc_s + "'," + ptas.Amount;
                                        str_mysql = str_mysql + "," + ptas.Pay_amt + "," + ptas.Discount + ",'" + ptas.Pos_serial + "','" + ptas.Pos_setbat;
                                        str_mysql = str_mysql + "','" + ptas.Hostserial + "','" + ptas.Authcode + "','" + ptas.Transtime + "','" + ptas.Check_value + "','" + ptas.Cardnum + "','" + ptas.Cardpass + "')";

                                        int int_result = MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);
                                        //增加手机号码识别,只能积分
                                        if (int_result > 0)
                                        {
                                            string sql;
                                            if ((ptas.Pay_type != "03" || ptas.Pay_type != "04") && ptas.Cardnum == "000000000000")
                                            {
                                                phan.Gen_Answer_XML(true, "", "");
                                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                string epwd = phan.Request_time + ptan.Pay_msg;
                                                ptan.ReadXML(ptan, epwd, "交易成功");
                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);

                                                sql = "update skt14 set skf201=1 where skf158='" + ptas.Order_no + "' ";
                                                int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                            }

                                            if (ptas.Pay_type=="03")
                                            {
                                                sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) and skf95='" + ptas.Cardpass + "' ";
                                            }
                                            else
                                            {
                                                sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "'))) ";
                                            }
                                            DataSet ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                            if (ds_sql.Tables[0].Rows.Count > 0)
                                            {
                                                string txtUserid = ds_sql.Tables[0].Rows[0]["skf36"].ToString();

                                                sql = "select * from skt8 where skf104=1 and skf97='" + ptas.Tid + "' ";
                                                ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                if (ds_sql.Tables[0].Rows.Count > 0)
                                                {
                                                    float floDis = float.Parse(ds_sql.Tables[0].Rows[0]["skf99"].ToString());
                                                    float floJF = ptas.Pay_amt * ((float.Parse("100") - floDis) / float.Parse("100"));
                                                    float floOldJF = 0;
                                                    string strOldMoneyid = "0";
                                                    sql = "select * from skt6 where skf91=1 and skf64='" + txtUserid + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (ds_sql.Tables[0].Rows.Count > 0)
                                                    {
                                                        floOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf66"].ToString());
                                                        strOldMoneyid = ds_sql.Tables[0].Rows[0]["skf63"].ToString();
                                                    }
                                                    if(ptas.Pay_type=="03")
                                                    {
                                                        sql = "update skt6 set skf66=skf66-" + floJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                    }
                                                    else
                                                    {
                                                        sql = "update skt6 set skf66=skf66+" + floJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                    }
                                                    
                                                    int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                    if (intds_sql > 0)
                                                    {
                                                        phan.Gen_Answer_XML(true, "", "");
                                                        PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                        string epwd = phan.Request_time + ptan.Pay_msg;
                                                        ptan.ReadXML(ptan, epwd, "交易成功");
                                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);

                                                        sql = "update skt14 set skf201=1 where skf158='" + ptas.Order_no + "' ";
                                                        intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                        string strNewMoneyid = "";
                                                        float floNewJF = 0;
                                                        sql = "select * from skt6 where skf91=1 and skf64='" + txtUserid + "' ";
                                                        ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                        if (ds_sql.Tables[0].Rows.Count > 0)
                                                        {
                                                            floNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf66"].ToString());
                                                            strNewMoneyid = ds_sql.Tables[0].Rows[0]["skf63"].ToString();
                                                        }

                                                        //插入历史
                                                        sql = "insert into skt7(skf73,skf74,skf75,skf78,skf79,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strNewMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + ptas.Order_no + "',1) ";
                                                        intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                    }
                                                    else
                                                    {
                                                        phan.Gen_Answer_XML(false, "交易失败,未知错误,联系管理员", "");

                                                        PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                        string epwd = phan.Request_time + ptan.Pay_msg;
                                                        ptan.ReadXML(ptan, epwd, "交易失败,未知错误,联系管理员");
                                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                                                      
                                                    }

                                                }
                                                else
                                                {
                                                    phan.Gen_Answer_XML(false, "交易失败,POS机未授权", "");

                                                    PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                    string epwd = phan.Request_time + ptan.Pay_msg;
                                                    ptan.ReadXML(ptan, epwd, "交易失败,POS机未授权");
                                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                                                }
                                            }
                                            else
                                            {
                                                phan.Gen_Answer_XML(false, "交易失败,卡号密码错误", "");

                                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                string epwd = phan.Request_time + ptan.Pay_msg;
                                                ptan.ReadXML(ptan, epwd, "交易失败,卡号密码错误");
                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                                            }
                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(false, "交易失败,未知错误,联系管理员", "");

                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                            ptan.ReadXML(ptan, epwd, "交易失败,未知错误,联系管理员");
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
                                    phan.Gen_Answer_XML(false, "交易失败,效验错误", "");

                                    PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                    string epwd = phan.Request_time + ptan.Pay_msg;
                                    ptan.ReadXML(ptan, epwd, "交易失败,效验错误");
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);
                                }
                            }
                            else
                            {
                                phan.Gen_Answer_XML(false, "交易失败,数据包传输错误", "");

                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                string epwd = phan.Request_time + ptan.Pay_msg;
                                ptan.ReadXML(ptan, epwd, "交易失败,数据包传输错误");
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

                                    string test_str_mysql = "select * from skt14 where skf160='02' and skf158='" + ptas.Order_no + "'";
                                    DataSet test_DS;
                                    test_DS = MySqlHelper.MySqlHelper.Query(test_str_mysql, LinkString);
                                    if (test_DS.Tables[0].Rows.Count == 0)
                                    {
                                        str_mysql = "insert into skt14(skf158,skf159,skf160,skf161,skf162,skf163,skf164,skf165,skf166,skf167,skf168,skf169,skf170,skf171,skf172,skf173,skf174,skf197,skf198) ";
                                        str_mysql = str_mysql + " value('" + ptas.Order_no + "','" + ptas.Pay_type + "','" + ptas.Trans_type + "'," + ptas.Info_type;
                                        str_mysql = str_mysql + "," + ptas.Net_type + ",'" + ptas.Mid + "','" + ptas.Tid + "','" + ptas.Cardacc_s + "'," + ptas.Amount;
                                        str_mysql = str_mysql + "," + ptas.Pay_amt + "," + ptas.Discount + ",'" + ptas.Pos_serial + "','" + ptas.Pos_setbat;
                                        str_mysql = str_mysql + "','" + ptas.Hostserial + "','" + ptas.Authcode + "','" + ptas.Transtime + "','" + ptas.Check_value + "','" + ptas.Cardnum + "','" + ptas.Cardpass + "')";

                                        int int_result = MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);

                                        if (int_result > 0)
                                        {
                                            string sql;
                                            if (ptas.Pay_type == "03")
                                            {
                                                sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) and skf95='" + ptas.Cardpass + "' ";
                                            }
                                            else
                                            {
                                                sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "'))) ";
                                            }
                                            DataSet ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                            if (ds_sql.Tables[0].Rows.Count > 0)
                                            {
                                                string txtUserid = ds_sql.Tables[0].Rows[0]["skf36"].ToString();

                                                sql = "select * from skt8 where skf104=1 and skf97='" + ptas.Tid + "' ";
                                                ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                if (ds_sql.Tables[0].Rows.Count > 0)
                                                {
                                                    float floNewJF = 0;
                                                    float floOldJF = 0;
                                                    string strOldMoneyid = "0";
                                                    //string strNewMoneyid = "0";

                                                    sql = "select * from skt6 where skf91=1 and skf64='" + txtUserid + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (ds_sql.Tables[0].Rows.Count > 0)
                                                    {
                                                        floOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf66"].ToString());
                                                        strOldMoneyid = ds_sql.Tables[0].Rows[0]["skf63"].ToString();
                                                    }
                                                    sql = "select * from skt7 where skf200=1 and skf199='" + ptas.Order_no + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (ds_sql.Tables[0].Rows.Count > 0)
                                                    {
                                                        float floOriNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf77"].ToString());
                                                        float floOriOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf76"].ToString());
                                                        if (floOldJF < (floOriNewJF - floOriOldJF))
                                                        {
                                                            floNewJF = floOldJF - (floOriNewJF - floOriOldJF);
                                                            sql = "update skt6 set skf66=skf66+" + floNewJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                            int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            if (intds_sql > 0)
                                                            {
                                                                phan.Gen_Answer_XML(true, "", "");
                                                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                string epwd = phan.Request_time + ptan.Pay_msg;
                                                                ptan.ReadXML(ptan, epwd, "交易成功");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);

                                                                sql = "update skt14 set skf201=2 where skf158='" + ptas.Order_no + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                //插入历史
                                                                sql = "insert into skt7(skf73,skf74,skf75,skf78,skf79,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strOldMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + ptas.Order_no + "',0) ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            }
                                                            else
                                                            {
                                                                phan.Gen_Answer_XML(false, "交易失败,未知错误,联系管理员", "");

                                                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                                string epwd = phan.Request_time + ptan.Pay_msg;
                                                                ptan.ReadXML(ptan, epwd, "交易失败,未知错误,联系管理员");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            phan.Gen_Answer_XML(false, "交易失败,积分不足", "");

                                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                                            ptan.ReadXML(ptan, epwd, "交易失败,积分不足");
                                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        phan.Gen_Answer_XML(false, "交易失败,交易号未授权", "");

                                                        PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                        string epwd = phan.Request_time + ptan.Pay_msg;
                                                        ptan.ReadXML(ptan, epwd, "交易失败,交易号未授权");
                                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                                    }

                                                }
                                                else
                                                {
                                                    phan.Gen_Answer_XML(false, "交易失败,POS机未授权", "");

                                                    PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                    string epwd = phan.Request_time + ptan.Pay_msg;
                                                    ptan.ReadXML(ptan, epwd, "交易失败,POS机未授权");
                                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                                }
                                            }
                                            else
                                            {
                                                phan.Gen_Answer_XML(false, "交易失败,卡号密码错误", "");

                                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                                string epwd = phan.Request_time + ptan.Pay_msg;
                                                ptan.ReadXML(ptan, epwd, "交易失败,卡号密码错误");
                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                            }
                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(false, "交易失败,未知错误,联系管理员", "");

                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                            ptan.ReadXML(ptan, epwd, "交易失败,未知错误,联系管理员");
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
                                    phan.Gen_Answer_XML(false, "交易失败,效验错误", "");

                                    PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                    string epwd = phan.Request_time + ptan.Pay_msg;
                                    ptan.ReadXML(ptan, epwd, "交易失败,效验错误");
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, ptan);
                                }
                            }
                            else
                            {
                                phan.Gen_Answer_XML(false, "交易失败,数据包传输错误", "");

                                PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                string epwd = phan.Request_time + ptan.Pay_msg;
                                ptan.ReadXML(ptan, epwd, "交易失败,数据包传输错误");
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

        //[WebMethod(Description = "测试_登录")]
        //public bool Login_Interface(string companyid = "", string username = "", string password = "")
        //{
        //    string str_mysql = "select count(username) as coutnum from user_info where company_id='" + companyid + "' and username='" + username + "' and password='" + password + "'";
        //    DataSet DS = MySqlHelper.MySqlHelper.Query(str_mysql, LinkString);
        //    if (DS.Tables[0].Rows[0].ItemArray[0].ToString() != null && int.Parse(DS.Tables[0].Rows[0].ItemArray[0].ToString()) > 0)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //[WebMethod(Description ="测试_交易接口_单包_登录")]
        //public string POS_Interface_Single_Login(string company_id,string tid,string delivery_man,string password,string check_value)
        //{
        //    PacketLogin_ask.PacketLogin_ask plas = new PacketLogin_ask.PacketLogin_ask();
        //    int int_request = PacketLogin_ask.PacketLogin_ask.singleLogin(LinkString, company_id, tid, delivery_man, password);

        //    return "";
        //}

        //[WebMethod(Description = "测试_交易接口_单包_登出")]
        //public string POS_Interface_Single_Logout()
        //{
        //    return "";
        //}

        //[WebMethod(Description = "测试_交易接口_单包_消费")]
        //public string POS_Interface_Single_Consupetion()
        //{
        //    return "";
        //}

        //[WebMethod(Description = "测试_交易接口_单包_撤销")]
        //public string POS_Interface_Single_Cancel()
        //{
        //    return "";
        //}

        //[WebMethod(Description = "测试_交易接口_单包_退货")]
        //public string POS_Interface_Single_UnSell()
        //{
        //    return "";
        //}

        //[WebMethod(Description = "测试_交易接口_单包_储值")]
        //public string POS_Interface_Single_Value()
        //{
        //    return "";
        //}

        //保存反馈的XML信息
        private void R_string(string in_string)
        {
            string str_mysql = "";

            str_mysql = "insert into skt15(skf180,skf181) ";
            str_mysql = str_mysql + " value('" + in_string + "',now())";
            int i = MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);
        }
    }
}
