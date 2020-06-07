using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using MountainMeadowEngine.Events;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Components {

  public class PointerPattern : GameObjectComponent {
    bool inputMouseClick = true;
    bool inputTouch = true;
    bool repeatAfterReleased = true;
    int touchIndex = 0;

    float holdMinDuration = -1;
    float holdMaxDuration = -1;
    Type holdEventType;

    float moveMinXDistance = -1;
    float moveMaxXDistance = -1;
    float moveMinYDistance = -1;
    float moveMaxYDistance = -1;
    float moveMinDuration = -1;
    float moveMaxDuration = -1;
    Type moveEventType;

    Func<Vector2> movementMinCallback;
    Func<Vector2> movementMaxCallback;

    public PointerPattern(GameObject context) : base(context) {
      EventManager.AddEventListener<InputEvent>(this);
    }

    public PointerPattern SetTouchIndex(int index) {
      touchIndex = index;
      inputTouch = true;
      return this;
    }

    public PointerPattern EnableInputType(bool mouseClick, bool touch) {
      inputMouseClick = mouseClick;
      inputTouch = touch;
      return this;
    }

    public PointerPattern SetHoldRequirements<T>(float minDuration, float maxDuration = -1) {
      holdMinDuration = minDuration;
      holdMaxDuration = maxDuration;
      holdEventType = typeof(T);
      return this;
    }

    public PointerPattern SetMovementRequirements<T>(float minXDistance, float maxXDistance, float minYDistance,
                                                     float maxYDistance, float minDuration, float maxDuration) {
      moveMinXDistance = minXDistance;
      moveMaxXDistance = maxXDistance;
      moveMinYDistance = minYDistance;
      moveMaxYDistance = maxYDistance;
      moveMinDuration = minDuration;
      moveMaxDuration = maxDuration;
      moveEventType = typeof(T);
      return this;
    }

    public PointerPattern SetMovementMinCallback<T>(Func<Vector2> callback) {
      movementMinCallback = callback;
      moveEventType = typeof(T);
      return this;
    }

    public PointerPattern SetMovementMaxCallback<T>(Func<Vector2> callback) {
      movementMinCallback = callback;
      moveEventType = typeof(T);
      return this;
    }

    public PointerPattern SetRepeatAfterEvent() {
      repeatAfterReleased = false;
      return this;
    }

    public PointerPattern SetRepeatAfterReleased() {
      repeatAfterReleased = true;
      return this;
    }

    public override void Initialize() {
      throw new NotImplementedException();
    }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      throw new NotImplementedException();
    }
  }
}
