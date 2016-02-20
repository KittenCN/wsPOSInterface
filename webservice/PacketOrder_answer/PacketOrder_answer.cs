using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketOrder_answer
{
    public class PacketOrder_answer
    {
        public PacketOrder_answer()
        { }

        //返回的明细金额个数
        private Int32 amt_count;
        public Int32 Amt_count
        {
            get { return amt_count; }
            set { amt_count = value; }
        }

        //返回金额的明细列表
        private string amt_list;
        public string Amt_list
        {
            get { return amt_list; }
            set { amt_list = value; }
        }

        //合并后的订单号
        private string order_union;
        public string Order_union
        {
            get { return order_union; }
            set { order_union = value; }
        }

        //折扣信息
        private float discount;
        public float Discount
        {
            get { return discount; }
            set { discount = value; }
        }

        //订单总金额
        private float total_amt;
        public float Total_amt
        {
            get { return total_amt; }
            set { total_amt = value; }
        }

        //订单备注信息
        private string pay_msg;
        public string Pay_msg
        {
            get { return pay_msg; }
            set { pay_msg = value; }
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
