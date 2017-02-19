using UnityEngine;

namespace MVVMDynamic.Demos
{
    internal class CarTouchController : MonoBehaviour
    {
        private ICarViewModel _carViewModel;
        private CarKeyboardController _keyboardController;
        private bool _touchEnabled;

        public void Start()
        {
            _carViewModel = FindObjectOfType<CarView>().CarViewModel;
            _keyboardController = GetComponent<CarKeyboardController>();
        }

        public void OnGUI()
        {
            int height = 150;

            GUILayout.BeginArea(new Rect(0, Screen.height - height, 300, height));

            _touchEnabled = GUILayout.Toggle(_touchEnabled, "enable screen controls"); //Avoiding conflicts
            _keyboardController.enabled = !_touchEnabled;

            GUILayout.BeginHorizontal();
            GUI.enabled = _touchEnabled;

            float turnSpeed = 0f;
            if (GUILayout.RepeatButton("turn left"))
            {
                turnSpeed = -1f;
            }

            if (GUILayout.RepeatButton("turn right"))
            {
                turnSpeed = 1f;
            }

            if (_touchEnabled)
            {
                _carViewModel.TurnSpeed = turnSpeed;
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            

            if (GUILayout.Button("short siren"))
            {
                _carViewModel.Beep();
            }

            if (GUILayout.Button("new text"))
            {
                _carViewModel.ShowText(GameController.Instance.GetRandomText());
            }
            _carViewModel.HeadlightsAreOn = GUILayout.Toggle(_carViewModel.HeadlightsAreOn, "headlights");
            _carViewModel.SirenIsOn = GUILayout.Toggle(_carViewModel.SirenIsOn, "siren");
            GUILayout.EndArea();
        }
    }
}