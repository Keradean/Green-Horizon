using UnityEngine;
using Dennis.DayAndNight;
//=== Andy ===//

public class BuildingLightController : MonoBehaviour
{
    [Header("Lichter")]
    [SerializeField] private Light[] lights;

    [Header("Fenster")]
    [SerializeField] private Material windowMaterial;
    [SerializeField] private Material nightWindow;

    private Renderer _renderer;
    private int _windowMaterialIndex = -1;
    private Material _dayWindow;
    private bool _pendingIsDay;
    private bool _isPlaced = false;

    private void Awake()
    {
        foreach (var light in lights)
            light.enabled = false;

        _renderer = GetComponentInChildren<Renderer>();

        for (var i = 0; i < _renderer.sharedMaterials.Length; i++)
        {
            if (_renderer.sharedMaterials[i] != windowMaterial) continue;
            _windowMaterialIndex = i;
            _dayWindow = _renderer.sharedMaterials[i];
            break;
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
        var lightManager = FindFirstObjectByType<LightManager>();
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

        if (_windowMaterialIndex == -1) return;

        var mats = _renderer.materials;
        mats[_windowMaterialIndex] = _pendingIsDay ? _dayWindow : nightWindow;
        _renderer.materials = mats;
    }
}