/************************************************************************************ 
 * Copyright (c) 2016 安讯科技（南京）有限公司 版权所有 All Rights Reserved.
 * 文件名：  EGT_OTA.Models.App.Article 
 * 版本号：  V1.0.0.0 
 * 创建人： 康春阳
 * 电子邮箱：kangcy@axon.com.cn 
 * 创建时间：2016/7/29 15:08:56 
 * 描述    :
 * =====================================================================
 * 修改时间：2016/7/29 15:08:56 
 * 修改人  ：  
 * 版本号  ：V1.0.0.0 
 * 描述    ：
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommonTools;
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 文章部分
    /// </summary>
    [Serializable]
    public class ArticlePart
    {
        public ArticlePart() { }

        public ArticlePart(string number, int type, int sortId, string introduction)
        {
            this.ArticleNumber = number;
            this.Introduction = introduction;
            this.Types = type;
            this.SortID = sortId;
            this.CreateDate = DateTime.Now;
            this.CreateIP = Tools.GetClientIP;
            this.Status = Enum_Status.Approved;
        }

        /// <summary>
        /// ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 文章编号
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ArticleNumber { get; set; }

        /// <summary>
        /// 类型（1：图片,2：文字,3：视频）
        /// </summary>
        public int Types { get; set; }

        /// <summary>
        /// 详细
        /// </summary>
        [SubSonicStringLength(5000), SubSonicNullString]
        public string Introduction { get; set; }

        public int SortID { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string CreateUserNumber { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        [SubSonicNullString]
        public string CreateIP { get; set; }

        #region 文本修饰

        /// <summary>
        /// 颜色
        /// </summary>
        [SubSonicNullString]
        public string Color { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        [SubSonicNullString]
        public string Align { get; set; }

        /// <summary>
        /// 字体大小
        /// </summary>
        [SubSonicNullString]
        public string FontSize { get; set; }

        /// <summary>
        /// 字体加粗
        /// </summary>
        [SubSonicNullString]
        public string FontWeight { get; set; }

        /// <summary>
        /// 超链接
        /// </summary>
        [SubSonicNullString]
        public string Link { get; set; }

        #endregion
    }
}