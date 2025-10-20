/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This class is the game view model that integate the function of 
 * core, services and models.
 * 
 * 
 * Author: Adam Chen
 * Date: 2025/10/20
 * 
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiPuzzleHeroGame.Core;
using MauiPuzzleHeroGame.Models;
using MauiPuzzleHeroGame.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiPuzzleHeroGame.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        // === Services ===
        private readonly ImageService _imageService;
        private readonly StorageService _storageService;
        private readonly TimerService _timerService;
        private readonly PuzzleGenerator _puzzleGenerator;
        private readonly PuzzleValidator _puzzleValidator;

        // === Models ===
        private PuzzleBoard _puzzleBoard;

        // === Observable Properties ===
        [ObservableProperty]
        private ImageSource? puzzleImage;

        [ObservableProperty]
        private TimeSpan? elapsedTime;

        [ObservableProperty]
        private bool? isGameActive;

        [ObservableProperty]
        private bool? isCompleted;

        public ObservableCollection<PuzzlePiece> PuzzlePieces { get; } = new();

        // === Constructor ===
        public GameViewModel()
        {
            _imageService = new ImageService();
            _storageService = new StorageService();
            _timerService = new TimerService();
            _puzzleGenerator = new PuzzleGenerator();
            _puzzleValidator = new PuzzleValidator();

            _timerService.OnTick += (elapsed) =>
            {
                ElapsedTime = elapsed;
            };
        }

        // === Commands ===

        [RelayCommand]
        private async Task StartGameAsync()
        {
            try
            {
                var path = await _imageService.PickFromGalleryAsync();
                if (string.IsNullOrEmpty(path))
                    return;

                PuzzleImage = await _imageService.ResizeImageAsync(path, 600, 600);

                _puzzleBoard = await _puzzleGenerator.GeneratePuzzleAsync(path, 3); // 預設 3x3

                PuzzlePieces.Clear();
                foreach (var p in _puzzleBoard.Pieces)
                    PuzzlePieces.Add(p);

                IsGameActive = true;
                IsCompleted = false;
                _timerService.Reset();
                _timerService.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Start game fail: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Shuffle()
        {
            if (_puzzleBoard == null)
                return;

            try
            {
                _puzzleGenerator.Shuffle();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Shuffle fail: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CheckWin()
        {
            if (_puzzleBoard == null)
                return;

            if (_puzzleValidator.IsPuzzleCompleted(_puzzleBoard.Pieces))
            {
                IsCompleted = true;
                IsGameActive = false;
                _timerService.Stop();

                Application.Current?.MainPage?.DisplayAlert("Congratulation", "Complete!!!", "OK")
                    .ContinueWith(t =>
                    {
                        PuzzlePieces.Clear();
                        PuzzleImage = null;
                        ElapsedTime = TimeSpan.Zero;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        [RelayCommand]
        private void MovePiece(PuzzlePiece piece)
        {
            if (_puzzleBoard == null || piece == null)
                return;


            _puzzleGenerator.MovePiece(piece);
        }

        [RelayCommand]
        public async Task SaveProgressAsync()
        {
            if (_puzzleBoard == null)
                return;

            await _storageService.SaveAsync("gameProgress.json", _puzzleBoard);
        }

        [RelayCommand]
        public async Task LoadProgressAsync()
        {
            var board = await _storageService.LoadAsync<PuzzleBoard>("gameProgress.json");
            if (board == null)
                return;

            _puzzleBoard = board;
            PuzzlePieces.Clear();
            foreach (var p in _puzzleBoard.Pieces)
                PuzzlePieces.Add(p);

            IsGameActive = true;
            IsCompleted = false;
            _timerService.Reset();
            _timerService.Start();
        }
    }
}
