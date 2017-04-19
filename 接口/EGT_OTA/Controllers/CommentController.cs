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
                //判断是否拉黑
                var black = db.Exists<Black>(x => x.CreateUserNumber == comment.ArticleUserNumber && x.ToUserNumber == user.Number);
                if (black)
                {
                    return Json(new { result = false, message = "没有权限" }, JsonRequestBehavior.AllowGet);
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
                var summary = SqlFilter(ZNRequest.GetString("Summary"), false, false);
                if (string.IsNullOrWhiteSpace(summary))
                {
                    return Json(new { result = false, message = "请填写评论内容" }, JsonRequestBehavior.AllowGet);
                }
                summary = CutString(summary, 2000);
                if (HasDirtyWord(summary))
                {
                    return Json(new { result = false, message = "您的输入内容含有敏感内容，请检查后重试哦" }, JsonRequestBehavior.AllowGet);
                }

                Article article = new SubSonic.Query.Select(provider, "Number", "CreateUserNumber").From<Article>().Where<Article>(x => x.ID == ArticleID).ExecuteSingle<Article>();
                if (article == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }

                //判断是否拉黑
                var black = db.Exists<Black>(x => x.CreateUserNumber == article.CreateUserNumber && x.ToUserNumber == user.Number);
                if (black)
                {
                    return Json(new { result = false, message = "没有权限" }, JsonRequestBehavior.AllowGet);
                }

                Comment model = new Comment();
                model.ArticleNumber = article.Number;
                model.ArticleUserNumber = article.CreateUserNumber;
                model.Summary = summary;
                model.Number = BuildNumber();
                model.CreateDate = DateTime.Now;
                model.CreateUserNumber = user.Number;
                model.CreateIP = Tools.GetClientIP;
                model.ParentCommentNumber = ZNRequest.GetString("ParentCommentNumber");
                model.ParentUserNumber = ZNRequest.GetString("ParentUserNumber");
                model.ID = Tools.SafeInt(db.Add<Comment>(model));
                if (model.ID > 0)
                {
                    return Json(new { result = true, message = model.ID }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("CommentController_Edit:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 详情
        /// </summary>
        public ActionResult Detail()
        {
            try
            {
                var id = ZNRequest.GetInt("ID");
                if (id <= 0)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                var model = db.Single<Comment>(x => x.ID == id);
                if (model == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                model.CreateDateText = model.CreateDate.ToString("yyyy-MM-dd");
                var user = db.Single<User>(x => x.Number == model.CreateUserNumber);
                model.UserID = user == null ? 0 : user.ID;
                model.UserNumber = user == null ? "" : user.Number;
                model.NickName = user == null ? "" : user.NickName;
                model.Avatar = user == null ? "" : user.Avatar;
                if (!string.IsNullOrWhiteSpace(model.ParentUserNumber))
                {
                    var puser = db.Single<User>(x => x.Number == model.ParentUserNumber);

                    model.ParentNickName = puser == null ? "" : puser.NickName;
                }
                if (!string.IsNullOrWhiteSpace(model.ParentCommentNumber))
                {
                    var comment = db.Single<Comment>(x => x.Number == model.ParentCommentNumber);

                    model.ParentCommentID = comment == null ? 0 : comment.ID;
                    model.ParentSummary = comment == null ? "" : comment.Summary;

                    var article = db.Single<Article>(x => x.Number == model.ArticleNumber);

                    model.Title = article == null ? "" : article.Title;
                }

                model.IsZan = 0;

                return Json(new { result = true, message = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("CommentController_Detail:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }
    }
}
