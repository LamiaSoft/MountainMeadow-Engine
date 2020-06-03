using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Collision;
using MountainMeadowEngine.Managers;
using MountainMeadowEngine.Tools;

namespace MountainMeadowEngine.Components {
  
  public class Background {
    string textureName;
    Rectangle3D location;
    Rectangle sourceRectangle = new Rectangle();
    int width, height;

    Vector2 currentDrawPosition = new Vector2();

    public Background(string name, Vector2 position) {
      Texture2D texture = GameContentManager.LoadTexture(name);
      width = texture.Width;
      height = texture.Height;
      location = new Rectangle3D(0, 0, 0, texture.Width, texture.Height, 1);
      location.SetPosition(position.X, position.Y, 0);
      textureName = name;
    }

    public string GetName() {
      return textureName;
    }

    public Rectangle3D GetLocation() {
      return location;
    }

    public Rectangle GetSourceRectangle() {
      return sourceRectangle;
    }

    public Vector2 GetCurrentDrawPosition() {
      return currentDrawPosition;
    }

    public void SetCurrentDrawPosition(Vector2 position) {
      currentDrawPosition = position;
    }

    public void SetSourceRectangle(Rectangle sourceRectangle) {
      this.sourceRectangle = sourceRectangle;
    }

    public void ApplyOffsetToLocation(Vector2 offset) {
      location.X += offset.X;
      location.Y += offset.Y;
    }
  }
}
