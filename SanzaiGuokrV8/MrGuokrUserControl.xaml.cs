using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Collections.Generic;

namespace SanzaiGuokr
{
	public partial class MrGuokrUserControl : UserControl
	{
		public MrGuokrUserControl()
		{
			// Required to initialize variables
			InitializeComponent();
		}

        public static double scroll_length;
        ScrollViewer sv;
        private void MrGuokr_List_Loaded(object sender, RoutedEventArgs e)
        {
            if(sv==null)
                sv = FindChildOfType<ScrollViewer>(this.LayoutRoot);

            if (sv!=null && MrGuokrUserControl.scroll_length != default(double))
                sv.ScrollToVerticalOffset(MrGuokrUserControl.scroll_length);
        }

        private void MrGuokr_List_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sv != null)
                MrGuokrUserControl.scroll_length = sv.VerticalOffset;
        }

        static T FindChildOfType<T>(DependencyObject root) where T : class
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                DependencyObject current = queue.Dequeue();
                for (int i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    queue.Enqueue(child);
                }
            }
            return null;
        }

	}
}