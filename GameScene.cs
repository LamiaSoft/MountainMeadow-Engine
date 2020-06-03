using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Interfaces;
using MountainMeadowEngine.Cameras;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Tools;

namespace MountainMeadowEngine {

  public abstract class GameScene : IEventListener {
    public enum SceneStatuses { ACTIVE_FRONT, ACTIVE_BACK, INACTIVE, TRANSITION_IN, TRANSITION_TO_BACK, TRANSITION_OUT };

    protected ObjectManager objectManager;
    protected SceneStatuses status = SceneStatuses.INACTIVE;
    protected List<GameCamera> gameCameras = new List<GameCamera>();
    protected List<Viewport> viewports = new List<Viewport>();
    protected GraphicsDevice graphicsDevice;
    protected bool paused = false;

    private Dictionary<int, GameObject> _gameObjects;

    public GameScene() {
      EventManager.AddEventListener<ObjectEvent>(this);
      EventManager.AddEventListener<UpdateEvent>(this, UpdateEvent.Values.POST_COLLISION);
    }

    public void SetStatus(SceneStatuses status) {
      if (this.status == status)
        return;
      
      bool error = false, transition = false;

      if (this.status == SceneStatuses.TRANSITION_IN) {
        transition = true;
        if (status != SceneStatuses.ACTIVE_FRONT) {
          error = true;
        }
      }

      if (this.status == SceneStatuses.TRANSITION_TO_BACK) {
        transition = true;
        if (status != SceneStatuses.ACTIVE_BACK) {
          error = true;
        }
      }

      if (this.status == SceneStatuses.TRANSITION_OUT) {
        transition = true;
        if (status != SceneStatuses.INACTIVE) {
          error = true;
        }
      }

      if (error) {
        throw new Exception(GetType().Name + ": Wrong scene transition from \"" + this.status.ToString() + "\" to \"" + status.ToString() + "\".");
      }

      this.status = status;

      if (transition) {
        EventManager.PushEvent(GameEvent.Create<SceneEvent>(SceneEvent.Values.TRANSITION_DONE, this));
      }
    }

    public SceneStatuses GetStatus() {
      return status;
    }

    public static T Create<T>(Vector3 spatialBucketPixelSize, GraphicsDevice graphicsDevice) {
      if (typeof(T).IsSubclassOf(typeof(GameScene))) {
        T instance = Activator.CreateInstance<T>();
        ((GameScene)(object)instance).graphicsDevice = graphicsDevice;
        ((GameScene)(object)instance).objectManager = new ObjectManager(spatialBucketPixelSize, typeof(T));
        ((GameScene)(object)instance).Initialize();
        return instance;
      } else {
        throw new Exception("Invalid Scene Type \"" + typeof(T).Name + "\" provided.");
      }
    }

    public ObjectManager GetObjectManager() {
      return objectManager;
    }

    public int AddGameCamera(GameCamera camera, Viewport? viewport = null) {
      gameCameras.Add(camera);
      if (viewport == null) {
        viewport = new Viewport(0, 0, (int)ViewportManager.SCREEN_RESOLUTION.X, (int)ViewportManager.SCREEN_RESOLUTION.Y);
      }
      viewports.Add((Viewport)viewport);
      return gameCameras.Count - 1;
    }

    public GameCamera GetCameraById(int id) {
      if (id < gameCameras.Count) {
        return gameCameras[id];
      }
      throw new Exception("Game Camera with id " + id + " does not exist.");
    }

    public List<GameCamera> GetCameras() {
      return gameCameras;
    }

    public Viewport GetViewportById(int id) {
      if (id < viewports.Count) {
        return viewports[id];
      }
      throw new Exception("Viewport with id " + id + " does not exist.");
    }


    public void Reset() {
      Initialize();
    }


    public virtual GameEvent OnEvent(GameEvent gameEvent) {
      if (gameEvent is ObjectEvent && ((ObjectEvent)gameEvent).GetSceneContextType() == this.GetType()) {
        if ((ObjectEvent.Values)gameEvent.GetValue() == ObjectEvent.Values.STATUS_CHANGE) {
          if (((ObjectEvent)gameEvent).GetStatus() == GameObject.ObjectStatuses.INACTIVE) {
            GetObjectManager().Delete(((ObjectEvent)gameEvent).GetGameObjectId());
            Tools.Debug.Json("DELETED");
          }
        }
      }

      if (gameEvent is UpdateEvent && (UpdateEvent.Values)gameEvent.GetValue() == UpdateEvent.Values.POST_COLLISION) {
        ProcessObjectEvents();
      }

      return gameEvent;
    }

    private void ProcessObjectEvents() {
      _gameObjects = GetObjectManager().GetAllGameObjects();

      foreach (GameObject _currentObject in _gameObjects.Values) {
        if (_currentObject.GetStatus() != GameObject.ObjectStatuses.INACTIVE && _currentObject.GetStatus() != GameObject.ObjectStatuses.STANDBY) {
          if (_currentObject.GetMovableEntity() != null) {
            ProcessObjectMovementEvent(_currentObject);
          }
          ProcessObjectGeneralEvent(_currentObject);
        }
      }
    }

    protected void ProcessObjectMovementEvent(GameObject gameObject) {
      if (Math.Abs(gameObject.GetMovableEntity().GetVelocity().X) > 0 || Math.Abs(gameObject.GetMovableEntity().GetVelocity().Y) > 0) {
        
      } else {
        gameObject.PushEventToComponents(GameEvent.Create<ActionEvent>(ActionEvent.Values.STAND, this));
      }
    }

    protected GameObject GetGameObject(int id) {
      return GetObjectManager().GetGameObjectById<GameObject>(id);
    }

    protected void ProcessObjectGeneralEvent(GameObject gameObject) { }

    public abstract void Initialize();

    public virtual void Show(bool inTransition = false) { }


    public virtual bool IsPaused() {
      return paused;
    }

    public virtual void Pause() {
      paused = true;
    }

    public virtual void Unpause() {
      paused = false;
    }

    public virtual bool Hide() {
      return true;  
    }

    public virtual bool MoveToBack() {
      Pause();
      return true;
    }

    public virtual void Upfate(GameTime gameTime, bool pushMovementEvents = false) {
      


        //if (pushMovementEvents) {
        //  if (_currentObject.GetMovableEntity() != null) {
        //    if (_currentObject.GetMovableEntity().GetVelocity().Z < 0) {
        //      _currentObject.OnEvent(GameEvent.Create<MovementEvent>(MovementEvent.Values.JUMP, this));
        //    } else if (_currentObject.GetMovableEntity().GetVelocity().Z > 0) {
        //      _currentObject.OnEvent(GameEvent.Create<MovementEvent>(MovementEvent.Values.FALLING, this));
        //    } else if (Math.Abs(_currentObject.GetMovableEntity().GetVelocity().X) > 0 || Math.Abs(_currentObject.GetMovableEntity().GetVelocity().Y) > 0) {
        //      _currentObject.OnEvent(GameEvent.Create<MovementEvent>(MovementEvent.Values.WALK, this));
        //    } else {
        //      _currentObject.OnEvent(GameEvent.Create<MovementEvent>(MovementEvent.Values.STAND, this));
        //    }
        //  }

        //}

    }


    public virtual bool DrafwUpdate(GameTime gameTime) { return true; }

  }
}
