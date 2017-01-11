using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 订单
    /// </summary>
    public class Order
    {
        /// <summary>
        /// ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        [SubSonicStringLength(100), SubSonicNullString]
        public string OrderNumber { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 支付方式（1：支付宝、2：微信）
        /// </summary>
        public int PayType { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}