using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class GeneDefGenerator
	{
		public static IEnumerable<GeneDef> ImpliedGeneDefs()
		{
			if (!ModsConfig.BiotechActive)
			{
				yield break;
			}
			foreach (GeneTemplateDef g in DefDatabase<GeneTemplateDef>.AllDefs)
			{
				switch (g.geneTemplateType)
				{
				case GeneTemplateDef.GeneTemplateType.Skill:
					foreach (SkillDef allDef in DefDatabase<SkillDef>.AllDefs)
					{
						yield return GetFromTemplate(g, allDef, allDef.index * 1000);
					}
					break;
				case GeneTemplateDef.GeneTemplateType.Chemical:
					foreach (ChemicalDef allDef2 in DefDatabase<ChemicalDef>.AllDefs)
					{
						if (allDef2.generateAddictionGenes)
						{
							yield return GetFromTemplate(g, allDef2, allDef2.index * 1000);
						}
					}
					break;
				}
			}
		}

		private static GeneDef GetFromTemplate(GeneTemplateDef template, Def def, int displayOrderBase)
		{
			GeneDef geneDef = new GeneDef
			{
				defName = template.defName + "_" + def.defName,
				geneClass = template.geneClass,
				label = template.label.Formatted(def.label),
				iconPath = template.iconPath.Formatted(def.defName),
				description = ResolveDescription(),
				labelShortAdj = template.labelShortAdj.Formatted(def.label),
				selectionWeight = template.selectionWeight,
				biostatCpx = template.biostatCpx,
				biostatMet = template.biostatMet,
				displayCategory = template.displayCategory,
				displayOrderInCategory = displayOrderBase + template.displayOrderOffset,
				minAgeActive = template.minAgeActive,
				modContentPack = template.modContentPack
			};
			if (!template.exclusionTagPrefix.NullOrEmpty())
			{
				geneDef.exclusionTags = new List<string> { template.exclusionTagPrefix + "_" + def.defName };
			}
			SkillDef skill;
			ChemicalDef chemical;
			if ((skill = def as SkillDef) != null)
			{
				if (template.aptitudeOffset != 0)
				{
					geneDef.aptitudes = new List<Aptitude>
					{
						new Aptitude(skill, template.aptitudeOffset)
					};
				}
				if (template.passionModType != 0)
				{
					geneDef.passionMod = new PassionMod(skill, template.passionModType);
				}
			}
			else if ((chemical = def as ChemicalDef) != null)
			{
				geneDef.chemical = chemical;
				geneDef.addictionChanceFactor = template.addictionChanceFactor;
			}
			return geneDef;
			string ResolveDescription()
			{
				if (template.geneClass == typeof(Gene_ChemicalDependency))
				{
					return template.description.Formatted(def.label, "PeriodDays".Translate(5f).Named("DEFICIENCYDURATION"), "PeriodDays".Translate(30f).Named("COMADURATION"), "PeriodDays".Translate(60f).Named("DEATHDURATION"));
				}
				return template.description.Formatted(def.label);
			}
		}
	}
}
