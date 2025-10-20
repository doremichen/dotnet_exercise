
/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This class is responsible for generating puzzles in the Maui Puzzle Hero Game.
 * 
 * 1. Cut the original image into NxN small images
 * 2. Create a corresponding PuzzlePiece for each piece
 * 3. Shuffle the puzzle pieces
 * 
 * 
 * Author: Adam Chen
 * Date: 2025/10/17
 * 
 */
using MauiPuzzleHeroGame.Models;
using Microsoft.Maui.Graphics.Platform;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Core
{
    public class PuzzleGenerator
    {
        // Random number generator for shuffling
        private static readonly Random _random = new ();

        private int _boardSize;
        private PuzzleBoard _puzzleBoard;
        private PuzzlePiece? _selectedPiece; // reference to the currently selected piece at first time

        /**
         * Generate an NxN puzzle and shuffle the positions
         * 
         * param filePath: path to the original image file
         * param boardSize: size of the puzzle board (N)
         * 
         * returns: a PuzzleBoard object containing the shuffled puzzle pieces
         */
        public async Task<PuzzleBoard> GeneratePuzzleAsync(string filePath, int boardSize)
        {
            if (boardSize < 2)
                throw new ArgumentException("Board size must be at least 2.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Image file not found", filePath);

            return await Task.Run(() =>
            {
                _boardSize = boardSize;

                using var input = File.OpenRead(filePath);
                using var original = SKBitmap.Decode(input);

                int pieceWidth = original.Width / boardSize;
                int pieceHeight = original.Height / boardSize;

                var pieces = new List<PuzzlePiece>();
                int id = 0;

                for (int row = 0; row < boardSize; row++)
                {
                    for (int col = 0; col < boardSize; col++)
                    {
                        // cut the image piece
                        var croppedImage = CropBitmap(original, col * pieceWidth, row * pieceHeight, pieceWidth, pieceHeight);

                        pieces.Add(new PuzzlePiece
                        {
                            Id = id++,
                            CorrectRow = row,
                            CorrectColumn = col,
                            CurrentRow = row,
                            CurrentColumn = col,
                            ImagePart = ImageSource.FromStream(() => croppedImage)
                        });
                    }
                }

                // shuffle pieces
                ShufflePieces(pieces, boardSize);

                var board = new PuzzleBoard(boardSize);
                foreach (var p in pieces)
                    board.Pieces.Add(p);

                _puzzleBoard = board;

                return board;
            });
        }

        /**
         * Cut a SKBitmap into a smaller piece
         * 
         * param original: original SKBitmap image
         * param x: x coordinate of the top-left corner of the piece
         * param y: y coordinate of the top-left corner of the piece
         * param width: width of the piece
         * param height: height of the piece
         * 
         * returns: MemoryStream containing the cropped image in Bitmap format
         */
        private MemoryStream CropBitmap(SKBitmap original, int x, int y, int width, int height)
        {
            var cropped = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(cropped))
            {
                var srcRect = new SKRectI(x, y, x + width, y + height);
                var destRect = new SKRectI(0, 0, width, height);
                canvas.DrawBitmap(original, srcRect, destRect);
            }

            using var image = SKImage.FromBitmap(cropped);
            var ms = new MemoryStream();
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
            ms.Position = 0;
            return ms;
        }

        /**
         * Fisher-Yates shuffle algorithm to randomize the order of puzzle pieces
         * 
         * param pieces: list of puzzle pieces to shuffle
         * param boardSize: size of the puzzle board (N)
         */
        private void ShufflePieces(List<PuzzlePiece> pieces, int boardSize)
        {
            for (int i = pieces.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                // Swap pieces[i] with pieces[j]
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }

            // ReDesign CurrentRow / CurrentColumn
            for (int index = 0; index < pieces.Count; index++)
            {
                pieces[index].CurrentRow = index / boardSize;
                pieces[index].CurrentColumn = index % boardSize;
            }
        }

        public void Shuffle()
        {
            ShufflePieces(_puzzleBoard.Pieces.ToList<PuzzlePiece>(), _boardSize);
            // clear pieces and re-add to trigger UI update
            var pieces = _puzzleBoard.Pieces.ToList<PuzzlePiece>();
            _puzzleBoard.Pieces.Clear();
            foreach (var p in pieces)
                _puzzleBoard.Pieces.Add(p);
        }

        internal void MovePiece(PuzzlePiece piece)
        {
            if (_puzzleBoard == null || piece == null)
                return;

            // If no piece is selected, select the current piece
            if (_selectedPiece == null)
            {
                _selectedPiece = piece;
                piece.IsSelected = true;
                return;
            }

            // If the same piece is selected again, deselect it
            if (_selectedPiece == piece)
            {
                piece.IsSelected = false;
                _selectedPiece = null;
                return;
            }

            // == swap the selected piece with the current piece ==
            _puzzleBoard.SwapPieces(_selectedPiece, piece);

            // Update the pieces collection to trigger UI update
            var pieces = _puzzleBoard.Pieces.OrderBy(p => p.CurrentRow * _boardSize + p.CurrentColumn).ToList();
            _puzzleBoard.Pieces.Clear();
            foreach (var p in pieces)
                _puzzleBoard.Pieces.Add(p);

            // Deselect the piece after swapping
            _selectedPiece.IsSelected = false;
            _selectedPiece = null;
        }
    }
}
