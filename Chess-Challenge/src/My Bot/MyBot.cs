using System;
using System.Collections;
using System.Collections.Generic;
using ChessChallenge.API;
using Microsoft.CodeAnalysis;

public sealed class MyBot : IChessBot
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

    const ulong lookupTableSize = 8 << 20;

    // store hashes for identity comparison, collisions should be reasonably rare
    static ulong[] hashes = new ulong[lookupTableSize];
    // store evaluations (goes with `hashes`)
    static int[] values = new int[lookupTableSize];


    Move chosenMove;

    int maxDepth = 5;



    int negaMax(Board board, int depth, int alpha, int beta, int color)
    {
        ulong hash = board.ZobristKey ^ (ulong)(color + 1);

        if (hashes[hash % lookupTableSize] == hash)
            return values[hash % lookupTableSize];

        if (board.IsDraw())
            return 0;
    
        Move[] legalMoves = board.GetLegalMoves();
        
        // if we finished the search, evaluate the position
        if (depth == 0 || legalMoves.Length == 0)
        {
            if (board.IsInCheckmate())
                return board.IsWhiteToMove ? -Int32.MaxValue : Int32.MaxValue;

            int eval = 0;

            if (board.IsInCheck())
                eval += board.IsWhiteToMove ? -100 : 100;

            // for (int i = 1; i < 7; ++i)
            // foreach (PieceList pieceList in board.GetAllPieceLists())
            // {
            //     
            // }

            for (int i = 1; i < 7; ++i)
                eval += board.GetPieceList((PieceType)i, true).Count * pieceValues[i];
            for (int i = 1; i < 7; ++i)
                eval -= board.GetPieceList((PieceType)i, false).Count * pieceValues[i];


            return color * eval;
        }

        int bestEval = -Int32.MaxValue;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int evaluation = -negaMax(board, depth - 1, -beta, -alpha, -color);
            board.UndoMove(move);

            if (evaluation > bestEval)
            {
                bestEval = evaluation;
                if (depth == maxDepth) chosenMove = move;
            }
            alpha = Math.Max(alpha, bestEval);
            if (alpha >= beta) break;
        }

        hashes[hash % lookupTableSize] = hash;
        values[hash % lookupTableSize] = bestEval;

        return bestEval;
    }
    public Move Think(Board board, Timer timer)
    {
        Array.Clear(hashes);
        Array.Clear(values);
        int enemyPieceCount = 0;
        int myPieceCount = 0;

        foreach (var pieceList in board.GetAllPieceLists())
        {
            if (pieceList.IsWhitePieceList == board.IsWhiteToMove)
                myPieceCount += pieceList.Count;
            else
                enemyPieceCount += pieceList.Count;
        }
        chosenMove = board.GetLegalMoves()[0];

        if (enemyPieceCount * 2 <= myPieceCount)
            maxDepth = 8;
        else
            maxDepth = 5;

        negaMax(board, maxDepth, -Int32.MaxValue, Int32.MaxValue, board.IsWhiteToMove ? 1 : -1);
        return chosenMove;
    }
}