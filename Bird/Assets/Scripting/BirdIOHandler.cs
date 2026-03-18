// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using CodeGamified.Engine;
using CodeGamified.Time;
using Bird.Game;

namespace Bird.Scripting
{
    /// <summary>
    /// Game I/O handler for Flappy Bird — bridges CUSTOM opcodes to game state.
    /// </summary>
    public class BirdIOHandler : IGameIOHandler
    {
        private readonly BirdMatchManager _match;
        private readonly BirdEntity _bird;
        private readonly float _pipeScrollSpeed;

        public BirdIOHandler(BirdMatchManager match, BirdEntity bird, float pipeScrollSpeed)
        {
            _match = match;
            _bird = bird;
            _pipeScrollSpeed = pipeScrollSpeed;
        }

        public bool PreExecute(Instruction inst, MachineState state) => true;

        public void ExecuteIO(Instruction inst, MachineState state)
        {
            int op = (int)inst.Op - (int)OpCode.CUSTOM_0;

            switch ((BirdOpCode)op)
            {
                // ── Queries → R0 ──
                case BirdOpCode.GET_BIRD_Y:
                    state.SetRegister(0, _bird.PosY);
                    break;
                case BirdOpCode.GET_BIRD_VY:
                    state.SetRegister(0, _bird.VelocityY);
                    break;
                case BirdOpCode.GET_PIPE_X:
                    state.SetRegister(0, _match.NearestPipe != null ? _match.NearestPipe.PosX : 999f);
                    break;
                case BirdOpCode.GET_PIPE_GAP_Y:
                    state.SetRegister(0, _match.NearestPipe != null ? _match.NearestPipe.GapCenterY : _bird.WorldHeight * 0.5f);
                    break;
                case BirdOpCode.GET_PIPE_GAP:
                    state.SetRegister(0, _match.NearestPipe != null ? _match.NearestPipe.GapSize : 3f);
                    break;
                case BirdOpCode.GET_PIPE2_X:
                    state.SetRegister(0, _match.SecondPipe != null ? _match.SecondPipe.PosX : 999f);
                    break;
                case BirdOpCode.GET_PIPE2_GAP_Y:
                    state.SetRegister(0, _match.SecondPipe != null ? _match.SecondPipe.GapCenterY : _bird.WorldHeight * 0.5f);
                    break;
                case BirdOpCode.GET_SCORE:
                    state.SetRegister(0, _match.Score);
                    break;
                case BirdOpCode.GET_HIGH_SCORE:
                    state.SetRegister(0, _match.HighScore);
                    break;
                case BirdOpCode.GET_WORLD_H:
                    state.SetRegister(0, _bird.WorldHeight);
                    break;
                case BirdOpCode.GET_GRAVITY:
                    state.SetRegister(0, _bird.Gravity);
                    break;
                case BirdOpCode.GET_FLAP_STR:
                    state.SetRegister(0, _bird.FlapStrength);
                    break;
                case BirdOpCode.GET_PIPE_SPEED:
                    state.SetRegister(0, _pipeScrollSpeed);
                    break;
                case BirdOpCode.GET_INPUT:
                    state.SetRegister(0, BirdInputProvider.Instance != null
                        ? BirdInputProvider.Instance.CurrentInput : 0f);
                    break;

                // ── Commands ──
                case BirdOpCode.FLAP:
                    _match.DoFlap();
                    break;
            }
        }

        public float GetTimeScale() => SimulationTime.Instance?.timeScale ?? 1f;
        public double GetSimulationTime() => SimulationTime.Instance?.simulationTime ?? 0.0;
    }
}
