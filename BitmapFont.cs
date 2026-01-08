using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;

public class BitmapFont
{
    private class CharInfo
    {
        public Rectangle SourceRect;
        public Vector2 Offset;
        public int XAdvance;
    }

    private Texture2D _texture;
    private Dictionary<char, CharInfo> _chars = new();

    public int LineHeight { get; private set; }

    public BitmapFont(Texture2D texture, string fntFilePath)
    {
        _texture = texture;
        LoadFnt(fntFilePath);
    }

private void LoadFnt(string path)
{
    foreach (var line in File.ReadAllLines(path))
    {
        // ⬇️ načtení lineHeight
        if (line.StartsWith("common "))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length != 2) continue;

                if (kv[0] == "lineHeight")
                {
                    LineHeight = int.Parse(kv[1]);
                }
            }
        }

        // ⬇️ glyphy
        if (line.StartsWith("char id="))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int id = 0, x = 0, y = 0, width = 0, height = 0;
            int xoffset = 0, yoffset = 0, xadvance = 0;

            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length != 2) continue;

                switch (kv[0])
                {
                    case "id": id = int.Parse(kv[1]); break;
                    case "x": x = int.Parse(kv[1]); break;
                    case "y": y = int.Parse(kv[1]); break;
                    case "width": width = int.Parse(kv[1]); break;
                    case "height": height = int.Parse(kv[1]); break;
                    case "xoffset": xoffset = int.Parse(kv[1]); break;
                    case "yoffset": yoffset = int.Parse(kv[1]); break;
                    case "xadvance": xadvance = int.Parse(kv[1]); break;
                }
            }

            _chars[(char)id] = new CharInfo
            {
                SourceRect = new Rectangle(x, y, width, height),
                Offset = new Vector2(xoffset, yoffset),
                XAdvance = xadvance
            };
        }
    }

    // fallback kdyby náhodou nebyl ve fnt
    if (LineHeight == 0 && _chars.Count > 0)
    {
        LineHeight = _chars.Values.Max(c => c.SourceRect.Height);
    }
}


    public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
    {
        Vector2 cursor = position;

        foreach (var c in text)
        {
            if (_chars.TryGetValue(c, out var info))
            {
                spriteBatch.Draw(_texture, cursor + info.Offset, info.SourceRect, color);
                cursor.X += info.XAdvance;
            }
        }
    }

    public Vector2 MeasureString(string text)
    {
        float width = 0;
        float maxWidth = 0;
        float height = LineHeight;

        foreach (char c in text)
        {
            if (c == '\n')
            {
                maxWidth = Math.Max(maxWidth, width);
                width = 0;
                height += LineHeight;
                continue;
            }

            if (_chars.TryGetValue(c, out var glyph))
            {
                width += glyph.XAdvance;
            }
        }

        maxWidth = Math.Max(maxWidth, width);
        return new Vector2(maxWidth, height);
    }

}
