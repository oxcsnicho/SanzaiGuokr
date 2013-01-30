//Author : Grigory Perepechko 
//blog   : gregit.wordpress.com
//e-mail : mylce@ya.ru
//You're free to change this file if you need, to use in any comercial and open-source products.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Microsoft.Phone.Controls;


namespace SanzaiWeibo.Control
{
    public class PivotWithHiding : Pivot
    {
        private readonly Dictionary<PivotItem, object> headers = new Dictionary<PivotItem, object>();
        private readonly List<PivotItem> hiddenItems = new List<PivotItem>();



        /// <summary>
        /// Marks pivot item as hidden, so when user scroll to it is skipped(scrolled over).
        /// </summary>        
        public void HidePivotItem(PivotItem item)
        {
            if (item == null || headers.ContainsKey(item) || hiddenItems.Contains(item))
                return;

            //Save old header
            headers[item] = item.Header;

            //Remove header to make UI more nice
            item.Header = null;

            hiddenItems.Add(item);
            if (base.SelectedItem == item)
            {
                //If hiding currently open item - scroll away
                JumpOver(item, item);
            }
        }

        public void UnHidePivotItem(PivotItem item)
        {
            if (item == null || false == headers.ContainsKey(item) || false == hiddenItems.Contains(item))
                return;

            item.Header = headers[item];

            headers.Remove(item);
            hiddenItems.Remove(item);
        }

        public bool IsPivotItemHidden(PivotItem item)
        {
            return hiddenItems.Contains(item);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {

            //Assume we can only have 1 added and removed item
            PivotItem newItemSelected = (PivotItem) e.AddedItems[0];
            PivotItem oldItemSelected = (PivotItem) e.RemovedItems[0];

            if (hiddenItems.Contains(newItemSelected))
            {
                JumpOver(newItemSelected, oldItemSelected);
            }
            else
            {
                base.OnSelectionChanged(e);
            }
        }

        private void JumpOver(PivotItem newItemSelected, PivotItem oldItemSelected)
        {
            int newindexOfSelected = base.Items.IndexOf(newItemSelected);
            int oldIndexOfSelected = base.Items.IndexOf(oldItemSelected);

            int newIndex;
            //Incrementing select index
            bool fromLastToFirst = newindexOfSelected == 0 && oldIndexOfSelected == base.Items.Count - 1;
            bool fromFirstToLast = newindexOfSelected == base.Items.Count - 1 && oldIndexOfSelected == 0;
            if ((newindexOfSelected > oldIndexOfSelected || fromLastToFirst) && (!fromFirstToLast))
                newIndex = newindexOfSelected + 1;
            else //Decrementing select index          
                newIndex = newindexOfSelected - 1;

            //Circle behavior
            if (newIndex == base.Items.Count)
                newIndex = 0;
            else if (newIndex == -1)
                newIndex = base.Items.Count - 1;

            base.SelectedIndex = newIndex;
        }
    }
}
