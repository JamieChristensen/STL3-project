using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EndScene : MonoBehaviour
{
    [SerializeField]
    private GameStats gameStats;

    [SerializeField]
    private TextMeshProUGUI text;


    private void Start()
    {
        string str = "Thanks for playing this demo project! Here are some stats about your playthrough:" +
        "\n" + "# of deaths: " + gameStats.deaths +
        "\n" + "# of kills: " + gameStats.kills +
        "\n" + "# of spears thrown: " + gameStats.spearsThrown +
        "\n" + "# of spear-redirects: " + gameStats.spearRedirects + 
        "\n" + "You can press T to go back to the first scene if you wish to. (Won't reset these stats)" + 
        "\n" + "Press Esc to close game." +
        "\n" + "- Jamie";

        text.text = str;
    }
}
