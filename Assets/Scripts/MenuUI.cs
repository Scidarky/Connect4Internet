using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
   
    public InputField inputIP;

    public void OnClickHost()
    {
        NetworkManagerP2P.Instance.StartHost(5000);
        Debug.Log("Host iniciado.");
        SceneManager.LoadScene("Gameplay"); 
    }

    public void OnClickJoin()
    {
        string ip = inputIP.text.Trim();
        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogWarning("IP vazio!");
            return;
        }

        // Registra o que fazer depois que conectar com sucesso
        NetworkManagerP2P.Instance.OnConnectedToHost = () =>
        {
            SceneManager.LoadScene("Gameplay");
        };

        NetworkManagerP2P.Instance.ConnectToHost(ip, 5000);
        Debug.Log("Tentando conectar em " + ip);
    }
}
