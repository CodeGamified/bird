// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using UnityEngine;
using CodeGamified.Quality;

namespace Bird.Game
{
    /// <summary>
    /// Visual renderer for the Flappy Bird world.
    /// Renders the bird (sphere), ground plane, ceiling line, and sky background.
    /// Pipe visuals are self-contained in PipePair.
    /// </summary>
    public class BirdRenderer : MonoBehaviour, IQualityResponsive
    {
        private BirdEntity _bird;
        private float _worldHeight;

        // Bird visual
        private GameObject _birdVisual;

        // Ground + ceiling
        private GameObject _groundPlane;
        private GameObject _ceilingBar;

        // Colors
        private static readonly Color BirdColor   = new Color(1f, 0.9f, 0.1f);   // yellow
        private static readonly Color GroundColor  = new Color(0.15f, 0.4f, 0.1f); // dark green
        private static readonly Color CeilingColor = new Color(0.3f, 0.3f, 0.4f);

        public void Initialize(BirdEntity bird, float worldHeight)
        {
            _bird = bird;
            _worldHeight = worldHeight;

            BuildBird();
            BuildGround();
            BuildCeiling();

            QualityBridge.Register(this);
        }

        private void OnDisable() => QualityBridge.Unregister(this);
        public void OnQualityChanged(QualityTier tier) { }

        private void LateUpdate()
        {
            if (_birdVisual != null)
                _birdVisual.transform.position = new Vector3(0f, _bird.PosY, 0f);
        }

        private void BuildBird()
        {
            _birdVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _birdVisual.name = "BirdVisual";
            _birdVisual.transform.localScale = Vector3.one * 0.5f;
            RemoveCollider(_birdVisual);
            SetColor(_birdVisual, BirdColor);
        }

        private void BuildGround()
        {
            _groundPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _groundPlane.name = "Ground";
            _groundPlane.transform.position = new Vector3(0f, -0.25f, 0f);
            _groundPlane.transform.localScale = new Vector3(30f, 0.5f, 2f);
            RemoveCollider(_groundPlane);
            SetColor(_groundPlane, GroundColor);
        }

        private void BuildCeiling()
        {
            _ceilingBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _ceilingBar.name = "Ceiling";
            _ceilingBar.transform.position = new Vector3(0f, _worldHeight + 0.1f, 0f);
            _ceilingBar.transform.localScale = new Vector3(30f, 0.2f, 2f);
            RemoveCollider(_ceilingBar);
            SetColor(_ceilingBar, CeilingColor);
        }

        private static void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            var mat = renderer.material;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else
                mat.color = color;
        }

        private static void RemoveCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
    }
}
