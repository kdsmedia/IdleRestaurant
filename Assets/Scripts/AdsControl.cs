using UnityEngine;
using System.Collections;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

/// <summary>
/// Manages all ads using Google AdMob only:
///   - Banner (bottom of screen)
///   - Interstitial (full-screen, shown every 3 actions)
///   - Rewarded Video
/// </summary>
public class AdsControl : MonoBehaviour
{
    protected AdsControl() { }

    private static AdsControl _instance;
    public static AdsControl Instance { get { return _instance; } }

    // ── Inspector fields ─────────────────────────────────────────────────────
    [Header("Interstitial Ad Unit IDs")]
    public string AdmobID_Android;
    public string AdmobID_IOS;

    [Header("Banner Ad Unit IDs")]
    public string BannerID_Android;
    public string BannerID_IOS;

    [Header("Rewarded Video Ad Unit IDs")]
    public string RewardVideoID_Android;
    public string RewardVideoID_IOS;

    // ── Private state ─────────────────────────────────────────────────────────
    private InterstitialAd interstitial;
    private RewardedAd      rewardedAd;
    private BannerView      bannerView;

    // Stored callback for reward video result
    private Action<bool> pendingRewardCallback;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (FindObjectsOfType(typeof(AdsControl)).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        MobileAds.Initialize(initStatus => { });   // Initialize AdMob SDK

        MakeNewInterstitial();
        RequestBanner();
        LoadRewardedAd();

        if (PlayerPrefs.GetInt("RemoveAds") == 0)
            ShowBanner();
        else
            HideBanner();

        DontDestroyOnLoad(gameObject);
    }

    // ── Interstitial ──────────────────────────────────────────────────────────
    private void MakeNewInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = AdmobID_Android;
#elif UNITY_IPHONE
        string adUnitId = AdmobID_IOS;
#else
        string adUnitId = "unused";
#endif
        if (interstitial != null)
            interstitial.Destroy();

        interstitial = new InterstitialAd(adUnitId);
        interstitial.OnAdClosed += HandleInterstitialAdClosed;
        interstitial.LoadAd(new AdRequest.Builder().Build());
    }

    private void HandleInterstitialAdClosed(object sender, EventArgs args)
    {
        interstitial.Destroy();
        MakeNewInterstitial();
    }

    /// <summary>Show interstitial every 3 calls (AdsCounter).</summary>
    public void showAds()
    {
        int adsCounter = PlayerPrefs.GetInt("AdsCounter");
        if (adsCounter >= 2)
        {
            if (PlayerPrefs.GetInt("RemoveAds") == 0 && interstitial != null && interstitial.IsLoaded())
                interstitial.Show();
            adsCounter = 0;
        }
        else
        {
            adsCounter++;
        }
        PlayerPrefs.SetInt("AdsCounter", adsCounter);
    }

    // ── Banner ────────────────────────────────────────────────────────────────
    private void RequestBanner()
    {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = BannerID_Android;
#elif UNITY_IPHONE
        string adUnitId = BannerID_IOS;
#else
        string adUnitId = "unexpected_platform";
#endif
        bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        bannerView.LoadAd(new AdRequest.Builder().Build());
    }

    public void ShowBanner()
    {
        if (bannerView != null) bannerView.Show();
    }

    public void HideBanner()
    {
        if (bannerView != null) bannerView.Hide();
    }

    // ── Rewarded Video ────────────────────────────────────────────────────────
    private void LoadRewardedAd()
    {
#if UNITY_ANDROID
        string adUnitId = RewardVideoID_Android;
#elif UNITY_IPHONE
        string adUnitId = RewardVideoID_IOS;
#else
        string adUnitId = "unused";
#endif
        if (rewardedAd != null)
            rewardedAd.Destroy();

        rewardedAd = new RewardedAd(adUnitId);

        // Fire true callback when reward is earned (fires before OnAdClosed)
        rewardedAd.OnUserEarnedReward += (sender, rewardArgs) =>
        {
            if (pendingRewardCallback != null)
            {
                pendingRewardCallback(true);
                pendingRewardCallback = null;
            }
        };

        // Fire false callback if closed without reward, then preload next ad
        rewardedAd.OnAdClosed += (sender, args) =>
        {
            if (pendingRewardCallback != null)
            {
                pendingRewardCallback(false);
                pendingRewardCallback = null;
            }
            LoadRewardedAd();   // Preload next rewarded ad
        };

        rewardedAd.LoadAd(new AdRequest.Builder().Build());
    }

    /// <summary>Returns true if a rewarded video is loaded and ready to show.</summary>
    public bool GetRewardAvailable()
    {
        return rewardedAd != null && rewardedAd.IsLoaded();
    }

    /// <summary>Show reward video. Callback: true = reward earned, false = skipped/failed.</summary>
    public void PlayDelegateRewardVideo(Action<bool> onVideoPlayed)
    {
        if (!GetRewardAvailable())
        {
            onVideoPlayed(false);
            return;
        }
        pendingRewardCallback = onVideoPlayed;
        rewardedAd.Show();
    }

    /// <summary>Show reward video without a result callback.</summary>
    public void ShowRewardVideo()
    {
        if (GetRewardAvailable())
            rewardedAd.Show();
    }

    // ── Misc ──────────────────────────────────────────────────────────────────
    public void ShowFB()
    {
        Application.OpenURL("https://www.facebook.com/PonyStudio2507/?ref=settings");
    }

    public void RateMyGame()
    {
#if UNITY_EDITOR
        Application.OpenURL("https://itunes.apple.com/us/app/color-flow-puzzle/id1436566275?ls=1&mt=8");
#elif UNITY_ANDROID
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.ponygames.MagicBlockPuzzle");
#elif UNITY_IPHONE
        Application.OpenURL("https://itunes.apple.com/us/app/color-flow-puzzle/id1436566275?ls=1&mt=8");
#else
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.ponygames.MagicBlockPuzzle");
#endif
    }
}
