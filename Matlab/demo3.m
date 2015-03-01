%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Shape from shade
% using a camel with 9 images (8 light source)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc;

lights=[0 0 20;
    20 0 20;
    0 20 20;
    -20 0 20;
    0 -20 20;
    10 10 20;
    -10 10 20;
    -10 -10 20;
    10 -10 20];

imageLocation = 'Images\';
imageExtension = '.jpg';
imageName = 'camel';
imageCount = 9;
imageHeight = 512;
imageWidth = 512;

shape = shapeFromShading(lights, imageLocation,imageExtension,...
    imageName, imageCount, imageHeight, imageWidth);
figure(3); clf;
mesh(shape);

figure(4); clf;
surf(shape,'EdgeColor','none','FaceColor','red');
camlight headlight;
lighting phong;











