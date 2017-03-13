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
using System.Net;
using HtmlAgilityPack;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 音乐管理
    /// </summary>
    public class MusicController : BaseController
    {
        /// <summary>
        /// 列表
        /// </summary>
        public ActionResult All()
        {
            try
            {
                var musicType = GetMusic().OrderBy(x => x.SortID).ToList();
                musicType.ForEach(x =>
                {
                    x.Music.ForEach(l =>
                    {
                        l.Cover = GetFullUrl(l.Cover);
                        l.FileUrl = GetFullUrl(l.FileUrl);
                    });
                });
                var result = new
                {
                    currpage = 1,
                    records = musicType.Count,
                    totalpage = 1,
                    list = musicType
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("MusicController_All:" + ex.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        #region  百度ApiStore

        public ActionResult SearchMusic()
        {
            var name = ZNRequest.GetString("name");
            var page = ZNRequest.GetInt("page", 1);
            var size = ZNRequest.GetInt("rows", 15);
            var url = string.Format("http://apis.baidu.com/geekery/music/query?s={0}&size={1}&page={2}", name, size, page);
            string str = HttpHelper.Get(url);

            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add("apikey", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            StringBuilder sbr = new StringBuilder();
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                sbr.Append(StrDate);
            }
            return Content(sbr.ToString());
        }

        public ActionResult MusicDetail()
        {
            var hash = ZNRequest.GetString("hash");
            var url = string.Format("http://apis.baidu.com/geekery/music/playinfo?hash={0}", hash);
            string str = HttpHelper.Get(url);

            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add("apikey", System.Configuration.ConfigurationManager.AppSettings["apikey"]);
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            StringBuilder sbr = new StringBuilder();
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                sbr.Append(StrDate);
            }
            return Content(sbr.ToString());
        }

        #endregion

        //查询单曲（type:1,s:检索词）
        //http://music.163.com/#/search/m/?id=2884361&s=%E6%88%91&type=1
        //http://music.163.com/api/song/detail?id=439911239&ids=%5B439911239%5D 

        //直接访问这个地址获取JSON
        //http://music.163.com/api/song/detail?ids=%5B 439911239 %5D    


        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// {songs: [ ],equalizers: { },code: 200}
        /// </returns>
        public ActionResult Load()
        {
            var result = string.Empty;
            try
            {
                var id = ZNRequest.GetInt("id", 439911239);
                var json = HttpHelper.Get("http://music.163.com/api/song/detail?ids=%5B" + id + "%5D");
                JArray array = JArray.Parse(JObject.Parse(json)["songs"].ToString());
                JObject model = JObject.Parse(array[0].ToString());
                var musicId = model["id"];
                var musicName = model["name"];
                var musicUrl = model["mp3Url"];
                var artistsArray = JArray.Parse(model["artists"].ToString());
                var artists = JObject.Parse(artistsArray[0].ToString());
                var artistsName = artists["name"];
                var album = JObject.Parse(model["album"].ToString());
                var albumName = album["name"];
                var musicPicUrl = album["picUrl"];
                return Content(musicId + "," + musicName + "," + musicUrl + "," + artistsName + "," + albumName + "," + musicPicUrl);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
