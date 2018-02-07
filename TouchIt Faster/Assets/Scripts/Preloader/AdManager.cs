using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;


public class AdManager : MonoBehaviour
{

    public static AdManager Instance { set; get; }
    private string AdUnitIDVideo = "ca-app-pub-1020122764598647/6260266886";
    private string AdUnitIDBanner = "ca-app-pub-1020122764598647/6312237775";


    private BannerView CurrBanner = null;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //showBannerAd();

        //Request Ad
       // RequestInterstitialAds();
    }

    private void showBannerAd()
    {
        return;
        string adID = "ca-app-pub-3940256099942544/6300978111";

        //***For Testing in the Device***
        AdRequest request = new AdRequest.Builder()
       .AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
       .AddTestDevice("2077ef9a63d2b398840261c8221a0c9b")  // My test device.
       .Build();

        //***For Production When Submit App***
        //AdRequest request = new AdRequest.Builder().Build();

        CurrBanner = new BannerView(adID, AdSize.SmartBanner, AdPosition.Bottom);
        CurrBanner.LoadAd(request);
    }

    public void DestroyBanner()
    {
        if(CurrBanner!=null)
            CurrBanner.Destroy();
    }



    public void showInterstitialAd()
    {
        return;
        //Show Ad
        if (interstitial.IsLoaded())
        {
            interstitial.Show();

            //Stop Sound
            //

            Debug.Log("SHOW AD XXX");
        }

    }

    InterstitialAd interstitial;
    private void RequestInterstitialAds()
    {
        return;
        string adID = "ca-app-pub-3940256099942544/1033173712";

#if UNITY_ANDROID
        string adUnitId = adID;
#elif UNITY_IOS
        string adUnitId = adID;
#else
        string adUnitId = adID;
#endif

        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(adUnitId);

        //***Test***
        AdRequest request = new AdRequest.Builder()
       .AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
       .AddTestDevice("2077ef9a63d2b398840261c8221a0c9b")  // My test device.
       .Build();

        //***Production***
        //AdRequest request = new AdRequest.Builder().Build();

        //Register Ad Close Event
        interstitial.OnAdClosed += Interstitial_OnAdClosed;

        // Load the interstitial with the request.
        interstitial.LoadAd(request);

        Debug.Log("AD LOADED XXX");

    }

    //Ad Close Event
    private void Interstitial_OnAdClosed(object sender, System.EventArgs e)
    {
        //Resume Play Sound

    }
}
