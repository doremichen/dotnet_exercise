#pragma once

// Adjust the include path to ensure it correctly points to the native C++ header file
#include "../CppNativeLibrary/CppNativeLibrary.h" // Adjusted the relative path

using namespace System;

namespace CppCliWrapper {
	public ref class NativeCalculator
	{
	public:
		// Wrapper for the native Add function
		static int Add(int a, int b) {
			return AddNumbers(a, b);
		}
		// Wrapper for the native get greeting function
		static String^ GetGreeting() {
			const char* greeting = ::GetGreeting();
			return gcnew String(greeting);
		}
	};
}
