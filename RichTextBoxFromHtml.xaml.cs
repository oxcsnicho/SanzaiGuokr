using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using HtmlAgilityPack;

namespace SanzaiGuokr
{
    public partial class RichTextBoxFromHtml : UserControl
    {
        public RichTextBoxFromHtml()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var hb = new Binding("Height");
            hb.Source = InternalRTB;
            hb.Mode = BindingMode.TwoWay;
            
            var wb = new Binding("Width");
            wb.Source = InternalRTB;
            wb.Mode = BindingMode.TwoWay;

            var bb = new Binding("Background");
            bb.Source = InternalRTB;
            bb.Mode = BindingMode.TwoWay;
        }

        #region HtmlSource
        public static readonly DependencyProperty HtmlSourceProperty =
            DependencyProperty.Register("HtmlSource", typeof(string), typeof(RichTextBoxFromHtml),
            new PropertyMetadata(default(string), new PropertyChangedCallback(HtmlSourceChanged)));
        public string HtmlSource
        {
            get
            {
                return (string)GetValue(HtmlSourceProperty);
            }
            set
            {
                SetValue(HtmlSourceProperty, value);
            }
        }
        static void HtmlSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var current = sender as RichTextBoxFromHtml;
            if (current == null)
                return; // throw exception
            var rtb = current.InternalRTB;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml((string)e.NewValue);
            var p = new Paragraph();
            foreach (var item in doc.DocumentNode.ChildNodes)
            {
                var r = new Run();
                switch (item.NodeType)
                {
                    case HtmlNodeType.Comment:
                        throw new NotImplementedException();
                    case HtmlNodeType.Document:
                        throw new NotImplementedException();
                    case HtmlNodeType.Element:
                        if (item.Name == "br")
                        {
                            p.Inlines.Add(new LineBreak());
                            continue;
                        }
                        else if (item.Name == "blockquote")
                        {
                            r.Foreground = current.SubtleForeground;
                            r.Text = item.InnerText;
                        }
                        else if (item.Name == "img")
                        {
                            // TODO: not implemented
                        }
                        else if (item.Name == "a")
                        {
                            // TODO: not implemented
                            r.Foreground = current.Foreground;
                            r.Text = item.InnerText;
                        }
                        else if (item.Name == "b")
                        {
                            r.FontWeight = FontWeights.Bold;
                            r.Foreground = current.Foreground;
                            r.Text = item.InnerText;
                        }
                        else if (item.Name == "i")
                        {
                            r.FontStyle = FontStyles.Italic;
                            r.Foreground = current.Foreground;
                            r.Text = item.InnerText;
                        }
                        else
                        {
                            //throw new NotImplementedException();
                        }
                        break;
                    case HtmlNodeType.Text:
                        r.Foreground = current.Foreground;
                        r.Text = item.InnerText;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                p.Inlines.Add(r);
            }
            rtb.Blocks.Clear();
            rtb.Blocks.Add(p);
        }
        #endregion

        #region Color

#if false
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(RichTextBoxFromHtml), null);
        public Brush Foreground
        {
            get
            {
                return (Brush)GetValue(ForegroundProperty);
            }
            set
            {
                SetValue(ForegroundProperty, value);
            }
        }
#endif
        public static readonly DependencyProperty SubtleForegroundProperty =
            DependencyProperty.Register("SubtleForeground", typeof(Brush), typeof(RichTextBoxFromHtml), null);
        public Brush SubtleForeground
        {
            get
            {
                return (Brush)GetValue(SubtleForegroundProperty);
            }
            set
            {
                SetValue(SubtleForegroundProperty, value);
            }
        }
        #endregion
    }
}
