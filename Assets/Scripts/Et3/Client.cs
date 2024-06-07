using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    [SerializeField] private Team2[] teams;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField teamInputField;
    [SerializeField] private TMP_Text connectionResult;
    public TMP_Text sending;
    public string sendingText = "";
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject startPanel;

    [SerializeField] private GameObject tank;
    [SerializeField] private GameObject missile;
    
    private Team2 _team;
    private ClientData _dataToSend;
    private List<ClientData> _receivedData;
    private TcpClient _client;
    private Thread _readThread;
    private Thread _writeThread;
    private bool _isRunning = true;
    private bool _gameStarted = false;
    private AutoResetEvent _resetEvent;
    
    private List<GameObject> _drawnTanks;
    private List<GameObject> _drawnMissiles;
    void Start()
    {
        _dataToSend = new ClientData();
        _receivedData = new List<ClientData>();
        
        _resetEvent = new AutoResetEvent(false);
        _readThread = new Thread(ReadData);
        _writeThread = new Thread(WriteData);
    }

    private void Update()
    {
        if(!_gameStarted)
            return;
        
        PrepareSendData();
        _resetEvent.Set();
        DrawData();
        sending.text = sendingText;
    }

    public void ConnectToServer()
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(IPAddress.Parse(ipInputField.text), 10001);
            connectionResult.text = "Connected successfully";
            ReceiveStartMessage();
        }
        catch (Exception e)
        {
            connectionResult.text = "Error while connecting";
        }
    }

    private void PrepareSendData()
    { 
        List<GameObject> tanks = _team.TanksObjects; 
        List<GameObject> missiles = _team.MissilesObjects;
        List<ObjectData> tanksData = new List<ObjectData>();
        foreach (GameObject tank in tanks)
        {
            ObjectData data = new ObjectData();
            data.Set(tank.activeInHierarchy, tank.transform.position, tank.transform.rotation);
            tanksData.Add(data);
        }        
        List<ObjectData> missilesData = new List<ObjectData>();
        foreach (GameObject missile in missiles)
        {
            ObjectData data = new ObjectData();
            data.Set(missile.activeInHierarchy, missile.transform.position, missile.transform.rotation);
            missilesData.Add(data);
        }
        _dataToSend.Set(_team.Color, _team.TeamNumber, tanksData, missilesData);
    }
    private void DrawData()
    {
        foreach (ClientData data in _receivedData)
        {
            DrawTanks(data);
            DrawMissiles(data);
        }
    }

    private void DrawTanks(ClientData data)
    {
        List<ObjectData> tanksData = data.Tanks;
        Color teamColor = data.TeamColor;
        List<GameObject> drawnList = _drawnTanks;
        int teamNumber = data.TeamNumber;
        for (int i = 0; i < tanksData.Count; i++)
        {
            if (drawnList.Count <= i)
            {
                drawnList.Add(Instantiate(tank, Vector3.zero, Quaternion.identity));
            }

            drawnList[i].transform.position = tanksData[i].Position;
            drawnList[i].transform.rotation = tanksData[i].Rotation;
            drawnList[i].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = teamColor;
            drawnList[i].SetActive(tanksData[i].Active);
            drawnList[i].GetComponent<Tank>().TeamNumber = teamNumber;
        }
    }
    private void DrawMissiles(ClientData data)
    {
        List<ObjectData> missilesData = data.Missiles;
        List<GameObject> drawnList = _drawnMissiles;
        for (int i = 0; i < missilesData.Count; i++)
        {
            if (drawnList.Count <= i)
            {
                drawnList.Add(Instantiate(missile, Vector3.zero, Quaternion.identity));
            }

            drawnList[i].transform.position = missilesData[i].Position;
            drawnList[i].transform.rotation = missilesData[i].Rotation;
            drawnList[i].SetActive(missilesData[i].Active);
        }
    }
    
    private void ReadData()
    {
        while (_isRunning)
        {
            byte[] sizeBytes = new byte[4];
            int bytesRead = _client.GetStream().Read(sizeBytes, 0, sizeBytes.Length);
            if (bytesRead == 0)
                break;

            int dataSize = BitConverter.ToInt32(sizeBytes, 0);

            byte[] data = new byte[dataSize];
            bytesRead = 0;

            while (bytesRead < dataSize)
            {
                int read = _client.GetStream().Read(data, bytesRead, dataSize - bytesRead);
                if (read == 0)
                    break;
                bytesRead += read;
            }
            if (bytesRead == dataSize)
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                _receivedData = JsonUtility.FromJson<List<ClientData>>(json);
            }
        }
    }

    private void WriteData()
    {
        while (_isRunning)
        {
            _resetEvent.WaitOne();
            MemoryStream memoryStream = new MemoryStream();
            
            string json = JsonUtility.ToJson(_dataToSend);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            
            int intSize = data.Length;
            byte[] dataSize = BitConverter.GetBytes(intSize);
            
            _client.GetStream().Write(dataSize, 0, dataSize.Length);
            _client.GetStream().Write(data, 0, data.Length);
        }
    }
    
    private void StartGame()
    {
        SelectTeam();
        _readThread.Start();
        _writeThread.Start();
        
        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        _gameStarted = true;
    }

    private void SelectTeam()
    {
        _team = teams[Int32.Parse(teamInputField.text)];
        _team.Activate();
    }
    
    private void ReceiveStartMessage()
    {
        try
        {
            byte[] sizeBytes = new byte[4];
            _client.GetStream().Read(sizeBytes, 0, sizeBytes.Length);

            int dataSize = BitConverter.ToInt32(sizeBytes, 0);

            
            byte[] data = new byte[dataSize];
            _client.GetStream().Read(data, 0, data.Length);

            
            bool gameStarted = BitConverter.ToBoolean(data, 0);
            if (gameStarted)
            {
                StartGame();
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError("Exception in ReceiveGameStartMessage: " + e.Message);
        }
    }
    
    void OnDestroy()
    {
        DisconnectFromServer();
    }

    void DisconnectFromServer()
    {
        if (_client != null)
        {
            _client.GetStream().Close();
            _client.Close();
            Debug.Log("Disconnected from server");
        }
    }
    
}

