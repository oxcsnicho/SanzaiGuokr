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
using SanzaiWeibo.Control;

namespace SanzaiGuokr
{
    public class InlinesHolder : List<Inline>
    {
        public List<List<Inline>> pp = new List<List<Inline>>();
        public int total = 0;
        public const int Threshold = 500;
        public void AddInline(Inline item, int count)
        {
            if (total + count > Threshold)
                flush();

            this.Add(item);
            total += count;
        }
        public void flush()
        {
            var p = new List<Inline>();
            pp.Add(p);
            foreach (var item in this)
                p.Add(item);
            this.Clear();
            total = 0;
        }
    }
    public partial class RichTextBoxFromHtml : UserControl
    {
        const int Threshold = 100;
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
            var p = new InlinesHolder();
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
                                //if (p.Count == 0 || p.Last().GetType() != typeof(LineBreak))
                                    //p.AddInline(new LineBreak(), 1);
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
                                    ImageProperties.SetSourceWithCustomReferer(MyImage, new Uri(url, UriKind.RelativeOrAbsolute));
                                    InlineUIContainer MyUI = new InlineUIContainer();
                                    MyImage.HorizontalAlignment = HorizontalAlignment.Left;
                                    var ext = url.Substring(url.Length - 3);
                                    if (ext == "gif")
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
                                    p.AddInline(MyUI, ext == "gif" ? 1 : InlinesHolder.Threshold);
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
                                p.AddInline(h, item.InnerText.Count());
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
                            r.Text = HtmlEntity.DeEntitize(item.InnerText);//.TrimStart(new char[] { '\n' });
                            if (string.IsNullOrEmpty(r.Text))
                                continue;
                            while (r.Text.Length > Threshold)
                            {
                                var rr = new Run();
                                rr.Foreground = r.Foreground;
                                rr.Text = r.Text.Substring(0, Threshold);
                                p.AddInline(rr, rr.Text.Length);
                                r.Text = r.Text.Substring(Threshold);
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
                p.AddInline(r, r.Text.Length);
            }
            p.flush();

            var rtb = current.InternalRTB;
            rtb.Blocks.Clear();
            rtb.Blocks.Add(new Paragraph());

            var sp = current.LayoutRoot;
            sp.Children.Clear();

            try
            {
                foreach (var iitem in p.pp)
                {
                    if (sp.Children.Count > 0)
                    {
                        rtb = new RichTextBox();
                        rtb.Blocks.Add(new Paragraph());
                    }

                    foreach (var iiitem in iitem)
                        (rtb.Blocks.Last() as Paragraph).Inlines.Add(iiitem);

                    sp.Children.Add(rtb);
                }
            }
            catch
            {
                sp.Children.Add(rtb);
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
