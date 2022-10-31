using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class PawnTable_Animals : PawnTable
	{
		protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
		{
			return from p in input
				orderby p.Name == null || p.Name.Numerical, p.RaceProps.petness, p.RaceProps.baseBodySize, (p.Name is NameSingle) ? ((NameSingle)p.Name).Number : 0, p.def.label
				select p;
		}

		public PawnTable_Animals(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
			: base(def, pawnsGetter, uiWidth, uiHeight)
		{
		}
	}
}
