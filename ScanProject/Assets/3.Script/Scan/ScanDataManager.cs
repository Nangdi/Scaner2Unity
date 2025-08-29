using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;

public class ScanDataManager : MonoBehaviour
{
    public enum Mode
    {
        Tuning,
        Game
    }
    [SerializeField]
    private ImageAnalysis imageAnalysis;

    [Header("ScanFile감시")]
    private FileSystemWatcher watcher;
    private string[] excludeFileNames = new string[2];

    public Texture2D CurrentScanImage ;
    public Texture2D TestImage ;
    private string folderPath;
    private string filePath;
    public string fileName1;
    public string fileName2;
    public Mode mode;
    

    void Awake()
    {

        //folderPath = Path.Combine(Application.streamingAssetsPath, "DataFiles");
        folderPath = "C:\\scanFile";

        // 폴더가 없으면 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log(" 폴더 생성됨: " + folderPath);
        }

    }
    private void Start()
    {
        watcher = new FileSystemWatcher(folderPath, "*.jpg");
        watcher.Created += OnNewScan;
        watcher.EnableRaisingEvents = true;

        fileName1 = CustomJsonManager.jsonManager.settingData.testFileName1;
        fileName2 = CustomJsonManager.jsonManager.settingData.testFileName2;
        CleanOldFiles();

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            string test = "C:\\scanFile\\";
            LoadTexture(test+ fileName1+ ".jpg");
        }
        else if(Input.GetKeyDown(KeyCode.W))
        {
            string test = "C:\\scanFile\\";
            LoadTexture(test + fileName2 + ".jpg");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
        }


    }
    private void OnNewScan(object sender, FileSystemEventArgs e)
    {
        Debug.Log($"새로운 스캔 감지: {e.FullPath}");
        //MainThreadDispatcher.Enqueue(() =>
        //{
        //    LoadTexture(e.FullPath); // 여기서 Texture2D 생성 + LoadImage 가능
        //});
          LoadTexture(e.FullPath);
    }
    private void CleanOldFiles()
    {
        Debug.Log("파일삭제");
        try
        {
            var files = Directory.GetFiles(folderPath, "*.jpg");

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);

                // 예외 파일은 건너뜀
                if(fileName ==fileName1 || fileName == fileName2)
                    continue;

                DateTime lastWriteTime = File.GetLastWriteTime(file);
                if (DateTime.Now - lastWriteTime > TimeSpan.FromHours(48))
                {
                    try
                    {
                        File.Delete(file);
                        Debug.Log($"삭제됨: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"파일 삭제 실패: {fileName}, 오류: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"폴더 정리 중 오류: {ex.Message}");
        }
    }
    private async void LoadTexture(string path)
    {
        OpenCVForUnity.CoreModule.Mat scannedMat = await Task.Run(() =>
        {
            byte[] imageBytes = File.ReadAllBytes(path);

            OpenCVForUnity.CoreModule.MatOfByte mob = new OpenCVForUnity.CoreModule.MatOfByte(imageBytes);
            OpenCVForUnity.CoreModule.Mat mat = Imgcodecs.imdecode(mob, Imgcodecs.IMREAD_COLOR);

            // 🔄 BGR → RGB 변환
            Imgproc.cvtColor(mat, mat, Imgproc.COLOR_BGR2RGB);

            return mat;
        });

        imageAnalysis.ProcessAnalysis(scannedMat);


    }
    IEnumerator LoadImageFromFile(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path))
        {
            float start = Time.realtimeSinceStartup;
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                yield return null;
                float afterDownload = Time.realtimeSinceStartup;
                byte[] fileData = File.ReadAllBytes(path);


                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);  // 이게 GPU 
                yield return null; // 한 프레임 쉬고

                float afterGetContent = Time.realtimeSinceStartup;

                CurrentScanImage = tex;
                yield return null; // 다시 한 프레임 쉬고
                float afterAssign = Time.realtimeSinceStartup;
                //imageAnalysis.ProcessAnalysis(CurrentScanImage);
                Debug.Log("로드 성공");
                Debug.Log($"다운로드 시간: {afterDownload - start:F4}s");
                Debug.Log($"텍스처 생성 시간: {afterGetContent - afterDownload:F4}s");
                Debug.Log($"텍스처 할당 시간: {afterAssign - afterGetContent:F4}s");
            }
            else
            {
                Debug.LogError("이미지 로드 실패: " + uwr.error);
            }
        }
    }
    private async Task WaitForFileAvailable(string path)
    {
        for (int i = 0; i < 10; i++) // 최대 1초까지 시도
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (stream.Length > 0)
                    {
                        return;
                    }
                }
            }
            catch (IOException)
            {
                // 파일이 아직 저장 중이면 예외 발생
            }

            await Task.Delay(100); // 0.1초 대기 후 다시 시도
        }

        Debug.LogError("파일이 열리지 않거나 저장되지 않았습니다: " + path);
    }
    IEnumerator GetFileName()
    {
        yield return new WaitForSeconds(1);
        fileName1 = GameManager.instance.gameSettingData.testFileName1;
        fileName2 = GameManager.instance.gameSettingData.testFileName2;
        CleanOldFiles();
    }
}
