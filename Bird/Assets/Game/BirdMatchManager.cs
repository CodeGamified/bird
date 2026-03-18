// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using System.Collections.Generic;
using UnityEngine;
using CodeGamified.Time;

namespace Bird.Game
{
    /// <summary>
    /// Match manager — pipe spawning, collision detection, scoring, game over, restart.
    /// The player's CODE calls flap(). This drives the world.
    ///
    /// Pipes scroll left. Bird is fixed at X=0.
    /// Score increments when bird passes a pipe's X position.
    /// </summary>
    public class BirdMatchManager : MonoBehaviour
    {
        private BirdEntity _bird;

        // Config
        private float _pipeSpawnInterval;
        private float _pipeScrollSpeed;
        private float _gapSize;
        private float _worldHeight;
        private float _pipeWidth;
        private float _birdRadius;

        // Pipe tracking
        private readonly List<PipePair> _activePipes = new();
        private float _spawnTimer;
        private float _spawnX;    // X position where new pipes appear
        private float _despawnX;  // X position where old pipes are destroyed

        // State
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public bool GameOver { get; private set; }
        public bool MatchInProgress { get; private set; }
        public int MatchesPlayed { get; private set; }

        // Nearest pipe (for AI queries)
        public PipePair NearestPipe { get; private set; }
        public PipePair SecondPipe { get; private set; }

        // Events
        public System.Action OnMatchStarted;
        public System.Action OnGameOver;
        public System.Action<int> OnScored;    // new score
        public System.Action OnFlapped;

        public BirdEntity Bird => _bird;

        public void Initialize(BirdEntity bird, float pipeSpawnInterval = 1.5f,
                               float pipeScrollSpeed = 4f, float gapSize = 3f,
                               float worldHeight = 10f, float pipeWidth = 1f,
                               float birdRadius = 0.3f)
        {
            _bird = bird;
            _pipeSpawnInterval = pipeSpawnInterval;
            _pipeScrollSpeed = pipeScrollSpeed;
            _gapSize = gapSize;
            _worldHeight = worldHeight;
            _pipeWidth = pipeWidth;
            _birdRadius = birdRadius;
            _spawnX = 12f;
            _despawnX = -6f;
        }

        public void StartMatch()
        {
            // Clear existing pipes
            foreach (var pipe in _activePipes)
                if (pipe != null) Destroy(pipe.gameObject);
            _activePipes.Clear();

            _bird.Reset(_worldHeight * 0.5f);
            Score = 0;
            GameOver = false;
            MatchInProgress = true;
            _spawnTimer = 0f; // spawn first pipe immediately after small delay
            NearestPipe = null;
            SecondPipe = null;

            OnMatchStarted?.Invoke();
        }

        private void Update()
        {
            if (!MatchInProgress || GameOver) return;
            if (SimulationTime.Instance == null || SimulationTime.Instance.isPaused) return;

            float dt = Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);

            SpawnPipes(dt);
            CleanupPipes();
            UpdateNearestPipe();
            CheckCollisions();
            CheckScoring();
        }

        // ═══════════════════════════════════════════════════════════════
        // PIPE SPAWNING
        // ═══════════════════════════════════════════════════════════════

        private void SpawnPipes(float dt)
        {
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = _pipeSpawnInterval;
                SpawnPipe();
            }
        }

        private void SpawnPipe()
        {
            float margin = _gapSize * 0.5f + 0.5f;
            float gapCenterY = Random.Range(margin, _worldHeight - margin);

            var go = new GameObject($"Pipe_{Score + _activePipes.Count}");
            var pipe = go.AddComponent<PipePair>();
            pipe.Initialize(_spawnX, gapCenterY, _gapSize, _pipeScrollSpeed,
                           _worldHeight, _pipeWidth, _despawnX);
            _activePipes.Add(pipe);
        }

        private void CleanupPipes()
        {
            _activePipes.RemoveAll(p => p == null);
        }

        private void UpdateNearestPipe()
        {
            NearestPipe = null;
            SecondPipe = null;

            // Find the two nearest pipes that haven't been passed yet
            // (PosX + halfWidth > bird X)
            float halfW = _pipeWidth * 0.5f;
            PipePair first = null;
            PipePair second = null;

            foreach (var pipe in _activePipes)
            {
                if (pipe == null) continue;
                if (pipe.PosX + halfW < -_birdRadius) continue; // already fully past

                if (first == null || pipe.PosX < first.PosX)
                {
                    second = first;
                    first = pipe;
                }
                else if (second == null || pipe.PosX < second.PosX)
                {
                    second = pipe;
                }
            }

            NearestPipe = first;
            SecondPipe = second;
        }

        // ═══════════════════════════════════════════════════════════════
        // COLLISION & SCORING
        // ═══════════════════════════════════════════════════════════════

        private void CheckCollisions()
        {
            if (_bird.IsDead)
            {
                EndGame();
                return;
            }

            foreach (var pipe in _activePipes)
            {
                if (pipe == null) continue;
                if (pipe.CheckCollision(0f, _bird.PosY, _birdRadius, _pipeWidth))
                {
                    _bird.Die();
                    EndGame();
                    return;
                }
            }
        }

        private void CheckScoring()
        {
            foreach (var pipe in _activePipes)
            {
                if (pipe == null || pipe.Scored) continue;

                // Bird passes pipe when pipe center is behind bird (X < 0)
                if (pipe.PosX < 0f)
                {
                    pipe.Scored = true;
                    Score++;
                    OnScored?.Invoke(Score);
                }
            }
        }

        private void EndGame()
        {
            GameOver = true;
            MatchInProgress = false;
            MatchesPlayed++;
            if (Score > HighScore) HighScore = Score;
            _bird.Stop();
            OnGameOver?.Invoke();
        }

        // ═══════════════════════════════════════════════════════════════
        // PLAYER ACTIONS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Flap the bird. Called by IOHandler.</summary>
        public void DoFlap()
        {
            if (!MatchInProgress || GameOver) return;
            _bird.Flap();
            OnFlapped?.Invoke();
        }

        private void OnDestroy()
        {
            foreach (var pipe in _activePipes)
                if (pipe != null) Destroy(pipe.gameObject);
            _activePipes.Clear();
        }
    }
}
