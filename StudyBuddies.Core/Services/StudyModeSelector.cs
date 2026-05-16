using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public static class StudyModeSelector
{
    public static StudyMode Pick(Word word, Review? review, int distractorCount)
    {
        var reps = review?.Repetitions ?? 0;
        var hasUsableExample = !string.IsNullOrWhiteSpace(word.Example)
            && word.Example!.Contains(word.Term, StringComparison.OrdinalIgnoreCase);
        var hasDistractors = distractorCount >= 3;

        if (reps == 0) return StudyMode.Flashcard;
        if (reps <= 2) return StudyMode.Type;
        if (reps <= 4)
        {
            if (hasUsableExample) return StudyMode.Cloze;
            if (hasDistractors) return StudyMode.MultipleChoice;
            return StudyMode.Type;
        }

        var slot = reps % 3;
        if (slot == 0 && hasDistractors) return StudyMode.MultipleChoice;
        if (slot == 1) return StudyMode.Listening;
        if (hasUsableExample) return StudyMode.Cloze;
        return StudyMode.Type;
    }
}
