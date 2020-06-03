using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {

  public class UpdateEvent : GameEvent {

    public enum Values {
      PRE_PHYSICS, OBJECT_MOVEMENT, OBJECT_PHYSICS, PHYSICS, POST_PHYSICS, POST_COLLISION, PRE_RENDERING, OBJECT_RENDERING, RENDERING, POST_RENDERING   
    };

    double timeSinceLastUpdate;
    GameTime gameTime;
    ObjectManager objectManager;

    public UpdateEvent SetTimeSinceLastUpdate(double time) { this.timeSinceLastUpdate = time; return this; }
    public UpdateEvent SetGameTime(GameTime time) { this.gameTime = time; return this; }
    public UpdateEvent SetObjectManager(ObjectManager objectManager) { this.objectManager = objectManager; return this; }
    public double GetTimeSinceLastUpdate() { return timeSinceLastUpdate; }
    public GameTime GetGameTime() { return gameTime; }
    public ObjectManager GetObjectManager() { return objectManager; }

  }

}
