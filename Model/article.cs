﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RestSharp;
using SanzaiGuokr.GuokrObject;
using SanzaiGuokr.Messages;
using SanzaiGuokr.Util;
using SanzaiGuokr.ViewModel;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Net;
using System.Data.Linq.Mapping;

namespace SanzaiGuokr.Model
{

    public class article : article_base<article>
    {
        #region abstract
        private string _abs;
        public string Abstract
        {
            get
            {
                return _abs;
            }
            set
            {
                _abs = value.TrimEnd(new char[] { '\n', ' ', '\t', '\r' });
            }
        }
        public string ShortAbstract
        {
            get
            {
                if (title.Length < 15)
                    if (Abstract.Length > 25)
                        return Abstract.Substring(0, 25) + "...";
                    else
                        return Abstract;
                else
                    if (Abstract.Length > 10)
                        return Abstract.Substring(0, 10) + "...";
                    else
                        return Abstract;
            }
        }
        #endregion
        public string minisite_name { get; set; }
        public override string parent_name
        {
            get
            {
                return minisite_name;
            }
        }
        public string pic { get; set; }
        public string small_pic
        {
            get
            {
                if (!string.IsNullOrEmpty(pic) && pic.Contains("/image/"))
                {
                    var s = pic.Replace("/image/", "/thumbnail/");
                    s = s.Insert(s.Length - 4, "_140x173");
                    return s;
                }
                else
                    return pic;
            }
        }
        public Uri small_pic_url
        {
            get
            {
                if (!string.IsNullOrEmpty(small_pic))
                    return new Uri(small_pic, UriKind.Absolute);
                return null;
            }
        }
        private BitmapImage _imgsrc;
        public BitmapImage ImgSrc
        {
            get
            {
                if (_imgsrc == null)
                {
                    _imgsrc = new BitmapImage();
                    _imgsrc.CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation;
#if false
                    _imgsrc.UriSource = HeadUri;
#endif
                    WebClient wc = new WebClient();
                    wc.Headers["Referer"] = "http://www.guokr.com";
                    wc.OpenReadCompleted += (s, e) =>
                    {
                        try
                        {
                            _imgsrc.SetSource(e.Result);
                            RaisePropertyChanged("ImgSrc");
                        }
                        catch
                        {

                        }
                    };
                    wc.OpenReadAsync(small_pic_url);
                }
                return _imgsrc;
            }
            set
            {
                _imgsrc = value;
                RaisePropertyChanged("ImgSrc");
            }
        }
        protected override void _readArticle(article_base a)
        {
            if (a.GetType() == typeof(article))
                Messenger.Default.Send<GoToReadArticle>(new GoToReadArticle() { article = (article)a });
        }

        #region new article content
        protected override async Task _loadArticle()
        {
            try
            {
                HtmlContent = await GuokrApi.GetArticleV2(this);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region sha
        private RelayCommand _sha;
        public RelayCommand ShaCommand
        {
            get
            {
                if (_sha == null)
                {
                    _sha = new RelayCommand(() =>
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(async () =>
                                {
                                    var t = MessageBox.Show("准备秒杀之，确定？\n\n ψ(╰_╯)σ‧‧☆杀!", "确定秒杀", MessageBoxButton.OKCancel);
                                    if (t == MessageBoxResult.OK || t == MessageBoxResult.Yes)
                                    {
                                        try
                                        {
                                            await GuokrApi.PostCommentV2(this, "杀！");
                                            await GuokrApi.GetArticleInfo(this);
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("杀败了。。momo。。（你没登录吧！还是我出八阿哥了！）");
                                        }
                                        if (this.CommentCount <= 1)
                                            MessageBox.Show("杀掉了～");
                                        else
                                            MessageBox.Show("杀败了。。momo。。手速不够快啊\n（删帖去吧～长按帖子可以删除哦）\n\n ╮(╯▽╰)╭");
                                    }
                                });
                        });
                }
                return _sha;
            }
        }
        bool canExecuteSha()
        {
            return CommentCount == 0;
        }
        #endregion

        #region reply count
        protected override async void PostLoadArticle()
        {
#if false
            string search_string = @"reply_count=(\d+)";
            var res = Regex.Match(HtmlContent, search_string).Groups;
            if (res.Count >= 0)
                CommentCount = Convert.ToInt32(res[1].Value);
#endif
        }
        #endregion
    }

}
