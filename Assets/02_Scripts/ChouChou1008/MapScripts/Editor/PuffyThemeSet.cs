using UnityEngine;

/// 6개 맵 테마 묶음.
/// 무거운 에셋(스카이박스·텍스처·소품)은 직접 참조하지 않고 Resources/PuffyWorld/{name}/ 에서
/// 필요할 때만 불러옴 → 현재 맵의 테마만 메모리에 올라감 (모바일 메모리 절약)
[CreateAssetMenu(menuName = "Puffy World/Theme Set")]
public class PuffyThemeSet : ScriptableObject
{
    public const string ResRoot = "PuffyWorld";

    [System.Serializable]
    public class Theme
    {
        [Tooltip("Resources/PuffyWorld/ 아래 폴더 이름")]
        public string name;
        public Color edgeColor = Color.white;
        [Header("조명")]
        public Color lightColor = Color.white;
        public Vector3 lightEuler = new Vector3(50, -30, 0);
        public Color fogColor = Color.white;

        public Material LoadSkybox() => Resources.Load<Material>($"{ResRoot}/{name}/Skybox");
        public Material LoadGround() => Resources.Load<Material>($"{ResRoot}/{name}/Ground");
        public Material LoadClay() => Resources.Load<Material>($"{ResRoot}/{name}/Clay");
        public GameObject[] LoadProps() => Resources.LoadAll<GameObject>($"{ResRoot}/{name}/Props");
    }

    public Theme[] themes;
}
