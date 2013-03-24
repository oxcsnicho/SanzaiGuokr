﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq;
using SanzaiGuokr.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;
using Microsoft.Phone.Data.Linq;
using Microsoft.Phone.Data.Linq.Mapping;
using GalaSoft.MvvmLight.Command;
using SanzaiGuokr.GuokrObject;

namespace SanzaiGuokr.ViewModel
{
    [Table]
    public class ArticleBookmark
    {
        [Column(IsPrimaryKey = true)]
        public Int64 id { get; set; }
        [Column]
        public string title { get; set; }
        [Column]
        public string Abstract { get; set; }
        [Column]
        public string url { get; set; }
        [Column]
        public string pic { get; set; }
        [Column]
        public string minisite_name { get; set; }
        [Column]
        public string datetime { get; set; }
        [Column]
        public string author { get; set; }
        public static implicit operator article(ArticleBookmark b)
        {
            var a = new article();
            a.id = b.id;
            a.title = b.title;
            a.Abstract = b.Abstract;
            a.url = b.url;
            a.pic = b.pic;
            a.minisite_name = b.minisite_name;
            return a;
        }
        public static implicit operator ArticleBookmark(article b)
        {
            var a = new ArticleBookmark();
            a.id = b.id;
            a.title = b.title;
            a.Abstract = b.Abstract;
            a.url = b.url;
            a.pic = b.pic;
            a.minisite_name = b.minisite_name;
            return a;
        }
    }
    public class BookmarkDataContext : DataContext
    {
        // Specify the connection string as a static, used in main page and app.xaml.
        public const string DBConnectionString = "Data Source=" + DBPath;
        public const string DBPath = "isostore:/Bookmarks.sdf";

        // Pass the connection string to the base class.
        public BookmarkDataContext(string connectionString = DBConnectionString)
            : base(connectionString)
        {
            if (DatabaseExists() == false)
                CreateDatabase();
#if DEBUG
            else
                DeleteDatabase();
#endif
            BookmarkItems = GetTable<ArticleBookmark>();
        }

        // Specify a single table for the to-do items.
        public Table<ArticleBookmark> BookmarkItems;


        internal void InsertBookmarkIfNotExist(article a)
        {
            if (!IsArticleExist(a.id))
                BookmarkItems.InsertOnSubmit(a);
        }

        private bool IsArticleExist(long p)
        {
            return GetItem(p) != null;
        }

        internal void RemoveBookmarkIfExist(Int64 id)
        {
            var a = GetItem(id);
            if (a != null)
                BookmarkItems.DeleteOnSubmit(a);
        }

        internal article GetItem(Int64 id)
        {
            var result = from ArticleBookmark a in BookmarkItems
                         where a.id == id
                         select a;
            if (result.Count() > 0)
                return result.ToList()[0];
            else
                return null;
        }

        internal List<article> ReturnAllBookmarks()
        {
            var items = from ArticleBookmark a in this.BookmarkItems
                        select (article)a;
            return items.ToList();
        }

        public async void SubmitChanges()
        {
            SubmitChanges(ConflictMode.ContinueOnConflict);
        }

        private static BookmarkDataContext _bm = null;
        public static BookmarkDataContext Current
        {
            get
            {
                if (_bm == null)
                {
                    _bm = new BookmarkDataContext();
                }
                return _bm;
            }
        }

    }
}
