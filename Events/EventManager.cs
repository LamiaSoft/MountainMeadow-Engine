using System;
using System.Collections.Generic;
using MountainMeadowEngine.Behavior;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {
  
  public static class EventManager {
    private static Dictionary<Type, Dictionary<object, List<IEventListener>>> eventListeners = new Dictionary<Type, Dictionary<object, List<IEventListener>>>();
    private static Dictionary<object, List<Tuple<Type, object>>> objectRegister = new Dictionary<object, List<Tuple<Type, object>>>();
    private static List<Tuple<Type, object>> pendingDeletion = new List<Tuple<Type, object>>();


    public static void AddEventListener<T>(IEventListener eventListener, object value = null) {
      pendingDeletion.Clear();

      if (value == null)
        value = "null";

      if (objectRegister.ContainsKey(eventListener)) {
        if (value != "null" && objectRegister[eventListener].Contains(Tuple.Create<Type, object>(typeof(T), "null")))
          return;
        
        foreach (Tuple<Type, object> t in objectRegister[eventListener]) {
          if (typeof(T) == t.Item1) {
            if (value == "null" || value == t.Item2) {
              pendingDeletion.Add(Tuple.Create(typeof(T), t.Item2));
            }
          }
        }
      }

      foreach (Tuple<Type, object> t in pendingDeletion) {
        eventListeners[t.Item1][t.Item2].Remove(eventListener);
        objectRegister[eventListener].Remove(t);
      }

      if (!eventListeners.ContainsKey(typeof(T))) {
        eventListeners.Add(typeof(T), new Dictionary<object, List<IEventListener>>());
      }
      if (!eventListeners[typeof(T)].ContainsKey(value)) {
        eventListeners[typeof(T)].Add(value, new List<IEventListener>());
      }

      eventListeners[typeof(T)][value].Add(eventListener);

      if (!objectRegister.ContainsKey(eventListener)) {
        objectRegister.Add(eventListener, new List<Tuple<Type, object>>());
      }
      objectRegister[eventListener].Add(Tuple.Create(typeof(T), value));
    }

    public static void RemoveEventListener(IEventListener eventListener) {
      if (objectRegister.ContainsKey(eventListener)) {
        foreach (Tuple<Type, object> t in objectRegister[eventListener]) {
          eventListeners[t.Item1][t.Item2].Remove(eventListener);
        }
        objectRegister.Remove(eventListener);
      }
    }

    public static void RemoveEventListener<T>(IEventListener eventListener, object atValue = null) {
      pendingDeletion.Clear();

      if (atValue == null)
        atValue = "null";
      
      if (objectRegister.ContainsKey(eventListener)) {
        foreach (Tuple<Type, object> t in objectRegister[eventListener]) {
          if (typeof(T) == t.Item1) {
            if (t.Item2 == atValue || atValue == "null") {
              pendingDeletion.Add(Tuple.Create(typeof(T), t.Item2));
            }
          }
        }

        foreach (Tuple<Type, object> t in pendingDeletion) {
          eventListeners[t.Item1][t.Item2].Remove(eventListener);
          objectRegister[eventListener].Remove(t);
        }

        if (objectRegister[eventListener].Count == 0) {
          objectRegister.Remove(eventListener);
        }
      }
    }


    public static void PushEvent(GameEvent gameEvent) {
      GameEvent currentEvent = gameEvent;

      if (eventListeners.ContainsKey(gameEvent.GetType())) {
        foreach (object obj in new List<object>() { "null", gameEvent.GetValue() }) {
          if (eventListeners[gameEvent.GetType()].ContainsKey(obj)) {
            for (int i = 0; i < eventListeners[gameEvent.GetType()][obj].Count; i++) {
              if (gameEvent.GetSceneContextType() != null) {
                Type type = eventListeners[gameEvent.GetType()][obj][i].GetType();

                if (type.IsSubclassOf(typeof(GameObject))) {
                  if (gameEvent.GetSceneContextType() != ((GameObject)eventListeners[gameEvent.GetType()][obj][i]).GetSceneContextType() ||
                      (((GameObject)eventListeners[gameEvent.GetType()][obj][i]).GetStatus() != GameObject.ObjectStatuses.ACTIVE && 
                       ((GameObject)eventListeners[gameEvent.GetType()][obj][i]).GetStatus() != GameObject.ObjectStatuses.IDLE)) {
                    continue;
                  }
                }

                if (type.IsSubclassOf(typeof(GameObjectComponent))) {
                  if (gameEvent.GetSceneContextType() != ((GameObjectComponent)eventListeners[gameEvent.GetType()][obj][i]).GetContext().GetSceneContextType() ||
                      (((GameObjectComponent)eventListeners[gameEvent.GetType()][obj][i]).GetContext().GetStatus() != GameObject.ObjectStatuses.ACTIVE &&
                       ((GameObjectComponent)eventListeners[gameEvent.GetType()][obj][i]).GetContext().GetStatus() != GameObject.ObjectStatuses.IDLE)) {
                    continue;
                  }
                }

                if (type.IsSubclassOf(typeof(GameScene))) {
                  if (gameEvent.GetSceneContextType() != ((GameScene)eventListeners[gameEvent.GetType()][obj][i]).GetType()) {
                    continue;
                  }
                }

                if (type == typeof(BehaviorTree)) {
                  if (gameEvent.GetSceneContextType() != ((BehaviorTree)eventListeners[gameEvent.GetType()][obj][i]).GetContext().GetSceneContextType() ||
                      (((BehaviorTree)eventListeners[gameEvent.GetType()][obj][i]).GetContext().GetStatus() != GameObject.ObjectStatuses.ACTIVE &&
                       ((BehaviorTree)eventListeners[gameEvent.GetType()][obj][i]).GetContext().GetStatus() != GameObject.ObjectStatuses.IDLE)) {
                    continue;
                  }
                }

                if (type.IsSubclassOf(typeof(BehaviorTreeNode))) {
                  if (((BehaviorTreeNode)eventListeners[gameEvent.GetType()][obj][i]).GetGameObject() != null &&
                      (gameEvent.GetSceneContextType() != ((BehaviorTreeNode)eventListeners[gameEvent.GetType()][obj][i]).GetGameObject().GetSceneContextType() ||
                       (((BehaviorTreeNode)eventListeners[gameEvent.GetType()][obj][i]).GetGameObject().GetStatus() != GameObject.ObjectStatuses.ACTIVE &&
                        ((BehaviorTreeNode)eventListeners[gameEvent.GetType()][obj][i]).GetGameObject().GetStatus() != GameObject.ObjectStatuses.IDLE))) {
                    continue;
                  }
                }
              }

              currentEvent = eventListeners[gameEvent.GetType()][obj][i].OnEvent(currentEvent);
              if (currentEvent == null)
                return;
            }
          }
        }
      }
    }

    //GameEvent.Create<SceneEvent>(SceneEvent.Values.SHOW, this);


  }
}
