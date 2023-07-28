using System;
using System.Collections;
using System.Collections.Generic;
using ChessChallenge.API;
using Microsoft.CodeAnalysis;

public sealed class MyBot : IChessBot
{
    // save tokens
    Move[] legalMoves;
    int eval;
    
    int[] pieceValues = { 0, 100, 305, 333, 563, 950, 10000 };
    int CHECKMATE = (int)1e9;
        
    const ulong lookupTableSize = 8 << 20;

    // store hashes for identity comparison, collisions should be reasonably rare
    static ulong[] hashes = new ulong[lookupTableSize];
    // store evaluations (goes with `hashes`)
    static int[] values = new int[lookupTableSize];

    int Eval(Board board)
    {
        eval = 0;
        //  being checkmated is bad
        if (board.IsInCheckmate())
            return -1000_000_000;

        if (board.IsInCheck())
            // has to be at least 100, or black won't checkmate
            eval += board.IsWhiteToMove ? -100 : 100;

        // sum up the pieces from whites perspective
        for (int i = 1; i < 7; ++i)
            eval += board.GetPieceList((PieceType)i, true).Count * pieceValues[i];

        // sum up the pieces from blacks perspective
        for (int i = 1; i < 7; ++i)
            eval -= board.GetPieceList((PieceType)i, false).Count * pieceValues[i];

        return (board.IsWhiteToMove ? 1 : -1) * eval;

    }

    int negaMax(Board board, int depth, int alpha, int beta, int bestEval = int.MinValue)
    {
        ulong hash = board.ZobristKey;

        if (hashes[hash % lookupTableSize] == hash)
            return values[hash % lookupTableSize];

        if (board.IsDraw())
            return 0;

        legalMoves = board.GetLegalMoves();
        // if we finished the search, evaluate the position
        if (depth == 0 || legalMoves.Length == 0)
            return Eval(board);

        //int bestEval = -Int32.MaxValue;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            // decay added for further-out moves to stop prioritizing longer checkmates
            int evaluation = (int)(0.99 * -negaMax(board, depth - 1, -beta, -alpha));
            board.UndoMove(move);

            if (evaluation > bestEval)
                bestEval = evaluation;
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
        
        legalMoves = board.GetLegalMoves();
        Move chosenMove = legalMoves[0];
        int bestEval = -CHECKMATE;;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            eval = -negaMax(board, 5 - 1, -CHECKMATE, CHECKMATE);
            board.UndoMove(move);
            if (eval > bestEval)
            {
                chosenMove = move;
                bestEval = eval;
            }
            
        }
        
        return chosenMove;
    }
}