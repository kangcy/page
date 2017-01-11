﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CommonTools;
using EGT_OTA.Helper;
using Newtonsoft.Json;
using EGT_OTA.Helper.Config;
using System.Drawing;

namespace EGT_OTA.Controllers
{
    /// <summary>
    /// 上传文件
    /// </summary>
    public class UploadController : BaseController
    {
        public static string TxtExtensions = ",doc,docx,docm,dotx,txt,xml,htm,html,mhtml,wps,";
        public static string XlsExtensions = ",xls,xlsm,xlsb,xlsm,";
        public static string ImageExtensions = ",jpg,jpeg,jpe,png,gif,bmp,";
        public static string CompressionExtensions = ",zip,rar,";
        public static string AudioExtensions = ",mp3,wav,";
        public static string VideoExtensions = ",mp4,avi,wmv,mkv,3gp,flv,rmvb,";

        public ActionResult UploadFile()
        {
            var result = false;
            var message = string.Empty;
            var count = Request.Files.Count;
            if (count == 0)
            {
                return Json(new { result = result, message = "未上传任何文件" }, JsonRequestBehavior.AllowGet);
            }

            var folder = ZNRequest.GetString("folder");

            var file = Request.Files[0];
            string extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = "Other";
            }
            else
            {
                if (folder.ToLower() == "pic" && !ImageExtensions.Contains(extension.ToLower().Replace(".", "")))
                {
                    return Json(new { result = false, message = "上传文件格式不正确" }, JsonRequestBehavior.AllowGet);
                }
                if (folder.ToLower() == "music" && !AudioExtensions.Contains(extension.ToLower().Replace(".", "")))
                {
                    return Json(new { result = false, message = "上传文件格式不正确" }, JsonRequestBehavior.AllowGet);
                }
                if (folder.ToLower() == "video" && !VideoExtensions.Contains(extension.ToLower().Replace(".", "")))
                {
                    return Json(new { result = false, message = "上传文件格式不正确" }, JsonRequestBehavior.AllowGet);
                }
            }
            var url = string.Empty;
            try
            {
                string data = DateTime.Now.ToString("yyyy-MM-dd");
                string virtualPath = "~/Upload/" + folder + "/" + data;
                string savePath = this.Server.MapPath(virtualPath);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string filename = Path.GetFileName(file.FileName);
                string code = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10000);
                string fileExtension = Path.GetExtension(filename);//获取文件后缀名(.jpg)
                //filename = code + fileExtension;//重命名文件
                var name = ZNRequest.GetString("name");
                if (string.IsNullOrEmpty(name))
                {
                    filename = code + fileExtension;
                }
                else
                {
                    filename = name + fileExtension;
                }
                filename = filename.Replace("3gp", "mp4");
                savePath = savePath + "\\" + filename;
                file.SaveAs(savePath);
                url = "Upload/" + folder + "/" + data + "/" + filename;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("UploadController_UploadFile" + ex.Message, ex);
                message = ex.Message;
            }
            return Json(new { result = true, message = url }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 带缩略图上传
        /// </summary>
        public ActionResult Upload()
        {
            var error = string.Empty;
            try
            {
                string stream = ZNRequest.GetString("str");
                stream = stream.IndexOf("data:image/jpeg;base64,") > -1 ? stream.Replace("data:image/jpeg;base64,", "") : stream;
                System.IO.MemoryStream ms = new System.IO.MemoryStream(Convert.FromBase64String(stream));
                string random = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10000);

                #region  保存缩略图

                string standards = ZNRequest.GetString("standard");///缩略图规格名称
                int isDraw = ZNRequest.GetInt("isDraw");  //是否生成水印
                int isThumb = ZNRequest.GetInt("isThumb"); //是否生成缩略图

                if (isThumb == 1 && !String.IsNullOrEmpty(standards))
                {
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
                }

                #endregion

                //保存原图
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

                return Json(new
                {
                    result = true,
                    message = ("Upload/Images/" + standards + "/" + DateTime.Now.ToString("yyyyMMdd") + "/" + random + "_0" + ".jpg")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                LogHelper.ErrorLoger.Error("UploadController_Upload" + ex.Message, ex);
            }
            return Json(new
            {
                result = false,
                message = error
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
