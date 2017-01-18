using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EGT_OTA.Models;
using System.IO;
using Newtonsoft.Json;
using CommonTools;
using EGT_OTA.Helper;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 收藏管理
    /// </summary>
    public class KeepController : BaseController
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
                Article article = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "CreateUserID", "Keeps", "Number").From<Article>().Where<Article>(x => x.ID == articleID).ExecuteSingle<Article>();
                if (article == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                Keep model = db.Single<Keep>(x => x.CreateUserID == user.ID && x.ArticleNumber == article.Number);
                if (model == null)
                {
                    model = new Keep();
                    model.CreateDate = DateTime.Now;
                    model.CreateUserID = user.ID;
                    model.CreateIP = Tools.GetClientIP;
                }
                model.ArticleNumber = article.Number;
                model.ArticleUserID = article.CreateUserID;
                var result = false;
                if (model.ID == 0)
                {
                    result = Tools.SafeInt(db.Add<Keep>(model)) > 0;
                }
                else
                {
                    result = db.Update<Keep>(model) > 0;
                }
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("KeepController_Edit:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public ActionResult Delete()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ID");
                var model = db.Single<Keep>(x => x.ID == id);
                if (model != null)
                {
                    var result = db.Delete<Keep>(id) > 0;
                    if (result)
                    {
                        return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("KeepController_Delete:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Keep>().Where<Keep>(x => x.ID > 0);
                var CreateUserID = ZNRequest.GetInt("CreateUserID");
                if (CreateUserID > 0)
                {
                    query = query.And("CreateUserID").IsEqualTo(CreateUserID);
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
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Keep>();
                var articles = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "Title", "TypeID", "Cover", "Views", "Goods", "Keeps", "Comments", "CreateUserID", "CreateDate", "ArticlePower", "ArticlePowerPwd", "Tag", "City").From<Article>().Where("Number").In(list.Select(x => x.ArticleNumber).ToArray()).OrderDesc(new string[] { "Tag", "ID" }).ExecuteTypedList<Article>();
                var articletypes = GetArticleType();

                //文章编号集合
                var array = articles.Select(x => x.Number).ToArray();

                var parts = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>().Where<ArticlePart>(x => x.Types == Enum_ArticlePart.Pic).And("ArticleNumber").In(array).OrderAsc("SortID").ExecuteTypedList<ArticlePart>();
                var orders = new SubSonic.Query.Select(Repository.GetProvider()).From<Order>().Where<Order>(x => x.Status == Enum_Status.Approved).And("ToArticleNumber").In(array).ExecuteTypedList<Order>();
                var keeps = new SubSonic.Query.Select(Repository.GetProvider()).From<Keep>().Where("ArticleNumber").In(array).ExecuteTypedList<Keep>();
                var comments = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where("ArticleID").In(articles.Select(x => x.ID).ToArray()).ExecuteTypedList<Comment>();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature").From<User>().Where("ID").In(articles.Select(x => x.CreateUserID).ToArray()).ExecuteTypedList<User>();

                List<ArticleJson> newlist = new List<ArticleJson>();
                articles.ForEach(x =>
                {
                    ArticleJson model = new ArticleJson();
                    var user = users.FirstOrDefault(y => y.ID == x.CreateUserID);
                    var articletype = articletypes.FirstOrDefault(y => y.ID == x.TypeID);
                    model.NickName = user == null ? "" : user.NickName;
                    model.Avatar = user == null ? "" : user.Avatar;
                    model.Signature = user == null ? "" : user.Signature;
                    model.ArticleID = x.ID;
                    model.ArticleNumber = x.Number;
                    model.Title = x.Title;
                    model.Views = x.Views;
                    model.Goods = x.Goods;
                    model.Comments = comments.Count(y => y.ArticleID == x.ID);
                    model.Keeps = keeps.Count(y => y.ArticleNumber == x.Number);
                    model.Pays = orders.Count(y => y.ToArticleNumber == x.Number);
                    model.UserID = x.CreateUserID;
                    model.Cover = x.Cover;
                    model.CreateDate = FormatTime(x.CreateDate);
                    model.TypeName = articletype == null ? "" : articletype.Name;
                    model.ArticlePart = parts.Where(y => y.ArticleNumber == x.Number).OrderBy(y => y.ID).Take(4).ToList();
                    model.ArticlePower = x.ArticlePower;
                    model.Tag = x.Tag;
                    model.City = x.City;
                    newlist.Add(model);
                });

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
                LogHelper.ErrorLoger.Error("KeepController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
