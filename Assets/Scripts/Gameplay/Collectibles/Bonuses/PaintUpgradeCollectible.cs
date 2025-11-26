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
                return GenerateColoredDisplaySprite(mainSprite, displayColor);
            }
            
            // Fallback to base implementation for non-colored upgrades
            return base.GetDisplaySprite();
        }

        private Sprite GenerateColoredDisplaySprite(Sprite mainSprite, Color tintColor)
        {
            if (mainSprite == null) return null;
            
            // Get sprite texture bounds
            Rect spriteRect = mainSprite.rect;
            
            // Determine if we need level indicators
            bool needsIndicators = m_Level > 1 && ShouldShowLevelIndicators();
            
            if (!needsIndicators)
            {
                // Simple colored sprite without level indicators
                return CreateColoredSprite(mainSprite, tintColor);
            }
            else
            {
                // Composite sprite with level indicators and color
                return CreateColoredCompositeSprite(mainSprite, tintColor);
            }
        }

        private Sprite CreateColoredSprite(Sprite mainSprite, Color tintColor)
        {
            if (mainSprite == null) return null;
            
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
            
            // Clean up temporary texture immediately since it's no longer needed
            DestroyImmediate(mainTextureReadable);
            
            // Note: coloredTexture is not destroyed here as it's owned by the returned Sprite.
            // Unity's sprite system will manage the texture lifecycle and clean it up when
            // the sprite is no longer referenced.
            
            // Create sprite with same settings as original
            return Sprite.Create(coloredTexture, new Rect(0, 0, width, height), 
                new Vector2(0.5f, 0.5f), mainSprite.pixelsPerUnit);
        }

        private Sprite CreateColoredCompositeSprite(Sprite mainSprite, Color tintColor)
        {
            if (mainSprite == null) return null;
            
            // Get sprite texture bounds
            Rect spriteRect = mainSprite.rect;
            int width = (int)spriteRect.width;
            int height = (int)spriteRect.height;
            
            // Create readable copy of main sprite texture
            Texture2D mainTextureReadable = GetReadableTexture(mainSprite.texture, (int)spriteRect.x, (int)spriteRect.y, width, height);
            
            // Create texture with padding for indicators (top-right)
            int padding = Mathf.Max(width, height) / 3; // 33% padding for indicators
            int compositeWidth = width + padding;
            int compositeHeight = height + padding;
            
            Texture2D compositeTexture = new Texture2D(compositeWidth, compositeHeight, TextureFormat.RGBA32, false);
            
            // Fill with transparent
            Color[] pixels = new Color[compositeWidth * compositeHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            // Copy main sprite to center-left and apply tint color
            Color[] mainSpritePixels = mainTextureReadable.GetPixels();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * width + x;
                    int dstIndex = (y + padding / 2) * compositeWidth + x;
                    if (dstIndex >= 0 && dstIndex < pixels.Length)
                    {
                        // Apply tint color to the sprite pixel
                        Color spritePixel = mainSpritePixels[srcIndex];
                        pixels[dstIndex] = new Color(
                            spritePixel.r * tintColor.r,
                            spritePixel.g * tintColor.g,
                            spritePixel.b * tintColor.b,
                            spritePixel.a * tintColor.a
                        );
                    }
                }
            }
            
            // Add level indicators in top-right
            Sprite plusSprite = BaseCollectible.GetPlusSpriteForUI();
            int indicatorCount = m_Level - 1;
            float indicatorSize = Mathf.Min(width, height) * 0.3f;
            float spacing = indicatorSize * 1.2f;
            
            // Create readable copy of plus sprite texture
            Texture2D plusTextureReadable = GetReadableTexture(plusSprite.texture, (int)plusSprite.rect.x, (int)plusSprite.rect.y, 
                (int)plusSprite.rect.width, (int)plusSprite.rect.height);
            
            for (int i = 0; i < indicatorCount; i++)
            {
                int indicatorWidth = (int)indicatorSize;
                int indicatorHeight = (int)indicatorSize;
                
                // Position in top-right, moving down
                int offsetX = width + padding / 2 - indicatorWidth / 2;
                int offsetY = height + padding / 2 - indicatorHeight / 2 - (int)(spacing * i);
                
                // Get plus sprite pixels
                Color[] plusPixels = plusTextureReadable.GetPixels();
                
                // Scale plus sprite to indicator size
                for (int y = 0; y < indicatorHeight && (offsetY + y) < compositeHeight; y++)
                {
                    for (int x = 0; x < indicatorWidth && (offsetX + x) < compositeWidth; x++)
                    {
                        int srcY = (int)((float)y / indicatorHeight * plusSprite.rect.height);
                        int srcX = (int)((float)x / indicatorWidth * plusSprite.rect.width);
                        int srcIndex = srcY * (int)plusSprite.rect.width + srcX;
                        
                        if (srcIndex >= 0 && srcIndex < plusPixels.Length)
                        {
                            int dstIndex = (offsetY + y) * compositeWidth + (offsetX + x);
                            if (dstIndex >= 0 && dstIndex < pixels.Length)
                            {
                                Color plusColor = plusPixels[srcIndex];
                                if (plusColor.a > 0)
                                {
                                    pixels[dstIndex] = plusColor;
                                }
                            }
                        }
                    }
                }
            }
            
            compositeTexture.SetPixels(pixels);
            compositeTexture.Apply();
            
            // Clean up temporary textures immediately since they're no longer needed
            DestroyImmediate(mainTextureReadable);
            DestroyImmediate(plusTextureReadable);
            
            // Note: compositeTexture is not destroyed here as it's owned by the returned Sprite.
            // Unity's sprite system will manage the texture lifecycle and clean it up when
            // the sprite is no longer referenced.
            
            // Create sprite with pivot at center (adjust for padding)
            float pivotX = (width / 2f + padding / 2f) / compositeWidth;
            float pivotY = (height / 2f + padding / 2f) / compositeHeight;
            
            return Sprite.Create(compositeTexture, new Rect(0, 0, compositeWidth, compositeHeight), 
                new Vector2(pivotX, pivotY), mainSprite.pixelsPerUnit);
        }

        private Texture2D GetReadableTexture(Texture2D source, int x, int y, int width, int height)
        {
            // Check if texture is already readable
            try
            {
                Color[] pixels = source.GetPixels(x, y, width, height);
                // If we get here, texture is readable - create a copy
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