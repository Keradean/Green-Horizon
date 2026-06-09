using System;
using UnityEngine;
using LightType = UnityEngine.LightType;
using RenderSettings = UnityEngine.RenderSettings;

namespace Dennis.DayAndNight
{
    [ExecuteAlways] // sorgt dafür, dass dieses Script auch im Editor läuft, nicht nur wenn man auf Play drückst(mit vorsicht zu genießen).
    public class LightManager : MonoBehaviour
    {
        [SerializeField] private LightingPreset preset;
        [SerializeField] private Light directionalLight;
        [SerializeField, Range(0,24)] private float timeOfDay;
        
        [SerializeField] private float dayTimeDurationInMinutes;
        [SerializeField] private float nightTimeDurationInMinutes;
        //////////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            if (preset == null) return;
            if (Application.isPlaying)
            {
                var isDay = timeOfDay is >= 6f and < 18f;
                var durationSeconds = (isDay ? dayTimeDurationInMinutes : nightTimeDurationInMinutes) * 60;
                var hoursPerSecond = 12f / durationSeconds;
                
                timeOfDay += hoursPerSecond * Time.deltaTime;
                timeOfDay %= 24;
            }
            UpdateLighting(timeOfDay / 24f);
        }
        //////////////////////////////////////////////////////////////////////////////////
        private void OnValidate()
        {
            FindDirectionalLight();
        }
        //////////////////////////////////////////////////////////////////////////////////
        private void UpdateLighting(float timePercent)
        {
            RenderSettings.ambientLight = preset.ambientColor.Evaluate(timePercent);
            RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);

            if (directionalLight == null) return;
            directionalLight.color = preset.directionalColor.Evaluate(timePercent);  
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) -90f, 170f, 0));
        }
        //////////////////////////////////////////////////////////////////////////////////
        private void FindDirectionalLight()
        {
            if (directionalLight != null)
                return;

            if (RenderSettings.sun != null)
            {
                directionalLight = RenderSettings.sun;
            }
            else
            {
                var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
                foreach (var light in lights)
                {
                    if (light.type != LightType.Directional) continue;
                    directionalLight = light;
                    return;
                }
            }
        }
    }
}
