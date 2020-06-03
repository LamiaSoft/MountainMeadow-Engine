using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MountainMeadowEngine {
  
  public static class ViewportManager {
    public enum Orientations { LANDSCAPE, PORTRAIT };
    public enum ScaleModes { X_LEADING, Y_LEADING, NEAREST };

    //public static Vector2 GAME_RESOLUTION = new Vector2();
    public static Vector2 GAME_VIEWPORT = new Vector2();
    public static Vector2 GAME_VIEWPORT_PADDING = new Vector2(0, 0);
    public static Vector2 SCREEN_RESOLUTION = new Vector2();
    public static Vector2 PREV_SCREEN_RESOLUTION = new Vector2();
    public static float GAME_VIEWPORT_SCALE = 1f;
    public static Orientations SCREEN_ORIENTATION = Orientations.LANDSCAPE;
    public static ScaleModes SCALE_MODE = ScaleModes.NEAREST;

    public static float leadingMaxPoint;


    public static void SetScaleMode(ScaleModes mode, float maxPoint = -1) {
      SCALE_MODE = mode;
      leadingMaxPoint = maxPoint;
    }


    public static bool Update(GraphicsDeviceManager graphics, bool force = false) {
#if __ANDROID__
      SCREEN_RESOLUTION.X = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
      SCREEN_RESOLUTION.Y = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#else
      SCREEN_RESOLUTION.X = graphics.PreferredBackBufferWidth; //graphics.GraphicsDevice.Viewport.Width;// graphics.PreferredBackBufferWidth;
      SCREEN_RESOLUTION.Y = graphics.PreferredBackBufferHeight; //graphics.GraphicsDevice.Viewport.Height;//graphics.PreferredBackBufferHeight;
#endif



      if (SCREEN_RESOLUTION == PREV_SCREEN_RESOLUTION && !force) {
        return false;
      }
      Console.WriteLine(SCREEN_RESOLUTION);

      PREV_SCREEN_RESOLUTION = SCREEN_RESOLUTION;

      float viewportRatio = SCREEN_RESOLUTION.X / SCREEN_RESOLUTION.Y;
      SCREEN_ORIENTATION = (viewportRatio >= 1) ? Orientations.LANDSCAPE : Orientations.PORTRAIT;

      Vector2 gameResolution = (SCREEN_ORIENTATION == Orientations.LANDSCAPE && GameWorld.GAME_RESOLUTION.X / GameWorld.GAME_RESOLUTION.Y >= 1 ||
                                SCREEN_ORIENTATION == Orientations.PORTRAIT && GameWorld.GAME_RESOLUTION.Y / GameWorld.GAME_RESOLUTION.X >= 1) ? GameWorld.GAME_RESOLUTION : GameWorld.ALT_GAME_RESOLUTION;
      
      Console.WriteLine("width: " + graphics.PreferredBackBufferWidth);
      Console.WriteLine("screen resolution: " + SCREEN_RESOLUTION);
      Console.WriteLine("res: " + gameResolution);
      Console.WriteLine("viewport ratio: " + viewportRatio);

      if (SCALE_MODE == ScaleModes.NEAREST) {
        if (gameResolution.X / gameResolution.Y >= 1) {
          if (gameResolution.X / viewportRatio >= gameResolution.Y) {
            GAME_VIEWPORT.Y = gameResolution.Y;
            GAME_VIEWPORT.X = gameResolution.Y * viewportRatio;
          } else {
            GAME_VIEWPORT.X = gameResolution.X;
            GAME_VIEWPORT.Y = gameResolution.X / viewportRatio;
          }
        } else {
          if (gameResolution.Y * viewportRatio >= gameResolution.X) {
            GAME_VIEWPORT.Y = gameResolution.Y;
            GAME_VIEWPORT.X = gameResolution.Y * viewportRatio;
          } else {
            GAME_VIEWPORT.X = gameResolution.X;
            GAME_VIEWPORT.Y = gameResolution.X / viewportRatio;
          }
        }
      }

      if (SCALE_MODE == ScaleModes.X_LEADING) {
        GAME_VIEWPORT.Y = gameResolution.Y;
        GAME_VIEWPORT.X = gameResolution.Y * viewportRatio;
        if (leadingMaxPoint == -1 || GAME_VIEWPORT.X <= leadingMaxPoint) {
          Tools.Debug.Json("YO");
          Tools.Debug.Json(gameResolution);
          GAME_VIEWPORT.X = gameResolution.X;
          GAME_VIEWPORT.Y = gameResolution.X / viewportRatio;
        }
      }

      if (SCALE_MODE == ScaleModes.Y_LEADING) {
        GAME_VIEWPORT.X = gameResolution.X;
        GAME_VIEWPORT.Y = gameResolution.X * viewportRatio;
        if (leadingMaxPoint == -1 || GAME_VIEWPORT.Y <= leadingMaxPoint) {
          GAME_VIEWPORT.Y = gameResolution.Y;
          GAME_VIEWPORT.X = gameResolution.Y / viewportRatio;
        }
      }

      GAME_VIEWPORT_SCALE = GAME_VIEWPORT.X / SCREEN_RESOLUTION.X;
      GAME_VIEWPORT_PADDING.X = (GAME_VIEWPORT.X - GameWorld.GAME_RESOLUTION.X) / 2;
      GAME_VIEWPORT_PADDING.Y = (GAME_VIEWPORT.Y - GameWorld.GAME_RESOLUTION.Y) / 2;

      Console.WriteLine("viewport: " + GAME_VIEWPORT);

      Console.WriteLine("padding: " + GAME_VIEWPORT_PADDING);

      return true;
    }


  }
}
