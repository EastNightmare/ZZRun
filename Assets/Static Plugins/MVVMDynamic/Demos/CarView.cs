using UnityEngine;
using System.Collections.Generic;

namespace MVVMDynamic.Demos
{
    internal class CarView : MonoBehaviour
    {
        private ICarViewModel _carViewModel = TypeEmitter.Instance.CreateViewModel<ICarViewModel>();

        public ICarViewModel CarViewModel
        {
            get
            {
                return _carViewModel;
            }
        }

        [SerializeField] private AudioSource _sirenAudioSource;
        [SerializeField] private AudioSource _beepAudioSource;
        [SerializeField] private List<Light> _sirenLights;
        [SerializeField] private List<Light> _headLights;
        [SerializeField] private Transform _sirenLightsHandler;
        [SerializeField] private TextMesh _textMesh;
        [SerializeField] private AudioSource _textBeepAudioSource;

        private Binder _binder = new Binder();

        private void Start()
        {
            _binder.BindAction(_carViewModel, vm => vm.Beep(), vm => _beepAudioSource.Play());

            _binder.BindProperty(_carViewModel, vm => vm.SirenIsOn,
                vm => _sirenAudioSource.enabled = vm.SirenIsOn, true);

            _binder.BindProperty(_carViewModel, vm => vm.HeadlightsAreOn,
                vm => _headLights.ForEach(p => p.enabled = vm.HeadlightsAreOn), true);

            _carViewModel.PropertyChanged += _carViewModel_PropertyChanged;
            _textMesh.text = "";
        }

        private void _carViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowText")
            {
                _textBeepAudioSource.Play();
                _textMesh.text = e.Argument1.ToString();
            }
        }

        private void Unbind()
        {
            _binder.Reset(_carViewModel);
            //OR
            _binder.Reset();
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, Time.deltaTime*_carViewModel.TurnSpeed*100f);

            foreach (Light sirenLight in _sirenLights)
            {
                sirenLight.intensity =
                    Mathf.Lerp(sirenLight.intensity, _carViewModel.SirenIsOn ? 100 : 0, Time.deltaTime*5f);
            }

            if (_carViewModel.SirenIsOn)
            {
                _sirenLightsHandler.Rotate(Vector3.up, Time.deltaTime*200f);
            }
        }
    }
}