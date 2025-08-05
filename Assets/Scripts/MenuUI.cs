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
        SceneManager.LoadScene("Gameplayyy");
    }

    public void OnClickJoin()
    {
        string ip = inputIP.text.Trim();
        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogWarning("IP vazio!");
            return;
        }

        // Adiciona o evento para trocar de cena quando conectar
        NetworkManagerP2P.Instance.OnConnectedToHost += OnConnectedHandler;

        NetworkManagerP2P.Instance.ConnectToHost(ip, 5000);
        Debug.Log("Tentando conectar em " + ip);
    }

    void OnConnectedHandler()
    {
        Debug.Log("Conectado ao host, carregando cena.");
        NetworkManagerP2P.Instance.OnConnectedToHost -= OnConnectedHandler; // Remove inscrição para evitar duplicação
        SceneManager.LoadScene("Gameplayyy");
    }
}