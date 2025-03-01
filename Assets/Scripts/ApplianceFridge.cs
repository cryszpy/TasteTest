using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ApplianceFridge : ApplianceBase
{

    [Header("FRIDGE")] // ----------------------------------------------------------------------------

    public FoodList recipesList;

    public GameObject fridgeScreen;

    public GameObject pivot;

    public GameObject cartElement;

    public Queue<GameObject> cartQueue = new();
    public List<GameObject> cartQueueTracker = new();

    public Queue<GameObject> cartScreenQueue = new();
    public List<GameObject> cartScreenTracker = new();

    public void Update() {
        cartQueueTracker = new(cartQueue);
        cartScreenTracker = new(cartScreenQueue);
    }

    public override void ServerUseAppliance(PlayerApplianceRaycast playerRaycast) {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UseAppliance(playerRaycast);
    }

    public override void UseAppliance(PlayerApplianceRaycast playerRaycast, GameObject applianceObject = default) {
        ToggleFridgeScreen();
    }

    public override void ServerLeftAppliance(PlayerApplianceRaycast playerRaycast) {
        LeftAppliance(playerRaycast);
    }

    public override void LeftAppliance(PlayerApplianceRaycast playerRaycast) {

        // Disable fridge screen if leaving fridge
        if (fridgeScreen.activeInHierarchy) {
            ToggleFridgeScreen();
        }
    }

    public void ToggleFridgeScreen() {
        
        // ENABLE
        if (!fridgeScreen.activeInHierarchy) {

            fridgeScreen.SetActive(true);
        } 
        // DISABLE
        else {

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            fridgeScreen.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerAddToCart(ApplianceFridge _fridge, GameObject food) {
        AddToCart(_fridge, food);
    }

    [ObserversRpc]
    public void AddToCart(ApplianceFridge _fridge, GameObject food) {

        // Add food to queue
        _fridge.cartQueue.Enqueue(food);

        // Update fridge UI cart
        GameObject element = Instantiate(_fridge.cartElement, _fridge.pivot.transform);

        if (element.TryGetComponent<CartElement>(out var script) && food.TryGetComponent<Pickup>(out var pickup)) {
            script.ingredientImage.sprite = pickup.normalSprite;
            script.ingredientText.text = food.name;
        }

        _fridge.cartScreenQueue.Enqueue(element);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerRemoveFromCart(ApplianceFridge _fridge) {
        RemoveFromCart(_fridge);
    }

    [ObserversRpc]
    public void RemoveFromCart(ApplianceFridge _fridge) {

        if (_fridge.cartQueue.Count > 0) {
            _fridge.cartQueue.Dequeue();
        }

        if (_fridge.cartScreenQueue.Count > 0) {

            GameObject toRemove = _fridge.cartScreenQueue.Dequeue();

            Destroy(toRemove);
        }
    }
}