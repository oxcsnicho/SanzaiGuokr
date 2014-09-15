using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SanzaiGuokr
{
    public partial class ChannelsUserControl : UserControl
    {
        public ChannelsUserControl()
        {
            // Required to initialize variables
            InitializeComponent();

            text_to_show.Text = "==此处暂时征用用于发布公告==\n\n";
            text_to_show.Text += "最近因为果壳改版，主题站被砍变成科学人，山寨果壳因为实在不想跟着改版，老的内容又没有了，只好先晾着，请大家耐心等待（再再）下一个更新哦！\n\n";
            text_to_show.Text += "==最新改动==\n\n";
            text_to_show.Text += "× 小组翻页\n";
            text_to_show.Text += "× 透明大图标(没事会翻跟斗那种)\n";

        }
    }
}