using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using uDesktopDuplication;
using UnityEngine;
using UnityEngine.EventSystems;

public class LaserPointerMouse : MonoBehaviour {
    private GameObject laserBeam;
    private GameObject laserPointer;
    private float beamLength = 100;
    private bool isEnabled = false;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    [Flags]
    public enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        MiddleDown = 0x00000020,
        MiddleUp = 0x00000040,
        Move = 0x00000001,
        Absolute = 0x00008000,
        RightDown = 0x00000008,
        RightUp = 0x00000010
    }

    public static void MouseEvent(MouseEventFlags value, int x, int y)
    {
        mouse_event((int)value, x, y, 0, 0);
    }

    void Start () {
		laserBeam = GameObject.Find("LaserBeam");
        laserPointer = GameObject.Find("LaserPointer");
        laserBeam.transform.localScale = new Vector3(0.01f, 0, 0.01f);
        laserBeam.transform.localPosition = new Vector3(0, beamLength, 0);
    }
	
	void Update () {

	    if (OVRInput.GetDown(OVRInput.Button.Start))
	    {
	        isEnabled = !isEnabled;
            laserBeam.transform.localScale = new Vector3(0.01f, isEnabled ? beamLength : 0, 0.01f);
        }

	    if (isEnabled)
	    {
	        foreach (var uddTexture in GameObject.FindObjectsOfType<uDesktopDuplication.Texture>())
	        {
	            var result = uddTexture.RayCast(laserPointer.transform.position, laserBeam.transform.up*beamLength);
	            if (result.hit)
	            {
	                var x = (int) result.desktopCoord.x;
	                var y = (int) result.desktopCoord.y;
	                SetCursorPos(x, y);

	                // https://developer3.oculus.com/documentation/game-engines/latest/concepts/unity-ovrinput/#unity-ovrinput-touch
	                if (OVRInput.GetDown(OVRInput.Button.Three))
	                {
	                    MouseEvent(MouseEventFlags.LeftDown, x, y);
	                }
	                if (OVRInput.GetUp(OVRInput.Button.Three))
	                {
	                    MouseEvent(MouseEventFlags.LeftUp, x, y);
	                }
	                if (OVRInput.GetDown(OVRInput.Button.Four))
	                {
	                    MouseEvent(MouseEventFlags.RightDown, x, y);
	                }
	                if (OVRInput.GetUp(OVRInput.Button.Four))
	                {
	                    MouseEvent(MouseEventFlags.RightUp, x, y);
	                }
	            }
	        }
	    }

	}
}
