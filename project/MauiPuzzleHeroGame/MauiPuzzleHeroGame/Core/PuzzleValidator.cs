/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This calss is mainly used to check whether the player has completed the puzzle, 
 * and can also be expanded into a prompt and automatic verification mechanism..
 * 
 * Author: Adam Chen
 * Date: 2025/10/20
 * 
 */
using MauiPuzzleHeroGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Core
{
    public class PuzzleValidator
    {
        /**
         * IsPuzzleCompleted
         * Check if the puzzle is complete (all pieces are in their correct places)
         * 
         * param All pieces of the current puzzle
         * 
         * returns: true if the puzzle is complete, false otherwise
         */
        public bool IsPuzzleCompleted(IList<PuzzlePiece> pieces)
        {
            // check if all pieces are in their correct positions or null (for empty piece)
            if (pieces == null || pieces.Count == 0)
                return false;

            return pieces.All(p => p.IsInCorrectPosition);
        }

        /**
         * GetCorrectPieces
         * Returns a list of puzzle pieces placed in the correct position
         * 
         * param Puzzle list
         * param Permissible error (e.g. 0.1 means a 10% position error is allowed)
         * 
         * returns: list of correctly placed puzzle pieces
         */
        public List<PuzzlePiece> GetCorrectPieces(IList<PuzzlePiece> pieces, double tolerance = 0.0)
        {
            if (pieces == null || pieces.Count == 0)
                return new List<PuzzlePiece>();


            return pieces.Where(p => IsPieceInCorrectPosition(p, tolerance)).ToList();
        }

        /**
         * IsPieceInCorrectPosition
         * check if a single puzzle piece is in the correct position within a given tolerance
         * 
         * param Puzzle piece
         * param Permissible error
         * 
         * returns: true if the piece is in the correct position, false otherwise
         */
        private bool IsPieceInCorrectPosition(PuzzlePiece p, double tolerance)
        {
            if (p == null)
                return false;

            if (tolerance <= 0.0)
                return p.IsInCorrectPosition;

            // Calculate the allowed deviation based on tolerance
            double rowDiff = Math.Abs(p.CorrectRow - p.CurrentRow);
            double colDiff = Math.Abs(p.CorrectColumn - p.CurrentColumn);
            return rowDiff <= tolerance && colDiff <= tolerance;
        }
    }
}
