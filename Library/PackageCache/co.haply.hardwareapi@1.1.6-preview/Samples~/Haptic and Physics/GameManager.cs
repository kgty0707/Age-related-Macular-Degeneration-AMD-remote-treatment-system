using Haply.HardwareAPI.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Haply.HapticsAndPhysicsEngine
{
    public class GameManager : MonoBehaviour
    {
        [Range(1, 10000)]
        [Tooltip("Adjust Fixed Timestep directly from here to compare with Haptics Thread frequency")]
        public int physicsFrequency = 1000;

        public HapticThread hapticThread;
        public SimplePhysicsHapticEffector simpleEffector;
        public AdvancedPhysicsHapticEffector advancedEffector;

        public Material enabledForceMaterial;
        public Material disabledForceMaterial;

        [Header("UI")]
        public Text helpText;
        public GameObject frequenciesPanel;
        public Text physicsFrequencyText;
        public Text hapticsFrequencyText;
        public string chooseModeMessage = "Press 1 to Simple Mode, 2 to Advanced";
        public string enableForceMessage= "Move the sphere at center not touching anything and hit SPACE to enable force";
        public string collisionMessage = "Press C to enable/disable collision detection";
        
        void Start()
        {
            if ( !simpleEffector.gameObject.activeSelf && !advancedEffector.gameObject.activeSelf )
            {
                helpText.text = chooseModeMessage;
            }
            else
            {
                helpText.text = enableForceMessage;
            }
            frequenciesPanel.SetActive( false );
        }

        // Update is called once per frame
        void Update()
        {
            // adjust Fixed Timestep from inspector to compare and understand for demo
            // don't do that in real case, prefer change from ProjectSettings>Time panel
            Time.fixedDeltaTime = 1f / physicsFrequency;

            if (hapticThread.isInitialized)
                hapticsFrequencyText.text = $"haptics : {hapticThread.actualFrequency}Hz";
            physicsFrequencyText.text = $"physics : {physicsFrequency}Hz";

            if ( Input.GetKeyDown( KeyCode.Escape ) )
            {
    #if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
    #else
                Application.Quit();
    #endif
            }
            else if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
            {
                if ( !simpleEffector.gameObject.activeSelf && !advancedEffector.gameObject.activeSelf )
                {
                    simpleEffector.gameObject.SetActive( true );
                    helpText.text = enableForceMessage;
                    frequenciesPanel.SetActive( true );
                }
            } 
            else if (Input.GetKeyDown( KeyCode.Alpha2 ))
            {
                if ( !simpleEffector.gameObject.activeSelf && !advancedEffector.gameObject.activeSelf )
                {
                    advancedEffector.gameObject.SetActive( true );
                    helpText.text = enableForceMessage;
                    frequenciesPanel.SetActive( true );
                }
            }
            else if ( Input.GetKeyDown( KeyCode.Space ) )
            {
                ToggleForceFeedback();
            }
            else if ( Input.GetKeyDown( KeyCode.C ) && advancedEffector.gameObject.activeSelf)
            {
                advancedEffector.collisionDetection = !advancedEffector.collisionDetection;
            }
            else if ( Input.GetKeyDown( KeyCode.UpArrow ) && frequenciesPanel.activeSelf && physicsFrequency < 10000)
            {
                physicsFrequency += 50;
            }
            else if ( Input.GetKeyDown( KeyCode.DownArrow ) && frequenciesPanel.activeSelf && physicsFrequency > 200)
            {
                physicsFrequency -= 100;
            }
            else if ( Input.GetKeyDown( KeyCode.DownArrow ) && frequenciesPanel.activeSelf && physicsFrequency > 0)
            {
                physicsFrequency /= 2;
            }
            else if ( Input.GetKeyDown( KeyCode.RightArrow ) && frequenciesPanel.activeSelf && hapticThread.targetFrequency < 10000)
            {
                hapticThread.targetFrequency += 50;
            }
            else if ( Input.GetKeyDown( KeyCode.LeftArrow ) && frequenciesPanel.activeSelf && hapticThread.targetFrequency > 200)
            {
                hapticThread.targetFrequency -= 100;
            }
            else if ( Input.GetKeyDown( KeyCode.LeftArrow ) && frequenciesPanel.activeSelf && hapticThread.targetFrequency > 0)
            {
                hapticThread.targetFrequency /= 2;
            }
        }
        
        // Display collision infos
        // ----------------------------------
        private void OnGUI()
        {
            if (advancedEffector.gameObject.activeSelf && advancedEffector.touched.Count > 0)
            {
                // display touched physic material infos on screen
                var physicMaterial = advancedEffector.touched[0].GetComponent<Collider>().material;
                var text = $"PhysicsMaterial: {physicMaterial.name.Replace( "(Instance)", "" )} \n" +
                           $"dynamic friction: {physicMaterial.dynamicFriction}, static friction: {physicMaterial.staticFriction}\n";
                
                // display touched rigidbody infos on screen
                var rb = advancedEffector.touched[0].GetComponent<Rigidbody>();
                if ( rb )
                {
                    text += $"mass: {rb.mass}, drag: {rb.drag}, angular drag: {rb.angularDrag}\n";
                }

                GUI.Label(new Rect(20, 40, 800f, 200f), text);
            }
        }

        public void ToggleForceFeedback()
        {
            if ( simpleEffector.gameObject.activeSelf )
            {
                simpleEffector.forceEnabled = !simpleEffector.forceEnabled;
                simpleEffector.gameObject.GetComponent<MeshRenderer>().enabled = simpleEffector.forceEnabled;
                    
                hapticThread.avatar.gameObject.GetComponent<MeshRenderer>().material =
                    simpleEffector.forceEnabled ? enabledForceMaterial : disabledForceMaterial;
                    
                helpText.text = simpleEffector.forceEnabled ? "" : enableForceMessage;
            }
            else if ( advancedEffector.gameObject.activeSelf )
            {
                advancedEffector.forceEnabled = !advancedEffector.forceEnabled;
                advancedEffector.gameObject.GetComponent<MeshRenderer>().enabled = advancedEffector.forceEnabled;
                    
                hapticThread.avatar.gameObject.GetComponent<MeshRenderer>().material =
                    advancedEffector.forceEnabled ? enabledForceMaterial : disabledForceMaterial;
                    
                helpText.text = advancedEffector.forceEnabled ? collisionMessage : enableForceMessage;
            }
        }
    }
}
