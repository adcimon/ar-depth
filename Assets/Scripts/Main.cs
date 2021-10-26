/*
|--------------------------------------------------------------------------
| Documentation
|--------------------------------------------------------------------------
|
| https://developers.google.com/ar/develop/unity-arf
| https://developers.google.com/ar/develop/unity-arf/depth/introduction
| https://developers.google.com/ar/develop/unity-arf/depth/developer-guide
| https://developers.google.com/ar/develop/unity-arf/depth/raw-depth
| https://github.com/google-ar/arcore-unity-extensions/
| https://github.com/Unity-Technologies/arfoundation-samples
|
*/

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

public class Main : MonoBehaviour
{
    public AROcclusionManager occlusionManager;
    public Material blitMaterial;
    public Logger logger;
    public UnityEvent<Texture2D> onDepthTexture;

    private bool initialized = false;
    private Texture2D depthTexture;

    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private void Update()
    {
        if( !initialized )
        {
            return;
        }

        XRCpuImage cpuImage;
        if( occlusionManager && occlusionManager.TryAcquireEnvironmentDepthCpuImage(out cpuImage) )
        {
            using( cpuImage )
            {
                bool recreated = UpdateDepthTexture(ref depthTexture, cpuImage);
                if( recreated )
                {
                    onDepthTexture?.Invoke(depthTexture);
                }

                LogInfo();

                cpuImage.Dispose();
            }
        }
    }

    private IEnumerator Initialize()
    {
        yield return new WaitForSeconds(3);

        // Check if AR is supported.
        if( occlusionManager == null || occlusionManager.descriptor == null )
        {
            logger.Error("Occlusion manager or descriptor is null");
            yield return null;
        }

        LogInfo();

        initialized = true;
    }

    private bool UpdateDepthTexture( ref Texture2D texture, XRCpuImage cpuImage )
    {
        bool recreated = false;

        // If the texture hasn't yet been created, or if its dimensions or format have changed, (re)create the texture.
        // Although texture dimensions do not normally change frame-to-frame, they can change in response to a change in the camera resolution (for camera images)
        // or changes to the quality of the human depth and human stencil buffers.
        if( (texture == null) || (texture.width != cpuImage.width) || (texture.height != cpuImage.height) || (texture.format != cpuImage.format.AsTextureFormat()) )
        {
            texture = new Texture2D(cpuImage.width, cpuImage.height, cpuImage.format.AsTextureFormat(), false);
            recreated = true;
        }

        // Mirror about the horizontal axis.
        XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams(cpuImage, cpuImage.format.AsTextureFormat(), XRCpuImage.Transformation.MirrorX);

        // Get the texture's underlying pixel data.
        NativeArray<byte> data = depthTexture.GetRawTextureData<byte>();

        // Make sure the pixel data is the same size as the converted data.
        if( data != null && data.Length == cpuImage.GetConvertedDataSize(conversionParams.outputDimensions, conversionParams.outputFormat) )
        {
            cpuImage.Convert(conversionParams, data);
            texture.Apply();
            data.Dispose();
        }

        return recreated;
    }

    private void LogInfo()
    {
        string message = "";

        message += "Environment depth image: " + occlusionManager?.descriptor?.supportsEnvironmentDepthImage + "\n";
        message += "Environment depth confidence image: " + occlusionManager?.descriptor?.supportsEnvironmentDepthConfidenceImage + "\n";
        message += "Environment depth mode: " + occlusionManager?.currentEnvironmentDepthMode.ToString() + "\n";
        message += "Occlusion preference mode: " + occlusionManager?.currentOcclusionPreferenceMode.ToString() + "\n";
        message += "Depth texture: " + ((depthTexture == null) ? "0x0" : (depthTexture?.width + "x" + depthTexture?.height)) + "\n";

        logger.Info(message);
    }
}