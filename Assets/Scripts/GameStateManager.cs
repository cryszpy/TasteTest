using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using FishNet.Connection;
using Unity.VisualScripting;
using UnityEngine.XR;

// Game state manager class to handle game's state machine.
public class GameStateManager : NetworkBehaviour
{

    // Reference to the active instance of this object
    public static GameStateManager instance;

    // Holds the game's current state.
    public static GameState currentState;

    [SerializeField]
    private GameObject joinMenu;

    public GameObject startGameButton;

    public GameObject mainMenu;

    public GameObject joinCodeDisplay;

    public GameObject loadingScreen;

    // Switch statement to do things based on what state the game is in.
    public void ChangeState(GameState state) {
        currentState = state;

        switch (state) {
            // Switches game state to Loading.
            case GameState.MENU:
                StartLoading();
                break;
            // Switches game state to Playing.
            case GameState.PLAYING:
                StartPlaying();
                break;
            // Switches game state to End.
            case GameState.GAMEOVER:
                StartEnd();
                break;
            // Switches game state to Restart.
            case GameState.MAINMENU:
                StartRestart();
                break;
            // Default to catch any mistakes.
            default:
                break;
        }
    }

    // Start function
    private void Start() {
        if (!base.IsServerInitialized)
            return;
        
        instance = this;
    }
    
    // Executes code upon switching to Loading state.
    void StartLoading() {
        Debug.Log("Loading...");

        // Hides the main menu.
        mainMenu.SetActive(false);

        // Shows the loading screen.
        loadingScreen.SetActive(true);
    }

    // Executes code upon switching to Playing state.
    void StartPlaying() {
        Debug.Log("Playing...");

        // Shows the loading screen.
        loadingScreen.SetActive(false);

        // Hides the join menu.
        joinMenu.SetActive(false);

        // Shows the lobby code UI.
        joinCodeDisplay.SetActive(true);
    }

    // Executes code upon switching to End state.
    void StartEnd() {
        Debug.Log("Ending...");
    }

    // Executes code upon switching to Restart state.
    void StartRestart() {
        Debug.Log("Restarting...");
        ChangeState(GameState.MENU);
    }

    // Called when the player clicks the "JOIN" button.
    public void JoinButton() {
        // Shows the join menu UI.
        joinMenu.SetActive(true);
    }

    // Called when the "BACK" button is pressed in the join menu.
    public void CloseJoinMenu() {
        // Hides the join menu.
        joinMenu.SetActive(false);
    }
}

// Enum list of all game states.
public enum GameState {
    PLAYING, MENU, MAINMENU, GAMEOVER
}