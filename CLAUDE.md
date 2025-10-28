# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HTMLToQPDF (Relorer.QuestPDF.HTML) is a C# library that extends QuestPDF to generate PDFs from HTML. It uses HtmlAgilityPack for parsing and provides a component-based architecture for rendering HTML elements as PDF content.

**Key Dependencies:**
- QuestPDF (2023.12.5) - PDF generation framework
- HtmlAgilityPack (1.11.59) - HTML parsing
- .NET 6.0

## Build Commands

Build the solution:
```bash
dotnet build HTMLToQPDF.sln
```

Build individual projects:
```bash
dotnet build HTMLToQPDF/HTMLToQPDF.csproj
dotnet build HTMLToQPDF.Example/HTMLToQPDF.Example.csproj
```

Run the WPF example application:
```bash
dotnet run --project HTMLToQPDF.Example/HTMLToQPDF.Example.csproj
```

Build for release:
```bash
dotnet build HTMLToQPDF.sln -c Release
```

Create NuGet package:
```bash
dotnet pack HTMLToQPDF/HTMLToQPDF.csproj -c Release
```

## Architecture

### Component System

The library uses a component-based architecture where HTML nodes are converted to QuestPDF components:

1. **Entry Point**: `IContainerExtensions.HTML()` creates an `HTMLDescriptor` which wraps an `HTMLComponent`
2. **HTML Processing**: `HTMLComponent` (HTMLToQPDF/Components/HTMLComponent.cs:15) parses HTML and creates component tree
3. **Component Hierarchy**:
   - `BaseHTMLComponent` - Abstract base for all HTML components
   - Tag-specific components in `Components/Tags/`:
     - `ImgComponent` - Handles `<img>` tags
     - `TableComponent` - Complex table rendering with colspan/rowspan support
     - `ListComponent` - Ordered and unordered lists
     - `AComponent` - Links
     - `BrComponent` - Line breaks
   - `ParagraphComponent` - Groups inline/text nodes
   - `HTMLComponent` - Root document handler

### HTML Node Classification

HTML elements are categorized in `StyleSettings.cs`:
- **Block Elements**: div, p, h1-h6, table, ul, ol, li, section, header, footer - create separate layout containers
- **Line Elements**: a, b, i, u, em, strong, sub, sup, img, br, td, th, tr - render inline

### CSS Class Support

The library now supports CSS class-based styling in addition to tag-based styling:

**Implementation Flow:**
1. `HtmlNodeExtensions.GetClasses()` (Extensions/HtmlNodeExtensions.cs:155) - Parses class attribute into individual class names
2. `BaseHTMLComponent.ApplyStyles()` (Components/BaseHTMLComponent.cs:36) - Applies both tag and class-based container styles
3. `ParagraphComponent.GetTextStyle()` (Components/ParagraphComponent.cs:138) - Applies both tag and class-based text styles
4. `ParagraphComponent.ApplyClassTextAlignments()` (Components/ParagraphComponent.cs:32) - Applies text-level alignment recursively from node and parents

**Key Behavior:**
- Class styles override tag styles (classes applied after tags)
- Multiple classes on one element are all processed in order
- Text alignments propagate from parent nodes
- Both container-level and text-level alignment applied for robust rendering

### Key Processing Logic

**Branch Separation** (HTMLComponent.cs:64-123):
The `CreateSeparateBranchesForTextNodes()` method restructures the HTML DOM to prevent unwanted line breaks. It splits nodes containing both block and inline elements:
```
<p><s><div>div</div>text in s</s>text in p</p>
→ <p><s><div>div</div></s><s>text in s</s>text in p</p>
```

**Rendering Strategy** (BaseHTMLComponent.cs:20-66):
- Block nodes with mixed content are split into columns
- Inline sequences are grouped into `ParagraphComponent`
- Each node applies container styles before rendering children

### Image Handling

Images are fetched via `GetImgBySrc` delegate (ImgUtils.cs:29):
- Base64 data URIs decoded inline
- HTTP(S) URLs downloaded using singleton HttpClient
- **Important**: Users should override with `OverloadImgReceivingFunc()` for async support

**Note**: The default implementation uses `.Result` (line 40) which blocks. Always recommend users provide custom async image fetching.

### Style Customization

Five style dictionaries in `HTMLComponent`:

**Tag-based styles:**
1. `TextStyles` (line 19) - Maps tag names to QuestPDF TextStyle (e.g., h1 → 32pt bold)
2. `ContainerStyles` (line 42) - Maps tag names to container transformations (e.g., p → 6pt vertical padding)

**Class-based styles** (added for CSS class support):
3. `ClassTextStyles` (line 49) - Maps class names to TextStyle
4. `ClassContainerStyles` (line 51) - Maps class names to container transformations
5. `ClassTextAlignments` (line 59) - Maps class names to TextDescriptor alignment actions

**Built-in class patterns:**
- `ql-align-center` - Centers both container and text
- `ql-align-right` - Right-aligns both container and text
- `ql-align-left` - Left-aligns both container and text

**Style Priority:** Class-based styles override tag-based styles when both exist. Multiple classes on a single element are all applied in order.

Users modify via `HTMLDescriptor`:
- `SetTextStyleForHtmlElement()` - Set styles by HTML tag
- `SetContainerStyleForHtmlElement()` - Set container styles by HTML tag
- `SetTextStyleForClass()` - Set text styles by CSS class
- `SetContainerStyleForClass()` - Set container styles by CSS class
- `SetTextAlignmentForClass()` - Set text alignment by CSS class
- `SetAlignmentForClass()` - Convenience method to set both container and text alignment
- `SetListVerticalPadding()`

## Project Structure

- **HTMLToQPDF/** - Main library
  - `Components/` - Component implementations
    - `Tags/` - Tag-specific components
  - `Extensions/` - Extension methods for QuestPDF and HtmlAgilityPack
  - `Utils/` - Utility classes (image handling, unit conversion, HTML preprocessing)
  - `HTMLDescriptor.cs` - Public API surface
  - `StyleSettings.cs` - HTML-to-component mapping

- **HTMLToQPDF.Example/** - WPF demo application
  - `ViewModels/` - MVVM view models
  - `Utilities/` - PDF creation and file dialog helpers

## Development Notes

- The library targets `net6.0`; the example app targets `net6.0-windows` (WPF)
- Namespace is `HTMLQuestPDF` for most internals, `HTMLToQPDF` for public API
- Debug builds include `IContainerExtensions.Debug()` helper (Extensions/IContainerExtensions.cs:8-13)
- Images use `AlignCenter().FitArea()` to prevent overflow (ImgComponent.cs:22)
- Table rendering handles complex colspan/rowspan via position tracking (TableComponent.cs:89-115)
