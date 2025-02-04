using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public class FoodPickup : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    
    public FoodBase baseDefinition;

    public PlayerItemPickup heldBy;

    public Sprite normalSprite;
    public Sprite selectedSprite;
}