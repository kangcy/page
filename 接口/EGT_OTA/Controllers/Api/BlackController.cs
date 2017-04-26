using System;
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
    public class BlackController : BaseApiController
    {
        /// <summary>
        /// 黑名单列表
        /// </summary>
        [HttpGet]
        [Route("Api/Black/All")]
        public string All()
        {
            ApiResult result = new ApiResult();
            try
            {
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                if (string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    result.message = "参数异常";
                    return JsonConvert.SerializeObject(result);
                }
                var pager = new Pager();
                var query = new SubSonic.Query.Select(provider).From<Black>().Where<Black>(x => x.CreateUserNumber == CreateUserNumber);
                var recordCount = query.GetRecordCount();
                if (recordCount == 0)
                {
                    result.message = string.Empty;
                    return JsonConvert.SerializeObject(result);
                }
                var list = query.OrderDesc("ID").ExecuteTypedList<Black>();
                var array = list.Select(x => x.ToUserNumber).ToArray();
                var users = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Signature", "Number").From<User>().Where("Number").In(array).ExecuteTypedList<User>();

                var newlist = (from b in list
                               join u in users on b.ToUserNumber equals u.Number
                               select new BlackJson
                               {
                                   ID = b.ID,
                                   CreateDate = b.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   Number = u.Number,
                                   NickName = u.NickName,
                                   Avatar = u.Avatar,
                                   Signature = u.Signature,
                               }).ToList();
                result.result = true;
                result.message = new
                {
                    currpage = 1,
                    records = newlist.Count,
                    totalpage = 1,
                    list = newlist
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Black_All:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}