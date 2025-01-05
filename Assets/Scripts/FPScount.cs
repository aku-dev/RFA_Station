using System.Diagnostics;
using UnityEngine;

namespace LeopotamGroup.DebugHelpers
{
    public sealed class FPScount : MonoBehaviour
    {
        private const int _updatesPerSecond = 2;

        private const float _invUpdatesPerSecond = 0.5f;

        [Range(0f, 1f)]
        public int calcType;

        public float updateInterval = 0.2f;

        private float _frameCount;

        private float _lastTime;

        private string _cullTime;

        private string _renderTime;

        private string _frameTime;

        private string _lastTimes;

        private float _lastTimesTime;

        private Stopwatch sw;

        private float accum;

        private int frames;

        private float timeleft;

        private string deviceModel;

        private string p_quality;

        private string p_resolution;

        private string p_textures;

        private string p_grass;

        private string p_tree;

        public float CurrentFps
        {
            get;
            private set;
        }
       

        private void Awake()
        {           
            base.useGUILayout = false;
            sw = new Stopwatch();
            sw.Reset();
            sw.Start();
        }

        private void Update()
        {
            if (calcType == 0)
            {
                timeleft -= Time.deltaTime;
                accum += Time.timeScale / Time.deltaTime;
                frames++;
                if (timeleft <= 0f)
                {
                    CurrentFps = accum / (float)frames;
                    timeleft = updateInterval;
                    accum = 0f;
                    frames = 0;
                }
            }
            else
            {
                _frameCount += 1f;
                if (Time.time - _lastTime >= 0.5f)
                {
                    CurrentFps = _frameCount * 2f;
                    _frameCount = 0f;
                    _lastTime = Time.time;
                }
            }
        }

        private void OnPreCull()
        {
            sw.Stop();
            _frameTime = ((int)sw.Elapsed.TotalMilliseconds).ToString();
            sw.Reset();
            sw.Start();
        }

        private void OnPreRender()
        {
            sw.Stop();
            _cullTime = ((int)sw.Elapsed.TotalMilliseconds).ToString();
            sw.Reset();
            sw.Start();
        }

        private void OnPostRender()
        {
            sw.Stop();
            _renderTime = ((int)sw.Elapsed.TotalMilliseconds).ToString();
            sw.Reset();
            sw.Start();
        }

        private void OnGUI()
        {
            if (Time.time - _lastTimesTime >= updateInterval)
            {
                _lastTimes = "(" + _cullTime + "." + _renderTime + "." + _frameTime + ")";
                _lastTimesTime = Time.time;
            }
            GUI.Label(new Rect(100f, Screen.height - 72, Screen.width, 36f), "fps: " + CurrentFps.ToString("F2") + _lastTimes + deviceModel);
        }
    }
}
