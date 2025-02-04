using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/FlavorAmount")]
public class FlavorAmount : ScriptableObject
{

    [Tooltip("The minimum amount of Flavor to satisfy this FlavorAmount.")]
    public int amountMinimum;
    
    [Tooltip("The maximum amount of Flavor to satisfy this FlavorAmount.")]
    public int amountMaximum;

    [Tooltip("What this Flavor Amount is called on an order.")]
    public string orderText;

    /* [Tooltip("The sprite background for an order with this Flavor Amount.")]
    public Sprite spriteDecor; */
}
