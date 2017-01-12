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
            InitHelp();
            InitUser();
            InitArticle();
            return Content("成功");
        }

        /// <summary>
        /// 初始化帮助
        /// </summary>
        protected void InitHelp()
        {
            List<HelpType> helpTypes = new List<HelpType>() { };
            helpTypes.Add(new HelpType(1, "使用说明", ""));
            helpTypes.Add(new HelpType(2, "常见使用问题", ""));
            helpTypes.Add(new HelpType(3, "账户问题", ""));
            helpTypes.Add(new HelpType(4, "投稿相关问题", ""));
            helpTypes.Add(new HelpType(5, "音乐与视频相关", ""));
            helpTypes.Add(new HelpType(6, "文章丢失相关问题", ""));
            helpTypes.Add(new HelpType(7, "互动相关问题", ""));
            helpTypes.Add(new HelpType(8, "打赏相关", ""));

            List<Help> helps = new List<Help>() { };
            helps.Add(new Help(21, 2, "如何开始制作Go？", "在主界面点击最下方的加号，即可开始创作。导入图片后，可以任意编辑文字，添加和调整段落。编辑完后可以设置模板，点击“分享”即可分享到朋友圈等平台。"));
            helps.Add(new Help(22, 2, "如何设置文章封面？", "在Go编辑界面，点击封面区域右下角的白色带有“左右箭头”样式的按钮，选择您所需的封面图片。"));
            helps.Add(new Help(23, 2, "如何使用模板？", "在编辑页面点击完成文章后，点击右下角模板字样，可以选择不同模板，模板9个一组，有多组可供选择。"));
            helps.Add(new Help(24, 2, "如何在图片上设置“水印”？", "在Go的“设置-通用设置”里打开“图片水印”，新插入的图片在发布时会自动生成水印。"));
            helps.Add(new Help(25, 2, "如何保存Go文章？", "只要一进发布的Go文章会永久保存在Go云端账户里。Go分享时可以复制链接得到网页地址，网址在浏览器里打开后可以保存电脑里。"));
            helps.Add(new Help(26, 2, "如何让文章带有超链接？", "在段落的文本编辑界面里，可以添加网页链接。设置好链接后可以在完成的文章里使用超链接功能。"));
            helps.Add(new Help(27, 2, "如何收藏喜欢的文章，以及如何取消收藏？", "浏览别人的文章时，点击“分享”按钮，有收藏文章的功能。收藏后在同样操作位置可以取消收藏操作。"));
            helps.Add(new Help(28, 2, "如何分享我的专栏？", "每个美篇用户都拥有自己的专栏，公开发布的文章会出现在专栏里。在美篇“我的里面”右上角为“分享专栏”功能按钮。从美篇编写文章的作者位置可进入作者的专栏。"));
            helps.Add(new Help(29, 2, "阅读权限具体含义？", "美篇文章针对不同用户需求，设定了4种可阅读权限。<br />公开：您的文章会被所有人看见，而且会出现在您的个人专栏里。<br />不公开：不公开的文章不会出现在您的个人专栏里，通过您个人的分享渠道去传播。<br />加密：您可以给文章设置一个密码，分享后必须用密码口令才能查看。<br />私密：文章仅能自己看见，不可分享，公开后的文章可以改为“私密”状态让文章链接失效。<br />这4种文章可以按需要互相转变。"));
            helps.Add(new Help(210, 2, "关于文章上显示的时间？", "美篇文章的时间是以文章初次发布时间自动生成，不可更改。"));
            helps.Add(new Help(211, 2, "美篇占用空间过大的情况？", "美篇为您最大限度的节省了空间，还可以在“设置-通用设置”里清理缓存，释放更多空间。"));
            helps.Add(new Help(212, 2, "美篇文章中出现的广告情况？", "分享出去的热门文章会在屏幕中间适当出现广告，轻轻点击关闭即可。请谅解！<br />若是在屏幕下方有恶俗广告内容的，非美篇所为，是流量被运营商网络所恶意劫持。"));

            helps.Add(new Help(31, 3, "如何获取我的美篇号？", "在美篇“设置-账户”里可以看到美篇号。"));
            helps.Add(new Help(32, 3, "如何找回美篇号？", "如你忘记自己的美篇号，可以从您发布分享的文章里，通过点击文章作者进入专栏查看美篇号。"));
            helps.Add(new Help(33, 3, "为何账户会被查封？", "美篇不允许发布违规内容的文章，情节严重者会被查封美篇账户。<br />关于违规文章，请参看：文章违规内容说明。"));

            helps.Add(new Help(41, 4, "投稿相关问题-上传文章出现“精”是什么意思？", "公开发布的文章可以投稿到美篇，投稿被采用和编辑选中的文章，在作者的个人专栏页面会出现“精”字标识。"));
            helps.Add(new Help(42, 4, "什么样的文章才可以上推荐？", "每天投稿的文章很多，编辑在投稿文章里选择部分内容到美篇的“发现”里。<br />每天上推荐的文章数量有限，如未被选中，不代表您的作品不优秀。"));
            helps.Add(new Help(43, 4, "如何向美篇投稿（投稿方式，投稿间隔时间等）？", "文章的“操作”按钮里有“投稿”功能按钮，每次投稿需间隔7天，请注意投稿时间。"));

            helps.Add(new Help(51, 5, "如何添加背景音乐", "在美篇编辑页面里，点击封面区域左下角的白色“音符”样式的按钮，就可以进入背景音乐选择界面。"));
            helps.Add(new Help(52, 5, "如何加载视频进美篇文章？", "点击段落间的“加号”，第三个按钮就是添加视频按钮。"));
            helps.Add(new Help(53, 5, "本地音乐无法使用？", "美篇目前不支持本地音乐上传服务，今后会去完善此功能。"));
            helps.Add(new Help(54, 5, "音乐如何自动播放？", "在美篇“设置-通用设置”里，可以设置音乐为自动播放。当滑动查找文章时会自动触发音乐播放。"));
            helps.Add(new Help(55, 5, "找不到需要的音乐", "美篇对接的音乐库有海量的内容，但不是每首歌都有，希望有适合您的音乐。"));

            helps.Add(new Help(61, 6, "删除文章需要恢复？", "若您不小心误删了自己文章，请在“求助或建议”里告知我们您所需恢复文章的标题，我们可以试着帮您找回。"));
            helps.Add(new Help(62, 6, "文章被查封了？", "美篇会查封不和规定的文章，若有异议可以向美篇申诉解封文章。<br />需要将文章分享链接发给我们，我们进行复核。"));

            helps.Add(new Help(71, 7, "如何找到其他美篇用户？", "在美篇“发现”里浏览文章时，点击文章的作者信息或者界面右上角的“作者专栏”，可以进入该作者的专栏里去关注他。<br />同时您可以在美篇首页顶部搜索栏，点击搜索用户的昵称或美篇号，关注更多朋友。"));
            helps.Add(new Help(72, 7, "如何点赞和评论？", "美篇文章支持在APP内点赞和评论。同时也支持在微信浏览时点赞。"));
            helps.Add(new Help(73, 7, "如何回答别人的评论和删除恶意评论？", "在评论中点击对方的评论，就可以针对这条评论进行回复了。<br />作者可以删除自己文章评论里他人的恶意评论。"));
            helps.Add(new Help(74, 7, "如何找到更多美篇文章内容？", "除了在美篇首页浏览各个分类里推荐的内容外，您可以在美篇首页顶部搜索栏，输入您想要获取的关键词，发现更多美篇内容。"));

            helps.Add(new Help(81, 8, "文章赞赏功能如何开启？", "在美篇“设置-我的打赏”里开启“启用打赏”，您的所有文章在微信浏览时都会有被赞赏功能。"));
            helps.Add(new Help(82, 8, "赞赏的费用是给谁的？", "赞赏的费用是打给作者的，费用会先保存在美篇里，达到100元后用户可以申请提现。"));
            helps.Add(new Help(83, 8, "收到赞赏后如何提现？", "赞赏金额达到100元就可以申请提现。在您申请提现后，我们会在5个工作日内将款打到您的账户，请务必填写已实名认证的支付宝账户信息。"));
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
