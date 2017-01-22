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
                var ToUserID = ZNRequest.GetInt("ToUserID");
                if (ToUserID <= 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var exist = db.Exists<Black>(x => x.CreateUserID == user.ID && x.ToUserID == ToUserID);
                if (exist)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
                Black model = new Black();
                model.ToUserID = ToUserID;
                model.CreateDate = DateTime.Now;
                model.CreateUserID = user.ID;
                model.CreateIP = Tools.GetClientIP;
                var result = Tools.SafeInt(db.Add<Black>(model)) > 0;
                if (result)
                {
                    //我拉黑的用户
                    var blacks = db.Find<Black>(x => x.CreateUserID == user.ID).Select(x => x.ToUserID).ToArray();
                    user.BlackText = "," + string.Join(",", blacks) + ",";

                    //取消拉黑用戶关注
                    var fan = db.Single<Fan>(x => x.FromUserID == user.ID && x.ToUserID == ToUserID);
                    if (fan != null)
                    {
                        db.Delete<Fan>(fan.ID);
                    }

                    //我关注的用户
                    var fans = db.Find<Fan>(x => x.FromUserID == user.ID).Select(x => x.ToUserID).ToArray();
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
                var ToUserID = ZNRequest.GetInt("ToUserID");
                var model = db.Single<Black>(x => x.CreateUserID == user.ID && x.ToUserID == ToUserID);
                if (model == null)
                {
                    return Json(new { result = false, message = "数据不存在" }, JsonRequestBehavior.AllowGet);
                }
                var result = db.Delete<Black>(model.ID) > 0;
                if (result)
                {
                    //我拉黑的用户
                    var blacks = db.Find<Black>(x => x.CreateUserID == user.ID).Select(x => x.ToUserID).ToArray();
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
                var CreateUserID = ZNRequest.GetInt("CreateUserID");
                if (CreateUserID > 0)
                {
                    query = query.And("CreateUserID").IsEqualTo(CreateUserID);
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
                var array = list.Select(x => x.ToUserID).ToArray();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature").From<User>().Where("ID").In(array).ExecuteTypedList<User>();

                List<BlackJson> newlist = new List<BlackJson>();
                list.ForEach(x =>
                {
                    var user = users.FirstOrDefault(y => y.ID == x.ToUserID);
                    BlackJson model = new BlackJson();
                    model.ID = x.ID;
                    model.UserID = user == null ? 0 : user.ID;
                    model.NickName = user == null ? "" : user.NickName;
                    model.Avatar = user == null ? "" : user.Avatar;
                    model.Signature = user == null ? "" : user.Signature;
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
