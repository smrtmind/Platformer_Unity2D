using System;

namespace PixelCrew.Model
{
    [Serializable]
    public class PlayerData
    {
        public int Coins;
        public int Hp;
        public bool IsArmed;
        public bool DoubleJumpIsActive;
        public bool DashIsActive;
        public bool OnWallHookIsActive;
    }
}
