using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ShopOfferListWrapper
{
    public List<ShopOfferData> offers;
}

public class ShopManagement : MonoBehaviour
{
    public int displayCount = 6;
    public GameObject shopItemPrefab;
    public Transform shopParent;

    private List<ShopOfferData> allOffers;
    private List<ShopOfferData> currentOffers;

    void Start()
    {
        LoadAllOffers();
        RefreshShop();
    }

    void LoadAllOffers()
    {
        var json = Resources.Load<TextAsset>("shop_offers");
        var wrapper = JsonUtility.FromJson<ShopOfferListWrapper>(json.text);
        allOffers = wrapper.offers;

        // icon 스프라이트 처리
        foreach (var offer in allOffers)
        {
            offer.item.iconPath = Resources.Load<Sprite>($"Icons/{offer.item.icon}");
        }
    }

    public void RefreshShop()
    {
        // 이전 슬롯 제거
        foreach (Transform child in shopParent)
            Destroy(child.gameObject);

        // 중복 없이 랜덤 선택
        currentOffers = allOffers.OrderBy(x => Random.value).Take(displayCount).ToList();

        foreach (var offer in currentOffers)
        {
            var slot = Instantiate(shopItemPrefab, shopParent);
            slot.GetComponent<ShopItemUI>().SetOffer(offer);
        }
    }
}
