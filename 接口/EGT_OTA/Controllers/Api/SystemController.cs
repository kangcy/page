﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using CommonTools;
using EGT_OTA.Controllers.Filter;
using EGT_OTA.Helper;
using EGT_OTA.Models;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers.Api
{
    public class SystemController : BaseApiController
    {
        /// <summary>
        /// 判断敏感词
        /// </summary>
        [HttpGet]
        [Route("Api/System/CheckDirtyword")]
        public string CheckDirtyword()
        {
            ApiResult result = new ApiResult();
            try
            {
                var title = ZNRequest.GetString("Title");
                if (HasDirtyWord(title))
                {
                    result.message = "您输入的内容含有敏感内容，请检查后重试哦";
                    return JsonConvert.SerializeObject(result);
                }
                result.result = true;
                result.message = "";
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Article_Detail:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(GetDirtyWord());
        }
    }
}