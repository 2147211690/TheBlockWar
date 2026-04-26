namespace TheBlockWar.Scripts.Resources;

public interface IInfo<TEnum> where TEnum : struct, Enum
{
    TEnum Name { get; set; }
}