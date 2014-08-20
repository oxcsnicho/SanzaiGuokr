using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SanzaiGuokr.Model;
using WeiboApi;
using SanzaiGuokr.GuokrObject;
using GalaSoft.MvvmLight.Command;
using System;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;

namespace SanzaiGuokr.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private article_list _la = null;
        public article_list latest_article_list
        {
            get
            {
                if (_la == null)
                    _la = new article_list() { Name = "最新文章" };
                return _la;
            }
        }
        public ObservableCollection<article> latest_articles
        {
            get
            {
                return latest_article_list.ArticleList;
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                var latest_articles = latest_article_list.ArticleList;
                // Code runs in Blend --> create design time data.
                #region articles
                latest_articles.Add(new article()
                {
                    minisite_name = "科技视点",
                    m_url = "/api/m/article/?article_id=95703&show_type=latest&which_one=this",
                    title = "被电视节目娱乐后，科学还剩下什么？",
                    Abstract = "科学作者马丁 · 罗宾斯指出，英国广播电视台（BBC）正在把科学简单化、碎片化；科学被当作科教或娱乐的把戏，跟时事和政治隔绝开来。罗宾斯表示，BBC 不应该弱智化自己的观众，它有义务、有能力为公众呈现完整真实的科学。",
                    pic = "http://img1.guokr.com/gkimage/1a/11/en/1a11en.png",
                    id = 95703
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "微科幻",
                    m_url = "/api/m/article/?article_id=95826&show_type=latest&which_one=this",
                    title = "末日十日谈：以美好之名",
                    Abstract = "“末日十日谈”小说系列之（五）。当一座城堡成为末日中的天堂，罪恶便借美好之名行走于世。",
                    pic = "http://img1.guokr.com/gkimage/9y/bk/y2/9ybky2.png",
                    id = 95826
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "健康朝九晚五",
                    m_url = "/api/m/article/?article_id=95660&show_type=latest&which_one=this",
                    title = "母乳：妈妈能给宝宝的最完美食物",
                    Abstract = "母乳是婴幼儿最完美的食物。母乳喂养，不仅是妈妈与宝宝最亲密的交流，还会带给宝宝一生的健康基础，让宝宝更聪明。我们应该维护并宣传母乳喂养的种种美好，而不是打击并妄图用各种产品替代它。",
                    pic = "http://img1.guokr.com/gkimage/ya/e6/00/yae600.png",
                    id = 95660
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "微科幻",
                    m_url = "/api/m/article/?article_id=95542&show_type=latest&which_one=this",
                    title = "末日十日谈：旁观者",
                    Abstract = "“末日十日谈”小说系列之（四）。1000种毁灭世界的方法之外总会有第1001种更离奇的。有时你旁观着世界被毁灭，转眼就轮到了自己。有时你行走在生活之中，转眼就成了旁观者。世界末日来时，你在世界的哪一处？",
                    pic = "http://img1.guokr.com/gkimage/6v/u9/qe/6vu9qe.png",
                    id = 95542
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "创意科技",
                    m_url = "/api/m/article/?article_id=95536&show_type=latest&which_one=this",
                    title = "iPad 3将有哪些改进?",
                    Abstract = "iPad 3平板电脑下个月初就将面世，我们越来越关心iPad 3的配置会有何不同。现在就让我们来看看gizmodo网站的一篇预测，看看iPad 3可能会有哪些方面的改进。",
                    pic = "http://img1.guokr.com/gkimage/dt/qm/7u/dtqm7u.png",
                    id = 95536
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "环球科技观光团",
                    m_url = "/api/m/article/?article_id=95487&show_type=latest&which_one=this",
                    title = "新艾滋病疫苗进行人体试验",
                    Abstract = "有关艾滋病病毒疫苗的尝试往往都以失败告终。近日，加拿大研究学家表示，他们正用一种与众不同的方式来研制疫苗。美国食品和药物管理局（FDA）已经准许研究组对人类进行实验性疫苗测试。",
                    pic = "http://img1.guokr.com/gkimage/ey/qe/5m/eyqe5m.png",
                    id = 95487
                });
                #endregion

                #region minisites
                var minisite_articles = ViewModelLocator.ChannelStatic.the_channel.MinisiteArticles.ArticleList;
                minisite_articles.Add(new article()
                {
                    minisite_name = "健康朝九晚五",
                    m_url = "/api/m/article/?article_id=95354&show_type=latest&which_one=this",
                    title = "酱油是怎么打出来的？",
                    Abstract = "酱油的颜色是哪来的？老抽和生抽有什么区别？酿造酱油和配制酱油的异同何在？头发丝为什么也能被不法商贩拿来做“酱油”？打了这么多年酱油，突然发现关于酱油的疑问还真不少……别着急，搞清楚酱油是怎么打出来的，这些问题就迎刃而解了。",
                    pic = "http://img1.guokr.com/gkimage/7a/j2/5p/7aj25p.png",
                    id = 95354
                });
                minisite_articles.Add(new article()
                {
                    minisite_name = "心事鉴定组",
                    m_url = "/api/m/article/?article_id=94788&show_type=latest&which_one=this",
                    title = "模仿，令我们更亲近 ",
                    Abstract = "与人交流时，你是否会不由自主地去模仿对方说话的语速、语调，或是表情与肢体动作？心理学家发现，这样的模仿可能是无意识的，却能使彼此更加亲近。\r\n",
                    pic = "http://img1.guokr.com/gkimage/kz/ai/ij/kzaiij.png",
                    id = 94788
                });
                minisite_articles.Add(new article()
                {
                    minisite_name = "性 情",
                    m_url = "/api/m/article/?article_id=95311&show_type=latest&which_one=this",
                    title = "避孕药，你所不知道的7件事",
                    Abstract = "众所周知，避孕药在控制人口方面起了巨大作用。不过你知道吗？最早的避孕药，是用来治疗严重月经问题的。避孕药还可能会影响女性的择偶标准。早期避孕药价格昂贵，而解决成本问题的东西居然是一种山药。",
                    pic = "http://img1.guokr.com/gkimage/rt/gm/g3/rtgmg3.png",
                    id = 95311
                });

                minisite_articles.Add(new article()
                {
                    minisite_name = "微科幻",
                    m_url = "/api/m/article/?article_id=95230&show_type=latest&which_one=this",
                    title = "末日十日谈：新年灵魂趴",
                    Abstract = "“末日十日谈”小说系列之（三）。这是一个信息爆炸的时代，媒体工作者的潜能已经被逼到极限，外部的人工智能被用于协助信息处理。然而这玩命的竞争把绝望者逼上了绝路。",
                    pic = "http://img1.guokr.com/gkimage/fa/gz/0x/fagz0x.png",
                    id = 95230
                });
                minisite_articles.Add(new article()
                {
                    minisite_name = "死理性派",
                    m_url = "/api/m/article/?article_id=95226&show_type=latest&which_one=this",
                    title = "用统计方法看纳税人的钱哪去了 ",
                    Abstract = "纳税人的哪去了？这无疑是每个人都关心的问题，前段时间有人做了个视频给出了详细解答。本着死理性的精神，我们从统计学的角度做了一番考证，来看看吧。",
                    pic = "http://img1.guokr.com/gkimage/wt/e1/ec/wte1ec.png",
                    id = 95226
                });
                minisite_articles.Add(new article()
                {
                    minisite_name = "环球科技观光团",
                    m_url = "/api/m/article/?article_id=95136&show_type=latest&which_one=this",
                    title = "早餐吃甜点也能减肥？ ",
                    Abstract = "要减肥就不能吃甜点了吗？以色列特拉维夫大学研究人员表示，丰盛早餐配以甜点有助于成功减去更多的体重。",
                    pic = "http://img1.guokr.com/gkimage/xl/n8/7c/xln87c.png",
                    id = 95136
                });
                #endregion

                #region Posts
                latest_posts.Add(new GuokrPost()
                {
                    title = "有没有姐妹跟我一样屁股大一次都用两片卫生巾的",
                    reply_count = 52,
                    group = new GuokrGroup() { name = "性 情" },
                    posted_by = new GuokrUser() { nickname = "owlcity小喵" },
                    HtmlContent = @"<div id=""articleContent"" class=""post-detail"">
            就是一段时间都是和同一个（或者同几个？重口了。。）<br>
纯好奇~对逛果壳的孩纸们的纯好奇~<br>
一天总要拿出点时间把节操关在抽屉里然后来逛果壳。。。
            

        </div>",
                    replied_dt = "2012-12-12 08:44:07"
                });
                latest_posts.Add(new GuokrPost()
                {
                    title = "【调查向】你是从哪儿知道玛雅人的？",
                    reply_count = 75,
                    group = new GuokrGroup() { name = "Geek笑点低" },
                    posted_by = new GuokrUser() { nickname = "Big.D" },
                    replied_dt = "2012-12-12 08:41:59"
                });
                latest_posts.Add(new GuokrPost()
                {
                    title = "大家做运动的时候都遇到过哪些扫兴的事情？",
                    reply_count = 57,
                    group = new GuokrGroup() { name = "性 情" },
                    posted_by = new GuokrUser() { nickname = "Hoverwei" },
                    replied_dt = " 2012-12-12 08:37:14"
                });
                latest_posts.Add(new GuokrPost()
                {
                    title = "如果我说打算在飞机上来场 fast sex……会太疯狂麽",
                    reply_count = 147,
                    group = new GuokrGroup() { name = "性 情" },
                    posted_by = new GuokrUser() { nickname = "yepyeper" },
                    replied_dt = "2012-12-12 08:36:53"
                });
                latest_posts.Add(new GuokrPost()
                {
                    title = "呐，逆膝枕大家觉得如何？",
                    reply_count = 52,
                    group = new GuokrGroup() { name = "Geek笑点低" },
                    posted_by = new GuokrUser() { nickname = "infinte" },
                    replied_dt = "2012-12-12 08:36:22"
                });
                #endregion

                #region notices
                NoticeList.ArticleList.Add(new GuokrApiV2.GuokrNotice()
                {
                    content = "希小澈在回复《男友最近迷上了口交，肿么破》时中提到了你",
                    is_read = false,
                    date_last_updated = "1362899808",
                    url = "http://www.guokr.com/post/reply/2794190/",
                    id = 263921
                });
                #endregion

                #region recommended
                RecommendedList.ArticleList.Add(new recommend_article()
                {
                    minisite_name = "学习之道",
                    title = "在Coursera，随时都是学习的好时候",
                    pic = "http://img1.guokr.com/image/PMRWCGl7G-BOWwpIE3rqGfIvwkQzM2cFbja_7EIOWfkEAQAAxAAAAEpQ.jpg",
                    Abstract = "什么时候才适合学习？本文作者，微软亚洲研究院院长张峥表示，“生活处处是考场，人生无时不学习”，幸好我们有了Coursera，可以“向名师要知识，向碎片要时间”。",
                    wwwurl = "http://www.guokr.com/article/436817/",
                    ImgSrc = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("http://img1.guokr.com/thumbnail/PMRWCGl7G-BOWwpIE3rqGfIvwkQzM2cFbja_7EIOWfkEAQAAxAAAAEpQ_130x100.jpg", System.UriKind.Absolute))
                });
                RecommendedList.ArticleList.Add(new recommend_article()
                {
                    minisite_name = "科技与商业",
                    title = "触动按钮的交易员",
                    pic = "http://img1.guokr.com/image/jXseJ0PqDG3m6mJ_BCAw3URVPlWjO5XuZZ1cH2nZ6psBAgAAUgEAAFBO.png",
                    Abstract = "核大国元首都有手中有个“核按钮”。在金融交易系统中，交易员的每一次按下回车，背后触发的庞大系统比起核按钮一点也不逊色。你愿意将你的大笔资金，通过一个按键，交付给一台远在千里之外的大型计算机么？",
                    wwwurl = "http://www.guokr.com/article/436817/",
                    ImgSrc = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("http://img1.guokr.com/thumbnail/jXseJ0PqDG3m6mJ_BCAw3URVPlWjO5XuZZ1cH2nZ6psBAgAAUgEAAFBO_130x100.png", System.UriKind.Absolute))
                });
                RecommendedList.ArticleList.Add(new recommend_article()
                {
                    minisite_name = "学习之道",
                    title = "在Coursera，随时都是学习的好时候",
                    pic = "http://img1.guokr.com/image/PMRWCGl7G-BOWwpIE3rqGfIvwkQzM2cFbja_7EIOWfkEAQAAxAAAAEpQ.jpg",
                    Abstract = "什么时候才适合学习？本文作者，微软亚洲研究院院长张峥表示，“生活处处是考场，人生无时不学习”，幸好我们有了Coursera，可以“向名师要知识，向碎片要时间”。",
                    wwwurl = "http://www.guokr.com/article/436817/",
                    ImgSrc = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("http://img1.guokr.com/thumbnail/PMRWCGl7G-BOWwpIE3rqGfIvwkQzM2cFbja_7EIOWfkEAQAAxAAAAEpQ_130x100.jpg", System.UriKind.Absolute))
                });
                #endregion
            }
            else
            {
                Messenger.Default.Register<GoToReadArticleComment>(this, (a) => _GoToReadArticleComment(a));
#if WP8
                Task.Run(async () =>
#else
                TaskEx.Run(async () =>
#endif

                    {
                        await RecommendedList.load_more();
                        await latest_article_list.load_more();
                        if (ViewModelLocator.ApplicationSettingsStatic.IsGroupEnabledSettingBool)
                        {
                            try
                            {
#if WP8
                                await Task.Run(() => GuokrApi.GetRNNumber());
#else
                                await TaskEx.Run(() => GuokrApi.GetRNNumber());
#endif
                                await latest_post_list.load_more();
                            }
                            catch (GuokrException ex)
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(()=>
                                MessageBox.Show(
                                    ex.error_code == GuokrErrorCodeV2.IllegalAccessToken ?
                                            "果壳登录过期啦，请重新登录一下" :
                                            ex.error
                                    ));
                            }
                        }
                        //await MrGuokrWeiboList.load_more();
                    });
            }
        }

        private NavigationService _ns = null;
        public void SetNavigationService(NavigationService ns)
        {
            if (_ns == null)
                _ns = ns;
        }
        private void _GoToReadArticleComment(GoToReadArticleComment a)
        {
            _ns.Navigate(new Uri(string.Format("/ViewComments.xaml?article_id={0}&article_type={1}", a.article.id, a.article.object_name), UriKind.Relative));
        }

        private channels_list _chs;

        public channels_list Channels
        {
            get
            {
                if (_chs == null)
                    _chs = new channels_list();
                return _chs;
            }
            set { _chs = value; }
        }

        private weibo_list _wblist;

        public weibo_list MrGuokrWeiboList
        {
            get
            {
                if (_wblist == null)
                    _wblist = new weibo_list();
                return _wblist;
            }
            set { _wblist = value; }
        }


        private status _wb;

        public status the_weibo
        {
            get { return _wb; }
            set { _wb = value; }
        }

        private GuokrPost_list2 _lp = null;
        public GuokrPost_list2 latest_post_list
        {
            get
            {
                if (_lp == null)
                    _lp = new GuokrPost_list2();
                return _lp;
            }
        }
        public ObservableCollection<GuokrPost> latest_posts
        {
            get
            {
                return latest_post_list.ArticleList;
            }
        }

        private bool _pu;
        private string ImagePopupOpenedPropertyName = "ImagePopupOpened";
        public bool ImagePopupOpened
        {
            get { return _pu; }
            set
            {
                _pu = value;
                RaisePropertyChanged(ImagePopupOpenedPropertyName);
            }
        }

        private notice_list _nl;

        public notice_list NoticeList
        {
            get
            {
                if (null == _nl)
                    _nl = new notice_list();
                return _nl;
            }
            set { _nl = value; }
        }

        private recommended_list _rl;

        public recommended_list RecommendedList
        {
            get
            {
                if (null == _rl)
                    _rl = new recommended_list();
                return _rl;
            }
            set { _rl = value; }
        }
        public ObservableCollection<recommend_article> RecommendedArticles
        {
            get
            {
                return RecommendedList.ArticleList;
            }
        }

        private RelayCommand _rg = null;
        public RelayCommand RandomGate
        {
            get
            {
                if (_rg == null)
                {
                    _rg = new RelayCommand(async () =>
                {
#if WP8
                    Task.Run(async () =>
#else
                    TaskEx.Run(async () =>
#endif

                            {
                                Random rnd = new Random();
                                IsLoading = true;
                                var a = await GuokrApi.GetLatestArticlesV2(1, rnd.Next(ViewModelLocator.ApplicationSettingsStatic.MaxArticleNumber));
                                IsLoading = false;
                                if (a != null && a.Count > 0)
                                {
                                    a[0].ReadNextArticle = RandomGate;
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                                    Messenger.Default.Send<GoToReadArticle>(new GoToReadArticle() { article = a[0] })
                                                    );
                                    GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RelayCommand", "RandomGate", "RandomGate", 1);
                                }
                            });
                });
                }
                return _rg;
            }
        }

        private RelayCommand _resetRN;
        public RelayCommand ResetRNNumber
        {
            get
            {
                if (_resetRN == null)
                {
                    _resetRN = new RelayCommand(async () =>
                    {
#if WP8
                        Task.Run(async () =>
#else
                        TaskEx.Run(async () =>
#endif
                        {
                            try
                            {
                                await GuokrApi.ResetRNNumber();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show("刷新失败，请稍候再试。\n" + e.Message);
                            }
                        });
                    });
                }
                return _resetRN;
            }
        }

        const string GuokrRnNumberPropertyName = "GuokrRnNumber";
        private GuokrApiV2.GuokrRnNum _rnnum = new GuokrApiV2.GuokrRnNum() { r = 0, n = 0 };
        public GuokrApiV2.GuokrRnNum GuokrRnNumber
        {
            get
            {
                return _rnnum;
            }
            set
            {
                if (value.TotalValue == _rnnum.TotalValue)
                    return;
                _rnnum.r = value.r;
                _rnnum.n = value.n;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    RaisePropertyChanged(GuokrRnNumberPropertyName);
                    RaisePropertyChanged(GuokrRnStringPropertyName);
                });
            }
        }
        const string GuokrRnStringPropertyName = "GuokrRnString";
        public string GuokrRnString
        {
            get
            {
#if false
                if (_rnnum.SumValue <= 1)
                    return "有消息~";
                else
#endif
                return "(" + _rnnum.SumValue.ToString() + ")";
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

        #region loading indicator
        private bool IsLoadingStatic = false;
        const string IsLoadingPropertyName = "IsLoading";
        public bool IsLoading
        {
            get
            {
                return IsLoadingStatic;
            }
            private set
            {
                if (value == IsLoadingStatic)
                    return;
                IsLoadingStatic = value;
                Deployment.Current.Dispatcher.BeginInvoke(() => RaisePropertyChanged(IsLoadingPropertyName));
            }
        }
        #endregion
    }
}