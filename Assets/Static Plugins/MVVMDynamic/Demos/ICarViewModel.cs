
namespace MVVMDynamic.Demos
{
    internal interface ICarViewModel : IViewModel
    {
        bool SirenIsOn { get; set; }
        bool HeadlightsAreOn { get; set; }
        void Beep();
        float TurnSpeed { get; set; }
        void ShowText(string text);
    }
}