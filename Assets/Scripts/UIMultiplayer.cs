using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class UIMultiplayer : MonoBehaviour
{
    [SerializeField] private TMP_InputField userNameInputfield;
    [SerializeField] private TMP_InputField sessionCodeInputfield;
    [SerializeField] private TMP_Dropdown tankColorDropdown;

//    [SerializeField] private string sessionName;
    private ISession currentSession;

    public void JoinMatch()
    {
        Debug.Log("JoinMatch() -- sessionCodeInputfield: " + sessionCodeInputfield.text);
        JoinSession(); 
        // NetworkManager.Singleton.StartClient();

        gameObject.SetActive(false);
    }

    public void HostMatch() 
    {
        CreateSession();
        //NetworkManager.Singleton.StartHost();

        gameObject.SetActive(false);
    }

    async Task JoinSession()
    {

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCodeInputfield.text);
        currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCodeInputfield.text);

        sessionCodeInputfield.interactable = false;
    }

    async Task CreateSession()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        SessionOptions options = new SessionOptions().WithRelayNetwork();

        options.MaxPlayers = 10;

        currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);

        sessionCodeInputfield.text = currentSession.Code;
        Debug.Log("current Session Code: " + currentSession.Code);

        sessionCodeInputfield.interactable = false;
    }

    private void OnDestroy()
    {
        if (currentSession != null)
        {
            currentSession.LeaveAsync();
        }
    }

    public string GetTypedUsername()
    {
        return userNameInputfield.text;
    }

    public Color GetSelectedColor()
    {
        int indexOfTheColor = tankColorDropdown.value;
        return tankColorDropdown.options[indexOfTheColor].color;
    } 

}
