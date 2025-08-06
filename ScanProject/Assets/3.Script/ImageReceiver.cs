using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCvSharp;
using System.Text;
using Microsoft.SqlServer.Server;
using OpenCVForUnity.ImgprocModule;

public class ImageReceiver : MonoBehaviour
{
    private TcpListener listener;
    public int port = 9999;
    [SerializeField]
    private ImageAnalysis imageAnalysis;
    void Start()
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Debug.Log("�̹��� ���� �����...");
        AcceptLoop();
    }

    private async void AcceptLoop()
    {
        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            // 1. �̹��� ���� �б� (4����Ʈ)
            byte[] lengthBytes = new byte[4];
            await stream.ReadAsync(lengthBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            int length = BitConverter.ToInt32(lengthBytes, 0);

            // 2. �̹��� ������ �ޱ�
            byte[] imageBytes = new byte[length];
            int totalRead = 0;
            while (totalRead < length)
            {
                int read = await stream.ReadAsync(imageBytes, totalRead, length - totalRead);
                if (read == 0) break;
                totalRead += read;
            }

            // 3. Unity ���ν����忡�� OpenCVForUnity.Mat���� ��ȯ
            MainThreadDispatcher.Enqueue(() =>
            {
                var matOfByte = new OpenCVForUnity.CoreModule.MatOfByte(imageBytes);
                OpenCVForUnity.CoreModule.Mat mat = Imgcodecs.imdecode(matOfByte, Imgcodecs.IMREAD_COLOR);
                if (mat.empty())
                {
                    Debug.LogError("���ڵ� ����");
                }
                else
                {
                    Debug.Log("OpenCVForUnity.Mat ���� �Ϸ�!");
                    Imgproc.cvtColor(mat, mat, Imgproc.COLOR_BGR2RGB);
                    imageAnalysis.ProcessAnalysis(mat);
                    // ���� �м� �� ó��
                }
            });
        }
    }
}
