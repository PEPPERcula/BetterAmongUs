namespace BetterAmongUs.Modules.OptionItems;

internal sealed class OptionPercentItem : OptionFloatItem
{
    internal static OptionPercentItem Create(int id, OptionTab tab, string tranStr, float defaultValue, OptionItem parent = null)
    {
        if (GetOptionById(id) is OptionPercentItem floatItem)
        {
            floatItem.CreateBehavior();
            return floatItem;
        }

        OptionPercentItem Item = new();
        AllTBROptions.Add(Item);
        Item._id = id;
        Item.Tab = tab;
        Item.Translation = tranStr;
        Item.Increment = 5;
        Item.Range = new FloatRange(0f, 100f);
        Item.DefaultValue = defaultValue;
        Item.Fixs = ("", "");

        if (parent != null)
        {
            Item.Parent = parent;
            parent.Children.Add(Item);
        }

        Item.CreateBehavior();
        return Item;
    }

    internal sealed override string ValueAsString() => $"<color={GetColor(Value)}>{Value}%</color>";

    internal string GetColor(float num)
    {
        switch (num)
        {
            case float n when n <= 0f:
                return "#ff0600";
            case float n when n <= 25f:
                return "#ff9d00";
            case float n when n <= 50f:
                return "#fff900";
            case float n when n <= 75f:
                return "#80ff00";
            default:
                return "#80ff00";
        }
    }
}
