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
using System.Threading;

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

        public static int num = 0;
        public ActionResult Init()
        {
            ThreadPool.SetMaxThreads(5, 5);//最多5个线程
            for (var i = 0; i < 5; i++)
            {
                ThreadPool.QueueUserWorkItem(A, 5000);
            }
            Thread.Sleep(1000);//休眠1秒
            return View();
        }

        private void A(object my)
        {
            num += 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// 1万-5000万
        /// 5000万-10000万
        /// 10000万-15000万
        /// 15000万-20000万
        /// 20000万-25000万
        /// 25000万-30000万
        /// 30000万-35000万
        /// 35000万-40000万
        /// 40000万-45000万
        /// 45000万-50000万
        /// <returns></returns>
        [HttpGet]
        public ActionResult InitMusic()
        {
            var str = "";
            try
            {
                var step = 50000000;
                for (var i = 0; i < 10; i++)
                {
                    var index = step * i + 1;

                    str += "线程：" + i + ",初始化：" + index;

                    Thread thread = new Thread(new ThreadStart(delegate
                    {
                        for (var id = index; id < index + step; id++)
                        {
                            try
                            {
                                LoadMusic(id);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.ErrorLoger.Error("InitMusic:" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    }));
                    thread.Name = "同步音乐接口线程" + i;
                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message + "," + str);
            }
            return Content("成功," + str);
        }

        public void LoadMusic(int id)
        {
            var json = HttpHelper.Get("http://music.163.com/api/song/detail?ids=%5B" + id + "%5D");

            JArray array = JArray.Parse(JObject.Parse(json)["songs"].ToString());
            if (array.Count > 0)
            {
                JObject model = JObject.Parse(array[0].ToString());
                var musicId = model["id"].ToString();
                var musicName = model["name"].ToString();
                var musicUrl = model["mp3Url"].ToString();
                var artistsArray = JArray.Parse(model["artists"].ToString());
                var artists = JObject.Parse(artistsArray[0].ToString());
                var artistsName = artists["name"].ToString();
                var album = JObject.Parse(model["album"].ToString());
                var albumName = album["name"].ToString();
                var musicPicUrl = album["picUrl"].ToString();

                Music music = new Music();
                music.Author = artistsName;
                music.Cover = musicPicUrl;
                music.FileUrl = musicUrl;
                music.Name = musicName;
                music.Number = musicId;
                db.Add<Music>(music);
            }
        }
    }
}
