using System.Globalization;
using System.Text;

namespace StudyBuddies.Core.Services;

public static class AnswerNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var trimmed = value.Trim().ToLowerInvariant();
        var formD = trimmed.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static (ReviewGrade Grade, bool IsCorrect) Score(string? userAnswer, string expected)
    {
        var u = Normalize(userAnswer);
        var e = Normalize(expected);

        if (u.Length == 0) return (ReviewGrade.Bad, false);
        if (u == e) return (ReviewGrade.Good, true);
        if (e.Length >= 4 && Levenshtein(u, e) == 1)
        {
            return (ReviewGrade.Hard, true);
        }
        return (ReviewGrade.Bad, false);
    }

    public static int Levenshtein(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) prev[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }
        return prev[b.Length];
    }
}
