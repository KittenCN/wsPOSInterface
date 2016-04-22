using System.Xml;

namespace PacketTrans_ask
{
    public class PacketTrans_ask
    {
        public PacketTrans_ask()
        { }

        //订单号
        private string order_no;
        public string Order_no
        {
            get { return order_no; }
            set { order_no = value; }
        }

        //支付方式
        private string pay_type;
        public string Pay_type
        {
            get { return pay_type; }
            set { pay_type = value; }
        }

        //交易类型
        private string trans_type;
        public string Trans_type
        {
            get { return trans_type; }
            set { trans_type = value; }
        }

        //通知类型
        private int info_type;
        public int Info_type
        {
            get { return info_type; }
            set { info_type = value; }
        }

        //通知方式
        private int net_type;
        public int Net_type
        {
            get { return net_type; }
            set { net_type = value; }
        }

        //商户号
        private string mid;
        public string Mid
        {
            get { return mid; }
            set { mid = value; }
        }

        //终端号
        private string tid;
        public string Tid
        {
            get { return tid; }
            set { tid = value; }
        }

        //交易卡号
        private string cardacc_s;
        public string Cardacc_s
        {
            get { return cardacc_s; }
            set { cardacc_s = value; }
        }

        //总支付金额
        private float amount;
        public float Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        //实际支付金额
        private float pay_amt;
        public float Pay_amt
        {
            get { return pay_amt; }
            set { pay_amt = value; }
        }

        //折扣信息
        private float discount;
        public float Discount
        {
            get { return discount; }
            set { discount = value; }
        }

        //支付凭证号
        private string pos_serial;
        public string Pos_serial
        {
            get { return pos_serial; }
            set { pos_serial = value; }
        }

        //交易批次号
        private string pos_setbat;
        public string Pos_setbat
        {
            get { return pos_setbat; }
            set { pos_setbat = value; }
        }

        //银联参考号
        private string hostserial;
        public string Hostserial
        {
            get { return hostserial; }
            set { hostserial = value; }
        }

        //银联授权号
        private string authcode;
        public string Authcode
        {
            get { return authcode; }
            set { authcode = value; }
        }

        //交易时间
        private string transtime;
        public string Transtime
        {
            get { return transtime; }
            set { transtime = value; }
        }

        //效验信息
        private string check_value;
        public string Check_value
        {
            get { return check_value; }
            set { check_value = value; }
        }

        //会员卡号
        private string cardnum;
        public string Cardnum
        {
            get { return cardnum; }
            set { cardnum = value; }
        }

        //会员支付密码
        private string cardpass;
        public string Cardpass
        {
            get { return cardpass; }
            set { cardpass = value; }
        }

        public void ReadXML(XmlNode xn)
        {
            Order_no = xn.SelectSingleNode("order_no").InnerText;
            Pay_type = xn.SelectSingleNode("pay_type").InnerText;
            Trans_type = xn.SelectSingleNode("trans_type").InnerText;
            if(xn.SelectSingleNode("info_type")==null)
            { Info_type =9999; }
            else
            { Info_type = int.Parse(xn.SelectSingleNode("info_type").InnerText); }
            if(xn.SelectSingleNode("net_type")==null)
            { Net_type = 9999; }
            else
            { Net_type = int.Parse(xn.SelectSingleNode("net_type").InnerText); }
            Mid = xn.SelectSingleNode("mid").InnerText;
            Tid = xn.SelectSingleNode("tid").InnerText;
            Cardacc_s = xn.SelectSingleNode("cardacc_s").InnerText;
            Amount = float.Parse(xn.SelectSingleNode("amount").InnerText);
            Pay_amt = float.Parse(xn.SelectSingleNode("pay_amt").InnerText);
            Discount = float.Parse(xn.SelectSingleNode("discount").InnerText);
            Pos_serial = xn.SelectSingleNode("pos_serial").InnerText;
            Pos_setbat = xn.SelectSingleNode("pos_setbat").InnerText;
            Hostserial = xn.SelectSingleNode("hostserial").InnerText;
            Authcode = xn.SelectSingleNode("authcode").InnerText;
            Transtime = xn.SelectSingleNode("transtime").InnerText;
            Check_value = xn.SelectSingleNode("check_value").InnerText;            
            if(xn.SelectSingleNode("cardnum")==null)
            {
                Cardnum = "000000000000";
            }
            else
            {
                Cardnum = xn.SelectSingleNode("cardnum").InnerText;
            }
            if(xn.SelectSingleNode("cardpass")==null)
            {
                Cardpass = "000000000";
            }
            else
            {
                Cardpass = xn.SelectSingleNode("cardpass").InnerText;
            }
        }
    }
}
