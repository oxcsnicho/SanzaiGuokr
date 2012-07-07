using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SanzaiWeibo.Controls
{
	public partial class WeiboInputTextBox : UserControl
	{
		public WeiboInputTextBox()
		{
			// Required to initialize variables
			InitializeComponent();
            TextLimit = 140;
		}
        private string _text;

        public string Text
        {
            get { return text_box.Text; }
            set { text_box.Text = value; }
        }

        private void text_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text.Length < TextLimit*0.7)
            {
                status_bar.Text = "";
            }
            else if (tb.Text.Length >= TextLimit)
            {
                status_bar.Text = string.Format("{0}个字，自动切割", tb.Text.Length);
                status_bar.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (tb.Text.Length >= TextLimit*0.7)
            {
                status_bar.Text = string.Format("{0}个字", tb.Text.Length);
                status_bar.Foreground = new SolidColorBrush(Colors.White);
            }

        }


        public int TextLimit { get; set; }
    }
}