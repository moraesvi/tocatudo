using MarcTron.Plugin;
using Plugin.Connectivity;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace TocaTudoPlayer.Xamarim
{
    public class CustomCrossMTAdmob
    {
        private static INavigation _navigation;
        public static void Init(INavigation navigation)
        {
            _navigation = navigation;
        }
        public static async Task ShowInterstitial(Action intertistialNotLoaded, Action callbackOnIntertistialIsOpened)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                AppHelper.HasInterstitialToShow = false;
                AppHelper.MusicPlayerInterstitialIsLoadded = false;
                return;
            }

            AppHelper.MusicPlayerInterstitialIsLoadded = CrossMTAdmob.Current.IsInterstitialLoaded();
            await LoadingAdMsg(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (AppHelper.MusicPlayerInterstitialIsLoadded)
                    {
                        CrossMTAdmob.Current.ShowInterstitial();
                        CrossMTAdmob.Current.OnInterstitialOpened += (sender, e) =>
                        {
                            AppHelper.MusicPlayerInterstitialIsLoadded = true;
                            callbackOnIntertistialIsOpened();
                        };
                    }
                    else
                    {
                        AppHelper.HasInterstitialToShow = false;
                        intertistialNotLoaded();
                    }
                });
            });

        }
        public static async Task LoadAndShowInterstitial(string adUnit, Action intertistialNotLoaded, Action callbackOnIntertistialIsOpened)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                AppHelper.HasInterstitialToShow = false;
                AppHelper.MusicPlayerInterstitialIsLoadded = false;
                return;
            }

            CrossMTAdmob.Current.LoadInterstitial(adUnit);
            await LoadingAdMsg(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AppHelper.MusicPlayerInterstitialIsLoadded = CrossMTAdmob.Current.IsInterstitialLoaded();
                    if (AppHelper.MusicPlayerInterstitialIsLoadded)
                    {
                        CrossMTAdmob.Current.OnInterstitialOpened += (sender, e) =>
                        {
                            AppHelper.MusicPlayerInterstitialIsLoadded = true;
                            callbackOnIntertistialIsOpened();
                        };
                        CrossMTAdmob.Current.ShowInterstitial();
                    }
                    else
                    {
                        AppHelper.HasInterstitialToShow = false;
                        intertistialNotLoaded();
                    }
                });
            });
        }
        private static async Task LoadingAdMsg(Action callback)
        {
            LoadingControlPopup popup = new LoadingControlPopup()
            {
                IsLightDismissEnabled = false
            };

            popup.Dismissed += (sender, e) =>
            {
                callback();
            };

            await _navigation.ShowPopupAsync(popup);
        }
    }
}
