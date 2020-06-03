using System;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {

  public class AnimationEvent : GameEvent {

    public enum Values {
      STARTED, PAUSED, UNPAUSED, STOPPED, FRAME_CHANGE
    };

    int currentFrame = -1;
    Type animationType;

    public AnimationEvent SetCurrentFrame(int frame) { currentFrame = frame; return this; }
    public AnimationEvent SetAnimationType(Type type) { animationType = type; return this; }

    public int GetCurrentFrame() { return currentFrame; }
    public Type GetAnimationType() { return animationType; }
  }



}
