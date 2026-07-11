# Better Comments for Visual Studio 2026

**Better Comments** is a Visual Studio extension that helps you create human-readable, categorized comments. This version features a high-performance parsing engine inspired by popular VSCode extensions.

## Features

- **Multi-line Support:** Highlight entire blocks of code in `/* ... */`, `<!-- ... -->`, or `(* ... *)`.
- **VSCode-Style Parsing:** Uses the same tag detection logic as VSCode (supports tags anywhere in the line, with optional parameters).
- **Custom Tags:** Define your own comment tags with custom colors or map them to existing types.
- **Wide Language Support:** C#, C++, JavaScript, TypeScript, Python, F#, VB, Markup (HTML/XML), Rust, and Go.
- **Deduplicated Rendering:** Optimized to prevent overlapping tags and flickering in the editor.

## How to Use

Start your comment section with a tag followed by a colon. Tags are **case-insensitive**.

### Critical (Red)
- `ERR:`, `ERROR:`, `FIX:`, `FIXME:`
- Example: `// ERR: This logic is broken`

### Warning (Orange)
- `WARN:`, `WARNING:`
- Example: `/* WARNING: Use with caution */`

### Tasks & Ideas (Blue)
- `TODO:`, `IDEA:`, `OPTIMIZE:`
- Example: `// TODO: Refactor this method`

### Information (Green)
- `NOTE:`, `INFO:`
- Example: `<!-- INFO: This is a shared component -->`

### Custom Tags
Define your own tags via **Tools -> Options -> Better Comments -> Custom Tags**:
- Example: `// REVIEW: Check this implementation`
- Example: `// HACK: Temporary workaround`

### Parameters
You can include optional parameters in parentheses:
- Example: `// TODO(Artem): Finish this`

## Configuration

Customize the extension via **Tools -> Options -> Better Comments**:

### Global Settings
- **Font & Size:** Offset the comment font size relative to the editor.
- **Opacity & Italics:** Make your comments subtle or prominent.

### Per-Type Settings
For each comment type (Critical, Warning, Ideas, Info):
- **Bold:** Make the comment bold.
- **Underline:** Add underline decoration.
- **Highlight keyword only:** Show only the tag keyword, not the entire comment.

### Custom Tags
Add your own tags that map to existing types or use custom colors.

### Colors & Styling
To customize the colors of each category:
1. Go to **Tools -> Options -> Environment -> Fonts and Colors**.
2. Select **Text Editor** in the dropdown.
3. Find items starting with `Better Comments` (e.g., `Better Comments - Critical`).

## Supported Languages

| Language | Comment Styles |
|----------|----------------|
| C# | `//`, `/* */` |
| C/C++ | `//`, `/* */` |
| JavaScript/TypeScript | `//`, `/* */` |
| Python | `#` |
| F# | `//`, `(* *)` |
| Visual Basic | `'` |
| Markup (HTML/XML) | `<!-- -->` |
| Rust | `//`, `/* */` |
| Go | `//`, `/* */` |

## Contributing

Contributions are welcome! Check out our [Contribution Guidelines](CONTRIBUTING.md).

## License

Licensed under the [Apache 2.0](LICENSE) License.
