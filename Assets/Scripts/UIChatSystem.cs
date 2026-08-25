using System;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class UIChatSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private TextMeshProUGUI textElement;

    public Action<FixedString128Bytes> OnMessageSent;

    public void DisplayMessageOnBox(FixedString128Bytes newMessage)
    {
        textElement.text += newMessage + "\n";
    }

    public void SendWrittenMessage()
    {
        OnMessageSent.Invoke(messageInputField.text);
        messageInputField.text = "";
    }
}
