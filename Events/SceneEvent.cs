using System;
using System.Collections.Generic;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {
  
  public class SceneEvent : GameEvent {
    bool showAtFront = true;
    Type sceneType;
    Type nextSceneType;

    public enum Values { SHOW, HIDE, PAUSE, MOVE_TO_BACK, MOVE_TO_FRONT, TRANSITION_DONE };

    public SceneEvent SetAtFront(bool atFront) { showAtFront = atFront; return this; }
    public SceneEvent SetScene(Type scene) { sceneType = scene; return this; }
    public SceneEvent SetNextScene(Type nextScene) { nextSceneType = nextScene; return this; }
    public bool ShowAtFront() { return showAtFront; }
    public Type GetScene() { return sceneType; }
    public Type GetNextScene() { return nextSceneType; }
  }
}
