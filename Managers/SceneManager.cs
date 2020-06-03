using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;
using static MountainMeadowEngine.GameScene;

namespace MountainMeadowEngine.Managers {
  
  public class SceneManager : IEventListener {
    protected static Dictionary<Type, GameScene> sceneList = new Dictionary<Type, GameScene>();
    List<GameScene> orderedScenes = new List<GameScene>();
    Type frontMost;
    Type prevFrontMost;
    GraphicsDevice graphicsDevice;
    ContentManager content;

    Vector3 spatialBucketPixelSize;

    public SceneManager(GraphicsDevice graphicsDevice, ContentManager content) {
      this.graphicsDevice = graphicsDevice;
      this.content = content;
      EventManager.AddEventListener<SceneEvent>(this);
    }

    public void SetSpatialBucketPixelSize(Vector3 size) {
      spatialBucketPixelSize = size;
    }

    public static SceneStatuses GetSceneStatus(Type type) {
      if (sceneList.ContainsKey(type)) {
        return sceneList[type].GetStatus();
      }
      throw new Exception("Scene of type \"" + type.Name + "\" does not exist in SceneManager.");
    }

    public static bool SceneIsPaused(Type type) {
      if (sceneList.ContainsKey(type)) {
        return sceneList[type].IsPaused();
      }
      throw new Exception("Scene of type \"" + type.Name + "\" does not exist in SceneManager.");
    }

    public void AddScene<T>() {
      if (!sceneList.ContainsKey(typeof(T))) {
        GameScene scene = (GameScene)(object)GameScene.Create<T>(spatialBucketPixelSize, graphicsDevice);
        sceneList.Add(typeof(T), scene);
      } else {
        throw new Exception("Scene of type \"" + typeof(T) + "\" already added to SceneManager.");
      }
    }

    public GameScene GetScene<T>() {
      if (sceneList.ContainsKey(typeof(T))) {
        return sceneList[typeof(T)];
      } else {
        throw new Exception("Scene of type \"" + typeof(T).Name + "\" does not exist in SceneManager.");
      }
    }

    protected void ShowScene(Type sceneType, bool showAtFront = true) {
      if (showAtFront) {
        if (frontMost != null) {
          bool inTransition = (sceneList[frontMost].GetStatus() == SceneStatuses.TRANSITION_TO_BACK || sceneList[frontMost].GetStatus() == SceneStatuses.TRANSITION_OUT);

          if (!inTransition && sceneList[frontMost].MoveToBack()) {
            sceneList[frontMost].SetStatus(GameScene.SceneStatuses.ACTIVE_BACK);
          } else {
            if (!inTransition) {
              sceneList[frontMost].SetStatus(SceneStatuses.TRANSITION_TO_BACK);
            }
            sceneList[sceneType].SetStatus(GameScene.SceneStatuses.TRANSITION_IN);
            sceneList[sceneType].Show(true);
            return;
          }
        }
        SetFrontMost(sceneType);
        sceneList[sceneType].SetStatus(GameScene.SceneStatuses.ACTIVE_FRONT);
        sceneList[sceneType].Show();
      } else {
        if (frontMost != null && frontMost == sceneType) {
          if (prevFrontMost != null) {
            ShowScene(prevFrontMost);
            return;
          }
        }
        sceneList[sceneType].SetStatus(GameScene.SceneStatuses.ACTIVE_BACK);
        sceneList[sceneType].Show();
      }

      UpdateList();
    }

    public void ShowScene<T>(bool showAtFront = true) {
      ShowScene(typeof(T), showAtFront);
    }

    public void CompleteTransitions() {
      foreach (GameScene scene in sceneList.Values) {
        if (scene.GetStatus() == SceneStatuses.TRANSITION_IN) {
          SetFrontMost(scene.GetType());
          scene.SetStatus(SceneStatuses.ACTIVE_FRONT);
          scene.Show();
        }
      }
      UpdateList();
    }

    protected void SetFrontMost(Type sceneType) {
      if (frontMost != null) {
        prevFrontMost = frontMost;
      }
      frontMost = sceneType;
    }

    public void HideScene<T>() {
      bool done = sceneList[typeof(T)].Hide();
      sceneList[typeof(T)].SetStatus(((done) ? GameScene.SceneStatuses.INACTIVE : SceneStatuses.TRANSITION_OUT));

      if (frontMost == typeof(T)) {
        if (done) {
          frontMost = null;
        }
        if (prevFrontMost != null) {
          ShowScene(prevFrontMost);
        } else {
          UpdateList();
        }
      } else if (prevFrontMost == typeof(T)) {
        prevFrontMost = null;
      }
    }

    public void PauseScene<T>() {
      sceneList[typeof(T)].Pause();
    }

    protected void UpdateList() {
      orderedScenes.Clear();
     
      foreach (KeyValuePair<Type, GameScene> entry in sceneList) {
        if ((frontMost == null || frontMost != entry.Key)) {
          orderedScenes.Add(entry.Value);
        } 
      }
      if (frontMost != null) {
        orderedScenes.Add(sceneList[frontMost]);
      }
    }
     
    public List<GameScene> GetOrderedScenes() {
      return orderedScenes;
    }

    public GameEvent OnEvent(GameEvent gameEvent) {
      if (gameEvent is SceneEvent) {
        bool? moveToFront = null;

        switch (gameEvent.GetValue()) {
          case SceneEvent.Values.MOVE_TO_BACK:
            moveToFront = false;
            goto case SceneEvent.Values.SHOW;
          case SceneEvent.Values.MOVE_TO_FRONT:
            moveToFront = true;
            goto case SceneEvent.Values.SHOW;
          case SceneEvent.Values.SHOW:
            moveToFront = (moveToFront == null) ? ((SceneEvent)gameEvent).ShowAtFront() : moveToFront;
            var showMethod = typeof(SceneManager).GetMethod("ShowScene");
            var showMethodScene = showMethod.MakeGenericMethod(new[] { ((SceneEvent)gameEvent).GetScene() });
            showMethodScene.Invoke(this, new object[] { moveToFront });
            break;
          case SceneEvent.Values.HIDE:
            var hideMethod = typeof(SceneManager).GetMethod("HideScene");
            var hideMethodScene = hideMethod.MakeGenericMethod(new[] { ((SceneEvent)gameEvent).GetScene() });
            hideMethodScene.Invoke(this, new object[] { });
            break;
          case SceneEvent.Values.PAUSE:
            var pauseMethod = typeof(SceneManager).GetMethod("PauseScene");
            var pauseMethodScene = pauseMethod.MakeGenericMethod(new[] { ((SceneEvent)gameEvent).GetScene() });
            pauseMethodScene.Invoke(this, new object[] { });
            break;
          case SceneEvent.Values.TRANSITION_DONE:
            CompleteTransitions();
            break;
        }

        return null;
      }

      return gameEvent;
    }
  }
}
