using System;
using System.Collections;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        PriorityQueue<Move, int> mvpq = new PriorityQueue<Move, int>();
        foreach (Move mv in moves)
        {
            int val = int.MaxValue;
                if (isCheckmate(board, mv))
                {
                    return mv;
                }
                if (mv.IsCapture)
                {
                    val = 100 - (int) mv.CapturePieceType;
                    
                }
                
                
                mvpq.Enqueue(mv, val);
            }

       
            return mvpq.Dequeue();
    }

    public bool isCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isCMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isCMate;
    }
}