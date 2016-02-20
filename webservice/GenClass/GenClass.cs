using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace GenClass
{
    public class GenClass
    {
        //将DataSet转换为xml字符串
        public static string ConvertDataSetToXMLFile(DataSet xmlDS, Encoding encoding)
        {
            MemoryStream stream = null;
            XmlTextWriter writer = null;
            string result = "<result>-3</result>";
            try
            {
                stream = new MemoryStream();
                //从stream装载到XmlTextReader
                writer = new XmlTextWriter(stream, encoding);
                //用WriteXml方法写入文件.
                xmlDS.WriteXml(writer);
                int count = (int)stream.Length;
                byte[] arr = new byte[count];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(arr, 0, count);
                result = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + encoding.GetString(arr).Trim();
            }
            catch { }
            finally
            {
                if (writer != null) writer.Close();
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

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpper();
        }

        public static XmlElement GetXmlElement(XmlDocument doc, string elementName, string value)
        {
            XmlElement element = doc.CreateElement(elementName);
            element.InnerText = value;
            return element;
        }
    }
}
