using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    [Header("Map Template")]
    public Transform map;
    public Transform StartConnector;
    public Transform EndConnector;

    [Header("Generation")]
    public int duplicates = 10;
    public Transform generationParent;

    private void Awake()
    {
        GenerateRow();
    }

    private void GenerateRow()
    {
        if (map == null)
        {
            Debug.LogWarning("GenerateMap: No map template assigned.");
            return;
        }

        if (duplicates < 1)
        {
            duplicates = 1;
        }

        Transform parentTransform = generationParent != null ? generationParent : (map.parent != null ? map.parent : transform);
        Transform previousPiece = null;
        Vector3 previousEndPosition = Vector3.zero;

        for (int i = 0; i < duplicates; i++)
        {
            Transform piece = Instantiate(map, parentTransform, false);
            piece.name = map.name + "_segment_" + (i + 1);

            Transform startConnector = FindConnector(piece, StartConnector);
            Transform endConnector = FindConnector(piece, EndConnector);

            if (startConnector == null || endConnector == null)
            {
                Debug.LogWarning("GenerateMap: Could not find connector transforms on the map template.");
                continue;
            }

            if (previousPiece == null)
            {
                piece.position = map.position;
                piece.rotation = map.rotation;
            }
            else
            {
                Vector3 startOffset = startConnector.position - piece.position;
                piece.position = previousEndPosition - startOffset;
            }

            previousPiece = piece;
            previousEndPosition = endConnector.position;
        }
    }

    private Transform FindConnector(Transform piece, Transform templateConnector)
    {
        if (piece == null || templateConnector == null)
        {
            return null;
        }

        Transform connector = piece.Find(templateConnector.name);
        if (connector != null)
        {
            return connector;
        }

        return FindInChildren(piece, templateConnector.name);
    }

    private Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            Transform found = FindInChildren(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
