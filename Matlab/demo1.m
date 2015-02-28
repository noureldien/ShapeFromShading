%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Shape from shade
% using camels with 5 images (4 light source)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

clc;

lights=[0 0 40;
    20 0 40;
    0 20 40;
    -20 0 40;
    0 -20 40];

imageLocation = 'Images\';
imageExtension = '.jpg';
imageName = 'camel';
imageCount = 5;
imageHeight = 512;
imageWidth = 512;

shape = shapeFromShading(lights, imageLocation,imageExtension,...
    imageName, imageCount, imageHeight, imageWidth);
figure(1); clf;
mesh(shape);

figure(2); clf;
surf(shape,'EdgeColor','none','FaceColor','red');
camlight headlight;
lighting phong;