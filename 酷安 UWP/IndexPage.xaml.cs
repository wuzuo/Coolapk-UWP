﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        MainPage mainPage;
        static int page = 0;
        static string lastItem;
        static ObservableCollection<Feed> FeedsCollection = new ObservableCollection<Feed>();
        public IndexPage()
        {
            this.InitializeComponent();
            listView.ItemsSource = FeedsCollection;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = e.Parameter as MainPage;
            if (FeedsCollection.Count == 0)
                GetIndexPage(++page);
            VScrollViewer.ChangeView(null, 20, null);
        }

        public async void GetIndexPage(int page)
        {
            mainPage.ActiveProgressRing();
            if (page == 1)
            {
                timer.Stop();
                timer = new DispatcherTimer();
                JArray Root = await CoolApkSDK.GetIndexList(page, string.Empty);
                if (FeedsCollection.Count != 0)
                    for (int i = 0; i < 3; i++)
                        FeedsCollection.RemoveAt(0);
                else lastItem = Root.Last["entityId"].ToString();
                for (int i = 0; i < Root.Count; i++)
                    FeedsCollection.Insert(i, new Feed((JObject)Root[i]));
                timer.Interval = new TimeSpan(0, 0, 7);
                timer.Tick += (s, e) =>
                {
                    if (flip.SelectedIndex < flip.Items.Count - 1)
                        flip.SelectedIndex++;
                    else
                        flip.SelectedIndex = 0;
                };
                timer.Start();
            }
            else
            {
                JArray Root = await CoolApkSDK.GetIndexList(page, lastItem);
                if (Root.Count != 0)
                {
                    lastItem = Root.Last["entityId"].ToString();
                    foreach (JObject i in Root)
                        FeedsCollection.Add(new Feed(i));
                }
                else page--;
            }
            mainPage.DeactiveProgressRing();
        }

        DispatcherTimer timer = new DispatcherTimer();
        FlipView flip;

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flip is null) flip = sender as FlipView;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            timer.Stop();
        }

        private void FeedListViewItem_Tapped(object sender, TappedRoutedEventArgs e) => mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { ((sender as FrameworkElement).Tag as Feed).GetValue("id"), mainPage, "动态", null });
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
                if (FeedsCollection.Count != 0)
                    if (VScrollViewer.VerticalOffset == 0)
                    {
                        GetIndexPage(1);
                        VScrollViewer.ChangeView(null, 20, null);
                    }
                    else if (VScrollViewer.VerticalOffset == VScrollViewer.ScrollableHeight)
                        GetIndexPage(++page);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button i = sender as Button;
            if (i.Tag as string == "Refresh")
            {
                GetIndexPage(1);
                VScrollViewer.ChangeView(null, 20, null);
            }
            else mainPage.Frame.Navigate(typeof(UserPage), new object[] { i.Tag as string, mainPage });
        }
    }
}
