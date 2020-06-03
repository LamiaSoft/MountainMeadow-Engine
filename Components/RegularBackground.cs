using System;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Cameras;

namespace MountainMeadowEngine.Components {
  public class RegularBackground : GameCameraComponent {
    int layerDepth = 1;
    string textureName;
    Vector2 position = new Vector2(0, 0);
    bool isForeground = false;

    public RegularBackground(GameCamera context) : base(context) { }

    public RegularBackground SetBackground(string name) {
      textureName = name;
      return this;
    }

    public RegularBackground SetPosition(Vector2 position) {
      this.position = position;
      return this;
    }

    public RegularBackground SetLayerDepth(int depth) {
      this.layerDepth = depth;
      return this;
    }

    public int GetLayerDepth() {
      return layerDepth;
    }

    public override void DrawUpdate(GameTime gameTime) { }

    public Vector2 GetCurrentDrawPosition() {
      return position;
    }

    public string GetName() {
      return textureName;
    }

    public bool IsForeGround() {
      return isForeground;
    }

    public RegularBackground SetForeground(bool foreground) {
      this.isForeground = foreground;
      return this;
    }

    public override void Initialize() { }

  }
}
