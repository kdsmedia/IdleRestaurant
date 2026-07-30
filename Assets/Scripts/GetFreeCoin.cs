using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GetFreeCoin : MonoBehaviour
{
    // Server time URL — replace with your own time API endpoint.
    // The endpoint must return a plain Unix timestamp (seconds since 1970-01-01 UTC).
    // If the server is unreachable the script falls back to device local time.
    private const string TIME_SERVER_URL = "https://worldtimeapi.org/api/timezone/Etc/UTC";

    private bool cooldown;
    private Coroutine cooldowing;
    private DateTime currentTime;
    private FreeCoinData freeCoinData;
    private WaitForSeconds waitForSeconds;

    [SerializeField] private Configuration config;
    [SerializeField] private Text watchAdsRemaining;
    [SerializeField] private GameObject watchAdsLabel;
    [SerializeField] private GameObject freeCashButton;
    [SerializeField] private GameObject watchAdsButton;
    [SerializeField] private GameObject[] notification;

    private void Start()
    {
        waitForSeconds = new WaitForSeconds(1f);
        freeCoinData = Singleton<DataManager>.Instance.database.freeCashData;
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        currentTime = DateTime.Now; // default fallback

        using (UnityWebRequest request = UnityWebRequest.Get(TIME_SERVER_URL))
        {
            request.timeout = 5; // seconds
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // worldtimeapi.org returns JSON; parse the "unixtime" field.
                string json = request.downloadHandler.text;
                string unixtimeKey = "\"unixtime\":";
                int idx = json.IndexOf(unixtimeKey);
                if (idx >= 0)
                {
                    int start = idx + unixtimeKey.Length;
                    int end = json.IndexOfAny(new char[] { ',', '}' }, start);
                    string unixStr = json.Substring(start, end - start).Trim();
                    double unixSeconds;
                    if (double.TryParse(unixStr, out unixSeconds))
                    {
                        currentTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                            .AddSeconds(unixSeconds)
                            .ToLocalTime();
                    }
                }
            }
            else
            {
                Debug.LogWarning("[GetFreeCoin] Could not reach time server (" + request.error + "). Using device time as fallback.");
                currentTime = DateTime.Now;
            }
        }

        // Check if a new day has passed since last free claim
        DateTime lastGetFree = Convert.ToDateTime(freeCoinData.lastTimeGetFree);
        bool newDay = (lastGetFree.Day != currentTime.Day || lastGetFree.Month != currentTime.Month);
        if (newDay && !Singleton<DataManager>.Instance.database.freeCashData.free)
        {
            Singleton<DataManager>.Instance.database.freeCashData.free = true;
        }

        // Reset watch-ad counter if cooldown has expired
        if (freeCoinData.watchAds == config.freeCash.watchAdLimited)
        {
            int elapsed = (int)currentTime.Subtract(Convert.ToDateTime(freeCoinData.lastTimeWatchAd)).TotalSeconds;
            if (elapsed >= config.freeCash.cooldownPerAds)
            {
                freeCoinData.watchAds = 0;
            }
        }

        FreeCashValidate();
    }

    private void FreeCashValidate()
    {
        freeCashButton.SetActive(freeCoinData.free);
        watchAdsButton.SetActive(!freeCoinData.free);

        for (int i = 0; i < notification.Length; i++)
        {
            notification[i].SetActive(freeCoinData.free);
        }

        bool limitReached = (freeCoinData.watchAds == config.freeCash.watchAdLimited);
        watchAdsLabel.SetActive(!limitReached);
        watchAdsRemaining.gameObject.SetActive(limitReached);

        if (limitReached && !cooldown)
        {
            cooldowing = StartCoroutine(Cooldown());
        }
    }

    public void GetFreeCash()
    {
        freeCoinData.free = false;
        freeCoinData.lastTimeGetFree = DateTime.Now.ToString();
        Singleton<GameManager>.Instance.SetDiamond(config.freeCash.diamondBonus);
        Notification.instance.Warning("Received <color=#00FFDFFF>" + config.freeCash.diamondBonus.ToString() + "</color> diamond");
        Singleton<SoundManager>.Instance.Play("Rewarded");
        FreeCashValidate();
    }

    public void WatchAdsFreeCash()
    {
        if (!AdsControl.Instance.GetRewardAvailable())
        {
            Notification.instance.Warning("No available video at the moment.");
            Singleton<SoundManager>.Instance.Play("Notification");
            return;
        }

        AdsControl.Instance.PlayDelegateRewardVideo(delegate
        {
            if (freeCoinData.watchAds == config.freeCash.watchAdLimited)
                return;

            freeCoinData.watchAds++;
            if (freeCoinData.watchAds == config.freeCash.watchAdLimited)
            {
                freeCoinData.lastTimeWatchAd = DateTime.Now.ToString();
            }

            Singleton<GameManager>.Instance.SetDiamond(config.freeCash.diamondBonus);
            Notification.instance.Warning("Received <color=#00FFDFFF>" + config.freeCash.diamondBonus.ToString() + "</color> diamond");
            Singleton<SoundManager>.Instance.Play("Rewarded");
            FreeCashValidate();
            Tracking.instance.Ads_Impress("reward", "GetFreeDiamond");
        });
    }

    private IEnumerator Cooldown()
    {
        cooldown = true;
        int elapsed = (int)DateTime.Now.Subtract(Convert.ToDateTime(freeCoinData.lastTimeWatchAd)).TotalSeconds;
        int duration = Mathf.Clamp(config.freeCash.cooldownPerAds - elapsed, 0, config.freeCash.cooldownPerAds);

        while (duration > 0)
        {
            GameUtilities.String.ToText(watchAdsRemaining, GameUtilities.DateTime.Convert(duration));
            yield return waitForSeconds;
            duration--;
        }

        cooldown = false;
        watchAdsLabel.SetActive(true);
        watchAdsRemaining.gameObject.SetActive(false);
        freeCoinData.watchAds = 0;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            if (cooldown)
            {
                cooldown = false;
                StopCoroutine(cooldowing);
            }
        }
        else
        {
            StartCoroutine(Initialize());
        }
    }
}
