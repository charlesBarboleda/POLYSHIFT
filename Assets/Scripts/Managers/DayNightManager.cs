using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class DayNightManager : NetworkBehaviour
{
    NetworkVariable<float> _fogDensity = new NetworkVariable<float>(0.03f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    NetworkVariable<float> _sunIntensity = new NetworkVariable<float>(2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] Light _sun;

    float _currentTime = 0f;
    float _dayNightDuration = 300f; // 5 minutes for day/night
    bool _isNight = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }

    void Update()
    {
        if (!IsServer) return;

        // Day/Night cycle logic
        _currentTime += Time.deltaTime;

        if (_currentTime >= _dayNightDuration)
        {
            _currentTime = 0f;
            _isNight = !_isNight; // Toggle between day and night

            // Start transitioning light intensity
            if (_isNight)
            {
                StartLightTransition(0f, 1.5f, _dayNightDuration);
            }
            else
            {
                StartLightTransition(1.5f, 0f, _dayNightDuration);
            }
        }

        // Fog density logic
        if (_fogDensity.Value == 0.03f)
        {
            ChangeFogDensity(0f, Random.Range(60f, 600f)); // 1 to 10 minutes
        }
        else if (_fogDensity.Value == 0f)
        {
            ChangeFogDensity(0.03f, Random.Range(60f, 600f)); // 1 to 10 minutes
        }
    }

    private void StartLightTransition(float fromIntensity, float toIntensity, float duration)
    {
        DOTween.To(() => _sunIntensity.Value, x => _sunIntensity.Value = x, toIntensity, duration)
            .OnUpdate(() =>
            {
                _sun.intensity = _sunIntensity.Value; // Update sun intensity in real-time
            });
    }

    private void ChangeFogDensity(float targetDensity, float duration)
    {
        DOTween.To(() => _fogDensity.Value, x => _fogDensity.Value = x, targetDensity, duration)
            .OnUpdate(() =>
            {
                RenderSettings.fogDensity = _fogDensity.Value; // Update fog density in real-time
            });
    }

}
