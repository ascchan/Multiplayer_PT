using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIMultiplayer : MonoBehaviour
{
    [SerializeField] private TMP_InputField userNameInputfield;
    [SerializeField] private TMP_InputField sessionCodeInputfield;
    [SerializeField] private TMP_Dropdown tankColorDropdown;

    public void JoinMatch()
    {
        NetworkManager.Singleton.StartClient();

        gameObject.SetActive(false);
    }

    public void HostMatch()
    {
        NetworkManager.Singleton.StartHost();

        gameObject.SetActive(false);
    }
}
