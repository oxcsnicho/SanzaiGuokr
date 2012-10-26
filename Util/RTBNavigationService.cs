using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using HtmlAgilityPack;

namespace SanzaiWeibo.Utils
{
    public static class RTBNavigationService
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached(
            "Content",
            typeof(HtmlDocument),
            typeof(RTBNavigationService),
            new PropertyMetadata(null, OnContentChanged)
        );

        public static HtmlDocument GetContent(DependencyObject d)
        { return d.GetValue(ContentProperty) as HtmlDocument; }

        public static void SetContent(DependencyObject d, HtmlDocument value)
        { d.SetValue(ContentProperty, value); }


        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox richTextBox = d as RichTextBox;
            if (richTextBox == null)
                return;

            HtmlDocument content = (HtmlDocument)e.NewValue;
            if (content == null)
                return;

            var HyperlinkForeground = Application.Current.Resources["DefaultGreenBrush"] as SolidColorBrush;
            if (HyperlinkForeground == null)
                HyperlinkForeground = new SolidColorBrush(Color.FromArgb(255, 245, 222, 179));

            richTextBox.Blocks.Clear();
            var p = new Paragraph();
            foreach (var item in content.DocumentNode.ChildNodes)
            {
                var r = new Run();
                if (item.NodeType == HtmlNodeType.Element)
                    r.Foreground = HyperlinkForeground;
                else
                    r.Foreground = richTextBox.Foreground;

                r.Text = item.InnerText;
                p.Inlines.Add(r);
            }
            richTextBox.Blocks.Add(p);


        }

    }
}
