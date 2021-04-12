using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class UiConsole : MonoBehaviour
 {
     [SerializeField] GameObject consoleCanvas;
     [SerializeField] Text fps;
     [SerializeField] Text consoleText;
     [SerializeField] InputField input;
     private string _myLog = "";
     private int _maxChar = 15000;
     private bool _doShow = true;
     private string _lastPing;
     void OnEnable() { Application.logMessageReceived += Log; }
     void OnDisable() { Application.logMessageReceived -= Log; }

     private float framerateThisFrame;
     void Update()
     {
         if (Input.GetKeyDown(KeyCode.Space))
         {
             consoleCanvas.SetActive(true);
         }
     
         if (Input.touchCount > 2)
         {
             consoleCanvas.SetActive(true);
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
         
         //prevent from vertex error
         if (_myLog.Length > _maxChar) { _myLog = _myLog.Remove(_myLog.LastIndexOf("----------")); }

         consoleText.text = _myLog;
     }

     private void Start()
     {
         StartCoroutine(FpsCalculator());
         InvokeRepeating(nameof(PingRepeat),0f,5f);
         MakeSomeError();
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
             fps.text = "FPS:~ " + framerateThisFrame.ToString() + " | DELAY:~ " + 
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

     public void ToggleCmd()
     {
         _doShow = !_doShow;
         consoleCanvas.SetActive(_doShow);
     }
 }
