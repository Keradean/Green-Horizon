using UnityEngine;
using Dennis.DayAndNight;
//=== Andy ===//

[System.Serializable]
public class CarWindowPair
{
    public Material dayMaterial;
    public Material nightMaterial;
}

public class CarLightController : MonoBehaviour
{
    [Header("Lichter")]
    [SerializeField] private Light[] headlights;
    [SerializeField] private Light[] taillights;

    [Header("Materialien")]
    [SerializeField] private CarWindowPair[] windows;

    private Renderer _carRenderer;
    private int[] _windowMaterialIndices;
    private bool _isDay = true;

    private void Awake()
    {
        SetLights(false);

        _carRenderer = GetComponentInChildren<Renderer>();

        if (_carRenderer != null && windows != null)
        {
            _windowMaterialIndices = new int[windows.Length];
            for (var w = 0; w < windows.Length; w++)
            {
                _windowMaterialIndices[w] = -1;
                for (var i = 0; i < _carRenderer.sharedMaterials.Length; i++)
                {
                    if (_carRenderer.sharedMaterials[i] != windows[w].dayMaterial) continue;
                    _windowMaterialIndices[w] = i;
                    break;
                }
            }
        }
    }

    private void OnEnable()
    {
        LightManager.OnDayNightChanged += OnDayNightChanged;
    }

    private void OnDisable()
    {
        LightManager.OnDayNightChanged -= OnDayNightChanged;
    }

    private void Start()
    {
        var lightManager = FindAnyObjectByType<LightManager>();
        if (lightManager != null)
        {
            var time = lightManager.TimeOfDay;
            _isDay = time >= 6f && time < 18f;
        }

        Invoke(nameof(ApplyLights), Random.Range(0f, 0.5f));
    }

    private void OnDayNightChanged(bool isDay)
    {
        _isDay = isDay;
        Invoke(nameof(ApplyLights), Random.Range(0f, 2f));
    }

    private void ApplyLights()
    {
        SetLights(!_isDay);
        ApplyMaterials();
    }

    private void SetLights(bool on)
    {
        foreach (var light in headlights)
            if (light != null) light.enabled = on;
        foreach (var light in taillights)
            if (light != null) light.enabled = on;
    }

    private void ApplyMaterials()
    {
        if (_carRenderer == null || windows == null) return;

        var mats = _carRenderer.materials;
        for (var w = 0; w < windows.Length; w++)
        {
            if (_windowMaterialIndices[w] == -1) continue;
            mats[_windowMaterialIndices[w]] = _isDay
                ? windows[w].dayMaterial
                : windows[w].nightMaterial;
        }
        _carRenderer.materials = mats;
    }
}