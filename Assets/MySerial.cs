using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System;

public class MySerial : MonoBehaviour
{
    public enum Baudrate
    {
        B_9600 = 9600,
        B_19200 = 19200,
        B_38400 = 38400,
        B_57600 = 57600,
        B_115200 = 115200,
        B_230400 = 230400,
    }

    public event Action<string> OnDataReceived;
    SerialPort _serialPort;
    Thread _thread;
    Queue<string> _messages;
    string _data = "";
    bool _isRunning;

    // Update is called once per frame
    void Update()
    {
        if (! _isRunning)
        {
            return;
        }
        lock(_messages)
        {
            while (_messages.Count > 0)
            {
                string msg = _messages.Dequeue();
                if (OnDataReceived != null)
                {
                    OnDataReceived(msg);
                }
            }
        }
    }

    public static string[] GetPortNames()
    {
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            return Directory.GetFiles(@"/dev/", "tty.usb*", SearchOption.AllDirectories);
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            return SerialPort.GetPortNames();
        }
        Debug.LogError("Unsupported platform");
        return new string[0];
    }

    public bool Open(string portName = null, Baudrate baudRate = Baudrate.B_9600)
    {
        if (string.IsNullOrEmpty(portName))
        {
            var ports = GetPortNames();
            if (Debug.isDebugBuild)
            {
                foreach (var port in ports)
                {
                    Debug.LogFormat("port : {0}", port);
                }
                if (ports.Length == 0)
                {
                    Debug.LogWarning("Serial port not found");
                }
            }
            if (ports.Length == 0)
            {
                return false;
            }
            portName = ports[0];
        }
        _messages = new Queue<string>();
        _serialPort = new SerialPort(portName, (int)baudRate, Parity.None, 8, StopBits.One);
        try
        {
            _serialPort.Open();
        }
        catch (IOException e)
        {
            Debug.LogError(e);
            _serialPort.Dispose();
            return false;
        }
        _isRunning = true;
        _thread = new Thread(Read);
        _thread.Start();
        return true;
    }

    public void Close()
    {
        _isRunning = false;
        if (_thread != null && _thread.IsAlive)
        {
            _thread.Join();
        }
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }
    }

    void Read()
    {
        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                byte b = (byte)_serialPort.ReadByte();
                while (b != 255 && _isRunning)
                {
                    char c = (char)b;
                    if (c == '\n')
                    {
                        lock (_messages)
                        {
                            _messages.Enqueue(_data);
                            _data = "";
                        }
                    }
                    else
                    {
                        _data += c;
                    }
                    b = (byte)_serialPort.ReadByte();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            Thread.Sleep(1);
        }
    }

    static MySerial _instance;

    public static MySerial Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject(typeof(MySerial).ToString());
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<MySerial>();
            }
            return _instance;
        }
    }
}
