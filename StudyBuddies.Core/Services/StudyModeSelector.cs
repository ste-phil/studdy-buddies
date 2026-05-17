using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public static class StudyModeSelector
{
    public static StudyMode Pick(Word word, Review? review, int distractorCount, IReadOnlySet<StudyMode>? allowedModes = null)
    {
        var hasUsableExample = !string.IsNullOrWhiteSpace(word.Example)
            && word.Example!.Contains(word.Term, StringComparison.OrdinalIgnoreCase);
        var hasDistractors = distractorCount >= 3;

        var eligible = new List<StudyMode>
        {
            StudyMode.Flashcard,
            StudyMode.Type,
            StudyMode.Listening,
        };
        if (hasDistractors) eligible.Add(StudyMode.MultipleChoice);
        if (hasUsableExample) eligible.Add(StudyMode.Cloze);

        if (allowedModes is { Count: > 0 })
        {
            var filtered = eligible.Where(allowedModes.Contains).ToList();
            // Fall back to all card-eligible modes if user's selection has no overlap (e.g. only Cloze, no example).
            if (filtered.Count > 0) eligible = filtered;
        }

        return eligible[Random.Shared.Next(eligible.Count)];
    }
}
