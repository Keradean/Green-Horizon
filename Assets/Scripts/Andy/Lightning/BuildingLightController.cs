using UnityEngine;
using Dennis.DayAndNight;
//=== Andy ===//

[System.Serializable]
public class WindowPair
{
    public Material dayMaterial;
    public Material nightMaterial;
}

public class BuildingLightController : MonoBehaviour
{
    [Header("Lichter")]
    [SerializeField] private Light[] lights;

    [Header("Fenster")]
    [SerializeField] private WindowPair[] windows;

    private Renderer _renderer;
    private int[] _windowMaterialIndices;
    private bool _pendingIsDay;
    private bool _isPlaced = false;

    private void Awake()
    {
        foreach (var light in lights)
            light.enabled = false;

        _renderer = GetComponentInChildren<Renderer>();
        _windowMaterialIndices = new int[windows.Length];

        for (var w = 0; w < windows.Length; w++)
        {
            _windowMaterialIndices[w] = -1;
            for (var i = 0; i < _renderer.sharedMaterials.Length; i++)
            {
                if (_renderer.sharedMaterials[i] != windows[w].dayMaterial) continue;
                _windowMaterialIndices[w] = i;
                break;
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

    public void OnPlaced()
    {
        _isPlaced = true;
        var isDay = !IsNight();
        OnDayNightChanged(isDay);
    }

    private bool IsNight()
    {
        var lightManager = FindAnyObjectByType<LightManager>();
        if (lightManager == null) return false;
        var time = lightManager.TimeOfDay;
        return time < 6f || time >= 18f;
    }

    private void OnDayNightChanged(bool isDay)
    {
        if (!_isPlaced) return;
        _pendingIsDay = isDay;
        Invoke(nameof(ApplyLighting), Random.Range(0f, 2f));
    }

    private void ApplyLighting()
    {
        foreach (var light in lights)
            light.enabled = !_pendingIsDay;

        var mats = _renderer.materials;
        for (var w = 0; w < windows.Length; w++)
        {
            if (_windowMaterialIndices[w] == -1) continue;
            mats[_windowMaterialIndices[w]] = _pendingIsDay
                ? windows[w].dayMaterial
                : windows[w].nightMaterial;
        }
        _renderer.materials = mats;
    }
}