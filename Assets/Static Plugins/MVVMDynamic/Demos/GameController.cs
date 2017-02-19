
namespace MVVMDynamic.Demos
{
    class GameController
    {
        static public GameController Instance { get; private set; }
        static GameController()
        {
            Instance = new GameController();
        }

        private string[] _phrases =
        {
            "Stop the car\n in the name of law!",
            "Donuts time!",
            "Roger",
            "Suspicious activity!",
            "One-11-one-10",
            "On the way!"
        };

        private int _lastIndex;

        public string GetRandomText()
        {
            int newIndex = _lastIndex;
            while (newIndex == _lastIndex)
            {
                newIndex = UnityEngine.Random.Range(0, _phrases.Length);
            }

            _lastIndex = newIndex;
            return _phrases[newIndex];
        }
        private GameController(){}
    }
}
