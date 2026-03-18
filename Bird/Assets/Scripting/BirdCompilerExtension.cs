// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using System.Collections.Generic;
using CodeGamified.Engine;
using CodeGamified.Engine.Compiler;

namespace Bird.Scripting
{
    /// <summary>
    /// Flappy Bird opcodes mapped to CUSTOM_0..CUSTOM_N.
    /// </summary>
    public enum BirdOpCode
    {
        // ── Queries (read game state → R0) ──
        GET_BIRD_Y       = 0,   // bird Y position
        GET_BIRD_VY      = 1,   // bird Y velocity
        GET_PIPE_X       = 2,   // nearest pipe X distance from bird
        GET_PIPE_GAP_Y   = 3,   // nearest pipe gap center Y
        GET_PIPE_GAP     = 4,   // nearest pipe gap size
        GET_PIPE2_X      = 5,   // second pipe X distance
        GET_PIPE2_GAP_Y  = 6,   // second pipe gap center Y
        GET_SCORE        = 7,   // current score
        GET_HIGH_SCORE   = 8,   // best score this session
        GET_WORLD_H      = 9,   // world height (ceiling)
        GET_GRAVITY      = 10,  // gravity value
        GET_FLAP_STR     = 11,  // flap strength
        GET_PIPE_SPEED   = 12,  // pipe scroll speed
        GET_INPUT        = 13,  // keyboard input (0=none, 1=flap)

        // ── Commands ──
        FLAP             = 14,  // flap the bird
    }

    /// <summary>
    /// Compiler extension for Flappy Bird — registers builtins for bird queries and flap command.
    /// </summary>
    public class BirdCompilerExtension : ICompilerExtension
    {
        public void RegisterBuiltins(CompilerContext ctx) { }

        public bool TryCompileCall(string functionName, List<AstNodes.ExprNode> args,
                                   CompilerContext ctx, int sourceLine)
        {
            switch (functionName)
            {
                // ── Queries ──
                case "get_bird_y":
                    Emit(ctx, BirdOpCode.GET_BIRD_Y, sourceLine, "get_bird_y → R0");
                    return true;
                case "get_bird_vy":
                    Emit(ctx, BirdOpCode.GET_BIRD_VY, sourceLine, "get_bird_vy → R0");
                    return true;
                case "get_pipe_x":
                    Emit(ctx, BirdOpCode.GET_PIPE_X, sourceLine, "get_pipe_x → R0");
                    return true;
                case "get_pipe_gap_y":
                    Emit(ctx, BirdOpCode.GET_PIPE_GAP_Y, sourceLine, "get_pipe_gap_y → R0");
                    return true;
                case "get_pipe_gap":
                    Emit(ctx, BirdOpCode.GET_PIPE_GAP, sourceLine, "get_pipe_gap → R0");
                    return true;
                case "get_pipe2_x":
                    Emit(ctx, BirdOpCode.GET_PIPE2_X, sourceLine, "get_pipe2_x → R0");
                    return true;
                case "get_pipe2_gap_y":
                    Emit(ctx, BirdOpCode.GET_PIPE2_GAP_Y, sourceLine, "get_pipe2_gap_y → R0");
                    return true;
                case "get_score":
                    Emit(ctx, BirdOpCode.GET_SCORE, sourceLine, "get_score → R0");
                    return true;
                case "get_high_score":
                    Emit(ctx, BirdOpCode.GET_HIGH_SCORE, sourceLine, "get_high_score → R0");
                    return true;
                case "get_world_height":
                    Emit(ctx, BirdOpCode.GET_WORLD_H, sourceLine, "get_world_height → R0");
                    return true;
                case "get_gravity":
                    Emit(ctx, BirdOpCode.GET_GRAVITY, sourceLine, "get_gravity → R0");
                    return true;
                case "get_flap_strength":
                    Emit(ctx, BirdOpCode.GET_FLAP_STR, sourceLine, "get_flap_strength → R0");
                    return true;
                case "get_pipe_speed":
                    Emit(ctx, BirdOpCode.GET_PIPE_SPEED, sourceLine, "get_pipe_speed → R0");
                    return true;
                case "get_input":
                    Emit(ctx, BirdOpCode.GET_INPUT, sourceLine, "get_input → R0");
                    return true;

                // ── Commands ──
                case "flap":
                    Emit(ctx, BirdOpCode.FLAP, sourceLine, "flap");
                    return true;

                default:
                    return false;
            }
        }

        private static void Emit(CompilerContext ctx, BirdOpCode op, int line, string comment)
        {
            ctx.Emit(OpCode.CUSTOM_0 + (int)op, 0, 0, 0, line, comment);
        }

        public bool TryCompileMethodCall(string objectName, string methodName,
                                         List<AstNodes.ExprNode> args,
                                         CompilerContext ctx, int sourceLine) => false;

        public bool TryCompileObjectDecl(string typeName, string varName,
                                         List<AstNodes.ExprNode> constructorArgs,
                                         CompilerContext ctx, int sourceLine) => false;
    }
}
