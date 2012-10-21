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

namespace SanzaiWeibo.Utils
{
    public static class RTBNavigationService
    {

        public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached(
            "Content",
            typeof(string),
            typeof(RTBNavigationService),
            new PropertyMetadata(null, OnContentChanged)
        );

        public static string GetContent(DependencyObject d)
        { return d.GetValue(ContentProperty) as string; }

        public static void SetContent(DependencyObject d, string value)
        { d.SetValue(ContentProperty, value); }


        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBox richTextBox = d as RichTextBox;
            if (richTextBox == null)
                return;


            string content = (string)e.NewValue;
            if (string.IsNullOrEmpty(content))
                return;

            richTextBox.Blocks.Clear();

            var cvtr = new WeiboLinkParser();


            Paragraph pg = new Paragraph();
            pg.Inlines.Add(content);

            richTextBox.Blocks.Add(pg);

            d.Dispatcher.BeginInvoke(() =>
            {
                richTextBox.Blocks.Clear();
                richTextBox.Blocks.Add(cvtr.Convert(content, typeof(Paragraph), null, null) as Paragraph);

            });


        }

    }
}
