using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using EGT_OTA.Models;
using CommonTools;
using SubSonic.Repository;
using System.Text.RegularExpressions;
using EGT_OTA.Helper;
using System.Drawing;
using EGT_OTA.Helper.Config;

namespace EGT_OTA.Controllers
{
    public class BaseController : Controller
    {
        protected readonly SimpleRepository db = Repository.GetRepo();

        //默认管理员账号
        protected readonly string Admin_Name = System.Web.Configuration.WebConfigurationManager.AppSettings["admin_name"];
        protected readonly string Admin_Password = System.Web.Configuration.WebConfigurationManager.AppSettings["admin_password"];
        protected readonly string Base_Url = System.Web.Configuration.WebConfigurationManager.AppSettings["base_url"];

        /// <summary>
        /// 分页基础类
        /// </summary>
        public class Pager
        {
            public int Index { get; set; }
            public int Size { get; set; }

            public Pager()
            {
                this.Index = ZNRequest.GetInt("page", 1);
                this.Size = ZNRequest.GetInt("rows", 15);
            }
        }

        /// <summary>
        /// 解码
        /// </summary>
        protected string UrlDecode(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return string.Empty;
            }
            return System.Web.HttpContext.Current.Server.UrlDecode(msg);
        }

        /// <summary>
        /// 防注入
        /// </summary>
        protected string SqlFilter(string inputString, bool nohtml = true)
        {
            string SqlStr = @"and|or|exec|execute|insert|select|delete|update|alter|create|drop|count|\*|chr|char|asc|mid|substring|master|truncate|declare|xp_cmdshell|restore|backup|net +user|net +localgroup +administrators";
            try
            {
                if (!string.IsNullOrEmpty(inputString))
                {
                    inputString = UrlDecode(inputString);
                    if (nohtml)
                    {
                        inputString = Tools.NoHTML(inputString);
                    }
                    inputString = Regex.Replace(inputString, @"\b(" + SqlStr + @")\b", string.Empty, RegexOptions.IgnoreCase);
                    if (nohtml)
                    {
                        inputString = inputString.Replace("&nbsp;", "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("SQL注入", ex);
            }
            return inputString;
        }

        /// <summary>
        /// 图片完整路径
        /// </summary>
        protected string GetFullUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Base_Url + "Images/default.png";
            }
            if (url.ToLower().StartsWith("http"))
            {
                return url;
            }
            return Base_Url + url;
        }

        /// <summary>
        /// APP访问用户信息
        /// </summary>
        protected User GetUserInfo()
        {
            var id = ZNRequest.GetInt("ID");
            if (id == 0)
            {
                return null;
            }
            return db.Single<User>(x => x.ID == id);
        }

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        protected string FormatTime(DateTime date)
        {
            var totalSeconds = Convert.ToInt32((DateTime.Now - date).TotalSeconds);
            var hour = (totalSeconds / 3600);
            var year = 24 * 365;
            if (hour > year)
            {
                return Convert.ToInt32(hour / year) + "年前";
            }
            else if (hour > 24)
            {
                return Convert.ToInt32(hour / 24) + "天前";
            }
            else if (hour > 0)
            {
                return Convert.ToInt32(hour) + "小时前";
            }
            else
            {
                var minute = totalSeconds / 60;
                if (minute > 0)
                {
                    return Convert.ToInt32(minute) + "分钟前";
                }
                else
                {
                    return totalSeconds + "秒前";
                }
            }
        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="content">原始字符串</param>
        protected string CutString(string content, int length)
        {
            var nameByte = System.Text.Encoding.Default.GetBytes(content);
            if (nameByte.Length > length)
            {
                byte[] b = new byte[length];
                Array.Copy(nameByte, 0, b, 0, length);
                content = System.Text.Encoding.Default.GetString(b); //重新获取字符串
            }
            return content;
        }

        /// <summary>
        /// 音乐
        /// </summary>
        protected List<MusicJson> GetMusic()
        {
            List<MusicJson> list = new List<MusicJson>();
            if (CacheHelper.Exists("Music"))
            {
                list = (List<MusicJson>)CacheHelper.GetCache("Music");
            }
            else
            {
                string str = string.Empty;
                string filePath = System.Web.HttpContext.Current.Server.MapPath("/Config/music.config");
                if (System.IO.File.Exists(filePath))
                {
                    StreamReader sr = new StreamReader(filePath, Encoding.Default);
                    str = sr.ReadToEnd();
                    sr.Close();
                }
                list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MusicJson>>(str);
                CacheHelper.Insert("Music", list);
            }
            return list.FindAll(x => x.Status == Enum_Status.Approved);
        }

        /// <summary>
        /// 文章类型
        /// </summary>
        protected List<ArticleType> GetArticleType()
        {
            List<ArticleType> list = new List<ArticleType>();
            if (CacheHelper.Exists("ArticleType"))
            {
                list = (List<ArticleType>)CacheHelper.GetCache("ArticleType");
            }
            else
            {
                string str = string.Empty;
                string filePath = System.Web.HttpContext.Current.Server.MapPath("/Config/articletype.config");
                if (System.IO.File.Exists(filePath))
                {
                    StreamReader sr = new StreamReader(filePath, Encoding.Default);
                    str = sr.ReadToEnd();
                    sr.Close();
                }
                list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ArticleType>>(str);
                CacheHelper.Insert("ArticleType", list);
            }
            return list.FindAll(x => x.Status == Enum_Status.Approved);
        }

        /// <summary>
        /// 文章模板
        /// </summary>
        protected List<Template> GetArticleTemp()
        {
            List<Template> list = new List<Template>();
            if (CacheHelper.Exists("Template"))
            {
                list = (List<Template>)CacheHelper.GetCache("Template");
            }
            else
            {
                string str = string.Empty;
                string filePath = System.Web.HttpContext.Current.Server.MapPath("/Config/template.config");
                if (System.IO.File.Exists(filePath))
                {
                    StreamReader sr = new StreamReader(filePath, Encoding.Default);
                    str = sr.ReadToEnd();
                    sr.Close();
                }
                list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Template>>(str);
                var baseurl = System.Configuration.ConfigurationManager.AppSettings["base_url"];
                list.ForEach(x =>
                {
                    x.ThumbUrl = baseurl + x.ThumbUrl;
                    x.Cover = baseurl + x.Cover;
                });
                CacheHelper.Insert("Template", list);
            }
            return list;
        }

        /// <summary>
        /// 敏感词
        /// </summary>
        protected List<DirtyWord> GetDirtyWord()
        {
            List<DirtyWord> list = new List<DirtyWord>();
            if (CacheHelper.Exists("DirtyWord"))
            {
                list = (List<DirtyWord>)CacheHelper.GetCache("DirtyWord");
            }
            else
            {
                string str = string.Empty;
                string filePath = System.Web.HttpContext.Current.Server.MapPath("/Config/dirtyword.config");
                if (System.IO.File.Exists(filePath))
                {
                    StreamReader sr = new StreamReader(filePath, Encoding.Default);
                    str = sr.ReadToEnd();
                    sr.Close();
                }
                list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DirtyWord>>(str);
                CacheHelper.Insert("DirtyWord", list);
            }
            return list;
        }

        /// <summary>
        /// 请求返回结果
        /// </summary>
        public class ResultJson
        {
            public bool State { get; set; }

            public string Message { get; set; }
        }

        /// <summary>
        /// 判断是否包含敏感词
        /// </summary>
        protected bool HasDirtyWord(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }
            var list = GetDirtyWord();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Name.Contains(content))
                {
                    return true;
                }
            }
            return false;
        }

        #region  生成缩略图

        ///<summary>  
        /// 生成缩略图  
        /// </summary>  
        /// <param name="originalImagePath">源图对象</param>  
        /// <param name="mode">生成缩略图的方式</param>
        /// <param name="width">缩略图宽度</param>  
        /// <param name="height">缩略图高度</param> 
        /// <param name="height">是否添加水印（0：不添加,1：添加）</param>  
        /// <param name="height">缩略图保存路径</param> 
        protected void MakeThumbnail(Image originalImage, string mode, int width, int height, int isDraw, string thumbnailPath)
        {
            int towidth = width;
            int toheight = height;
            int x = 0;
            int y = 0;
            int ow = originalImage.Width;//原图宽度
            int oh = originalImage.Height;//原图高度
            switch (mode)
            {
                case "HW"://指定高宽缩放（可能变形）                  
                    break;
                case "W"://指定宽，高按比例                      
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H"://指定高，宽按比例  
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut"://指定高宽裁减（不变形）                  
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }

            Image bitmap = new Bitmap(towidth, toheight);//新建一个bmp图片  
            Graphics g = Graphics.FromImage(bitmap);//新建一个画板  
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;//设置高质量插值法  
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//设置高质量,低速度呈现平滑程度  
            g.Clear(Color.Transparent);//清空画布并以透明背景色填充  
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);//在指定位置并且按指定大小绘制原图片的指定部分  
            try
            {
                ///添加水印
                if (isDraw == 1)
                {
                    bitmap = WaterMark(bitmap);
                }
                //以jpg格式保存缩略图  
                bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                bitmap.Dispose();
                g.Dispose();
            }
        }
        #endregion

        #region 水印

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="bitmap">原始图片</param>
        protected Image WaterMark(Image image)
        {
            ///读取水印配置
            CommonConfig.ConfigItem watermarkmodel = CommonConfig.Instance.GetConfig("WaterMark");

            if (watermarkmodel != null)
            {
                if (watermarkmodel.Cate == 1) //判断水印类型
                {
                    //水印图片
                    Image copyImage = Image.FromFile(System.Web.HttpContext.Current.Server.MapPath("/Image/WaterMark/" + watermarkmodel.ImageUrl));
                    int width = 0;
                    int height = 0;
                    switch (watermarkmodel.Location)
                    {
                        case 1: width = 0; height = 0; break;
                        case 2: width = (image.Width - copyImage.Width) / 2; height = 0; break;
                        case 3: width = image.Width - copyImage.Width; height = 0; break;
                        case 4: width = 0; height = (image.Height - copyImage.Height) / 2; break;
                        case 5: width = (image.Width - copyImage.Width) / 2; height = (image.Height - copyImage.Height) / 2; break;
                        case 6: width = image.Width - copyImage.Width; height = (image.Height - copyImage.Height) / 2; break;
                        case 7: width = 0; height = image.Height - copyImage.Height; break;
                        case 8: width = (image.Width - copyImage.Width) / 2; height = image.Height - copyImage.Height; break;
                        case 9: width = image.Width - copyImage.Width; height = image.Height - copyImage.Height; break;
                    }
                    Graphics g = Graphics.FromImage(image);
                    g.DrawImage(copyImage, new Rectangle(width, height, Convert.ToInt16(watermarkmodel.Width), Convert.ToInt16(watermarkmodel.Height)), 0, 0, copyImage.Width, copyImage.Height, GraphicsUnit.Pixel);
                    g.Dispose();
                    copyImage.Dispose();
                }
                else
                {
                    //文字水印
                    int width = 0;
                    int height = 0;
                    int fontwidth = Convert.ToInt32(watermarkmodel.FontSize * watermarkmodel.Word.Length);
                    int fontheight = Convert.ToInt32(watermarkmodel.FontSize);
                    switch (watermarkmodel.Location)
                    {
                        case 1: width = 0; height = 0; break;
                        case 2: width = (image.Width - fontwidth) / 2; height = 0; break;
                        case 3: width = image.Width - fontwidth; height = 0; break;
                        case 4: width = 0; height = (image.Height - fontheight) / 2; break;
                        case 5: width = (image.Width - fontwidth) / 2; height = (image.Height - fontheight) / 2; break;
                        case 6: width = image.Width - fontwidth; height = (image.Height - fontheight) / 2; break;
                        case 7: width = 0; height = image.Height - fontheight; break;
                        case 8: width = (image.Width - fontwidth) / 2; height = image.Height - fontheight; break;
                        case 9: width = image.Width - fontwidth; height = image.Height - fontheight; break;
                    }
                    Graphics g = Graphics.FromImage(image);
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                    Font f = new Font("Verdana", float.Parse(watermarkmodel.FontSize.ToString()));
                    Brush b = new SolidBrush(Color.White);
                    g.DrawString(watermarkmodel.Word, f, b, width, height);
                    g.Dispose();
                }
            }
            return image;
        }
        #endregion
    }
}
