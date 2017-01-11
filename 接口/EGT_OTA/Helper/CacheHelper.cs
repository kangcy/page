﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Collections;

namespace EGT_OTA.Helper
{
    public class CacheHelper
    {
        /// <summary>
        /// 获取数据缓存
        /// </summary>
        /// <param name="CacheKey">键</param>
        public static object GetCache(string CacheKey)
        {
            return HttpRuntime.Cache[CacheKey];
        }
        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void Insert(string CacheKey, object objObject)
        {
            HttpRuntime.Cache.Insert(CacheKey, objObject);
        }
        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void Insert(string CacheKey, object objObject, TimeSpan Timeout)
        {
            HttpRuntime.Cache.Insert(CacheKey, objObject, null, DateTime.MaxValue, Timeout, CacheItemPriority.NotRemovable, null);
        }
        /// <summary>
        /// 设置数据缓存
        /// </summary>
        public static void Insert(string CacheKey, object objObject, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            HttpRuntime.Cache.Insert(CacheKey, objObject, null, absoluteExpiration, slidingExpiration);
        }
        /// <summary>
        /// 是否存在指定数据缓存
        /// </summary>
        public static bool Exists(string CacheKey)
        {
            return HttpRuntime.Cache[CacheKey] == null ? false : true;
        }
        /// <summary>
        /// 移除指定数据缓存
        /// </summary>
        public static void Remove(string CacheKey)
        {
            HttpRuntime.Cache.Remove(CacheKey);
        }
        /// <summary>
        /// 移除全部缓存
        /// </summary>
        public static void RemoveAll()
        {
            Cache _cache = HttpRuntime.Cache;
            IDictionaryEnumerator CacheEnum = _cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                _cache.Remove(CacheEnum.Key.ToString());
            }
        }
    }
}
