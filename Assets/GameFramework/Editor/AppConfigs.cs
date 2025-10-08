using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AppConfigs", menuName = "AppConfigs [配置App运行时所需数据表、配置表、流程]")]
public class AppConfigs : ScriptableObject
{
    private static AppConfigs mInstance = null;

    [SerializeField] bool m_LoadFromBytes = false;
    public bool LoadFromBytes
    {
        get => m_LoadFromBytes;
        set => m_LoadFromBytes = value;
    }
    [Header("数据表")]
    [SerializeField] string[] mDataTables;
    public string[] DataTables => mDataTables;


    [Header("配置表")]
    [SerializeField] string[] mConfigs;
    public string[] Configs => mConfigs;

    private void Awake()
    {
        mInstance = this;
    }


#if UNITY_EDITOR
    /// <summary>
    /// 编辑器下获取实例
    /// </summary>
    /// <returns></returns>
    public static AppConfigs GetInstanceEditor()
    {
        if (mInstance == null)
        {
            mInstance = Resources.Load<AppConfigs>("AppConfigs");
        }
        return mInstance;
    }
#endif
    /// <summary>
    /// 运行时获取实例
    /// </summary>
    /// <returns></returns>
    public static AppConfigs GetInstanceSync()
    {
        if (mInstance == null)
        {
            mInstance = Resources.Load<AppConfigs>("AppConfigs");
        }
        return mInstance;
    }

}
