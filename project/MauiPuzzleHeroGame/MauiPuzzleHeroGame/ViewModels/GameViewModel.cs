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
using MauiPuzzleHeroGame.Views;
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
        private GridItemsLayout puzzleLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical);


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
                // get grid size from preferences
                int gridSize = Preferences.Get(Util.PREFS_PUZZLE_GRID_SIZE, 3);

                // Resize + cache
                var (puzzleImage, puzzleStream) = await _imageService.ResizeImageAsync(path, 600, 600);

                // Update PuzzleImage (UI Thread)
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PuzzleImage = puzzleImage;
                });

                // generate puzzle board (Background Thread)
                _puzzleBoard = await _puzzleGenerator.GeneratePuzzleAsync(puzzleStream, gridSize);

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
                Util.Log("[GameViewModel] BackAsync triggered.");

                // 停止計時（暫停遊戲）
                _timerService.Stop();
                IsGameActive = false;

                // 讀取偏好設定中最新的難度
                int gridSize = Preferences.Get(Util.PREFS_PUZZLE_GRID_SIZE, 3);

                Util.Log($"[GameViewModel] Returning from settings, gridSize = {gridSize}");

                // 重新套用難度（會自動清空、重建拼圖）
                await ApplyDifficultyAsync(gridSize);

                Util.Log("[GameViewModel] BackAsync finished successfully.");
            }
            catch (Exception ex)
            {
                Util.Log($"[GameViewModel] BackAsync failed: {ex}");
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

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await Shell.Current.GoToAsync("///SettingsPage");
        }

        /// <summary>
        /// 套用新的拼圖難度（依照使用者設定的格數重新生成）
        /// </summary>
        internal async Task ApplyDifficultyAsync(int gridSize)
        {
            try
            {
                Util.Log($"[GameViewModel] ApplyDifficultyAsync: Changing to {gridSize}x{gridSize}");
                // update puzzle layout span
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PuzzleLayout.Span = gridSize;
                    PuzzleLayout.HorizontalItemSpacing = 0;
                    PuzzleLayout.VerticalItemSpacing = 0;
                    OnPropertyChanged(nameof(PuzzleLayout));
                });

                // 如果目前遊戲正在進行，先暫停計時
                _timerService.Stop();
                IsGameActive = false;

                // 清除舊資料
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PuzzlePieces.Clear();
                    PuzzleImage = null;
                    IsCompleted = false;
                    ElapsedTime = TimeSpan.Zero;
                });

                // 重新開始遊戲（使用新的 gridSize）
                await RestartWithNewGridSize(gridSize);

                Util.Log($"[GameViewModel] Difficulty applied successfully: {gridSize}x{gridSize}");
            }
            catch (Exception ex)
            {
                Util.Log($"[GameViewModel] ApplyDifficultyAsync failed: {ex}");
            }
        }

        /// <summary>
        /// 依照新的 gridSize 重新生成拼圖
        /// </summary>
        private async Task RestartWithNewGridSize(int gridSize)
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

                // 更新圖片
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PuzzleImage = puzzleImage;
                });

                // 產生新的拼圖
                _puzzleBoard = await _puzzleGenerator.GeneratePuzzleAsync(puzzleStream, gridSize);

                // 更新 UI
                await updateMainUI();

                // 重啟計時器
                _timerService.Reset();
                _timerService.Start();

                IsGameActive = true;
            }
            catch (Exception ex)
            {
                Util.Log($"[GameViewModel] RestartWithNewGridSize failed: {ex}");
            }
        }

    }
}
