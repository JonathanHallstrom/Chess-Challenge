using System;
using System.Collections;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        int[] pieceValues = { 0, 100, 305, 333, 563, 950, 10000 };
        Move[] moves = board.GetLegalMoves();
        PriorityQueue<Move, int> mvpq = new PriorityQueue<Move, int>();
        foreach (Move mv in moves)
        {
            int val = 10000;
                if (isCheckmate(board, mv))
                {
                    return mv;
                }
                if (mv.IsCapture)
                {
                    Piece capturedPiece = board.GetPiece(mv.TargetSquare);
                    int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
                    val = val - capturedPieceValue + pieceValues[(int)mv.CapturePieceType];
                }

                if (mv.IsPromotion)
                {   
                    board.MakeMove(mv);
                    val -= pieceValues[(int)board.GetPiece(mv.TargetSquare).PieceType];
                    // pieceValues[(int)mv.PromotionPieceType];
                    
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