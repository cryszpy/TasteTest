using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using FishNet.Connection;
using Unity.VisualScripting;
using UnityEngine.XR;
using System.Linq;

// Game state manager class to handle game's state machine.
public class GameStateManager : NetworkBehaviour
{

    // Reference to the active instance of this object
    public static GameStateManager instance;

    // Holds the game's current state.
    public static GameState currentState;
    public GameState stateTracker;

    public List<GameObject> players;

    public Menu menu;

    public OrderManager orderManager;

    public delegate void EventHandler();

    public static EventHandler EOnGamestateChange;

    // Switch statement to do things based on what state the game is in.
    public static void SetState(GameState newState) {
        currentState = newState;

        EOnGamestateChange?.Invoke();
    }

    // Start function
    private void Awake() {
        if (!base.IsServerInitialized)
            return;
        
        instance = this;

        if (!orderManager) {
            orderManager = GetComponent<OrderManager>();
        }

        SetState(GameState.MAINMENU);
    }

    private void Update() {
        stateTracker = currentState;
    }

    public void StartButton() {
        ServerStartButtonPressed(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerStartButtonPressed(GameStateManager gameStateManager) {
        StartButtonPressed(gameStateManager);
    }

    [ObserversRpc]
    private void StartButtonPressed(GameStateManager gameStateManager) {
        GameStateManager.SetState(GameState.PLAYING);
        Debug.Log("PLAYING");

        // Hide start button
        gameStateManager.menu.startButton.SetActive(false);

        // Start the day
        gameStateManager.orderManager.shopIsOpen = true;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

// Enum list of all game states.
public enum GameState {
    MAINMENU, PLAYING, LOADING, GAMEOVER
}