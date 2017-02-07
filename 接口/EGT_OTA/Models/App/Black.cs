﻿/************************************************************************************ 
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
    public class Black : BaseModelShort
    {
        /// <summary>
        /// 拉黑用户
        /// </summary>
        [SubSonicStringLength(30), SubSonicNullString]
        public string ToUserNumber { get; set; }
    }

    public class BlackJson
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Number { get; set; }
        public string NickName { get; set; }
        public string Avatar { get; set; }
        public string Signature { get; set; }
        public string CreateDate { get; set; }
    }
}