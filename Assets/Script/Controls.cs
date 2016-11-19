using UnityEngine;
using System.Collections;

public class Controls : MonoBehaviour
{
	public static KeyCode runButton = KeyCode.L;
	public static KeyCode attackModeButton = KeyCode.Return;
	public static KeyCode weaponModeButton = KeyCode.LeftControl;
	public static KeyCode plasmaModeButton = KeyCode.LeftShift;

	public static float getHorizontal()
	{
		return Input.GetAxisRaw("Horizontal");
	}

	public static float getVertical()
	{
		return Input.GetAxisRaw("Vertical");
	}

	public static bool isMoving()
	{
		if(getHorizontal() != 0 || getVertical() != 0)
		{
			return true;
		}

		return false;
	}
}
