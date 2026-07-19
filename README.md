> **Attribution:** Based on work by **Andrey Lundin** ([Advanced Text Editor with Ruler](https://www.codeproject.com/Articles/22783/Advanced-Text-Editor-with-Ruler), CodeProject 2008, CPOL). See [LICENSE](LICENSE) for details.

# VaderConsulting.TextRuler

A Windows Forms `UserControl` rendering a fully interactive text ruler similar to the ruler found in Microsoft Word. NuGet-packaged for use in rich-text or document-editing applications.

**Initiated:** 2015-02-13 · **Framework:** .NET Framework 4.8 · **Output:** Class Library

---

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