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
        Debug.Log("이미지 수신 대기중...");
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
            // 1. 이미지 길이 읽기 (4바이트)
            byte[] lengthBytes = new byte[4];
            await stream.ReadAsync(lengthBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            int length = BitConverter.ToInt32(lengthBytes, 0);

            // 2. 이미지 데이터 받기
            byte[] imageBytes = new byte[length];
            int totalRead = 0;
            while (totalRead < length)
            {
                int read = await stream.ReadAsync(imageBytes, totalRead, length - totalRead);
                if (read == 0) break;
                totalRead += read;
            }

            // 3. Unity 메인스레드에서 OpenCVForUnity.Mat으로 변환
            MainThreadDispatcher.Enqueue(() =>
            {
                var matOfByte = new OpenCVForUnity.CoreModule.MatOfByte(imageBytes);
                OpenCVForUnity.CoreModule.Mat mat = Imgcodecs.imdecode(matOfByte, Imgcodecs.IMREAD_COLOR);
                if (mat.empty())
                {
                    Debug.LogError("디코딩 실패");
                }
                else
                {
                    Debug.Log("OpenCVForUnity.Mat 생성 완료!");
                    Imgproc.cvtColor(mat, mat, Imgproc.COLOR_BGR2RGB);
                    imageAnalysis.ProcessAnalysis(mat);
                    // 이후 분석 등 처리
                }
            });
        }
    }
}
