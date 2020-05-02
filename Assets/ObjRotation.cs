using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjRotation : MonoBehaviour
{
    MySerial serial;
    float _pitch;
    float _roll;
    //Vector2 rotation;
    public string portNum;
    //public Transform rotationObj;
    //public string message;

    // Start is called before the first frame update
    void Start()
    {
        serial = MySerial.Instance;
        bool success = serial.Open(portNum, MySerial.Baudrate.B_115200);
        if (! success)
        {
            return;
        }
        serial.OnDataReceived += SerialCallBack;
    }

    private void OnDisable()
    {
        serial.Close();
        serial.OnDataReceived -= SerialCallBack;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SerialCallBack(string m)
    {
        objRotation(m);
        //message = m;
    }

    void objRotation(string message)
    {
        string[] a;
        a = message.Split("="[0]);
        if (a.Length != 2)
        {
            return;
        }
        int v = int.Parse(a[1]);
        switch (a[0])
        {
            case "pitch":
                _pitch = v;
                //rotation = new Vector2(v, rotation.y);
                break;
            case "roll":
                _roll = v;
                //rotation = new Vector2(rotation.x, v);
                break;
        }
        Quaternion AddRot = Quaternion.identity;
        //AddRot.eulerAngles = new Vector3(-rotation.x, 0, -rotation.y);
        AddRot.eulerAngles = new Vector3(-_pitch, 0, -_roll);
        transform.rotation = AddRot;
    }
}
