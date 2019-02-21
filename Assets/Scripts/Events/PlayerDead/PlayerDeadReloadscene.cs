using UnityEngine;
using PseudoTools;
using UnityEngine.SceneManagement;
[ReceiveEvent("PlayerDead")]
public class PlayerDeadReloadscene : ObserverMonoBehaviour {
    public void ReceivePlayerDead() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}