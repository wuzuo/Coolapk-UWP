﻿using CoolapkUWP.Helpers;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class NotificationsPage : Page
    {
        private ViewModels.NotificationsPage.ViewModel provider;

        public NotificationsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            titleBar.ShowProgressRing();
            provider = e.Parameter as ViewModels.NotificationsPage.ViewModel;

            if (provider.ListType == ViewModels.NotificationsPage.ListType.Comment)
            {
                _ = FindName(nameof(NavigateItems));
            }
            list.ItemsSource = provider.Models;
            await Load(-2);
            titleBar.Title = provider.Title;
            _ = scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
            titleBar.HideProgressRing();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            provider.VerticalOffsets[0] = scrollViewer.VerticalOffset;
            titleBar.Title = string.Empty;

            base.OnNavigatingFrom(e);
        }

        private async Task Load(int p = -1)
        {
            titleBar.ShowProgressRing();
            if (p == -2)
            {
                _ = scrollViewer.ChangeView(null, 0, null);
                titleBar.Title = provider.Title;
                UIHelper.NotificationNums.GetNums();
            }
            await provider?.Refresh(p);
            titleBar.HideProgressRing();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            switch ((e.ClickedItem as StackPanel).Tag as string)
            {
                case "atMe":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.AtMe));
                    break;

                case "atCommentMe":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.AtCommentMe));
                    break;

                case "like":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Like));
                    break;

                case "follow":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Follow));
                    break;

                case "message":
                    _ = Frame.Navigate(typeof(NotificationsPage), new ViewModels.NotificationsPage.ViewModel(ViewModels.NotificationsPage.ListType.Message));
                    break;
                default:
                    break;
            }
        }

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e) => _ = Load(-2);

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                UIHelper.NotificationNums.GetNums();
            }
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                _ = Load();
            }
        }

        private async void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            using (Windows.Foundation.Deferral RefreshCompletionDeferral = args.GetDeferral())
            {
                await Load(-2);
            }
        }
    }
}