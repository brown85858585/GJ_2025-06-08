using System;

namespace CharacterSelect
{
    public sealed class CharacterModel
    {
        public CharacterData Data { get; }
        public int Level { get; private set; }
        public int Xp    { get; private set; }

        public event Action<int,int> OnXpChanged;
    
        public CharacterModel(CharacterData data, int level, int xp)
        {
            Data = data;
            Level = level;
            Xp    = xp;
        }

        public void AddXp(int amount)
        {
            Xp += amount;
            
            while (Xp >= GetXpToNextLevel() && Level < Data.maxLevel)
            {
                Xp -= GetXpToNextLevel();
                Level++;
            }
            OnXpChanged?.Invoke(Xp, Level);
        }

        public int GetXpToNextLevel() => Data.baseXp + Level * 50;
    }
}