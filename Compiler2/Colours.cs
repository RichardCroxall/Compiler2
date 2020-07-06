/*
    \file Colours.cs
    Copyright Notice\n
    Copyright (C) 1995-2020 Richard Croxall - developer and architect\n
    Copyright (C) 1995-2020 Dr Sylvia Croxall - code reviewer and tester\n

    This file is part of Compiler2.

    Compiler2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Compiler2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Compiler2.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

public static class Colours
{
    public const int AliceBlue = 0xF0F8FF;
    public const int AntiqueWhite = 0xFAEBD7;
    public const int Aqua = 0x00FFFF;
    public const int Aquamarine = 0x7FFFD4;
    public const int Azure = 0xF0FFFF;
    public const int Beige = 0xF5F5DC;
    public const int Bisque = 0xFFE4C4;
    public const int Black = 0x000000;
    public const int BlanchedAlmond = 0xFFEBCD;
    public const int Blue = 0x0000FF;
    public const int BlueViolet = 0x8A2BE2;
    public const int Brown = 0xA52A2A;
    public const int BurlyWood = 0xDEB887;
    public const int CadetBlue = 0x5F9EA0;
    public const int Chartreuse = 0x7FFF00;
    public const int Chocolate = 0xD2691E;
    public const int Coral = 0xFF7F50;
    public const int CornflowerBlue = 0x6495ED;
    public const int Cornsilk = 0xFFF8DC;
    public const int Crimson = 0xDC143C;
    public const int Cyan = 0x00FFFF;
    public const int DarkBlue = 0x00008B;
    public const int DarkCyan = 0x008B8B;
    public const int DarkGoldenRod = 0xB8860B;
    public const int DarkGray = 0xA9A9A9;
    public const int DarkGrey = 0xA9A9A9;
    public const int DarkGreen = 0x006400;
    public const int DarkKhaki = 0xBDB76B;
    public const int DarkMagenta = 0x8B008B;
    public const int DarkOliveGreen = 0x556B2F;
    public const int Darkorange = 0xFF8C00;
    public const int DarkOrchid = 0x9932CC;
    public const int DarkRed = 0x8B0000;
    public const int DarkSalmon = 0xE9967A;
    public const int DarkSeaGreen = 0x8FBC8F;
    public const int DarkSlateBlue = 0x483D8B;
    public const int DarkSlateGray = 0x2F4F4F;
    public const int DarkSlateGrey = 0x2F4F4F;
    public const int DarkTurquoise = 0x00CED1;
    public const int DarkViolet = 0x9400D3;
    public const int DeepPink = 0xFF1493;
    public const int DeepSkyBlue = 0x00BFFF;
    public const int DimGray = 0x696969;
    public const int DimGrey = 0x696969;
    public const int DodgerBlue = 0x1E90FF;
    public const int FireBrick = 0xB22222;
    public const int FloralWhite = 0xFFFAF0;
    public const int ForestGreen = 0x228B22;
    public const int Fuchsia = 0xFF00FF;
    public const int Gainsboro = 0xDCDCDC;
    public const int GhostWhite = 0xF8F8FF;
    public const int Gold = 0xFFD700;
    public const int GoldenRod = 0xDAA520;
    public const int Gray = 0x808080;
    public const int Grey = 0x808080;
    public const int Green = 0x008000;
    public const int GreenYellow = 0xADFF2F;
    public const int HoneyDew = 0xF0FFF0;
    public const int HotPink = 0xFF69B4;
    public const int IndianRed = 0xCD5C5C;
    public const int Indigo = 0x4B0082;
    public const int Ivory = 0xFFFFF0;
    public const int Khaki = 0xF0E68C;
    public const int Lavender = 0xE6E6FA;
    public const int LavenderBlush = 0xFFF0F5;
    public const int LawnGreen = 0x7CFC00;
    public const int LemonChiffon = 0xFFFACD;
    public const int LightBlue = 0xADD8E6;
    public const int LightCoral = 0xF08080;
    public const int LightCyan = 0xE0FFFF;
    public const int LightGoldenRodYellow = 0xFAFAD2;
    public const int LightGray = 0xD3D3D3;
    public const int LightGrey = 0xD3D3D3;
    public const int LightGreen = 0x90EE90;
    public const int LightPink = 0xFFB6C1;
    public const int LightSalmon = 0xFFA07A;
    public const int LightSeaGreen = 0x20B2AA;
    public const int LightSkyBlue = 0x87CEFA;
    public const int LightSlateGray = 0x778899;
    public const int LightSlateGrey = 0x778899;
    public const int LightSteelBlue = 0xB0C4DE;
    public const int LightYellow = 0xFFFFE0;
    public const int Lime = 0x00FF00;
    public const int LimeGreen = 0x32CD32;
    public const int Linen = 0xFAF0E6;
    public const int Magenta = 0xFF00FF;
    public const int Maroon = 0x800000;
    public const int MediumAquaMarine = 0x66CDAA;
    public const int MediumBlue = 0x0000CD;
    public const int MediumOrchid = 0xBA55D3;
    public const int MediumPurple = 0x9370D8;
    public const int MediumSeaGreen = 0x3CB371;
    public const int MediumSlateBlue = 0x7B68EE;
    public const int MediumSpringGreen = 0x00FA9A;
    public const int MediumTurquoise = 0x48D1CC;
    public const int MediumVioletRed = 0xC71585;
    public const int MidnightBlue = 0x191970;
    public const int MintCream = 0xF5FFFA;
    public const int MistyRose = 0xFFE4E1;
    public const int Moccasin = 0xFFE4B5;
    public const int NavajoWhite = 0xFFDEAD;
    public const int Navy = 0x000080;
    public const int OldLace = 0xFDF5E6;
    public const int Olive = 0x808000;
    public const int OliveDrab = 0x6B8E23;
    public const int Orange = 0xFFA500;
    public const int OrangeRed = 0xFF4500;
    public const int Orchid = 0xDA70D6;
    public const int PaleGoldenRod = 0xEEE8AA;
    public const int PaleGreen = 0x98FB98;
    public const int PaleTurquoise = 0xAFEEEE;
    public const int PaleVioletRed = 0xD87093;
    public const int PapayaWhip = 0xFFEFD5;
    public const int PeachPuff = 0xFFDAB9;
    public const int Peru = 0xCD853F;
    public const int Pink = 0xFFC0CB;
    public const int Plum = 0xDDA0DD;
    public const int PowderBlue = 0xB0E0E6;
    public const int Purple = 0x800080;
    public const int Red = 0xFF0000;
    public const int RosyBrown = 0xBC8F8F;
    public const int RoyalBlue = 0x4169E1;
    public const int SaddleBrown = 0x8B4513;
    public const int Salmon = 0xFA8072;
    public const int SandyBrown = 0xF4A460;
    public const int SeaGreen = 0x2E8B57;
    public const int SeaShell = 0xFFF5EE;
    public const int Sienna = 0xA0522D;
    public const int Silver = 0xC0C0C0;
    public const int SkyBlue = 0x87CEEB;
    public const int SlateBlue = 0x6A5ACD;
    public const int SlateGray = 0x708090;
    public const int SlateGrey = 0x708090;
    public const int Snow = 0xFFFAFA;
    public const int SpringGreen = 0x00FF7F;
    public const int SteelBlue = 0x4682B4;
    public const int Tan = 0xD2B48C;
    public const int Teal = 0x008080;
    public const int Thistle = 0xD8BFD8;
    public const int Tomato = 0xFF6347;
    public const int Turquoise = 0x40E0D0;
    public const int Violet = 0xEE82EE;
    public const int Wheat = 0xF5DEB3;
    public const int White = 0xFFFFFF;
    public const int WhiteSmoke = 0xF5F5F5;
    public const int Yellow = 0xFFFF00;
    public const int YellowGreen = 0x9ACD32;

    private static readonly Dictionary<string, int> ColourName = new Dictionary<string, int>();

    static Colours()
    {
        ColourName.Add("AliceBlue", AliceBlue);
        ColourName.Add("AntiqueWhite", AntiqueWhite);
        ColourName.Add("Aqua", Aqua);
        ColourName.Add("Aquamarine", Aquamarine);
        ColourName.Add("Azure", Azure);
        ColourName.Add("Beige", Beige);
        ColourName.Add("Bisque", Bisque);
        ColourName.Add("Black", Black);
        ColourName.Add("BlanchedAlmond", BlanchedAlmond);
        ColourName.Add("Blue", Blue);
        ColourName.Add("BlueViolet", BlueViolet);
        ColourName.Add("Brown", Brown);
        ColourName.Add("BurlyWood", BurlyWood);
        ColourName.Add("CadetBlue", CadetBlue);
        ColourName.Add("Chartreuse", Chartreuse);
        ColourName.Add("Chocolate", Chocolate);
        ColourName.Add("Coral", Coral);
        ColourName.Add("CornflowerBlue", CornflowerBlue);
        ColourName.Add("Cornsilk", Cornsilk);
        ColourName.Add("Crimson", Crimson);
        ColourName.Add("Cyan", Cyan);
        ColourName.Add("DarkBlue", DarkBlue);
        ColourName.Add("DarkCyan", DarkCyan);
        ColourName.Add("DarkGoldenRod", DarkGoldenRod);
        ColourName.Add("DarkGray", DarkGray);
        ColourName.Add("DarkGrey", DarkGrey);
        ColourName.Add("DarkGreen", DarkGreen);
        ColourName.Add("DarkKhaki", DarkKhaki);
        ColourName.Add("DarkMagenta", DarkMagenta);
        ColourName.Add("DarkOliveGreen", DarkOliveGreen);
        ColourName.Add("Darkorange", Darkorange);
        ColourName.Add("DarkOrchid", DarkOrchid);
        ColourName.Add("DarkRed", DarkRed);
        ColourName.Add("DarkSalmon", DarkSalmon);
        ColourName.Add("DarkSeaGreen", DarkSeaGreen);
        ColourName.Add("DarkSlateBlue", DarkSlateBlue);
        ColourName.Add("DarkSlateGray", DarkSlateGray);
        ColourName.Add("DarkSlateGrey", DarkSlateGrey);
        ColourName.Add("DarkTurquoise", DarkTurquoise);
        ColourName.Add("DarkViolet", DarkViolet);
        ColourName.Add("DeepPink", DeepPink);
        ColourName.Add("DeepSkyBlue", DeepSkyBlue);
        ColourName.Add("DimGray", DimGray);
        ColourName.Add("DimGrey", DimGrey);
        ColourName.Add("DodgerBlue", DodgerBlue);
        ColourName.Add("FireBrick", FireBrick);
        ColourName.Add("FloralWhite", FloralWhite);
        ColourName.Add("ForestGreen", ForestGreen);
        ColourName.Add("Fuchsia", Fuchsia);
        ColourName.Add("Gainsboro", Gainsboro);
        ColourName.Add("GhostWhite", GhostWhite);
        ColourName.Add("Gold", Gold);
        ColourName.Add("GoldenRod", GoldenRod);
        ColourName.Add("Gray", Gray);
        ColourName.Add("Grey", Grey);
        ColourName.Add("Green", Green);
        ColourName.Add("GreenYellow", GreenYellow);
        ColourName.Add("HoneyDew", HoneyDew);
        ColourName.Add("HotPink", HotPink);
        ColourName.Add("IndianRed ", IndianRed);
        ColourName.Add("Indigo ", Indigo);
        ColourName.Add("Ivory", Ivory);
        ColourName.Add("Khaki", Khaki);
        ColourName.Add("Lavender", Lavender);
        ColourName.Add("LavenderBlush", LavenderBlush);
        ColourName.Add("LawnGreen", LawnGreen);
        ColourName.Add("LemonChiffon", LemonChiffon);
        ColourName.Add("LightBlue", LightBlue);
        ColourName.Add("LightCoral", LightCoral);
        ColourName.Add("LightCyan", LightCyan);
        ColourName.Add("LightGoldenRodYellow", LightGoldenRodYellow);
        ColourName.Add("LightGray", LightGray);
        ColourName.Add("LightGrey", LightGrey);
        ColourName.Add("LightGreen", LightGreen);
        ColourName.Add("LightPink", LightPink);
        ColourName.Add("LightSalmon", LightSalmon);
        ColourName.Add("LightSeaGreen", LightSeaGreen);
        ColourName.Add("LightSkyBlue", LightSkyBlue);
        ColourName.Add("LightSlateGray", LightSlateGray);
        ColourName.Add("LightSlateGrey", LightSlateGrey);
        ColourName.Add("LightSteelBlue", LightSteelBlue);
        ColourName.Add("LightYellow", LightYellow);
        ColourName.Add("Lime", Lime);
        ColourName.Add("LimeGreen", LimeGreen);
        ColourName.Add("Linen", Linen);
        ColourName.Add("Magenta", Magenta);
        ColourName.Add("Maroon", Maroon);
        ColourName.Add("MediumAquaMarine", MediumAquaMarine);
        ColourName.Add("MediumBlue", MediumBlue);
        ColourName.Add("MediumOrchid", MediumOrchid);
        ColourName.Add("MediumPurple", MediumPurple);
        ColourName.Add("MediumSeaGreen", MediumSeaGreen);
        ColourName.Add("MediumSlateBlue", MediumSlateBlue);
        ColourName.Add("MediumSpringGreen", MediumSpringGreen);
        ColourName.Add("MediumTurquoise", MediumTurquoise);
        ColourName.Add("MediumVioletRed", MediumVioletRed);
        ColourName.Add("MidnightBlue", MidnightBlue);
        ColourName.Add("MintCream", MintCream);
        ColourName.Add("MistyRose", MistyRose);
        ColourName.Add("Moccasin", Moccasin);
        ColourName.Add("NavajoWhite", NavajoWhite);
        ColourName.Add("Navy", Navy);
        ColourName.Add("OldLace", OldLace);
        ColourName.Add("Olive", Olive);
        ColourName.Add("OliveDrab", OliveDrab);
        ColourName.Add("Orange", Orange);
        ColourName.Add("OrangeRed", OrangeRed);
        ColourName.Add("Orchid", Orchid);
        ColourName.Add("PaleGoldenRod", PaleGoldenRod);
        ColourName.Add("PaleGreen", PaleGreen);
        ColourName.Add("PaleTurquoise", PaleTurquoise);
        ColourName.Add("PaleVioletRed", PaleVioletRed);
        ColourName.Add("PapayaWhip", PapayaWhip);
        ColourName.Add("PeachPuff", PeachPuff);
        ColourName.Add("Peru", Peru);
        ColourName.Add("Pink", Pink);
        ColourName.Add("Plum", Plum);
        ColourName.Add("PowderBlue", PowderBlue);
        ColourName.Add("Purple", Purple);
        ColourName.Add("Red", Red);
        ColourName.Add("RosyBrown", RosyBrown);
        ColourName.Add("RoyalBlue", RoyalBlue);
        ColourName.Add("SaddleBrown", SaddleBrown);
        ColourName.Add("Salmon", Salmon);
        ColourName.Add("SandyBrown", SandyBrown);
        ColourName.Add("SeaGreen", SeaGreen);
        ColourName.Add("SeaShell", SeaShell);
        ColourName.Add("Sienna", Sienna);
        ColourName.Add("Silver", Silver);
        ColourName.Add("SkyBlue", SkyBlue);
        ColourName.Add("SlateBlue", SlateBlue);
        ColourName.Add("SlateGray", SlateGray);
        ColourName.Add("SlateGrey", SlateGrey);
        ColourName.Add("Snow", Snow);
        ColourName.Add("SpringGreen", SpringGreen);
        ColourName.Add("SteelBlue", SteelBlue);
        ColourName.Add("Tan", Tan);
        ColourName.Add("Teal", Teal);
        ColourName.Add("Thistle", Thistle);
        ColourName.Add("Tomato", Tomato);
        ColourName.Add("Turquoise", Turquoise);
        ColourName.Add("Violet", Violet);
        ColourName.Add("Wheat", Wheat);
        ColourName.Add("White", White);
        ColourName.Add("WhiteSmoke", WhiteSmoke);
        ColourName.Add("Yellow", Yellow);
        ColourName.Add("YellowGreen", YellowGreen);
    }

    public static bool ContainsKey(string colourName)
    {
        return ColourName.ContainsKey(colourName);
    }

    public static int RGBColour(string colourName)
    {
        return ColourName[colourName];
    }
}
