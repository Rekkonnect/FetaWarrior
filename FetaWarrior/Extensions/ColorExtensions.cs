﻿using Discord;

namespace FetaWarrior.Extensions;

/// <summary>Provides extensions for the <seealso cref="Color"/> struct.</summary>
public static class ColorExtensions
{
    /// <summary>Darkens a <seealso cref="Color"/> and returns a new instance with the darkened <seealso cref="Color"/>.</summary>
    /// <param name="input">The input <seealso cref="Color"/> to darken.</param>
    /// <param name="multiplier">The multiplier of darkness to apply. This means the brightness will be multiplied by 1 - multiplier.</param>
    public static Color Darken(this Color input, double multiplier)
    {
        return new(
            DarkenInt(input.R, multiplier),
            DarkenInt(input.G, multiplier),
            DarkenInt(input.B, multiplier));
    }

    private static byte DarkenInt(int input, double multiplier) => (byte)(input * (1 - multiplier));
}
