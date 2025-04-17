using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "Datas/GiftDatabase", fileName = "GiftDatabase.asset")]
public class GiftDatabase : SerializedScriptableObject
{
    public Dictionary<GiftType, Gift> giftList;

    public bool GetGift(GiftType giftType, out Gift gift)
    {
        return giftList.TryGetValue(giftType, out gift);
    }

    public Sprite GetIconItem(GiftType giftType)
    {
        Gift gift = null;
        //if (IsCharacter(giftType))
        //{
        //    var Char = GameController.Instance.dataContain.dataSkins.GetSkinInfo(giftType);
        //    if (Char != null)
        //        return Char.iconSkin;
        //}
        bool isGetGift = GetGift(giftType, out gift);
        return isGetGift ? gift.getGiftSprite : null;
    }
    public GameObject GetAnimItem(GiftType giftType)
    {
        Gift gift = null;
        bool isGetGift = GetGift(giftType, out gift);
        return isGetGift ? gift.getGiftAnim : null;
    }

    public void Claim(GiftType giftType, int amount, Reason reason = Reason.none)
    {

        switch (giftType)
        {
            case GiftType.Coin:
                UseProfile.Coin += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_COIN);
                break;
            case GiftType.Heart:
                UseProfile.Heart += amount;
                break;
            case GiftType.RemoveAds:
                GameController.Instance.useProfile.IsRemoveAds = true;
           
                break;
            case GiftType.TNT_Booster:
                UseProfile.TNT_Booster += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_ATOM_BOOSTER);

                break;
            case GiftType.Rocket_Booster:
                UseProfile.Roket_Booster += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_ROCKET_BOOSTER);
                break;
            case GiftType.Freeze_Booster:
                UseProfile.Freeze_Booster += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_FREEZE_BOOSTER);
                break;

            case GiftType.Atom_Booster:
                UseProfile.Atom_Booster += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHANGE_ATOM_BOOSTER);
                break;

            case GiftType.FlameUp_Item:
                UseProfile.FlameUp_Item += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.FLAMEUP_ITEM);
                break;
            case GiftType.FastBoom_Item:
                UseProfile.FastBoom_Item += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.FASTBOOM_ITEM);
                break;
            case GiftType.TimeBoom_Item:
                UseProfile.TimeBoom_Item += amount;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.TIMEBOOM_ITEM);
                break;
            case GiftType.Fire_Start:
                UseProfile.Fire_Start = true;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.SHOP_CHECK);
                break;
            case GiftType.Boom_Start:
                UseProfile.Boom_Start = true;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.SHOP_CHECK);
                break;
            case GiftType.Heart_Unlimit:
            
                UseProfile.TimeUnlimitHeart = DateTime.Now.AddHours(1);
                UseProfile.Heart = 5;
                UseProfile.isUnlimitHeart = true;
                UseProfile.WasBoughtUnlimitTime = true;
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.SHOP_CHECK);
                break;
        }
    }

    public static bool IsCharacter(GiftType giftType)
    {
        //switch (giftType)
        //{
        //    case GiftType.RandomSkin:
        //        return true;
        //}
        return false;
    }
}

public class Gift
{
    [SerializeField] private Sprite giftSprite;
    [SerializeField] private GameObject giftAnim;
    public virtual Sprite getGiftSprite => giftSprite;
    public virtual GameObject getGiftAnim => giftAnim;

}

public enum GiftType
{
    None = 0,
    RemoveAds = 1,
    Coin = 2,
    Heart = 3,
    TNT_Booster = 4,
    Rocket_Booster = 5,
    Freeze_Booster = 6,
    Atom_Booster = 7,
    FlameUp_Item = 8,
    FastBoom_Item = 9,
    TimeBoom_Item = 10,
    Boom_Normal = 11,
    Boom_Start = 12,
    Fire_Start = 13,
    Heart_Unlimit = 14,




}

public enum Reason
{
    none = 0,
    play_with_item = 1,
    watch_video_claim_item_main_home = 2,
    daily_login = 3,
    lucky_spin = 4,
    unlock_skin_in_special_gift = 5,
    reward_accumulate = 6,
}

[Serializable]
public class RewardRandom
{
    public int id;
    public GiftType typeItem;
    public int amount;
    public int weight;

    public RewardRandom()
    {
    }
    public RewardRandom(GiftType item, int amount, int weight = 0)
    {
        this.typeItem = item;
        this.amount = amount;
        this.weight = weight;
    }

    public GiftRewardShow GetReward()
    {
        GiftRewardShow rew = new GiftRewardShow();
        rew.type = typeItem;
        rew.amount = amount;

        return rew;
    }
}
