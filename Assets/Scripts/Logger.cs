using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
	private Text text;

	private void Awake()
	{
		text = this.transform.GetChild(0).GetComponent<Text>();
	}

	public void Info(string message)
	{
		text.text = message;
	}

	public void Error(string message)
	{
		text.text = "<color=red>" + message + "</color>";
	}
}