/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This class is the full container for the puzzle board in the Maui Puzzle Hero Game.
 * 
 * Author: Adam Chen
 * Date: 2025/10/17
 * 
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Models
{
    public class PuzzleBoard
    {
        // dimessions of the puzzle board
        public int Size { get; set; }

        // total pieces in the puzzle board
        public ObservableCollection<PuzzlePiece> Pieces { get; set; } = new ();

        /**
         * Constructor to initialize the puzzle board with a given size.
         */
        public PuzzleBoard(int size)
        {
            Size = size;
        }

        /**
         * Get a puzzle piece by its current row and column.
         */
        public PuzzlePiece? GetPieceAt(int row, int column)
        {
            return Pieces.FirstOrDefault(p => p.CurrentRow == row && p.CurrentColumn == column);
        }

        /**
         * Check if the puzzle is completely solved.
         */
        public bool IsSolved()
        {
            return Pieces.All(p => p.IsInCorrectPosition);
        }

        /**
         * Swap two puzzle pieces' positions.
         */
        public void SwapPieces(PuzzlePiece a, PuzzlePiece b)
        {
            (a.CurrentRow, b.CurrentRow) = (b.CurrentRow, a.CurrentRow);
            (a.CurrentColumn, b.CurrentColumn) = (b.CurrentColumn, a.CurrentColumn);
        }

    }
}
