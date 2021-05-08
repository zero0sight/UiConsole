using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class UiConsole : MonoBehaviour
 {
     [Header("Required Components")]
     [SerializeField] GameObject consoleCanvas;
     [SerializeField] Text fps;
     [SerializeField] Text consoleText;
     [SerializeField] Image circle;
     [SerializeField] InputField input;
     
     [Header("Settings")]
     [SerializeField][Range(1, 10)] int timeOut = 2;
     [SerializeField] string connectionServer;
     
     private string _myLog = "";
     private int _maxChar = 15000;
     private bool _doShow = true;
     private string _lastPing;
     
     void OnEnable() { Application.logMessageReceived += Log; }
     void OnDisable() { Application.logMessageReceived -= Log; }

     private float framerateThisFrame;
     void Update()
     {
         if (Input.GetKeyDown(KeyCode.BackQuote))
         {
             ToggleCmd();
         }
         else if (Input.touchCount > 3)
         {
             ToggleCmd();
         }
         else if (Input.GetKeyDown(KeyCode.Return))
         {
             ReadInputValue();
             input.text = "";
         }
     }
     private void Log(string logString, string stackTrace, LogType type)
     {
         //append first
         if (logString.Contains("Exception") || logString.Contains("exception"))
         {
             _myLog = "\n" + "<color=red>"  + GetTime() + "-" + stackTrace + "\n" + logString + "</color>" + "\n----------" +  _myLog;
         }
         else if (logString.Contains(">_: "))
         {
             if (logString.Contains("clear"))
             {
                 _myLog = "\n" + "<color=cyan>" + GetTime() + "-" + logString + "</color>" + "\n----------";
             }
             else
             {
                 _myLog = "\n" + "<color=cyan>" + GetTime() + "-" + logString + "</color>" + "\n----------" +  _myLog;
             }
         }
         else
         {
             _myLog = "\n" + GetTime() + "-" + stackTrace + "\n" + logString + "\n----------" + _myLog;
         }
         
         UpdateConsoleText();
     }

     private void UpdateConsoleText()
     {
         //prevent from vertex error
         if (_myLog.Length > _maxChar) { _myLog = _myLog.Remove(_myLog.LastIndexOf("----------")); }

         consoleText.text = _myLog;
     }

     private void Start()
     {
         StartCoroutine(FpsCalculator());
         InvokeRepeating(nameof(PingRepeat),0f,5f);
         InvokeRepeating(nameof(CheckInternetConnection),0f,2f);
         //MakeSomeError();
     }

     private void MakeSomeError()
     {
         print("CHEESE");
         GetComponent<Rigidbody2D>().velocity = Vector2.down;
     }

     private void PingRepeat()
     {
         StartCoroutine(PingCalculator());
     }
     IEnumerator FpsCalculator()
     {
         while (true)
         {
             framerateThisFrame  = Mathf.RoundToInt(1/Time.deltaTime);
             fps.text = "FPS:~ " + framerateThisFrame + " | DELAY:~ " + 
                        Mathf.RoundToInt(Time.deltaTime * 100000f)/100f +
                        " | PING:~ " + _lastPing;
             yield return new WaitForSeconds(.25f);
         }
     }

     IEnumerator PingCalculator()
     {
         Ping ping = new Ping("8.8.8.8");
         float t = 0;
         while (!ping.isDone && t < 4f)
         {
             t += Time.deltaTime;
             yield return null;
         }
         if (ping.time == -1)
         {
             _lastPing = "XX";
         }
         else
         {
             _lastPing = ping.time.ToString();
         }
     }

     public void CheckInternetConnection()
     {
         StartCoroutine(CheckInternetConnectionCoroutine(connection =>
         {
             if (connection)
             {
                 //print("Connected");
                 circle.color = Color.green;
             }
             else
             {
                 //print("Not Connected");
                 circle.color = Color.red;
             }
         }));
     }

     private IEnumerator CheckInternetConnectionCoroutine(Action<bool> syncResult)
     {
         bool connectivityResult;
         using (var request = UnityWebRequest.Head(connectionServer))
         {
             request.timeout = timeOut;
             yield return request.SendWebRequest();
             connectivityResult = request.result != UnityWebRequest.Result.ConnectionError &&
                                  request.result != UnityWebRequest.Result.ProtocolError &&
                                  request.result != UnityWebRequest.Result.DataProcessingError &&
                                  request.responseCode == 200;
         }
         syncResult(connectivityResult);
     }

     public void ReadInputValue()
     {
         print($">_: {input.text}");
     }

     private string GetTime()
     {
         DateTime theTime = DateTime.Now;
         //string date = theTime.ToString("yyyy-MM-dd\\Z");
         string time = theTime.ToString("HH:mm:ss");
         //string datetime = theTime.ToString("yyyy-MM-dd\\THH:mm:ss\\Z");
         return time;
     }

     private IEnumerator ApiCaller(string apiUrl, Action<string> result)
     {
         UnityWebRequest uwr = UnityWebRequest.Get(apiUrl);
         uwr.timeout = timeOut;
         yield return uwr.SendWebRequest();
         print(uwr.responseCode);
         result(uwr.downloadHandler.text);
         
         
         //if request is not done in given time halts it
         //if (!uwr.isDone) { uwr.Abort(); }
     }

     public void ToggleCmd()
     {
         _doShow = !_doShow;
         consoleCanvas.SetActive(_doShow);
     }

     public void ApiCall()
     {
         string url = input.text;
         if (url == string.Empty)
         {
             return;
         }

         StartCoroutine(ApiCaller(url, result =>
         {
             
             _myLog = "\n" + "<color=green>" + GetTime() + "-" + result + "</color>" + "\n----------" +  _myLog;
             UpdateConsoleText();
         }));
     }

     public IEnumerator FileDownloader()
     {
         UnityWebRequest uwr = UnityWebRequest.Get("");
         //uwr.timeout = timeOut;
         yield return uwr.SendWebRequest();
         print(uwr.responseCode);
         //uwr.downloadHandler.data
         //result(uwr.downloadHandler.text);
         //System.IO.File.
     }
 }
