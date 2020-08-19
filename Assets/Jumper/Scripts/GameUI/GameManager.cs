using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jumper.Rework.Scriptable
{
    public class GameManager : MonoBehaviour
    {

        public void LoadScene()
        {
            SceneManager.LoadScene(0);
        }
    }
}




