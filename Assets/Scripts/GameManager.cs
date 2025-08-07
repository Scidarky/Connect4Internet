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
  [SerializeField] GameObject red, green;
    [SerializeField] Text turnMessage;

    bool isPlayerTurn = true; // Sua vez de jogar (host começa true)
    bool hasGameFinished = false;

    const string RED_MESSAGE = "Red's Turn";
    const string GREEN_MESSAGE = "Green's Turn";

    Color RED_COLOR = new Color(231f/255f, 29f/255f, 54f/255f, 1f);
    Color GREEN_COLOR = new Color(0f, 222f/255f, 1f, 1f);

    Board myBoard;

    Column[] columns; // Cache para as colunas do tabuleiro

    private void Awake()
    {
        hasGameFinished = false;

    // Corrigido: só o host começa com o turno ativo
    isPlayerTurn = NetworkManagerP2P.Instance.isHost;

    turnMessage.text = isPlayerTurn ? RED_MESSAGE : GREEN_MESSAGE;
    turnMessage.color = isPlayerTurn ? RED_COLOR : GREEN_COLOR;

    myBoard = new Board();
    columns = FindObjectsOfType<Column>();

    NetworkManagerP2P.Instance.OnMoveReceived += OnMoveReceived;
    }

    private void OnDestroy()
    {
        // Remover inscrição para evitar vazamento
        if(NetworkManagerP2P.Instance != null)
            NetworkManagerP2P.Instance.OnMoveReceived -= OnMoveReceived;
    }

    private void Update()
    {
        if (hasGameFinished) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!isPlayerTurn)
            {
                Debug.Log("Não é sua vez!");
                return;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit.collider) return;

            if (hit.collider.CompareTag("Press"))
            {
                Column colComponent = hit.collider.gameObject.GetComponent<Column>();
                if (colComponent == null)
                    return;

                if (colComponent.targetlocation.y > 1.5f) return; // Coluna cheia

                int columnIndex = colComponent.col - 1;

                // Aplica a jogada local
                ApplyMove(columnIndex);

                // Envia a jogada para o outro jogador
                NetworkManagerP2P.Instance.SendMove(columnIndex);
            }
        }
    }

    void OnMoveReceived(int column)
    {
        if (hasGameFinished) return;

        if (isPlayerTurn)
        {
            Debug.Log("Recebeu jogada, mas ainda é sua vez. Ignorando...");
            return;
        }

        Debug.Log("Jogada recebida na coluna: " + column);
        ApplyMove(column);
    }

    void ApplyMove(int column)
    {
        Column colComponent = FindColumnByIndex(column);
        if (colComponent == null)
        {
            Debug.LogWarning("Coluna não encontrada: " + column);
            return;
        }

        if (colComponent.targetlocation.y > 1.5f)
        {
            Debug.LogWarning("Coluna cheia: " + column);
            return;
        }

        // Spawn da peça
        Vector3 spawnPos = colComponent.spawnLocation;
        Vector3 targetPos = colComponent.targetlocation;

        GameObject circle = Instantiate(isPlayerTurn ? red : green);
        circle.transform.position = spawnPos;
        circle.GetComponent<Mover>().targetPostion = targetPos;

        // Atualiza posição da próxima peça naquela coluna
        colComponent.targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

        // Atualiza o tabuleiro lógico
        myBoard.UpdateBoard(column, isPlayerTurn);

        // Verifica se alguém ganhou
        if (myBoard.Result(isPlayerTurn))
        {
            turnMessage.text = (isPlayerTurn ? "Red" : "Green") + " Wins!";
            hasGameFinished = true;
            return;
        }

        // Atualiza mensagem e cor do turno
        isPlayerTurn = !isPlayerTurn;
        turnMessage.text = isPlayerTurn ? RED_MESSAGE : GREEN_MESSAGE;
        turnMessage.color = isPlayerTurn ? RED_COLOR : GREEN_COLOR;
    }

    Column FindColumnByIndex(int index)
    {
        foreach (var c in columns)
        {
            if (c.col - 1 == index)
                return c;
        }
        return null;
    }
}
