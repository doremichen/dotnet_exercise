/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This class is used to control game timing, support Start / Stop / Reset 
 * and event callback
 * Functions:
 * Asynchronous timing, no UI blocking.
 * Provides an OnTick event (ViewModels can subscribe to update the screen).
 * Supports pause/resume/reset.
 * 
 * Author: Adam Chen
 * Date: 2025/10/20
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Services
{
    public class TimerService
    {
        // Cancellation token for stopping the timer
        private CancellationTokenSource _cts;
        // Tick interval
        private DateTime _startTime;
        // Elapsed time
        private TimeSpan _elapsed;
        // Is the timer running
        private bool _isRunning;

        // Elapsed time
        public TimeSpan Elapsed => _elapsed;

        // Event triggered on each tick
        public event Action<TimeSpan>? OnTick;

        // Is the timer running
        public bool IsRunning => _isRunning;


        /**
         * Start the timer
         * 
         * param updateIntervalMs: interval in milliseconds for each tick (default 1000ms)
         */
        public void Start(int updateIntervalMs = 1000)
        {
            // check if already running
            if (_isRunning)
                return;
            _isRunning = true;
            _startTime = DateTime.Now - _elapsed;
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    _elapsed = DateTime.Now - _startTime;
                    OnTick?.Invoke(_elapsed);
                    await Task.Delay(updateIntervalMs, _cts.Token);
                }
            }, _cts.Token);
        }

        /**
         * Stop the timer
         */
        public void Stop()
        {
            if (!_isRunning)
                return;
            _isRunning = false;
            _cts.Cancel();
            _elapsed = DateTime.Now - _startTime;
        }

        /**
         * Reset the timer
         */
        public void Reset()
        {
            Stop();
            _elapsed = TimeSpan.Zero;
            OnTick?.Invoke(_elapsed);
        }

    }
}
