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
        public static String TITLE = "标题";
        public static String ALERT = "提醒";
        public static String MSG_CONTENT = "内容2";
        public static String REGISTRATION_ID = "0900e8d85ef";
        public static String TAG = "tag_api";
        public static String app_key = "3049c387ec1778b46e4e64a8";
        public static String master_secret = "cabe3777244b81384a522f51";

        string appkey = "55794c3667e58ee85b000e84";
        string appMasterSecret = "2rqhtrbxevsekhssi6l8ledupdcnohli";
        string timestamp = UnixTimeHelper.FromDateTime(DateTime.Now).ToString();

        // POST api/<controller>
        [HttpGet]//ios
        public string ios(string deviceid, string msg, string extra)
        {
            using (var db = new imoralContext())
            {
                var ut = db.user_terminal.FirstOrDefault(x => x.PushID == deviceid);

                if (db.msg_user.Count(x => x.uid == ut.UID && x.msg_desc == msg) == 0)
                {
                    var msg_user = new msg_user();
                    msg_user.ctime = DateTime.Now;
                    msg_user.uid = ut.UID;
                    msg_user.msg_desc = msg;
                    msg_user.msg_title = msg.Length > 10 ? msg.Substring(0, 10) : msg;
                    msg_user.msg_icon = "http://res.pinzhi.xin/service/msg.png";
                    if (extra.IndexOf("http://") >= 0)
                    {
                        msg_user.msg_controller = "";
                        msg_user.msg_view = "";
                        msg_user.msg_url = extra;
                    }
                    else
                    {
                        msg_user.msg_controller = extra;
                        msg_user.msg_view = extra;
                        msg_user.msg_url = "";
                    }
                    //db.msg_user.Add(msg_user);
                    //db.SaveChanges();
                }
            }

            bool sandbox = false;
            string testDeviceToken = deviceid.Replace("<", "").Replace(">", "").Replace(" ", "");
            string p12File = "imoral.p12";
            string p12FilePassword = "qiuping";
            string p12Filename = HttpContext.Current.Server.MapPath("~/Cert/" + p12File);

            NotificationService service = new NotificationService(sandbox, p12Filename, p12FilePassword, 1);
            service.SendRetries = 1;
            service.ReconnectDelay = 0;

            service.Error += new NotificationService.OnError(service_Error);
            service.NotificationTooLong += new NotificationService.OnNotificationTooLong(service_NotificationTooLong);
            service.BadDeviceToken += new NotificationService.OnBadDeviceToken(service_BadDeviceToken);
            service.NotificationFailed += new NotificationService.OnNotificationFailed(service_NotificationFailed);
            service.NotificationSuccess += new NotificationService.OnNotificationSuccess(service_NotificationSuccess);
            service.Connecting += new NotificationService.OnConnecting(service_Connecting);
            service.Connected += new NotificationService.OnConnected(service_Connected);
            service.Disconnected += new NotificationService.OnDisconnected(service_Disconnected);

            //Create a new notification to send
            Notification alertNotification = new Notification(testDeviceToken);

            if (extra == "refresh")
            {
                alertNotification.Payload.ContentAvailable = 1;
                //alertNotification.Payload.Sound = "default";
            }
            else
            {
                alertNotification.Payload.Alert.Body = msg;
                alertNotification.Payload.Sound = "default";
                alertNotification.Payload.Badge = 1;
            }

            string[] arr = { extra };
            if (extra != "refresh")
            {
                alertNotification.Payload.CustomItems.Add("msg", arr);
            }
            if (service.QueueNotification(alertNotification))
            {
                service.Close();
                service.Dispose();
                return "success";
            }
            else
            {
                service.Close();
                service.Dispose();
                return "fail";
            }
        }
        [HttpGet]//开发版本
        public string ios_dev(string deviceid, string msg, string extra)
        {
            using (var db = new imoralContext())
            {
                var ut = db.user_terminal.FirstOrDefault(x => x.PushID == deviceid);
                if (db.msg_user.Count(x => x.uid == ut.UID && x.msg_desc == msg) == 0)
                {
                    var msg_user = new msg_user();
                    msg_user.ctime = DateTime.Now;
                    msg_user.uid = ut.UID;
                    msg_user.msg_desc = msg;
                    if (msg.IndexOf("还款") > 0)
                    {
                        msg_user.msg_title = "品值白条还款通知";
                    }
                    else if (msg.IndexOf("额度调整") > 0 || msg.IndexOf("认证信息") > 0)
                    {
                        msg_user.msg_title = "品值额度审核通知";

                    }
                    else
                    {
                        msg_user.msg_title = "品值新消息";
                    }
                    msg_user.msg_icon = "http://res.pinzhi.xin/service/msg.png";
                    if (extra.IndexOf("http://") >= 0)
                    {
                        msg_user.msg_controller = "";
                        msg_user.msg_view = "";
                        msg_user.msg_url = extra;
                    }
                    else
                    {
                        msg_user.msg_controller = extra;
                        msg_user.msg_view = extra;
                        msg_user.msg_url = "";
                    }
                    //db.msg_user.Add(msg_user);
                    //db.SaveChanges();
                }
            }

            bool sandbox = true;
            string testDeviceToken = deviceid.Replace("<", "").Replace(">", "").Replace(" ", "");
            string p12File = "imoral_dev.p12";
            string p12FilePassword = "qiuping";
            string p12Filename = HttpContext.Current.Server.MapPath("~/Cert/" + p12File);

            NotificationService service = new NotificationService(sandbox, p12Filename, p12FilePassword, 1);
            service.SendRetries = 1;
            service.ReconnectDelay = 0;

            service.Error += new NotificationService.OnError(service_Error);
            service.NotificationTooLong += new NotificationService.OnNotificationTooLong(service_NotificationTooLong);
            service.BadDeviceToken += new NotificationService.OnBadDeviceToken(service_BadDeviceToken);
            service.NotificationFailed += new NotificationService.OnNotificationFailed(service_NotificationFailed);
            service.NotificationSuccess += new NotificationService.OnNotificationSuccess(service_NotificationSuccess);
            service.Connecting += new NotificationService.OnConnecting(service_Connecting);
            service.Connected += new NotificationService.OnConnected(service_Connected);
            service.Disconnected += new NotificationService.OnDisconnected(service_Disconnected);

            //Create a new notification to send
            Notification alertNotification = new Notification(testDeviceToken);

            if (extra == "refresh")
            {
                alertNotification.Payload.ContentAvailable = 1;
                //alertNotification.Payload.Sound = "default";
            }
            else
            {
                alertNotification.Payload.Alert.Body = msg;
                alertNotification.Payload.Sound = "default";
                alertNotification.Payload.Badge = 1;
            }

            string[] arr = { extra };
            if (extra != "refresh")
            {
                alertNotification.Payload.CustomItems.Add("msg", arr);
            }
            if (service.QueueNotification(alertNotification))
            {
                service.Close();
                service.Dispose();
                return "success";
            }
            else
            {
                service.Close();
                service.Dispose();
                return "fail";
            }
        }



        /// <summary>
        /// 安卓
        /// </summary>
        /// <param name="alias">可选 要求不超过50个alias,多个alias以英文逗号间隔</param>
        /// <param name="ticker">必填 通知栏提示文字</param>
        /// <param name="title">必填 通知标题</param>
        /// <param name="text">必填 通知文字描述</param>
        /// <param name="after_open">必填 值可以为: go_app（打开应用） 、go_url（跳转到URL）、go_activity（打开特定的activity）、go_custom（用户自定义内容）</param>
        /// <param name="activity">可选 当 after_open 为 go_activity 时，必填</param>
        /// <param name="url">可选 当"after_open"为"go_url"时，必填  通知栏点击后跳转的URL，要求以http或者https开头 </param>
        /// <returns></returns>
        [HttpGet]
        public string androidums(string alias, string ticker, string title, string text, string activity, string url)
        {

            using (var db = new imoralContext())
            {
                var ut = db.user_terminal.FirstOrDefault(x => x.PushID == alias);
                if (db.msg_user.Count(x => x.uid == ut.UID && x.msg_desc == text) == 0)
                {
                    var msg_user = new msg_user();
                    msg_user.ctime = DateTime.Now;
                    msg_user.uid = ut.UID;
                    msg_user.msg_desc = text;
                    msg_user.msg_title = title;
                    msg_user.msg_icon = "http://res.pinzhi.xin/service/msg.png";
                    if (activity == "BrowserActivity")
                    {
                        msg_user.msg_controller = "";
                        msg_user.msg_view = "";
                        msg_user.msg_url = url;
                    }
                    else
                    {
                        msg_user.msg_controller = activity;
                        msg_user.msg_view = activity;
                        msg_user.msg_url = "";
                    }
                    //db.msg_user.Add(msg_user);
                    //db.SaveChanges();
                }
            }

            string ret = "";
            string posturl = "http://msg.umeng.com/api/send";

            string postparam = @"{
  ""appkey"":""" + appkey + @""",          
  ""timestamp"":""" + timestamp + @""",       
  ""type"":""unicast"", 
  ""device_tokens"":""" + alias + @""",  
  ""payload"":             
    {
      ""display_type"":""notification"",  
      ""body"":               
        {
          ""ticker"":""" + ticker + @""",   
          ""title"":""" + title + @""",     
          ""text"":""" + text + @""",     
          ""after_open"": ""go_activity"" ,
          ""activity"": ""com.axon.imoral.activity." + activity + @""",     
        }, ""extra"":                
          {
            ""title"": """ + title + @""",
            ""url"": """ + url + @"""
          }
    }
}";
            string sign = "POST" + posturl + postparam + appMasterSecret;
            sign = Axon.Crypto.MD5.Encrypt(sign, 32);

            //Axon.Log.SaveLog(posturl + "?sign=" + sign, "E://log.txt");
            //Axon.Log.SaveLog(postparam, "E://log.txt");

            ret = Axon.Http.Post(posturl + "?sign=" + sign, postparam);
            if (ret.ToLower().Contains("success"))
            {
                ret = "success";
            }
            else
            {
                //ret = ret;
            }
            return ret;
        }

        public cn.jpush.api.push.mode.PushPayload PushObject_all_alias_alert(string alias, string message, string extra)
        {
            cn.jpush.api.push.mode.PushPayload pushPayload = new cn.jpush.api.push.mode.PushPayload();
            pushPayload.platform = cn.jpush.api.push.mode.Platform.android();
            pushPayload.audience = cn.jpush.api.push.mode.Audience.s_alias(alias);
            //pushPayload.message = cn.jpush.api.push.mode.Message.content(message).AddExtras("msg", extra);
            pushPayload.notification = new cn.jpush.api.push.mode.Notification().setAlert(message).setAndroid(new AndroidNotification().AddExtra("msg", extra));


            //Message msg = Message.content(MSG_CONTENT);
            //msg.msg_content = MSG_CONTENT;
            //pushPayload.message = msg;
            return pushPayload;
        }

        static void service_BadDeviceToken(object sender, BadDeviceTokenException ex)
        {
            Console.WriteLine("Bad Device Token: {0}", ex.Message);
        }

        static void service_Disconnected(object sender)
        {
            Console.WriteLine("Disconnected...");
        }

        static void service_Connected(object sender)
        {
            Console.WriteLine("Connected...");
        }

        static void service_Connecting(object sender)
        {
            Console.WriteLine("Connecting...");
        }

        static void service_NotificationTooLong(object sender, NotificationLengthException ex)
        {
            Console.WriteLine(string.Format("Notification Too Long: {0}", ex.Notification.ToString()));
        }

        static void service_NotificationSuccess(object sender, Notification notification)
        {
            //Axon.Log.SaveLog(string.Format("Notification Success: {0}", notification.ToString()), HttpContext.Current.Server.MapPath("~/Log/" + DateTime.Now.ToString("yyyyMMdd") + "-push.txt"));
            IPro.Comm.Helpers.AppLog.saveLogFiles(string.Format("Notification Success: {0}", notification.ToString()), "push");
            Console.WriteLine(string.Format("Notification Success: {0}", notification.ToString()));
        }

        static void service_NotificationFailed(object sender, Notification notification)
        {
            IPro.Comm.Helpers.AppLog.saveLogFiles(string.Format("Notification Failed: {0}", notification.ToString()), "push");
            Console.WriteLine(string.Format("Notification Failed: {0}", notification.ToString()));
        }

        static void service_Error(object sender, Exception ex)
        {
            IPro.Comm.Helpers.AppLog.saveLogFiles(string.Format("Error: {0}", ex.Message), "push");
            Console.WriteLine(string.Format("Error: {0}", ex.Message));
        }
    }
}