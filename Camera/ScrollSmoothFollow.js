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

// horizontal distance component to the target
var minDistance : float = 25;
var maxDistance : float = 250;

// vertical distance component to the target
// note by making the minHeight smaller than minDistance
// the camera will become less top down as it zooms in
// on the other hand, by allowing the maxHieght to go
// much higher than maxDistance  It will allow the camera
// to become much more top down
var minHeight : float = 10;
var maxHeight : float = 250;

var loweringSpeed : float = 100;

var moveSensitivity : float = 10;
var wheelSensitivity : float = 100;

private var smoothFollow;
private var ratio;

function Awake () {
    smoothFollow = GetComponent (SmoothFollow);
    ratio = (maxDistance - minDistance) / (maxHeight - minHeight); 
}

function Update () {
    var vertical = 0.0;
    if (Input.GetMouseButton (2) || Input.GetButton ("Command")) {
        vertical = Input.GetAxis ("Mouse Y") * moveSensitivity; 
    } else {
        vertical = Input.GetAxis ("Scroll") * wheelSensitivity; 
    }
    smoothFollow.distance = Mathf.Clamp (smoothFollow.distance - (vertical * ratio),
                                         minDistance, maxDistance);
    smoothFollow.height = Mathf.Clamp (smoothFollow.height - vertical,
                                         minHeight, maxHeight);
}
