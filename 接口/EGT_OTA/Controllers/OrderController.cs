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
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Order>().Where<Order>(x => x.Status == Enum_Status.Approved);
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
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Order>();

                var fromarray = list.Select(x => x.UserNumber).Distinct().ToList();
                var toarray = list.Select(x => x.ToUserNumber).Distinct().ToList();
                fromarray.AddRange(toarray);
                var allusers = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar","Number").From<User>().And("Number").In(fromarray.ToArray()).ExecuteTypedList<User>();

                List<OrderJson> newlist = new List<OrderJson>();
                list.ForEach(x =>
                {
                    var fromUser = allusers.FirstOrDefault(y => y.Number == x.UserNumber);
                    var toUser = allusers.FirstOrDefault(y => y.Number == x.ToUserNumber);
                    OrderJson model = new OrderJson();
                    model.ID = x.ID;
                    model.CreateDate = x.CreateDate.ToString("yyyy-MM-dd");
                    model.Price = x.Price;
                    if (fromUser != null)
                    {
                        model.FromUserID = fromUser.ID;
                        model.FromUserAvatar = fromUser.Avatar;
                        model.FromUserName = fromUser.NickName;
                    }
                    if (toUser != null)
                    {
                        model.ToUserID = toUser.ID;
                        model.ToUserAvatar = toUser.Avatar;
                        model.ToUserName = toUser.NickName;
                    }
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
                LogHelper.ErrorLoger.Error("OrderController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
