using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Cameras;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Managers;
using MountainMeadowEngine.Tools;

namespace MountainMeadowEngine {
  
  public abstract class GameWorld : Game, IEventListener {
    public enum DebugModes { NONE, NOTICE, WARNING, CRITICAL };

    public static DebugModes DEBUG_MODE = DebugModes.NOTICE;

    protected GraphicsDeviceManager graphics;

    public static double UPDATE_STEP = 1f / 30;

    public static Vector2 GAME_RESOLUTION = new Vector2(1920, 1080);
    public static Vector2 ALT_GAME_RESOLUTION = new Vector2(1920, 1080);

    protected SceneManager sceneManager;
    protected InputManager inputManager;
    protected RenderManager renderManager;
    protected PhysicsManager physicsManager;
    protected GameContentManager contentManager;

    double timeSinceLastUpdate;

    public GameWorld() {
      graphics = new GraphicsDeviceManager(this);
      contentManager = new GameContentManager();
      contentManager.Initialize(this);

      EventManager.AddEventListener<UpdateEvent>(this);

      IsFixedTimeStep = true;
      graphics.SynchronizeWithVerticalRetrace = true;
      graphics.IsFullScreen = true;

      graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
      graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

      //graphics.PreferredBackBufferWidth = 800;
      //graphics.PreferredBackBufferHeight = 640;

      //graphics.PreferredBackBufferWidth = 1170;
      //graphics.PreferredBackBufferHeight = 540;


      IsMouseVisible = true;
      Window.AllowUserResizing = false;
      graphics.ApplyChanges();
    }

    protected override void Initialize() {
      base.Initialize();

      ViewportManager.Update(graphics);

      GameCamera HUDCam = new GameCamera(GraphicsDevice, (int)ViewportManager.GAME_VIEWPORT.X, (int)ViewportManager.GAME_VIEWPORT.Y);

      sceneManager = new SceneManager(GraphicsDevice, Content);
      inputManager = new InputManager();
      renderManager = new RenderManager(GraphicsDevice, HUDCam);
      physicsManager = new PhysicsManager();
     
      sceneManager.SetSpatialBucketPixelSize(new Vector3((float)Math.Round(GAME_RESOLUTION.X / 6), (float)Math.Round(GAME_RESOLUTION.Y / 6), (float)Math.Round(GAME_RESOLUTION.Y / 4)));
      //sceneManager.SetSpatialBucketPixelSize(new Vector3(100, 100, 100));

      inputManager.SetHUDCam(HUDCam);
    }

    protected override void Draw(GameTime gameTime) {
      timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalSeconds;

      while (timeSinceLastUpdate >= UPDATE_STEP) {
        inputManager.Update(gameTime);

        if (ViewportManager.Update(graphics)) {
          EventManager.PushEvent(GameEvent.Create<ViewportEvent>(ViewportEvent.Values.CHANGED, this));
        }
        physicsManager.Update(gameTime, sceneManager.GetOrderedScenes());
        timeSinceLastUpdate -= UPDATE_STEP;
      }

      renderManager.Update(gameTime, timeSinceLastUpdate, sceneManager.GetOrderedScenes());
    }

    public abstract GameEvent OnEvent(GameEvent gameEvent);

    public static float GetTimeSteppedValue(float value) {
      //Console.WriteLine((1f / 60f) / UPDATE_STEP);
      return (float)(value * (UPDATE_STEP / (1f / 60f)));
    }
  }
}
