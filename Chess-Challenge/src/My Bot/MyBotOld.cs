using System;
using System.Collections;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBotOld : IChessBot
{
    int[] pieceValues = { 0, 100, 305, 333, 563, 950, 10000 };

    int value(Piece p)
    {
        return pieceValues[(int)p.PieceType];
    }

    int value(PieceType p)
    {
        return pieceValues[(int)p];
    }

    int eval(Board board, Move move)
    {
        if (isCheckmate(board, move))
            return int.MaxValue;

        int res = 0;
        // if (isCheck(board, move))
        //     res += 50;

        int myPieceValue = value(move.MovePieceType);
        
        Piece targetedPiece = board.GetPiece(move.TargetSquare);
        if (move.IsCapture)
             // value of opponents piece  value of our piece (to prefer capturing with less valuable pieces
            res += value(targetedPiece) - myPieceValue / 4;

        if (move.IsPromotion)
            //     should be a better piece         pawn
            res += value(move.PromotionPieceType) - myPieceValue;

        // prefer to move pawns
        res += move.MovePieceType == PieceType.Pawn ? 1 : 0;
        return res;
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        // PriorityQueue<Move, int> mvpq = new PriorityQueue<Move, int>();
        Move chosenMove = moves[0];
        foreach (Move move in moves)
        {
            if (eval(board, move) > eval(board, chosenMove))
                chosenMove = move;
            // int val = 10000;
            
            // if (isCheckmate(board, mv))
            // {
            //     return mv;
            // }
            // if (mv.IsCapture)
            // {
            //     Piece capturedPiece = board.GetPiece(mv.TargetSquare);
            //     int capturedPieceValue = value(capturedPiece);
            //     val = val - capturedPieceValue + pieceValues[(int)mv.CapturePieceType];
            // }
            //
            // if (mv.IsPromotion)
            // {
            //     board.MakeMove(mv);
            //     val -= value(board.GetPiece(mv.TargetSquare));
            //     // pieceValues[(int)mv.PromotionPieceType];
            //
            // }


            // mvpq.Enqueue(move, val);
        }


        // return mvpq.Dequeue();
        return chosenMove;
    }


    bool isCheck(Board board, Move move)
    {
        board.MakeMove(move);
        bool isCheck = board.IsInCheck();
        board.UndoMove(move);
        return isCheck;
    }
    
    bool isCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isCMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isCMate;
    }
}