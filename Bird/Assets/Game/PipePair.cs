// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using UnityEngine;
using CodeGamified.Time;

namespace Bird.Game
{
    /// <summary>
    /// A single pipe pair — top and bottom obstacles with a gap.
    /// Scrolls left at a constant speed. Destroyed when off-screen.
    /// </summary>
    public class PipePair : MonoBehaviour
    {
        public float GapCenterY { get; private set; }
        public float GapSize { get; private set; }
        public float PosX { get; private set; }
        public bool Scored { get; set; }

        private float _scrollSpeed;
        private float _worldHeight;
        private float _despawnX;

        // Visual children
        private GameObject _topPipe;
        private GameObject _bottomPipe;

        public void Initialize(float posX, float gapCenterY, float gapSize,
                               float scrollSpeed, float worldHeight,
                               float pipeWidth, float despawnX)
        {
            PosX = posX;
            GapCenterY = gapCenterY;
            GapSize = gapSize;
            _scrollSpeed = scrollSpeed;
            _worldHeight = worldHeight;
            _despawnX = despawnX;
            Scored = false;

            BuildVisuals(pipeWidth);
            SyncTransform();
        }

        private void Update()
        {
            if (SimulationTime.Instance == null || SimulationTime.Instance.isPaused) return;

            float dt = Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);
            PosX -= _scrollSpeed * dt;
            SyncTransform();

            if (PosX < _despawnX)
                Destroy(gameObject);
        }

        private void SyncTransform()
        {
            transform.position = new Vector3(PosX, 0f, 0f);
        }

        private void BuildVisuals(float pipeWidth)
        {
            float topOfGap = GapCenterY + GapSize * 0.5f;
            float bottomOfGap = GapCenterY - GapSize * 0.5f;
            Color pipeColor = new Color(0.1f, 0.8f, 0.2f);

            // Bottom pipe: from ground (0) to bottomOfGap
            if (bottomOfGap > 0.01f)
            {
                _bottomPipe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _bottomPipe.name = "BottomPipe";
                _bottomPipe.transform.SetParent(transform, false);
                float height = bottomOfGap;
                _bottomPipe.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
                _bottomPipe.transform.localScale = new Vector3(pipeWidth, height, pipeWidth);
                SetColor(_bottomPipe, pipeColor);
                RemoveCollider(_bottomPipe);
            }

            // Top pipe: from topOfGap to worldHeight
            float topHeight = _worldHeight - topOfGap;
            if (topHeight > 0.01f)
            {
                _topPipe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _topPipe.name = "TopPipe";
                _topPipe.transform.SetParent(transform, false);
                _topPipe.transform.localPosition = new Vector3(0f, topOfGap + topHeight * 0.5f, 0f);
                _topPipe.transform.localScale = new Vector3(pipeWidth, topHeight, pipeWidth);
                SetColor(_topPipe, pipeColor);
                RemoveCollider(_topPipe);
            }
        }

        /// <summary>Check if a point (birdY at birdX) collides with this pipe pair.</summary>
        public bool CheckCollision(float birdX, float birdY, float birdRadius, float pipeWidth)
        {
            float halfW = pipeWidth * 0.5f;
            // Is the bird horizontally overlapping this pipe?
            if (birdX + birdRadius < PosX - halfW) return false;
            if (birdX - birdRadius > PosX + halfW) return false;

            // Bird is in the pipe column — check if inside the gap
            float topOfGap = GapCenterY + GapSize * 0.5f;
            float bottomOfGap = GapCenterY - GapSize * 0.5f;

            if (birdY - birdRadius < bottomOfGap) return true;  // hit bottom pipe
            if (birdY + birdRadius > topOfGap) return true;     // hit top pipe
            return false;
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
            if (col != null) Object.Destroy(col);
        }
    }
}
