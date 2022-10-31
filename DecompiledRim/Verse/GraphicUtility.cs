using System;
using RimWorld;

namespace Verse
{
	public static class GraphicUtility
	{
		public static Graphic ExtractInnerGraphicFor(this Graphic outerGraphic, Thing thing, int? indexOverride = null)
		{
			Graphic_RandomRotated graphic_RandomRotated = outerGraphic as Graphic_RandomRotated;
			if (graphic_RandomRotated != null)
			{
				return ResolveGraphicInner(graphic_RandomRotated.SubGraphic);
			}
			return ResolveGraphicInner(outerGraphic);
			Graphic ResolveGraphicInner(Graphic g)
			{
				Graphic_Random graphic_Random = g as Graphic_Random;
				if (graphic_Random != null)
				{
					if (indexOverride.HasValue)
					{
						return graphic_Random.SubGraphicAtIndex(indexOverride.Value);
					}
					return graphic_Random.SubGraphicFor(thing);
				}
				Graphic_Appearances graphic_Appearances = g as Graphic_Appearances;
				if (graphic_Appearances != null)
				{
					return graphic_Appearances.SubGraphicFor(thing);
				}
				Graphic_Genepack graphic_Genepack = g as Graphic_Genepack;
				if (graphic_Genepack != null)
				{
					return graphic_Genepack.SubGraphicFor(thing);
				}
				Graphic_MealVariants graphic_MealVariants = g as Graphic_MealVariants;
				if (graphic_MealVariants != null)
				{
					return graphic_MealVariants.SubGraphicFor(thing);
				}
				return g;
			}
		}

		public static Graphic_Linked WrapLinked(Graphic subGraphic, LinkDrawerType linkDrawerType)
		{
			return linkDrawerType switch
			{
				LinkDrawerType.None => null, 
				LinkDrawerType.Basic => new Graphic_Linked(subGraphic), 
				LinkDrawerType.CornerFiller => new Graphic_LinkedCornerFiller(subGraphic), 
				LinkDrawerType.Transmitter => new Graphic_LinkedTransmitter(subGraphic), 
				LinkDrawerType.TransmitterOverlay => new Graphic_LinkedTransmitterOverlay(subGraphic), 
				LinkDrawerType.Asymmetric => new Graphic_LinkedAsymmetric(subGraphic), 
				_ => throw new ArgumentException(), 
			};
		}
	}
}
