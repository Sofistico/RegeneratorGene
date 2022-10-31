using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PreceptComp_SelfTookMemoryThought : PreceptComp_Thought
	{
		public HistoryEventDef eventDef;

		public bool onlyForNonSlaves;

		public override IEnumerable<TraitRequirement> TraitsAffecting => ThoughtUtility.GetNullifyingTraits(thought);

		public override void Notify_MemberTookAction(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts)
		{
			if (ev.def != eventDef || !canApplySelfTookThoughts)
			{
				return;
			}
			Pawn arg = ev.args.GetArg<Pawn>(HistoryEventArgsNames.Doer);
			if (arg.needs != null && arg.needs.mood != null && (!onlyForNonSlaves || !arg.IsSlave) && (thought.minExpectation == null || ExpectationsUtility.CurrentExpectationFor(arg).order >= thought.minExpectation.order))
			{
				Thought_Memory thought_Memory = ThoughtMaker.MakeThought(thought, precept);
				Thought_KilledInnocentAnimal thought_KilledInnocentAnimal;
				if ((thought_KilledInnocentAnimal = thought_Memory as Thought_KilledInnocentAnimal) != null && ev.args.TryGetArg(HistoryEventArgsNames.Victim, out Pawn arg2))
				{
					thought_KilledInnocentAnimal.SetAnimal(arg2);
				}
				Thought_MemoryObservation thought_MemoryObservation;
				if ((thought_MemoryObservation = thought_Memory as Thought_MemoryObservation) != null && ev.args.TryGetArg(HistoryEventArgsNames.Subject, out Corpse arg3))
				{
					thought_MemoryObservation.Target = arg3;
				}
				arg.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
			}
		}
	}
}
