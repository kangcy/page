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
                Article article = new SubSonic.Query.Select(provider, "ID", "CreateUserNumber", "Goods", "Number").From<Article>().Where<Article>(x => x.ID == articleID).ExecuteSingle<Article>();
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

                Zan model = db.Single<Zan>(x => x.CreateUserNumber == user.Number && x.ArticleNumber == article.Number);
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
                model.ZanType = Enum_ZanType.Article;
                var result = Tools.SafeInt(db.Add<Zan>(model)) > 0;

                //修改点赞数
                if (result)
                {
                    var goods = article.Goods + 1;
                    result = new SubSonic.Query.Update<Article>(provider).Set("Goods").EqualTo(goods).Where<Article>(x => x.ID == articleID).Execute() > 0;
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
                        Article article = new SubSonic.Query.Select(provider, "Goods").From<Article>().Where<Article>(x => x.Number == model.ArticleNumber).ExecuteSingle<Article>();
                        if (article != null && article.Goods > 0)
                        {
                            new SubSonic.Query.Update<Article>(provider).Set("Goods").EqualTo(article.Goods - 1).Where<Article>(x => x.Number == model.ArticleNumber).Execute();
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
        /// 赞过我的
        /// </summary>
        public ActionResult ToMe()
        {
            try
            {
                var UserNumber = ZNRequest.GetString("UserNumber");
                var pager = new Pager();
                var query = new SubSonic.Query.Select(provider).From<Zan>().Where<Zan>(x => x.ArticleUserNumber == UserNumber && x.ZanType == Enum_ZanType.Article);
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

                var articleArray = list.Select(x => x.ArticleNumber).ToArray();
                var articles = new SubSonic.Query.Select(provider, "ID", "Number", "Cover", "ArticlePower", "CreateUserNumber", "Status", "Title").From<Article>().Where("Number").In(articleArray).And("Status").IsNotEqualTo(Enum_Status.Audit).And("CreateUserNumber").IsEqualTo(UserNumber).ExecuteTypedList<Article>();

                var userArray = list.Select(x => x.CreateUserNumber).ToArray();
                var users = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Number").From<User>().Where("Number").In(userArray).ExecuteTypedList<User>();

                List<ZanJson> zans = new List<ZanJson>();
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
                        zans.Add(model);
                    }
                });
                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = zans
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ZanController_ToMe" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
