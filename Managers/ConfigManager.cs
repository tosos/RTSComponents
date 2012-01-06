using UnityEngine;
using System.Collections;

public class ConfigManager : MonoBehaviour {
    public LayerMask geyserLayer;
    public LayerMask depotLayer;

    static private ConfigManager instance = null;
    static public ConfigManager GetInstance () {
        if (instance == null) {
            instance = (ConfigManager) FindObjectOfType(typeof(ConfigManager));
        }
        return instance;
    }

    public float small = 1e-4f;
    public bool near (float a, float b) {
        return a > (b - small) && a < (b + small);
    }
}
