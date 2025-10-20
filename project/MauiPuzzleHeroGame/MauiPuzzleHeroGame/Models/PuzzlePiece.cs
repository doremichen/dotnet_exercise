/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This Class is defined to represent a single puzzle piece structure in the Maui Puzzle Hero Game.
 * 
 * Author: Adam Chen
 * Date: 2025/10/17
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Models
{
    public class PuzzlePiece
    {
        // id: unique identifier for the puzzle piece
        public int Id { get; set; }

        // right position of the puzzle piece: Column and Row
        public int CorrectRow { get; set; }
        public int CorrectColumn { get; set; }

        // current position of the puzzle piece: Column and Row
        public int CurrentRow { get; set; }
        public int CurrentColumn { get; set; }

        // used to check if the piece is in the correct position
        public bool IsInCorrectPosition =>
            CorrectRow == CurrentRow && CorrectColumn == CurrentColumn;

        // Image source for the puzzle piece
        public ImageSource? ImagePart { get; set; }

        // used to check if the piece is selected
        public bool IsSelected { get; set; }

        /**
         * Clone method to create a copy of the current puzzle piece.
         */
        public PuzzlePiece Clone()
        {
            return new PuzzlePiece
            {
                Id = this.Id,
                CorrectRow = this.CorrectRow,
                CorrectColumn = this.CorrectColumn,
                CurrentRow = this.CurrentRow,
                CurrentColumn = this.CurrentColumn,
                ImagePart = this.ImagePart,
                IsSelected = this.IsSelected
            };
        }

    }
}
