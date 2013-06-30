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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;

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
#if false
            var hb = new Binding("Height");
            hb.Source = InternalRTB;
            hb.Mode = BindingMode.TwoWay;

            var wb = new Binding("Width");
            wb.Source = InternalRTB;
            wb.Mode = BindingMode.TwoWay;

            var bb = new Binding("Background");
            bb.Source = InternalRTB;
            bb.Mode = BindingMode.TwoWay;
#endif
        }

        #region HtmlDocSource
        public static readonly DependencyProperty HtmlDocSourceProperty =
            DependencyProperty.Register("HtmlDocSource", typeof(HtmlDocument), typeof(RichTextBoxFromHtml),
            new PropertyMetadata(default(HtmlDocument), new PropertyChangedCallback(HtmlDocSourceChanged)));
        public HtmlDocument HtmlDocSource
        {
            get
            {
                return (HtmlDocument)GetValue(HtmlDocSourceProperty);
            }
            set
            {
                SetValue(HtmlDocSourceProperty, value);
            }
        }
        static void HtmlDocSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var current = sender as RichTextBoxFromHtml;
            if (current == null)
                return; // throw exception

            HtmlDocument doc = e.NewValue as HtmlDocument;
            if (doc == null)
                return;
            Brush linkForeGround = Application.Current.Resources["DefaultGreenBrush"] as Brush;
            Brush linkMouseOverForeGround = Application.Current.Resources["DefaultBlueBrush"] as Brush;
            var p = new List<Inline>();
            foreach (var item in doc.DocumentNode.ChildNodes)
            {
                var r = new Run();
                try
                {
                    switch (item.NodeType)
                    {
                        case HtmlNodeType.Comment:
                            throw new NotImplementedException();
                        case HtmlNodeType.Document:
                            throw new NotImplementedException();
                        case HtmlNodeType.Element:
                            if (item.Name == "br")
                            {
                                if (p.Count == 0 || p.Last().GetType() != typeof(LineBreak))
                                    p.Add(new LineBreak());
                                continue;
                            }
                            else if (item.Name == "blockquote")
                            {
                                // TODO: iterate into the element for one more level
                                r.Foreground = current.SubtleForeground;
                                r.Text = item.InnerText;
                            }
                            else if (item.Name == "img")
                            {

                                if (Microsoft.Phone.Info.DeviceStatus.DeviceTotalMemory / 1048576 < 256)
                                {
                                    r.Foreground = current.Foreground;
                                    r.Text = item.GetAttributeValue("src", "<有图片>");
                                }
                                else
                                {
                                    Image MyImage = new Image();
                                    string url = item.Attributes["src"].Value;
                                    var _imgsrc = new BitmapImage();
                                    _imgsrc.CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation;
                                    _imgsrc.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                                    _imgsrc.ImageFailed += (ss, ee) =>
                                    {
                                        WebClient wc = new WebClient();
                                        wc.Headers["Referer"] = "http://www.guokr.com";
                                        wc.OpenReadCompleted += (s, eee) =>
                                            {
                                                try
                                                {
                                                    _imgsrc.SetSource(eee.Result);
                                                }
                                                catch
                                                {

                                                }
                                            };
                                        wc.OpenReadAsync(_imgsrc.UriSource);
                                    };
                                    MyImage.Source = _imgsrc;
                                    InlineUIContainer MyUI = new InlineUIContainer();
                                    MyImage.HorizontalAlignment = HorizontalAlignment.Left;
                                    if (url.Substring(url.Length - 3) == "gif")
                                        MyImage.Width = 22;
                                    else
                                    {
                                        MyImage.Width = 300;
                                        MyImage.Tap += (s, eee) =>
                                            Messenger.Default.Send<ViewImageMessage>(new ViewImageMessage()
                                            {
                                                small_uri = null,
                                                med_uri = url,
                                                large_uri = null
                                            });
                                    }
                                    MyUI.Child = MyImage;
                                    p.Add(MyUI);
                                    continue;
                                }
                            }
                            else if (item.Name == "a")
                            {
                                var h = new Hyperlink();
                                h.Foreground = linkForeGround;
                                h.TextDecorations = null;
                                h.MouseOverForeground = linkMouseOverForeGround;
                                h.MouseOverTextDecorations = null;
                                h.Inlines.Add(HtmlEntity.DeEntitize(item.InnerText));
                                if (item.Attributes.Contains("href"))
                                {
                                    string url = item.Attributes["href"].Value;
                                    h.Click += (ss, ee) =>
                                    {
                                        var t = new WebBrowserTask();
                                        t.Uri = new Uri(url, UriKind.RelativeOrAbsolute);
                                        t.Show();
                                    };
                                }
                                p.Add(h);
                                continue;
                            }
                            else if (item.Name == "b")
                            {
                                r.FontWeight = FontWeights.Bold;
                                r.Foreground = current.Foreground;
                                r.Text = HtmlEntity.DeEntitize(item.InnerText);
                            }
                            else if (item.Name == "i")
                            {
                                r.FontStyle = FontStyles.Italic;
                                r.Foreground = current.Foreground;
                                r.Text = HtmlEntity.DeEntitize(item.InnerText);
                            }
                            else
                            {
                                //throw new NotImplementedException();
                                r.Foreground = current.Foreground;
                                r.Text = item.InnerText;
                            }
                            break;
                        case HtmlNodeType.Text:
                            r.Foreground = current.Foreground;
                            r.Text = HtmlEntity.DeEntitize(item.InnerText);
                            while (r.Text.Length > 2000)
                            {
                                var rr = r;
                                rr.Foreground = r.Foreground;
                                rr.Text = r.Text.Substring(0, 2000);
                                p.Add(rr);
                                r.Text = r.Text.Substring(2000);
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                catch
                {
                    r = new Run();
                    r.Foreground = current.Foreground;
                    r.Text = item.InnerText;
                }
                p.Add(r);
            }
            var rtb = current.InternalRTB;
            rtb.Blocks.Clear();
            rtb.Blocks.Add(new Paragraph());

            var sp = current.LayoutRoot;
            sp.Children.Clear();
            sp.Children.Add(rtb);

            foreach (var item in p)
            {
                if ((rtb.Blocks.Last() as Paragraph).Inlines.Count > 15)
                {
                    rtb = new RichTextBox();
                    rtb.Blocks.Add(new Paragraph());
                    sp.Children.Add(rtb);
                }

                (rtb.Blocks.Last() as Paragraph).Inlines.Add(item);
            }
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
