# VaderConsulting.TextRuler

Windows Forms `UserControl` that draws an interactive Word-style text ruler for rich-text or document editors: draggable left/right margins, first-line and hanging indents, right indent, click-to-add tab stops, and DPI-aware millimetre scaling. NuGet-packaged class library targeting .NET Framework 4.8.

**Source last updated:** 2020-04-22 · **Language:** C# · **Target:** .NET Framework 4.8 · **Output:** class library

## Attribution and provenance

> **Attribution:** Based on work by **Andrey Lundin** ([Advanced Text Editor with Ruler](https://www.codeproject.com/Articles/22783/Advanced-Text-Editor-with-Ruler), CodeProject 2008, CPOL). See [LICENSE](LICENSE) for details.

## Features

| Feature | Details |
|---------|---------|
| Left / Right Margin | Draggable page margin markers |
| Left Indent (upper) | First-line indent marker |
| Left Hanging Indent (lower) | Hanging/body indent marker |
| Right Indent | Right paragraph indent marker |
| Tab Stops | Click to add, drag off ruler to remove |
| DPI-aware scaling | Physical millimetre accuracy via Graphics.DpiX |

---

## Events

| Event | Raised when |
|-------|-------------|
| `LeftIndentChanging` | First-line indent dragged |
| `RightIndentChanging` | Right indent dragged |
| `LeftMarginChanging` | Left margin dragged |
| `RightMarginChanging` | Right margin dragged |
| `BothLeftIndentsChanged` | Both left markers moved together |
| `TabAdded` / `TabChanged` / `TabRemoved` | Tab stop created / repositioned / removed |

---

## Usage

```csharp
var ruler = new VaderConsulting.TextRuler
{
    Dock = DockStyle.Top,
    Height = 22,
    LeftMargin = 25,
    RightMargin = 20,
    TabsEnabled = true
};
ruler.LeftIndentChanging += newValue => richTextBox1.SelectionIndent = newValue;
this.Controls.Add(ruler);
```

## Requirements

- Visual Studio 2013 or later, .NET Framework 4.8

