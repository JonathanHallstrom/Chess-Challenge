using System;
using System.Collections;
using System.Collections.Generic;
using ChessChallenge.API;
using Microsoft.CodeAnalysis;

public sealed class MyBotOld : IChessBot
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

    int maxDepth;

    bool endGame;

    int negaMax(Board board, int depth, int alpha, int beta, int color, int bestEval = int.MinValue)
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
            int eval = 0;
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

            // if we're white, color will be 1 otherwise -1
            return color * eval;
        }

        //int bestEval = -Int32.MaxValue;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            // decay added for further-out moves to stop prioritizing longer checkmates
            int evaluation = (int) (0.99 * -negaMax(board, depth - 1, -beta, -alpha, -color));
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
            endGame = true;
        else
            endGame = false;
        
        // so far this seems to be fine
        maxDepth = endGame ? 8 : 5;

        negaMax(board, maxDepth, -Int32.MaxValue, Int32.MaxValue, board.IsWhiteToMove ? 1 : -1);
        

        return chosenMove;
    }
}