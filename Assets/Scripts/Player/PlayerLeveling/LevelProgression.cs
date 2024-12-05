public class LevelProgression
{
    public int CurrentLevel { get; private set; } = 1;
    public float NeededExperience { get; private set; } = 10;
    public float CurrentExperience { get; private set; } = 0;

    public bool AddExperience(float experience)
    {
        CurrentExperience += experience;
        if (CurrentExperience >= NeededExperience)
        {
            CurrentLevel++;
            CurrentExperience = 0;
            NeededExperience = (CurrentLevel ^ 20) + (CurrentLevel * 20); // Level progression formula
            return true; // Indicating a level up
        }
        return false; // No level up
    }
}

