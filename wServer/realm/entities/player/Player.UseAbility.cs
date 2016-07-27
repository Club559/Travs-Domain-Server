using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wServer.networking;
using wServer.networking.svrPackets;

namespace wServer.realm.entities
{
    partial class Player
    {
        private void HandleCooldowns(RealmTime t)
        {
            bool changed = false;
            for (int i = 0; i < 3; i++)
            {
                int prevCooldown = AbilityCooldown[i];

                if (AbilityCooldown[i] > 0)
                    AbilityCooldown[i] = AbilityCooldown[i] - t.thisTickTimes;
                if (AbilityCooldown[i] < 0)
                    AbilityCooldown[i] = 0;

                if (AbilityCooldown[i] != prevCooldown)
                    changed = true;
            }
			if(changed)
				UpdateCount++;
        }

        private void ActivateAbilityShoot(RealmTime time, Ability ability, Position target)
        {
            double arcGap = ability.ArcGap * Math.PI / 180;
            double startAngle = Math.Atan2(target.Y - Y, target.X - X) - (ability.NumProjectiles - 1) / 2 * arcGap;
            ProjectileDesc prjDesc = ability.Projectiles[0]; //Assume only one

            var batch = new Packet[ability.NumProjectiles];
            for (int i = 0; i < ability.NumProjectiles; i++)
            {
                Projectile proj = CreateProjectile(prjDesc, ObjectDesc.ObjectType,
                    (int)statsMgr.GetAttackDamage(prjDesc.MinDamage, prjDesc.MaxDamage),
                    time.tickTimes, new Position { X = X, Y = Y }, (float)(startAngle + arcGap * i), 1);
                Owner.EnterWorld(proj);
                fames.Shoot(proj);
                batch[i] = new ShootPacket
                {
                    BulletId = proj.ProjectileId,
                    OwnerId = Id,
                    ContainerType = ability.AbilityType,
                    Position = new Position { X = X, Y = Y },
                    Angle = proj.Angle,
                    Damage = (short)proj.Damage,
                    FromAbility = true
                };
            }
            BroadcastSync(batch, p => this.Dist(p) < 25);
        }

        public void UseAbility(RealmTime time, int abilitySlot, Position target)
        {
            if (Ability[abilitySlot] == null)
                return;
            var ability = Ability[abilitySlot];
            if (MP < ability.MpCost || AbilityCooldown[abilitySlot] != 0)
                return;
            MP -= ability.MpCost;
            AbilityCooldown[abilitySlot] = ability.Cooldown;
            foreach (ActivateEffect eff in ability.ActivateEffects)
            {
                switch (eff.Effect)
                {
                    case ActivateEffects.BulletNova:
                        {
                            ProjectileDesc prjDesc = ability.Projectiles[0]; //Assume only one
                            var batch = new Packet[21];
                            uint s = Random.CurrentSeed;
                            Random.CurrentSeed = (uint)(s * time.tickTimes);
                            for (int i = 0; i < 20; i++)
                            {
                                Projectile proj = CreateProjectile(prjDesc, ability.AbilityType,
                                    (int)statsMgr.GetAttackDamage(prjDesc.MinDamage, prjDesc.MaxDamage),
                                    time.tickTimes, target, (float)(i * (Math.PI * 2) / 20));
                                Owner.EnterWorld(proj);
                                fames.Shoot(proj);
                                batch[i] = new ShootPacket
                                {
                                    BulletId = proj.ProjectileId,
                                    OwnerId = Id,
                                    ContainerType = ability.AbilityType,
                                    Position = target,
                                    Angle = proj.Angle,
                                    Damage = (short)proj.Damage,
									FromAbility = true
                                };
                            }
                            Random.CurrentSeed = s;
                            batch[20] = new ShowEffectPacket
                            {
                                EffectType = EffectType.Trail,
                                PosA = target,
                                TargetId = Id,
                                Color = new ARGB(0xFFFF00AA)
                            };
                            BroadcastSync(batch, p => this.Dist(p) < 25);
                        } break;
                    case ActivateEffects.Shoot:
                        {
                            ActivateAbilityShoot(time, ability, target);
                        } break;
                }
            }
            UpdateCount++;
        }
    }
}
