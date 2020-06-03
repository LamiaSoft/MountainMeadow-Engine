using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// ReSharper disable once CheckNamespace

namespace MountainMeadowEngine.ViewportAdapters {
  public class ScalingViewportAdapter : ViewportAdapter {
    public ScalingViewportAdapter(GraphicsDevice graphicsDevice, int virtualWidth, int virtualHeight)
      : base(graphicsDevice) {
      VirtualWidth = virtualWidth;
      VirtualHeight = virtualHeight;
    }

    public override int VirtualWidth { get; }
    public override int VirtualHeight { get; }
    public override int ViewportWidth => (int)(GraphicsDevice.PresentationParameters.Bounds.Width);
    public override int ViewportHeight => (int)(GraphicsDevice.PresentationParameters.Bounds.Height);// / (GameWorld.GAME_RESOLUTION.X / GameWorld.GAME_RESOLUTION.Y));

    public override Matrix GetScaleMatrix() {
      var scaleX = (float)ViewportWidth / VirtualWidth;
      var scaleY = (float)ViewportHeight / VirtualHeight;
      return Matrix.CreateScale(scaleX, scaleY, 1.0f);
    }
  }
}