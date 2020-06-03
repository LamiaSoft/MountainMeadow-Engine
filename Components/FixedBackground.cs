using System;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Cameras;

namespace MountainMeadowEngine.Components {
  public class FixedBackground : GameCameraComponent {
    string textureName;
    Vector2 position = new Vector2(0, 0);
    Vector2 currentDrawPosition = new Vector2(0, 0);
    bool isForeground = false;

    public FixedBackground(GameCamera context) : base(context) { }

    public FixedBackground SetBackground(string name) {
      textureName = name;
      return this;
    }

    public FixedBackground SetPosition(Vector2 position) {
      this.position = position;
      return this;
    }

    public override void DrawUpdate(GameTime gameTime) {
      currentDrawPosition = context.GetPosition() + position;
    }

    public Vector2 GetCurrentDrawPosition() {
      return currentDrawPosition;
    }

    public string GetName() {
      return textureName;
    }

    public bool IsForeGround() {
      return isForeground;
    }

    public FixedBackground SetForeground(bool foreground) {
      this.isForeground = foreground;
      return this;
    }

    public override void Initialize() {   
    }

  }
}
