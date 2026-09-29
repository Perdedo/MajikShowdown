using UnityEditor;
using UnityEngine;

public static class SetNodePrices
{
    [MenuItem("Tools/Runes/Set Node Prices")]
    public static void SetPrices()
    {
        string[] guids = AssetDatabase.FindAssets("t:SpellNode");

        int rustyCount = 0;
        int forgedCount = 0;
        int factoryNewCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpellNode node = AssetDatabase.LoadAssetAtPath<SpellNode>(path);

            if (node == null)
                continue;

            // Safety in case an older asset doesn't have Price initialized.
            if (node.price == null)
                node.price = new SimpleInt();

            switch (node.quality)
            {
                case SpellNode.Quality.Rusty:
                    SetRandomPrice(node.price, 350, 500);
                    rustyCount++;
                    break;

                case SpellNode.Quality.Forged:
                    SetRandomPrice(node.price, 900, 1500);
                    forgedCount++;
                    break;

                case SpellNode.Quality.Refined:
                    SetRandomPrice(node.price, 2000, 3500);
                    factoryNewCount++;
                    break;
            }

            EditorUtility.SetDirty(node);
        }

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Rune prices updated. " +
            $"Rusty: {rustyCount} | " +
            $"Forged: {forgedCount} | " +
            $"Factory New: {factoryNewCount}"
        );
    }

    private static void SetRandomPrice(SimpleInt price, int min, int max)
    {
        price.type = SimpleVar.ValueType.Random;
        price.min = min;
        price.max = max;
    }
}