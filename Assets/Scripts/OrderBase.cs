using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum OrderType {
    CRITIC, CUSTOMER
}

[System.Serializable]
public struct OrderItem {

    public Flavor flavor;

    public FlavorAmount flavorAmount;

    public OrderItem(Flavor flavor, FlavorAmount flavorAmount) {

        this.flavor = flavor;

        this.flavorAmount = flavorAmount;
    }
}

public class OrderBase : NetworkBehaviour
{

    public Image image;

    public TMP_Text nameText;

    public TMP_Text itemText;

    public OrderType type;

    public FoodBase specificRecipeOverride = null;

    public List<OrderItem> items = new();
    
    public string ordererName = null;

    public float price = 0;
}