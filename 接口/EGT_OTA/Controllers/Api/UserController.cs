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
    public class UserController : BaseApiController
    {
        /// <summary>
        /// 点赞我
        /// </summary>
        [DeflateCompression]
        [HttpGet]
        [Route("Api/User/Pic")]
        public string ToMe()
        {
            ApiResult result = new ApiResult();
            try
            {
                var Number = ZNRequest.GetString("Number");
                var UserNumber = ZNRequest.GetString("UserNumber");
                var pager = new Pager();
                var query = new SubSonic.Query.Select(provider).From<ArticlePart>().Where<ArticlePart>(x => x.Types == Enum_ArticlePart.Pic && x.Status != Enum_Status.DELETE);
                if (Number != UserNumber)
                {
                    query = query.And("Status").IsEqualTo(Enum_Status.Approved);
                }
                if (string.IsNullOrWhiteSpace(UserNumber))
                {
                    result.message = new { records = 0, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }

                query = query.And("CreateUserNumber").IsEqualTo(UserNumber);

                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<ArticlePart>();

                result.result = true;
                result.message = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = list
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_User_Pic:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}