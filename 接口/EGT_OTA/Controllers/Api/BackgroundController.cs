using System;
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
    public class BackgroundController : BaseApiController
    {
        /// <summary>
        /// 编辑
        /// </summary>
        [HttpPost]
        [Route("Api/Background/Edit")]
        public string Edit()
        {
            ApiResult result = new ApiResult();
            try
            {
                User user = GetUserInfo();
                if (user == null)
                {
                    result.message = "用户信息验证失败";
                    return JsonConvert.SerializeObject(result);
                }
                var number = ZNRequest.GetString("ArticleNumber");
                var url = ZNRequest.GetString("Url");
                if (string.IsNullOrWhiteSpace(number) || string.IsNullOrWhiteSpace(url))
                {
                    result.message = "信息异常";
                    return JsonConvert.SerializeObject(result);
                }
                Background model = db.Single<Background>(x => x.ArticleNumber == number);
                if (model == null)
                {
                    model = new Background();
                    model.Number = BuildNumber();
                    model.ArticleNumber = number;
                    model.CreateUserNumber = user.Number;
                }
                else
                {
                    if (user.Number != model.CreateUserNumber)
                    {
                        result.message = "没有权限";
                        return JsonConvert.SerializeObject(result);
                    }
                }
                model.Full = ZNRequest.GetInt("Full");
                model.High = ZNRequest.GetInt("Hign");
                model.Transparency = ZNRequest.GetInt("Transparency");
                model.Url = url;
                var success = true;
                if (model.ID == 0)
                {
                    success = Tools.SafeInt(db.Add<Background>(model)) > 0;
                }
                else
                {
                    success = db.Update<Background>(model) > 0;
                }
                if (success)
                {
                    result.result = true;
                    result.message = model.Number;
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Background_Edit" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 详情
        /// </summary>
        [HttpGet]
        [Route("Api/Background/Info")]
        public string Info()
        {
            ApiResult result = new ApiResult();
            try
            {
                var number = ZNRequest.GetString("Number");
                if (string.IsNullOrWhiteSpace(number))
                {
                    result.message = "信息异常";
                    return JsonConvert.SerializeObject(result);
                }
                Background model = db.Single<Background>(x => x.ArticleNumber == number);
                if (model == null)
                {
                    model = new Background();
                }

                result.result = true;
                result.message = model;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_Background_Info" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}