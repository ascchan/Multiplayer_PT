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

    [SerializeField] private string sessionName;
    private ISession currentSession;

    public void JoinMatch()
    {
        JoinOrCreateSession(); 
        // NetworkManager.Singleton.StartClient();

        gameObject.SetActive(false);
    }

    public void HostMatch() //NOT use!!
    {
        return;
        NetworkManager.Singleton.StartHost();

        gameObject.SetActive(false);
    }

    async Task JoinOrCreateSession()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        SessionOptions options = new SessionOptions().WithDistributedAuthorityNetwork();

        options.MaxPlayers = 10;
        options.Name = sessionName;

        currentSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
    }

    private void OnDestroy()
    {
        if (currentSession != null)
        {
            currentSession.LeaveAsync();
        }

    }
}
