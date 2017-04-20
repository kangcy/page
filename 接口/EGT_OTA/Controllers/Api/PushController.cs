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
    public class PushController : BaseApiController
    {
        /// <summary>
        /// 推送全部
        /// </summary>
        [HttpGet]
        [Route("Api/Push/ALL")]
        public string Edit()
        {
            ApiResult result = new ApiResult();
            try
            {
                var title = ZNRequest.GetString("Title");
                var text = ZNRequest.GetString("Text");

                string clientId = "557e5625f84c82517457c43024b03b0c";
                PushHelper message = new PushHelper(clientId);

                var beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var endTime = DateTime.Now.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss");
                //string msg1 = message.PushMessageToSingleByNotificationTemplate("XXX - 单用户", "您有新的任务，点击查看！", "", "", "", beginTime, endTime);
                //string msg2 = message.PushMessageToListByNotificationTemplate("XXX  - 多用户", "您有新的任务，点击查看！", "", "", "", beginTime, endTime);
                string msg3 = message.PushMessageToAppByNotificationTemplate("XXX  - APP应用", "您有新的任务，点击查看啊！", "", "", "{id:1}", beginTime, endTime);

            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_UserLog_Edit" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 推送单个
        /// </summary>
        [HttpGet]
        [Route("Api/Push/Single")]
        public string Edit()
        {
            ApiResult result = new ApiResult();
            try
            {
                var title = ZNRequest.GetString("Title");
                var text = ZNRequest.GetString("Text");

                string clientId = "557e5625f84c82517457c43024b03b0c";
                PushHelper message = new PushHelper(clientId);

                var beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var endTime = DateTime.Now.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss");
                string msg1 = message.PushMessageToSingleByNotificationTemplate("XXX - 单用户", "您有新的任务，点击查看！", "", "", "", beginTime, endTime);
                string msg2 = message.PushMessageToListByNotificationTemplate("XXX  - 多用户", "您有新的任务，点击查看！", "", "", "", beginTime, endTime);
                string msg3 = message.PushMessageToAppByNotificationTemplate("XXX  - APP应用", "您有新的任务，点击查看啊！", "", "", "{id:1}", beginTime, endTime);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("Api_UserLog_Edit" + ex.Message);
                result.message = ex.Message;
            }
            return JsonConvert.SerializeObject(result);
        }
    }
}