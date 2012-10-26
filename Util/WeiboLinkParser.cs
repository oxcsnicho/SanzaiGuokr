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
using System.Windows.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using SanzaiGuokr.ViewModel;

namespace SanzaiWeibo.Utils
{
    public class WeiboLinkParser : IValueConverter
    {
        private RichTextBuilder rtbb;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /*
            if (targetType != typeof(Block))
            {
                throw new NotImplementedException();
            }
             */

            rtbb = new RichTextBuilder(new Paragraph());

            HyperlinkForeground = Application.Current.Resources["DefaultBlueBrush"] as SolidColorBrush;
            if (HyperlinkForeground == null)
                HyperlinkForeground = new SolidColorBrush(Color.FromArgb(255, 245, 222, 179));

            rtbb.HyperlinkForeground = HyperlinkForeground;
            //rtbb.RichTextBoxStyle = RichTextBoxStyle;

            ParseAndCreate(value as string);

            return rtbb.Paragraph;

        }

        private Brush HyperlinkForeground;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private Paragraph ParseAndCreate(string Text)
        {
            if (String.IsNullOrEmpty(Text))
                throw new ArgumentNullException();

            //if (_is_parsed == true)
             //   return;

            /*
            // testing code - use rtb
            var tb = new RichTextBox();
            var pgh = new Paragraph();
            pgh.Inlines.Add(Text);
            tb.Blocks.Add(pgh);
            tb.TextWrapping = TextWrapping.Wrap;
            Children.Add(tb);
            return;
             */

            /*
            // testing code - use textblock
            var tb = new TextBlock();
            tb.Text = Text;
            tb.TextWrapping = TextWrapping.Wrap;
            Children.Add(tb);
            return;
             */

            for (int i = 0, j = 0; i < Text.Length; i++)
            {
                if (i == Text.Length - 1)
                    if (j <= i)
                    {
                        rtbb.Add(Text.Substring(j, i - j + 1));
                        break;
                    }

                Match match;
                var c=Text[i];
                switch (c)
                {
                    case '@':
                        match = new Regex(@"@[\-_0-9a-zA-Z\u4e00-\u9fa5]{2,30}").Match(Text, i);
                        if (match.Index==i && match.Length > 0)
                        {
                            if(i+match.Length <Text.Length &&
                                Text[i+match.Length]=='.')
                            {
                                // todo: should recognize email address
                            }
                            else
                            {
                                // create textblock
                                if (j < i)
                                    rtbb.Add(Text.Substring(j, i - j));
                                //create hyperlink button
                                rtbb.AddHyperlink(match.Value,new Uri("/Pages/User.xaml?name="+match.Value.Substring(1),UriKind.Relative));
                                //update index
                                i += match.Length - 1;
                                j = i + 1;
                            }
                        }
                        break;
                    case 'h':
                        string pattern = "http://t.cn/";
                        int k = 0;
                        while (k < pattern.Length && i + k < Text.Length && pattern[k] == Text[i + k])
                            k++;
                        if (k == pattern.Length)
                        {
                            match = (new Regex(@"[a-zA-Z0-9]{4,10}")).Match(Text, i + k);
                            if (match.Index == i+k && match.Length > 0)
                            {
                                // flush textblock
                                if (j < i)
                                    rtbb.Add(Text.Substring(j, i - j));
                                // create hyperlink button
                                rtbb.AddHyperlink(pattern + match.Value, new Uri((pattern + match.Value), UriKind.Absolute));
                                // update index
                                i += pattern.Length + match.Length - 1;
                                j = i + 1;
                                break;
                            }
                        }
                        i += k - 1;
                        break;
                    case '#':
                        int l = i+1;
                        while (l < Text.Length
                            && (Text[l] >= 'a' && Text[l] <= 'z'
                                || Text[l] >= 'A' && Text[l] <= 'Z'
                                || Text[l] == '-' || Text[l] == '_' || Text[l] == ' ' || Text[l] == '·'
                                || Text[l] >= '\u4e00' && Text[l] <= '\u9fa5'
                                || Text[l] >= '0' && Text[l] <= '9')
                            && Text[l] != '#') l++;

                        if (l==Text.Length)
                        {
                            if (j <= l)
                                rtbb.Add(Text.Substring(j, l - j));
                            i = l;
                            j = i + 1;
                        }
                        else if (Text[l] != '#')
                        {
                            if (j <= l)
                                rtbb.Add(Text.Substring(j, l - j + 1));
                            i = l;
                            j = i + 1;
                        }
                        else
                        {
                            string t = Text.Substring(i, l - i + 1);
                            // flush textblock
                            if (j < i)
                                rtbb.Add(Text.Substring(j, i - j));
                            // create hyperlink button
                            rtbb.AddHyperlink(t, new Uri("/Pages/ViewTopic.xaml?topic=" + t.Substring(1, t.Length - 2), UriKind.Relative));
                            // update index
                            i += t.Length - 1;
                            j = i + 1;
                        }
                        break;
                }
            }

            return rtbb.Paragraph;



        }

    }
}
