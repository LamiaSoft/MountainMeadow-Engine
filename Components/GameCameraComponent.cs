using System;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Cameras;

namespace MountainMeadowEngine.Components {
  
  public abstract class GameCameraComponent {
    protected GameCamera context;

    public GameCameraComponent(GameCamera context) {
      this.context = context;
    }

    public abstract void DrawUpdate(GameTime gameTime);
    public abstract void Initialize();

  }
}
