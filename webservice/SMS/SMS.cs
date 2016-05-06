using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;


namespace SMS
{
    /// <summary>
    /// SMS 的摘要说明
    /// </summary>
    public class SMS
    {
        string userid = "410";
        string account = "mhkj";
        string password = "mhkj_654317";

        public SMS()
        { }

        //短信发送内容请严格按下面格式发送：.
        //15902142966用户,您的验证码是:364499，请在3分钟内完成验证.【霓虹儿童广场】
        //15902142966用户,您刚刚消费获得:500个哈宝币.【霓虹儿童广场】
        public string send_reg_sms(int int_proindex, string mobile,float floHBB)
        {
            //string RanNum = Number(6, false);
            string content = mobile + "用户,您刚刚消费获得:" + floHBB + "个哈宝币.【霓虹儿童广场】";
            string RequestXML = "";
            string strRequest = "";

            switch (int_proindex)
            {
                case 1:
                    {
                        //SendSms(string userid, string account, string password, string mobile, string content, string sendTime, string extno)
                        //构造soap请求信息
                        StringBuilder soap = new StringBuilder();
                        soap.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                        soap.Append("<soap:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
                        soap.Append("<soap12:Body>");
                        soap.Append("<SendSms xmlns=\"http://61.129.70.241/\">");
                        soap.Append("<userid>" + userid + "</userid>");
                        soap.Append("<account>" + account + "</account>");
                        soap.Append("<password>" + password + "</password>");
                        soap.Append("<mobile>" + mobile + "</mobile>");
                        soap.Append("<content>" + content + "</content>");
                        soap.Append("<sendTime></sendTime>");
                        soap.Append("<extno></extno>");
                        soap.Append("</SendSms>");
                        soap.Append("</soap12:Body>");
                        soap.Append("</soap12:Envelope>");

                        //发起请求
                        Uri uri = new Uri("http://114.215.136.60:8888/SmsWebService.asmx?wsdl");
                        WebRequest webRequest = WebRequest.Create(uri);
                        webRequest.ContentType = "text/xml; charset=utf-8";
                        webRequest.Method = "POST";
                        using (Stream requestStream = webRequest.GetRequestStream())
                        {
                            byte[] paramBytes = Encoding.UTF8.GetBytes(soap.ToString());
                            requestStream.Write(paramBytes, 0, paramBytes.Length);
                        }

                        //响应
                        WebResponse webResponse = webRequest.GetResponse();
                        using (StreamReader myStreamReader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                        {
                            RequestXML = myStreamReader.ReadToEnd();
                            //strRequest = RanNum;
                        }
                        break;
                    }
                case 2:
                    {
                        string str_sendUrl = "http://114.215.136.60:8888/sms.aspx?action=send&userid=" + userid + "&account=" + account + "&password=" + password + "&mobile=" + mobile + "&content=" + content + "&sendTime=&extno=";
                        HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(str_sendUrl);
                        HttpWebResponse webreponse = (HttpWebResponse)webrequest.GetResponse();
                        Stream stream = webreponse.GetResponseStream();
                        byte[] rsByte = new Byte[webreponse.ContentLength];  //save data in the stream
                        try
                        {
                            stream.Read(rsByte, 0, (int)webreponse.ContentLength);
                            RequestXML = System.Text.Encoding.UTF8.GetString(rsByte, 0, rsByte.Length).ToString();
                        }
                        catch (Exception exp)
                        {
                            RequestXML = exp.ToString();
                        }
                        if (CheckXML(RequestXML))
                        {
                            returnsms RT = new returnsms();
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(RequestXML);
                            // 得到根节点
                            XmlNode xn = xmlDoc.SelectSingleNode("returnsms");
                            RT.ReadXML(xn);
                            if (RT.ReturnStatus == "Success" && RT.Message == "ok")
                            {
                                strRequest = "OK!";
                            }
                            else
                            {
                                strRequest = "Error:" + RT.ReturnStatus + "--" + RT.Message;
                            }
                        }
                        else
                        {
                            strRequest = "Error:Request Error!";
                        }
                        break;
                    }
            }

            return strRequest;
        }

        //生成数字随机数
        public static string Number(int length, bool Sleep)
        {
            if (Sleep)
            {
                System.Threading.Thread.Sleep(3);
            }
            string result = "";
            System.Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                result += random.Next(10).ToString();
            }
            return result;
        }

        public static Boolean CheckXML(string in_XML)
        {
            XmlDocument _XMLdoc = new XmlDocument();
            //in_XML ="<?xml version=\"1.0\" encoding=\"utf-8\"?><custinfo><custname>123</custname><telno>456</telno></custinfo>";
            //in_XML = in_XML.Trim();
            //_XMLdoc.LoadXml(in_XML);
            try
            {
                in_XML = in_XML.Trim();
                _XMLdoc.LoadXml(in_XML);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}