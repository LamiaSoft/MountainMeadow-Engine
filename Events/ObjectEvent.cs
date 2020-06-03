using System;
using System.Collections.Generic;
using MountainMeadowEngine.Interfaces;
using static MountainMeadowEngine.Components.GameObject;

namespace MountainMeadowEngine.Events {
  
  public class ObjectEvent : GameEvent {
    int gameObjectId;
    ObjectStatuses status;

    public enum Values { STATUS_CHANGE };

    public ObjectEvent SetGameObjectId(int id) { this.gameObjectId = id; return this; }
    public ObjectEvent SetStatus(ObjectStatuses status) { this.status = status; return this; }
    public int GetGameObjectId() { return gameObjectId; }
    public ObjectStatuses GetStatus() { return status; }
  }
}
