using StudyBuddies.Core.Data;

namespace StudyBuddies.Core.Services;

public enum ReviewGrade
{
    Bad = 1,
    Hard = 3,
    Good = 4,
    Easy = 5
}

public static class Sm2Service
{
    public static Review Apply(Review review, ReviewGrade grade, DateTime now)
    {
        var q = (int)grade;

        if (q < 3)
        {
            review.Repetitions = 0;
            review.Interval = 1;
        }
        else
        {
            review.Repetitions += 1;
            review.Interval = review.Repetitions switch
            {
                1 => 1,
                2 => 6,
                _ => (int)Math.Ceiling(review.Interval * review.EaseFactor)
            };
        }

        var newEase = review.EaseFactor + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
        review.EaseFactor = Math.Max(1.3, newEase);

        review.LastReview = now;
        review.DueDate = now.Date.AddDays(review.Interval);

        return review;
    }
}
