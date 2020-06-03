using MountainMeadowEngine.Interfaces;
//using static MountainMeadowEngine.Collision.Rectangle3D;

namespace MountainMeadowEngine.Components
{

    public class Liftable : GameObjectComponent {

    int weight;
    GameObject liftedBy;

    public Liftable(GameObject context) : base(context) { }


    public void SetWeight(int weight) {
      this.weight = weight;
    }

    public int GetWeight() {
      return weight;
    }

    public void SetLiftedBy(GameObject gameObject) {
      this.liftedBy = gameObject;
    }

    public GameObject GetLiftedBy() {
      return this.liftedBy;
    }

    public override void Initialize() { }

    public override GameEvent OnEvent(GameEvent gameEvent) {
      return gameEvent;
    }
  }
}
