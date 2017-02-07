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
    /// 文章类型管理
    /// </summary>
    public class ArticleTypeController : BaseController
    {
        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var list = GetArticleType().OrderBy(x => x.SortID).ToList();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   Cover = GetFullUrl(l.Cover),
                                   Name = l.Name,
                                   Summary = l.Summary,
                                   ParentID = l.ParentID,
                                   ParentIDList = l.ParentIDList
                               }).ToList();
                var result = new
                {
                    currpage = 1,
                    records = list.Count(),
                    totalpage = 1,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleTypeController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All2()
        {
            try
            {
                var articleType = GetArticleType();
                var firstType = articleType.FindAll(x => x.ParentID == 0).OrderBy(x => x.ID).ToList();

                firstType.ForEach(x =>
                {
                    x.List = new List<ArticleType>();
                    x.List.AddRange(articleType.FindAll(y => y.ParentID == x.ID).OrderBy(y => y.ID).ToList());
                });
                var result = new
                {
                    currpage = 1,
                    records = firstType.Count,
                    totalpage = 1,
                    list = firstType
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleTypeController_All2:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
