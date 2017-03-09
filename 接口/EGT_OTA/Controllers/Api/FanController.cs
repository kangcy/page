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
    public class FanController : BaseApiController
    {
        /// <summary>
        /// 关注列表
        /// </summary>
        [DeflateCompression]
        [HttpGet]
        [Route("Api/Fan/FollowsAll")]
        public string FollowsAll()
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
                var query = new SubSonic.Query.Select(provider).From<Fan>().Where<Fan>(x => x.CreateUserNumber == CreateUserNumber);
                var recordCount = query.GetRecordCount();
                if (recordCount == 0)
                {
                    result.message = new { records = recordCount, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
                var array = list.Select(x => x.ToUserNumber).Distinct().ToList();
                var users = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Signature", "Number").From<User>().Where<User>(x => x.Status == Enum_Status.Approved).And("Number").In(array.ToArray()).ExecuteTypedList<User>();

                var newlist = (from l in list
                               join u in users on l.ToUserNumber equals u.Number
                               select new
                               {
                                   ID = l.ID,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Signature = u.Signature,
                                   Avatar = u.Avatar,
                                   Number = u.Number
                               }).ToList();
                result.result = true;
                result.message = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Fan_FollowsAll:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 粉丝列表
        /// </summary>
        [DeflateCompression]
        [HttpGet]
        [Route("Api/Fan/FansAll")]
        public string FansAll()
        {
            ApiResult result = new ApiResult();
            try
            {
                var ToUserNumber = ZNRequest.GetString("ToUserNumber");
                if (string.IsNullOrWhiteSpace(ToUserNumber))
                {
                    result.message = "参数异常";
                    return JsonConvert.SerializeObject(result);
                }
                var pager = new Pager();
                var query = new SubSonic.Query.Select(provider).From<Fan>().Where<Fan>(x => x.ToUserNumber == ToUserNumber);
                var recordCount = query.GetRecordCount();
                if (recordCount == 0)
                {
                    result.message = new { records = recordCount, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
                var array = list.Select(x => x.CreateUserNumber).Distinct().ToList();
                var users = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Signature", "Number").From<User>().Where<User>(x => x.Status == Enum_Status.Approved).And("Number").In(array.ToArray()).ExecuteTypedList<User>();
                var follows = db.Find<Fan>(x => x.CreateUserNumber == ToUserNumber).ToList();

                var newlist = (from l in list
                               join u in users on l.CreateUserNumber equals u.Number
                               select new
                               {
                                   ID = l.ID,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Signature = u.Signature,
                                   Avatar = u.Avatar,
                                   Number = u.Number,
                                   IsFollow = follows.Exists(x => x.ToUserNumber == u.Number) ? 1 : 0
                               }).ToList();
                result.result = true;
                result.message = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Fan_FansAll:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}