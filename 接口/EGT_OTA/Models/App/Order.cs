﻿using System;
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
        [SubSonicStringLength(30), SubSonicNullString]
        public string FromUserNumber { get; set; }

        /// <summary>
        /// 打赏对象
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ToUserNumber { get; set; }

        /// <summary>
        /// 打赏文章
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ToArticleNumber { get; set; }

        /// <summary>
        /// 支付金额（单位：分）
        /// </summary>
        public int Price { get; set; }

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

        /// <summary>
        /// 描述
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 是否匿名
        /// </summary>
        public int Anony { get; set; }
    }

    public class OrderJson
    {
        public int ID { get; set; }
        public int FromUserID { get; set; }
        public string FromUserNumber { get; set; }
        public string FromUserAvatar { get; set; }
        public string FromUserName { get; set; }
        public int ToUserID { get; set; }
        public string ToUserNumber { get; set; }
        public string ToUserAvatar { get; set; }
        public string ToUserName { get; set; }
        public string CreateDate { get; set; }
        public int Price { get; set; }
    }
}