using System;
using System.Collections;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        ArrayList moveorder = new ArrayList();
        foreach (Move mv in moves)
            {
                if (mv.IsCapture || mv.IsPromotion)
                {
                    moveorder.Add(mv);
                }
            }

        if (moveorder.Count > 0)
        {
            return (Move) moveorder[0];
        } 
            return moves[0];
    }
}