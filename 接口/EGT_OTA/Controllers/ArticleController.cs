using System;
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
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Status").EqualTo(Enum_Status.DELETE).Where<Article>(x => x.ID == article.ID).Execute() > 0;
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
                model.Title = AntiXssChineseString.ChineseStringSanitize(SqlFilter(ZNRequest.GetString("Title")));
                model.Title = CutString(model.Title, 60);
                if (HasDirtyWord(model.Title))
                {
                    return Json(new { result = false, message = "您输入的标题含有敏感内容，请检查后重试哦" }, JsonRequestBehavior.AllowGet);
                }
                model.MusicID = ZNRequest.GetInt("MusicID");
                model.MusicName = AntiXssChineseString.ChineseStringSanitize(ZNRequest.GetString("MusicName"));
                model.MusicUrl = AntiXssChineseString.ChineseStringSanitize(ZNRequest.GetString("MusicUrl"));
                model.Province = AntiXssChineseString.ChineseStringSanitize(ZNRequest.GetString("Province"));
                model.City = AntiXssChineseString.ChineseStringSanitize(ZNRequest.GetString("City"));
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
                    model.TypeID = 10000;
                    model.TypeIDList = "-10000-";
                    model.ArticlePower = Enum_ArticlePower.Myself;
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

                    var parts = ZNRequest.GetString("PartIDs");
                    if (!string.IsNullOrWhiteSpace(parts))
                    {
                        var ids = parts.Split(',').ToList();
                        ids.ForEach(x =>
                        {
                            var id = x.Split('-');
                            var partid = Tools.SafeInt(id[0]);
                            var index = Tools.SafeInt(id[1]);
                            new SubSonic.Query.Update<ArticlePart>(Repository.GetProvider()).Set("SortID").EqualTo(index).Where<ArticlePart>(y => y.ID == partid).Execute();
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
        /// 详情
        /// </summary>
        public ActionResult Detail()
        {
            try
            {
                var id = ZNRequest.GetInt("ArticleID");
                if (id == 0)
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                Article model = db.Single<Article>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { result = false, message = "文章信息异常" }, JsonRequestBehavior.AllowGet);
                }
                if (model.Status == Enum_Status.DELETE)
                {
                    return Json(new { result = false, message = "当前文章已删除，请刷新重试" }, JsonRequestBehavior.AllowGet);
                }

                string password = ZNRequest.GetString("ArticlePassword");

                //浏览数
                new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Views").EqualTo(model.Views + 1).Where<Article>(x => x.ID == model.ID).Execute();

                //打赏数
                model.Pays = new SubSonic.Query.Select(Repository.GetProvider()).From<Order>().Where<Order>(x => x.ToArticleNumber == model.Number && x.Status == Enum_Status.Approved).GetRecordCount();

                //收藏数
                model.Keeps = new SubSonic.Query.Select(Repository.GetProvider()).From<Keep>().Where<Keep>(x => x.ArticleNumber == model.Number).GetRecordCount();

                //评论数
                model.Comments = new SubSonic.Query.Select(Repository.GetProvider()).From<Comment>().Where<Comment>(x => x.ArticleNumber == model.Number).GetRecordCount();

                //创建人
                User createUser = db.Single<User>(x => x.Number == model.CreateUserNumber);
                if (createUser != null)
                {
                    model.UserID = createUser.ID;
                    model.NickName = createUser.NickName;
                    model.Avatar = createUser.Avatar;
                    model.AutoMusic = createUser.AutoMusic;
                    model.ShareNick = createUser.ShareNick;
                }

                //类型
                ArticleType articleType = GetArticleType().FirstOrDefault<ArticleType>(x => x.ID == model.TypeID);
                model.TypeName = articleType == null ? string.Empty : articleType.Name;

                //音乐
                if (model.MusicID > 0)
                {
                    List<Music> musics = new List<Music>();
                    List<MusicJson> list = GetMusic();
                    list.ForEach(x =>
                    {
                        musics.AddRange(x.Music);
                    });
                    Music music = musics.FirstOrDefault<Music>(x => x.ID == model.MusicID);
                    model.MusicUrl = music == null ? "" : music.FileUrl;
                    model.MusicName = music == null ? "" : music.Name;
                }

                //文章部分
                model.ArticlePart = db.Find<ArticlePart>(x => x.ArticleNumber == model.Number).OrderBy(x => x.SortID).ToList();

                model.CreateDateText = DateTime.Now.ToString("yyyy-MM-dd");
                model.ShareUrl = System.Configuration.ConfigurationManager.AppSettings["share_url"] + model.Number;

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
                LogHelper.ErrorLoger.Error("ArticleController_Detail:" + ex.Message);
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
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Template").EqualTo(Template).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
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
                var Cover = AntiXssChineseString.ChineseStringSanitize(SqlFilter(ZNRequest.GetString("Cover")));
                if (string.IsNullOrWhiteSpace(Cover))
                {
                    return Json(new { result = false, message = "参数异常" }, JsonRequestBehavior.AllowGet);
                }
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Cover").EqualTo(Cover).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
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
                var MusicName = AntiXssChineseString.ChineseStringSanitize(SqlFilter(ZNRequest.GetString("MusicName")));
                var MusicUrl = AntiXssChineseString.ChineseStringSanitize(SqlFilter(ZNRequest.GetString("MusicUrl")));
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("MusicID").EqualTo(MusicID).Set("MusicUrl").EqualTo(MusicUrl).Set("MusicName").EqualTo(MusicName).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
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
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("ArticlePower").EqualTo(ArticlePower).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
                if (result)
                {
                    //用户相册是否展示
                    var status = ArticlePower == Enum_ArticlePower.Public ? Enum_Status.Approved : Enum_Status.Audit;
                    new SubSonic.Query.Update<ArticlePart>(Repository.GetProvider()).Set("Status").EqualTo(status).Where<ArticlePart>(x => x.ArticleNumber == article.Number).Execute();

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
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("TypeID").EqualTo(TypeID).Set("TypeIDList").EqualTo(articleType.ParentIDList).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
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
                var background = ZNRequest.GetInt("Background");
                var result = new SubSonic.Query.Update<Article>(Repository.GetProvider()).Set("Background").EqualTo(background).Where<Article>(x => x.ID == ArticleID).Execute() > 0;
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
                var pwd = ZNRequest.GetInt("ArticlePowerPwd");
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
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                //创建人
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Article>().Where<Article>(x => x.Status == Enum_Status.Approved);

                //昵称
                var title = ZNRequest.GetString("Title");
                if (!string.IsNullOrWhiteSpace(title))
                {
                    query.And("Title").Like("%" + title + "%");
                }
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                if (!string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("CreateUserNumber").IsEqualTo(CreateUserNumber);
                }

                //其他用户的文章
                var CurrUserNumber = ZNRequest.GetString("CurrUserNumber");
                if (CreateUserNumber != CurrUserNumber || string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("ArticlePower").IsEqualTo(Enum_ArticlePower.Public);
                }

                //文章类型
                var TypeID = ZNRequest.GetInt("TypeID");
                if (TypeID > 0)
                {
                    query = query.And("TypeIDList").Like("%-" + TypeID.ToString() + "-%");
                }

                //搜索默认显示推荐文章
                var Source = ZNRequest.GetString("Source");
                if (!string.IsNullOrWhiteSpace(Source))
                {
                    query = query.And("Recommend").IsEqualTo(Enum_ArticleRecommend.Recommend);
                }

                //过滤黑名单
                var Number = ZNRequest.GetString("Number");
                if (!string.IsNullOrWhiteSpace(Number))
                {
                    var black = db.Find<Black>(x => x.CreateUserNumber == Number);
                    if (black.Count > 0)
                    {
                        var userids = black.Select(x => x.ToUserNumber).ToArray();
                        query = query.And("CreateUserNumber").NotIn(userids);
                    }
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
                var list = query.Paged(pager.Index, pager.Size).OrderDesc(new string[] { "Recommend", "ID" }).ExecuteTypedList<Article>();
                List<ArticleJson> newlist = ArticleListInfo(list, Number);
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
                LogHelper.ErrorLoger.Error("ArticleController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult Top()
        {
            try
            {
                //创建人
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Article>().Where<Article>(x => x.Status == Enum_Status.Approved);

                //昵称
                var title = ZNRequest.GetString("Title");
                if (!string.IsNullOrEmpty(title))
                {
                    query.And("Title").Like("%" + title + "%");
                }
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                if (!string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("CreateUserNumber").IsEqualTo(CreateUserNumber);
                }

                var CurrUserNumber = ZNRequest.GetString("CurrUserNumber");
                if (CreateUserNumber != CurrUserNumber || string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("ArticlePower").IsEqualTo(Enum_ArticlePower.Public);
                }

                //文章类型
                var TypeID = ZNRequest.GetInt("TypeID");
                if (TypeID > 0)
                {
                    query = query.And("TypeIDList").Like("%-" + TypeID + "-%");
                }

                //搜索默认显示推荐文章
                var Source = ZNRequest.GetString("Source");
                if (!string.IsNullOrWhiteSpace(Source))
                {
                    query = query.And("Recommend").IsEqualTo(Enum_ArticleRecommend.Recommend);
                }

                //过滤黑名单
                var Number = ZNRequest.GetString("Number");
                if (!string.IsNullOrWhiteSpace(Number))
                {
                    var black = db.Find<Black>(x => x.CreateUserNumber == Number);
                    if (black.Count > 0)
                    {
                        var userids = black.Select(x => x.ToUserNumber).ToArray();
                        query = query.And("CreateUserNumber").NotIn(userids);
                    }
                }

                var recordCount = query.GetRecordCount();
                var list = query.Paged(pager.Index, pager.Size).OrderDesc(new string[] { "Recommend", "ID" }).ExecuteTypedList<Article>();

                var newlist = (from a in list
                               select new
                               {
                                   ArticleID = a.ID,
                                   Title = a.Title,
                                   Views = a.Views,
                                   Goods = a.Goods,
                                   Comments = a.Comments,
                                   Keeps = a.Keeps,
                                   Pays = a.Pays,
                                   UserNumber = a.CreateUserNumber,
                                   Cover = a.Cover,
                                   CreateDate = FormatTime(a.CreateDate),
                                   ArticlePower = a.ArticlePower,
                                   Recommend = a.Recommend,
                                   City = a.City
                               }).ToList();
                var result = new
                {
                    records = recordCount,
                    list = newlist
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleController_Top:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
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
