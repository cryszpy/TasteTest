using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet.Example;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Managing.Transporting;
using FishNet.Transporting.UTP;
using TMPro;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;

public class Relay : MonoBehaviour
{
    [SerializeField]
    private NetworkManager networkManager;

    [SerializeField]
    private NetworkHudCanvases canvasScript;

    [SerializeField]
    private GameStateManager gameStateManager;

    [SerializeField]
    private TMP_InputField iField;
    private string inputCode;

    [SerializeField]
    private TMP_Text joinCodeText;

    // Called when pressing the HOST button
    public async void StartServer() {

        // Changes the game's state to loading.
        GameStateManager.SetState(GameState.LOADING);
        
        gameStateManager.menu.EnableLoadingScreen();

        // Calls the CreateRelay function to start server.
        await CreateRelay();
    }

    // Called when the player hits the JOIN button.
    public async void StartClient() {

        // Ensures that the player must input a join code to click the button
        if (iField.text.IsNullOrEmpty()) {
            Debug.LogWarning("Please input a join code!");
            return;
        }

        // Changes the game's state to loading.
        GameStateManager.SetState(GameState.LOADING);

        gameStateManager.menu.EnableLoadingScreen();

        // Calls the JoinRelay function to start client join.
        await JoinRelay();
    }

    // Create asynchronous behavior to run in the background so that the game isn't waiting on it.

    // Creates a relay of size (3) for a maximum of 4 players (host + 3), also trying to catch when it fails so that the game doesn't break.
    public async Task<string> CreateRelay(int maxConnections = 3)
    {
        // Initializes Unity Services engine.
        await UnityServices.InitializeAsync();

        // When a player signs into Relay, assign a PlayerId and put it into the console.
        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        }

        // Creates an allocation on the Relay service.
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

        // Gets a join code to that allocation.
        joinCodeText.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log(joinCodeText.text);

        var transport = networkManager.TransportManager.GetTransport<FishyUnityTransport>();
        var serverData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        transport.SetRelayServerData(serverData);
        //new RelayServerData(allocation.RelayServer.IpV4, 
        //    (ushort)allocation.RelayServer.Port, allocation.AllocationId.ToByteArray(), allocation.ConnectionData, 
        //    allocation.ConnectionData, allocation.Key, true, true)
        //connectionType:"dtls"
        
        // If canvas is not null-
        if (canvasScript != null) {

            // Establish server connection.
            canvasScript.OnClick_Server();

            // Establish host client connection.
            canvasScript.OnClick_Client();

            // Changes the game's state to playing and enables start button for the host.
            gameStateManager.menu.startButton.SetActive(true);
            gameStateManager.menu.DisableLoadingScreen();
            gameStateManager.menu.joinCodeDisplay.SetActive(true);

            Debug.Log("Started Server Connection and joined as Host");

            return joinCodeText.text;
        }
        return null;
    }

    public async Task<bool> JoinRelay() {

        // Initializes Unity Services engine.
        await UnityServices.InitializeAsync();

        // When a player signs into Relay, assign a PlayerId and put it into the console.
        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        }

        // Tries joining relay with respective code.
        try {
            Debug.Log("Joining Relay. . . : " + inputCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputCode);

            var transport = networkManager.TransportManager.GetTransport<FishyUnityTransport>();
            var serverData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            transport.SetRelayServerData(serverData);
            //new RelayServerData(joinAllocation.RelayServer.IpV4, 
            //    (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationId.ToByteArray(), joinAllocation.ConnectionData, 
            //    joinAllocation.ConnectionData, joinAllocation.Key, true, true)

            // If the code is valid and links to a created server relay-
            if (!string.IsNullOrEmpty(inputCode)) {

                // Connect client.
                canvasScript.OnClick_Client();

                gameStateManager.menu.DisableLoadingScreen();

                return true;
            }
            return false;

        } catch (RelayServiceException e) {
            Debug.Log("JOIN CODE IS INVALID! See exception for further details:");
            Debug.Log(e);

            // Resets
            gameStateManager.menu.DisableLoadingScreen();
            gameStateManager.menu.EnableStartMenu();
            return false;
        }
    }

    // Function to update the text visual for inputting code as client.
    public void UpdateText() {

        inputCode = iField.text;
    }
}
