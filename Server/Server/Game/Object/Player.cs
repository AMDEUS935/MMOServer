using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game.Object
{
	public class Player : GameObject
	{
		public ClientSession Session { get; set; }
		public VisionCube Vision { get; private set; }

		public Player()
		{
			ObjectType = GameObjectType.Player;
			Vision = new VisionCube(this);
		}

		public void OnLeaveGame()
		{
			// TODO: DB 저장 연동 시 여기에 추가
		}

		public override void OnDamaged(GameObject attacker, int damage)
		{
			base.OnDamaged(attacker, damage);
		}

		public override void OnDead(GameObject attacker)
		{
			base.OnDead(attacker);
		}
	}
}
