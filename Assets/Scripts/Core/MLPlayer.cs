namespace Chess.Game {
	using System.Threading.Tasks;
	using System.Threading;
	using System.Collections.Generic;

	public class MLPlayer : Player {

		public MLPlayer (Board board) {
		}

		public override void Update () {

		}

		public void publicChoseMove(Move move) {
			ChoseMove(move);
		}

		public override void NotifyTurnToMove () {
		}
	}
}