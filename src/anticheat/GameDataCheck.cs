using AmongUs.InnerNet.GameDataMessages;
using Hazel;

namespace MalumMenu.anticheat
{
    internal abstract class GameDataCheck : ICheck
    {
        protected const int HandlingId = 50002;

        public bool Enabled { get; set; } = true;

        public virtual void Validate(MessageReader reader, ref bool blockMessage) { }

        public abstract GameDataTypes GetGameDataType();
    }
}
