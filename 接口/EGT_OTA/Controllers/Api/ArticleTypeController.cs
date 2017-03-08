using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using EGT_OTA.Controllers.Filter;
using EGT_OTA.Helper;
using Newtonsoft.Json;

namespace EGT_OTA.Controllers.Api
{
    public class ArticleTypeController : BaseApiController
    {
        [DeflateCompression]
        [HttpGet]
        [Route("api/type/all")]
        public string Get()
        {
            try
            {
                var list = GetArticleType().OrderBy(x => x.SortID).ToList();
                var newlist = (from l in list
                               select new
                               {
                                   ID = l.ID,
                                   Cover = GetFullUrl(l.Cover),
                                   Name = l.Name,
                                   Summary = l.Summary,
                                   ParentID = l.ParentID,
                                   ParentIDList = l.ParentIDList
                               }).ToList();
                var result = new
                {
                    currpage = 1,
                    records = list.Count(),
                    totalpage = 1,
                    list = newlist
                };
                return JsonConvert.SerializeObject(result);
                //return new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.GetEncoding("UTF-8"), "application/json") };
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLoger.Error("ArticleTypeController_All:" + ex.Message);
                return JsonConvert.SerializeObject(ex.Message);
                //return new HttpResponseMessage { Content = new StringContent(null, Encoding.GetEncoding("UTF-8"), "application/json") };
            }
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}