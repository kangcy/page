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
    }
}
