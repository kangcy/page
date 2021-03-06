﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Controllers.Filter;
using EGT_OTA.Helper;
using EGT_OTA.Models;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 文章
    /// </summary>
    public class ArticleController : BaseController
    {
        /// <summary>
        /// 复制
        /// </summary>
        public ActionResult Copy()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                var id = ZNRequest.GetInt("ArticleID");
                Article article = db.Single<Article>(x => x.ID == id);
                if (article == null)
                {
                    return Json(new { result = false, message = "当前文章不存在" }, JsonRequestBehavior.AllowGet);
                }
                var ip = Tools.GetClientIP;
                var userNumber = user.Number;
                var number = article.Number;
                var result = false;
                var model = article;
                model.Title = article.Title + "(副本)";
                model.Province = ZNRequest.GetString("Province");
                model.City = ZNRequest.GetString("City");
                model.District = ZNRequest.GetString("District");
                model.Street = ZNRequest.GetString("Street");
                model.DetailName = ZNRequest.GetString("DetailName");
                model.CityCode = ZNRequest.GetString("CityCode");
                model.Latitude = Tools.SafeDouble(ZNRequest.GetString("Latitude"));
                model.Longitude = Tools.SafeDouble(ZNRequest.GetString("Longitude"));
                model.CreateUserNumber = userNumber;
                model.CreateDate = DateTime.Now;
                model.CreateIP = ip;
                model.UpdateUserNumber = userNumber;
                model.UpdateDate = DateTime.Now;
                model.UpdateIP = ip;
                model.Status = Enum_Status.Approved;
                model.Views = 0;
                model.Goods = 0;
                model.Keeps = 0;
                model.Comments = 0;
                model.Pays = 0;
                model.Recommend = Enum_ArticleRecommend.None;
                model.ArticlePower = Enum_ArticlePower.Myself;
                model.Number = BuildNumber();
                model.ID = Tools.SafeInt(db.Add<Article>(model));
                result = model.ID > 0;

                if (result)
                {
                    List<ArticlePart> list = new List<ArticlePart>();
                    var parts = db.Find<ArticlePart>(x => x.ArticleNumber == number).ToList();
                    parts.ForEach(x =>
                    {
                        x.ArticleNumber = model.Number;
                        x.Status = Enum_Status.Audit;
                        x.CreateDate = DateTime.Now;
                        x.CreateUserNumber = userNumber;
                        x.CreateIP = ip;
                        list.Add(x);
                    });
                    db.AddMany<ArticlePart>(list);
                }
                if (result)
                {
                    return Json(new { result = true, message = model.ID }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_Copy:" + ex.Message);
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
                var id = ZNRequest.GetInt("ArticleID");
                Article article = db.Single<Article>(x => x.ID == id);
                if (article == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                if (article.CreateUserNumber != user.Number)
                {
                    return Json(new { result = false, message = "没有权限" }, JsonRequestBehavior.AllowGet);
                }
                var result = new SubSonic.Query.Update<Article>(provider).Set("Status").EqualTo(Enum_Status.DELETE).Where<Article>(x => x.ID == article.ID).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_Delete:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        [HttpPost]
        public ActionResult Edit()
        {
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
                }
                Article model = new Article();
                model.ID = ZNRequest.GetInt("ArticleID");
                if (model.ID > 0)
                {
                    model = db.Single<Article>(x => x.ID == model.ID);
                    if (model == null)
                    {
                        model = new Article();
                    }
                }
                model.Title = SqlFilter(ZNRequest.GetString("Title"));
                model.Title = CutString(model.Title, 60);
                if (HasDirtyWord(model.Title))
                {
                    return Json(new { result = false, message = "您输入的标题含有敏感内容，请检查后重试哦" }, JsonRequestBehavior.AllowGet);
                }
                model.MusicID = ZNRequest.GetInt("MusicID");
                model.MusicName = ZNRequest.GetString("MusicName");
                model.MusicUrl = ZNRequest.GetString("MusicUrl");
                model.Province = ZNRequest.GetString("Province");
                model.City = ZNRequest.GetString("City");
                model.District = ZNRequest.GetString("District");
                model.Street = ZNRequest.GetString("Street");
                model.DetailName = ZNRequest.GetString("DetailName");
                model.CityCode = ZNRequest.GetString("CityCode");
                model.Latitude = Tools.SafeDouble(ZNRequest.GetString("Latitude"));
                model.Longitude = Tools.SafeDouble(ZNRequest.GetString("Longitude"));
                model.UpdateUserNumber = user.Number;
                model.UpdateDate = DateTime.Now;
                model.UpdateIP = Tools.GetClientIP;
                model.Status = Enum_Status.Approved;
                var result = false;
                if (model.ID == 0)
                {
                    var cover = ZNRequest.GetString("Cover");
                    if (string.IsNullOrWhiteSpace(cover))
                    {
                        return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                    }
                    var covers = cover.Split(',');
                    model.Cover = covers[0];
                    model.Recommend = Enum_ArticleRecommend.None;
                    model.TypeID = 0;
                    model.TypeIDList = "-0-0-";
                    model.ArticlePower = Enum_ArticlePower.Myself;
                    model.ArticlePowerPwd = string.Empty;
                    model.CreateUserNumber = user.Number;
                    model.CreateDate = DateTime.Now;
                    model.CreateIP = Tools.GetClientIP;
                    model.Number = BuildNumber();
                    model.ID = Tools.SafeInt(db.Add<Article>(model));
                    result = model.ID > 0;

                    //初始化文章段落
                    if (result)
                    {
                        for (var i = 0; i < covers.Length; i++)
                        {
                            ArticlePart part = new ArticlePart();
                            part.ArticleNumber = model.Number;
                            part.Types = Enum_ArticlePart.Pic;
                            part.Introduction = covers[i];
                            part.SortID = i;
                            part.Status = Enum_Status.Audit;
                            part.CreateDate = DateTime.Now;
                            part.CreateUserNumber = user.Number;
                            part.CreateIP = Tools.GetClientIP;
                            part.ID = Tools.SafeInt(db.Add<ArticlePart>(part));
                            result = part.ID > 0;
                        }
                    }
                    if (result)
                    {
                        return Json(new { result = true, message = new { ID = model.ID, Number = model.Number } }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (model.CreateUserNumber != user.Number)
                    {
                        return Json(new { result = false, message = "没有权限" }, JsonRequestBehavior.AllowGet);
                    }
                    result = db.Update<Article>(model) > 0;

                    var parts = SqlFilter(ZNRequest.GetString("Part").Trim(), false, false);

                    if (!string.IsNullOrWhiteSpace(parts))
                    {
                        List<PartJson> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PartJson>>(parts);
                        list.ForEach(x =>
                        {
                            if (x.Status == 0)
                            {
                                //编辑
                                var partid = Tools.SafeInt(x.ID);
                                var part = db.Single<ArticlePart>(y => y.ID == partid);
                                if (part != null)
                                {
                                    part.SortID = x.SortID;
                                    db.Update<ArticlePart>(part);
                                }
                            }
                            else if (x.Status == 1)
                            {
                                //新增
                                ArticlePart part = new ArticlePart();
                                part.ArticleNumber = model.Number;
                                part.Types = x.PartType;
                                part.Introduction = x.Introduction;
                                part.SortID = x.SortID;
                                part.Status = Enum_Status.Audit;
                                part.CreateDate = DateTime.Now;
                                part.CreateUserNumber = user.Number;
                                part.CreateIP = Tools.GetClientIP;
                                part.ID = Tools.SafeInt(db.Add<ArticlePart>(part));
                            }
                            else if (x.Status == 2)
                            {
                                //编辑
                                var partid = Tools.SafeInt(x.ID);
                                var part = db.Single<ArticlePart>(y => y.ID == partid);
                                if (part != null)
                                {
                                    part.Introduction = x.Introduction;
                                    part.SortID = x.SortID;
                                    db.Update<ArticlePart>(part);
                                }
                            }
                            else if (x.Status == 3)
                            {
                                //删除
                                db.Delete<ArticlePart>(x.ID);
                            }
                        });
                    }
                }
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_Edit:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑模板
        /// </summary>
        [ArticlePower]
        public ActionResult EditArticleTemp()
        {
            try
            {
                var ArticleID = ZNRequest.GetInt("ArticleID");
                var Template = ZNRequest.GetInt("Template");
                var result = new SubSonic.Query.Update<Article>(provider).Set("Template").EqualTo(Template).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_EditArticleTemp:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑封面
        /// </summary>
        [ArticlePower]
        public ActionResult EditArticleCover()
        {
            try
            {
                var ArticleID = ZNRequest.GetInt("ArticleID");
                var Cover = ZNRequest.GetString("Cover");
                if (string.IsNullOrWhiteSpace(Cover))
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var result = new SubSonic.Query.Update<Article>(provider).Set("Cover").EqualTo(Cover).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_EditArticleCover:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑音乐
        /// </summary>
        [ArticlePower]
        public ActionResult EditArticleMusic()
        {
            try
            {
                var ArticleID = ZNRequest.GetInt("ArticleID");
                var MusicID = ZNRequest.GetInt("MusicID");
                var MusicName = ZNRequest.GetString("MusicName");
                var MusicUrl = ZNRequest.GetString("MusicUrl");
                var result = new SubSonic.Query.Update<Article>(provider).Set("MusicID").EqualTo(MusicID).Set("MusicUrl").EqualTo(MusicUrl).Set("MusicName").EqualTo(MusicName).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_EditArticleMusic:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑权限
        /// </summary>
        [ArticlePower]
        public ActionResult EditArticlePower()
        {
            try
            {
                var ArticleID = ZNRequest.GetInt("ArticleID");
                Article article = db.Single<Article>(x => x.ID == ArticleID);
                var ArticlePower = ZNRequest.GetInt("ArticlePower", Enum_ArticlePower.Myself);
                var result = new SubSonic.Query.Update<Article>(provider).Set("ArticlePower").EqualTo(ArticlePower).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
                if (result)
                {
                    //用户相册是否展示
                    var status = ArticlePower == Enum_ArticlePower.Public ? Enum_Status.Approved : Enum_Status.Audit;
                    new SubSonic.Query.Update<ArticlePart>(provider).Set("Status").EqualTo(status).Where<ArticlePart>(x => x.ArticleNumber == article.Number).Execute();

                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_EditArticlePower:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        [ArticlePower]
        public ActionResult EditArticleType()
        {
            try
            {
                var ArticleID = ZNRequest.GetInt("ArticleID");
                var TypeID = ZNRequest.GetInt("ArticleType");
                if (TypeID <= 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var articleType = GetArticleType().FirstOrDefault<ArticleType>(x => x.ID == TypeID);
                if (articleType == null)
                {
                    return Json(new { result = false, message = "不存在当前类型" }, JsonRequestBehavior.AllowGet);
                }
                var result = new SubSonic.Query.Update<Article>(provider).Set("TypeID").EqualTo(TypeID).Set("TypeIDList").EqualTo(articleType.ParentIDList).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_EditArticleType:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑背景
        /// </summary>
        [ArticlePower]
        public ActionResult EditBackground()
        {
            try
            {
                var ArticleID = ZNRequest.GetInt("ArticleID");
                var background = ZNRequest.GetString("Background");
                var result = new SubSonic.Query.Update<Article>(provider).Set("Background").EqualTo(background).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_EditBackground:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 校验权限
        /// </summary>
        public ActionResult CheckPowerPwd()
        {
            try
            {
                var ArticleID = ZNRequest.GetInt("ArticleID");
                if (ArticleID <= 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var pwd = ZNRequest.GetString("ArticlePowerPwd");
                var result = db.Exists<Article>(x => x.ID == ArticleID && x.ArticlePower == Enum_ArticlePower.Password && x.ArticlePowerPwd == pwd && x.Status == Enum_Status.Approved);
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_CheckPowerPwd:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 模板列表
        /// </summary>
        public ActionResult Template()
        {
            try
            {
                var list = GetArticleTemp().OrderBy(x => x.ID).ToList();
                var result = new
                {
                    currpage = 1,
                    records = list.Count(),
                    totalpage = 1,
                    list = list
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_Template:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 投稿
        /// </summary>
        [ArticlePower]
        public ActionResult Recommend()
        {
            try
            {
                var UserNumber = ZNRequest.GetString("UserNumber");
                var ArticleNumber = ZNRequest.GetString("ArticleNumber");
                if (string.IsNullOrWhiteSpace(UserNumber) || string.IsNullOrWhiteSpace(ArticleNumber))
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var time = DateTime.Now.AddDays(-7);
                var log = db.Single<ArticleRecommend>(x => x.CreateUserNumber == UserNumber && x.CreateDate > time);
                if (log != null)
                {
                    return Json(new { result = false, message = "每7日只有一次投稿机会，上次投稿时间为：" + log.CreateDate.ToString("yyyy-MM-dd") }, JsonRequestBehavior.AllowGet);
                }
                ArticleRecommend model = new ArticleRecommend();
                model.ArticleNumber = ArticleNumber;
                model.CreateUserNumber = UserNumber;
                model.CreateDate = DateTime.Now;
                model.CreateIP = Tools.GetClientIP;
                var result = Tools.SafeInt(db.Add<ArticleRecommend>(model)) > 0;
                if (result)
                {
                    return Json(new { result = true, message = "成功" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_Recommend:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }
    }
}
