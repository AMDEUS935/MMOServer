using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Server.Game.Object
{
	public class Monster : GameObject
	{
		public int TemplateId { get; private set; }

		public Monster()
		{
			ObjectType = GameObjectType.Monster;
		}

		public void Init(int templateId)
		{
			TemplateId = templateId;

			// TEMP: MonsterData 미구현, 임시 스탯 직접 설정
			Stat.Level = 1;
			Stat.Hp = 100;
			Stat.MaxHp = 100;
			Stat.Attack = 5;
			Stat.Speed = 5.0f;
			State = CreatureState.Idle;
		}

		// FSM (Finite State Machine)
		public override void Update()
		{
			switch (State)
			{
				case CreatureState.Idle:
					UpdateIdle();
					break;
				case CreatureState.Moving:
					UpdateMoving();
					break;
				case CreatureState.Skill:
					UpdateSkill();
					break;
				case CreatureState.Dead:
					UpdateDead();
					break;
			}
		}

		Player _target;
		int _searchCellDist = 10;
		int _chaseCellDist = 20;

		long _nextSearchTick = 0;
		protected virtual void UpdateIdle()
		{
			if (_nextSearchTick > Environment.TickCount64)
				return;
			_nextSearchTick = Environment.TickCount64 + 1000;

			Player target = Room.FindPlayer(p =>
			{
				Vector2Int dir = p.CellPos - CellPos;
				return dir.cellDistFromZero <= _searchCellDist;
			});

			if (target == null)
				return;

			_target = target;
			State = CreatureState.Moving;
		}

		int _skillRange = 1;
		long _nextMoveTick = 0;
		protected virtual void UpdateMoving()
		{
			if (_nextMoveTick > Environment.TickCount64)
				return;
			int moveTick = (int)(1000 / Speed);
			_nextMoveTick = Environment.TickCount64 + moveTick;

			if (_target == null || _target.Room != Room)
			{
				_target = null;
				State = CreatureState.Idle;
				BroadcastMove();
				return;
			}

			Vector2Int dir = _target.CellPos - CellPos;
			int dist = dir.cellDistFromZero;

			if (dist == 0 || dist > _chaseCellDist)
			{
				_target = null;
				State = CreatureState.Idle;
				BroadcastMove();
				return;
			}

			List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: false);
			if (path.Count < 2 || path.Count > _chaseCellDist)
			{
				_target = null;
				State = CreatureState.Idle;
				BroadcastMove();
				return;
			}

			// 스킬로 넘어갈지 체크
			if(dist <= _skillRange && (dir.x == 0 || dir.y == 0))
			{
				_coolTick = 0;
				State = CreatureState.Skill;
				return;
			}

			// 이동
			Dir = GetDirFromVec(path[1] - CellPos);
			Room.Map.ApplyMove(this, path[1]);
			BroadcastMove();
		}

		void BroadcastMove()
		{
			// 다른 플레이어한테도 알려준다
			S_Move movePacket = new S_Move();
			movePacket.ObjectId = id;
			movePacket.PosInfo = PosInfo;
			Room.Broadcast(CellPos, movePacket);
		}

		long _coolTick = 0;

		protected virtual void UpdateSkill()
		{
			if (_coolTick == 0)
			{
				// 유효한 타겟인지
				if (_target == null || _target.Room != Room || _target.hp == 0)
				{
					_target = null;
					State = CreatureState.Moving;
					BroadcastMove();
					return;
				}

				// 스킬이 아직 사용 가능한지
				Vector2Int dir = _target.CellPos - CellPos;
				int dist = dir.cellDistFromZero;
				bool canUseSkill = dist <= _skillRange && (dir.x == 0 || dir.y == 0);
				if (!canUseSkill)
				{
					State = CreatureState.Moving;
					BroadcastMove();
					return;
				}

				// 타게팅 방향 주시
				MoveDir lookDir = GetDirFromVec(dir);
				if (Dir != lookDir)
				{
					Dir = lookDir;
					BroadcastMove();
				}
				Skill skillData = null;
				DataManager.SkillDict.TryGetValue(1, out skillData);

				// 데미지 판정
				_target.OnDamaged(this, skillData.Damage + Stat.Attack);

				// 스킬 사용 Broadcast
				S_Skill skill = new S_Skill() { Info = new Skill_Info() };
				skill.ObjectId = id;
				skill.Info.SkillId = skillData.Id;
				Room.Broadcast(CellPos, skill);

				// 스킬 쿨타임
				int coolTick = (int)(1000 * skillData.Cooldown);
				_coolTick = Environment.TickCount64 + coolTick;
			}
			if (_coolTick > Environment.TickCount64)
				return;

			_coolTick = 0;
		}

		protected virtual void UpdateDead()
		{

		}
	}
}