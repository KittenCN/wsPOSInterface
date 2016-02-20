using System;

namespace PacketOrder_ask
{
    public class PacketOrder_ask
    {
        public PacketOrder_ask()
        { }

        //交易类型
        private string trans_type;
        public string Trans_type
        {
            get { return trans_type; }
            set { trans_type = value; }
        }

        //本次上送的合并订单个数
        private Int32 order_count;
        public Int32 Order_count
        {
            get { return order_count; }
            set { order_count = value; }
        }

        //订单列表
        private string order_set;
        public string Order_set
        {
            get { return order_set; }
            set { order_set = value; }
        }

        //效验信息
        private string check_value;
        public string Check_value
        {
            get { return check_value; }
            set { check_value = value; }
        }

    }
}
