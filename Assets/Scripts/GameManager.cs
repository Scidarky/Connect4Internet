using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject red, green;
    [SerializeField] Text turnMessage;

    bool isPlayer, hasGameFinished;

    const string RED_MESSAGE = "Red's Turn";
    const string GREEN_MESSAGE = "Green's Turn";

    Color RED_COLOR = new Color(231, 29, 54, 255) / 255;
    Color GREEN_COLOR = new Color(0, 222, 1, 255) / 255;

    Board myBoard;

    private void Awake()
    {
        isPlayer = NetworkManagerP2P.Instance.isHost; // Host começa jogando
        hasGameFinished = false;

        turnMessage.text = RED_MESSAGE;
        turnMessage.color = RED_COLOR;

        myBoard = new Board();

        NetworkManagerP2P.Instance.OnMoveReceived += OnOpponentMove;
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
        if (hasGameFinished || !isPlayer) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit.collider) return;

            if (hit.collider.CompareTag("Press"))
            {
                Column col = hit.collider.GetComponent<Column>();

                if (col.targetlocation.y > 1.5f) return;

                RealizarJogada(col, isPlayer);
                NetworkManagerP2P.Instance.SendMove(col.col - 1); // envia para o outro jogador
                isPlayer = false; // aguarda o turno do outro
            }
        }
    }

    private void OnOpponentMove(int column)
    {
        Column[] columns = FindObjectsOfType<Column>();
        foreach (var col in columns)
        {
            if (col.col - 1 == column)
            {
                RealizarJogada(col, !isPlayer); // Jogada do oponente
                isPlayer = true;
                break;
            }
        }
    }

    private void RealizarJogada(Column col, bool jogadorAtual)
    {
        Vector3 spawnPos = col.spawnLocation;
        Vector3 targetPos = col.targetlocation;

        GameObject circle = Instantiate(jogadorAtual ? red : green);
        circle.transform.position = spawnPos;
        circle.GetComponent<Mover>().targetPostion = targetPos;

        col.targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

        myBoard.UpdateBoard(col.col - 1, jogadorAtual);

        if (myBoard.Result(jogadorAtual))
        {
            turnMessage.text = (jogadorAtual ? "Red" : "Green") + " Wins!";
            hasGameFinished = true;
            return;
        }

        turnMessage.text = !jogadorAtual ? RED_MESSAGE : GREEN_MESSAGE;
        turnMessage.color = !jogadorAtual ? RED_COLOR : GREEN_COLOR;
    }

}
