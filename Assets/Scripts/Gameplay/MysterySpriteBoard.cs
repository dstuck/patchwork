using UnityEngine;

namespace Patchwork.Gameplay
{
    /// <summary>
    /// Boss board variant where all collectibles look identical.
    /// Players must guess whether they're picking up bonuses or dangers.
    /// </summary>
    public class MysterySpriteBoard : Board
    {
        #region Constants
        private const int c_MysteryTextureSize = 32;
        private const string c_OverlayName = "MysteryOverlay";
        #endregion

        #region Private Fields
        private static Sprite s_MysterySprite;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Call base initialization (creates holes and collectibles)
            InitializeBoard();
            
            // Add mystery overlays to all collectibles
            AddMysteryOverlays();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Disable tooltips for mystery board - players shouldn't know what collectibles are.
        /// </summary>
        public new void ToggleCollectibleTooltips(bool show)
        {
            // Do nothing - tooltips are disabled on mystery board
        }

        /// <summary>
        /// Disable tooltip cycling for mystery board.
        /// </summary>
        public new bool CycleToNextCollectibleTooltip()
        {
            // Do nothing - tooltips are disabled on mystery board
            return false;
        }
        #endregion

        #region Private Methods
        private void AddMysteryOverlays()
        {
            // Ensure mystery sprite is created
            if (s_MysterySprite == null)
            {
                s_MysterySprite = CreateMysterySprite();
            }

            // Add overlay to each collectible
            foreach (var collectible in m_Collectibles)
            {
                MonoBehaviour collectibleMono = collectible as MonoBehaviour;
                if (collectibleMono != null)
                {
                    AddOverlayToCollectible(collectibleMono.gameObject);
                }
            }
        }

        private void AddOverlayToCollectible(GameObject _collectibleObject)
        {
            // Create overlay as child of collectible
            GameObject overlay = new GameObject(c_OverlayName);
            overlay.transform.SetParent(_collectibleObject.transform);
            overlay.transform.localPosition = Vector3.zero;
            
            // Counter-scale to maintain consistent size regardless of parent's scale
            Vector3 parentScale = _collectibleObject.transform.localScale;
            overlay.transform.localScale = new Vector3(
                2.1f / parentScale.x,
                2.1f / parentScale.y,
                1f
            );

            // Add sprite renderer above the collectible's sprite
            SpriteRenderer overlayRenderer = overlay.AddComponent<SpriteRenderer>();
            overlayRenderer.sprite = s_MysterySprite;
            overlayRenderer.sortingOrder = 2; // Above collectible sprites (which are at 1)
            overlayRenderer.color = Color.white;
        }

        private Sprite CreateMysterySprite()
        {
            int size = c_MysteryTextureSize;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            Color[] pixels = new Color[size * size];
            
            // Fill with a purple/mystery color background
            Color bgColor = new Color(0.5f, 0.2f, 0.6f, 1f);
            Color fgColor = Color.white;
            
            // Fill background
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = bgColor;
            }
            
            // Draw a "?" symbol
            DrawQuestionMark(pixels, size, fgColor);
            
            texture.SetPixels(pixels);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private void DrawQuestionMark(Color[] _pixels, int _size, Color _color)
        {
            int center = _size / 2;
            
            // Draw the curved top of the "?"
            // Top arc
            for (int y = _size - 8; y < _size - 4; y++)
            {
                for (int x = center - 4; x <= center + 4; x++)
                {
                    SetPixelSafe(_pixels, x, y, _size, _color);
                }
            }
            
            // Right side of arc
            for (int y = _size - 12; y < _size - 6; y++)
            {
                for (int x = center + 2; x <= center + 5; x++)
                {
                    SetPixelSafe(_pixels, x, y, _size, _color);
                }
            }
            
            // Diagonal to center
            for (int y = center + 2; y < _size - 12; y++)
            {
                for (int x = center - 1; x <= center + 2; x++)
                {
                    SetPixelSafe(_pixels, x, y, _size, _color);
                }
            }
            
            // Stem
            for (int y = center - 2; y <= center + 2; y++)
            {
                for (int x = center - 2; x <= center + 1; x++)
                {
                    SetPixelSafe(_pixels, x, y, _size, _color);
                }
            }
            
            // Dot at bottom
            for (int y = 4; y <= 8; y++)
            {
                for (int x = center - 2; x <= center + 1; x++)
                {
                    SetPixelSafe(_pixels, x, y, _size, _color);
                }
            }
        }

        private void SetPixelSafe(Color[] _pixels, int _x, int _y, int _size, Color _color)
        {
            if (_x >= 0 && _x < _size && _y >= 0 && _y < _size)
            {
                _pixels[_y * _size + _x] = _color;
            }
        }
        #endregion
    }
}

