using System.Collections.Generic;
using UnityEngine;

public static class SpriteMapParser
{
    public static List<ColorMapping> ParsePalette(Texture2D texture, float tolerance)
    {
        var mappings = new List<ColorMapping>();
        if (texture == null) return mappings;

        Color[] pixels = texture.GetPixels();

        foreach (var pixel in pixels)
        {
            if (pixel.a < 0.1f) continue;

            ColorMapping existing = FindSimilarColor(mappings, pixel, tolerance);

            if (existing != null)
            {
                existing.PixelCount++;
            }
            else
            {
                mappings.Add(new ColorMapping
                {
                    OriginalColor = pixel,
                    TargetColor = pixel,
                    PixelCount = 1
                });
            }
        }

        mappings.Sort((a, b) => b.PixelCount.CompareTo(a.PixelCount));
        return mappings;
    }

    public static Texture2D GeneratePreviewTexture(Texture2D source, List<ColorMapping> mappings, float tolerance, Texture2D existingPreview = null)
    {
        if (source == null) return null;

        int width = source.width;
        int height = source.height;

        Texture2D preview = existingPreview;
        if (preview == null || preview.width != width || preview.height != height)
        {
            preview = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
        }

        Color[] pixels = source.GetPixels();
        Color[] updatedPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color current = pixels[i];
            if (current.a < 0.1f)
            {
                updatedPixels[i] = Color.clear;
                continue;
            }

            ColorMapping mapping = FindSimilarColor(mappings, current, tolerance);
            updatedPixels[i] = mapping != null ? mapping.TargetColor : current;
        }

        preview.SetPixels(updatedPixels);
        preview.Apply();
        return preview;
    }

    public static LevelDataSO.CellData[] GenerateGridCells(Texture2D source, List<ColorMapping> mappings, float tolerance)
    {
        int width = source.width;
        int height = source.height;
        Color[] pixels = source.GetPixels();

        var gridCells = new LevelDataSO.CellData[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color currentPixel = pixels[index];

                int colorIdx = LevelDataSO.CellData.Empty;

                if (currentPixel.a >= 0.1f)
                {
                    ColorMapping mapping = FindSimilarColor(mappings, currentPixel, tolerance);
                    if (mapping != null)
                    {
                        colorIdx = mappings.IndexOf(mapping);
                    }
                }

                var cell = new LevelDataSO.CellData();
                cell.Set(x, y, colorIdx);
                gridCells[index] = cell;
            }
        }

        return gridCells;
    }

    public static ColorMapping FindSimilarColor(List<ColorMapping> mappings, Color target, float tolerance)
    {
        foreach (var mapping in mappings)
        {
            if (ColorDistance(mapping.OriginalColor, target) <= tolerance)
            {
                return mapping;
            }
        }
        return null;
    }

    public static float ColorDistance(Color c1, Color c2)
    {
        float rDiff = c1.r - c2.r;
        float gDiff = c1.g - c2.g;
        float bDiff = c1.b - c2.b;
        return Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }
}