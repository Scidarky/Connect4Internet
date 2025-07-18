using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    TcpListener server;
    Thread serverThread;
    
    [SerializeField]
    GameObject red, green;

    bool isPlayer, hasGameFinished;

    [SerializeField]
    Text turnMessage;

    const string RED_MESSAGE = "Red's Turn";
    const string GREEN_MESSAGE = "Greens's Turn";

    Color RED_COLOR = new Color(231, 29, 54, 255) / 255;
    Color GREEN_COLOR = new Color(0, 222, 1, 255) / 255;

    Board myBoard;

    private Vector3 spawnPos;
    private Vector3 targetPos;


    private void Awake()
    {
        isPlayer = true;
        hasGameFinished = false;
        turnMessage.text = RED_MESSAGE;
        turnMessage.color = RED_COLOR;
        myBoard = new Board();
        serverThread = new Thread(new ThreadStart(StartServer));
        serverThread.IsBackground = true;
        serverThread.Start();
    }
    
    void StartServer()
    {
        IPAddress localAddr = IPAddress.Parse("10.57.10.39");
        server = new TcpListener(localAddr, 8080);
        server.Start();
    }
    
    void SendMessageToServer()
    {
        TcpClient client = new TcpClient("10.57.10.36", 8080);
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes(mensagem);
        stream.Write(data, 0, data.Length);
        stream.Close();
        client.Close();
    }
    
    public void GameStart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }


    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //If GameFinsished then return
            if (hasGameFinished) return;

            //Raycast2D
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit.collider) return;

            if(hit.collider.CompareTag("Press"))
            {
                //Check out of Bounds
                if (hit.collider.gameObject.GetComponent<Column>().targetlocation.y > 1.5f) return;

                //Spawn the GameObject
                spawnPos = hit.collider.gameObject.GetComponent<Column>().spawnLocation;
                targetPos = hit.collider.gameObject.GetComponent<Column>().targetlocation;
                GameObject circle = Instantiate(isPlayer ? red : green);
                circle.transform.position = spawnPos;
                circle.GetComponent<Mover>().targetPostion = targetPos;

                //Increase the targetLocationHeight
                hit.collider.gameObject.GetComponent<Column>().targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

                //UpdateBoard
                myBoard.UpdateBoard(hit.collider.gameObject.GetComponent<Column>().col - 1, isPlayer);
                if(myBoard.Result(isPlayer))
                {
                    turnMessage.text = (isPlayer ? "Red" : "Green") + " Wins!";
                    hasGameFinished = true;
                    return;
                }

                //TurnMessage
                turnMessage.text = !isPlayer ? RED_MESSAGE : GREEN_MESSAGE;
                turnMessage.color = !isPlayer ? RED_COLOR : GREEN_COLOR;

                //Change PlayerTurn
                isPlayer = !isPlayer;
            }

        }
    }
    void OnApplicationQuit()
    {
        server?.Stop();
        serverThread?.Abort();
    }
}
