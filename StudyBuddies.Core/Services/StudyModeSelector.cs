using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public static class StudyModeSelector
{
    public static StudyMode Pick(Word word, Review? review, int distractorCount)
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

        return eligible[Random.Shared.Next(eligible.Count)];
    }
}
