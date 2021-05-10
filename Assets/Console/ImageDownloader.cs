using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageDownloader : MonoBehaviour
{
    //https://ftopx.com/images/201012/ftop.ru_13381.jpg
    [SerializeField] private Slider _slider;
    [SerializeField] private InputField[] _inputFields;
    [SerializeField] private Text currentUrl;
    [SerializeField] private Text curentFailed;
    
    public string[] urlParts = null;
    public string totalUrl = "";
    
    private string savePathFolder = "";
    private int faildTimes = 0;

    private void Start()
    {
        //Save Path init
        savePathFolder = Path.Combine(Application.persistentDataPath, "data");
        savePathFolder = Path.Combine(savePathFolder, "images");
    }
    public void StartDownload()
    {
        urlParts[1]= _inputFields[0].text;
        urlParts[2]= _inputFields[1].text;
        urlParts[4]= _inputFields[2].text;
        StartCoroutine(SubMain());
    }
    public void StopDownload()
    {
        StopAllCoroutines();
    }

    private IEnumerator SubMain()
    {
        MakeUrl();
        string[] words = totalUrl.Split('/');
        
        string savePath = "";
        savePath = Path.Combine(savePathFolder, words[words.Length - 1]);
        currentUrl.text = totalUrl;
        
        //print("wow "+ savePath);
        //if file exist don't download it even if its broken!
         if (File.Exists(savePath))
         {
             print("file skipped");
             GoToNextUrl();
             yield break;
         }
        
        bool res = false;
        yield return CheckUrlExist(result => { res = result;}, totalUrl);
        if (res)
        {
            faildTimes = 0;
            yield return DownloadData(totalUrl, savePath);
            GoToNextUrl();
        }
        else
        {
            Debug.Log("jpg failed");
            //check for jpeg or png
            
            string tempUrl = totalUrl;
            tempUrl.Replace(".jpg", ".jpeg");
            yield return CheckUrlExist(result => { res = result;}, tempUrl);
            if (res)
            {
                faildTimes = 0;
                yield return DownloadData(totalUrl, savePath.Replace(".jpg", ".jpeg"));
                GoToNextUrl();
            }
            else
            {
                Debug.Log("jpeg failed");
                tempUrl = totalUrl;
                tempUrl.Replace(".jpg", ".png");
                yield return CheckUrlExist(result => { res = result;}, tempUrl);
                if (res)
                {
                    faildTimes = 0;
                    yield return DownloadData(totalUrl, savePath.Replace(".jpg", ".png"));
                    GoToNextUrl();
                }
                else
                {
                    Debug.Log("png failed");
                    faildTimes ++;
                    if (faildTimes > 20)
                    {
                        faildTimes = 0;
                        GoToNextCollection();
                    }
                    else
                    {
                        GoToNextUrl();
                    }
                }
            }
        }
        curentFailed.text = faildTimes.ToString();
        yield return null;
    }

    private void MakeUrl()
    {
        totalUrl = "";
        foreach (var item in urlParts)
        {
            totalUrl += item;
        }
        //Debug.Log(totalUrl);
    }

    private void GoToNextUrl()
    {
        urlParts[4] = (int.Parse(urlParts[4]) + 1).ToString();
        StartCoroutine(SubMain());
    }

    private void GoToNextCollection()
    {
        //int year = int.Parse(urlParts[1]);
        //int mounth = int.Parse(urlParts[2]);
        if (int.Parse(urlParts[2]) < 12)
        {
            urlParts[2] = (int.Parse(urlParts[2]) + 1).ToString();
        }
        else
        {
            urlParts[2] = "01";
            urlParts[1] = (int.Parse(urlParts[1]) + 1).ToString();
        }
        StartCoroutine(SubMain());
    }
    private IEnumerator CheckUrlExist(Action<bool> syncResult,string url)
    {
        bool connectivityResult;
        using (var request = UnityWebRequest.Head(url))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();
            connectivityResult = request.result != UnityWebRequest.Result.ConnectionError &&
                                 request.result != UnityWebRequest.Result.ProtocolError &&
                                 request.result != UnityWebRequest.Result.DataProcessingError &&
                                 request.responseCode == 200;
        }
        syncResult(connectivityResult);
    }
    private IEnumerator DownloadData(string url, string savePath)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.downloadHandler = new DownloadHandlerFile(savePath);
            uwr.SendWebRequest();

            while (!uwr.isDone)
            {
                _slider.value = uwr.downloadProgress;
                yield return null;
            }

            yield return null;
            
            // if (uwr.isNetworkError || uwr.isHttpError)
            // {
            //     Debug.LogError(uwr.error);
            // }
            // else
            // {
            //     Debug.Log("Success");
            //     Texture myTexture = DownloadHandlerTexture.GetContent(uwr);
            //     //Debug.Log(myTexture.);
            //     
            //     
            //     byte[] results = uwr.downloadHandler.data;
            //     saveImage(savePath, results);
            //
            // }
        }
    }

    // void saveImage(string path, byte[] imageBytes)
    // {
    //     //Create Directory if it does not exist
    //     if (!Directory.Exists(Path.GetDirectoryName(path)))
    //     {
    //         Directory.CreateDirectory(Path.GetDirectoryName(path));
    //     }
    //
    //     try
    //     {
    //         File.WriteAllBytes(path, imageBytes);
    //         Debug.Log("Saved Data to: " + path.Replace("/", "\\"));
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogWarning("Failed To Save Data to: " + path.Replace("/", "\\"));
    //         Debug.LogWarning("Error: " + e.Message);
    //     }
    // }

    // byte[] loadImage(string path)
    // {
    //     byte[] dataByte = null;
    //
    //     //Exit if Directory or File does not exist
    //     if (!Directory.Exists(Path.GetDirectoryName(path)))
    //     {
    //         Debug.LogWarning("Directory does not exist");
    //         return null;
    //     }
    //
    //     if (!File.Exists(path))
    //     {
    //         Debug.Log("File does not exist");
    //         return null;
    //     }
    //
    //     try
    //     {
    //         dataByte = File.ReadAllBytes(path);
    //         Debug.Log("Loaded Data from: " + path.Replace("/", "\\"));
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogWarning("Failed To Load Data from: " + path.Replace("/", "\\"));
    //         Debug.LogWarning("Error: " + e.Message);
    //     }
    //
    //     return dataByte;
    // }
}