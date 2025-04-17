﻿//using com.adjust.sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static MaxSdkBase;
//using com.adjust.sdk;

public class AdmobAds : MonoBehaviour
{
    public bool offBanner;

    public float countdownAds;
    public float countdownAdsOpenAppAds;
    public bool IsMRecReady;
    public bool wasShowMer;
    private bool _isInited;
    public bool canShowOpenAppAds;
    public bool wasShowOpenAppAdsInGame;
    public bool lockShowOpenAppAds;
    public int coutOpenAdsLoad;
 
    public bool showingMREC;
#if UNITY_ANDROID
    private const string MaxSdkKey = "izbW4oEiJA_cdTh6wc0r6Cqyel80b8VaLe1pL0pAKx7TvV9BoLk4F29V4R3OUqiynDPwowsUIsszEb66mbssOZ";
    private const string InterstitialAdUnitId = "046ee05ba30fcde2";
    private const string RewardedAdUnitId = "1e308facd5cc01c1";
    private const string BanerAdUnitId = "42afee22aa89924a";
    //private string AppOpenId = "45fa180466aff54e";
    //private const string MREC_Id = "9569f2b411b0eddf";

#elif UNITY_IOS
    private const string MaxSdkKey = "oL9CCQ2BlEl-78avNTt7qkDU1Tl_Pkb2pgv9g9m5cLUiiJcxwGv_2T1_T9OsBIhSM4UPhnkDXyMO8HNGmFnEKu";
    private const string InterstitialAdUnitId = "1382d286409432c4";
    private const string RewardedAdUnitId = "cee6de95dc42bddf";
    private const string BanerAdUnitId = "eeb72552f04456e7";
    private string AppOpenId = "45fa180466aff54e";
    private const string MREC_Id = "9569f2b411b0eddf";
#endif
    public void Init()
    {
        coutOpenAdsLoad = 0;
        lockShowOpenAppAds = false;
        canShowOpenAppAds = false;
        wasShowOpenAppAdsInGame = false;
        IsMRecReady = false;
        countdownAds = 10000;
        countdownAdsOpenAppAds = 1000;
        #region Applovin Ads
        CheckResetCaping();
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {

            InitInterstitial();
            InitRewardVideo();
            InitializeBannerAds();
            //InitializeMRecAds();
            //InitializeOpenAppAds();

            // MaxSdk.ShowMediationDebugger();
        };
        MaxSdk.SetVerboseLogging(true);
        MaxSdk.SetSdkKey(MaxSdkKey);
        MaxSdk.InitializeSdk();
        #endregion
        _isInited = true;
        //Debug.LogError("AppOpenId" + AppOpenId);
    }

    #region Interstitial

    public UnityAction actionInterstitialClose;

