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
                    //取消关注
                    var fan = db.Single<Fan>(x => x.CreateUserNumber == user.Number && x.ToUserNumber == ToUserNumber);
                    if (fan != null)
                    {
                        db.Delete<Fan>(fan.ID);
                    }

                    //更新关注
                    user.Follows = db.Find<Fan>(x => x.CreateUserNumber == user.Number).Count;

                    return Json(new { result = true, message = user.Follows }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { result = true, message = string.Empty }, JsonRequestBehavior.AllowGet);
                }
                var result = db.Delete<Black>(model.ID) > 0;
                if (result)
                {
                    return Json(new { result = true, message = string.Empty }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("BlackController_Delete:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }
    }
}
