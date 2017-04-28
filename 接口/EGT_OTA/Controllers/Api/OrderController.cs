﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using CommonTools;
using EGT_OTA.Controllers.Filter;
using EGT_OTA.Helper;
using EGT_OTA.Models;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers.Api
{
    public class OrderController : BaseApiController
    {
        /// <summary>
        /// 打赏
        /// </summary>
        [DeflateCompression]
        [HttpGet]
        [Route("Api/Order/All")]
        public string ToMe()
        {
            ApiResult result = new ApiResult();
            try
            {
                var pager = new Pager();
                var query = new SubSonic.Query.Select(provider).From<Order>().Where<Order>(x => x.Status == Enum_Status.Approved);
                var CreateUserNumber = ZNRequest.GetString("CreateUserNumber");
                var ToUserNumber = ZNRequest.GetString("ToUserNumber");
                if (!string.IsNullOrWhiteSpace(CreateUserNumber))
                {
                    query = query.And("CreateUserNumber").IsEqualTo(CreateUserNumber);
                }
                if (!string.IsNullOrWhiteSpace(ToUserNumber))
                {
                    query = query.And("ToUserNumber").IsEqualTo(ToUserNumber);
                }
                var recordCount = query.GetRecordCount();

                if (recordCount == 0)
                {
                    result.message = new { records = 0, totalpage = 1 };
                    return JsonConvert.SerializeObject(result);
                }

                var totalPage = recordCount % pager.Size == 0 ? recordCount / pager.Size : recordCount / pager.Size + 1;
                var list = query.Paged(pager.Index, pager.Size).OrderDesc("ID").ExecuteTypedList<Order>();

                //var fromarray = list.Select(x => x.CreateUserNumber).Distinct().ToList();
                //var toarray = list.Select(x => x.ToUserNumber).Distinct().ToList();
                //fromarray.AddRange(toarray);
                //var allusers = new SubSonic.Query.Select(provider, "ID", "NickName", "Avatar", "Number").From<User>().And("Number").In(fromarray.ToArray()).ExecuteTypedList<User>();

                List<OrderJson> newlist = new List<OrderJson>();
                list.ForEach(x =>
                {
                    var fromUser = db.Single<User>(y => y.Number == x.CreateUserNumber);
                    var toUser = db.Single<User>(y => y.Number == x.ToUserNumber);

                    //var fromUser = allusers.FirstOrDefault(y => y.Number == x.CreateUserNumber);
                    //var toUser = allusers.FirstOrDefault(y => y.Number == x.ToUserNumber);
                    OrderJson model = new OrderJson();
                    model.ID = x.ID;
                    model.CreateDate = x.CreateDate.ToString("yyyy-MM-dd");
                    model.Price = x.Price;
                    if (fromUser != null)
                    {
                        model.FromUserID = fromUser.ID;
                        model.FromUserNumber = fromUser.Number;
                        model.FromUserAvatar = fromUser.Avatar;
                        model.FromUserName = fromUser.NickName;
                    }
                    if (toUser != null)
                    {
                        model.ToUserID = toUser.ID;
                        model.ToUserNumber = toUser.Number;
                        model.ToUserAvatar = toUser.Avatar;
                        model.ToUserName = toUser.NickName;
                    }
                    newlist.Add(model);
                });

                result.result = true;
                result.message = new
                {
                    currpage = pager.Index,
                    records = recordCount,
                    totalpage = totalPage,
                    list = newlist
                };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Order_All:" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}