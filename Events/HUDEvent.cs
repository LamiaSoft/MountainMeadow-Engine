using System;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {
  
  public class HUDEvent : GameEvent {

    public enum Values {
      PRESSED, RELEASED, DRAGGED
    };
       
    Vector2 coordinates;

    public HUDEvent SetCoordinates(Vector2 coordinates) { this.coordinates = coordinates; return this; }
    public Vector2 GetCoordinates() { return coordinates; }
   
  }
}
