using UnityEngine;
using PseudoTools;
namespace PseudoTools {
    public abstract class ObserverMonoBehaviour : MonoBehaviour {
       public void OnEnable() {
           EventBus.Register(this);
       }
       public void OnDisable() {
           EventBus.Deregister(this);
       }
    }
}