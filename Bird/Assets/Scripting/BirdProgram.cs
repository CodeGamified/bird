// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using UnityEngine;
using CodeGamified.Engine;
using CodeGamified.Engine.Compiler;
using CodeGamified.Engine.Runtime;
using CodeGamified.Time;
using Bird.Game;

namespace Bird.Scripting
{
    /// <summary>
    /// BirdProgram — code-controlled flappy bird AI.
    /// Subclasses ProgramBehaviour from .engine.
    ///
    /// EXECUTION MODEL (tick-based, deterministic):
    ///   - Each simulation tick (~20 ops/sec sim-time), the script runs from the top
    ///   - Memory (variables) persists across ticks
    ///   - PC resets to 0 each tick
    ///   - Each tick the script reads bird/pipe state and decides whether to flap()
    ///   - Results are IDENTICAL at 0.5x, 1x, 100x speed
    ///
    /// BUILTINS:
    ///   get_bird_y()        → bird Y position
    ///   get_bird_vy()       → bird Y velocity
    ///   get_pipe_x()        → nearest pipe X distance
    ///   get_pipe_gap_y()    → nearest pipe gap center Y
    ///   get_pipe_gap()      → gap size
    ///   get_pipe2_x()       → second pipe X
    ///   get_pipe2_gap_y()   → second pipe gap Y
    ///   get_input()         → keyboard (0=none, 1=flap)
    ///   flap()              → flap the bird
    /// </summary>
    public class BirdProgram : ProgramBehaviour
    {
        private BirdMatchManager _match;
        private BirdEntity _bird;
        private BirdIOHandler _ioHandler;
        private BirdCompilerExtension _compilerExt;
        private float _pipeScrollSpeed;

        public const float OPS_PER_SECOND = 20f;
        private float _opAccumulator;

        private const string DEFAULT_CODE = @"# 🐦 FLAPPY BIRD — Write your flap AI!
# Your script runs at 20 ops/sec (sim-time).
# When it finishes, it restarts from the top.
# Variables persist — use them to track state.
#
# BUILTINS — Queries:
#   get_bird_y()        → bird Y position
#   get_bird_vy()       → bird Y velocity (+ = up)
#   get_pipe_x()        → nearest pipe X distance
#   get_pipe_gap_y()    → nearest pipe gap center Y
#   get_pipe_gap()      → gap size
#   get_pipe2_x()       → second pipe X distance
#   get_pipe2_gap_y()   → second pipe gap center Y
#   get_score()         → current score
#   get_high_score()    → best score this session
#   get_world_height()  → ceiling Y
#   get_gravity()       → gravity value (negative)
#   get_flap_strength() → flap impulse value
#   get_pipe_speed()    → pipe scroll speed
#   get_input()         → keyboard (0=none, 1=flap)
#
# BUILTINS — Commands:
#   flap()              → flap the bird upward
#
# This starter passes keyboard/tap input through:
inp = get_input()
if inp == 1:
    flap()
";

        public string CurrentSourceCode => _sourceCode;
        public System.Action OnCodeChanged;

        public void Initialize(BirdMatchManager match, BirdEntity bird,
                               float pipeScrollSpeed,
                               string initialCode = null, string programName = "BirdAI")
        {
            _match = match;
            _bird = bird;
            _pipeScrollSpeed = pipeScrollSpeed;
            _compilerExt = new BirdCompilerExtension();

            _programName = programName;
            _sourceCode = initialCode ?? DEFAULT_CODE;
            _autoRun = true;

            LoadAndRun(_sourceCode);
        }

        protected override void Update()
        {
            if (_executor == null || _program == null || _isPaused) return;
            if (_match == null || !_match.MatchInProgress || _match.GameOver) return;

            float timeScale = SimulationTime.Instance?.timeScale ?? 1f;
            if (SimulationTime.Instance != null && SimulationTime.Instance.isPaused) return;

            float simDelta = UnityEngine.Time.deltaTime * timeScale;
            _opAccumulator += simDelta * OPS_PER_SECOND;

            int opsToRun = (int)_opAccumulator;
            _opAccumulator -= opsToRun;

            for (int i = 0; i < opsToRun; i++)
            {
                if (_executor.State.IsHalted)
                {
                    _executor.State.PC = 0;
                    _executor.State.IsHalted = false;
                }
                _executor.ExecuteOne();
            }

            if (opsToRun > 0)
                ProcessEvents();
        }

        protected override IGameIOHandler CreateIOHandler()
        {
            _ioHandler = new BirdIOHandler(_match, _bird, _pipeScrollSpeed);
            return _ioHandler;
        }

        protected override CompiledProgram CompileSource(string source, string name)
        {
            return PythonCompiler.Compile(source, name, _compilerExt);
        }

        protected override void ProcessEvents()
        {
            if (_executor?.State == null) return;
            while (_executor.State.OutputEvents.Count > 0)
                _executor.State.OutputEvents.Dequeue();
        }

        public void UploadCode(string newSource)
        {
            _sourceCode = newSource ?? DEFAULT_CODE;
            LoadAndRun(_sourceCode);
            Debug.Log($"[BirdAI] Uploaded new code ({_program?.Instructions?.Length ?? 0} instructions)");
            OnCodeChanged?.Invoke();
        }

        public void ResetExecution()
        {
            if (_executor?.State == null) return;
            _executor.State.Reset();
            _opAccumulator = 0f;
        }
    }
}
