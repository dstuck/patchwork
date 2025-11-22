using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PaintUpgradeCollectible : BaseCollectible
    {
        private ITileUpgrade m_Upgrade;

        protected virtual ITileUpgrade GetUpgrade() => null;

        protected override void Awake()
        {
            base.Awake();
            m_Upgrade = GetUpgrade();
            if (m_Upgrade != null && m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = m_Upgrade.DisplayColor;
            }
        }

        public ITileUpgrade CurrentUpgrade => m_Upgrade;

        public override string DisplayName => m_Upgrade?.DisplayName ?? "Upgrade";
        public override string Description => m_Upgrade?.Description ?? "Upgrades collecting tile";

        protected override Sprite GetSprite() => GameResources.Instance.PaintSprite;
        protected override float GetScale() => GameResources.Instance.PaintScale;

        public override Sprite GetDisplaySprite()
        {
            Sprite mainSprite = GetSpriteForLevel(m_Level);
            
            // Get the color from the upgrade
            Color displayColor = m_Upgrade?.DisplayColor ?? Color.white;
            
            // For paint upgrades, we need to apply color even at level 1
            // since the color is the defining characteristic
            if (displayColor != Color.white)
            {
                // Always generate a colored sprite for paint upgrades
                if (m_Level <= 1 || !ShouldShowLevelIndicators())
                {
                    // Create a simple colored version without level indicators
                    return GenerateColoredSprite(mainSprite, displayColor);
                }
                else
                {
                    // Generate composite sprite with level indicators and color tint
                    return GenerateCompositeSprite(mainSprite, displayColor);
                }
            }
            
            // Fallback to base implementation for non-colored upgrades
            return base.GetDisplaySprite();
        }

        private Sprite GenerateColoredSprite(Sprite mainSprite, Color tintColor)
        {
            if (mainSprite == null) return null;
            
            // Get sprite texture bounds
            Rect spriteRect = mainSprite.rect;
            int width = (int)spriteRect.width;
            int height = (int)spriteRect.height;
            
            // Create readable copy of main sprite texture
            Texture2D mainTextureReadable = GetReadableTexture(mainSprite.texture, (int)spriteRect.x, (int)spriteRect.y, width, height);
            
            // Create new texture
            Texture2D coloredTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            
            // Apply tint color to all pixels
            Color[] pixels = mainTextureReadable.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color spritePixel = pixels[i];
                pixels[i] = new Color(
                    spritePixel.r * tintColor.r,
                    spritePixel.g * tintColor.g,
                    spritePixel.b * tintColor.b,
                    spritePixel.a * tintColor.a
                );
            }
            
            coloredTexture.SetPixels(pixels);
            coloredTexture.Apply();
            
            // Clean up temporary texture
            Destroy(mainTextureReadable);
            
            // Create sprite with same settings as original
            return Sprite.Create(coloredTexture, new Rect(0, 0, width, height), 
                new Vector2(0.5f, 0.5f), mainSprite.pixelsPerUnit);
        }

        private Texture2D GetReadableTexture(Texture2D source, int x, int y, int width, int height)
        {
            // Check if texture is already readable
            try
            {
                source.GetPixels(x, y, width, height);
                // If we get here, texture is readable - create a copy
                Color[] pixels = source.GetPixels(x, y, width, height);
                Texture2D readableTexture = new Texture2D(width, height);
                readableTexture.SetPixels(pixels);
                readableTexture.Apply();
                return readableTexture;
            }
            catch
            {
                // Texture is not readable, use RenderTexture approach
                RenderTexture renderTexture = RenderTexture.GetTemporary(
                    source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                
                Graphics.Blit(source, renderTexture);
                
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = renderTexture;
                
                Texture2D readableTexture = new Texture2D(width, height);
                readableTexture.ReadPixels(new Rect(x, source.height - y - height, width, height), 0, 0);
                readableTexture.Apply();
                
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTexture);
                
                return readableTexture;
            }
        }

        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                if (collectingTile != null && m_Upgrade != null)
                {
                    collectingTile.TileData.AddUpgrade(m_Upgrade);
                }
                return true;
            }
            return false;
        }
    }
} 