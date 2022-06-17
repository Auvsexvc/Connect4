using System;
using System.Collections.Generic;
using System.Linq;

namespace Connect4WPF
{
    public class GameState
    {
        private readonly Player player1, player2;

        public readonly List<Disc> discs;

        public Player[] Players { get; }
        public Player CurrentPlayer { get; private set; }
        public int TurnsPassed { get; private set; }
        public bool GameOver { get; private set; }

        public event Action<int>? MoveMade;

        public event Action? ColumnFull;

        public event Action<GameResult>? GameEnded;

        public event Action? GameRestarted;

        public event Action? SwitchedPlayer;

        public GameState()
        {
            GameOver = false;
            TurnsPassed = 1;
            player1 = new Player("Green");
            player2 = new Player("Red");
            Players = new[] { player1, player2 };
            CurrentPlayer = player1;
            discs = new List<Disc>();
        }

        public void MakeMove(int col)
        {
            if (!CanMakeMove())
            {
                return;
            }

            if (IsMoveValid(col))
            {
                Disc disc = new(col, discs.Where(d => d.X == col).Select(d => d.Y).Any() ? discs.Where(d => d.X == col).Max(d => d.Y) + 1 : 0, CurrentPlayer);
                discs.Add(disc);

                MoveMade?.Invoke(col);
                if (DidMoveEndGame(out GameResult gameResult))
                {
                    GameOver = true;
                    GameEnded?.Invoke(gameResult);
                }
                if (!GameOver)
                {
                    SwitchPlayer();
                    EndTurn();
                }
            }
            else
            {
                ColumnFull?.Invoke();
            }
        }

        public void EndTurn()
        {
            if (CurrentPlayer == player1)
            {
                TurnsPassed++;
            }
        }

        public void Reset()
        {
            CurrentPlayer = player1;
            TurnsPassed = 1;
            GameOver = false;
            discs.Clear();
            GameRestarted?.Invoke();
        }

        private bool CanMakeMove()
        {
            return !GameOver;
        }

        private bool IsMoveValid(int col)
        {
            return !discs.Where(d => d.X == col).Select(d => d.Y).Any() || discs.Where(d => d.X == col).Max(d => d.Y) < 5;
        }

        private bool DidMoveEndGame(out GameResult gameResult)
        {
            if (DidMoveWin(out WinInfo winInfo))
            {
                gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }

            if (IsGridFull())
            {
                gameResult = new GameResult { Winner = null };
                return true;
            }

            gameResult = null!;
            return false;
        }

        private void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == player1 ? player2 : player1;
            SwitchedPlayer?.Invoke();
        }

        private bool DidMoveWin(out WinInfo winInfo)
        {
            if (IsHorizontalWin(out List<(int, int)> tmp))
            {
                winInfo = new WinInfo { Type = WinType.Horizontal, NumberOfTurnsToWin = TurnsPassed, StartCoordsOfWinningLine = tmp.First(), EndCoordsOfWinningLine = tmp.Last() };
                return true;
            }
            else if (IsVerticalWin(out tmp))
            {
                winInfo = new WinInfo { Type = WinType.Vertical, NumberOfTurnsToWin = TurnsPassed, StartCoordsOfWinningLine = tmp.First(), EndCoordsOfWinningLine = tmp.Last() };
                return true;
            }
            else if (IsDiagonalAscendingWin(out tmp))
            {
                winInfo = new WinInfo { Type = WinType.Diagonal, NumberOfTurnsToWin = TurnsPassed, StartCoordsOfWinningLine = tmp.First(), EndCoordsOfWinningLine = tmp.Last() };
                return true;
            }
            else if (IsDiagonalDescendingWin(out tmp))
            {
                winInfo = new WinInfo { Type = WinType.AntiDiagonal, NumberOfTurnsToWin = TurnsPassed, StartCoordsOfWinningLine = tmp.First(), EndCoordsOfWinningLine = tmp.Last() };
                return true;
            }

            winInfo = null!;
            return false;
        }

