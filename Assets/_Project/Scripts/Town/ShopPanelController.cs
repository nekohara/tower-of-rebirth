using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private Button productButtonPrefab;
    [SerializeField] private List<ShopProduct> products;
    [SerializeField] private TownManager townManager;

    [Header("効果音")]
    [SerializeField]
    private AudioSource seAudioSource;

    [SerializeField]
    private AudioClip purchaseSe;
    public void CreateProductButtons()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (ShopProduct product in products)
        {
            if (product == null || string.IsNullOrEmpty(product.ProductName))
                continue;

            Button button = Instantiate(productButtonPrefab, content);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text =
                    $"{product.ProductName}\n{product.Price} G";
            }

            ShopProduct capturedProduct = product;
            button.onClick.AddListener(
                () => Purchase(capturedProduct));
        }
    }

    private bool CanAddProduct(ShopProduct product)
    {
        return product.productType switch
        {
            ShopProductType.Item =>
                product.item != null &&
                product.item.id == "potion",

            ShopProductType.Weapon =>
                product.weapon != null,

            ShopProductType.Armor =>
                product.armor != null,

            _ => false
        };
    }

    private void Purchase(ShopProduct product)
    {
        GameManager gm = GameManager.Instance;

        if (gm == null || product == null)
            return;

        if (!CanAddProduct(product))
        {
            townManager.SetMessage(
                $"{product.ProductName}は現在購入できません。");
            return;
        }

        if (IsAlreadyOwned(product))
        {
            townManager.SetMessage(
                $"{product.ProductName}はすでに所持しています。");
            return;
        }

        // 支払い前に重量を確認
        if (product.productType == ShopProductType.Item &&
            !gm.playerStatus.CanAddItem(product.item))
        {
            townManager.SetMessage(
                "荷物が重すぎて、これ以上持てない……");
            return;
        }

        if (gm.playerStatus.gold < product.Price)
        {
            townManager.SetMessage("お金が足りない……");
            return;
        }

        gm.playerStatus.gold -= product.Price;

        switch (product.productType)
        {
            case ShopProductType.Item:
                AddItem(product.item);
                break;

            case ShopProductType.Weapon:
                gm.ownedWeapons.Add(product.weapon);
                gm.EquipWeapon(product.weapon);
                break;

            case ShopProductType.Armor:
                gm.ownedArmors.Add(product.armor);
                gm.EquipArmor(product.armor);
                break;
        }

        PlayPurchaseSe();

        townManager.SetMessage(
            $"{product.ProductName}を購入した！");

        townManager.RefreshUI();
    }

    private bool IsAlreadyOwned(ShopProduct product)
    {
        GameManager gm = GameManager.Instance;

        return product.productType switch
        {
            ShopProductType.Weapon =>
                product.weapon != null &&
                gm.ownedWeapons.Exists(
                    weapon => weapon.name == product.weapon.name),

            ShopProductType.Armor =>
                product.armor != null &&
                gm.ownedArmors.Exists(
                    armor => armor.name == product.armor.name),

            // 道具は複数購入できる
            _ => false
        };
    }

    private void AddItem(InventoryItem item)
    {
        if (item == null)
            return;

        PlayerStatus status =
            GameManager.Instance.playerStatus;

        InventoryItem ownedItem =
            status.inventory.Find(
                inventoryItem => inventoryItem.id == item.id);

        if (ownedItem != null)
        {
            ownedItem.count++;
            return;
        }

        item.count = 1;
        status.inventory.Add(item);
    }

    private void PlayPurchaseSe()
    {
        if (seAudioSource == null ||
            purchaseSe == null)
        {
            return;
        }

        seAudioSource.PlayOneShot(purchaseSe);
    }
}