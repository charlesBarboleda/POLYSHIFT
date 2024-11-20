using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections;


public class CameraController : NetworkBehaviour
{
    public CinemachineBasicMultiChannelPerlin isometricShakeSource;
    public CinemachineBasicMultiChannelPerlin firstPersonShakeSource;

    public void TriggerShake(float intensity = 1.0f, float duration = 0.6f)
    {

        StartCoroutine(ShakeCamera(intensity, duration));

    }

    IEnumerator ShakeCamera(float intensity, float duration)
    {
        // Set the Amplitude and Duration directly on each shake source
        isometricShakeSource.AmplitudeGain = intensity;
        isometricShakeSource.FrequencyGain = intensity * 2.0f;
        firstPersonShakeSource.AmplitudeGain = intensity;
        firstPersonShakeSource.FrequencyGain = intensity * 2.0f;


        // Wait for the duration of the shake
        yield return new WaitForSeconds(duration);

        // Lerp the Amplitude back to zero
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            isometricShakeSource.AmplitudeGain = Mathf.Lerp(intensity, 0, t);
            isometricShakeSource.FrequencyGain = Mathf.Lerp(intensity * 2.0f, 0, t);
            firstPersonShakeSource.AmplitudeGain = Mathf.Lerp(intensity, 0, t);
            firstPersonShakeSource.FrequencyGain = Mathf.Lerp(intensity * 2.0f, 0, t);
            yield return null;
        }
    }

}
