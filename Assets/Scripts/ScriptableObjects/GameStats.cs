
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "GameStats", menuName = "ScriptableObject/GameStats")]
public class GameStats : ScriptableObject {
    public int spearsThrown;
    public int deaths;
    public int kills;
    public int spearRedirects;
}
