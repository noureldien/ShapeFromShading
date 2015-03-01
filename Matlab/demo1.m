%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Shape from shade
% using a person with 5 images (4 light source)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc;

lights=[0 0 40;
    20 0 40;
    0 20 40;
    -20 0 40;
    0 -20 40];

imageLocation = 'Images\';
imageExtension = '.jpg';
imageName = 'person';
imageCount = 5;
imageHeight = 240;
imageWidth = 320;

[shape, map, map_smooth] = shapeFromShading(lights, imageLocation,imageExtension,...
    imageName, imageCount, imageHeight, imageWidth, true);

%figure(1); clf;
%mesh(shape);

figure(1); clf;
surf(shape,'EdgeColor','none','FaceColor','red');
camlight headlight;
lighting phong;

figure(2); clf;
mesh(map_smooth);

figure(3); clf;
imshow(map);








