using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PanGu;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using PanGu.HighLight;
using SubSonic.Repository;
using EGT_OTA.Models;
using CommonTools;

namespace EGT_OTA.Helper.Search
{
    public class MusicSearch : SearchBase
    {
        /// <summary>
        /// 资讯全文检索索引保存的路径
        /// </summary>
        private static DirectoryInfo indexPath = new DirectoryInfo(HttpContext.Current.Server.MapPath("/Index/Music"));

        #region  资讯全文检索

        /// <summary>
        /// 检测索引是否存在
        /// </summary>
        /// <returns></returns>
        public static bool IndexExists()
        {
            return IndexReader.IndexExists(FSDirectory.Open(indexPath));
        }

        #region  重置索引

        /// <summary>
        /// 重新创建所有资讯的索引(可能会花费较长时间)
        /// </summary>
        public static void IndexReset()
        {
            SimpleRepository db = Repository.GetRepo();

            var music01 = db.All<Music01>().ToList();
            var music02 = db.All<Music02>().ToList();
            var music03 = db.All<Music03>().ToList();
            var music04 = db.All<Music04>().ToList();
            var music05 = db.All<Music05>().ToList();
            var music06 = db.All<Music06>().ToList();
            var music07 = db.All<Music07>().ToList();
            var music08 = db.All<Music08>().ToList();
            var music09 = db.All<Music09>().ToList();
            var music10 = db.All<Music10>().ToList();
            var music11 = db.All<Music11>().ToList();
            var music12 = db.All<Music12>().ToList();
            var music13 = db.All<Music13>().ToList();

            var count = 0;
            count += music01.Count;
            count += music02.Count;
            count += music03.Count;
            count += music04.Count;
            count += music05.Count;
            count += music06.Count;
            count += music07.Count;
            count += music08.Count;
            count += music09.Count;
            count += music10.Count;
            count += music11.Count;
            count += music12.Count;
            count += music13.Count;

            if (count == 0)
            {
                Index(new List<Music>(), true);
            }
            else
            {
                var list = new List<Music>();
                list.AddRange(music01);
                list.AddRange(music02);
                list.AddRange(music03);
                list.AddRange(music04);
                list.AddRange(music05);
                list.AddRange(music06);
                list.AddRange(music07);
                list.AddRange(music08);
                list.AddRange(music09);
                list.AddRange(music10);
                list.AddRange(music11);
                list.AddRange(music12);
                list.AddRange(music13);

                Init(list);
            }
        }

        public static void Init(List<Music> list)
        {
            if (list == null)
            {
                return;
            }
            if (list.Count == 0)
            {
                return;
            }

            int recordCount = list.Count;
            int currentPage = 0;
            int pageSize = 50;
            double pageCount = Math.Ceiling(recordCount / (double)pageSize);
            for (int i = 0; i < pageCount; i++)
            {
                list = list.Skip(currentPage * pageSize).Take(pageSize).ToList();
                Index(list, i == 0);
                currentPage++;
            }
        }


        /// <summary>
        /// 将资讯添加到索引中
        /// </summary>
        /// <param name="pList"></param>
        public static void Index(List<Music> pList, bool rewrite)
        {
            Analyzer analyzer = new PanGuAnalyzer();
            IndexWriter writer = new IndexWriter(FSDirectory.Open(indexPath), analyzer, rewrite, IndexWriter.MaxFieldLength.LIMITED);
            //添加到索引中
            foreach (Music p in pList)
            {
                AddDocument(writer, p);
            }
            writer.Commit();
            writer.Optimize();
            writer.Dispose();
        }

        /// <summary>
        /// 将商品添加到索引中
        /// </summary>
        /// <param name="pList"></param>
        public static void Index(List<Music> pList)
        {
            Index(pList, false);
        }

        /// <summary>
        /// 将资讯添加到索引中
        /// </summary>
        /// <param name="product"></param>
        public static void Index(Music Music)
        {
            Index(new List<Music>() { Music }, false);
        }

        /// <summary>
        /// 将资讯添加到索引中
        /// </summary>
        /// <param name="product"></param>
        public static void Index(Music Music, bool rewrite)
        {
            Index(new List<Music>() { Music }, rewrite);
        }

