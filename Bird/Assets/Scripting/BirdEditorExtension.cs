// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using System.Collections.Generic;
using CodeGamified.Editor;

namespace Bird.Scripting
{
    /// <summary>
    /// Editor extension for Flappy Bird — provides game-specific options
    /// to CodeEditorWindow's option tree for tap-to-code editing.
    /// </summary>
    public class BirdEditorExtension : IEditorExtension
    {
        public List<EditorTypeInfo> GetAvailableTypes() => new();

        public List<EditorFuncInfo> GetAvailableFunctions() => new()
        {
            // Queries
            new EditorFuncInfo { Name = "get_bird_y",        Hint = "bird Y position",          ArgCount = 0 },
            new EditorFuncInfo { Name = "get_bird_vy",       Hint = "bird Y velocity (+ = up)",  ArgCount = 0 },
            new EditorFuncInfo { Name = "get_pipe_x",        Hint = "nearest pipe X distance",   ArgCount = 0 },
            new EditorFuncInfo { Name = "get_pipe_gap_y",    Hint = "nearest pipe gap center Y",  ArgCount = 0 },
            new EditorFuncInfo { Name = "get_pipe_gap",      Hint = "gap size",                  ArgCount = 0 },
            new EditorFuncInfo { Name = "get_pipe2_x",       Hint = "second pipe X distance",    ArgCount = 0 },
            new EditorFuncInfo { Name = "get_pipe2_gap_y",   Hint = "second pipe gap center Y",  ArgCount = 0 },
            new EditorFuncInfo { Name = "get_score",         Hint = "current score",             ArgCount = 0 },
            new EditorFuncInfo { Name = "get_high_score",    Hint = "best score this session",   ArgCount = 0 },
            new EditorFuncInfo { Name = "get_world_height",  Hint = "ceiling Y",                ArgCount = 0 },
            new EditorFuncInfo { Name = "get_gravity",       Hint = "gravity value (negative)",  ArgCount = 0 },
            new EditorFuncInfo { Name = "get_flap_strength", Hint = "flap impulse value",        ArgCount = 0 },
            new EditorFuncInfo { Name = "get_pipe_speed",    Hint = "pipe scroll speed",         ArgCount = 0 },
            new EditorFuncInfo { Name = "get_input",         Hint = "keyboard (0=none, 1=flap)", ArgCount = 0 },

            // Commands
            new EditorFuncInfo { Name = "flap",              Hint = "flap the bird upward",      ArgCount = 0 },
        };

        public List<EditorMethodInfo> GetMethodsForType(string typeName) => new();

        public List<string> GetVariableNameSuggestions() => new()
        {
            "bird_y", "vy", "pipe_x", "gap_y", "gap",
            "pipe2_x", "pipe2_gap", "dist", "inp", "score"
        };

        public List<string> GetStringLiteralSuggestions() => new();
    }
}
