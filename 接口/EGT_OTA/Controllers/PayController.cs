﻿using System;
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
    public class PayController : BaseController
    {
        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Pay>().Where<Pay>(x => x.Status == Enum_Status.Approved);
                var FromUserID = ZNRequest.GetInt("FromUserID");
                var ToUserID = ZNRequest.GetInt("ToUserID");
                if (FromUserID > 0)
                {
                    query = query.And("FromUserID").IsEqualTo(FromUserID);
                }
                if (ToUserID > 0)
                {
                    query = query.And("ToUserID").IsEqualTo(ToUserID);
                }
                var recordCount = query.GetRecordCount();
                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Pay>();

                var fromarray = list.Select(x => x.FromUserID).Distinct().ToList();
                var toarray = list.Select(x => x.ToUserID).Distinct().ToList();
                fromarray.AddRange(toarray);
                var allusers = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar").From<User>().And("ID").In(fromarray.ToArray()).ExecuteTypedList<User>();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   FromUserID = l.FromUserID,
                                   FromUserAvatar = GetFullUrl(allusers.Exists(x => x.ID == l.FromUserID) ? allusers.FirstOrDefault(x => x.ID == l.FromUserID).Avatar : ""),
                                   FromUserName = allusers.Exists(x => x.ID == l.FromUserID) ? allusers.FirstOrDefault(x => x.ID == l.FromUserID).NickName : "",
                                   ToUserAvatar = GetFullUrl(allusers.Exists(x => x.ID == l.ToUserID) ? allusers.FirstOrDefault(x => x.ID == l.ToUserID).Avatar : ""),
                                   ToUserName = allusers.Exists(x => x.ID == l.ToUserID) ? allusers.FirstOrDefault(x => x.ID == l.ToUserID).NickName : "",
                                   ToUserID = l.ToUserID,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   Money = l.Money
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
