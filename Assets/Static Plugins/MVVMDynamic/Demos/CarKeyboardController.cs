using UnityEngine;

namespace MVVMDynamic.Demos
{
    internal class CarKeyboardController : MonoBehaviour
    {
        private ICarViewModel _carViewModel;

        public void Start()
        {
            _carViewModel = FindObjectOfType<CarView>().CarViewModel;
        }

        public void Update()
        {
            _carViewModel.TurnSpeed = Input.GetAxis("Horizontal");

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _carViewModel.SirenIsOn = !_carViewModel.SirenIsOn;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _carViewModel.HeadlightsAreOn = !_carViewModel.HeadlightsAreOn;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _carViewModel.Beep();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                _carViewModel.ShowText(GameController.Instance.GetRandomText());
            }
        }

        public void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 300, 100),
                @"A,D - turn
Space - short siren
Left Shift - siren
F - new text
Left Control - headlights" + UnityEngine.Random.value);
        }
    }
}