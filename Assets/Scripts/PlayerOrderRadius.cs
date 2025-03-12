using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet;
using System.Collections;
using System.Collections.Generic;

public class PlayerOrderRadius : NetworkBehaviour
{

    [Header("SCRIPT REFERENCES")] // -------------------------------------------------------------------------

    private OrderManager orderManager;
    private CombinationManager combinationManager;

    public PlayerController player;

    private GameObject selectedCounter;

    [Header("STATS")] // -------------------------------------------------------------------------

    private bool inRadius;

    public LayerMask layerMask;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color selectedColor;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
        }

        if (!orderManager) {
            orderManager = GameObject.FindGameObjectWithTag("GameStateManager").GetComponent<OrderManager>();
        }

        if (!combinationManager) {
            combinationManager = GameObject.FindGameObjectWithTag("CombinationManager").GetComponent<CombinationManager>();
        }
    }

    private void Update() {

        if (GameStateManager.currentState == GameState.PLAYING) {

            // Raycast to select counter
            if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, player.reach, layerMask) && hit.collider.CompareTag("Counter")) {

                inRadius = true;

                ServerAssignCounter(this, hit.collider.gameObject, orderManager);
            } else {

                if (selectedCounter) {

                    // Disable changed color on previously selected counters
                    if (selectedCounter.TryGetComponent<MeshRenderer>(out var nonCounter)) {
                        nonCounter.material.color = defaultColor;
                    }
                }
                
                // Unassign order counter
                ServerUnassignOrder(this);
                inRadius = false;
            }

            Debug.DrawLine(transform.position, hit.point, Color.cyan, 0.01f);

            // If the player presses E and is holding an item—
            if (Input.GetKeyDown(KeyCode.E) && inRadius && player.itemPickup.pickupInHand && orderManager && combinationManager) {

                // ORDER EVALUATION MECHANICS
                ServerEvaluateOrder(this, orderManager, player.itemPickup.pickupInHand);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerAssignCounter(PlayerOrderRadius orderRadius, GameObject counter, OrderManager manager) {
        AssignCounter(orderRadius, counter, manager);
    }

    [ObserversRpc]
    public void AssignCounter(PlayerOrderRadius orderRadius, GameObject counter, OrderManager manager) {

        if (orderRadius.selectedCounter != counter) {
            orderRadius.selectedCounter = counter;

            // Temporarily change material color
            if (orderRadius.selectedCounter.TryGetComponent<MeshRenderer>(out var script)) {
                script.material.color = orderRadius.selectedColor;
            }

            // Disable other counters
            List<GameObject> tempCounters = new(manager.counters);
            tempCounters.Remove(orderRadius.selectedCounter);

            foreach (var target in tempCounters) {

                // Disable changed color on previously selected counters
                if (target.TryGetComponent<MeshRenderer>(out var nonCounter)) {
                    nonCounter.material.color = defaultColor;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUnassignOrder(PlayerOrderRadius orderRadius) {
        UnassignOrder(orderRadius);
    }

    [ObserversRpc]
    public void UnassignOrder(PlayerOrderRadius orderRadius) {
        orderRadius.selectedCounter = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerEvaluateOrder(PlayerOrderRadius orderRadius, OrderManager manager, GameObject food) {

        // Get the proper order ticket for this respective counter
        OrderBase order = null;

        if (orderRadius.selectedCounter.TryGetComponent<Counter>(out var counter)) {

            // Prevent submitting to a nonexistent critic/customer
            if (manager.activeOrders.Count < counter.slotNumber + 1) return;

            if (manager.activeOrders[counter.slotNumber] != null) {

                order = manager.activeOrders[counter.slotNumber];
            }

            if (order == null) {
                Debug.LogError("Evaluated order is null!");
            }

            // SUBMITTING A RECIPE
            if (food.TryGetComponent<FoodBase>(out var foodBase)) {

                if (foodBase.type == FoodType.RECIPE) {

                    // Update food item transform and parent
                    ServerUpdateTransform(food, counter);

                    // Evaluate order
                    EvaluateRecipe(manager, foodBase, order);

                    // Reset hand
                    counter.ServerResetHand(orderRadius);
                }
            } 
            // SUBMITTING A RECIPE FRAMEWORK
            else if (food.TryGetComponent<RecipeFramework>(out var framework)) {

                // Add the recipe to unlocked recipes
                foreach (var recipe in framework.undiscoveredRecipes) {
                    OrderManager.DiscoveredRecipes.Add(recipe);
                }

                // Update food item transform and parent
                ServerUpdateTransform(food, counter);

                // Evaluate order
                EvaluateFramework(framework, manager, order);

                // Reset hand
                counter.ServerResetHand(orderRadius);

                // Unlock recipes from framework
                counter.ServerUnlockRecipe(orderRadius, counter);
            } else {
                Debug.LogError("Submitted food item is not a Recipe or Framework!");
            }
        } else {
            Debug.LogError("Counter component could not be found on: " + orderRadius.selectedCounter.name + "!");
        }
    }

    [ObserversRpc]
    public void EvaluateRecipe(OrderManager manager, FoodBase food, OrderBase order) {

        int score = 0;

        // Evaluate
        if (order.specificRecipeOverride == food) {

            score++;
        }

        Color scoreColor = Color.white;

        if (score > 0) {
            Debug.Log("Scored a GREEN customer order!");
            scoreColor = Color.green;
        } else {
            Debug.Log("Scored a RED customer order!");
            scoreColor = Color.red;
        }

        ServerRemoveOrder(manager, order, scoreColor);
    }

    public void EvaluateFramework(RecipeFramework framework, OrderManager manager, OrderBase order) {

        int score = 0;

        // If there are valid recipes in the framework—
        if (framework.undiscoveredRecipes.Count > 0) {

            // For every flavor and flavor amount requested—
            switch (order.items.Count) {

                case 0:
                    Debug.Log("lol lmao");
                    break;
                case 1:
                    Debug.Log("1 coutn");

                    FoodBase foundRecipe = framework.undiscoveredRecipes[0];

                    // Find if the recipe meets Flavor criteria
                    FlavorProfile flavor = foundRecipe.flavorProfile.Find(x => x.flavor.orderText == order.items[0].flavor.orderText);

                    if (flavor != null) {

                        // CORRECTLY FULFILLED (GREEN)
                        if (flavor.value >= order.items[0].flavorAmount.amountMinimum && flavor.value <= order.items[0].flavorAmount.amountMaximum) {

                            score = 3;
                            break;
                        } 
                        // NOT QUITE (ORANGE)
                        else {
                            score = 1;
                            break;
                        }
                    } 
                    // UTTER FAILURE (RED)
                    else {
                        score = 0;
                        break;
                    }
                case 2:
                    Debug.Log("2 count");

                    foreach (FoodBase recipe in framework.undiscoveredRecipes) {

                        // Find if the recipe meets Flavor criteria
                        FlavorProfile firstFlavor = recipe.flavorProfile.Find(x => x.flavor.orderText == order.items[0].flavor.orderText);
                        FlavorProfile secondFlavor = recipe.flavorProfile.Find(x => x.flavor.orderText == order.items[1].flavor.orderText);

                        // If it does, check for Amount criteria
                        if (firstFlavor != null && secondFlavor != null) {
                            
                            // CORRECTLY FULFILLED (GREEN)
                            if (firstFlavor.value >= order.items[0].flavorAmount.amountMinimum && firstFlavor.value <= order.items[0].flavorAmount.amountMaximum
                                && secondFlavor.value >= order.items[1].flavorAmount.amountMinimum && secondFlavor.value <= order.items[1].flavorAmount.amountMaximum) {

                                score = 3;
                                break;
                            } 
                            // MOSTLY CORRECT (YELLOW)
                            else {

                                score = 2;
                                break;
                            }
                        }
                        // NOT QUITE (ORANGE)
                        else if (!(firstFlavor == null && secondFlavor == null)) {

                            score = 1;
                            break;
                        } 
                        // UTTER FAILURE (RED)
                        else {
                            score = 0;
                            break;
                        }
                    }
                    break;
            }
        }

        Color scoreColor = Color.white;

        switch (score) {

            case 0:
                Debug.Log("Scored a RED order!");
                scoreColor = Color.red;
                break;
            case 1:
                Debug.Log("Scored an ORANGE order!");
                scoreColor = new(250f, 85f, 25f);
                break;
            case 2:
                Debug.Log("Scored a YELLOW order!");
                scoreColor = Color.yellow;
                break;
            case 3:
                Debug.Log("Scored a GREEN order!");
                scoreColor = Color.green;
                break;
        }

        ServerRemoveOrder(manager, order, scoreColor);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateTransform(GameObject target, Counter counter) {
        UpdateTransform(target, counter);
    }

    [ObserversRpc]
    public void UpdateTransform(GameObject target, Counter counter) {

        // Switch parent and reset position
        target.transform.parent = counter.spawnPoint.transform;
        target.transform.localPosition = new(0, 0, 0);

        // Set counter's held food item
        counter.heldItem = target;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerRemoveOrder(OrderManager manager, OrderBase order, Color color) {
        RemoveOrder(manager, order, color);
    }

    [ObserversRpc]
    public void RemoveOrder(OrderManager manager, OrderBase order, Color color) {
        StartCoroutine(StartRemoveOrder(manager, order, color));
    }

    public IEnumerator StartRemoveOrder(OrderManager manager, OrderBase order, Color color) {

        Color prevColor = order.image.color;

        order.image.color = color;

        yield return new WaitForSeconds(2f);

        order.image.color = prevColor;

        // Remove order from active orders
        manager.activeOrders.Remove(order);

        if (base.IsHostInitialized) {
            ServerRemoveOrder(manager, order);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerRemoveOrder(OrderManager manager, OrderBase order) {

        // Delete order object
        InstanceFinder.ServerManager.Despawn(order.gameObject);
    }
}