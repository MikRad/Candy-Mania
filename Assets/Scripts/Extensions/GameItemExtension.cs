using UnityEngine;

public static class GameItemExtension
{
    public static bool IsEqual(this GameItem gItem, GameItem gItemToCompare)
    {
        return gItem.ItemType == gItemToCompare.ItemType;
    }

    public static bool IsSame(this GameItem gItem, GameItem gItemToCompare)
    {
        return gItem.BaseItemType == gItemToCompare.BaseItemType;
    }

    public static bool IsSame(this GameItem gItem, GameItemType comparedGameItemBaseType)
    {
        return gItem.BaseItemType == comparedGameItemBaseType;
    }
    
    public static bool IsUsual(this GameItem gItem)
    {
        return gItem.ItemType < GameItemType.ItemBombOffset;
    }
    public static bool IsUsual(GameItemType type)
    {
        return type < GameItemType.ItemBombOffset;
    }

    public static bool IsSpecial(this GameItem gItem)
    {
        return !IsUsual(gItem);
    }

    public static bool IsSpecial(GameItemType type)
    {
        return !IsUsual(type);
    }

    public static bool IsExplosive(this GameItem gItem)
    {
        return (gItem.ItemType > GameItemType.ItemBombOffset) && (gItem.ItemType < GameItemType.ItemStarOffset);
    }

    public static bool IsStar(this GameItem gItem)
    {
        return (gItem.ItemType > GameItemType.ItemStarOffset);
    }

    public static bool IsStar(GameItemType type)
    {
        return (type > GameItemType.ItemStarOffset);
    }

    public static LevelPassCondition.Type GetPassConditionType(this GameItem gItem)
    {
        if (gItem.IsStar())
            return LevelPassCondition.Type.ItemStarCollect;

        return (LevelPassCondition.Type)gItem.BaseItemType;
    }

    public static bool IsUsualBomb(this GameItem gItem)
    {
        return (gItem.ItemType > GameItemType.ItemBombOffset) && (gItem.ItemType < GameItemType.ItemVertBombOffset);
    }

    public static bool IsUsualBomb(GameItemType type)
    {
        return (type > GameItemType.ItemBombOffset) && (type < GameItemType.ItemVertBombOffset);
    }

    public static bool IsVerticalBomb(this GameItem gItem)
    {
        return (gItem.ItemType > GameItemType.ItemVertBombOffset) && (gItem.ItemType < GameItemType.ItemHorBombOffset);
    }

    public static bool IsVerticalBomb(GameItemType type)
    {
        return (type > GameItemType.ItemVertBombOffset) && (type < GameItemType.ItemHorBombOffset);
    }

    public static bool IsHorizontalBomb(this GameItem gItem)
    {
        return (gItem.ItemType > GameItemType.ItemHorBombOffset) && (gItem.ItemType < GameItemType.ItemStarOffset);
    }

    public static bool IsHorizontalBomb(GameItemType type)
    {
        return (type > GameItemType.ItemHorBombOffset) && (type < GameItemType.ItemStarOffset);
    }

    public static GameItemType GetBaseType(GameItemType type)
    {
        int intType = ((int)(type)) % 10;
        return (GameItemType)intType;
    }

    public static GameItemType GetBaseType(this GameItem gItem)
    {
        int intType = ((int)(gItem.ItemType)) % 10;
        return (GameItemType)intType;
    }
    
    public static int GetBaseTypeInt(GameItemType type)
    {
        return ((int)(type)) % 10;
    }
    
    public static GameItemType GetUpgradedToBombType(this GameItem gItem)
    {
        int intType = (int)gItem.BaseItemType;
        intType += (int)GameItemType.ItemBombOffset;
        return (GameItemType)intType;
    }

    public static GameItemType GetUpgradedToLineBombType(this GameItem gItem)
    {
        int rndNum = Random.Range(0, 2);
        bool isVerticalBomb = (rndNum > 0);

        int intType = (int)gItem.BaseItemType;
        if (isVerticalBomb)
        {
            intType += (int)GameItemType.ItemVertBombOffset;
        }
        else
        {
            intType += (int)GameItemType.ItemHorBombOffset;
        }

        return (GameItemType)intType;
    }
}