// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bird.Scripting
{
    /// <summary>
    /// Captures input for Flappy Bird.
    /// Encodes as a single float: 0=none, 1=flap.
    /// Space / tap / gamepad A = flap.
    /// </summary>
    public class BirdInputProvider : MonoBehaviour
    {
        public static BirdInputProvider Instance { get; private set; }

        public const float INPUT_NONE = 0f;
        public const float INPUT_FLAP = 1f;

        public float CurrentInput { get; private set; }

        private InputAction _flapAction;

        private void Awake()
        {
            Instance = this;

            _flapAction = new InputAction("Flap", InputActionType.Button);
            _flapAction.AddBinding("<Keyboard>/space");
            _flapAction.AddBinding("<Mouse>/leftButton");
            _flapAction.AddBinding("<Gamepad>/buttonSouth");
            _flapAction.Enable();
        }

        private void Update()
        {
            CurrentInput = _flapAction.WasPressedThisFrame() ? INPUT_FLAP : INPUT_NONE;
        }

        private void OnDestroy()
        {
            _flapAction?.Disable();
            _flapAction?.Dispose();
            if (Instance == this) Instance = null;
        }
    }
}
