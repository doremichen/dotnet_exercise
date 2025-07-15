// CppNativeLibrary.cpp : Defines the functions for the static library.
//

#include "pch.h"
#include "framework.h"
#include "CppNativeLibrary.h"

// AddNumbers: Returns the sum of two numbers
int AddNumbers(int a, int b) {
	return a + b;
}

// GetGreeting: Returns a greeting message
const char* GetGreeting() {
	return "Hello, ! Welcome to the C++ Native Library.";
}