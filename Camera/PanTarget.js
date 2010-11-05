// Copyright (c) 2010 Nathan Fabian

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// these values must be set based on the size of the playing field
public var panSpeedKey : float = 100.0;
public var panSpeedMouseScroll : float = 100.0;
public var panSpeedMouseMove : float = 1.0;

// this is the border within which the mouse will start scrolling 
// the window.  it's nice not to have it be more than 0, but it'll
// work with just zero
public var horizontalBorder : float = 10.0;
public var verticalBorder : float = 10.0;

// min and max position are normally set by the GridManager
// which determines the extent of the playing field.
public var minPan : Vector3; 
public var maxPan : Vector3;

// Update is called once per frame
function Update () {
    if (Input.GetButton ("Alt")) {
        // In RisingStorm there is a buildPanel along the left side
        // this recenters the mouse position to the viewable space
        // that's inside the buildPanel.  For panels which are 
        // on the bottom, we'd need to modify height
        // var buildPanelWidth = BuildPanel.GetInstance ().Width ();
        var buildPanelWidth = 0.0;
        var mid_x = (Screen.width - buildPanelWidth) * 0.5f + buildPanelWidth;
        var mid_y = Screen.height * 0.5f;

        var m = Input.mousePosition;
        panTarget (transform.right * (m.x - mid_x), panSpeedMouseMove);
        panTarget (transform.forward * (m.y - mid_y), panSpeedMouseMove);
    } else if (!Input.GetMouseButton (2) && !Input.GetButton ("Command")) {
        var rightLimit = Screen.width - horizontalBorder;
        var topLimit = Screen.height - verticalBorder;

        if (Input.mousePosition.x <= horizontalBorder) {
            panTarget (-transform.right, panSpeedMouseScroll);
        } else if (Input.mousePosition.x >= rightLimit) {
            panTarget (transform.right, panSpeedMouseScroll);
        }
        if (Input.mousePosition.y <= verticalBorder) {
            panTarget (-transform.forward, panSpeedMouseScroll);
        } else if (Input.mousePosition.y >= topLimit) {
            panTarget (transform.forward, panSpeedMouseScroll);
        }
    }
    panTarget (transform.right * Input.GetAxis ("Horizontal"), panSpeedKey);
    panTarget (transform.forward * Input.GetAxis ("Vertical"), panSpeedKey);	
}

function panTarget (dir : Vector3, speed : float) {
    transform.position += dir * speed * Time.deltaTime;
    transform.position.x = Mathf.Clamp (transform.position.x, minPan.x, maxPan.x);
    transform.position.z = Mathf.Clamp (transform.position.z, minPan.z, maxPan.z); 
}
