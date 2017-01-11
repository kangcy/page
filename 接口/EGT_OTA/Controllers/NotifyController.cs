using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Models;
using IPay.Alipay;

namespace EGT_OTA.Controllers
{
    public class NotifyController : BaseController
    {
        protected SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();

        /// <summary>
        /// 支付宝
        /// </summary>
        public ActionResult AliPay()
        {
            var result = new ResultJson();
            bool verifyResult = false;
            string strResult = "";
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();

            Notify aliNotify = new Notify();
            verifyResult = aliNotify.Verify(sArray, ZNRequest.GetString("notify_id"), ZNRequest.GetString("sign"));
            foreach (KeyValuePair<string, string> kvp in sArray)
            {
                strResult += kvp.Key;
                strResult += ":";
                strResult += kvp.Value;
                strResult += "\r\n";
            }

            //记录文本日志
            LogHelper.InfoLoger.Info("NotifyController_AliPay:" + "支付通知参数：" + strResult);

            if (verifyResult && !string.IsNullOrWhiteSpace(strResult))
            {
                //商户订单号
                string oid = sArray["out_trade_no"];

                //支付宝交易号
                string trade_no = sArray["trade_no"];

                //商品名称
                var subject = sArray["subject"];

                //支付价格
                var price = sArray["price"];

                //交易状态
                string trade_status = sArray["trade_status"];

                var order = db.Single<Order>(x => x.OrderNumber == oid);

                if (trade_status == "TRADE_FINISHED" && order != null)
                {
                    //更新充值状态
                    order.Status = Enum_Status.Approved;

                    db.Update<Order>(order);
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WxPay()
        {
            var result = new ResultJson();
            Stream stream = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[stream.Length];
            stream.Read(b, 0, (int)stream.Length);
            string xml = Encoding.UTF8.GetString(b);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            LogHelper.InfoLoger.Info("NotifyController_WxPay:" + "支付通知参数：" + xml);

            if (xml.IndexOf("SUCCESS") > 0)
            {
                FromXml(xml);

                if (CheckSign())
                {
                    string out_trade_no = GetValue("out_trade_no").ToString();
                    string productname = GetValue("attach").ToString();
                    string openid = GetValue("openid").ToString();
                    string pay_fee = GetValue("cash_fee").ToString();
                    double price = double.Parse(pay_fee) / 100;
                    var order = db.Single<Order>(x => x.OrderNumber == out_trade_no);

                    if (order != null)
                    {
                        //更新充值状态
                        order.Status = Enum_Status.Approved;

                        db.Update<Order>(order);
                    }
                }
                else
                {

                    LogHelper.InfoLoger.Info("NotifyController_WxPay:" + "微信支付签名验证失败");
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// xml转排序字典
        /// </summary>
        public SortedDictionary<string, object> FromXml(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode xmlNode = xmlDoc.FirstChild;//获取到根节点<xml>
            XmlNodeList nodes = xmlNode.ChildNodes;
            foreach (XmlNode xn in nodes)
            {
                XmlElement xe = (XmlElement)xn;
                m_values[xe.Name] = xe.InnerText;//获取xml的键值对到WxPayData内部的数据中
            }

            return m_values;
        }

        //检查签名
        public bool CheckSign()
        {
            //获取接收到的签名
            string return_sign = GetValue("sign").ToString();
            //在本地计算新的签名
            string cal_sign = MakeSign();
            if (cal_sign == return_sign)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //本地签名
        public string MakeSign()
        {
            //转url格式
            string str = ToUrl();
            //在string后加入API KEY
            string partnerKey = System.Configuration.ConfigurationManager.AppSettings["wxapppartnerkey"];
            str += "&key=" + partnerKey;
            //MD5加密
            str = MD5Helper.GetMD532(str);
            //所有字符转为大写
            return str.ToUpper();
        }

        //获取字典值
        public object GetValue(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            return o;
        }

        //字典转URL
        public string ToUrl()
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value == null)
                {
                    //Axon.Log.SaveLog("通知参数中含有null字段", context.Server.MapPath("~/log/" + DateTime.Now.ToString("yyyyMMdd") + ".log"));
                }
                if (pair.Key != "sign" && pair.Value.ToString() != "")
                {
                    buff += pair.Key + "=" + pair.Value + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }
    }
}
