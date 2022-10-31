using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class ShipJob_WaitSendable : ShipJob_Wait
	{
		public MapParent destination;

		protected override bool ShouldEnd => false;

		public override bool HasDestination => destination != null;

		public override IEnumerable<Gizmo> GetJobGizmos()
		{
			return Enumerable.Empty<Gizmo>();
		}

		protected override void SendAway()
		{
			ShipJob_FlyAway shipJob_FlyAway = (ShipJob_FlyAway)ShipJobMaker.MakeShipJob(ShipJobDefOf.FlyAway);
			shipJob_FlyAway.destinationTile = destination.Tile;
			shipJob_FlyAway.arrivalAction = new TransportPodsArrivalAction_TransportShip(destination, transportShip);
			shipJob_FlyAway.dropMode = TransportShipDropMode.None;
			transportShip.SetNextJob(shipJob_FlyAway);
			transportShip.TryGetNextJob();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref destination, "destination");
		}
	}
}
