using Verse;

namespace RimWorld
{
	public class ThoughtWorker_GeneticChemicalDependency : ThoughtWorker
	{
		public override string PostProcessDescription(Pawn p, string description)
		{
			float num = 0f;
			Gene_ChemicalDependency gene_ChemicalDependency = null;
			foreach (Gene item in p.genes.GenesListForReading)
			{
				Gene_ChemicalDependency gene_ChemicalDependency2;
				if ((gene_ChemicalDependency2 = item as Gene_ChemicalDependency) != null && gene_ChemicalDependency2.LinkedHediff != null && (gene_ChemicalDependency == null || gene_ChemicalDependency2.LinkedHediff.Severity > num))
				{
					num = gene_ChemicalDependency2.LinkedHediff.Severity;
					gene_ChemicalDependency = gene_ChemicalDependency2;
				}
			}
			return base.PostProcessDescription(p, description.Formatted(gene_ChemicalDependency.def.chemical.Named("CHEMICAL")));
		}

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!ModsConfig.BiotechActive || p.genes == null)
			{
				return ThoughtState.Inactive;
			}
			if (p.genes.GetFirstGeneOfType<Gene_ChemicalDependency>() == null)
			{
				return ThoughtState.Inactive;
			}
			float num = 0f;
			foreach (Gene item in p.genes.GenesListForReading)
			{
				Gene_ChemicalDependency gene_ChemicalDependency;
				if ((gene_ChemicalDependency = item as Gene_ChemicalDependency) != null && gene_ChemicalDependency.LinkedHediff != null)
				{
					num += gene_ChemicalDependency.LinkedHediff.Severity;
				}
			}
			if (num < 1f)
			{
				return ThoughtState.Inactive;
			}
			return true;
		}
	}
}
