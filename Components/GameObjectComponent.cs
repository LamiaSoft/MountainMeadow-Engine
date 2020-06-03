using System;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Components {
  
  public abstract class GameObjectComponent : IEventListener, IDisposable {
    protected GameObject context;

    public GameObjectComponent(GameObject context) {
      this.context = context;
    }

    public GameObject GetContext() {
      return context;
    }

    public abstract void Initialize();
    public abstract GameEvent OnEvent(GameEvent gameEvent);

    public virtual void Dispose() {
      EventManager.RemoveEventListener(this);
    }

  }
}
