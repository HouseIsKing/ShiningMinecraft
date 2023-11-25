﻿namespace MinecraftLibrary.Engine.Blocks;

public sealed class AirBlock() : Block(BlockType.Air)
{
    public override bool IsSolid()
    {
        return false;
    }
}