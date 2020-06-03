using System;
namespace MountainMeadowEngine.Interfaces {
  
  public interface IEventListener {

    GameEvent OnEvent(GameEvent gameEvent);
    
  }
}
