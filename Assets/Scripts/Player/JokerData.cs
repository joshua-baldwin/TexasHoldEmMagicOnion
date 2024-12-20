using System;
using System.Collections.Generic;
using System.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;

namespace THE.Player
{
    public class JokerData
    {
        public Guid UniqueId { get; set; }
        public int JokerId { get; set; }
        public int BuyCost { get; set; }
        public int UseCost { get; set; }
        public int MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public List<JokerAbilityEntity> JokerAbilities { get; set; }
        public bool CanUse { get; set; }
        public Enums.JokerTypeEnum JokerType { get; set; }
        public Enums.TargetTypeEnum TargetType { get; set; }
        
        public JokerData(JokerEntity joker)
        {
            UniqueId = joker.UniqueId;
            JokerId = joker.JokerId;
            BuyCost = joker.BuyCost;
            UseCost = joker.UseCost;
            MaxUses = joker.MaxUses;
            CurrentUses = joker.CurrentUses;
            JokerAbilities = joker.JokerAbilityEntities;
            CanUse = joker.CanUse;
            JokerType = joker.JokerType;
            TargetType = joker.TargetType;
        }
    }
}