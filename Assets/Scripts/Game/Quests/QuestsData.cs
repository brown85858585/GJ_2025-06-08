using System.Collections;
using System.Collections.Generic;

namespace Game.Quests
{
	public class QuestsData
	{
		private KitchenQuest KitchenQuest { get; } = new();
		private SprintQuest SprintQuest { get; } = new();
		private FlowerQuest FlowerQuest { get; } = new();
		private WorkQuest WorkQuest { get; } = new();

		// public int DifficultyLevel { get; set; } = 1;

		public IEnumerable<Quest> GetQuests()
		{
			var quests = new List<Quest>
			{
				KitchenQuest,
				SprintQuest,
				FlowerQuest,
				WorkQuest
			};

			return quests;
		}
	}

	public class Quest
	{
		public QuestType Type { get; protected set; }
		public string Key { get; protected set; }	
		public bool IsCompleted { get; set; }
		public bool IsWin { get; set; } = false;
	}

	public class KitchenQuest : Quest
	{ 
		public KitchenQuest()
		{
			Type = QuestType.Kitchen;
			Key = "Quest_Kitchen0";
			IsCompleted = false;
			IsWin = false;
		}
	}

	public class SprintQuest : Quest
	{
		public SprintQuest()
		{
			Type = QuestType.Sprint;
			Key = "Quest_Door0";
			IsCompleted = false;
			IsWin = false;
		}
	}

	public class FlowerQuest : Quest
	{ 
		public FlowerQuest()
		{
			Type = QuestType.Flower;
			Key = "Quest_Flower0";
			IsCompleted = false;
			IsWin = false;
		}
	}

	public class WorkQuest : Quest
	{ 
		public WorkQuest()
		{
			Type = QuestType.Work;
			Key = "Quest_Computer0";
			IsCompleted = false;
			IsWin = false;
		}
	}

	public enum QuestType
	{
		Kitchen,
		Sprint,
		Flower,
		Work
	}
}