using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    private TcpClient _client;
    private bool _isRunning;
    private Thread _readThread;
    private Thread _writeThread;

    private int _drawnIndex;
    
    private ClientData _receivedData;
    private List<ClientData> _dataToSend;
    
    private AutoResetEvent _serverResetEvent;

    public ClientData ReceivedData => _receivedData;
    public List<ClientData> DataToSend => _dataToSend;
    public TcpClient Client => _client;
    public int DrawnIndex => _drawnIndex;
    
    public void Initialize(TcpClient client, AutoResetEvent resetEvent, int drawnIndex)
    {
        _client = client;
        _serverResetEvent = resetEvent;
        _isRunning = true;

        _drawnIndex = drawnIndex;
        
        _readThread = new Thread(ReadThread);
        _writeThread = new Thread(WriteThread);
        
        _readThread.Start();
        _writeThread.Start();
    }

    
    // reading single client data
    private void ReadThread()
    {
        BinaryFormatter formatter = new BinaryFormatter();
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
                using (MemoryStream memoryStream = new MemoryStream(data))
                    _receivedData = (ClientData)formatter.Deserialize(memoryStream);
            }
        }
    }
    
    // writing List
    private void WriteThread()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        while (_isRunning)
        {
            _serverResetEvent.WaitOne();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, _dataToSend);
                byte[] data = memoryStream.ToArray();
                int intSize = data.Length;
                byte[] dataSize = BitConverter.GetBytes(intSize);
                
                _client.GetStream().Write(dataSize, 0, dataSize.Length);
                _client.GetStream().Write(data, 0, data.Length);
            }
            
            _dataToSend.Clear();
        }
    }
    
    public void Shutdown()
    {
        _isRunning = false;
        _readThread.Abort();
        _writeThread.Abort();
        _client.Close();
    }
}
