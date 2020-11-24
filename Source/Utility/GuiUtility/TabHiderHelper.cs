using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AutosarGuiEditor.Source.Utility.GuiUtility
{
    /* This class helps to hide innactive tabs and */
    public class TabHiderHelper
    {
        TabControl tabControl;
        List<TabItem> allTabs = new List<TabItem>();
        public TabHiderHelper(TabControl tabControl)
        {
            this.tabControl = tabControl;
            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                allTabs.Add(tabControl.Items[i] as TabItem);
            }                
        }

        public void ProcessTabs()
        {            
            /* remove all inactive tabs */
            for (int i = tabControl.Items.Count - 1; i>= 0; i--)
            {
                if ((tabControl.Items[i] as TabItem).IsEnabled == false)
                {
                    tabControl.Items.RemoveAt(i);
                }
            }

            /* Add active tabs */
            foreach(TabItem tabItem in allTabs)
            {
                if (tabItem.IsEnabled)
                {
                    /* Check if tab has already in the tabcontrol */
                    if (!IsTabAlredyInTabControl(tabItem))
                    {

                        /* if not then add tab to tabcontrol */
                        tabControl.Items.Add(tabItem);
                    }
                }
            }
        }

        public void SelectTab(TabItem selectedTab)
        {
            if (selectedTab != null)
            {
                selectedTab.IsEnabled = true;
                /* Add this tab to list */
                if (!tabControl.Items.Contains(selectedTab))
                {
                    tabControl.Items.Add(selectedTab);
                }
            }

            /* Select tab */
            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                if (tabControl.Items[i].Equals(selectedTab))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)delegate { tabControl.SelectedIndex = i; }, DispatcherPriority.Render, null);
                    break;
                }
            }
        }

        private bool IsTabAlredyInTabControl(TabItem item)
        {
            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                if (tabControl.Items[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
