using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SerialPortManager : MonoBehaviour
{
    public static SerialPortManager Instance { get; private set; }

    PortJson portJson;

    SerialPort serialPort;
    private CancellationTokenSource cancellationTokenSource; // CancellationTokenSource �߰�
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);

        }
        else
        {
            Destroy(gameObject);
        }
       
    }
    protected virtual void Start()
    {
        // ��Ʈ ����
        portJson = CustomJsonManager.jsonManager.LoadPortData();
        Debug.Log($"��Ʈ ������ �ε��: COM={portJson.com}, Baud={portJson.baudLate}");
        serialPort = new SerialPort(portJson.com, portJson.baudLate, Parity.None, 8, StopBits.One);
        Debug.Log("��Ʈ����õ�");
        //serialPort.ReadTimeout = 500;
        serialPort.Open();
        if (serialPort.IsOpen)
        {

            Debug.Log("����Ϸ�");
            StartSerialPortReader();
        }
    }


    // ������ �б�
    void Update()
    {
    
    }
    async void StartSerialPortReader()
    {
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        while (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                // �����͸� ����

                string input = await Task.Run(() => ReadSerialData() , token);
                string data = GetData(input);

                if (!string.IsNullOrEmpty(data) && data.Length >= 3)
                {
                    Debug.Log("���������� : " + data);
                    ReceivedData(data);
                }

            }
            catch (TimeoutException ex)
            {
                // �����Ͱ� ���� ���� ����
                Debug.LogWarning("������ ���� �ð� �ʰ�: " + ex.Message);
            }
        }
    }
    private string ReadSerialData()
    {
        try
        {
           
            string input = serialPort.ReadExisting().Trim(); // ������ �б�
            Debug.Log(input);
            return GetData(input);
            //return serialPort.ReadLine(); // ������ �б�
        }
        catch (TimeoutException)
        {
            return null; // �ð� �ʰ� �� null ��ȯ
        }
    }
    private string GetData(string input)
    {
        //input �����Ͱ� 80�����ԵǸ� 80�� �״������ڿ� �ش��ϴ� ���ڿ� ��ŭ ��ȯ.
        if (input == null)
        {
            return null;
        }
        if (input.Contains("80"))   
        {
            int index = input.IndexOf("80");
            if (index + 3 < input.Length)
            {
                // "80" ���� ���ڱ��� �����Ͽ� �ڸ���
                // �Ǵ� 0D ����ڵ�� �����ϱ�
                //Debug.Log(data.Substring(index, 3));
                return input.Substring(index  , 4);
            }
        }
        return "";
    }

    protected void SendData(string message)
    {
        if (serialPort.IsOpen)
        {
            try
            {
                serialPort.WriteLine(message); // �޽��� �۽� (�� �ٲ� �߰�)
                Debug.Log("Sent: " + message);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("�۽� ����: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("��Ʈ�� ���� ���� ���� - �۽� ����");
        }
    }

    void OnApplicationQuit()
    {
        // ��Ʈ �ݱ�

        // ���� �� ������ ���� �� ��Ʈ �ݱ�

        if (cancellationTokenSource != null)
        {
            Debug.Log("Task ����");
            cancellationTokenSource.Cancel(); // �۾� ���
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }

    }
    protected virtual void ReceivedData(string data)
    { 
        //����ϰ� ���������ͷ� ������Ʈ�� �´� ��� ����

    }




}
