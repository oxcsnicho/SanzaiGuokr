using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SanzaiGuokrV8.Controls
{
    public class LongListSelector : Microsoft.Phone.Controls.LongListSelector
    {
        public LongListSelector()
            : base()
        {
            Loaded += MyLongListSelector_Loaded;
        }

        ViewportControl vc;
        private void MyLongListSelector_Loaded(object sender, RoutedEventArgs e)
        {
            vc = FindSomething<ViewportControl>(this);
            vc.ViewportChanged += vc_ViewportChanged;
        }

        bool _scrollLock = false;
        int _offsetKnob = 500;
        private void vc_ViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            var bar = vc.Bounds.Bottom - _offsetKnob;
            if (vc.Viewport.Bottom < bar)
                _scrollLock = false;
            if (_scrollLock == false
                && vc.Viewport.Bottom > bar)
            {
                if (ViewportReachedBottom != null)
                    ViewportReachedBottom(this, new RoutedEventArgs());
                _scrollLock = true;
            }
        }

        private static T FindSomething<T>(DependencyObject parent) where T : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childCount; i++)
            {
                var elt = VisualTreeHelper.GetChild(parent, i);
                if (elt is T) return (T)elt;
                var result = FindSomething<T>(elt);
                if (result != null) return result;
            }
            return null;
        }

        public event RoutedEventHandler ViewportReachedBottom;
        public event RoutedEventHandler BottomStretched;

        protected override void OnManipulationStarted(System.Windows.Input.ManipulationStartedEventArgs e)
        {
            if (vc.Viewport.Bottom == vc.Bounds.Bottom)
                if (BottomStretched != null)
                    BottomStretched(this, new RoutedEventArgs());
            base.OnManipulationStarted(e);
        }
    }
}
