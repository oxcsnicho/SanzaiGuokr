using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Phone.Tasks;

namespace SanzaiWeibo.Utils
{
        class RichTextBuilder
        {
            Paragraph _pgh;
            public Brush HyperlinkForeground { get; set; }
            
            public RichTextBuilder()
            {
                _pgh = new Paragraph();
            }
            public RichTextBuilder(Paragraph p)
            {
                _pgh = p;
            }
            public void Add(string text)
            {
                _pgh.Inlines.Add(text);
            }
            public void AddHyperlink(string text, Uri uri)
            {
                /*
                // testing code
                var run = new Run();
                run.TextDecorations = TextDecorations.Underline;
                run.Foreground = HyperlinkForeground;
                run.Text = text;
                _pgh.Inlines.Add(run);
                return;
                 */

                Hyperlink _hp = new Hyperlink();

                if (HyperlinkForeground != null)
                    _hp.Foreground = HyperlinkForeground;
                _hp.TextDecorations = null;

                if (text[0] == '@')
                {
                    /*
                    var at = new Run();
                    at.FontFamily = buxton;
                    at.Text = "@";
                    _hp.Inlines.Add(at);
                     */

                    var str = new Run();
                    str.Text = text;
                    _hp.Inlines.Add(str);

                    _hp.NavigateUri = uri;
                    /*
                    _hp.Click+=new RoutedEventHandler((ss,ee)=>
                    {
                        (PhoneApplicationPageLocator.Page as MainPage).SingleTapWithDelay(uri, new System.Windows.Input.GestureEventArgs() );
                    });
                     */
                }
                else if(text[0]== '#')
                {
                    /*
                    var sharp = new Run();
                    sharp.FontFamily = buxton;
                    sharp.Text = "#";
                    _hp.Inlines.Add(sharp);
                     */

                    var str = new Run();
                    str.Text = text;
                    str.FontFamily = new FontFamily("Segoe WP");
                    _hp.Inlines.Add(str);

                    _hp.NavigateUri = uri;
                    /*
                    _hp.Click+=new RoutedEventHandler((ss,ee)=>
                    {
                        (PhoneApplicationPageLocator.Page as MainPage).SingleTapWithDelay(uri, new System.Windows.Input.GestureEventArgs() );
                    });
                     */
                }
                else
                {
                    _hp.Inlines.Add(text);
                    _hp.Click += delegate(object sender, RoutedEventArgs e)
                    {
                        WebBrowserTask wb = new WebBrowserTask();
                        wb.Uri = uri;
                        try
                        {
                            wb.Show();
                        }
                        catch
                        {
                            // TODO: should check the navigation status. need reference to the application
                        }
                    };
                }

                _pgh.Inlines.Add(_hp);
            }
            private RichTextBox rtb;

            public RichTextBox RichTextBox
            {
                get
                {
                    var rtb = new RichTextBox();
                    rtb.Blocks.Add(_pgh);
                    return rtb;
                }
                set { rtb = value; }
            }

            public RichTextBox GetRichTextBox()
            {
                var rtb = new RichTextBox();
                rtb.Blocks.Add(_pgh);
                return rtb;
            }

            public Paragraph Paragraph
            {
                get { return _pgh; }
                set { _pgh = value; }
            }

        }
}
