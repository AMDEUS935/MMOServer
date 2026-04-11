using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game.Object
{
	public class Arrow : Projectile
	{
		public GameObject Owner { get; set; }

		public override void Update()
		{
			if (Data == null || Data.Projectile == null || Owner == null || Room == null)
				return;

			int tick = (int)(1000 / Data.Projectile.speed);
			Room.PushAfter(Update, tick);

			Vector2Int destPos = GetFrontCellPos();

			if (Room.Map.ApplyMove(this, destPos, collision: false))
			{
				S_Move movePacket = new S_Move();
				movePacket.ObjectId = id;
				movePacket.PosInfo = PosInfo;
				Room.Broadcast(CellPos, movePacket);
			}
			else
			{
				GameObject target = Room.Map.Find(destPos);

				if (target != null)
				{
					target.OnDamaged(this, Data.Damage + Owner.TotalAttack);
				}

				Room.Push(Room.LeaveGame, id);
			}
		}

		public override GameObject GetOwner()
		{
			return Owner;
		}
	}
}