        /// <summary>
        /// 将资讯添加到索引器中
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static int AddDocument(IndexWriter writer, Music p)
        {
            Document doc = new Document();
            doc.Add(new Field("ID", p.ID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Number", p.Number, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Author", p.Author, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Name", p.Name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("FileUrl", p.FileUrl, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Cover", p.Cover, Field.Store.YES, Field.Index.NOT_ANALYZED));
            writer.AddDocument(doc);
            int num = writer.MaxDoc();
            return num;
        }

        #endregion

        #region 从索引中删除项目

        /// <summary>
        /// 从资讯索引中将索引删除
        /// </summary>
        /// <param name="IDList">需要删除索引的资讯的ID列表（以 , 隔开）</param>
        public static void MusicDelete(string IDList)
        {
            string[] arrMusicID = IDList.Split(',');
            IndexReader reader = IndexReader.Open(FSDirectory.Open(indexPath), false);
            for (int i = 0, len = arrMusicID.Length; i < len; i++)
            {
                Term term = new Term("ID", arrMusicID[i]);
                reader.DeleteDocuments(term);
            }
            reader.Commit();
            reader.Dispose();
        }

        /// <summary>
        /// 从资讯索引中将索引删除
        /// </summary>
        public static void MusicDelete(int id)
        {
            MusicDelete(id.ToString());
        }

        /// <summary>
        /// 从资讯索引中将索引删除
        /// </summary>
        public static void MusicDelete(Music model)
        {
            MusicDelete(model.ID.ToString());
        }

        /// <summary>
        /// 从资讯索引中将索引删除
        /// </summary>
        public static void MusicDelete(List<Music> pList)
        {
            string IDList = "";
            foreach (Music p in pList)
            {
                IDList += "," + p.ID;
            }
            if (IDList.Length > 0)
            {
                MusicDelete(IDList.Substring(1));
            }
        }

        #endregion

        /// <summary>
        /// 资讯查询
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <returns>符合查询条件的资讯列表</returns>
        public static List<Music> Search(string keyword, Sort sort, int currentPage, int pageSize, out int recordCount)
        {
            if (!IndexExists())
            {
                IndexReset();
            }
            if (sort == null)
            {
                sort = new Sort(new SortField("Number", 0));
            }
            Analyzer analyzer = new PanGuAnalyzer();
            BooleanQuery totalQuery = new BooleanQuery();
            MultiFieldQueryParser parser = new MultiFieldQueryParser(VERSION, new string[] { "Author", "Name" }, analyzer);
            if (!String.IsNullOrEmpty(keyword))
            {
                keyword = ClearKeyword(keyword);
                Lucene.Net.Search.Query query = parser.Parse(SearchBase.AnalysisKeyword(keyword));
                totalQuery.Add(query, Occur.MUST);
            }
            if (currentPage < 0)
            {
                currentPage = 0;
            }
            int beginIndex = currentPage * pageSize;
            int lastIndex = (currentPage + 1) * pageSize;

            IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(indexPath), true);
            TopDocs result = searcher.Search(totalQuery, null, lastIndex, sort);
            ScoreDoc[] hits = result.ScoreDocs;
            recordCount = result.TotalHits;
            if (recordCount < lastIndex)
            {
                lastIndex = recordCount;
            }
            List<Music> pList = new List<Music>();
            Music p = null;

            for (int i = beginIndex; i < lastIndex; i++)
            {
                var doc = searcher.Doc(hits[i].Doc);
                p = new Music();
                p.ID = Tools.SafeInt(doc.Get("ID"));
                p.Author = doc.Get("Author");
                p.Name = doc.Get("Name");
                p.FileUrl = doc.Get("FileUrl");
                p.Cover = doc.Get("Cover");
                ////高亮显示
                //SimpleHTMLFormatter simpleHTMLFormatter = new SimpleHTMLFormatter("<font color=\"red\">", "</font>");
                //Highlighter highlighter = new Highlighter(simpleHTMLFormatter, new Segment());
                //highlighter.FragmentSize = 50;
                //p.NameHL = highlighter.GetBestFragment(keyword, p.Name);
                //if (string.IsNullOrEmpty(p.NameHL))
                //{
                //    p.NameHL = doc.Get("Name");
                //}
                pList.Add(p);
            }
            searcher.Dispose();
            return pList;
        }

        #endregion
    }
}