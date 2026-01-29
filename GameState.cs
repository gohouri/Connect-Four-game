namespace ConnectFour;

/// <summary>
/// Manages the game state for Connect Four including board, turns, and win detection
/// </summary>
public class GameState
{
    static GameState()
    {
        CalculateWinningPlaces();
    }

    /// <summary>
    /// Indicate whether a player has won, the game is a tie, or game in ongoing
    /// </summary>
    public enum WinState
    {
        No_Winner = 0,
        Player1_Wins = 1,
        Player2_Wins = 2,
        Tie = 3
    }

    /// <summary>
    /// Represents a single move in the game
    /// </summary>
    public record Move(int MoveNumber, int Player, int Column, int Row);

    /// <summary>
    /// The player whose turn it is. By default, player 1 starts first
    /// </summary>
    public int PlayerTurn => TheBoard.Count(x => x != 0) % 2 + 1;

    /// <summary>
    /// Number of turns completed and pieces played so far in the game
    /// </summary>
    public int CurrentTurn { get { return TheBoard.Count(x => x != 0); } }

    /// <summary>
    /// List of all winning position combinations
    /// </summary>
    public static readonly List<int[]> WinningPlaces = new();

    /// <summary>
    /// History of moves made during the current game
    /// </summary>
    public List<Move> MoveHistory { get; private set; } = new();

    /// <summary>
    /// Consecutive wins for Player 1
    /// </summary>
    public int Player1ConsecutiveWins { get; private set; } = 0;

    /// <summary>
    /// Consecutive wins for Player 2
    /// </summary>
    public int Player2ConsecutiveWins { get; private set; } = 0;

    /// <summary>
    /// Records a win for a player and updates consecutive win streaks
    /// </summary>
    public void RecordWin(int player)
    {
        if (player == 1)
        {
            Player1ConsecutiveWins++;
            Player2ConsecutiveWins = 0;
        }
        else if (player == 2)
        {
            Player2ConsecutiveWins++;
            Player1ConsecutiveWins = 0;
        }
    }

    /// <summary>
    /// Resets consecutive wins for both players
    /// </summary>
    public void ResetConsecutiveWins()
    {
        Player1ConsecutiveWins = 0;
        Player2ConsecutiveWins = 0;
    }

    /// <summary>
    /// Pre-calculate all possible winning combinations
    /// </summary>
    public static void CalculateWinningPlaces()
    {
        // Horizontal rows
        for (byte row = 0; row < 6; row++)
        {
            byte rowCol1 = (byte)(row * 7);
            byte rowColEnd = (byte)((row + 1) * 7 - 1);
            byte checkCol = rowCol1;
            while (checkCol <= rowColEnd - 3)
            {
                WinningPlaces.Add(new int[] {
                    checkCol,
                    (byte)(checkCol + 1),
                    (byte)(checkCol + 2),
                    (byte)(checkCol + 3)
                });
                checkCol++;
            }
        }

        // Vertical Columns
        for (byte col = 0; col < 7; col++)
        {
            byte colRow1 = col;
            byte colRowEnd = (byte)(35 + col);
            byte checkRow = colRow1;
            while (checkRow <= 14 + col)
            {
                WinningPlaces.Add(new int[] {
                    checkRow,
                    (byte)(checkRow + 7),
                    (byte)(checkRow + 14),
                    (byte)(checkRow + 21)
                });
                checkRow += 7;
            }
        }

        // Forward slash diagonal "/"
        for (byte col = 0; col < 4; col++)
        {
            byte colRow1 = (byte)(21 + col);
            byte colRowEnd = (byte)(35 + col);
            byte checkPos = colRow1;
            while (checkPos <= colRowEnd)
            {
                WinningPlaces.Add(new int[] {
                    checkPos,
                    (byte)(checkPos - 6),
                    (byte)(checkPos - 12),
                    (byte)(checkPos - 18)
                });
                checkPos += 7;
            }
        }

        // Back slash diagonal "\"
        for (byte col = 0; col < 4; col++)
        {
            byte colRow1 = (byte)(0 + col);
            byte colRowEnd = (byte)(14 + col);
            byte checkPos = colRow1;
            while (checkPos <= colRowEnd)
            {
                WinningPlaces.Add(new int[] {
                    checkPos,
                    (byte)(checkPos + 8),
                    (byte)(checkPos + 16),
                    (byte)(checkPos + 24)
                });
                checkPos += 7;
            }
        }
    }

    /// <summary>
    /// Check the state of the board for a winning scenario
    /// </summary>
    /// <returns>The current win state</returns>
    public WinState CheckForWin()
    {
        // Exit immediately if less than 7 pieces are played
        if (TheBoard.Count(x => x != 0) < 7) return WinState.No_Winner;

        foreach (var scenario in WinningPlaces)
        {
            if (TheBoard[scenario[0]] == 0) continue;

            if (TheBoard[scenario[0]] ==
                TheBoard[scenario[1]] &&
                TheBoard[scenario[1]] ==
                TheBoard[scenario[2]] &&
                TheBoard[scenario[2]] ==
                TheBoard[scenario[3]]) return (WinState)TheBoard[scenario[0]];
        }

        if (TheBoard.Count(x => x != 0) == 42) return WinState.Tie;

        return WinState.No_Winner;
    }

    /// <summary>
    /// Takes the current turn and places a piece in the 0-indexed column requested
    /// </summary>
    /// <param name="column">0-indexed column to place the piece into</param>
    /// <returns>The final row where the piece lands (1-indexed)</returns>
    public byte PlayPiece(int column)
    {
        // Check for a current win
        if (CheckForWin() != 0) throw new ArgumentException("Game is over");

        // Check the column
        if (TheBoard[column] != 0) throw new ArgumentException("Column is full");

        // Drop the piece in
        var landingSpot = column;
        for (var i = column; i < 42; i += 7)
        {
            if (landingSpot + 7 < 42 && TheBoard[landingSpot + 7] != 0) break;
            landingSpot = i;
        }

        var currentPlayer = PlayerTurn;
        TheBoard[landingSpot] = currentPlayer;

        var row = ConvertLandingSpotToRow(landingSpot);

        // Record the move in history
        MoveHistory.Add(new Move(
            MoveNumber: CurrentTurn,
            Player: currentPlayer,
            Column: column + 1,  // 1-indexed for display
            Row: row
        ));

        return row;
    }

    /// <summary>
    /// The game board represented as a list of 42 positions
    /// </summary>
    public List<int> TheBoard { get; private set; } = new List<int>(new int[42]);

    /// <summary>
    /// Reset the board to start a new game
    /// </summary>
    public void ResetBoard()
    {
        TheBoard = new List<int>(new int[42]);
        MoveHistory = new List<Move>();
    }

    private byte ConvertLandingSpotToRow(int landingSpot)
    {
        return (byte)(Math.Floor(landingSpot / (decimal)7) + 1);
    }
}
