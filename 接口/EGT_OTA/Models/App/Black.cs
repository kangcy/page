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
    /// 黑名单
    /// </summary>
    [Serializable]
    public class Black
    {
        /// <summary>
        /// ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 拉黑用户
        /// </summary>
        public int ToUserID { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public int CreateUserID { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 创建IP
        /// </summary>
        [SubSonicStringLength(100), SubSonicNullString]
        public string CreateIP { get; set; }
    }

    public class BlackJson
    {
        public int ID { get; set; }
        public string NickName { get; set; }
        public string Avatar { get; set; }
        public string Signature { get; set; }
        public string CreateDate { get; set; }
    }
}