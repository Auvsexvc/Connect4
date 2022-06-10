namespace Connect4WPF
{
    public class Disc
    {
        public Player Owner { get; }
        public int X { get; }
        public int Y { get; }

        public Disc(int x, int y, Player owningPlayer)
        {
            Owner = owningPlayer;
            X = x;
            Y = y;
        }
    }
}