using System;
using UnityEngine;

public class RunStatsService
{
    private readonly int pointsForExtraHeart;
    private readonly int maxReviveCost;

    private int nextHeartThreshold;

    public event Action<int> ScoreChanged;

    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public int AvailableHearts { get; private set; }
    public int ReviveCount { get; private set; }

    public RunStatsService(int pointsForExtraHeart, int maxReviveCost, int bestScore)
    {
        this.pointsForExtraHeart = Mathf.Max(1, pointsForExtraHeart);
        this.maxReviveCost = Mathf.Max(1, maxReviveCost);
        BestScore = Mathf.Max(0, bestScore);
        ResetSession();
    }

    public void ResetSession()
    {
        Score = 0;
        AvailableHearts = 0;
        ReviveCount = 0;
        nextHeartThreshold = pointsForExtraHeart;
        ScoreChanged?.Invoke(Score);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        ScoreChanged?.Invoke(Score);

        while (Score >= nextHeartThreshold)
        {
            AvailableHearts++;
            nextHeartThreshold += pointsForExtraHeart;
        }

        if (Score > BestScore)
        {
            BestScore = Score;
        }
    }

    public int GetCurrentReviveCost()
    {
        return Mathf.Min(ReviveCount + 1, maxReviveCost);
    }

    public bool TrySpendHeartsForRevive()
    {
        int reviveCost = GetCurrentReviveCost();
        if (AvailableHearts < reviveCost)
        {
            return false;
        }

        AvailableHearts -= reviveCost;
        ReviveCount++;
        return true;
    }
}
