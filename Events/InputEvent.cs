using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MountainMeadowEngine.Components;
using MountainMeadowEngine.Interfaces;

namespace MountainMeadowEngine.Events {

  public class InputEvent : GameEvent {

    public enum Values {
      KEY_DOWN, KEY_UP, TOUCH_DOWN, TOUCH_UP, TOUCH_MOVED, MOUSE_LEFT_UP, MOUSE_LEFT_DOWN, MOUSE_DRAGGED, OBJECT_TOUCHED,
      MOUSE_MIDDLE_DOWN, MOUSE_MIDDLE_UP, MOUSE_RIGHT_DOWN, MOUSE_RIGHT_UP, MOUSE_SCROLL, MOUSE_HORIZONTAL_SCROLL 
    };


    Keys key;
    int scrolledValue;
    Vector2 coordinates;
    Vector2 camCoordinates;
    GameObject touchedObject;

    public InputEvent SetKey(Keys key) { this.key = key; return this; }
    public InputEvent SetCoordinates(Vector2 coordinates) { this.coordinates = coordinates; return this; }
    public InputEvent SetCamCoordinates(Vector2 camCoordinates) { this.camCoordinates = camCoordinates; return this; }
    public InputEvent SetScrolledValue(int value) { this.scrolledValue = value; return this; }
    public InputEvent SetTouchedObject(GameObject gameObject) { this.touchedObject = gameObject; return this; }
    public Keys GetKey() { return key; }
    public Vector2 GetCoordinates() { return coordinates; }
    public Vector2 GetCamCoordinates() { return camCoordinates; }
    public int GetScrolledValue() { return scrolledValue; }
    public GameObject GetGameObject() { return touchedObject; }
  }

}
