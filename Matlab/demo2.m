%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Shape from shade
% using camels with 9 images (8 light source)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc;

lights=[0 0 40;
    20 0 40;
    0 20 40;
    -20 0 40;
    0 -20 40;
    10 -10 40;
    10 10 40;
    -10 10 40;
    -10 -10 40];

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