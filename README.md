# Better Comments for Visual Studio 2026

**Better Comments** is a Visual Studio extension that helps you create human-readable, categorized comments. This version features a high-performance parsing engine inspired by popular VSCode extensions.

## 🚀 Key Features

- **Multi-line Support:** Highlight entire blocks of code in `/* ... */`, `<!-- ... -->`, or `(* ... *)`.
- **VSCode-Style Parsing:** Uses the same tag detection logic as VSCode (supports tags anywhere in the line, with optional parameters).
- **Smooth Integration:** Works across C#, C++, JavaScript, TypeScript, Python, F#, VB, and Markup (HTML/XML).
- **Deduplicated Rendering:** Optimized to prevent overlapping tags and flickering in the editor.

## 💡 How to Use

Start your comment section with a tag followed by a colon. Tags are **case-insensitive**.

### 🔥 Critical (Critical)
- `ERR:`, `ERROR:`, `FIX:`, `FIXME:`
- *Example:* `// ERR: This logic is broken`

### ⚠️ Warning (Warning)
- `WARN:`, `WARNING:`
- *Example:* `/* WARNING: Use with caution */`

### 💡 Tasks & Ideas (Ideas)
- `TODO:`, `IDEA:`, `OPTIMIZE:`
- *Example:* `// TODO: Refactor this method`

### ℹ️ Information (Info)
- `NOTE:`, `INFO:`
- *Example:* `<!-- INFO: This is a shared component -->`

### 🔧 Parameters
You can include optional parameters in parentheses:
- *Example:* `// TODO(Artem): Finish this`

## ⚙️ Configuration

Customize the extension via **Tools -> Options -> Better Comments**:
- **Font & Size:** Offset the comment font size relative to the editor.
- **Opacity & Italics:** Make your comments subtle or prominent.
- **Highlight Mode:** Choose between highlighting the entire line or just the keyword.

### Colors & Styling
To customize the colors of each category:
1. Go to **Tools -> Options -> Environment -> Fonts and Colors**.
2. Select **Text Editor** in the dropdown.
3. Find items starting with `Better Comments` (e.g., `Better Comments - Critical`).

## 🤝 Contribute
Contributions are welcome! Check out our [Contribution Guidelines](CONTRIBUTING.md).

## 📄 License
Licensed under the [Apache 2.0](LICENSE) License.