using AmongUs.InnerNet.GameDataMessages;
using Hazel;

namespace MalumMenu.anticheat
{
    internal abstract class GameDataCheck : ICheck
    {
        public bool Enabled { get; set; } = true;

        public virtual void Validate(MessageReader reader, ref bool blockMessage) { }

        public abstract GameDataTypes GetGameDataType();
    }
}
