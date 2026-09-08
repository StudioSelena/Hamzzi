using Unity.AI.Navigation;
using UnityEngine;

public class NavigationManager : SingletonBase<NavigationManager>
{
    [SerializeField] private NavMeshSurface NavMeshSurface;

    public void BuildNav()
    {
        NavMeshSurface.BuildNavMesh();
    }
}