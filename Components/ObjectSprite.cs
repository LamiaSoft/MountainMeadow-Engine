using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.TexturePacker;

namespace MountainMeadowEngine.Objects {

  public class ObjectSprite {
    public Texture2D texture;
    public int textureWidth = 0, textureHeight = 0;
    public Rectangle sourceRectangle;
    public Color color;
    public float rotation;
    public Vector2 origin;
    public Vector2 scale;
    public Vector2 pivotPoint;
    public SpriteEffects spriteEffects;

    //Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);
    private const float ClockwiseNinetyDegreeRotation = (float)(Math.PI / 2.0f);

    public void SetSprite(SpriteFrame sprite, Color color, float rotation = 0, float scale = 1, SpriteEffects spriteEffects = SpriteEffects.None) {
      if (sprite != null) {
        origin = sprite.Origin;

        this.color = color;
        this.scale = new Vector2(scale, scale);
        this.spriteEffects = SpriteEffects.None;
        this.rotation = rotation;

        if (sprite.IsRotated) {
          rotation -= ClockwiseNinetyDegreeRotation;
          switch (spriteEffects) {
            case SpriteEffects.FlipHorizontally: spriteEffects = SpriteEffects.FlipVertically; break;
            case SpriteEffects.FlipVertically: spriteEffects = SpriteEffects.FlipHorizontally; break;
          }
          pivotPoint.X = sprite.SourceRectangle.Height * sprite.PivotPoint.X;
          pivotPoint.Y = sprite.SourceRectangle.Width * sprite.PivotPoint.Y;

          textureWidth = sprite.SourceRectangle.Height;
          textureHeight = sprite.SourceRectangle.Width;
        } else {
          pivotPoint.X = sprite.SourceRectangle.Width * sprite.PivotPoint.X;
          pivotPoint.Y = sprite.SourceRectangle.Height * sprite.PivotPoint.Y;

          textureWidth = sprite.SourceRectangle.Width;
          textureHeight = sprite.SourceRectangle.Height;
        }

        switch (spriteEffects) {
          case SpriteEffects.FlipHorizontally: origin.X = sprite.SourceRectangle.Width - origin.X; break;
          case SpriteEffects.FlipVertically: origin.Y = sprite.SourceRectangle.Height - origin.Y; break;
        }

        texture = sprite.Texture;
        sourceRectangle = sprite.SourceRectangle;
        this.rotation = rotation;
        this.spriteEffects = spriteEffects;
      } else {
        texture = null;
      }
    }
  }
}
