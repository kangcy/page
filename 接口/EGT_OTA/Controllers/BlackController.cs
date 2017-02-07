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
    /// 黑名单管理
    /// </summary>
    public class BlackController : BaseController
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
                var ToUserNumber = ZNRequest.GetString("ToUserNumber");
                if (string.IsNullOrWhiteSpace(ToUserNumber))
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var exist = db.Exists<Black>(x => x.CreateUserNumber == user.Number && x.ToUserNumber == ToUserNumber);
                if (exist)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
                Black model = new Black();
                model.ToUserNumber = ToUserNumber;
                model.CreateDate = DateTime.Now;
                model.CreateUserNumber = user.Number;
                model.CreateIP = Tools.GetClientIP;
                var result = Tools.SafeInt(db.Add<Black>(model)) > 0;
                if (result)
                {
                    //更新黑名单
                    var blacks = db.Find<Black>(x => x.CreateUserNumber == user.Number).Select(x => x.ToUserNumber).ToArray();
                    user.BlackText = "," + string.Join(",", blacks) + ",";

                    //取消关注
                    var fan = db.Single<Fan>(x => x.CreateUserNumber == user.Number && x.ToUserNumber == ToUserNumber);
                    if (fan != null)
                    {
                        db.Delete<Fan>(fan.ID);
                    }

                    //更新关注
                    var fans = db.Find<Fan>(x => x.CreateUserNumber == user.Number).Select(x => x.ToUserNumber).ToArray();
                    user.FanText = "," + string.Join(",", fans) + ",";
                    user.Follows = fans.Length;

                    return Json(new { result = true, message = new { BlackText = user.BlackText, FanText = user.FanText, Follows = user.Follows } }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("BlackController_Edit:" + ex.Message);
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
                var ToUserNumber = ZNRequest.GetString("ToUserNumber");
                var model = db.Single<Black>(x => x.CreateUserNumber == user.Number && x.ToUserNumber == ToUserNumber);
                if (model == null)
                {
                    return Json(new { result = false, message = "数据不存在" }, JsonRequestBehavior.AllowGet);
                }
                var result = db.Delete<Black>(model.ID) > 0;
                if (result)
                {
                    //更新黑名单
                    var blacks = db.Find<Black>(x => x.CreateUserNumber == user.Number).Select(x => x.ToUserNumber).ToArray();
                    user.BlackText = "," + string.Join(",", blacks) + ",";

                    return Json(new { result = true, message = user.BlackText }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("BlackController_Delete:" + ex.Message);
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
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Black>().Where<Black>(x => x.ID > 0);
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                if (!string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("CreateUserNumber").IsEqualTo(CreateUserNumber);
                }
                else
                {
                    return Json(new
                    {
                        currpage = pager.Index,
                        records = 0,
                        list = string.Empty
                    }, JsonRequestBehavior.AllowGet);
                }

                var recordCount = query.GetRecordCount();

                if (recordCount == 0)
                {
                    return Json(new
                    {
                        currpage = pager.Index,
                        records = 0,
                        list = string.Empty
                    }, JsonRequestBehavior.AllowGet);
                }

                var list = query.OrderDesc("ID").ExecuteTypedList<Black>();
                var array = list.Select(x => x.ToUserNumber).ToArray();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature", "Number").From<User>().Where("Number").In(array).ExecuteTypedList<User>();

                List<BlackJson> newlist = new List<BlackJson>();
                list.ForEach(x =>
                {
                    var user = users.FirstOrDefault(y => y.Number == x.ToUserNumber);
                    BlackJson model = new BlackJson();
                    model.ID = x.ID;
                    model.UserID = user == null ? 0 : user.ID;
                    model.Number = user == null ? string.Empty : user.Number;
                    model.NickName = user == null ? string.Empty : user.NickName;
                    model.Avatar = user == null ? string.Empty : user.Avatar;
                    model.Signature = user == null ? string.Empty : user.Signature;
                    model.CreateDate = x.CreateDate.ToString("yyyy-MM-dd");
                    newlist.Add(model);
                });

                var result = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("BlackController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