    public int amountInterClick
    {
        get
        {
            return PlayerPrefs.GetInt("Amount_Inter_Click", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Amount_Inter_Click", value);
        }
    }

    public int amountLoadFailInter
    {
        get
        {
            return PlayerPrefs.GetInt("Amount_Load_Fail_Inter", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Amount_Load_Fail_Inter", value);
        }
    }

    public DateTime timeLoadFailInter
    {
        get
        {
            var timeLoad = DateTime.Now.AddSeconds(0);
            if (PlayerPrefs.HasKey("Time_Load_Fail_Inter"))
            {
                var binaryDateTime = long.Parse(PlayerPrefs.GetString("Time_Load_Fail_Inter"));
                timeLoad = DateTime.FromBinary(binaryDateTime);
            }

            return timeLoad;
        }
        set
        {
            PlayerPrefs.SetString("Time_Load_Fail_Inter", DateTime.Now.ToBinary().ToString());
        }
    }

    private bool _isLoading;
    private int errorCodeLoadFail_Inter;

    public bool IsLoadedInterstitial()
    {
        return MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
    }

    private void InitInterstitial()
    {
        MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.OnInterstitialClickedEvent += MaxSdkCallbacks_OnInterstitialClickedEvent;
        MaxSdkCallbacks.OnInterstitialDisplayedEvent += MaxSdkCallbacks_OnInterstitialDisplayedEvent;

        RequestInterstitial();

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        // MaxSdkCallbacks.

    }

    public void ShowInterstitial(bool isShowImmediatly = false, string actionWatchLog = "other", UnityAction actionIniterClose = null, string level = null)
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            if (actionIniterClose != null)
                actionIniterClose();
            return;
        }


        if (isShowImmediatly)
        {
          
            ShowInterstitialHandle(isShowImmediatly, actionWatchLog, actionIniterClose, level);
        }
        else
        {


            if (UseProfile.CurrentLevel >= RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_INITSTIALL, 1))
            {

                Debug.LogError("ShowInterstitialHandle_" + RemoteConfigController.GetFloatConfig(FirebaseConfig.DELAY_SHOW_INITSTIALL, 90));
                if (countdownAds > RemoteConfigController.GetFloatConfig(FirebaseConfig.DELAY_SHOW_INITSTIALL, 90))
                {
                    ShowInterstitialHandle(isShowImmediatly, actionWatchLog, actionIniterClose, level);

                    Debug.LogError("ShowInterstitialHandle");
                }
                else
                {
                    if (actionIniterClose != null)
                        actionIniterClose();
                }
            }
            else
            {
                if (actionIniterClose != null)
                    actionIniterClose();
            }
        }

    }

    private void ShowInterstitialHandle(bool isShowImmediatly = false, string actionWatchLog = "other", UnityAction actionIniterClose = null, string level = null)
    {
        lockShowOpenAppAds = true;
        if (IsLoadedInterstitial())
        {
            this.actionInterstitialClose = actionIniterClose;
            MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);

            if (!isShowImmediatly)
                countdownAds = 0;

            //GameController.Instance.AnalyticsController.LogInterShow(actionWatchLog);
            GameController.Instance.AnalyticsController.LogInterShow();
            UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
            UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;

        }
        else
        {
            if (actionIniterClose != null)
                actionIniterClose();
            RequestInterstitial();

        }

    }

    private void RequestInterstitial()
    {
        if (_isLoading) return;

        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
        GameController.Instance.AnalyticsController.LogInterLoad();
        _isLoading = true;
    }

    #endregion

    #region Video Reward
    private UnityAction _actionClose;
    private UnityAction _actionRewardVideo;
    private UnityAction _actionNotLoadedVideo;
    private ActionWatchVideo actionWatchVideo;

    public int amountVideoRewardClick
    {
        get
        {
            return PlayerPrefs.GetInt("Amount_VideoReward_Click", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Amount_VideoReward_Click", value);
        }
    }
    private int numRequestedInScene_Video;

    private bool isVideoDone;

    private void InitRewardVideo()
    {
        InitializeRewardedAds();
    }

    public bool IsLoadedVideoReward()
    {
        var result = MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
        if (!result)
        {
            RequestInterstitial();
        }
        return result;
    }

    public bool IsLoadedAds()
    {
        var result = IsLoadedVideoReward();
        return !result ? IsLoadedInterstitial() : result;
    }

    public bool ShowVideoReward(UnityAction actionReward, UnityAction actionNotLoadedVideo, UnityAction actionClose, ActionWatchVideo actionType, string level)
    {
        lockShowOpenAppAds = true;
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            actionNotLoadedVideo?.Invoke();
            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, false, level);
            return false;
        }
        actionWatchVideo = actionType;
        GameController.Instance.AnalyticsController.LogRequestVideoReward(actionType.ToString());
     
        if (IsLoadedVideoReward())
        {

            countdownAds = 0;
            this._actionNotLoadedVideo = actionNotLoadedVideo;
            this._actionClose = actionClose;
            this._actionRewardVideo = actionReward;

            MaxSdk.ShowRewardedAd(RewardedAdUnitId, actionType.ToString());
            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, true, level);
            GameController.Instance.AnalyticsController.LogVideoRewardShow(actionWatchVideo.ToString());
            GameController.Instance.AnalyticsController.LogVideoRewardShow();

        }
        else
        {
            if (IsLoadedInterstitial())
            {
                this._actionNotLoadedVideo = actionNotLoadedVideo;
                this._actionClose = actionClose;
                this._actionRewardVideo = actionReward;

                ShowInterstitial(isShowImmediatly: true, actionType.ToString(), actionIniterClose: () => { }, level);
                GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, true, level);
                Debug.Log("ShowInterstitial !!!");
                countdownAds = 0;
                return true;
            }
            else
            {
                //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
                Debug.Log("No ads !!!");
                actionNotLoadedVideo?.Invoke();
                GameController.Instance.AnalyticsController.LogWatchVideo(actionType, false, true, level);
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Applovin Rewards Ads
    private void InitializeRewardedAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        // Load the first RewardedAd
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId)
    {
        GameController.Instance.AnalyticsController.LogVideoRewardReady();
    }

    private void OnRewardedAdFailedEvent(string adUnitId, int errorCode)
    {
        Debug.Log("Rewarded ad failed to load with error code: " + errorCode);
        Invoke("LoadRewardedAd", 15);
        GameController.Instance.AnalyticsController.LogVideoRewardLoadFail(actionWatchVideo.ToString(), errorCode.ToString());
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        Debug.Log("Rewarded ad failed to display with error code: " + errorCode);
        isVideoDone = false;

        //if (IsLoadedInterstitial())
        //{
        //    ShowInterstitial(isShowImmediatly: true);
        //}
        //else
        //{
        //    //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
        //}
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId)
    {
        Debug.Log("Rewarded ad displayed " + isVideoDone);
        GameController.Instance.AnalyticsController.HandleFireEvent_Total_Reward_Count();
        isVideoDone = false;
    }

    private void OnRewardedAdClickedEvent(string adUnitId)
    {
        amountVideoRewardClick++;
        Debug.Log("Rewarded ad clicked");
        isVideoDone = true;
        GameController.Instance.AnalyticsController.LogClickToVideoReward(actionWatchVideo.ToString());
    }

    private void OnRewardedAdDismissedEvent(string adUnitId)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        lockShowOpenAppAds = false;
        Debug.Log("Rewarded ad dismissed");
        _actionClose?.Invoke();
        _actionClose = null;
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward)
    {
        // Rewarded ad was displayed and user should receive the reward
        Debug.Log("Rewarded ad received reward");
        isVideoDone = true;
        _actionRewardVideo?.Invoke();
        _actionRewardVideo = null;
        countdownAds = 0;
        GameController.Instance.AnalyticsController.LogVideoRewardShowDone(actionWatchVideo.ToString());
    }
    #endregion

    #region Applovin Interstitial
    private void OnInterstitialLoadedEvent(string adUnitId)
    {
        _isLoading = true;
        GameController.Instance.AnalyticsController.LogInterReady();
    }

    private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
    {
        _isLoading = false;
        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;
        Invoke("RequestInterstitial", 3);

        errorCodeLoadFail_Inter = errorCode;
        GameController.Instance.AnalyticsController.LogInterLoadFail(errorCodeLoadFail_Inter.ToString());
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        _isLoading = false;
        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;
        RequestInterstitial();
    }

    private void OnInterstitialHiddenEvent(string adUnitId)
    {
        _isLoading = false;
        Debug.Log("InterstitialAdClosedEvent");
        Time.timeScale = 1;

        _actionRewardVideo?.Invoke();
        _actionRewardVideo = null;

        _actionClose?.Invoke();
        _actionClose = null;

        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;
        lockShowOpenAppAds = false;
        RequestInterstitial();
    }
    private void MaxSdkCallbacks_OnInterstitialDisplayedEvent(string adUnitId)
    {
        //if (UseProfile.RetentionD <= 1)
        //{
        //    UseProfile.NumberOfDisplayedInterstitialD0_D1++;
        //}
        //GameController.Instance.AnalyticsController.LogDisplayedInterstitialDay01();
        Debug.Log("InterstitialAdOpenedEvent");
        GameController.Instance.AnalyticsController.HandleFireEvent_Total_Inter_Count();
        _isLoading = false;
        Time.timeScale = 0;
    }

    private void MaxSdkCallbacks_OnInterstitialClickedEvent(string adUnitId)
    {
        amountInterClick++;
        GameController.Instance.AnalyticsController.LogInterClick();
        _isLoading = false;
    }
    #endregion

    #region Applovin Baner


    public int amountBanerClick
    {
        get
        {
            return PlayerPrefs.GetInt("Amount_Baner_Click", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Amount_Baner_Click", value);
        }
    }

    public int amountLoadFailBaner
    {
        get
        {
            return PlayerPrefs.GetInt("Amount_Load_Fail_Baner", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Amount_Load_Fail_Baner", value);
        }
    }

    public DateTime timeLoadFailBaner
    {
        get
        {
            var timeLoad = DateTime.Now.AddSeconds(0);
            if (PlayerPrefs.HasKey("Time_Load_Fail_Baner"))
            {
                var binaryDateTime = long.Parse(PlayerPrefs.GetString("Time_Load_Fail_Baner"));
                timeLoad = DateTime.FromBinary(binaryDateTime);
            }

            return timeLoad;
        }
        set
        {
            PlayerPrefs.SetString("Time_Load_Fail_Baner", DateTime.Now.ToBinary().ToString());
        }
    }

    private IEnumerator reloadBannerCoru;

    public void InitializeBannerAds()
    {
        MaxSdkCallbacks.OnBannerAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.OnBannerAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.OnBannerAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

        MaxSdk.CreateBanner(BanerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerExtraParameter(BanerAdUnitId, "adaptive_banner", "true");
        MaxSdk.SetBannerBackgroundColor(BanerAdUnitId, Color.black);
        MaxSdk.SetBannerWidth(BanerAdUnitId, 520);

        GameController.Instance.admobAds.ShowBanner();
    }
   
    private void OnBannerAdClickedEvent(string obj)
    {
        //inter click
        Debug.Log("Click Baner !!!");
        amountBanerClick++;

    }

    private void OnBannerAdLoadFailedEvent(string arg1, int arg2)
    {
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }
        reloadBannerCoru = Helper.StartAction(() => { ShowBanner(); }, 0.3f);
        StartCoroutine(reloadBannerCoru);
    }

    private void OnBannerAdLoadedEvent(string obj)
    {
        Debug.Log("Request success");
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }
       
    }

    public void DestroyBanner()
    {
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }
        MaxSdk.HideBanner(BanerAdUnitId);
    }

    public void ShowBanner()
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            return;
        }

        if (wasShowMer)
        {
            return;
        }



        MaxSdk.ShowBanner(BanerAdUnitId);
    }


    #endregion

    #region Limit Click
    public DateTime ToDayAds
    {
        get
        {
            if (!PlayerPrefs.HasKey("TODAY_ADS"))
                PlayerPrefs.SetString("TODAY_ADS", DateTime.Now.AddDays(-1).ToString());
            return DateTime.Parse(PlayerPrefs.GetString("TODAY_ADS"));
        }
        set
        {
            PlayerPrefs.SetString("TODAY_ADS", value.ToString());
        }
    }

    public void CheckResetCaping()
    {
        // bool isPassday = TimeManager.IsPassTheDay(ToDayAds, DateTime.Now);
        // if (isPassday)
        {
            amountLoadFailInter = 0;
            amountLoadFailBaner = 0;
            amountInterClick = 0;
            amountBanerClick = 0;
            amountVideoRewardClick = 0;
            ToDayAds = DateTime.Now;
        }
    }
    #endregion

    private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
    

        double revenue = impressionData.Revenue;
        var impressionParameters = new[] {
    new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
    new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
    new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
    new Firebase.Analytics.Parameter("value", revenue),
    new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
};
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_max", impressionParameters);
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
    

 
    }

    private void OnLevelWasLoaded(int level)
    {
        _actionRewardVideo = null;
        _actionClose = null;
        actionInterstitialClose = null;
    }

    private void Update()
    {
        countdownAds += Time.unscaledDeltaTime;
        countdownAdsOpenAppAds += Time.unscaledTime;
    }

    //public bool IsOpenAdsReady
    //{
    //    get
    //    {
    //        return MaxSdk.IsAppOpenAdReady(AppOpenId);
    //    }

    //}
    //public void InitializeOpenAppAds()
    //{
    //    MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += delegate { };
    //    MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += delegate { };
    //    MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += delegate { MaxSdk.LoadAppOpenAd(AppOpenId); };
    //    MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
    //    MaxSdk.LoadAppOpenAd(AppOpenId);

    //}

    //public void LoadOpenAdsIfFalse()
    //{
    //    if (!IsOpenAdsReady)
    //    {
    //        MaxSdk.LoadAppOpenAd(AppOpenId);

    //    }

    //}
    //public void ShowOpenAppAdsReady()
    //{
    //    if (GameController.Instance.useProfile.IsRemoveAds)
    //    {
    //        return;
    //    }

    //    if (!UseProfile.FirstShowOpenAds)
    //    {

    //        UseProfile.FirstShowOpenAds = true;
    //    }
    //    else
    //    {
    //        if (RemoteConfigController.GetBoolConfig(FirebaseConfig.SHOW_OPEN_ADS, true))
    //        {
    //            if (MaxSdk.IsAppOpenAdReady(AppOpenId))
    //            {
    //                MaxSdk.ShowAppOpenAd(AppOpenId);
    //                countdownAdsOpenAppAds = 0;
    //                Debug.LogError("SHOW_OPEN_ADS");
    //            }
    //            else
    //            {
    //                MaxSdk.LoadAppOpenAd(AppOpenId);
    //            }
    //        }
    //        Debug.LogError("FirstShowOpenAds_2");
    //    }



    //}

    //public void ShowOpenAppAdsInGame()
    //{
    //    if (wasShowOpenAppAdsInGame == false)
    //    {
    //        ShowOpenAppAdsReady();
    //        wasShowOpenAppAdsInGame = true;
    //    }

    //}
    //public void OnApplicationPause(bool pause)
    //{

    //    if (!pause)
    //    {

    //        if (canShowOpenAppAds)
    //        {

    //            if (lockShowOpenAppAds == false)
    //            {
    //                ShowOpenAppAdsReady();

    //            }

    //        }
    //    }

    //}
    //public void InitializeMRecAds()
    //{
    //    // MRECs are sized to 300x250 on phones and tablets
    //    MaxSdk.CreateMRec(MREC_Id, MaxSdkBase.AdViewPosition.BottomCenter);

    //    MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
    //    MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
    //    MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
    //    MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
    //    MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
    //    MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;


    //}

    public void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        IsMRecReady = true;

    }

    public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
    {

        IsMRecReady = false;


    }

    public void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    public void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    public void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }


    //public void HandleShowMerec()
    //{
    //    if (GameController.Instance.useProfile.IsRemoveAds)
    //    {
    //        return;
    //    }

    //    DestroyBanner();
    //    MaxSdk.ShowMRec(MREC_Id);
    //    showingMREC = true;
    //}
    //public void HandleHideMerec()
    //{
    //    if (showingMREC)
    //    {
    //        MaxSdk.HideMRec(MREC_Id);
    //        ShowBanner();
    //        showingMREC = false;
    //    }

    //}

}
