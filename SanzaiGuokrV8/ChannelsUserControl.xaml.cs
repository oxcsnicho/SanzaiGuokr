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
            text_to_show.Text += "最近因为果壳改版，主题站被砍变成科学人，山寨果壳还在积极抢修，有一些技术支持还在等果壳官方完善，请大家耐心等待下一个更新哦！";
        }
    }
}