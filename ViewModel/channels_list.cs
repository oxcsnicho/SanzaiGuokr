using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace SanzaiGuokr.Model
{
    public class channels_list : ViewModelBase
    {
        public channels_list()
        {
            AllChannels.Add(new channel()
            {
                introduction = "捍卫真相与细节，一切谣言将在这里被终结",
                key = "fact",
                name = "谣言粉碎机"
            });
            AllChannels.Add(new channel()
            {
                introduction = "从今天起，做个理性的人，读书、思考、挑战上帝。关心逻辑和智力，追寻思维的乐趣",
                key = "logos",
                name = "死理性派"
            });
            AllChannels.Add(new channel()
            {
                introduction = "尸体、犯罪、法医什么的最喜欢了",
                key = "crime",
                name = "谋杀 现场 法医"
            });
            if (DateTime.Now > new DateTime(2013, 2, 2))
            {
                AllChannels.Add(new channel()
                {
                    introduction = "从暧昧到高潮……重口味，无力者慎入",
                    key = "sex",
                    name = "性 情"
                });
            }
            AllChannels.Add(new channel()
            {
                introduction = "大脑爱偷懒，大脑爱说谎，经验靠不住，实验来帮忙",
                key = "psybst",
                name = "心事鉴定组"
            });
            AllChannels.Add(new channel()
            {
                introduction = "每天的朝九晚五，上几篇健康小文，有趣、靠谱，还很给力",
                key = "health",
                name = "健康朝九晚五"
            });
            AllChannels.Add(new channel()
            {
                introduction = "让我们一起来分享花草树木、鸟兽虫鱼，还有灿烂星河带来的喜悦，以博物精神之名",
                key = "natural",
                name = "自然控"
            });
            AllChannels.Add(new channel()
            {
                introduction = "发明实用的装置，制造拉风的玩具，验证有趣的实验，尽在果壳网DIY",
                key = "diy",
                name = "DIY"
            });
            AllChannels.Add(new channel()
            {
                introduction = "护肤、美容、纤体、整形……想变身美女？先来这里修炼",
                key = "beauty",
                name = "美丽也是技术活"
            });
            AllChannels.Add(new channel()
            {
                introduction = "通过科学抵达的魔法世界，带你领略科学幻想维度",
                key = "microsf",
                name = "微科幻"
            });
            AllChannels.Add(new channel()
            {
                introduction = "全球各大媒体在报道什么？我们会跟踪几十种报刊中的优秀科技文章，为您提供快速导读",
                key = "digest",
                name = "环球科技观光团"
            });
            AllChannels.Add(new channel()
            {
                introduction = "科学与思考是人类前进的两只脚，协同一致，我们才能真正进步",
                key = "sciblog",
                name = "科技评论"
            });
            AllChannels.Add(new channel()
            {
                introduction = "科学+艺术+blabla，用另一视角从更宽泛的外围来审视科学的内涵",
                key = "artsci",
                name = "文艺科学"
            });
            AllChannels.Add(new channel()
            {
                introduction = "关注最有钱途的技术、人、公司，关注趋势",
                key = "techb",
                name = "科技与商业"
            });
            AllChannels.Add(new channel()
            {
                introduction = "自我学习，永无止境。",
                key = "learning",
                name = "学习之道"
            });
        }
#if false
        private ObservableCollection<channel> _ch;
        public ObservableCollection<channel> AllChannels
        {
            get
            {
                if (_ch == null)
                    _ch = new ObservableCollection<channel>();
                return _ch;
            }
            set { _ch = value; }
        }
#endif

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
