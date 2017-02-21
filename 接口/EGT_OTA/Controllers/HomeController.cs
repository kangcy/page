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
    public class HomeController : BaseController
    {
        /// <summary>
        /// 跳转文章详情
        /// </summary>
        public ActionResult Short(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
            }
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["share_url"] + "Home/index.html?key=" + number);
        }

        /// <summary>
        /// 跳转用户详情
        /// </summary>
        public ActionResult UserShort(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
            }
            return Redirect(System.Configuration.ConfigurationManager.AppSettings["share_url"] + "Home/user.html?key=" + number);
        }

        /// <summary>
        /// 详情
        /// </summary>
        public ActionResult Info()
        {
            try
            {
                var number = ZNRequest.GetString("key");

                if (string.IsNullOrWhiteSpace(number))
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                Article model = db.Single<Article>(x => x.Number == number);
                if (model == null)
                {
                    return Json(new { result = false, message = "信息异常" }, JsonRequestBehavior.AllowGet);
                }
                if (model.Status == Enum_Status.DELETE)
                {
                    return Json(new { result = false, message = "当前文章已删除，请刷新重试" }, JsonRequestBehavior.AllowGet);
                }
                string password = ZNRequest.GetString("ArticlePassword");

                //浏览数
                new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Views").EqualTo(model.Views + 1).Where<Article>(x => x.ID == model.ID).Execute();

                //创建人
                User createUser = db.Single<User>(x => x.Number == model.CreateUserNumber);
                if (createUser != null)
                {
                    model.NickName = createUser == null ? "" : createUser.NickName;
                    model.Avatar = createUser == null ? "" : createUser.Avatar;
                    model.AutoMusic = createUser.AutoMusic;
                    model.ShareNick = createUser.ShareNick;
                }

                //音乐
                if (model.MusicID > 0)
                {
                    Music music = db.Single<Music>(x => x.ID == model.MusicID);
                    model.MusicUrl = music == null ? "" : music.FileUrl;
                    model.MusicName = music == null ? "" : music.Name;
                }

                //文章部分
                model.ArticlePart = new SubSonic.Query.Select(Repository.GetProvider()).From<ArticlePart>().Where<ArticlePart>(x => x.ArticleNumber == model.Number).OrderAsc("SortID").ExecuteTypedList<ArticlePart>();

                model.CreateDateText = DateTime.Now.ToString("yyyy-MM-dd");

                //模板配置
                if (model.Template > 0)
                {
                    model.TemplateJson = GetArticleTemp().FirstOrDefault(x => x.ID == model.Template);
                    if (model.TemplateJson == null)
                    {
                        model.TemplateJson = new Template();
                    }
                }
                else
                {
                    model.TemplateJson = new Template();
                }

                return Json(new { result = true, message = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("HomeController_Info:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }
    }
}
