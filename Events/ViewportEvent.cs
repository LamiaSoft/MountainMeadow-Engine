using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {

  public class ViewportEvent : GameEvent {

    int camIndex = 0;
    Vector2 camPosition = new Vector2(0, 0);

    public enum Values { CHANGED, CAMERA_MOVED };

    public ViewportEvent SetCameraIndex(int camIndex) { this.camIndex = camIndex; return this; }
    public ViewportEvent SetCameraPosition(Vector2 camPos) { camPosition = camPos; return this; }
    public int GetCameraIndex() { return camIndex; }
    public Vector2 GetCameraPosition() { return camPosition; }

  }
}
