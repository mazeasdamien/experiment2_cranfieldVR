using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using System.Collections;
using System.Threading;
using TMPro;

public class KinectDepthHandler : MonoBehaviour
{
    public KinectHandler kinectHandler;
    public TMP_Text depthText;
    private int midDepth = -1;
    private int prevDepth = -1;
    private CancellationTokenSource cts;

    void Start()
    {
        if (kinectHandler != null)
        {
            cts = new CancellationTokenSource();
            StartCoroutine(UpdateDepthFrame(cts.Token));
        }
        else
        {
            Debug.LogError("KinectHandler reference is not set");
        }
    }

    void Update()
    {
        if (depthText != null)
        {
            int midDepthInCm = Mathf.CeilToInt(midDepth / 10.0f);
            int prevDepthInCm = Mathf.CeilToInt(prevDepth / 10.0f);

            if (midDepthInCm == 0)
            {
                if (prevDepthInCm > 40)
                {
                    depthText.text = "Error";
                }
                else
                {
                    depthText.text = "Too close to acquire distance";
                }
                depthText.color = Color.red;
            }
            else
            {
                depthText.text = midDepthInCm.ToString() + " cm";
                depthText.color = Color.black;
            }
            prevDepth = midDepth;
        }
    }

    private IEnumerator UpdateDepthFrame(CancellationToken cancellationToken)
    {
        yield return new WaitUntil(() => kinectHandler._device != null);

        ushort[] depthData;

        while (!cancellationToken.IsCancellationRequested)
        {
            Device device = kinectHandler._device;

            if (device != null)
            {
                using (Capture capture = device.GetCapture())
                {
                    Microsoft.Azure.Kinect.Sensor.Image depthImage = capture.Depth;

                    if (depthImage != null)
                    {
                        int centerIndex = (depthImage.WidthPixels / 2) + (depthImage.HeightPixels / 2) * depthImage.WidthPixels;
                        depthData = depthImage.GetPixels<ushort>().ToArray();
                        midDepth = depthData[centerIndex];
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnDestroy()
    {
        if (cts != null)
        {
            cts.Cancel();
        }
    }
}
