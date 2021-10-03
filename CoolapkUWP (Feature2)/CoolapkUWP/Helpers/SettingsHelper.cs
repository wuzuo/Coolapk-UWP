﻿using CoolapkUWP.Control;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Data
{
    internal static class SettingsHelper
    {
        public static ulong version = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static string cookie = string.Empty;
        public static string GetString(string key) => localSettings.Values[key] as string;

        public static double PageTitleHeight => HasStatusBar ? 40 : 80;
        public static Thickness StackPanelMargin => new Thickness(0, PageTitleHeight, 0, 2);
        public static Thickness ButtonMargin => new Thickness(0, PageTitleHeight - 48, 0, 2);

        public static bool GetBoolen(string key) => (bool)localSettings.Values[key];
        public static bool HasStatusBar => Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        public static bool IsAuthor => ApplicationData.Current.LocalSettings.Values["IsAuthor"] != null && (bool)ApplicationData.Current.LocalSettings.Values["IsAuthor"];
        public static bool IsSpecialUser => (ApplicationData.Current.LocalSettings.Values["IsAuthor"] != null && (bool)ApplicationData.Current.LocalSettings.Values["IsAuthor"]) || (ApplicationData.Current.LocalSettings.Values["IsSpecial"] != null && (bool)ApplicationData.Current.LocalSettings.Values["IsSpecial"]);
        
        public static UISettings UISetting => new UISettings();
        public static void Set(string key, object value) => localSettings.Values[key] = value;
        public static double WindowsVersion = double.Parse($"{(ushort)((version & 0x00000000FFFF0000L) >> 16)}.{(ushort)(SettingsHelper.version & 0x000000000000FFFFL)}");
        public static VerticalAlignment TitleContentVerticalAlignment => VerticalAlignment.Bottom;
        public static ElementTheme Theme => GetBoolen("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (GetBoolen("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);
        public static SolidColorBrush SystemAccentColorBrush => Windows.UI.Xaml.Application.Current.Resources.ThemeDictionaries["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
        
        static SettingsHelper()
        {
            if (!localSettings.Values.ContainsKey("IsNoPicsMode"))
            { localSettings.Values.Add("IsNoPicsMode", false); }
            if (!localSettings.Values.ContainsKey("IsUseOldEmojiMode"))
            { localSettings.Values.Add("IsUseOldEmojiMode", false); }
            if (!localSettings.Values.ContainsKey("IsDarkMode"))
            { localSettings.Values.Add("IsDarkMode", false); }
            if (!localSettings.Values.ContainsKey("CheckUpdateWhenLuanching"))
            { localSettings.Values.Add("CheckUpdateWhenLuanching", false); }
            if (!localSettings.Values.ContainsKey("IsBackgroundColorFollowSystem"))
            { localSettings.Values.Add("IsBackgroundColorFollowSystem", true); }
            if (localSettings.Values.ContainsKey("UserName"))
            {
                _ = localSettings.Values.Remove("Uid");
                _ = localSettings.Values.Remove("UserName");
                _ = localSettings.Values.Remove("UserAvatar");
            }
            if (!localSettings.Values.ContainsKey("Uid"))
            { localSettings.Values.Add("Uid", string.Empty); }
            CheckTheme();
        }

        public static bool IsDarkTheme()
        {
            if (Theme == ElementTheme.Default)
            {
                return Windows.UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Dark;
            }
            return Theme == ElementTheme.Dark;
        }

        public static async Task CheckUpdate(bool IsBackground = false)
        {
            if (WindowsVersion > 16266)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        JsonObject keys;
                        try { keys = JsonObject.Parse(await UIHelper.GetHTML("https://api.github.com/repos/Coolapk-UWP/Coolapk-UWP/releases/latest", "XMLHttpRequest", true)); }
                        catch { keys = JsonObject.Parse(await UIHelper.GetHTML("https://v2.kkpp.cc/repos/Coolapk-UWP/Coolapk-UWP/releases/latest", "XMLHttpRequest", true)); }
                        string[] ver = keys["tag_name"].GetString().Replace("v", string.Empty).Split('.');
                        if (ushort.Parse(ver[0]) > Package.Current.Id.Version.Major
                            || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) > Package.Current.Id.Version.Minor)
                            || (ushort.Parse(ver[0]) == Package.Current.Id.Version.Major && ushort.Parse(ver[1]) == Package.Current.Id.Version.Minor && ushort.Parse(ver[2]) > Package.Current.Id.Version.Build))
                        {
                            GetUpdateContentDialog dialog = new GetUpdateContentDialog(keys["html_url"].GetString(), keys["body"].GetString()) { RequestedTheme = Theme };
                            _ = dialog.ShowAsync();
                        }
                        else if (!IsBackground) { UIHelper.ShowMessage("当前无可用更新。"); }
                    }
                }
                catch (HttpRequestException ex) { UIHelper.ShowHttpExceptionMessage(ex); }
            }
            else if (!IsBackground)
            {
                UIHelper.ShowMessage($"正式版通道不再支持 Build {WindowsVersion}，请前往 Github 下载 Feature2 更新");
            }
        }

        public static async void CheckTheme()
        {
            while (Window.Current?.Content is null)
            { await Task.Delay(100); }
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = Theme;
                foreach (Windows.UI.Xaml.Controls.Primitives.Popup item in UIHelper.popups)
                { item.RequestedTheme = Theme; }

                bool IsDark = IsDarkTheme();

                if (HasStatusBar)
                {
                    try
                    {
                        Microsoft.UI.Xaml.Media.AcrylicBrush AccentColor = (Microsoft.UI.Xaml.Media.AcrylicBrush)Windows.UI.Xaml.Application.Current.Resources["SystemControlChromeMediumLowAcrylicElementMediumBrush"];
                        if (IsDark)
                        {
                            StatusBar statusBar = StatusBar.GetForCurrentView();
                            statusBar.BackgroundColor = AccentColor.FallbackColor;
                            statusBar.ForegroundColor = Colors.White;
                            statusBar.BackgroundOpacity = 1; // 透明度
                        }
                        else
                        {
                            StatusBar statusBar = StatusBar.GetForCurrentView();
                            statusBar.BackgroundColor = AccentColor.FallbackColor;
                            statusBar.ForegroundColor = Colors.Black;
                            statusBar.BackgroundOpacity = 1; // 透明度
                        }
                    }
                    catch
                    {
                        AcrylicBrush AccentColor = (AcrylicBrush)Windows.UI.Xaml.Application.Current.Resources["SystemControlChromeMediumLowAcrylicElementMediumBrush"];
                        if (IsDark)
                        {
                            StatusBar statusBar = StatusBar.GetForCurrentView();
                            statusBar.BackgroundColor = AccentColor.FallbackColor;
                            statusBar.ForegroundColor = Colors.White;
                            statusBar.BackgroundOpacity = 1; // 透明度
                        }
                        else
                        {
                            StatusBar statusBar = StatusBar.GetForCurrentView();
                            statusBar.BackgroundColor = AccentColor.FallbackColor;
                            statusBar.ForegroundColor = Colors.Black;
                            statusBar.BackgroundOpacity = 1; // 透明度
                        }
                    }
                }
                else if (IsDark)
                {
                    ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                    view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                    view.ButtonForegroundColor = Colors.White;
                }
                else
                {
                    ApplicationViewTitleBar view = ApplicationView.GetForCurrentView().TitleBar;
                    view.ButtonBackgroundColor = view.InactiveBackgroundColor = view.ButtonInactiveBackgroundColor = Colors.Transparent;
                    view.ButtonForegroundColor = Colors.Black;
                }
            }
        }

        public static async Task<bool> CheckLoginInfo()
        {
            using (Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter())
            {
                Windows.Web.Http.HttpCookieManager cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (Windows.Web.Http.HttpCookie item in cookieManager.GetCookies(new Uri("http://coolapk.com")))
                {
                    switch (item.Name)
                    {
                        case "uid":
                            uid = item.Value;
                            break;
                        case "username":
                            userName = item.Value;
                            break;
                        case "token":
                            token = item.Value;
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userName))
                {
                    cookie = $"uid={uid}; username={userName}; token={token}";
                    Set("Uid", uid);
                    JsonObject json = UIHelper.GetJSonObject(await UIHelper.GetJson("/account/checkLoginInfo"));
                    if (json != null)
                    {
                        if (json.TryGetValue("notifyCount", out IJsonValue notifyCount) && !string.IsNullOrEmpty(notifyCount.GetObject().ToString()))
                        {
                            UIHelper.notifications.Initial(notifyCount.GetObject());
                        }
                        if (json.TryGetValue("userAvatar", out IJsonValue userAvatar) && !string.IsNullOrEmpty(userAvatar.GetString()))
                        {
                            UIHelper.mainPage.UserAvatar = ImageCache.defaultNoAvatarUrl.Contains(userAvatar.GetString()) ? ImageCache.NoPic : await ImageCache.GetImage(ImageType.BigAvatar, userAvatar.GetString());
                        }
                        if (json.TryGetValue("username", out IJsonValue username) && !string.IsNullOrEmpty(username.GetString()))
                        {
                            UIHelper.mainPage.UserNames = username.GetString();
                        }
                    }
                    else
                    {
                        UIHelper.mainPage.UserAvatar = ImageCache.NoPic;
                        UIHelper.mainPage.UserNames = "网络错误";
                    }
                    return true;
                }
                else
                {
                    await Task.Delay(1);//等待一下
                    UIHelper.mainPage.UserAvatar = null;
                    UIHelper.mainPage.UserNames = "登录";
                    return false;
                }
            }
        }

        public static void Logout()
        {
            Windows.Web.Http.HttpCookieManager cookieManager = new Windows.Web.Http.Filters.HttpBaseProtocolFilter().CookieManager;
            foreach (Windows.Web.Http.HttpCookie item in cookieManager.GetCookies(new Uri("http://coolapk.com")))
            { cookieManager.DeleteCookie(item); }
            cookie = string.Empty;
            Set("Uid", string.Empty);
            UIHelper.mainPage.UserAvatar = null;
            UIHelper.mainPage.UserNames = "登录";
        }
    }
}
