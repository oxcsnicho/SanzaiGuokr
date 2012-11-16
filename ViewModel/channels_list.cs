using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace SanzaiGuokr.Model
{
    public class channels_list : ViewModelBase
    {
        public channels_list()
        {
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/fact_b.jpg",
                introduction = "捍卫真相与细节，一切谣言将在这里被终结",
                pic_small = "http://m.guokr.com/skin/mini_logo/fact_s.jpg",
                id = 14,
                order = 1,
                name = "谣言粉碎机"
            });
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/logos_b.jpg",
                introduction = "从今天起，做个理性的人，读书、思考、挑战上帝。关心逻辑和智力，追寻思维的乐趣",
                pic_small = "http://m.guokr.com/skin/mini_logo/logos_s.jpg",
                id = 13,
                order = 2,
                name = "死理性派"
            });
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/crime_b.jpg",
                introduction = "尸体、犯罪、法医什么的最喜欢了",
                pic_small = "http://m.guokr.com/skin/mini_logo/crime_s.jpg",
                id = 5,
                order = 3,
                name = "谋杀 现场 法医"
            });
            if (DateTime.Now > new DateTime(2012, 11, 20))
            {
                PriorityChannels.Add(new channel()
                {
                    pic_large = "http://m.guokr.com/skin/mini_logo/sex_b.jpg",
                    introduction = "从暧昧到高潮……重口味，无力者慎入",
                    pic_small = "http://m.guokr.com/skin/mini_logo/sex_s.jpg",
                    id = 4,
                    order = 4,
                    name = "性 情"
                });
            }
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/psybst_b.jpg",
                introduction = "大脑爱偷懒，大脑爱说谎，经验靠不住，实验来帮忙",
                pic_small = "http://m.guokr.com/skin/mini_logo/psybst_s.jpg",
                id = 7,
                order = 5,
                name = "心事鉴定组"
            });
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/health_b.jpg",
                introduction = "每天的朝九晚五，上几篇健康小文，有趣、靠谱，还很给力",
                pic_small = "http://m.guokr.com/skin/mini_logo/health_s.jpg",
                id = 2,
                order = 6,
                name = "健康朝九晚五"
            });
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/natural_b.jpg",
                introduction = "让我们一起来分享花草树木、鸟兽虫鱼，还有灿烂星河带来的喜悦，以博物精神之名",
                pic_small = "http://m.guokr.com/skin/mini_logo/natural_s.jpg",
                id = 10,
                order = 7,
                name = "自然控"
            });
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/gizfan_b.jpg",
                introduction = "关注最新鲜最有趣的科技资讯，还有最好玩最温暖的发明设计",
                pic_small = "http://m.guokr.com/skin/mini_logo/gizfan_s.jpg",
                id = 12,
                order = 8,
                name = "创意科技"
            });
            PriorityChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/diy_b.jpg",
                introduction = "发明实用的装置，制造拉风的玩具，验证有趣的实验，尽在果壳网DIY",
                pic_small = "http://m.guokr.com/skin/mini_logo/diy_s.jpg",
                id = 1,
                order = 9,
                name = "DIY"
            });


            // totally 5 tiles are on the first page...

            foreach (var item in PriorityChannels)
            {
                AllChannels.Add(item);
            }
            AllChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/beauty_b.jpg",
                introduction = "护肤、美容、纤体、整形……想变身美女？先来这里修炼",
                pic_small = "http://m.guokr.com/skin/mini_logo/beauty_s.jpg",
                id = 17,
                order = 10,
                name = "美丽也是技术活"
            });
            AllChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/microsf_b.jpg",
                introduction = "通过科学抵达的魔法世界，带你领略科学幻想维度",
                pic_small = "http://m.guokr.com/skin/mini_logo/microsf_s.jpg",
                id = 8,
                order = 10,
                name = "微科幻"
            });
            AllChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/digest_b.jpg",
                introduction = "全球各大媒体在报道什么？我们会跟踪几十种报刊中的优秀科技文章，为您提供快速导读",
                pic_small = "http://m.guokr.com/skin/mini_logo/digest_s.jpg",
                id = 11,
                order = 11,
                name = "环球科技观光团"
            });
            AllChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/sciblog_b.jpg",
                introduction = "科学与思考是人类前进的两只脚，协同一致，我们才能真正进步",
                pic_small = "http://m.guokr.com/skin/mini_logo/sciblog_s.jpg",
                id = 9,
                order = 12,
                name = "科技视点"
            });
            AllChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/artsci_b.jpg",
                introduction = "科学+艺术+blabla，用另一视角从更宽泛的外围来审视科学的内涵",
                pic_small = "http://m.guokr.com/skin/mini_logo/artsci_s.jpg",
                id = 6,
                order = 13,
                name = "文艺科学"
            });
            AllChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/pet_b.jpg",
                introduction = "除了对动物的爱，再没有什么能带人类重返天堂",
                pic_small = "http://m.guokr.com/skin/mini_logo/pet_s.jpg",
                id = 3,
                order = 14,
                name = "爱宠"
            });
            AllChannels.Add(new channel()
            {
                pic_large = "http://m.guokr.com/skin/mini_logo/techb_b.jpg",
                introduction = "关注最有钱途的技术、人、公司，关注趋势",
                pic_small = "http://m.guokr.com/skin/mini_logo/techb_s.jpg",
                id = 16,
                order = 16,
                name = "科技与商业"
            });
        }
        private ObservableCollection<channel> _ch;
        public ObservableCollection<channel> PriorityChannels
        {
            get
            {
                if (_ch == null)
                    _ch = new ObservableCollection<channel>();
                return _ch;
            }
            set { _ch = value; }
        }

        private ObservableCollection<channel> _all_ch;
        public ObservableCollection<channel> AllChannels
        {
            get
            {
                if (_all_ch == null)
                    _all_ch = new ObservableCollection<channel>();
                return _all_ch;
            }
            set { _all_ch = value; }
        }



    }
}
