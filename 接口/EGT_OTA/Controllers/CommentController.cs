using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 评论
    /// </summary>
    public class CommentController : BaseController
    {
        /// <summary>
        /// 评论点赞
        /// </summary>
        public ActionResult Zan()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var commentID = ZNRequest.GetInt("CommentID");
                if (commentID <= 0)
                {
                    return Json(new { result = false, message = "评论信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Comment comment = db.Single<Comment>(x => x.ID == commentID);
                if (comment == null)
                {
                    return Json(new { result = false, message = "评论信息异常" }, JsonRequestBehavior.AllowGet);
                }

                Zan model = db.Single<Zan>(x => x.CreateUserNumber == user.Number && x.CommentNumber == comment.Number);
                if (model == null)
                {
                    model = new Zan();
                    model.CreateDate = DateTime.Now;
                    model.CreateUserNumber = user.Number;
                    model.CreateIP = Tools.GetClientIP;
                }
                else
                {
                    return Json(new { result = false, message = "已赞" }, JsonRequestBehavior.AllowGet);
                }
                model.ArticleNumber = string.Empty;
                model.CommentNumber = comment.Number;
                model.ArticleUserNumber = comment.ArticleUserNumber;
                model.ZanType = Enum_ZanType.Comment;
                var result = Tools.SafeInt(db.Add<Zan>(model)) > 0;
                if (result)
                {
                    comment.Goods += 1;
                    result = db.Update<Comment>(comment) > 0;
                }
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("CommentController_Zan:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 评论编辑
        /// </summary>
        public ActionResult Edit()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var ArticleID = ZNRequest.GetInt("ArticleID");
                if (ArticleID <= 0)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                var summary = SqlFilter(ZNRequest.GetString("Summary"), false);
                if (string.IsNullOrWhiteSpace(summary))
                {
                    return Json(new { result = false, message = "请填写评论内容" }, JsonRequestBehavior.AllowGet);
                }
                summary = CutString(summary, 2000);
                if (HasDirtyWord(summary))
                {
                    return Json(new { result = false, message = "您的输入内容含有敏感内容，请检查后重试哦" }, JsonRequestBehavior.AllowGet);
                }
                Article article = new SubSonic.Query.Select(Repository.GetProvider(), "Number", "CreateUserNumber").From<Article>().Where<Article>(x => x.ID == ArticleID).ExecuteSingle<Article>();
                if (article == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Comment model = new Comment();
                model.ArticleNumber = article.Number;
                model.ArticleUserNumber = article.CreateUserNumber;
                model.Summary = summary;
                model.Number = BuildNumber();
                model.Province = AntiXssChineseString.ChineseStringSanitize(ZNRequest.GetString("Province"));
                model.City = AntiXssChineseString.ChineseStringSanitize(ZNRequest.GetString("City"));
                model.CreateDate = DateTime.Now;
                model.CreateUserNumber = user.Number;
                model.CreateIP = Tools.GetClientIP;
                model.ParentCommentNumber = ZNRequest.GetString("ParentCommentNumber");
                model.ParentUserNumber = ZNRequest.GetString("ParentUserNumber");
                var result = Tools.SafeInt(db.Add<Comment>(model)) > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("CommentController_Edit:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 文章评论
        /// </summary>
        public ActionResult ArticleComment()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where<Comment>(x => x.ID > 0);

                //文章
                var ArticleNumber = ZNRequest.GetString("ArticleNumber");
                if (string.IsNullOrWhiteSpace(ArticleNumber))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    query = query.And("ArticleNumber").IsEqualTo(ArticleNumber);
                }
                var recordCount = query.GetRecordCount();

                if (recordCount == 0)
                {
                    return Json(new
                    {
                        currpage = pager.Index,
                        records = recordCount,
                        totalpage = 1,
                        list = string.Empty
                    }, JsonRequestBehavior.AllowGet);
                }

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
                //所有用户
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(list.Select(x => x.CreateUserNumber).Distinct().ToArray()).ExecuteTypedList<User>();

                //父评论
                var parentComment = new List<Comment>();
                var parentUser = new List<User>();
                if (list.Exists(x => !string.IsNullOrWhiteSpace(x.ParentUserNumber)))
                {
                    parentComment = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Summary", "Number").From<Comment>().Where("Number").In(list.Select(x => x.ParentCommentNumber).Distinct().ToArray()).ExecuteTypedList<Comment>();
                    parentUser = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(list.Select(x => x.ParentUserNumber).Distinct().ToArray()).ExecuteTypedList<User>();
                }
                var newlist = (from l in list
                               join u in users on l.CreateUserNumber equals u.Number
                               select new
                               {
                                   ID = l.ID,
                                   Summary = l.Summary,
                                   City = l.City,
                                   Goods = l.Goods,
                                   Number = l.Number,
                                   CreateDate = isNew > 0 ? FormatTime(l.CreateDate) : l.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   UserNumber = u.Number,
                                   NickName = u.NickName,
                                   Avatar = u.Avatar,
                                   ParentCommentID = l.ParentCommentNumber,
                                   ParentUserNumber = l.ParentUserNumber,
                                   ParentNickName = string.IsNullOrWhiteSpace(l.ParentUserNumber) ? "" : (parentUser.Exists(x => x.Number == l.ParentUserNumber) ? parentUser.FirstOrDefault(x => x.Number == l.ParentUserNumber).NickName : ""),
                                   ParentSummary = string.IsNullOrWhiteSpace(l.ParentCommentNumber) ? "" : (parentComment.Exists(x => x.Number == l.ParentCommentNumber) ? parentComment.FirstOrDefault(x => x.Number == l.ParentCommentNumber).Summary : "")
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("CommentController_ArticleComment:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where<Comment>(x => x.ID > 0);

                //评论人
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                if (!string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("CreateUserNumber").IsEqualTo(CreateUserNumber);
                }

                //文章作者
                var ArticleUserNumber = ZNRequest.GetString("ArticleUserNumber");
                if (!string.IsNullOrWhiteSpace(ArticleUserNumber))
                {
                    query = query.And("ArticleUserNumber").IsEqualTo(ArticleUserNumber);
                }

                var recordCount = query.GetRecordCount();

                if (recordCount == 0)
                {
                    return Json(new
                    {
                        currpage = pager.Index,
                        records = recordCount,
                        totalpage = 1,
                        list = string.Empty
                    }, JsonRequestBehavior.AllowGet);
                }

                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Comment>();
                var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "ArticlePower", "Number", "CreateUserNumber").From<Article>().Where("Number").In(list.Select(x => x.ArticleNumber).ToArray()).ExecuteTypedList<Article>();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(list.Select(x => x.CreateUserNumber).ToArray()).ExecuteTypedList<User>();

                //父评论
                var parentComment = new List<Comment>();
                var parentUser = new List<User>();
                if (list.Exists(x => !string.IsNullOrWhiteSpace(x.ParentUserNumber)))
                {
                    parentComment = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Summary", "Number").From<Comment>().Where("Number").In(list.Select(x => x.ParentCommentNumber).Distinct().ToArray()).ExecuteTypedList<Comment>();
                    parentUser = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(list.Select(x => x.ParentUserNumber).Distinct().ToArray()).ExecuteTypedList<User>();
                }

                var newlist = (from l in list
                               join a in articles on l.ArticleNumber equals a.Number
                               join u in users on l.CreateUserNumber equals u.Number
                               select new
                               {
                                   ID = l.ID,
                                   Summary = l.Summary,
                                   City = l.City,
                                   Goods = l.Goods,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Avatar = u.Avatar,
                                   UserNumber = u.Number,
                                   ArticleID = a.ID,
                                   Title = a.Title,
                                   ArticleUserID = a.CreateUserNumber,
                                   ArticlePower = a.ArticlePower,
                                   ParentCommentID = l.ParentCommentNumber,
                                   ParentUserNumber = l.ParentUserNumber,
                                   ParentNickName = string.IsNullOrWhiteSpace(l.ParentUserNumber) ? "" : (parentUser.Exists(x => x.Number == l.ParentUserNumber) ? parentUser.FirstOrDefault(x => x.Number == l.ParentUserNumber).NickName : ""),
                                   ParentSummary = string.IsNullOrWhiteSpace(l.ParentCommentNumber) ? "" : (parentComment.Exists(x => x.Number == l.ParentCommentNumber) ? parentComment.FirstOrDefault(x => x.Number == l.ParentCommentNumber).Summary : "")
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("CommentController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult Top()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where<Comment>(x => x.ID > 0);

                //创建人
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                if (!string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("CreateUserNumber").IsEqualTo(CreateUserNumber);
                }

                //父评论人
                var ParentUserNumber = ZNRequest.GetString("ParentUserNumber");
                if (!string.IsNullOrWhiteSpace(ParentUserNumber))
                {
                    query = query.And("ParentUserNumber").IsEqualTo(ParentUserNumber);
                }

                var IsReply = ZNRequest.GetInt("IsReply", 0);
                if (IsReply == 0)
                {
                    query = query.And("ParentCommentNumber").IsNull();
                }
                else if (IsReply == 1)
                {
                    query = query.And("ParentCommentNumber").IsNotNull();
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Comment>();
                var comments = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where("Number").In(list.Select(x => x.ParentCommentNumber).Distinct().ToArray()).ExecuteTypedList<Comment>();

                var today = DateTime.Now.ToString("yyyyMMdd");
                var yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   Summary = l.Summary,
                                   City = l.City,
                                   Goods = l.Goods,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   Month = l.CreateDate.ToString("yyyyMMdd") == today ? "今天" : l.CreateDate.ToString("yyyyMMdd") == yesterday ? "昨天" : l.CreateDate.Month.ToString(),
                                   Day = l.CreateDate.ToString("yyyyMMdd") == today ? "今天" : l.CreateDate.ToString("yyyyMMdd") == yesterday ? "昨天" : l.CreateDate.Day.ToString(),
                                   ArticleID = l.ArticleNumber,
                                   ParentSummary = comments.Exists(x => x.Number == l.ParentCommentNumber) ? comments.FirstOrDefault(x => x.Number == l.ParentCommentNumber).Summary : ""
                               }).ToList();
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("CommentController_Top:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
