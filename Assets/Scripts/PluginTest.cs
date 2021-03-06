﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PluginTest : MonoBehaviour
{

#if UNITY_IOS

    private delegate void intCallback(int result);

    [DllImport("__Internal")]
    private static extern double IOSgetElapsedTime();

    [DllImport("__Internal")]
    private static extern void IOScreateNativeAlert(string[] strings, int stringCount, intCallback callback);

    [DllImport("__Internal")]
    private static extern void IOSshowWebView(string URL, int pixelSpace);

    [DllImport("__Internal")]
    private static extern void IOShideWebView(intCallback callback);

#endif

    // Use this for initialization
    void Start()
    {

        Debug.Log("Elapsed Time: " + getElapsedTime());
        StartCoroutine(ShowDialog(Random.Range(7, 12)));
    }

    int frameCounter = 0;
    // Update is called once per frame
    void Update()
    {

        frameCounter++;
        if (frameCounter >= 5)
        {
            Debug.Log("Tick: " + getElapsedTime());
            frameCounter = 0;
        }
    }

    IEnumerator ShowDialog(float delayTime)
    {
        Debug.Log("Will show alert after " + delayTime + " seconds");
        if (delayTime > 0)
            yield return new WaitForSeconds(delayTime);
        CreateIOSAlert(new string[] { "Title", "Message", "DefaultButton", "OtherButton" });
    }


    double getElapsedTime()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            return IOSgetElapsedTime();
        Debug.LogWarning("Wrong platform!");
        return 0;
    }

    [AOT.MonoPInvokeCallback(typeof(intCallback))]
    static void nativeAlertHandler(int result)
    {
        Debug.Log("Unity: clicked button at index: " + result);
    }

    public void CreateIOSAlert(string[] strings)
    {
        if (strings.Length < 3)
        {
            Debug.LogError("Alert requires at least 3 strings!");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer)
            IOScreateNativeAlert(strings, strings.Length, nativeAlertHandler);
        else
            Debug.LogWarning("Can only display alert on iOS");
        Debug.Log("Alert shown after: " + getElapsedTime() + " seconds");
    }

    public RectTransform webPanel;
    public RectTransform buttonStrip;
    public void OpenWebView(string url, int pixelShift)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            IOSshowWebView(url, pixelShift);
        }
    }

    public void CloseWebView(System.Action<int> closeComplete)
    {
        onCloseWebView = closeComplete;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            IOShideWebView(closeWebViewHandler);
        }
        else
            closeWebViewHandler(0);
    }

    [AOT.MonoPInvokeCallback(typeof(intCallback))]
    static void closeWebViewHandler(int result)
    {
        if (onCloseWebView != null)
            onCloseWebView(result);
        onCloseWebView = null;
    }
    static System.Action<int> onCloseWebView;



    public void OpenWebViewTapped()
    {
        Canvas parentCanvas = buttonStrip.GetComponentInParent<Canvas>();
        int stripHeight = (int)(buttonStrip.rect.height * parentCanvas.scaleFactor + 0.5f);
        webPanel.gameObject.SetActive(true);
        OpenWebView("http://www.cwgtech.com", stripHeight);
    }

    public void CloseWebViewTapped()
    {
        CloseWebView((int result) =>
        {
            webPanel.gameObject.SetActive(false);
        });
    }
}