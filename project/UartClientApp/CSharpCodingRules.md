C# Coding Rules for UartClientApp

Purpose
-------
A concise, project-level set of C# coding rules for UartClientApp (WPF, .NET 8, C# 12). Follow these rules to keep the codebase consistent, secure, testable, and maintainable.

General
-------
- Target framework: .NET 8, C# 12. Keep language features compatible with the project's TFM.
- File-scoped namespaces: use file-scoped namespace declarations where appropriate.
- Nullable annotations: keep nullable context enabled. Use `?` for nullable references and prefer non-nullable by default.
- Keep diffs small and focused. Make single-purpose commits.

Formatting & Style
------------------
- Use UTF-8 without BOM.
- Use 4 spaces for indentation. No tabs.
- Line endings: CRLF (Windows) consistent with the project.
- Keep max line length ~120 characters; prefer breaking long expressions.
- Using directives:
  - Prefer file-scoped `global using` in a single project-level file only if it reduces noise.
  - Sort usings: System first, then Microsoft, then others. Remove unused usings.
- Braces: Always use braces for multi-line and single-line statements.

Naming
------
- Types (classes, structs, enums, records, interfaces): PascalCase.
- Interfaces: prefix with `I` (e.g., `ITransport`).
- Methods and Properties: PascalCase.
- Local variables and parameters: camelCase.
- Private fields: _camelCase (leading underscore). Example: `private readonly ILogger _logger;`.
- Constants: PascalCase.
- Async methods: end with `Async` and return Task/Task<T> or ValueTask when appropriate.

Accessibility
-------------
- Apply the least privilege principle. Default to `private` unless the API must be exposed.
- Avoid public surface area unless needed. Prefer internal for project-only types.

Null checks & Argument validation
---------------------------------
- Guard public API inputs early using precise exceptions. Example:
  ArgumentNullException.ThrowIfNull(param);
  if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");

Async / Concurrency
-------------------
- Accept a CancellationToken on public async methods when the operation can be cancelled. Place it as the last parameter.
- Use `ConfigureAwait(false)` in library/helper code (non-UI code). UI code (WPF event handlers) may omit it.
- Avoid `.Result` or `.GetAwaiter().GetResult()` on tasks (no sync-over-async).
- Prefer `Task`/`Task<T>` over `void` for async methods. Use `async void` only for event handlers.
- Use `await using` for async disposables.

Exceptions & Error Handling
---------------------------
- Throw specific exception types (ArgumentNullException, InvalidOperationException, TimeoutException) rather than `Exception`.
- Do not swallow exceptions silently. Log before rethrowing or let them bubble to a meaningful boundary.
- Prefer `throw;` to preserve stack trace when rethrowing.

Logging & Diagnostics
---------------------
- Use Microsoft.Extensions.Logging. Inject `ILogger<T>` where needed rather than static logging.
- Include useful context in logs. Avoid logging sensitive data.

Security
--------
- Do not commit secrets or credentials. Use configuration and environment variables.
- Validate input from external sources.
- Use least-privileged permissions for any system resources.

Performance
-----------
- Avoid unnecessary allocations in hot paths. Use Span<T>/Memory<T> when it gives benefit and is appropriate.
- Stream large payloads instead of buffering entire content in memory.
- Measure before optimizing.

Testing
-------
- Add unit tests for public behavior and edge cases.
- Use the project's existing test framework when adding tests. Follow Arrange-Act-Assert.
- Tests must be deterministic and isolated from external state.

Design
------
- Follow SOLID principles.
- Prefer composition over inheritance when it reduces coupling.
- Do not add interfaces or abstractions unless they are necessary for external dependencies or for testing.

Code Organization
-----------------
- Keep UI (WPF) logic separate from application logic. Prefer MVVM.
- Put small utility/helper methods near their primary usage unless broadly useful.

PR & Review
-----------
- Provide a short, descriptive PR title and body explaining the why and what changed.
- Keep changes minimal and scoped. Add tests for behavior changes.

EditorConfig (recommended)
--------------------------
Add or update an .editorconfig at the repository root to enforce basic formatting. Example settings to include:

root = true
[*]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
max_line_length = 120

[*.{cs,csproj}]
# prefer file-scoped namespace where supported
csharp_style_namespace_declarations = file_scoped:suggestion

License header
--------------
- All source files must include an MIT license header comment at the top of the file.
- You may use either a short SPDX-style header or the full MIT license text; always include the copyright year and copyright holder.
- Place the header as the first non-empty lines in each source file.
- Example (short SPDX header):
  // SPDX-License-Identifier: MIT
  // Copyright (c) 2026 YourCompany

- Example (full MIT header):
  /*
   * MIT License
   * Copyright (c) 2026 YourCompany
   *
   * Permission is hereby granted, free of charge, to any person obtaining a copy
   * of this software and associated documentation files (the "Software"), to deal
   * in the Software without restriction, including without limitation the rights
   * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   * copies of the Software, and to permit persons to whom the Software is
   * furnished to do so, subject to the following conditions:
   *
   * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   * SOFTWARE.
   */

Notes
-----
- This file documents recommended conventions for the UartClientApp project. When stricter enforcement is required, add analyzers or editorconfig rules and communicate them in the contributor guidelines.
- If a rule conflicts with automated analyzer results already configured in the repo, prefer the repository analyzer settings.

