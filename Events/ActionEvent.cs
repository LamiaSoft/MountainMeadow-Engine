namespace MountainMeadowEngine.Interfaces {

  public class ActionEvent : GameEvent {

    public enum Values { STAND, MOVE, JUMP, FALL, DIE };

    private float? moveParamX, moveParamY, moveParamZ;


    public ActionEvent SetMoveParams(float? x, float? y, float? z) { moveParamX = x; moveParamY = y; moveParamZ = z; return this; }
    public void GetMoveParams(out float? x, out float? y, out float? z) { x = moveParamX; y = moveParamY; z = moveParamZ; }

  }
}
