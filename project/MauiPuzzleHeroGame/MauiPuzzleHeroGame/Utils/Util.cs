/**
 * Copyright (c) 2025 Adam Game. All rights reserved.
 * 
 * Description: This Class is utility functions for the Maui Puzzle Hero Game.
 * 
 * Author: Adam Chen
 * Date: 2025/10/17
 * 
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPuzzleHeroGame.Utils
{
    public static class Util
    {
        /**
         * Logger with timestamp
         */
        public static void Log(string message)
        {
            // Get current timestamp
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.WriteLine($"[{timestamp}] {message}");
        }


    }
}
