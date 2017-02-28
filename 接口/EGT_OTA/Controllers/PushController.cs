using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using EGT_OTA.Helper;

namespace EGT_OTA.Controllers
{
    public class PushController : Controller
    {

        public ActionResult Index()
        {
            var result = string.Empty;
            //try
            //{
            //    Console.OutputEncoding = Encoding.GetEncoding(936);
            //    Environment.SetEnvironmentVariable("gexin_pushList_needDetails", "true");
            //    result = PushHelper.PushMessageToApp();
            //}
            //catch (Exception ex)
            //{
            //    result = ex.Message;
            //}

            string clientId = "f67adb8d5b04a225fa7192b8741eb47b";
            PushHelper message = new PushHelper(clientId);
            //方法调用过程中一定要注意，截止时间一定要大于当前操作时间，建议在当前操作时间的基础上加5分钟时间  
            string msg = message.PushMessageToSingleByNotificationTemplate("XXX - 单用户", "您有新的任务，点击查看！" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", "", "2015-04-20 15:10", "2015-04-20 16:30");
            string msg2 = message.PushMessageToAppByNotificationTemplate("XXX  - APP应用", "您有新的任务，点击查看！", "", "", "", "2015-04-20 10:10", "2015-04-20 14:30");
            string msg3 = message.PushMessageToListByNotificationTemplate("XXX  - 多用户", "您有新的任务，点击查看！", "", "", "", "2015-04-20 10:10", "2015-04-20 14:30");
            Response.Write(msg + "</br>");
            Response.Write(msg2 + "</br>");
            Response.Write(msg3 + "</br>");
            //解析输出结果  
            //{"taskId":"OSS-0420_ZiFBb3Sx7A7Pz7YWMwJdD9","result":"ok","status":"successed_online"} 在线状态  
            //{"taskId":"OSS-0420_2qtgpolflJAuYGSiGTfQ04","result":"ok","status":"successed_offline"} 离线状态  

            return Content(result);
        }

    }
}
