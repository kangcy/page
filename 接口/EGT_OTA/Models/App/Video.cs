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
    /// 视频
    /// </summary>
    [Serializable]
    public class Video : BaseModel
    {
        /// <summary>
        /// ID
        /// </summary>
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [SubSonicNullString]
        public string Name { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [SubSonicNullString]
        public string Author { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        [SubSonicNullString]
        public string Cover { get; set; }

        /// <summary>
        /// 文件地址
        /// </summary>
        [SubSonicNullString]
        public string FileUrl { get; set; }
    }
}