﻿using UnityEngine;

namespace TouhouPrideGameJam5.SO
{
    [CreateAssetMenu(menuName = "ScriptableObject/PlayerInfo", fileName = "PlayerInfo")]
    public class PlayerInfo : ScriptableObject
    {
        public float LinearSpeed, AngularSpeed;
        public float BulletForce;
        public int BaseHealth;
        public float ShootReloadTime;
        public float SkillReloadTime;
        public float DecoyLifetime;
    }
}