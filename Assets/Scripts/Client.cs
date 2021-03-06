using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;

public class Client : MonoBehaviour
{
    public InputField IPInput, PortInput, NickInput;
    string clientName;

    bool socketReady;
    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;

    // IP와 Port를 통해 서버에 연결합니다. InputField에 적힌게 없다면 기본 정보로 연결합니다.
    public void ConnetToServer()
    {
        if (socketReady) return;

        string ip = IPInput.text == "" ? "127.0.0.1" : IPInput.text;
        int port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);

        try
        {
            socket = new TcpClient(ip, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
        }
        catch (Exception ex)
        {
            Chat.instance.ShowMessage($"소켓에러 : {ex.Message}");
        }
    }

    private void Update()
    {
        if (socketReady && stream.DataAvailable)
        {
            string data = reader.ReadLine();
            if (data != null)
            {
                OnIncomingData(data);
            }
        }
    }

    void OnIncomingData(string data)
    {
        if (data == "%NAME")
        {
            clientName = NickInput.text == "" ? "Guest" + UnityEngine.Random.Range(1000, 10000) : NickInput.text;
            Send($"&NAME|{clientName}");
            return;
        }

        Chat.instance.ShowMessage(data);
    }

    // 채팅에 작성한 데이터를 보내는 함수입니다.
    void Send(string data)
    {
        if (!socketReady) return;

        Chat.instance.ScaleUpContent();
        writer.WriteLine(data);
        writer.Flush();
    }

    // 채팅을 보내는 함수입니다. 엔터를 누를시 데이터를 쓰고 출력합니다.
    public void OnSendButton(InputField SendInput)
    {
#if(UNITY_EDITOR||UNITY_STANDALONE)
        if (!Input.GetButtonDown("Submit")) return;
        SendInput.ActivateInputField();
#endif
        if (SendInput.text.Trim() == "") return;

        string message = SendInput.text;
        SendInput.text = "";
        Send(message);
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    // 서버로 부터 연결을 끊습니다.
    void CloseSocket()
    {
        if (!socketReady) return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }
}