        private bool IsGridFull()
        {
            return discs.Count == 42;
        }

        private bool IsVerticalWin(out List<(int, int)> tmp)
        {
            int counter = 0;
            tmp = new List<(int, int)>();
            List<Disc> vertical = discs
                .Where(d => d.X == discs.Last().X && d.Owner.Name.Contains(CurrentPlayer.Name))
                .OrderBy(d => d.Y)
                .ToList();

            for (int i = 1; i < vertical.Count; i++)
            {
                if (vertical[i].Y - vertical[i - 1].Y == 1)
                {
                    tmp.Add((vertical[i - 1].X, vertical[i - 1].Y));
                    counter++;
                }
                else
                {
                    counter = 0;
                }
            }
            if (counter >= 3)
            {
                tmp.Add((vertical.Last().X, vertical.Last().Y));
                return true;
            }

            return false;
        }

        private bool IsHorizontalWin(out List<(int, int)> tmp)
        {
            int counter = 0;
            tmp = new List<(int, int)>();
            List<Disc> horizontal = discs
                .Where(d => d.Y == discs.Last().Y && d.Owner.Name.Contains(CurrentPlayer.Name))
                .OrderBy(d => d.X)
                .ToList();

            for (int i = 1; i < horizontal.Count; i++)
            {
                if (horizontal[i].X - horizontal[i - 1].X == 1)
                {
                    tmp.Add((horizontal[i - 1].X, horizontal[i - 1].Y));
                    counter++;
                }
                else
                {
                    counter = 0;
                }
            }
            if (counter >= 3)
            {
                tmp.Add((horizontal.Last().X, horizontal.Last().Y));
                return true;
            }

            return false;
        }

        private bool IsDiagonalAscendingWin(out List<(int, int)> tmp)
        {
            int counter = 0;
            tmp = new List<(int, int)>();
            List<Disc> diagonalAsc = discs
                .Where(d => (discs.Last().X - d.X) == (discs.Last().Y - d.Y) && d.Owner.Name.Contains(CurrentPlayer.Name))
                .OrderBy(d => d.X)
                .ThenBy(d => d.Y)
                .ToList();

            for (int i = 1; i < diagonalAsc.Count; i++)
            {
                if (diagonalAsc[i].X - diagonalAsc[i - 1].X == 1 && diagonalAsc[i].Y - diagonalAsc[i - 1].Y == 1)
                {
                    tmp.Add((diagonalAsc[i - 1].X, diagonalAsc[i - 1].Y));
                    counter++;
                }
                else
                {
                    counter = 0;
                }
            }
            if (counter >= 3)
            {
                tmp.Add((diagonalAsc.Last().X, diagonalAsc.Last().Y));
                return true;
            }

            return false;
        }

        private bool IsDiagonalDescendingWin(out List<(int, int)> tmp)
        {
            int counter = 0;
            tmp = new List<(int, int)>();
            List<Disc> diagonalDesc = discs
                .Where(d => (discs.Last().X - d.X) == -(discs.Last().Y - d.Y) && d.Owner.Name.Contains(CurrentPlayer.Name))
                .OrderBy(d => d.X)
                .ThenByDescending(d => d.Y)
                .ToList();

            for (int i = 1; i < diagonalDesc.Count; i++)
            {
                if (diagonalDesc[i].X - diagonalDesc[i - 1].X == 1 && -(diagonalDesc[i].Y - diagonalDesc[i - 1].Y) == 1)
                {
                    tmp.Add((diagonalDesc[i - 1].X, diagonalDesc[i - 1].Y));
                    counter++;
                }
                else
                {
                    counter = 0;
                }
            }
            if (counter >= 3)
            {
                tmp.Add((diagonalDesc.Last().X, diagonalDesc.Last().Y));
                return true;
            }

            return false;
        }
    }
}