using System;
using System.Collections.Generic;
using System.Linq;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Managers;

namespace MountainMeadowEngine.Interfaces {
  
  public abstract class GameEvent {
    object value;
    protected object context;
    protected Type sceneContextType;
    protected static Dictionary<Type, Type> eventTypes = new Dictionary<Type, Type>();

    public Type GetSceneContextType() { 
      return sceneContextType; 
    }

    public object GetValue() {
      return value;
    }

    public object GetContext() {
      return context;
    }

    public static T Create<T>(object enumValue, object context, Type sceneContextType = null) {
      if (!eventTypes.ContainsKey(typeof(T))) {
        if (typeof(T).IsSubclassOf(typeof(GameEvent))) {
          if (enumValue.GetType().IsEnum) {
            if (typeof(T).FullName == enumValue.GetType().FullName.Split('+')[0]) {
              eventTypes.Add(typeof(T), enumValue.GetType());
            } else {
              throw new Exception("Provided Enum Type \"" + enumValue.GetType().Name + "\" does not belong to Event Type \"" + typeof(T).Name + "\".");
            }
          } else {
            throw new Exception("Invalid Enum Value \"" + enumValue.GetType().Name + "\" provided.");
          }
        } else {
          throw new Exception("Invalid Event Type \"" + typeof(T).Name + "\" provided.");
        }
      }

      if (typeof(T) == typeof(UpdateEvent)) {
        switch ((UpdateEvent.Values)enumValue) {
          case UpdateEvent.Values.RENDERING:
          case UpdateEvent.Values.OBJECT_RENDERING:
          case UpdateEvent.Values.PRE_RENDERING:
          case UpdateEvent.Values.POST_RENDERING:
            if (!(context is RenderManager)) {
              throw new Exception("\"" + context.GetType().Name + "\" is not allowed to push a Rendering Update event.");
            }
            break;
          default:
            if (!(context is PhysicsManager)) {
              throw new Exception("\"" + context.GetType().Name + "\" is not allowed to push a Physics Update event.");
            }
            break;
        }
      }

      if (eventTypes[typeof(T)] == enumValue.GetType()) {
        T instance = (T)Activator.CreateInstance(typeof(T));
        ((GameEvent)(object)instance).value = enumValue;
        ((GameEvent)(object)instance).sceneContextType = sceneContextType;
        return instance;
      } else {
        throw new Exception("Provided Enum Type \"" + enumValue.GetType().Name + "\" is not primary for Event Type \"" + typeof(T).Name + "\".");
      }
    }


  }
}
