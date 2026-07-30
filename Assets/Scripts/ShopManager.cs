using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
	[Serializable]
	public struct PackProduct
	{
		public string id;

		/// <summary>Number of ads the player must watch to unlock this pack.</summary>
		public int adsRequired;

		public Text priceLabel;

		public GameObject target;
	}

	[Serializable]
	public struct CoinProduct
	{
		public int value;

		public int price;

		public Text priceLabel;

		public Text valueLabel;
	}

	[Serializable]
	public struct BoostProduct
	{
		public int time;

		public int value;

		public int price;

		public Text priceLabel;

		public Text valueLabel;
	}

	[Serializable]
	public struct DiamondProduct
	{
		public string id;

		/// <summary>Always 1 — kept for Inspector compatibility. 1 ad = 1 diamond.</summary>
		public int value;

		public Text valueLabel;

		public Text priceLabel;
	}

	[SerializeField]
	private ShopManager.PackProduct[] packProduct;

	[SerializeField]
	private ShopManager.CoinProduct[] coinProduct;

	[SerializeField]
	private ShopManager.BoostProduct[] boostProduct;

	[SerializeField]
	private ShopManager.DiamondProduct[] diamondProduct;

	[SerializeField]
	private RectTransform shopRectransform;

	[SerializeField]
	private GameObject packHeader;

	[SerializeField]
	private GameObject targetPopup;

	[SerializeField]
	private GameManager gameManager;

	[SerializeField]
	private BoostManager boostManager;

	[SerializeField]
	private CoinItemPool coinItemPool;

	[SerializeField]
	private Transform coinTargetLabel;

	private void Start()
	{
		this.LoadDefaultPackProductPrice();
		this.LoadDefaultDiamondProductPrice();
		this.LoadDefaultBoostProductPrice();
		this.LoadDefaultCoinProductPrice();
	}

	// ── Price label loaders ──────────────────────────────────────────────────

	private void LoadDefaultPackProductPrice()
	{
		for (int i = 0; i < this.packProduct.Length; i++)
		{
			if (Singleton<DataManager>.Instance.database.nonConsume.Contains(this.packProduct[i].id))
			{
				this.packProduct[i].target.SetActive(false);
			}
			else
			{
				int required = Mathf.Max(1, this.packProduct[i].adsRequired);
				string progressKey = "PackAdProgress_" + this.packProduct[i].id;
				int current = PlayerPrefs.GetInt(progressKey, 0);
				GameUtilities.String.ToText(this.packProduct[i].priceLabel, current + "/" + required + " ads");
			}
		}
		if (Singleton<DataManager>.Instance.database.nonConsume.Count == this.packProduct.Length)
		{
			this.packHeader.SetActive(false);
		}
	}

	private void LoadDefaultCoinProductPrice()
	{
		for (int i = 0; i < this.coinProduct.Length; i++)
		{
			GameUtilities.String.ToText(this.coinProduct[i].valueLabel, GameUtilities.Currencies.Convert(this.GetInstantCash() * (double)this.coinProduct[i].value));
			GameUtilities.String.ToText(this.coinProduct[i].priceLabel, this.coinProduct[i].price.ToString());
		}
	}

	private void LoadDefaultBoostProductPrice()
	{
		for (int i = 0; i < this.boostProduct.Length; i++)
		{
			GameUtilities.String.ToText(this.boostProduct[i].valueLabel, "+" + GameUtilities.DateTime.Convert(this.boostProduct[i].time));
			GameUtilities.String.ToText(this.boostProduct[i].priceLabel, this.boostProduct[i].price.ToString());
		}
	}

	private void LoadDefaultDiamondProductPrice()
	{
		for (int i = 0; i < this.diamondProduct.Length; i++)
		{
			// 1 ad = 1 diamond
			GameUtilities.String.ToText(this.diamondProduct[i].priceLabel, "1 ad");
			GameUtilities.String.ToText(this.diamondProduct[i].valueLabel, "+1");
		}
	}

	// ── Buy actions ──────────────────────────────────────────────────────────

	/// <summary>
	/// Watch a rewarded ad to progress toward unlocking this pack.
	/// Each ad watch counts as 1. Once the player has watched <c>adsRequired</c>
	/// ads for this pack the pack is unlocked (non-consumable, one-time only).
	/// </summary>
	public void BuyPack(int index)
	{
		if (Singleton<DataManager>.Instance.database.nonConsume.Contains(this.packProduct[index].id))
		{
			// Already owned — nothing to do.
			return;
		}

		if (!AdsControl.Instance.GetRewardAvailable())
		{
			Notification.instance.Warning("No available video at the moment.");
			Singleton<SoundManager>.Instance.Play("Notification");
			return;
		}

		int required = Mathf.Max(1, this.packProduct[index].adsRequired);
		string progressKey = "PackAdProgress_" + this.packProduct[index].id;

		AdsControl.Instance.PlayDelegateRewardVideo(delegate(bool rewarded)
		{
			if (!rewarded) return;

			int current = PlayerPrefs.GetInt(progressKey, 0) + 1;
			PlayerPrefs.SetInt(progressKey, current);

			if (current >= required)
			{
				// All required ads watched — unlock the pack.
				PlayerPrefs.SetInt(progressKey, 0);

				if (index == 0)
				{
					Singleton<DataManager>.Instance.database.nonConsume.Add(this.packProduct[index].id);
					this.packProduct[index].target.SetActive(false);
				}
				else if (index == 1)
				{
					Singleton<DataManager>.Instance.database.nonConsume.Add(this.packProduct[index].id);
					this.boostManager.TotalEffectiveCompute();
					this.packProduct[index].target.SetActive(false);
				}

				Singleton<SoundManager>.Instance.Play("Purchased");
				Notification.instance.Warning("Pack unlocked!");

				if (Singleton<DataManager>.Instance.database.nonConsume.Count == this.packProduct.Length)
				{
					this.packHeader.SetActive(false);
				}

				Tracking.instance.IAP(this.packProduct[index].id);
			}
			else
			{
				// Progress notification.
				Singleton<SoundManager>.Instance.Play("Rewarded");
				Notification.instance.Warning("Progress: " + current + "/" + required + " ads watched.");
			}

			// Refresh the progress label.
			this.LoadDefaultPackProductPrice();
		});
	}

	/// <summary>
	/// Watch a rewarded ad to receive 1 diamond.
	/// The <c>index</c> parameter selects the UI slot — all slots give 1 diamond per ad.
	/// </summary>
	public void BuyDiamond(int index)
	{
		if (!AdsControl.Instance.GetRewardAvailable())
		{
			Notification.instance.Warning("No available video at the moment.");
			Singleton<SoundManager>.Instance.Play("Notification");
			return;
		}

		AdsControl.Instance.PlayDelegateRewardVideo(delegate(bool rewarded)
		{
			if (!rewarded) return;

			this.gameManager.SetDiamond(1); // 1 ad = 1 diamond
			Notification.instance.Warning("Received <color=#00FFDFFF>1</color> diamond");
			Singleton<SoundManager>.Instance.Play("Purchased");
			Tracking.instance.IAP(this.diamondProduct[index].id);
		});
	}

	public void BuyCoin(int index)
	{
		if (Singleton<DataManager>.Instance.database.diamond < this.coinProduct[index].price)
		{
			Notification.instance.Warning("Not Enough Diamond");
			Singleton<SoundManager>.Instance.Play("Notification");
			return;
		}
		Notification.instance.Confirm(delegate
		{
			this.gameManager.SetDiamond(-this.coinProduct[index].price);
			double cash = this.GetInstantCash() * (double)this.coinProduct[index].value;
			this.coinItemPool.Pool(this.coinTargetLabel, cash);
			Singleton<SoundManager>.Instance.Play("Purchased");
		}, "Do you want to buy this item for <color=#00B5FFFF>" + this.coinProduct[index].price.ToString() + "</color> diamond ?");
	}

	public void BuyBoost(int index)
	{
		if (Singleton<DataManager>.Instance.database.diamond < this.boostProduct[index].price)
		{
			Notification.instance.Warning("Not Enough Diamond");
			Singleton<SoundManager>.Instance.Play("Notification");
			return;
		}
		Notification.instance.Confirm(delegate
		{
			this.gameManager.SetDiamond(-this.boostProduct[index].price);
			Item item = new Item();
			item.duration = this.boostProduct[index].time;
			item.effective = this.boostProduct[index].value;
			item.itemCount = 1;
			Singleton<Inventory>.Instance.Add(item);
			Singleton<SoundManager>.Instance.Play("Purchased");
		}, "Do you want to buy this item for <color=#00B5FFFF>" + this.boostProduct[index].price.ToString() + "</color> diamond ?");
	}

	// ── Utilities ────────────────────────────────────────────────────────────

	public void ShowPopup(bool value)
	{
		if (value)
		{
			Singleton<SoundManager>.Instance.Play("Popup");
			this.LoadDefaultCoinProductPrice();
		}
		this.targetPopup.SetActive(value);
	}

	public void MoveToBoth(RectTransform rectTransform)
	{
		this.shopRectransform.anchoredPosition = new Vector2(this.shopRectransform.anchoredPosition.x, -(rectTransform.anchoredPosition.y + 120f));
	}

	private double GetInstantCash()
	{
		double idleCash = Singleton<DataManager>.Instance.database.restaurant[Singleton<DataManager>.Instance.database.targetRestaurant].idleCash;
		return Singleton<GameProcess>.Instance.GetInstantCash(idleCash);
	}
}
