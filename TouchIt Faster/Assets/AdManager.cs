using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;


public class AdManager : MonoBehaviour
{

    public static AdManager Instance { set; get; }
    private string AdUnitIDVideo = "ca-app-pub-1020122764598647/6260266886";
    private string AdUnitIDBanner = "ca-app-pub-1020122764598647/6312237775";

    // Use this for initialization
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        showBannerAd();
        //#if UNITY_EDITOR
        //#elif UNITY_ANDROID
        //        Admob.Instance().initAdmob( AdUnitIDBanner, AdUnitIDVideo);
        //        Admob.Instance().loadInterstitial();
        //#endif
    }

    public void ShowBanner()
    {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId =AdUnitIDBanner;
#elif UNITY_IPHONE
        string adUnitId = "INSERT_IOS_BANNER_AD_UNIT_ID_HERE";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        BannerView bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

    private void showBannerAd()
    {
        string adID = "ca-app-pub-3940256099942544/6300978111";

        //***For Testing in the Device***
        AdRequest request = new AdRequest.Builder()
       .AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
       .AddTestDevice("2077ef9a63d2b398840261c8221a0c9b")  // My test device.
       .Build();

        //***For Production When Submit App***
        //AdRequest request = new AdRequest.Builder().Build();

        BannerView bannerAd = new BannerView(adID, AdSize.SmartBanner, AdPosition.Top);
        bannerAd.LoadAd(request);
    }
}
