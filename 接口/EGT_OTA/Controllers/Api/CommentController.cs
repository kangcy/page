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
    public class CommentController : BaseApiController
    {
        /// <summary>
        /// 评论列表
        /// </summary>
        [DeflateCompression]
        [HttpGet]
        [Route("Api/Comment/All")]
        public string All()
        {
            ApiResult result = new ApiResult();
            try
            {
                var pager = new Pager();
                var id = ZNRequest.GetInt("NewId");
                var ArticleUserNumber = ZNRequest.GetString("ArticleUserNumber");//文章作者
                if (string.IsNullOrWhiteSpace(ArticleUserNumber))
                {
                    result.message = "参数异常";
                    return JsonConvert.SerializeObject(result);
                }
                var query = new SubSonic.Query.Select(provider).From<Comment>().Where<Comment>(x => x.ArticleUserNumber == ArticleUserNumber);
                var recordCount = query.GetRecordCount();
                if (recordCount == 0)
                {
                    result.result = true;
                    result.message = new { records = recordCount, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }

                if (recordCount == 1 && id > 0)
                {
                    result.result = true;
                    result.message = new { records = recordCount, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }
                query = query.And("ID").IsNotEqualTo(id);

                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Comment>();
                var articles = new SubSonic.Query.Select(provider, "ID", "Title", "ArticlePower", "Number", "CreateUserNumber").From<Article>().Where("Number").In(list.Select(x => x.ArticleNumber).ToArray()).ExecuteTypedList<Article>();
                var users = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(list.Select(x => x.CreateUserNumber).ToArray()).ExecuteTypedList<User>();

                var newlist = (from l in list
                               join a in articles on l.ArticleNumber equals a.Number
                               join u in users on l.CreateUserNumber equals u.Number
                               select new CommentJson
                               {
                                   ID = l.ID,
                                   Number = l.Number,
                                   Summary = l.Summary,
                                   Goods = l.Goods,
                                   CreateDateText = FormatTime(l.CreateDate),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Avatar = u.Avatar,
                                   UserNumber = u.Number,
                                   ArticleID = a.ID,
                                   Title = a.Title,
                                   ArticleUserNumber = a.CreateUserNumber,
                                   ArticlePower = a.ArticlePower,
                                   ParentCommentNumber = l.ParentCommentNumber,
                                   ParentUserNumber = l.ParentUserNumber,
                                   ParentNickName = "",
                                   ParentSummary = ""
                               }).ToList();

                result.result = true;
                result.message = new
                    {
                        currpage = pager.Index,
                        records = recordCount,
                        totalpage = totalPage,
                        list = FormatCommentInfo(list, newlist, ArticleUserNumber)
                    };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Comment_All:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }


        /// <summary>
        /// 文章评论
        /// </summary>
        [DeflateCompression]
        [HttpGet]
        [Route("Api/Comment/ArticleComment")]
        public string ArticleComment()
        {
            ApiResult result = new ApiResult();
            try
            {
                var pager = new Pager();
                var id = ZNRequest.GetInt("NewId");
                var ArticleNumber = ZNRequest.GetString("ArticleNumber");
                if (string.IsNullOrWhiteSpace(ArticleNumber))
                {
                    result.message = "参数异常";
                    return JsonConvert.SerializeObject(result);
                }
                var UserNumber = ZNRequest.GetString("UserNumber");

                var query = new SubSonic.Query.Select(provider).From<Comment>().Where<Comment>(x => x.ArticleNumber == ArticleNumber);
                var recordCount = query.GetRecordCount();
                if (recordCount == 0)
                {
                    result.result = true;
                    result.message = new { records = recordCount, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }
                if (recordCount == 1 && id > 0)
                {
                    result.result = true;
                    result.message = new { records = recordCount, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }
                query = query.And("ID").IsNotEqualTo(id);

                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;

                var isNew = ZNRequest.GetInt("New");
                var list = new List<Comment>();
                if (isNew > 0)
                {
                    list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Comment>();
                }
                else
                {
                    list = query.Paged(pager.Index, pager.Size).OrderAsc("ID").ExecuteTypedList<Comment>();
                }
                var users = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(list.Select(x => x.CreateUserNumber).Distinct().ToArray()).ExecuteTypedList<User>();

                var newlist = (from l in list
                               join u in users on l.CreateUserNumber equals u.Number
                               select new CommentJson
                               {
                                   ID = l.ID,
                                   Summary = l.Summary,
                                   Goods = l.Goods,
                                   Number = l.Number,
                                   CreateDateText = isNew > 0 ? FormatTime(l.CreateDate) : l.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   UserNumber = u.Number,
                                   NickName = u.NickName,
                                   Avatar = u.Avatar,
                                   ParentCommentNumber = l.ParentCommentNumber,
                                   ParentUserNumber = l.ParentUserNumber,
                                   ParentNickName = "",
                                   ParentSummary = ""
                               }).ToList();

                result.result = true;
                result.message = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = FormatCommentInfo(list, newlist, UserNumber)
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_ArticleComment_All:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 获取父评论信息,格式化数据
        /// </summary>
        protected List<CommentJson> FormatCommentInfo(List<Comment> list, List<CommentJson> newlist, string UserNumber)
        {
            var ParentCommentNumber = new List<string>();
            var ParentUserNumber = new List<string>();
            list.ForEach(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.ParentCommentNumber))
                {
                    ParentCommentNumber.Add(x.ParentCommentNumber);
                }
                if (!string.IsNullOrWhiteSpace(x.ParentUserNumber))
                {
                    ParentUserNumber.Add(x.ParentUserNumber);
                }
            });
            var parentComment = new List<Comment>();
            var parentUser = new List<User>();
            if (ParentCommentNumber.Count > 0)
            {
                parentComment = new SubSonic.Query.Select(provider, "ID", "Summary", "Number").From<Comment>().Where("Number").In(ParentCommentNumber.Distinct().ToArray()).ExecuteTypedList<Comment>();
            }
            if (ParentUserNumber.Count > 0)
            {
                parentUser = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(ParentUserNumber.Distinct().ToArray()).ExecuteTypedList<User>();
            }

            //判断是否点赞
            var zans = new List<Zan>();
            if (!string.IsNullOrWhiteSpace(UserNumber))
            {
                zans = db.Find<Zan>(x => x.CreateUserNumber == UserNumber && x.ZanType == Enum_ZanType.Comment).ToList();
            }

            newlist.ForEach(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.ParentCommentNumber))
                {
                    var comment = parentComment.FirstOrDefault(y => y.Number == x.ParentCommentNumber);
                    if (comment != null)
                    {
                        x.ParentSummary = comment.Summary;
                    }
                }
                if (!string.IsNullOrWhiteSpace(x.ParentUserNumber))
                {
                    var user = parentUser.FirstOrDefault(y => y.Number == x.ParentUserNumber);
                    if (user != null)
                    {
                        x.ParentNickName = user.NickName;
                    }
                }
                x.IsZan = zans.Count(y => y.CommentNumber == x.Number);
            });

            return newlist;
        }
    }
}