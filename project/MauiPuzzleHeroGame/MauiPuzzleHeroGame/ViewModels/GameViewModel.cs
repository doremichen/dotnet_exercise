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
using MauiPuzzleHeroGame.Utils;
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


        [ObservableProperty]
        private GridItemsLayout puzzleLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
        {
            Span = 3,
            HorizontalItemSpacing = 0,
            VerticalItemSpacing = 0

        };


        public ObservableCollection<PuzzlePiece> PuzzlePieces { get; private set; } = new();

        // === Constructor ===
        public GameViewModel()
        {
            // log
            Util.Log("[GameViewModel] Initializing GameViewModel...");

            _imageService = new ImageService();
            _storageService = new StorageService();
            _timerService = new TimerService();
            _puzzleGenerator = new PuzzleGenerator();
            _puzzleValidator = new PuzzleValidator();

            _timerService.OnTick += (elapsed) =>
            {
                ElapsedTime = elapsed;
            };

            startGame().ContinueWith(
                t =>
                {
                    bool flowControl = t.Result;
                    if (!flowControl)
                    {
                        Util.Log("[GameViewModel] Initial game start failed.");
                    }
                });
        }

        // === Commands ===

        [RelayCommand]
        private async Task StartGameAsync()
        {
            Util.Log("[StartGameAsync] Starting game...");

            try
            {
                bool success = await startGame();
                if (!success)
                {
                    Util.Log("[StartGameAsync] Game start failed.");
                    return;
                }

                Util.Log("[StartGameAsync] Game started successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartGameAsync fail: {ex.Message}");
                Util.Log($"[StartGameAsync] Exception: {ex}");
            }
        }

        private async Task<bool> startGame()
        {
            try
            {
#if ANDROID || IOS
                var path = await _imageService.PickFromGalleryAsync();
#else
                string path = null;
#endif

                // Resize + cache
                var (puzzleImage, puzzleStream) = await _imageService.ResizeImageAsync(path, 600, 600);

                // Update PuzzleImage (UI Thread)
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PuzzleImage = puzzleImage;
                });

                // generate puzzle board (Background Thread)
                _puzzleBoard = await _puzzleGenerator.GeneratePuzzleAsync(puzzleStream, 3);

                // UI update
                await updateMainUI();

                _timerService.Reset();
                _timerService.Start();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Start game fail: {ex.Message}");
                Util.Log($"[startGame] Exception: {ex}");
                return false;
            }
        }

        [RelayCommand]
        private async Task ShuffleAsync()
        {
            bool flowControl = await ShufflePuzzule();
            if (!flowControl)
            {
                return;
            }
        }

        private async Task<bool> ShufflePuzzule()
        {
            if (_puzzleBoard == null)
                return false;

            try
            {
                _puzzleBoard = await _puzzleGenerator.Shuffle();
                await updateMainUI();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Shuffle fail: {ex.Message}");
            }

            return true;
        }

        /// <summary>
        /// Update main UI
        /// </summary>
        /// <returns></returns>
        /// Ensure this runs on the main thread
        /// 
        private async Task updateMainUI()
        {
            // update UI
            await MainThread.InvokeOnMainThreadAsync(() =>
            {

                var newPieces = _puzzleBoard.Pieces.ToList();
                PuzzlePieces = new ObservableCollection<PuzzlePiece>(newPieces);
                OnPropertyChanged(nameof(PuzzlePieces)); // ui update


                IsGameActive = true;
                IsCompleted = false;
            });
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

                        // reset game
                        Task<bool> task = startGame();
                        // show result
                        task.ContinueWith(_ =>
                        {
                            bool flowControl = task.Result;
                            if (!flowControl)
                            {
                                // log
                                Util.Log("[CheckWin] Failed to restart game after completion.");
                                return;
                            }

                        });

                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void MovePiece(PuzzlePiece piece)
        {
            //log
            Util.Log($"[MovePiece] Moving piece: {piece.Id}");

            if (_puzzleBoard == null || piece == null)
            {
                Util.Log("[MovePiece] Invalid puzzle board or piece.");
                return;
            }

            var newBoard = _puzzleGenerator.MovePiece(piece);
            if (newBoard != null)
            {
                _puzzleBoard = newBoard;
            }

            // update UI
            updateMainUI().Wait();
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

        [RelayCommand]
        public async Task RestartGameAsync()
        {
            PuzzlePieces.Clear();
            PuzzleImage = null;
            ElapsedTime = TimeSpan.Zero;
            IsGameActive = false;
            IsCompleted = false;
            _timerService.Reset();

            bool flowControl = await startGame();
            if (!flowControl)
            {
                return;
            }
        }

        [RelayCommand]
        public async Task BackAsync()
        {
            try
            {
                // assure to run on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsGameActive = false;
                    IsCompleted = false;
                    PuzzlePieces.Clear();
                    PuzzleImage = null;
                    ElapsedTime = TimeSpan.Zero;

                });

                _timerService.Stop();

                Util.Log("[GameViewModel]: Navigating back to previous page.");

                // navigate back
                await Shell.Current.GoToAsync("///MainPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BackAsync failed: {ex.Message}");
                Util.Log($"[BackAsync] Exception: {ex}");
            }
        }

        [RelayCommand]
        public void PieceTapped(PuzzlePiece piece)
        {
            // log
            Util.Log($"[PieceTapped] Piece tapped: {piece.Id}");
            MovePiece(piece);
            CheckWin();
        }

    }
}
