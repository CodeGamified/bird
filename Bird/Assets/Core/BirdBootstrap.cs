// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CodeGamified.Camera;
using CodeGamified.Time;
using CodeGamified.Settings;
using CodeGamified.Quality;
using CodeGamified.Bootstrap;
using Bird.Game;
using Bird.Scripting;

namespace Bird.Core
{
    /// <summary>
    /// Bootstrap for Flappy Bird — code-controlled side-scroller.
    ///
    /// Architecture (same pattern as Pong / Tetris / Snake):
    ///   - Instantiate managers → wire cross-references → configure scene
    ///   - .engine submodule gives us TUI + Code Execution for free
    ///   - Players don't press space — they WRITE CODE to decide when to flap
    ///   - "Unit test" your flap AI by watching it play at 100x speed
    ///
    /// Attach to a GameObject. Press Play → Bird appears.
    /// </summary>
    public class BirdBootstrap : GameBootstrap, IQualityResponsive
    {
        protected override string LogTag => "BIRD";

        // =================================================================
        // INSPECTOR
        // =================================================================

        [Header("Bird Physics")]
        [Tooltip("Gravity acceleration (negative = down)")]
        public float gravity = -25f;

        [Tooltip("Upward impulse from a flap")]
        public float flapStrength = 8f;

        [Tooltip("Terminal fall speed (negative)")]
        public float maxFallSpeed = -15f;

        [Header("Pipes")]
        [Tooltip("Seconds between pipe spawns")]
        public float pipeSpawnInterval = 1.5f;

        [Tooltip("Pipe scroll speed (units/sec)")]
        public float pipeScrollSpeed = 4f;

        [Tooltip("Vertical gap between top and bottom pipe")]
        public float gapSize = 3f;

        [Tooltip("Width of each pipe")]
        public float pipeWidth = 1f;

        [Header("World")]
        [Tooltip("World height (ceiling Y)")]
        public float worldHeight = 10f;

        [Tooltip("Bird collision radius")]
        public float birdRadius = 0.3f;

        [Header("Match")]
        [Tooltip("Auto-restart after game over")]
        public bool autoRestart = true;

        [Tooltip("Delay before restarting (sim-seconds)")]
        public float restartDelay = 2f;

        [Header("Time")]
        [Tooltip("Enable time scale modulation for fast testing")]
        public bool enableTimeScale = true;

        [Header("Scripting")]
        [Tooltip("Enable code execution (.engine)")]
        public bool enableScripting = true;

        [Header("Camera")]
        public bool configureCamera = true;

        // =================================================================
        // RUNTIME REFERENCES
        // =================================================================

        private BirdEntity _bird;
        private BirdRenderer _renderer;
        private BirdMatchManager _match;
        private BirdProgram _playerProgram;

        // Camera
        private CameraAmbientMotion _cameraSway;

        // Post-processing
        private Bloom _bloom;
        private Volume _postProcessVolume;

        // =================================================================
        // UPDATE
        // =================================================================

        private void Update()
        {
            UpdateBloomScale();
        }

        private void UpdateBloomScale()
        {
            if (_bloom == null || !_bloom.active) return;
            var cam = Camera.main;
            if (cam == null) return;
            float dist = Vector3.Distance(cam.transform.position, SceneCenter());
            float defaultDist = 12f;
            float scale = Mathf.Clamp01(defaultDist / Mathf.Max(dist, 0.01f));
            _bloom.intensity.value = Mathf.Lerp(0.5f, 1.0f, scale);
        }

        // =================================================================
        // BOOTSTRAP
        // =================================================================

        private void Start()
        {
            Log("🐦 Bird Bootstrap starting...");

            SettingsBridge.Load();
            QualityBridge.SetTier((QualityTier)SettingsBridge.QualityLevel);
            QualityBridge.Register(this);
            Log($"Settings loaded (Quality={SettingsBridge.QualityLevel}, Font={SettingsBridge.FontSize}pt)");

            SetupSimulationTime();
            SetupCamera();
            CreateBird();
            CreateMatchManager();
            CreateRenderer();
            CreateInputProvider();

            if (enableScripting) CreatePlayerProgram();

            WireEvents();
            StartCoroutine(RunBootSequence());
        }

        public void OnQualityChanged(QualityTier tier)
        {
            Log($"Quality changed → {tier}");
        }

        // =================================================================
        // SIMULATION TIME
        // =================================================================

        private void SetupSimulationTime()
        {
            EnsureSimulationTime<BirdSimulationTime>();
        }

        // =================================================================
        // CAMERA — side-scroller perspective
        // =================================================================

        private Vector3 SceneCenter()
        {
            return new Vector3(0f, worldHeight * 0.5f, 0f);
        }

