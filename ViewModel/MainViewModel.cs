using GalaSoft.MvvmLight;
using SanzaiGuokr.Model;
using GalaSoft.MvvmLight.Command;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;
using WeiboApi;

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
                latest_articles.Add(new article()
                {
                    minisite_name = "健康朝九晚五",
                    m_url = "/api/m/article/?article_id=95421&show_type=latest&which_one=this",
                    title = "关于酒精的20个趣闻",
                    Abstract = "有这么一种物质，你能在自己的肌肉中找到它，能在肠道里制造它，甚至能在太空中发现它。这种物质是什么？答案就是酒啦！",
                    pic = "http://img1.guokr.com/gkimage/78/t0/4c/78t04c.png",
                    id = 95421
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "性 情",
                    m_url = "/api/m/article/?article_id=95489&show_type=latest&which_one=this",
                    title = "你的精液合格吗？",
                    Abstract = "我们知道，体检的时候会验血验尿验大便，不过还有一种东西的检查，却通常不包括在一般体检中，那就是验精。",
                    pic = "http://img1.guokr.com/gkimage/jt/03/sr/jt03sr.png",
                    id = 95489
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "谋杀 现场 法医",
                    m_url = "/api/m/article/?article_id=95492&show_type=latest&which_one=this",
                    title = "FBI怎样对乔布斯进行背景调查？",
                    Abstract = "美国联邦调查局（FBI）公布了一份1991年对乔布斯的背景调查报告。报告中，乔布斯的同事称他“品德有问题”，并且“不惜通过歪曲事实来达到自己的目的”。FBI是怎样进行神秘的“背景调查”的？让果壳谋杀站带你解读这191页的调查报告吧。",
                    pic = "http://img1.guokr.com/gkimage/st/go/ll/stgoll.png",
                    id = 95492
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "创意科技",
                    m_url = "/api/m/article/?article_id=95495&show_type=latest&which_one=this",
                    title = "最好的淋浴系统是什么样的？",
                    Abstract = "德国当代公司（Dornbracht）最新推出了一款垂直型淋浴系统，外形像嵌入浴室墙壁中的日光浴床，各种淋浴方式和水温的控制可与整体浴室的灯光、香味等协调一致，这是不是你心中最好的淋浴系统呢？",
                    pic = "http://img1.guokr.com/gkimage/gr/iy/lw/griylw.png",
                    id = 95495
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "环球科技观光团",
                    m_url = "/api/m/article/?article_id=95485&show_type=latest&which_one=this",
                    title = "斑马的黑白条纹到底有什么用？",
                    Abstract = "为什么斑马有黑白条纹？曾有人提出条纹可以迷惑捕食者、起伪装作用，并便于在大群斑马中互相识别，但没人用实验验证过。新近有研究发现黑白条纹可以干扰虻科昆虫对光的感受，或许能保护斑马免受叮咬。",
                    pic = "http://img1.guokr.com/gkimage/b2/se/m2/b2sem2.png",
                    id = 95485
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "谣言粉碎机",
                    m_url = "/api/m/article/?article_id=95405&show_type=latest&which_one=this",
                    title = "“吃人塘鲺”现形记",
                    Abstract = "网上不断地有帖子称“某地水库惊现食人塘虱”，内容基本一样，但地点天南地北。这是怎么回事？",
                    pic = "http://img1.guokr.com/gkimage/d3/yr/6v/d3yr6v.png",
                    id = 95405
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "心事鉴定组",
                    m_url = "/api/m/article/?article_id=95417&show_type=latest&which_one=this",
                    title = "你的抑郁需要“休假式治疗”吗？ ",
                    Abstract = "“长期超负荷工作”会导致抑郁吗？抑郁成了个特别流行的词汇，但怎样判断自己有抑郁症呢？其实抑郁症不是那么容易得的。\r\n",
                    pic = "http://img1.guokr.com/gkimage/jg/qr/b0/jgqrb0.png",
                    id = 95417
                });
                latest_articles.Add(new article()
                {
                    minisite_name = "创意科技",
                    m_url = "/api/m/article/?article_id=95392&show_type=latest&which_one=this",
                    title = "苹果一天比推特一年赚得还多",
                    Abstract = "市场研究机构eMarketer发布了对Twitter未来收入情况的预测，预计Twitter的营收将会在2014年超过5亿美元，这个数据非常不错，但比起苹果还是差距太大，苹果每天就有这么多收入。",
                    pic = "http://img1.guokr.com/gkimage/z9/t2/7s/z9t27s.png",
                    id = 95392
                });

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
            }
            else
            {
                TaskEx.Run(async () =>
                    {
                        await latest_article_list.load_more();
                        await MrGuokrWeiboList.load_more();
                    });
            }
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
        
        
        

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}