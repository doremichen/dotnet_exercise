/**
* This file is header for the native functions used in the C++ library.
*/
#pragma once

#ifdef CPPUSEDLL_EXPORTS
#define CPPUSEDLL_API __declspec(dllexport)
#else
#define CPPUSEDLL_API __declspec(dllimport)
#endif

// Function to add two integers
extern "C" CPPUSEDLL_API int AddNumbers(int a, int b);

// Function to get greeting message
extern "C" CPPUSEDLL_API const char* GetGreeting();