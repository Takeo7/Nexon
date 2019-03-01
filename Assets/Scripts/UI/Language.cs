using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Language : ScriptableObject {

	public language _language;

	public List<string> lines;
}
public enum language { Español, English, Deutsch, Français}
