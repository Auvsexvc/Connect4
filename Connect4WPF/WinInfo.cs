namespace Connect4WPF
{
    public class WinInfo
    {
        public WinType Type { get; set; }
        public int NumberOfTurnsToWin { get; set; }
        public (int, int) StartCoordsOfWinningLine { get; set; }
        public (int, int) EndCoordsOfWinningLine { get; set; }
    }
}