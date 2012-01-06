using UnityEngine;
using System.Collections;

public class DisplayBar : MonoBehaviour {

    public float currentRatio;

    public Texture2D healthBarBackground;
    public Texture2D healthBar;
    public float verticalOffset = 5;

    public bool constructed = false;

    private float healthBarWidth;
    private Rect healthBarBGRect;
    private Rect healthBarRect;

    public void Awake () {
        Vector3 diag = Vector3.zero;
        diag.x = collider.bounds.extents.x;
        diag.z = collider.bounds.extents.z;
        healthBarWidth = diag.magnitude;

        currentRatio = 1.0f;

        enabled = false;
    }

    void OnConstructionFinished () {
        constructed = true;
    }

    void OnDying () {
        constructed = false;
        enabled = false;
    }

    void OnSelected (bool v) {
        if (constructed) {
            enabled = v;
        }
    }

    void LateUpdate () {
        Vector3 right = Camera.main.transform.right * healthBarWidth;
        Vector3 up = Camera.main.transform.up * 0.75f;

        Vector3 center = collider.bounds.center;
        center += Camera.main.transform.up * verticalOffset;

        Vector3 min = center - right - up;
        Vector3 max = center + right + up;
        
        Vector3 ll = Camera.main.WorldToScreenPoint (min);
        Vector3 ur = Camera.main.WorldToScreenPoint (max);

        float min_x = (ll.x < ur.x ? ll.x : ur.x);
        float max_y = (ll.y > ur.y ? ll.y : ur.y);

        healthBarBGRect = new Rect (min_x, Screen.height - max_y,
            Mathf.Abs (ur.x - ll.x), Mathf.Abs (ur.y - ll.y));
        healthBarRect = new Rect ();
        healthBarRect.x = healthBarBGRect.x + 1;
        healthBarRect.y = healthBarBGRect.y + 1;
        healthBarRect.width = (healthBarBGRect.width - 2) * currentRatio;
        healthBarRect.height = healthBarBGRect.height - 2;
        
    }

    void OnGUI () {
        GUI.DrawTexture (healthBarBGRect, healthBarBackground, 
            ScaleMode.StretchToFill, true);
        GUI.DrawTexture (healthBarRect, healthBar, 
            ScaleMode.StretchToFill, true);
    }

};
