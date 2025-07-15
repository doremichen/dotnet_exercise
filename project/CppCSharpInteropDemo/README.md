# CppCSharpInteropDemo

This is a demonstration project illustrating how to call native C++ functions from a C# WPF application via C++/CLI (Common Language Infrastructure).

## Project Structure
###### This solution comprises three projects:

CppNativeLibrary (C++ Dynamic Link Library):

Contains pure native C++ code.

Provides two simple functions: AddNumbers (performs integer addition) and GetGreeting (returns a C++ string literal).

This project compiles into a .dll file.

CppCliWrapper (C++/CLI Class Library):

Acts as the bridge between C# and native C++.

It's a mixed-mode assembly capable of calling the native C++ functions from CppNativeLibrary.

Encapsulates the native C++ functions within a public managed class (NativeCalculator), making them easily accessible to the C# project.

Handles data type conversions between C++ and C# (e.g., const char* to System.String^).

This project compiles into a .dll file.

WpfAppCSharp (C# WPF Application):

A standard C# WPF desktop application.

Calls methods defined in the NativeCalculator class within CppCliWrapper.dll by referencing it.

The user interface includes input boxes, buttons, and text display areas to demonstrate the results of the C++ function calls.

## How to Build and Run

### Open the Project:

Open the CppCSharpInteropDemo.sln solution file using Visual Studio 2019 or a later version.

### Set as Startup Project:

In the Solution Explorer, right-click on the WpfAppCSharp project.

### Select "Set as Startup Project".

Build the Solution:

From the Visual Studio top menu, click "Build" > "Build Solution".

Ensure that the build platform configuration (e.g., x64 or x86) is consistent across all projects to avoid DLL incompatibility issues.

### Run the Application:

Click the "Start" button (green arrow, typically F5) on the Visual Studio toolbar.

## Expected Outcome

Once the application launches, you'll see a WPF window:

Addition Functionality: Enter numbers in the "Number 1" and "Number 2" input boxes. Click the "Add Numbers (from C++)" button, and the "Result" label below will display the sum, calculated by the C++ function in CppNativeLibrary.

Greeting Message: Click the "Get Greeting (from C++)" button, and the "Greeting" label below will display "Hello from Native C++!", a string returned directly from the C++ function in CppNativeLibrary.

## Key Technical Concepts

C++/CLI: A .NET language used to write mixed-mode assemblies, bridging the native C++ and .NET worlds.

Platform Interoperability: The ability for different programming languages or execution environments to communicate and call functions from each other.

DLL (Dynamic Link Library): A library containing code and data that can be used by multiple programs.

WPF (Windows Presentation Foundation): A UI framework, part of the Microsoft .NET Framework, used for building desktop applications.
## Further Notes
This project serves as a foundational example of C# and C++ interoperability. For more complex scenarios, a deeper understanding of C++/CLI's memory management, exception handling, and the encapsulation of intricate C++ classes and objects will be necessary.
