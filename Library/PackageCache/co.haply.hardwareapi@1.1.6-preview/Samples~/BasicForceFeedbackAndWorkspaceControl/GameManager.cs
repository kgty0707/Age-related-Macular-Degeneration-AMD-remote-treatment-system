using UnityEditor;
using UnityEngine;

namespace Haply.Samples.BasicForceFeedbackAndWorkspaceControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private WorkspaceOffsetController offsetController;
        [SerializeField]
        private WorkspaceScaleController scaleController;

        [Tooltip("Visual game object used to display cursor movement scope (optional)")]
        [SerializeField]
        private GameObject bounds;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) offsetController.active = true;
            if (Input.GetKeyUp(KeyCode.M)) offsetController.active = false;
            
            if (Input.GetKeyDown(KeyCode.S)) scaleController.active = true;
            if (Input.GetKeyUp(KeyCode.S)) scaleController.active = false;

            bounds.SetActive(offsetController.active || scaleController.active);
            
            if ( Input.GetKeyDown( KeyCode.Escape ) )
            {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
                Application.Quit();
#endif
            }
        }

        private void OnGUI()
        {
            var text = "";
            if (!offsetController.active && !scaleController.active)
            {
                text = "Press HANDLE BUTTON to calibrate workspace\n" + 
                       "(press M to MOVE only, S to SCALE only)";
            }
            if (offsetController.active)
            {
                text += $"Move the cursor to move the workspace : {offsetController.transform.position}\n";
            }
            if (scaleController.active)
            {
                text += $"Rotate the handle to scale up/down the workspace : ({scaleController.transform.localScale.x:0.000})\n";
            }

            GUI.Label(new Rect(20, 40, 800f, 200f), text);
        }
    }
}