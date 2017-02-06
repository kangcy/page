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
using SubSonic.SqlGeneration.Schema;

namespace EGT_OTA.Models
{
    /// <summary>
    /// 文章评论
    /// </summary>
    [Serializable]
    public class Comment
    {
        /// <summary>
        /// ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 文章ID
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ArticleNumber { get; set; }

        /// <summary>
        /// 文章作者
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ArticleUserNumber { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        [SubSonicStringLength(5000), SubSonicNullString]
        public string Summary { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string Number { get; set; }

        /// <summary>
        /// 评论省份
        /// </summary>
        [SubSonicNullString]
        public string Province { get; set; }

        /// <summary>
        /// 评论城市
        /// </summary>
        [SubSonicNullString]
        public string City { get; set; }

        /// <summary>
        /// 回复评论ID
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ParentCommentNumber { get; set; }

        /// <summary>
        /// 回复用戶ID
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ParentUserNumber { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>
        public int Goods { get; set; }

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
        /// 创建IP
        /// </summary>
        [SubSonicNullString]
        public string CreateIP { get; set; }

        #region 扩展

        /// <summary>
        /// 创建月
        /// </summary>
        [SubSonicIgnore]
        public string Month { get; set; }

        /// <summary>
        /// 创建日
        /// </summary>
        [SubSonicIgnore]
        public string Day { get; set; }

        #endregion
    }

    public class CommentJson
    {
        public int ID { get; set; }
        public string Summary { get; set; }
        public int UserID { get; set; }
        public string UserNumber { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
    }
}