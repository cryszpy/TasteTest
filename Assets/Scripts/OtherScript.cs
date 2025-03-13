// other script
using Interactions;
using UnityEngine;

public class OtherScript : MonoBehaviour {

    private Sonartake sonartakestuff;

    public void Start() {
        sonartakestuff = GameObject.FindObjectOfType<Sonartake>();

        sonartakestuff.isChecked = true;
    }
}