        private void SetupCamera()
        {
            if (!configureCamera) return;

            var cam = EnsureCamera();

            cam.orthographic = false;
            cam.fieldOfView = 60f;
            // Side-scroller: camera looks at bird area from the front
            var center = SceneCenter();
            cam.transform.position = center + new Vector3(2f, 0f, -12f);
            cam.transform.LookAt(center, Vector3.up);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // Ambient sway
            _cameraSway = cam.gameObject.AddComponent<CameraAmbientMotion>();
            _cameraSway.lookAtTarget = center;

            // Post-processing: bloom
            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
                camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;

            var volumeGO = new GameObject("PostProcessVolume");
            _postProcessVolume = volumeGO.AddComponent<Volume>();
            _postProcessVolume.isGlobal = true;
            _postProcessVolume.priority = 1;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _bloom = profile.Add<Bloom>();
            _bloom.threshold.overrideState = true;
            _bloom.threshold.value = 0.8f;
            _bloom.intensity.overrideState = true;
            _bloom.intensity.value = 1.0f;
            _bloom.scatter.overrideState = true;
            _bloom.scatter.value = 0.5f;
            _bloom.clamp.overrideState = true;
            _bloom.clamp.value = 20f;
            _bloom.highQualityFiltering.overrideState = true;
            _bloom.highQualityFiltering.value = true;
            _postProcessVolume.profile = profile;

            Log("Camera: perspective, FOV=60, side-scroller view + sway + bloom");
        }

        // =================================================================
        // BIRD ENTITY
        // =================================================================

        private void CreateBird()
        {
            var go = new GameObject("Bird");
            _bird = go.AddComponent<BirdEntity>();
            _bird.Initialize(worldHeight * 0.5f, gravity, flapStrength, maxFallSpeed, worldHeight);
            Log($"Created Bird (gravity={gravity}, flap={flapStrength}, world={worldHeight})");
        }

        // =================================================================
        // MATCH MANAGER
        // =================================================================

        private void CreateMatchManager()
        {
            var go = new GameObject("MatchManager");
            _match = go.AddComponent<BirdMatchManager>();
            _match.Initialize(_bird, pipeSpawnInterval, pipeScrollSpeed,
                             gapSize, worldHeight, pipeWidth, birdRadius);
            Log($"Created MatchManager (interval={pipeSpawnInterval}s, speed={pipeScrollSpeed}, gap={gapSize})");
        }

        // =================================================================
        // RENDERER
        // =================================================================

        private void CreateRenderer()
        {
            var go = new GameObject("BirdRenderer");
            _renderer = go.AddComponent<BirdRenderer>();
            _renderer.Initialize(_bird, worldHeight);
            Log("Created BirdRenderer (bird sphere, ground, ceiling)");
        }

        // =================================================================
        // INPUT PROVIDER
        // =================================================================

        private void CreateInputProvider()
        {
            var go = new GameObject("InputProvider");
            go.AddComponent<BirdInputProvider>();
            Log("Created BirdInputProvider (space/click/gamepad → flap)");
        }

        // =================================================================
        // PLAYER SCRIPTING (.engine powered)
        // =================================================================

        private void CreatePlayerProgram()
        {
            var go = new GameObject("PlayerProgram");
            _playerProgram = go.AddComponent<BirdProgram>();
            _playerProgram.Initialize(_match, _bird, pipeScrollSpeed);
            Log("Created PlayerProgram (code-controlled bird AI)");
        }

        // =================================================================
        // EVENT WIRING
        // =================================================================

        private void WireEvents()
        {
            if (SimulationTime.Instance != null)
            {
                SimulationTime.Instance.OnTimeScaleChanged += s => Log($"Time scale → {s:F0}x");
                SimulationTime.Instance.OnPausedChanged += p => Log(p ? "⏸ PAUSED" : "▶ RESUMED");
            }

            if (_match != null)
            {
                _match.OnMatchStarted += () => Log("MATCH STARTED");

                _match.OnScored += score =>
                {
                    Log($"SCORE! │ {score}");
                };

                _match.OnGameOver += () =>
                {
                    Log($"GAME OVER │ Score: {_match.Score} │ High: {_match.HighScore}");
                    if (autoRestart)
                        StartCoroutine(RestartAfterDelay());
                };

                _match.OnFlapped += () => { /* visual/audio hook */ };
            }

            if (_bird != null)
            {
                _bird.OnDied += () => Log("Bird died");
            }
        }

        // =================================================================
        // BOOT SEQUENCE
        // =================================================================

        private IEnumerator RunBootSequence()
        {
            yield return null;
            yield return null;

            LogDivider();
            Log("🐦 FLAPPY BIRD — Code Your Flap AI!");
            LogDivider();
            LogStatus("GRAVITY", $"{gravity}");
            LogStatus("FLAP", $"{flapStrength}");
            LogStatus("WORLD", $"{worldHeight} tall");
            LogStatus("PIPES", $"interval={pipeSpawnInterval}s, speed={pipeScrollSpeed}, gap={gapSize}");
            LogEnabled("SCRIPTING", enableScripting);
            LogEnabled("TIME SCALE", enableTimeScale);
            LogEnabled("AUTO RESTART", autoRestart);
            LogDivider();

            _match.StartMatch();
            Log("First match started — GO!");
        }

        private IEnumerator RestartAfterDelay()
        {
            float waited = 0f;
            while (waited < restartDelay)
            {
                if (SimulationTime.Instance != null && !SimulationTime.Instance.isPaused)
                    waited += Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);
                yield return null;
            }

            _match.StartMatch();
            _playerProgram?.ResetExecution();
            Log("Match restarted");
        }

        private void OnDestroy()
        {
            QualityBridge.Unregister(this);
        }
    }
}
