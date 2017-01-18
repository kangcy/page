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
    /// 打赏管理
    /// </summary>
    public class OrderController : BaseController
    {
        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Pay>().Where<Order>(x => x.Status == Enum_Status.Approved);
                var FromUserID = ZNRequest.GetString("UserNumber");
                var ToUserID = ZNRequest.GetString("ToUserNumber");
                if (!string.IsNullOrWhiteSpace(FromUserID))
                {
                    query = query.And("UserNumber").IsEqualTo(FromUserID);
                }
                if (!string.IsNullOrWhiteSpace(ToUserID))
                {
                    query = query.And("ToUserNumber").IsEqualTo(ToUserID);
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Order>();

                var fromarray = list.Select(x => x.UserNumber).Distinct().ToList();
                var toarray = list.Select(x => x.ToUserNumber).Distinct().ToList();
                fromarray.AddRange(toarray);
                var allusers = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().And("Number").In(fromarray.ToArray()).ExecuteTypedList<User>();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   FromUserID = allusers.Exists(x => x.Number == l.UserNumber) ? allusers.FirstOrDefault(x => x.Number == l.UserNumber).ID : 0,
                                   FromUserAvatar = GetFullUrl(allusers.Exists(x => x.Number == l.UserNumber) ? allusers.FirstOrDefault(x => x.Number == l.UserNumber).Avatar : ""),
                                   FromUserName = allusers.Exists(x => x.Number == l.UserNumber) ? allusers.FirstOrDefault(x => x.Number == l.UserNumber).NickName : "",
                                   ToUserAvatar = GetFullUrl(allusers.Exists(x => x.Number == l.ToUserNumber) ? allusers.FirstOrDefault(x => x.Number == l.ToUserNumber).Avatar : ""),
                                   ToUserName = allusers.Exists(x => x.Number == l.ToUserNumber) ? allusers.FirstOrDefault(x => x.Number == l.ToUserNumber).NickName : "",
                                   ToUserID = allusers.Exists(x => x.Number == l.ToUserNumber) ? allusers.FirstOrDefault(x => x.Number == l.ToUserNumber).ID : 0,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   Money = l.Price
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
                LogHelper.ErrorLoger.Error("PayController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
