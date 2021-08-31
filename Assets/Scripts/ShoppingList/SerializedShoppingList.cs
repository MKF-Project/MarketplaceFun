using System.Collections.Generic;
using MLAPI.Serialization;

public struct SerializedShoppingList : INetworkSerializable
{
    public ShoppingListItem[] Array;


    public SerializedShoppingList(List<ShoppingListItem> list)
    {
        Array = list.ToArray();
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        // Length
        int length = 0;
        if (!serializer.IsReading)
        {
            length = Array.Length;
        }

        serializer.Serialize(ref length);

        // Array
        if (serializer.IsReading)
        {
            Array = new ShoppingListItem[length];
        }

        for (int n = 0; n < length; ++n)
        {
            Array[n].NetworkSerialize(serializer);
        }
    }
}
