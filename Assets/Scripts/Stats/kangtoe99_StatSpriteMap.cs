using System;
using UnityEngine;

[Serializable]
public sealed class kangtoe99_StatSpriteMap : kangtoe99_EnumMap<kangtoe99_StatType, Sprite>
{
    public kangtoe99_StatSpriteMap() : base() { }
    public kangtoe99_StatSpriteMap(Func<kangtoe99_StatType, Sprite> initializer) : base(initializer) { }
}
