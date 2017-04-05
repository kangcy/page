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
    public class UserLogController : BaseApiController
    {
        /// <summary>
        /// 编辑
        /// </summary>
        [HttpGet]
        [Route("Api/UserLog/Edit")]
        public string Edit()
        {
            ApiResult result = new ApiResult();
            try
            {
                var info = ZNRequest.GetString("info");
                if (!string.IsNullOrWhiteSpace(info))
                {
                    UserLog log = new UserLog();
                    log.Info = info;
                    db.Add<UserLog>(log);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_UserLog_Edit" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 点赞我
        /// </summary>
        [DeflateCompression]
        [HttpGet]
        [Route("Api/Zan/ToMe")]
        public string ToMe()
        {
            ApiResult result = new ApiResult();
            try
            {
                var UserNumber = ZNRequest.GetString("UserNumber");
                if (string.IsNullOrWhiteSpace(UserNumber))
                {
                    result.message = "参数异常";
                    return JsonConvert.SerializeObject(result);
                }
                var pager = new Pager();
                var query = new SubSonic.Query.Select(provider).From<Zan>().Where<Zan>(x => x.ArticleUserNumber == UserNumber && x.ZanType == Enum_ZanType.Article);
                var recordCount = query.GetRecordCount();
                if (recordCount == 0)
                {
                    result.message = new { records = recordCount, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }

                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Zan>();

                var articles = new SubSonic.Query.Select(provider, "ID", "Number", "Cover", "ArticlePower", "CreateUserNumber", "Status", "Title").From<Article>().Where("Number").In(list.Select(x => x.ArticleNumber).ToArray()).And("CreateUserNumber").IsEqualTo(UserNumber).ExecuteTypedList<Article>();
                var users = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(list.Select(x => x.CreateUserNumber).ToArray()).ExecuteTypedList<User>();

                List<ZanJson> newlist = new List<ZanJson>();
                list.ForEach(x =>
                {
                    var article = articles.FirstOrDefault(y => y.Number == x.ArticleNumber);
                    var user = users.FirstOrDefault(y => y.Number == x.CreateUserNumber);
                    if (article != null && user != null)
                    {
                        ZanJson model = new ZanJson();
                        model.ID = x.ID;
                        model.CreateDate = x.CreateDate.ToString("yyyy-MM-dd");
                        model.ArticleID = article.ID;
                        model.Title = article.Title;
                        model.Number = article.Number;
                        model.Cover = article.Cover;
                        model.ArticlePower = article.ArticlePower;
                        model.CreateUserNumber = article.CreateUserNumber;
                        model.NickName = user.NickName;
                        model.Avatar = user.Avatar;
                        model.UserNumber = user.Number;
                        newlist.Add(model);
                    }
                });
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
                LogHelper.ErrorLoger.Error("Api_Zan_All:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}