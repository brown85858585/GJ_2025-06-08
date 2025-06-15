using System.Collections;
using System.Collections.Generic;

namespace Game.Quests
{
	public class QuestsModel
	{
		public KitchenQuest KitchenQuest { get; private set; } = new();
		public SprintQuest SprintQuest { get; private set; } = new();
		public FlowerQuest FlowerQuest { get; private set; } = new();
		public WorkQuest WorkQuest { get; private set; } = new();

		public int DifficultyLevel { get; set; } = 1;

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

	public class KitchenQuest : Quest
	{ 
		public KitchenQuest()
		{
			Type = QuestType.Kitchen;
			Description = "Cook a meal in the kitchen.";
			IsCompleted = false;
		}
	}

	public class SprintQuest : Quest
	{
		public SprintQuest()
		{
			Type = QuestType.Sprint;
			Description = "Run around the house and collect items.";
			IsCompleted = false;
		}
	}

	public class FlowerQuest : Quest
	{ 
		public FlowerQuest()
		{
			Type = QuestType.Flower;
			Description = "Collect flowers in the garden.";
			IsCompleted = false;
		}
	}

	public class WorkQuest : Quest
	{ 
		public WorkQuest()
		{
			Type = QuestType.Work;
			Description = "Complete your work tasks.";
			IsCompleted = false;
		}
	}

	public class Quest
	{
		public QuestType Type { get; protected set; }
		public string Description { get; protected set; }	
		public bool IsCompleted { get; set; }
	}

	public enum QuestType
	{
		Kitchen,
		Sprint,
		Flower,
		Work
	}
}