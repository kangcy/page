using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EGT_OTA.Models;
using System.IO;
using CommonTools;
using EGT_OTA.Helper;
using System.Web.Security;
using System.Text;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 点赞管理
    /// </summary>
    public class ZanController : BaseController
    {
        /// <summary>
        /// 编辑
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
                var articleID = ZNRequest.GetInt("ArticleID");
                if (articleID == 0)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Article article = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "CreateUserNumber", "Goods", "Number").From<Article>().Where<Article>(x => x.ID == articleID).ExecuteSingle<Article>();
                if (article == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Zan model = db.Single<Zan>(x => x.CreateUserNumber == user.Number && x.ArticleNumber == article.Number && !string.IsNullOrWhiteSpace(x.CommentNumber));
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
                model.ArticleNumber = article.Number;
                model.CommentNumber = string.Empty;
                model.ArticleUserNumber = article.CreateUserNumber;
                var result = Tools.SafeInt(db.Add<Zan>(model)) > 0;

                //修改点赞数
                if (result)
                {
                    var goods = article.Goods + 1;
                    result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Goods").EqualTo(goods).Where<Article>(x => x.ID == articleID).Execute() > 0;
                    if (result)
                    {
                        return Json(new { result = true, message = goods }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ZanController_Edit" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public ActionResult Delete()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }

            var result = false;
            var message = string.Empty;
            var id = ZNRequest.GetInt("ids");
            var model = db.Single<Zan>(x => x.ID == id);
            try
            {
                if (model != null)
                {
                    result = db.Delete<Zan>(id) > 0;

                    //修改文章点赞数
                    if (result)
                    {
                        Article article = new SubSonic.Query.Select(Repository.GetProvider(), "Goods").From<Article>().Where<Article>(x => x.Number == model.ArticleNumber).ExecuteSingle<Article>();
                        if (article != null && article.Goods > 0)
                        {
                            new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Goods").EqualTo(article.Goods - 1).Where<Article>(x => x.Number == model.ArticleNumber).Execute();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ZanController_Delete" + ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 我赞过的
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Zan>().Where<Zan>(x => string.IsNullOrWhiteSpace(x.CommentNumber));
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                if (!string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("CreateUserNumber").IsEqualTo(CreateUserNumber);
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
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Zan>();
                var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Number", "Title", "TypeID", "Cover", "Views", "Goods", "Keeps", "Comments", "CreateUserNumber", "CreateDate", "ArticlePower", "ArticlePowerPwd", "Recommend", "City").From<Article>().Where("Number").In(list.Select(x => x.ArticleNumber).ToArray()).OrderDesc(new string[] { "Recommend", "ID" }).ExecuteTypedList<Article>();

                List<ArticleJson> newlist = ArticleListInfo(articles);

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
                LogHelper.ErrorLoger.Error("ZanController_All" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 赞过我的
        /// </summary>
        public ActionResult All2()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Zan>().Where<Zan>(x => string.IsNullOrWhiteSpace(x.CommentNumber));
                var ArticleUserNumber = ZNRequest.GetString("ArticleUserNumber");
                if (string.IsNullOrWhiteSpace(ArticleUserNumber))
                {
                    query = query.And("ArticleUserNumber").IsEqualTo(ArticleUserNumber);
                }
                var all = query.ExecuteTypedList<Zan>();
                var recordCount = all.Select(x => x.CreateUserNumber).Distinct().Count();

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
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Zan>();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature", "Number").From<User>().Where("Number").In(list.Select(x => x.CreateUserNumber).Distinct().ToArray()).ExecuteTypedList<User>();
                var newlist = (from u in users
                               select new
                                   {
                                       UserID = u.ID,
                                       NickName = u.NickName,
                                       Signature = u.Signature,
                                       Avatar = u.Avatar,
                                       Number = u.Number
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
                LogHelper.ErrorLoger.Error("ZanController_All2" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
