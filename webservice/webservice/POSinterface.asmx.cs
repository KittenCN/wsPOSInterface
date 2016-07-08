using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data;
using System.Xml;
using System.IO;
using System;

namespace WebService
{
    /// <summary>
    /// POSinterface 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://61.129.70.241/")]
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
        string PacketBody_Error = "<Transaction_Body><transaction_id>TRANS001</transaction_id><company_id></company_id><delivery_info>Service Center</delivery_info><delivery_man>test</delivery_man><delivery_name>test</delivery_name></Transaction_Body>";
        string pKey = "";

        [WebMethod(Description = "霓虹POS接口_多包统一")]
        public string POS_Interface(string in_string = "")
        {
            //读取配置文件config.xml
            string strLocalAdd = "C:\\Config.xml";
            if (File.Exists(strLocalAdd))
            {
                try
                {
                    XmlDocument xmlCon = new XmlDocument();
                    xmlCon.Load(strLocalAdd);
                    XmlNode xnCon = xmlCon.SelectSingleNode("Config");
                    LinkString = xnCon.SelectSingleNode("LinkString").InnerText;
                    pKey = xnCon.SelectSingleNode("pKey").InnerText;
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
                            string strpKey = "";
                            string strBatchNum = "";

                            if (PacketLogin_ask.PacketLogin_ask.setLogin(in_string, LinkString))
                            {
                                PacketLogin_answer.PacketLogin_answer plan = new PacketLogin_answer.PacketLogin_answer();

                                phan.Gen_Answer_XML(true, "", "");

                                XmlNode xn_login = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");
                                if (xn_login != null)
                                {
                                    string strSQL = "select skf205,skf229 from skt17 where skf204=1 and skf203='" + phas.Terminal_eqno + "' ";
                                    DataSet DS;
                                    DS = MySqlHelper.MySqlHelper.Query(strSQL, LinkString);
                                    if (DS.Tables[0].Rows.Count > 0)   //取消未登出不能登入限制
                                    {
                                        strpKey = DS.Tables[0].Rows[0][0].ToString();
                                        strBatchNum = DS.Tables[0].Rows[0][1].ToString();

                                        string epwd = phan.Request_time + plan.Delivery_man;
                                        plan.ReadXML(xn_login, epwd, pKey,strpKey,strBatchNum);
                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS001", phan, plan);

                                        ///取消登录记录
                                        //strSQL = "insert into skt17(skf203,skf204,skf205,skf206,skf208) ";
                                        //strSQL = strSQL + "value('" + phas.Terminal_eqno + "',0,'" + plan.Public_key + "','" + System.DateTime.Now.ToString() + "','" + plan.Delivery_man + "') ";
                                        //DS = MySqlHelper.MySqlHelper.Query(strSQL, LinkString);
                                    }
                                    else
                                    {
                                        phan.Gen_Answer_XML(false, "登录失败,POS机号未登记", "");
                                        string epwd = phan.Request_time + plan.Delivery_man;
                                        plan.ReadXML(xn_login, epwd, pKey, strpKey, strBatchNum);
                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS001", phan, plan);
                                    }
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
                                phan.Gen_Answer_XML(false, "登录失败,账号密码错误", "");

                                XmlNode xn_login = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");
                                if (xn_login != null)
                                {
                                    string epwd = phan.Request_time + plan.Delivery_man;
                                    plan.ReadXML(xn_login, epwd, pKey, strpKey, strBatchNum);
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
                        //case "TRANS003":
                        //    break;

                        //消费通知
                        case "TRANS004":
                            XmlNode xn_trans004 = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");

                            if (xn_trans004 != null)
                            {
                                //取消登录判断
                                //string strSQL = "select * from skt17 where skf204=0 and skf203='" + phas.Terminal_eqno + "' ";
                                string strSQL = "select * from skt17 where skf204=1 and skf203='" + phas.Terminal_eqno + "' ";
                                DataSet DS;
                                DS = MySqlHelper.MySqlHelper.Query(strSQL, LinkString);
                                if (DS.Tables[0].Rows.Count == 1 && xn_trans004.SelectSingleNode("check_value").InnerText != null && xn_trans004.SelectSingleNode("check_value").InnerText != "")  //判断效验值,待补充
                                {
                                    PacketTrans_ask.PacketTrans_ask ptas = new PacketTrans_ask.PacketTrans_ask();
                                    EnDeCode.EnDeCode edc = new EnDeCode.EnDeCode();
                                    string str_mysql = "";
                                    string strEnpKey = DS.Tables[0].Rows[0]["skf205"].ToString();
                                    string strDepKey = edc.DesDecrypt(strEnpKey, pKey);

                                    ptas.ReadXML(xn_trans004);

                                    string test_str_mysql = "select * from skt14 where skf160='01' and skf158='" + ptas.Order_no + "'";
                                    DataSet test_DS;
                                    test_DS = MySqlHelper.MySqlHelper.Query(test_str_mysql, LinkString);
                                    if (test_DS.Tables[0].Rows.Count == 0)
                                    {
                                        str_mysql = "insert into skt14(skf158,skf159,skf160,skf161,skf162,skf163,skf164,skf165,skf166,skf167,skf168,skf169,skf170,skf171,skf172,skf173,skf174,skf197,skf198,skf233) ";
                                        str_mysql = str_mysql + " value('" + ptas.Order_no + "','" + ptas.Pay_type + "','" + ptas.Trans_type + "'," + ptas.Info_type;
                                        str_mysql = str_mysql + "," + ptas.Net_type + ",'" + ptas.Mid + "','" + ptas.Tid + "','" + ptas.Cardacc_s + "'," + ptas.Amount;
                                        str_mysql = str_mysql + "," + ptas.Pay_amt + "," + ptas.Discount + ",'" + ptas.Pos_serial + "','" + ptas.Pos_setbat;
                                        str_mysql = str_mysql + "','" + ptas.Hostserial + "','" + ptas.Authcode + "','" + ptas.Transtime + "','" + ptas.Check_value + "','" + ptas.Cardnum + "','" + ptas.Cardpass + "','" + ptas.Posid + "')";

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
                                            else
                                            {
                                                if (ptas.Pay_type == "03")
                                                {
                                                    string strInSql= "select skvf12,skvf2 from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) ";
                                                    string DePass = "";
                                                    string strUserID = "";
                                                    DataSet dsInSql= MySqlHelper.MySqlHelper.Query(strInSql, LinkString);
                                                    if (dsInSql.Tables[0].Rows.Count > 0)
                                                    {
                                                        DePass = dsInSql.Tables[0].Rows[0][0].ToString();
                                                        strUserID = dsInSql.Tables[0].Rows[0][1].ToString();
                                                        //更新当面明文密码的加密密码
                                                        string strEnSql = "update skt4 set skf228='" + edc.GetFCS(edc.GetMD5(edc.GetASCII(DePass))) + "' where skf36='" + strUserID + "' ";
                                                        DataSet dsTemp = MySqlHelper.MySqlHelper.Query(strEnSql, LinkString);
                                                    }
                                                    //判断加密后密码的匹配性
                                                    sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) and skvf21='" + edc.DesDecrypt(ptas.Cardpass, strDepKey) + "' ";
                                                }
                                                else
                                                {
                                                    sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) ";
                                                }
                                                DataSet ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                if (ds_sql.Tables[0].Rows.Count > 0)
                                                {
                                                    string txtUserid = ds_sql.Tables[0].Rows[0]["skvf2"].ToString();

                                                    sql = "select * from skt8 where skf104=1 and skf97='" + ptas.Tid + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (ds_sql.Tables[0].Rows.Count > 0)
                                                    {
                                                        float floDis = float.Parse(ds_sql.Tables[0].Rows[0]["skf99"].ToString());
                                                        float floNum = float.Parse(ds_sql.Tables[0].Rows[0]["skf230"].ToString());
                                                        //float floJF = ptas.Pay_amt * ((float.Parse("100") - floDis) / float.Parse("100"));
                                                        //int intJF = Convert.ToInt32(ptas.Pay_amt * floDis * floNum);
                                                        int intJF = 0;
                                                        float floOldJF = 0;
                                                        float floNewJF = 0;
                                                        string strOldMoneyid = "0";
                                                        sql = "select * from skt6 where skf91=1 and skf64='" + txtUserid + "' ";
                                                        ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                        if (ds_sql.Tables[0].Rows.Count > 0)
                                                        {
                                                            floOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf67"].ToString());
                                                            strOldMoneyid = ds_sql.Tables[0].Rows[0]["skf63"].ToString();
                                                        }
                                                        //修改当天生成积分至冻结积分,待夜间由基础服务统计
                                                        if (ptas.Pay_type == "03")
                                                        {
                                                            intJF = Convert.ToInt32(ptas.Pay_amt * floNum);
                                                            floOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf66"].ToString());
                                                            sql = "update skt6 set skf66=skf66-" + intJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                        }
                                                        else
                                                        {
                                                            intJF = Convert.ToInt32(ptas.Pay_amt * floDis);
                                                            floOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf67"].ToString());
                                                            sql = "update skt6 set skf67=skf67+" + intJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                        }

                                                        int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                        if (intds_sql > 0)
                                                        {
                                                            phan.Gen_Answer_XML(true, "", "");
                                                            PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                            string epwd = phan.Request_time + ptan.Pay_msg;
                                                            ptan.ReadXML(ptan, epwd, "交易成功");
                                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS004", phan, ptan);

                                                            if(ptas.Pay_type!="03")
                                                            {
                                                                string strSMS = "select skf26 from skt3 where skf53=1 and skf20='" + txtUserid + "' ";
                                                                DataSet dsSMS = MySqlHelper.MySqlHelper.Query(strSMS, LinkString);
                                                                if(dsSMS.Tables[0].Rows.Count>0)
                                                                {
                                                                    SMS.SMS sms = new SMS.SMS();
                                                                    sms.send_reg_sms(1,2, dsSMS.Tables[0].Rows[0].ItemArray[0].ToString(), intJF);
                                                                }                                                               
                                                            }
                                                            else
                                                            {
                                                                string strSMS = "select skf26 from skt3 where skf53=1 and skf20='" + txtUserid + "' ";
                                                                DataSet dsSMS = MySqlHelper.MySqlHelper.Query(strSMS, LinkString);
                                                                if (dsSMS.Tables[0].Rows.Count > 0)
                                                                {
                                                                    SMS.SMS sms = new SMS.SMS();
                                                                    sms.send_reg_sms(2, 2, dsSMS.Tables[0].Rows[0].ItemArray[0].ToString(), intJF);
                                                                }
                                                            }
                                                            //skf201 0:异常   1:正常    2:冲正    3:撤销
                                                            sql = "update skt14 set skf201=1 where skf158='" + ptas.Order_no + "' ";
                                                            intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                            string strNewMoneyid = "";                                                          
                                                            sql = "select * from skt6 where skf91=1 and skf64='" + txtUserid + "' ";
                                                            ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                            if (ds_sql.Tables[0].Rows.Count > 0)
                                                            {
                                                                //floNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf67"].ToString());
                                                                strNewMoneyid = ds_sql.Tables[0].Rows[0]["skf63"].ToString();
                                                                if (ptas.Pay_type == "03")
                                                                {
                                                                    floNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf66"].ToString());
                                                                    sql = "insert into skt7(skf73,skf74,skf75,skf78,skf79,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strNewMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + ptas.Order_no + "',1) ";
                                                                }
                                                                else
                                                                {
                                                                    floNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf67"].ToString());
                                                                    sql = "insert into skt7(skf73,skf74,skf75,skf80,skf81,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strNewMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + ptas.Order_no + "',1) ";
                                                                }
                                                            }

                                                            //插入历史
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
                                    phan.Gen_Answer_XML(false, "交易失败,效验错误或POS设备未登记", "");

                                    PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();

                                    string epwd = phan.Request_time + ptan.Pay_msg;
                                    ptan.ReadXML(ptan, epwd, "交易失败,效验错误或POS设备未登记");
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

                        //冲正通知
                        case "TRANS005":
                            XmlNode xn_trans005 = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");

                            if (xn_trans005 != null)
                            {
                                //取消登录判断
                                //string strSQL = "select * from skt17 where skf204=0 and skf203='" + phas.Terminal_eqno + "' ";
                                string strSQL = "select * from skt17 where skf204=1 and skf203='" + phas.Terminal_eqno + "' ";
                                DataSet DS;
                                DS = MySqlHelper.MySqlHelper.Query(strSQL, LinkString);
                                if (DS.Tables[0].Rows.Count == 1 && xn_trans005.SelectSingleNode("check_value").InnerText != null && xn_trans005.SelectSingleNode("check_value").InnerText != "")  //判断效验值,待补充
                                {
                                    //PacketTrans_ask.PacketTrans_ask ptas = new PacketTrans_ask.PacketTrans_ask();
                                    PacketWash_ask.PacketWash_ask pwas = new PacketWash_ask.PacketWash_ask();
                                    EnDeCode.EnDeCode edc = new EnDeCode.EnDeCode();
                                    string str_mysql = "";
                                    string strEnpKey = DS.Tables[0].Rows[0]["skf205"].ToString();
                                    string strDepKey = edc.DesDecrypt(strEnpKey, pKey);
                                    pwas.ReadXML(xn_trans005);
                                    string test_str_mysql = "select * from skt14 where skf160='02' and skf158='" + pwas.Order_no + "'";
                                    DataSet test_DS;
                                    test_DS = MySqlHelper.MySqlHelper.Query(test_str_mysql, LinkString);
                                    if (test_DS.Tables[0].Rows.Count == 0)
                                    {
                                        str_mysql = "insert into skt14(skf158,skf232,skf159,skf160,skf161,skf162,skf163,skf164,skf165,skf166,skf167,skf168,skf169,skf170,skf171,skf172,skf173,skf174,skf197,skf198) ";
                                        str_mysql = str_mysql + " value('" + pwas.Order_no + "','" + pwas.Order_no_washed +"','" + pwas.Pay_type + "','" + pwas.Trans_type + "'," + pwas.Info_type;
                                        str_mysql = str_mysql + "," + pwas.Net_type + ",'" + pwas.Mid + "','" + pwas.Tid + "','" + pwas.Cardacc_s + "'," + pwas.Amount;
                                        str_mysql = str_mysql + "," + pwas.Pay_amt + "," + pwas.Discount + ",'" + pwas.Pos_serial + "','" + pwas.Pos_setbat;
                                        str_mysql = str_mysql + "','" + pwas.Hostserial + "','" + pwas.Authcode + "','" + pwas.Transtime + "','" + pwas.Check_value + "','" + pwas.Cardnum + "','" + pwas.Cardpass + "')";

                                        int int_result = MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);
                                        if (int_result > 0)
                                        {
                                            string sql;
                                            //取消交易,不验证密码
                                            sql = "select * from skv1 where ((skvf7='" + pwas.Cardnum + "' and skvf10=1) or (skvf8='" + pwas.Cardnum + "' and skvf11=1) or (skvf20='" + pwas.Cardnum + "')) ";
                                            //if (ptas.Pay_type == "03")
                                            //{
                                            //    string strInSql = "select skfv12,skvf2 from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) ";
                                            //    string DePass = "";
                                            //    string strUserID = "";
                                            //    DataSet dsInSql = MySqlHelper.MySqlHelper.Query(strInSql, LinkString);
                                            //    if (dsInSql.Tables[0].Rows.Count > 0)
                                            //    {
                                            //        DePass = dsInSql.Tables[0].Rows[0][0].ToString();
                                            //        strUserID = dsInSql.Tables[0].Rows[0][1].ToString();
                                            //        string strEnSql = "update skt4 set skf228='" + edc.GetFCS(edc.GetMD5(edc.GetASCII(DePass))) + "' where skf36='" + strUserID + "' ";
                                            //        DataSet dsTemp = MySqlHelper.MySqlHelper.Query(strEnSql, LinkString);
                                            //    }
                                            //    sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) and skvf21='" + edc.DesDecrypt(ptas.Cardpass, strDepKey) + "' ";
                                            //}
                                            //else
                                            //{
                                            //    sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) ";
                                            //}
                                            DataSet ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                            if (ds_sql.Tables[0].Rows.Count > 0)
                                            {
                                                string txtUserid = ds_sql.Tables[0].Rows[0]["skvf2"].ToString();

                                                sql = "select * from skt8 where skf104=1 and skf97='" + pwas.Tid + "' ";
                                                ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                if (ds_sql.Tables[0].Rows.Count > 0)
                                                {
                                                    float floNewJF = 0;
                                                    float floOldJF = 0;
                                                    float floOldCurJF = 0;
                                                    string strOldMoneyid = "0";
                                                    //string strNewMoneyid = "0";

                                                    sql = "select * from skt6 where skf91=1 and skf64='" + txtUserid + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (ds_sql.Tables[0].Rows.Count > 0)
                                                    {
                                                        floOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf66"].ToString());
                                                        floOldCurJF = float.Parse(ds_sql.Tables[0].Rows[0]["skt67"].ToString());
                                                        strOldMoneyid = ds_sql.Tables[0].Rows[0]["skf63"].ToString();
                                                    }
                                                    sql = "select * from skt14 where skf158='" + pwas.Order_no_washed + "' ";
                                                    DataSet dsData = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    sql = "select * from skt7 where skf200=1 and skf199='" + pwas.Order_no_washed + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (dsData.Tables[0].Rows[0]["skf159"].ToString() == "03")    
                                                    {
                                                        float floOriNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf79"].ToString());
                                                        float floOriOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf78"].ToString());
                                                        if (ds_sql.Tables[0].Rows.Count > 0)
                                                        {
                                                            //floNewJF = floOldJF - (floOriNewJF - floOriOldJF);
                                                            floNewJF = floOriNewJF - floOriOldJF;
                                                            sql = "update skt6 set skf66=skf66-" + floNewJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                            int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            if (intds_sql > 0)
                                                            {
                                                                phan.Gen_Answer_XML(true, "", "");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易成功");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);

                                                                sql = "update skt14 set skf201=1 where skf158='" + pwas.Order_no + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                sql = "update skt14 set skf201=2 where skf158='" + pwas.Order_no_washed + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                //插入历史
                                                                sql = "insert into skt7(skf73,skf74,skf75,skf78,skf79,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strOldMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + pwas.Order_no + "',0) ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            }
                                                            else
                                                            {
                                                                phan.Gen_Answer_XML(true, "", "交易失败,未知错误,联系管理员");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易失败,未知错误,联系管理员");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            phan.Gen_Answer_XML(true, "", "交易失败,积分不足");
                                                            //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                            PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                            string epwd = phan.Request_time + pwan.Pay_msg;
                                                            pwan.ReadXML(pwan, epwd, "交易失败,积分不足");
                                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        float floOriNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf81"].ToString());
                                                        float floOriOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf80"].ToString());
                                                        if (ds_sql.Tables[0].Rows.Count > 0)
                                                        {
                                                            //floNewJF = floOldJF - (floOriNewJF - floOriOldJF);
                                                            floNewJF = floOriNewJF - floOriOldJF;
                                                            sql = "update skt6 set skf67=skf67-" + floNewJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                            int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            if (intds_sql > 0)
                                                            {
                                                                phan.Gen_Answer_XML(true, "", "");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易成功");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);

                                                                sql = "update skt14 set skf201=1 where skf158='" + pwas.Order_no + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                sql = "update skt14 set skf201=2 where skf158='" + pwas.Order_no_washed + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                //插入历史
                                                                sql = "insert into skt7(skf73,skf74,skf75,skf80,skf81,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strOldMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + pwas.Order_no + "',0) ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            }
                                                            else
                                                            {
                                                                phan.Gen_Answer_XML(true, "", "交易失败,未知错误,联系管理员");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易失败,未知错误,联系管理员");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            phan.Gen_Answer_XML(true, "", "交易失败,积分不足");
                                                            //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                            PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                            string epwd = phan.Request_time + pwan.Pay_msg;
                                                            pwan.ReadXML(pwan, epwd, "交易失败,积分不足");
                                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    phan.Gen_Answer_XML(true, "", "交易失败,POS机未授权");
                                                    //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                    PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                    string epwd = phan.Request_time + pwan.Pay_msg;
                                                    pwan.ReadXML(pwan, epwd, "交易失败,POS机未授权");
                                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                }
                                            }
                                            else
                                            {
                                                phan.Gen_Answer_XML(true, "", "交易失败,卡号密码错误");
                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                pwan.ReadXML(pwan, epwd, "交易失败,卡号密码错误");
                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                            }
                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(true, "", "交易失败,未知错误,联系管理员");
                                            //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                            PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                            string epwd = phan.Request_time + pwan.Pay_msg;
                                            pwan.ReadXML(pwan, epwd, "交易失败,未知错误,联系管理员");
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                        }
                                    }
                                    else
                                    {
                                        phan.Gen_Answer_XML(true, "", "交易失败,交易号重复");
                                        //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                        PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                        string epwd = phan.Request_time + pwan.Pay_msg;
                                        pwan.ReadXML(pwan, epwd, "交易失败,交易号重复");
                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                    }
                                }
                                else
                                {
                                    phan.Gen_Answer_XML(true, "", "交易失败,效验错误或POS设备未登记");
                                    //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                    PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                    string epwd = phan.Request_time + pwan.Pay_msg;
                                    pwan.ReadXML(pwan, epwd, "交易失败,效验错误或POS设备未登记");
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                }
                            }
                            else
                            {
                                phan.Gen_Answer_XML(true, "", "交易失败,数据包传输错误");
                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                string epwd = phan.Request_time + pwan.Pay_msg;
                                pwan.ReadXML(pwan, epwd, "交易失败,数据包传输错误");
                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                            }
                            break;

                        //登出及对账
                        case "TRANS006":
                            XmlNode xn_trans006 = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");
                            
                            if (xn_trans006 != null)
                            {
                                PacketLogout_ask.PacketLogout_ask plas = new PacketLogout_ask.PacketLogout_ask();
                                plas.ReadXML(xn_trans006);
                                string strSQL = "select * from skt17 where skf204=0 and skf203='" + phas.Terminal_eqno + "' ";
                                DataSet DS;
                                DS = MySqlHelper.MySqlHelper.Query(strSQL, LinkString);
                                if (DS.Tables[0].Rows.Count == 1 && xn_trans006.SelectSingleNode("check_value").InnerText != null && xn_trans006.SelectSingleNode("check_value").InnerText != "")
                                {
                                    string strID = DS.Tables[0].Rows[0][0].ToString();
                                    DateTime dtBeginDateTime = DateTime.Parse(DS.Tables[0].Rows[0]["skf206"].ToString());
                                    string strBeginDateTime = dtBeginDateTime.ToString("yyyyMMddHHmmss");
                                    strSQL = "SELECT COUNT(*),IF(ISNULL(SUM(skf166)),0,SUM(skf166)) FROM skt14 WHERE skf201=1 and skf164='" + plas.Tid + "' and skf173>='" + strBeginDateTime + "' ";
                                    DataSet dsDT;
                                    dsDT = MySqlHelper.MySqlHelper.Query(strSQL, LinkString);
                                    if(dsDT.Tables[0].Rows.Count>0)
                                    {
                                        if((int.Parse(dsDT.Tables[0].Rows[0][0].ToString()) == int.Parse(plas.Total_times.ToString()) && float.Parse(dsDT.Tables[0].Rows[0][1].ToString()) == float.Parse(plas.Total_amount.ToString())) || 1==1)   //临时取消登出金额,次数比对限制
                                        {
                                            phan.Gen_Answer_XML(true, "", "");
                                            PacketLogout_answer.PacketLogout_answer plan = new PacketLogout_answer.PacketLogout_answer();
                                            string epwd = phan.Request_time + phan.Resp_msg;
                                            plan.ReadXML(plas, epwd);
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS006", phan, plan);
                                            string sql = "update skt17 set skf204=1,skf207='" + System.DateTime.Now.ToString() + "' where skf202='" + strID + "' ";
                                            int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(false, "登出失败,总次数或总金额不匹配", "");
                                            PacketLogout_answer.PacketLogout_answer plan = new PacketLogout_answer.PacketLogout_answer();
                                            string epwd = phan.Request_time + phan.Resp_msg;
                                            plan.ReadXML(plas, epwd);
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS006", phan, plan);
                                        }
                                    }
                                    else
                                    {
                                        if ((int.Parse(plas.Total_times.ToString()) == 0 && float.Parse(plas.Total_amount.ToString()) == 0) || 1==1)    //临时取消登出金额,次数比对限制
                                        {
                                            phan.Gen_Answer_XML(true, "", "");
                                            PacketLogout_answer.PacketLogout_answer plan = new PacketLogout_answer.PacketLogout_answer();
                                            string epwd = phan.Request_time + phan.Resp_msg;
                                            plan.ReadXML(plas, epwd);
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS006", phan, plan);
                                            string sql = "update skt17 set skf204=1,skf207='" + System.DateTime.Now.ToString() + "' where skf202='" + strID + "' ";
                                            int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(false, "登出失败,总次数或总金额不匹配", "");
                                            PacketLogout_answer.PacketLogout_answer plan = new PacketLogout_answer.PacketLogout_answer();
                                            string epwd = phan.Request_time + phan.Resp_msg;
                                            plan.ReadXML(plas, epwd);
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS006", phan, plan);
                                        }
                                    }                              
                                }
                                else
                                {
                                    phan.Gen_Answer_XML(false, "登出失败,效验错误或POS设备未登录", "");
                                    PacketLogout_answer.PacketLogout_answer plan = new PacketLogout_answer.PacketLogout_answer();
                                    string epwd = phan.Request_time + phan.Resp_msg;
                                    plan.ReadXML(plas, epwd);
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS006", phan, plan);
                                }
                            }
                            else
                            {
                                R_string(STDHead + PacketHead_Error + PacketBody_Error + STDFoot);
                                return STDHead + PacketHead_Error + PacketBody_Error + STDFoot;
                            }
                            break;

                        //撤销操作
                        case "TRANS007":
                            XmlNode xn_trans007 = xmlDoc.SelectSingleNode("Transaction/Transaction_Body");

                            if (xn_trans007 != null)
                            {
                                //取消登录判断
                                //string strSQL = "select * from skt17 where skf204=0 and skf203='" + phas.Terminal_eqno + "' ";
                                string strSQL = "select * from skt17 where skf204=1 and skf203='" + phas.Terminal_eqno + "' ";
                                DataSet DS;
                                DS = MySqlHelper.MySqlHelper.Query(strSQL, LinkString);
                                if (DS.Tables[0].Rows.Count == 1 && xn_trans007.SelectSingleNode("check_value").InnerText != null && xn_trans007.SelectSingleNode("check_value").InnerText != "")  //判断效验值,待补充
                                {
                                    //PacketTrans_ask.PacketTrans_ask ptas = new PacketTrans_ask.PacketTrans_ask();
                                    PacketWash_ask.PacketWash_ask pwas = new PacketWash_ask.PacketWash_ask();
                                    EnDeCode.EnDeCode edc = new EnDeCode.EnDeCode();
                                    string str_mysql = "";
                                    string strEnpKey = DS.Tables[0].Rows[0]["skf205"].ToString();
                                    string strDepKey = edc.DesDecrypt(strEnpKey, pKey);
                                    pwas.ReadXML(xn_trans007);
                                    string test_str_mysql = "select * from skt14 where skf160='02' and skf158='" + pwas.Order_no + "'";
                                    DataSet test_DS;
                                    test_DS = MySqlHelper.MySqlHelper.Query(test_str_mysql, LinkString);
                                    if (test_DS.Tables[0].Rows.Count == 0)
                                    {
                                        str_mysql = "insert into skt14(skf158,skf232,skf159,skf160,skf161,skf162,skf163,skf164,skf165,skf166,skf167,skf168,skf169,skf170,skf171,skf172,skf173,skf174,skf197,skf198) ";
                                        str_mysql = str_mysql + " value('" + pwas.Order_no + "','" + pwas.Order_no_washed + "','" + pwas.Pay_type + "','" + pwas.Trans_type + "'," + pwas.Info_type;
                                        str_mysql = str_mysql + "," + pwas.Net_type + ",'" + pwas.Mid + "','" + pwas.Tid + "','" + pwas.Cardacc_s + "'," + pwas.Amount;
                                        str_mysql = str_mysql + "," + pwas.Pay_amt + "," + pwas.Discount + ",'" + pwas.Pos_serial + "','" + pwas.Pos_setbat;
                                        str_mysql = str_mysql + "','" + pwas.Hostserial + "','" + pwas.Authcode + "','" + pwas.Transtime + "','" + pwas.Check_value + "','" + pwas.Cardnum + "','" + pwas.Cardpass + "')";

                                        int int_result = MySqlHelper.MySqlHelper.ExecuteSql(str_mysql, LinkString);

                                        if (int_result > 0)
                                        {
                                            string sql;
                                            //取消交易,不验证密码
                                            sql = "select * from skv1 where ((skvf7='" + pwas.Cardnum + "' and skvf10=1) or (skvf8='" + pwas.Cardnum + "' and skvf11=1) or (skvf20='" + pwas.Cardnum + "')) ";
                                            //if (ptas.Pay_type == "03")
                                            //{
                                            //    string strInSql = "select skfv12,skvf2 from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) ";
                                            //    string DePass = "";
                                            //    string strUserID = "";
                                            //    DataSet dsInSql = MySqlHelper.MySqlHelper.Query(strInSql, LinkString);
                                            //    if (dsInSql.Tables[0].Rows.Count > 0)
                                            //    {
                                            //        DePass = dsInSql.Tables[0].Rows[0][0].ToString();
                                            //        strUserID = dsInSql.Tables[0].Rows[0][1].ToString();
                                            //        string strEnSql = "update skt4 set skf228='" + edc.GetFCS(edc.GetMD5(edc.GetASCII(DePass))) + "' where skf36='" + strUserID + "' ";
                                            //        DataSet dsTemp = MySqlHelper.MySqlHelper.Query(strEnSql, LinkString);
                                            //    }
                                            //    sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) and skvf21='" + edc.DesDecrypt(ptas.Cardpass, strDepKey) + "' ";
                                            //}
                                            //else
                                            //{
                                            //    sql = "select * from skv1 where ((skvf7='" + ptas.Cardnum + "' and skvf10=1) or (skvf8='" + ptas.Cardnum + "' and skvf11=1) or (skvf20='" + ptas.Cardnum + "')) ";
                                            //}
                                            DataSet ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                            if (ds_sql.Tables[0].Rows.Count > 0)
                                            {
                                                string txtUserid = ds_sql.Tables[0].Rows[0]["skvf2"].ToString();

                                                sql = "select * from skt8 where skf104=1 and skf97='" + pwas.Tid + "' ";
                                                ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                if (ds_sql.Tables[0].Rows.Count > 0)
                                                {
                                                    float floNewJF = 0;
                                                    float floOldJF = 0;
                                                    float floOldCurJF = 0;
                                                    string strOldMoneyid = "0";
                                                    //string strNewMoneyid = "0";

                                                    sql = "select * from skt6 where skf91=1 and skf64='" + txtUserid + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (ds_sql.Tables[0].Rows.Count > 0)
                                                    {
                                                        floOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf66"].ToString());
                                                        floOldCurJF = float.Parse(ds_sql.Tables[0].Rows[0]["skt67"].ToString());
                                                        strOldMoneyid = ds_sql.Tables[0].Rows[0]["skf63"].ToString();
                                                    }
                                                    sql = "select * from skt14 where skf158='" + pwas.Order_no_washed + "' ";
                                                    DataSet dsData = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    sql = "select * from skt7 where skf200=1 and skf199='" + pwas.Order_no_washed + "' ";
                                                    ds_sql = MySqlHelper.MySqlHelper.Query(sql, LinkString);
                                                    if (dsData.Tables[0].Rows[0]["skf159"].ToString() == "03")
                                                    {
                                                        float floOriNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf79"].ToString());
                                                        float floOriOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf78"].ToString());
                                                        if (ds_sql.Tables[0].Rows.Count > 0)
                                                        {
                                                            //floNewJF = floOldJF - (floOriNewJF - floOriOldJF);
                                                            floNewJF = floOriNewJF - floOriOldJF;
                                                            sql = "update skt6 set skf66=skf66-" + floNewJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                            int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            if (intds_sql > 0)
                                                            {
                                                                phan.Gen_Answer_XML(true, "", "");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易成功");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);

                                                                sql = "update skt14 set skf201=1 where skf158='" + pwas.Order_no + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                sql = "update skt14 set skf201=3 where skf158='" + pwas.Order_no_washed + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                //插入历史
                                                                sql = "insert into skt7(skf73,skf74,skf75,skf78,skf79,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strOldMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + pwas.Order_no + "',0) ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            }
                                                            else
                                                            {
                                                                phan.Gen_Answer_XML(false, "交易失败,未知错误,联系管理员", "");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易失败,未知错误,联系管理员");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            phan.Gen_Answer_XML(false, "交易失败,积分不足", "");
                                                            //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                            PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                            string epwd = phan.Request_time + pwan.Pay_msg;
                                                            pwan.ReadXML(pwan, epwd, "交易失败,积分不足");
                                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        float floOriNewJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf81"].ToString());
                                                        float floOriOldJF = float.Parse(ds_sql.Tables[0].Rows[0]["skf80"].ToString());
                                                        if (ds_sql.Tables[0].Rows.Count > 0)
                                                        {
                                                            //floNewJF = floOldJF - (floOriNewJF - floOriOldJF);
                                                            floNewJF = floOriNewJF - floOriOldJF;
                                                            sql = "update skt6 set skf67=skf67-" + floNewJF + " where skf91=1 and skf64='" + txtUserid + "' ";
                                                            int intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            if (intds_sql > 0)
                                                            {
                                                                phan.Gen_Answer_XML(true, "", "");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易成功");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);

                                                                sql = "update skt14 set skf201=1 where skf158='" + pwas.Order_no + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                sql = "update skt14 set skf201=2 where skf158='" + pwas.Order_no_washed + "' ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);

                                                                //插入历史
                                                                sql = "insert into skt7(skf73,skf74,skf75,skf80,skf81,skf82,skf199,skf200) value('" + txtUserid + "','" + strOldMoneyid + "','" + strOldMoneyid + "','" + floOldJF + "','" + floNewJF + "','" + System.DateTime.Now.ToString() + "','" + pwas.Order_no + "',0) ";
                                                                intds_sql = MySqlHelper.MySqlHelper.ExecuteSql(sql, LinkString);
                                                            }
                                                            else
                                                            {
                                                                phan.Gen_Answer_XML(false, "交易失败,未知错误,联系管理员", "");
                                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                                pwan.ReadXML(pwan, epwd, "交易失败,未知错误,联系管理员");
                                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            phan.Gen_Answer_XML(false, "交易失败,积分不足", "");
                                                            //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                            PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                            string epwd = phan.Request_time + pwan.Pay_msg;
                                                            pwan.ReadXML(pwan, epwd, "交易失败,积分不足");
                                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    phan.Gen_Answer_XML(false, "交易失败,POS机未授权", "");
                                                    //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                    PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                    string epwd = phan.Request_time + pwan.Pay_msg;
                                                    pwan.ReadXML(pwan, epwd, "交易失败,POS机未授权");
                                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                                }
                                            }
                                            else
                                            {
                                                phan.Gen_Answer_XML(false, "交易失败,卡号密码错误", "");
                                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                                string epwd = phan.Request_time + pwan.Pay_msg;
                                                pwan.ReadXML(pwan, epwd, "交易失败,卡号密码错误");
                                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                            }
                                        }
                                        else
                                        {
                                            phan.Gen_Answer_XML(false, "交易失败,未知错误,联系管理员", "");
                                            //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                            PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                            string epwd = phan.Request_time + pwan.Pay_msg;
                                            pwan.ReadXML(pwan, epwd, "交易失败,未知错误,联系管理员");
                                            str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                        }
                                    }
                                    else
                                    {
                                        phan.Gen_Answer_XML(false, "交易失败,交易号重复", "");
                                        //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                        PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                        string epwd = phan.Request_time + pwan.Pay_msg;
                                        pwan.ReadXML(pwan, epwd, "交易失败,交易号重复");
                                        str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                    }
                                }
                                else
                                {
                                    phan.Gen_Answer_XML(false, "交易失败,效验错误或POS设备未登记", "");
                                    //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                    PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                    string epwd = phan.Request_time + pwan.Pay_msg;
                                    pwan.ReadXML(pwan, epwd, "交易失败,效验错误或POS设备未登记");
                                    str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                                }
                            }
                            else
                            {
                                phan.Gen_Answer_XML(false, "交易失败,数据包传输错误", "");
                                //PacketTrans_answer.PacketTrans_answer ptan = new PacketTrans_answer.PacketTrans_answer();
                                PacketWash_answer.PacketWash_answer pwan = new PacketWash_answer.PacketWash_answer();
                                string epwd = phan.Request_time + pwan.Pay_msg;
                                pwan.ReadXML(pwan, epwd, "交易失败,数据包传输错误");
                                str_result = XMLHelper.XMLHelper.Create_XML_Head("TRANS005", phan, pwan);
                            }
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
