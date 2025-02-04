using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class OrderManager : NetworkBehaviour
{
    [Header("SCRIPT REFERENCES")] // ------------------------------------------------------------------------------------

    [SerializeField] private GameObject orderBar;

    [SerializeField] private GameObject orderPrefab;

    [SerializeField] private GameObject bruh;

    [SerializeField] private GameObject pivot;

    public OrdersList ordersList;

    [Header("STATS")] // ------------------------------------------------------------------------------------

    [Tooltip("List of all discovered recipes.")]
    public static List<FoodPair> DiscoveredRecipes = new();

    [Tooltip("Dictates whether orders are spawned or not.")]
    public bool shopIsOpen = false;

    [Tooltip("The maximum amount of orders in one day.")]
    public int maxOrderAmount;

    [Tooltip("The time in between order spawns.")]
    public float orderTime;
    
    // Timer to spawn orders
    public float orderTimer = 0;

    [Tooltip("Under or equal to this threshold is CRITIC chance, over is CUSTOMER chance.")]
    public float orderTypeChanceThreshold;

    [Tooltip("The minimum amount of items in an order. (Includes this value)")]
    public int orderItemMinInclusive;

    [Tooltip("The maximum amount of items in an order. (DOES NOT include this value (e.g. 3 --> 2 max items))")]
    public int orderItemMaxExclusive;

    [Tooltip("List of all active orders in the current shift.")]
    public List<OrderBase> activeOrders = new();

    // Queued list of 4 previous orders for spawn randomization purposes
    private Queue<OrderBase> recentOrders = new();
    
    private void Update() {

        if (base.IsServerInitialized) {

            // If the shop is open (game has started) and the max order amount hasn't been reached—
            if (shopIsOpen && activeOrders.Count < maxOrderAmount) {
                orderTimer += Time.deltaTime;

                // If able to spawn an order—
                if (orderTimer > orderTime) {
                    orderTimer = 0;

                    // Spawn order
                    ServerCreateRandomOrder(this, orderPrefab, orderBar.transform);
                }
            } else {
                orderTimer = 0;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUpdateTransform(GameObject spawnedObject, Transform parent) {
        UpdateTransform(spawnedObject, parent);
    }

    [ObserversRpc]
    public void UpdateTransform(GameObject spawnedObject, Transform parent) {

        spawnedObject.transform.SetParent(parent, false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerCreateRandomOrder(OrderManager orderManager, GameObject prefab, Transform parent) {

        // Create new order
        GameObject spawnedOrder = Instantiate(prefab, parent) as GameObject;
        InstanceFinder.ServerManager.Spawn(spawnedOrder);

        ServerUpdateTransform(spawnedOrder, parent);

        if (spawnedOrder.TryGetComponent<OrderBase>(out var script)) {

            // If players have discovered recipes—
            if (OrderManager.DiscoveredRecipes.Count > 0) {

                // Get random spawn chance
                float rand = Random.value;

                // Rolled a customer!
                if (rand > orderManager.orderTypeChanceThreshold) {

                    // Check that there aren't too many previous customers
                    int customerCounter = 0;
                    int criticCounter = 0;

                    foreach (var recentOrder in orderManager.recentOrders) {

                        if (recentOrder.TryGetComponent<OrderBase>(out var recentScript)) {

                            if (recentScript.specificRecipeOverride != null) {

                                customerCounter++;
                            } else {
                                criticCounter++;
                            }
                        }
                    }

                    // Too many customers recently, spawn a CRITIC
                    if (customerCounter > 2) {
                        script.type = OrderType.CRITIC;
                    } 
                    // Spawn a CUSTOMER
                    else {
                        script.type = OrderType.CUSTOMER;
                    }
                }
            } 
            // If players haven't discovered any recipes—
            else {
                script.type = OrderType.CRITIC;
            }

            switch (script.type) {

                case OrderType.CRITIC:
                    SpawnCritic(orderManager, script);
                    break;
                case OrderType.CUSTOMER:
                    SpawnCustomer(orderManager, script);
                    break;
                default:
                    SpawnCritic(orderManager, script);
                    break;
            }
        } else {
            Debug.LogError("Could not find OrderBase component on this order!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCritic(OrderManager orderManager, OrderBase order) {

        // Get a random amount of items in the order
        int itemAmounts = GetRandomItemAmounts(orderManager.orderItemMinInclusive, orderManager.orderItemMaxExclusive);

        List<OrderItem> itemsList = new();

        // For each item in the order—
        for (int i = 0; i < itemAmounts; i++) {

            // Generate a random flavor
            Flavor selectedFlavor = GetRandomFlavor(orderManager.ordersList.flavors);

            // Generate a random amount of said flavor
            FlavorAmount selectedFlavorAmount = GetRandomFlavorAmount(orderManager.ordersList.flavorAmounts);

            itemsList.Add(new(selectedFlavor, selectedFlavorAmount));
        }

        string nameDiscard = GetRandomOrdererName(orderManager.ordersList.criticTitles) + " " + GetRandomOrdererName(orderManager.ordersList.criticSurnames);

        // Update order for all players
        UpdateCriticOrder(orderManager, order, nameDiscard, itemsList);

        Debug.Log("Critic order created!");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCustomer(OrderManager orderManager, OrderBase order) {

        // Generate a random discovered recipe to ask for
        FoodPair selectedRecipe = OrderManager.DiscoveredRecipes[Random.Range(0, OrderManager.DiscoveredRecipes.Count - 1)];

        // Generate random name
        string nameDiscard = GetRandomOrdererName(orderManager.ordersList.customerNames);

        // Update order for all players
        UpdateCustomerOrder(orderManager, order, nameDiscard, selectedRecipe);

        Debug.Log("Customer order created!");
    }

    [ObserversRpc]
    public void UpdateCriticOrder(OrderManager orderManager, OrderBase order, string personName, List<OrderItem> itemsList) {

        foreach (OrderItem item in itemsList) {
            order.items.Add(item);
        }

        // Update orderer name
        order.ordererName = personName;

        // Updates order visuals
        if (order.nameText && order.ordererName != null) {
            order.nameText.text = order.ordererName;
        }

        if (order.items.Count > 0) {
            order.itemText.text = order.items[0].flavorAmount.orderText + " " + order.items[0].flavor.orderText;
        }

        // Add spawned order to active orders listed
        if (!orderManager.activeOrders.Contains(order)) {
            orderManager.activeOrders.Add(order);
        }

        // If recent orders list is full—
        if (orderManager.recentOrders.Count >= 4) {
            
            // Remove the oldest order
            orderManager.recentOrders.Dequeue();

            // Add the order
            orderManager.recentOrders.Enqueue(order);
        } 
        // If it's not full, add the order
        else {
            orderManager.recentOrders.Enqueue(order);
        }
    }

    [ObserversRpc]
    public void UpdateCustomerOrder(OrderManager orderManager, OrderBase order, string personName, FoodPair recipe) {

        // Update chosen recipe to ask for
        order.specificRecipeOverride = recipe.pickup.baseDefinition;

        // Update orderer name
        order.ordererName = personName;

        // Updates order visuals
        if (order.nameText && order.ordererName != null) {
            order.nameText.text = order.ordererName;
        }

        if (order.items.Count > 0) {
            order.itemText.text = order.items[0].flavorAmount.orderText + " " + order.items[0].flavor.orderText;
        }

        // Add spawned order to active orders listed
        if (!orderManager.activeOrders.Contains(order)) {
            orderManager.activeOrders.Add(order);
        }

        // If recent orders list is full—
        if (orderManager.recentOrders.Count >= 4) {
            
            // Remove the oldest order
            orderManager.recentOrders.Dequeue();

            // Add the order
            orderManager.recentOrders.Enqueue(order);
        } 
        // If it's not full, add the order
        else {
            orderManager.recentOrders.Enqueue(order);
        }
    }

    public int GetRandomItemAmounts(int min, int max) {
        return Random.Range(min, max);
    }

    public Flavor GetRandomFlavor(List<Flavor> list) {
        return list[Random.Range(0, list.Count)];
    }
    
    public FlavorAmount GetRandomFlavorAmount(List<FlavorAmount> list) {
        return list[Random.Range(0, list.Count)];
    }

    public string GetRandomOrdererName(List<string> list) {
        return list[Random.Range(0, list.Count)];
    }
}