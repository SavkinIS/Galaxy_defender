using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    
    public void SetData(LevelDataSO.CellData cellData, Color color)
    {
        if (cellData.ColorIndex == LevelDataSO.CellData.Empty)
        {
            gameObject.SetActive(false);
            return;
        }
            
        gameObject.SetActive(false);
        _meshRenderer.material.color = color;
    }
}