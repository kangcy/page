using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CommonTools;
using EGT_OTA.Helper;
using EGT_OTA.Helper.Config;
using EGT_OTA.Models;

namespace EGT_OTA.Controllers
{
    public class InitController : BaseController
    {
        protected string baseUrl = System.Configuration.ConfigurationManager.AppSettings["base_url"];

        /// <summary>
        /// 初始化数据
        /// </summary>
        public ActionResult Index()
        {
            InitUser();
            InitArticle();
            return Content("成功");
        }

        /// <summary>
        /// 初始化用户
        /// </summary>
        protected void InitUser()
        {
            var users = new List<User>();
            var userLogins = new List<UserLogin>();

            users.Add(new User
            {
                UserName = "18652913873",
                NickName = "Kangcy",
                Password = DesEncryptHelper.Encrypt("123456"),
                Sex = 1,
                Avatar = Server.MapPath("~/Init/UserAvatar/2.jpg"),
                Cover = "../images/header.png",
                ProvinceName = "江苏",
                CityName = "南京",
                Birthday = new DateTime(1991, 2, 15),
                Signature = "幸福就是每天和家人分享快乐甜蜜和喜悦，就是这么简单"
            });

            users.Add(new User
            {
                UserName = "Kaynne",
                NickName = "Kaynne",
                Password = DesEncryptHelper.Encrypt("123456"),
                Sex = 1,
                Avatar = Server.MapPath("~/Init/UserAvatar/1.jpg"),
                Cover = "../images/header.png",
                ProvinceName = "江苏",
                CityName = "南京",
                Birthday = new DateTime(1991, 2, 15),
                Signature = "幸福就是每天和家人分享快乐甜蜜和喜悦，就是这么简单"
            });

            users.ForEach(x =>
            {
                userLogins.Add(new UserLogin(x.Number, Guid.NewGuid().ToString("N"), Enum_UserLogin.Common));
                x.Avatar = Thumb(x.Avatar, baseUrl, "UserAvatar", 0);
                Thread.Sleep(2000);
            });

            db.AddMany<User>(users);
            db.AddMany<UserLogin>(userLogins);
        }

        /// <summary>
        /// 初始化文章
        /// </summary>
        protected void InitArticle()
        {
            var articles = new List<Article>();
            var articleParts = new List<ArticlePart>();

            articles.Add(new Article
            {
                Title = "测试文章1",
                Province = "江苏",
                City = "南京",
                Cover = Server.MapPath("~/Init/Article/1.jpg"),
                CreateUserID = 1,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Views = new Random().Next(200, 10000),
                Goods = new Random().Next(100),
                Keeps = new Random().Next(100),
                ArticlePart = new List<ArticlePart>
                {
                    new ArticlePart(string.Empty,Enum_ArticlePart.Pic,0,Server.MapPath("~/Init/Article/1.jpg")),
                    new ArticlePart(string.Empty,Enum_ArticlePart.Pic,0,Server.MapPath("~/Init/Article/2.jpg")),
                    new ArticlePart(string.Empty,Enum_ArticlePart.Pic,0,Server.MapPath("~/Init/Article/3.jpg"))
                }
            });

            articles.Add(new Article
            {
                Title = "测试文章2",
                Province = "江苏",
                City = "常州",
                Cover = Server.MapPath("~/Init/Article/2.jpg"),
                CreateUserID = 1,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Views = new Random().Next(200, 10000),
                Goods = new Random().Next(100),
                Keeps = new Random().Next(100),
                ArticlePart = new List<ArticlePart>
                {
                    new ArticlePart(string.Empty,Enum_ArticlePart.Pic,0,Server.MapPath("~/Init/Article/4.jpg")),
                    new ArticlePart(string.Empty,Enum_ArticlePart.Pic,0,Server.MapPath("~/Init/Article/5.jpg")),
                    new ArticlePart(string.Empty,Enum_ArticlePart.Pic,0,Server.MapPath("~/Init/Article/6.jpg"))
                }
            });

            articles.ForEach(x =>
            {
                x.Number = ValidateCodeHelper.BuildCode(15);
                x.Cover = Thumb(x.Cover, baseUrl, "Article", 0);
                Thread.Sleep(2000);

                x.ArticlePart.ForEach(y =>
                {
                    y.ArticleNumber = x.Number;

                    if (y.Types == Enum_ArticlePart.Pic)
                    {
                        y.Introduction = Thumb(y.Introduction, baseUrl, "Article", 0);
                        Thread.Sleep(2000);
                    }
                });
                articleParts.AddRange(x.ArticlePart);
            });

            db.AddMany<Article>(articles);
            db.AddMany<ArticlePart>(articleParts);
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="url">原始图片本地路径</param>
        /// <param name="baseUrl">基本路径</param>
        /// <param name="standards">缩略图规格</param>
        /// <param name="isDraw">是否添加水印</param>
        protected string Thumb(string fileUrl, string baseUrl, string standards, int isDraw)
        {
            try
            {
                string random = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10000);
                using (FileStream fsRead = new FileStream(fileUrl, FileMode.Open))
                {
                    byte[] heByte = new byte[(int)fsRead.Length];
                    fsRead.Read(heByte, 0, heByte.Length);
                    MemoryStream ms = new MemoryStream(heByte);

                    #region  保存缩略图

                    UploadConfig.ConfigItem config = UploadConfig.Instance.GetConfig(standards);
                    if (config != null)
                    {
                        //缩略图存放根目录
                        string strFile = System.Web.HttpContext.Current.Server.MapPath(config.SavePath) + "/" + DateTime.Now.ToString("yyyyMMdd");
                        if (!Directory.Exists(strFile))
                        {
                            Directory.CreateDirectory(strFile);
                        }
                        Image image = Image.FromStream(ms, true);
                        ///生成缩略图（多种规格的）
                        int i = 0;
                        foreach (UploadConfig.ThumbMode mode in config.ModeList)
                        {
                            ///保存缩略图地址
                            i++;
                            MakeThumbnail(image, mode.Mode, mode.Width, mode.Height, isDraw, strFile + "\\" + random + "_" + i.ToString() + ".jpg");
                        }
                        image.Dispose();
                    }

                    #endregion

                    #region  保存原图

                    string savePath = System.Web.HttpContext.Current.Server.MapPath("~/Upload/Images/" + standards + "/" + DateTime.Now.ToString("yyyyMMdd") + "/");
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    savePath = savePath + "\\" + random + "_0" + ".jpg";
                    Image image2 = Image.FromStream(ms, true);

                    //添加水印
                    if (isDraw == 1)
                    {
                        image2 = WaterMark(image2);
                    }
                    image2.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    image2.Dispose();

                    #endregion

                    fileUrl = baseUrl + "Upload/Images/" + standards + "/" + DateTime.Now.ToString("yyyyMMdd") + "/" + random + "_0" + ".jpg";
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("InitController_Init" + ex.Message, ex);
            }
            return fileUrl;
        }
    }
}
