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
    /// 关注、粉丝管理
    /// </summary>
    public class FanController : BaseController
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
                var number = ZNRequest.GetString("ToUserNumber");
                if (string.IsNullOrWhiteSpace(number))
                {
                    return Json(new { result = false, message = "信息异常,请刷新重试" }, JsonRequestBehavior.AllowGet);
                }
                Fan model = db.Single<Fan>(x => x.FromUserNumber == user.Number && x.ToUserNumber == number);
                if (model == null)
                {
                    model = new Fan();
                    model.FromUserNumber = user.Number;
                    model.ToUserNumber = number;
                }
                else
                {
                    return Json(new { result = true, message = "已关注" }, JsonRequestBehavior.AllowGet);
                }
                model.CreateDate = DateTime.Now;
                model.CreateIP = Tools.GetClientIP;
                var result = false;
                if (model.ID == 0)
                {
                    result = Tools.SafeInt(db.Add<Fan>(model)) > 0;
                }
                else
                {
                    result = db.Update<Fan>(model) > 0;
                }
                if (result)
                {
                    return Json(new { result = true, message = "已关注" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("FanController_Edit:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 检测是否关注
        /// </summary>
        public ActionResult Check()
        {
            User user = GetUserInfo();
            if (user == null)
            {
                return Json(new { result = false, message = "用户信息验证失败" }, JsonRequestBehavior.AllowGet);
            }
            var message = string.Empty;
            var FromUserNumber = ZNRequest.GetString("FromUserNumber");
            var ToUserNumber = ZNRequest.GetString("ToUserNumber");
            if (string.IsNullOrWhiteSpace(FromUserNumber) || string.IsNullOrWhiteSpace(ToUserNumber))
            {
                return Json(new { result = false, message = "信息异常,请刷新重试" }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var result = db.Exists<Fan>(x => x.FromUserNumber == FromUserNumber && x.ToUserNumber == ToUserNumber);
                return Json(new { result = result, message = message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("FanController_Check:" + ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = false, message = message }, JsonRequestBehavior.AllowGet);
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
                var id = ZNRequest.GetInt("FanID");
                var model = db.Single<Fan>(x => x.ID == id);
                if (model == null)
                {
                    return Json(new { result = false, message = "数据不存在" }, JsonRequestBehavior.AllowGet);
                }
                if (model.FromUserNumber != user.Number)
                {
                    return Json(new { result = false, message = "没有权限" }, JsonRequestBehavior.AllowGet);
                }
                var result = db.Delete<Fan>(id) > 0;
                if (result)
                {
                    //更新关注用户
                    var fans = db.Find<Fan>(x => x.FromUserNumber == user.Number).Select(x => x.ToUserNumber).ToArray();
                    user.FanText = "," + string.Join(",", fans) + ",";

                    return Json(new { result = true, message = user.FanText }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("FanController_Delete:" + ex.Message);
            }
            return Json(new { result = false, message = "失败" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 我的关注列表
        /// </summary>
        public ActionResult FollowsAll()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Fan>().Where<Fan>(x => x.ID > 0);

                var FromUserNumber = ZNRequest.GetString("FromUserNumber");
                if (string.IsNullOrWhiteSpace(FromUserNumber))
                {
                    return Json(new
                    {
                        currpage = pager.Index,
                        records = 0,
                        totalpage = 1,
                        list = string.Empty
                    }, JsonRequestBehavior.AllowGet);
                }
                query = query.And("FromUserNumber").IsEqualTo(FromUserNumber);
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
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
                var array = list.Select(x => x.ToUserNumber).Distinct().ToList();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature", "Number").From<User>().Where<User>(x => x.Status == Enum_Status.Approved).And("Number").In(array.ToArray()).ExecuteTypedList<User>();

                var newlist = (from l in list
                               join u in users on l.ToUserNumber equals u.Number
                               select new
                               {
                                   ID = l.ID,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Signature = u.Signature,
                                   Avatar = GetFullUrl(u.Avatar),
                                   Number = u.Number
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
                LogHelper.ErrorLoger.Error("FanController_FollowsAll:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 我的粉丝列表
        /// </summary>
        public ActionResult FansAll()
        {
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Fan>().Where<Fan>(x => x.ID > 0);

                var ToUserNumber = ZNRequest.GetString("ToUserNumber");
                if (string.IsNullOrWhiteSpace(ToUserNumber))
                {
                    return Json(new
                    {
                        currpage = pager.Index,
                        records = 0,
                        totalpage = 1,
                        list = string.Empty
                    }, JsonRequestBehavior.AllowGet);
                }
                query = query.And("ToUserNumber").IsEqualTo(ToUserNumber);
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
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Fan>();
                var array = list.Select(x => x.FromUserNumber).Distinct().ToList();
                var users = new SubSonic.Query.Select(Repository.GetProvider(), "ID", "NickName", "Avatar", "Signature", "Number").From<User>().Where<User>(x => x.Status == Enum_Status.Approved).And("Number").In(array.ToArray()).ExecuteTypedList<User>();

                var newlist = (from l in list
                               join u in users on l.FromUserNumber equals u.Number
                               select new
                               {
                                   ID = l.ID,
                                   CreateDate = l.CreateDate.ToString("yyyy-MM-dd"),
                                   UserID = u.ID,
                                   NickName = u.NickName,
                                   Signature = u.Signature,
                                   Avatar = GetFullUrl(u.Avatar),
                                   Number = u.Number
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
                LogHelper.ErrorLoger.Error("FanController_FansAll:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 关注用户文章
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var userID = ZNRequest.GetString("Number");
                var pager = new Pager();
                var query = new SubSonic.Query.Select(Repository.GetProvider()).From<Article>().Where<Article>(x => x.Status == Enum_Status.Approved);

                var fans = db.Find<Fan>(x => x.FromUserNumber == userID);


                query.And("CreateUserNumber").In(fans.Select(x => x.ToUserNumber).ToArray()).OrderDesc(new string[] { "ID" }).ExecuteTypedList<Article>();

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

                //过滤黑名单
                var Number = ZNRequest.GetString("Number");
                if (!string.IsNullOrWhiteSpace(Number))
                {
                    var black = db.Find<Black>(x => x.FromUserNumber == Number);
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
                List<ArticleJson> newlist = ArticleListInfo(list);
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
    }
}
