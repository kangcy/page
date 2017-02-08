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
    /// 文章部分管理
    /// </summary>
    public class ArticlePartController : BaseController
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
                ArticlePart model = new ArticlePart();
                var id = ZNRequest.GetInt("PartID");
                if (id > 0)
                {
                    model = db.Single<ArticlePart>(x => x.ID == id);
                    if (model == null)
                    {
                        model = new ArticlePart();
                    }
                }
                if (model.ID == 0)
                {
                    model.ArticleNumber = ZNRequest.GetString("ArticleNumber");
                    model.Types = ZNRequest.GetInt("Types", 0);
                    if (string.IsNullOrWhiteSpace(model.ArticleNumber))
                    {
                        return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                    }
                    if (model.Types <= 0)
                    {
                        return Json(new { result = false, message = "段落类型异常" }, JsonRequestBehavior.AllowGet);
                    }
                    model.SortID = 0;
                    model.CreateDate = DateTime.Now;
                }
                model.Introduction = SqlFilter(ZNRequest.GetString("Introduction"), false);

                if (string.IsNullOrWhiteSpace(model.Introduction))
                {
                    return Json(new { result = false, message = "段落信息异常" }, JsonRequestBehavior.AllowGet);
                }
                if (HasDirtyWord(model.Introduction) && model.Types == Enum_ArticlePart.Text)
                {
                    return Json(new { result = false, message = "您的输入内容含有敏感内容，请检查后重试哦" }, JsonRequestBehavior.AllowGet);
                }

                //model.Color = ZNRequest.GetString("Color");
                //model.Align = ZNRequest.GetString("Align");
                //model.FontSize = ZNRequest.GetString("FontSize");
                //model.FontWeight = ZNRequest.GetString("FontWeight");
                //model.Link = ZNRequest.GetString("Link");

                if (model.Types == Enum_ArticlePart.Video)
                {
                    var videoType = ZNRequest.GetInt("VideoType");
                    //非优酷视频
                    if (videoType == 0)
                    {
                        if (!model.Introduction.Contains("id_") || !model.Introduction.Contains(".html"))
                        {
                            return Json(new { result = false, message = "视频网址错误，无法识别" }, JsonRequestBehavior.AllowGet);
                        }
                        var urls = model.Introduction.Split('/');
                        model.Introduction = urls[urls.Length - 1];
                        urls = model.Introduction.Split('.');
                        model.Introduction = urls[0].Replace("id_", "");
                    }
                }
                var newId = model.ID;
                var result = false;
                if (model.ID == 0)
                {
                    model.CreateUserNumber = user.Number;
                    model.CreateIP = Tools.GetClientIP;
                    model.Status = Enum_Status.Audit;
                    newId = Tools.SafeInt(db.Add<ArticlePart>(model));
                    result = newId > 0;
                }
                else
                {
                    result = db.Update<ArticlePart>(model) > 0;
                }
                if (model.Types == Enum_ArticlePart.Video)
                {
                    return Json(new { result = true, message = new { ID = newId, Url = model.Introduction } }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { result = true, message = newId }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticlePartController_Edit:" + ex.Message);
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
                var id = ZNRequest.GetInt("PartID");
                var model = db.Single<ArticlePart>(x => x.ID == id);
                if (model != null)
                {
                    if (!model.CreateUserNumber.Equals(user.Number))
                    {
                        return Json(new { result = false, message = "没有权限" }, JsonRequestBehavior.AllowGet);
                    }
                    var result = db.Delete<ArticlePart>(id) > 0;
                    if (result)
                    {
                        return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticlePartController_Delete:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }
    }
}
