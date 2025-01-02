# antenna-radiation-mapper
a simple anntena image to csv mapper
when buying a new rf anntena the specification/manual of the product most of the time gives you a graph of the polarization
but not actual datatable of values if you want to make digital calculations/estimations this attempts to convert an image graph to a csv of magnitude and angle by scaling


## how to use

### positioning
select your graph (make sure the image is big/medium size >250x250 pixels and 0 degress points to left axis)
![image](https://github.com/user-attachments/assets/0789d369-cebe-4c68-b23a-7f34a7ccd52f)

set the center of the graph (left click to apply)
![image](https://github.com/user-attachments/assets/bb651935-e1b6-49e1-a686-3caa7441261c)

### scaling Magnitude
set the graph scale in pixels
![image](https://github.com/user-attachments/assets/31ca8707-5da3-4d8f-9fc1-e43a63d87eb7)

set start as the minimum value of the graph (the center value in the example it is -40)
set the scale as the distance between the minimum value of the graph and the maximum value (in the example -40 -> 0 so the distance is 40)

### automatic detection

click on pick color and click on the graph
![image](https://github.com/user-attachments/assets/9753c160-9c4d-4054-bfaa-b77ce2f8b88a)

the program will attempt to detect automatically the graph using pixel-scan
click detect

after detection you may (and probably will) fix any imperfections of the results
using the 3rd graph it will lay out the estimation on the image
![image](https://github.com/user-attachments/assets/a7d24a45-6034-42b6-b03b-5cf2a27ccd93)
simply left click on the actual position of every angle you may want to fix
click save and the output will be on the same path of the image
![image](https://github.com/user-attachments/assets/e194d8c2-4955-4605-8dd6-d8ebd45d48ae)
![image](https://github.com/user-attachments/assets/1aa8efb3-e9d1-43f3-b46b-6b471687ca30)
(excel radar graph)

### how it works and limitations
it works by converting cartisian to polar points on a plane
i made the resolution of the mapper to 1 degree step you may increase the resolution to your own needs in the code
the results accuracy are depended on the image's resolution
